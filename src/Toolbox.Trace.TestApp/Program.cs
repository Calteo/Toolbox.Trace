using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Toolbox.Trace.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            TestInformation();
        }

        public static TraceSource CreateSource()
        {
            return new TraceSource("TestApp");
        }

        static void TestInformation()
        {
            var source = CreateSource();
            var listener = source.Listeners["object"] as ObjectFileTraceListener;
            
            source.TraceInformation("simple information");

            source.TraceData(TraceEventType.Information, 42, "Hello");

            var data = new SimpleData { Name = "Bob", Number = 678798798 };

            source.TraceData(TraceEventType.Warning, 42, "Something important", data);
            source.TraceData(TraceEventType.Error, 42, "no data given", null);
            source.TraceData(TraceEventType.Error, 42, "array of data", new[] { data, data });
            source.TraceData(TraceEventType.Error, 42, "List of data", new List<SimpleData> { data, data });
            source.TraceData(TraceEventType.Error, 42, "List of string", new List<string> { "One", "Two", "Three" });

            source.Close();
            
            using (var reader = new StreamReader(listener.Filename))
            {
                var text = reader.ReadToEnd();
            }
        }
    }
}