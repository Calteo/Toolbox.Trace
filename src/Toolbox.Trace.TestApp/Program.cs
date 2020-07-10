using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace Toolbox.Trace.TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // TestFromCode();
            TestDataFromCode();
            // TestInformation();
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

            source.TraceData(TraceEventType.Warning, 43, "Something important", data);
            source.TraceData(TraceEventType.Error, 44, "no data given", null);
            source.TraceData(TraceEventType.Error, 45, "array of data", new[] { data, data });
            source.TraceData(TraceEventType.Error, 46, "List of data", new List<SimpleData> { data, data });
            source.TraceData(TraceEventType.Error, 47, "List of string", new List<string> { "One", "Two", "Three" });

            var list = new List<int>();
            var random = new Random(47);
            for (int i = 0; i < 100; i++)
            {
                list.Add(random.Next(10000));
            }
            source.TraceData(TraceEventType.Information, 48, list);

            var password = new SecureString();
            foreach (var c in "passW0rd")
            {
                password.AppendChar(c);
            } 
            source.TraceData(TraceEventType.Critical, 49, "some password", password);

            source.Close();
            
            using (var reader = new StreamReader(listener.Filename))
            {
                var text = reader.ReadToEnd();
            }
        }

        static void TestFromCode()
        {
            var source = new TraceSource("TestFromCode", SourceLevels.All);
            var listener = new ObjectFileTraceListener { Filename = "trace.txt" };
            source.Listeners.Add(listener);

            source.TraceInformation("Hello trace!");
        }

        static void TestDataFromCode()
        {
            var source = new TraceSource("TestDataFromCode", SourceLevels.All);
            var listener = new ObjectFileTraceListener { Filename = "trace.txt" };
            source.Listeners.Add(listener);

            var data = new SimpleData { Name = "Alice" };

            source.TraceData(TraceEventType.Error, 42, "some text", data);

            source.Close();
        }

    }
}