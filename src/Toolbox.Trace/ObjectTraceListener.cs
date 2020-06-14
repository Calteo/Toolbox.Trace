using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Toolbox.Trace
{
    public class ObjectTraceListener : TraceListener
    {
        public ObjectTraceListener()
        {
        }

        public ObjectTraceListener(string initData)
            : this()
        {
        }

        protected override string[] GetSupportedAttributes()
        {
            return new[] { "filename" };
        }

        public override void Write(string message)
        {
            Output.Append(message);
        }

        public override void WriteLine(string message)
        {
            Output.AppendLine(message);
        }

        public StringBuilder Output { get; set; } = new StringBuilder();

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            var frame = (StackFrame)Thread.GetData(ObjectTraceSource.DataSlot);
            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }
    }
}
