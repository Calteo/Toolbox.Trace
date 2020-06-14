using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Toolbox.Trace
{
    public class ObjectTraceSource : TraceSource
    {
        public ObjectTraceSource(string name = "ObjectTraceSource", SourceLevels defaultLevel = SourceLevels.All) : base(name, defaultLevel)
        {
        }

        [ThreadStatic] private static LocalDataStoreSlot _dataSlot;
        internal static LocalDataStoreSlot DataSlot => _dataSlot ?? (_dataSlot = Thread.GetNamedDataSlot("Joe"));

        [MethodImpl(MethodImplOptions.NoInlining)]
        public new void TraceEvent(TraceEventType eventType, int id, string format, params object[] args)
        {
            Thread.SetData(DataSlot, new StackFrame(1, true));
            base.TraceEvent(eventType, id, format, args);
            Thread.SetData(DataSlot, null);
        }
    }
}