using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Toolbox.Trace
{
    public abstract class ObjectTraceListener : TraceListener
    {
        public ObjectTraceListener()
        {
        }

        public ObjectTraceListener(string initData)
        {            
        }

        public override bool IsThreadSafe => true;

        protected TextWriter Writer { get; private set; }

        protected abstract TextWriter CreateWriter();
        
        private Queue<TraceItem> Items { get; } = new Queue<TraceItem>();

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

        #region TraceItem
        protected class TraceItem
        {
            public DateTime Timestamp { get; set; }
            public string Source { get; set; }
            public TraceEventType EventType { get; set; }
            public int Id { get; set; }
            public int ThreadId { get; set; }
            public int ProcessId { get; set; }
            public string Text { get; set; }
            public StackFrame Caller { get; set; }
            public MethodBase Method => Caller.GetMethod();
        }

        abstract protected void Write(TraceItem item);
        protected virtual void Init()
        {
            Writer = CreateWriter();
        }
        #endregion

        #region OutputWorker
        private Thread OutputWorker { get; set; }
        private bool OutputRunning { get; set; }
        private AutoResetEvent OutputWait { get; } = new AutoResetEvent(false);
        private void DoOutput()
        {            
            Init();

            while (OutputRunning || Items.Count>0)
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
        }
        #endregion

        public override void Flush()
        {
            Writer?.Flush();
            base.Flush();
            OutputWait.Set();
        }

        public override void Close()
        {
            OutputRunning = false;
            OutputWait.Set();
            OutputWorker?.Join();
            OutputWorker = null;

            Flush();

            Writer.Close();

            base.Close();
        }

        private StackFrame[] GetFrames()
        {
            var frames = new StackTrace(2, true).GetFrames()
                .SkipWhile(f => f.GetMethod().DeclaringType.Namespace == "System.Diagnostics" || typeof(ObjectTraceListener).IsAssignableFrom(f.GetMethod().DeclaringType))
                .ToArray();
            return frames;
        }

        public override void WriteLine(string message)
        {
        }

        public override void Write(string message)
        {
        }

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

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, "");
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            TraceEvent(eventCache, source, eventType, id, message, null);
        }
    }
}
