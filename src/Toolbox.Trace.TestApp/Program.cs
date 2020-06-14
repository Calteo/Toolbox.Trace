using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Trace.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {           
            TraceSource.TraceEvent(TraceEventType.Information, 4711, "Result = {0}", 42);                        
        }

        public static  ObjectTraceSource TraceSource { get; } = new ObjectTraceSource("TestApp");
    }
}
