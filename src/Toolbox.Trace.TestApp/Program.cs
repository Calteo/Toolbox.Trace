using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            
            source.Close();
            
            using (var reader = new StreamReader(listener.Filename))
            {
                var text = reader.ReadToEnd();
            }
        }
    }
}
