using System;

namespace Toolbox.Trace.TestApp
{
    class SimpleData
    {
        public string Name { get; set; }
        public int Number { get; set; }
        [NotTraceable]
        public string Secret => "This should not appear in the trace.";

        public DateTime TimeStamp { get; set; } = DateTime.Now;

        [TraceConverter(typeof(TraceConverterValueType), "yyyy-MM-dd-HH-mm-ss-ffffff")]
        public DateTime Detail { get; set; } = DateTime.Now;
    }
}
