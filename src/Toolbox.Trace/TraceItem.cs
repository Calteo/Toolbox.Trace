using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Trace
{
    public class TraceItem
    {
        public DateTime Timestamp { get; internal set; }
        public string Source { get; internal set; }
        public TraceEventType EventType { get; internal set; }
        public int Id { get; internal set; }
        public int ThreadId { get; internal set; }
        public int ProcessId { get; internal set; }
        public string Text { get; internal set; }
        public StackFrame Caller { get; internal set; }
        public MethodBase Method => Caller.GetMethod();
        public TraceCapture[] Objects { get; internal set; }
    }
}
