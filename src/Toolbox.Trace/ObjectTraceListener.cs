﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Toolbox.Trace
{
    /// <summary>
    /// Base class for all listeners that trace objects.
    /// </summary>
    public abstract class ObjectTraceListener : TraceListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTraceListener"/> class.
        /// </summary>
        public ObjectTraceListener()
        {
            RegisterConverter<TraceConverterString>();
            RegisterConverter<TraceConverterValueType>();
            RegisterConverter<TraceConverterSecureString>();

            ObjectConverter = new TraceConverterObject { Listener = this };
            EnumerableConverter = new TraceConverterEnumerable { Listener = this };
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectTraceListener"/> class.
        /// </summary>
        /// <param name="initData">this is ignored</param>
        public ObjectTraceListener(string initData)
            : this()
        {            
        }

        /// <inheritdoc/>
        public override bool IsThreadSafe => true;

        protected TextWriter Writer { get; private set; }

        protected abstract TextWriter CreateWriter();
        
        private Queue<TraceItem> Items { get; } = new Queue<TraceItem>();

        protected T GetAttribute<T>(string key)
        {
            var value = Attributes[key];

            if (value == null) return default;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        protected void SetAttribute<T>(string key, T value)
        {
            if (value == null)
                Attributes.Remove(key);
            else
                Attributes[key] = Convert.ToString(value);
        }

        protected override string[] GetSupportedAttributes()
        {
            var names = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => p.GetCustomAttribute<SupportedAttributeAttribute>(true)?.Name)
                            .Where(n => n != null);

            return (base.GetSupportedAttributes()?.Concat(names) ?? names).ToArray();
        }

        #region Append
        private const string AttributeMaxCollectionCount = "maxCollectionCount";
        /// <summary>
        /// Gets or sets the length of elements trace from a collection.
        /// </summary>
        /// <remarks>
        /// This property can be set from the configuration file with the attribute 'maxCollectionCount'.
        /// </remarks>        
        [SupportedAttribute(AttributeMaxCollectionCount)]
        public int MaxCollectionCount         
        {
            get => GetAttribute<int>(AttributeMaxCollectionCount);
            set => SetAttribute(AttributeMaxCollectionCount, value);
        }
        #endregion

        protected void Enqueue(TraceItem item)
        {
            item.Timestamp = DateTime.Now;
            
            lock (Items) { Items.Enqueue(item); }
            
            if (OutputWorker == null)
            {
                OutputWorker = new Thread(DoOutput)
                {
                    IsBackground = true,
                    Name = $"{GetType().Name}.{Name}.Output",
                    Priority = ThreadPriority.Lowest
                };
                OutputRunning = true;
                OutputWorker.Start();
            }
            OutputWait.Set();
        }

        abstract protected void Write(TraceItem item);

        protected virtual void Init()
        {
            Writer = CreateWriter();
        }

        #region OutputWorker
        private Thread OutputWorker { get; set; }
        private bool OutputRunning { get; set; }
        private AutoResetEvent OutputWait { get; } = new AutoResetEvent(false);
        private void DoOutput()
        {            
            Init();

            do
            {
                TraceItem item = null;
                lock (Items)
                {
                    if (Items.Count > 0)
                        item = Items.Dequeue();
                }
                if (item != null)
                {
                    Write(item);
                }
                else if (OutputRunning)
                {
                    OutputWait.WaitOne(1000);
                }
            }
            while (OutputRunning || Items.Count > 0);
        }
        #endregion

        /// <inheritdoc/>
        public override void Flush()
        {
            Writer?.Flush();
            base.Flush();
            OutputWait.Set();
        }

        /// <inheritdoc/>
        public override void Close()
        {
            OutputRunning = false;
            OutputWait.Set();
            OutputWorker?.Join();
            OutputWorker = null;

            Flush();

            Writer?.Close();

            base.Close();
        }

        private StackFrame[] GetFrames()
        {
            var frames = new StackTrace(2, true).GetFrames()
                .SkipWhile(f => f.GetMethod().DeclaringType.Namespace == "System.Diagnostics" || typeof(ObjectTraceListener).IsAssignableFrom(f.GetMethod().DeclaringType))
                .ToArray();
            return frames;
        }

        /// <inheritdoc/>
        public override void WriteLine(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Write(string message)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            var frames = GetFrames();

            Enqueue(
                new TraceItem
                {
                    Source = source,
                    EventType = eventType,
                    Id = id,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    ProcessId = Process.GetCurrentProcess().Id,
                    Text = string.Format(format, args ?? new object[0]),
                    Caller = frames[0]
                });
        }

        /// <inheritdoc/>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, "");
        }

        /// <inheritdoc/>
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, message, null);
        }

        /// <inheritdoc/>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            TraceData(eventCache, source, eventType, id, new[] { data });
        }

        /// <inheritdoc/>
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            var frames = GetFrames();

            var captures = data?.Aggregate(
                new List<TraceCapture>(), 
                (list, obj) => 
                    {
                        var capture = obj != null
                                        ? GetConverter(obj).CaptureCore(obj)
                                        : new TraceCapture { Text = "<null>" };
                        capture.Name = $"[{list.Count}]";
                        list.Add(capture);

                        return list; 
                    }).ToArray();

            Enqueue(
                new TraceItem
                {
                    Source = source,
                    EventType = eventType,
                    Id = id,
                    ThreadId = Thread.CurrentThread.ManagedThreadId,
                    ProcessId = Process.GetCurrentProcess().Id,
                    Text = data == null ? "no objects" : $"{data.Length} object(s)",
                    Objects = captures,
                    Caller = frames[0]
                });
        }

        private Dictionary<Type, TraceConverterBase> TraceConverters { get; } = new Dictionary<Type, TraceConverterBase>();
        public void RegisterConverter<T>() where T : TraceConverterBase, new()
        {
            var converter = new T { Listener = this };
            TraceConverters[converter.ConvertType] = converter;
        }

        private TraceConverterObject ObjectConverter { get; } 
        private TraceConverterEnumerable EnumerableConverter { get; }

        internal TraceConverterBase GetConverter(object obj)
        {
            var converter = GetConverter(obj.GetType());

            if (converter == null)
            {
                if (obj is IEnumerable)
                    return EnumerableConverter;

                return ObjectConverter;
            }

            return converter;
        }

        internal TraceConverterBase GetConverter(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (!TraceConverters.TryGetValue(type, out var converter))
            {
                var attribute = type.GetCustomAttribute<TraceConverterAttribute>();
                if (attribute != null)
                {
                    converter = attribute.CreateConverter(type);
                    converter.Listener = this;
                    TraceConverters[type] = converter;
                }
            }

            if (type.BaseType == null)
            {
                return null;
            }

            return converter ?? GetConverter(type.BaseType);
        }
    }
}
