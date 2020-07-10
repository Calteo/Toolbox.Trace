using System;

namespace Toolbox.Trace
{
    public class TraceConverterValueType : TraceConverter<ValueType>
    {
        public TraceConverterValueType()
        {
        }

        public TraceConverterValueType(string format)
            : this()
        {
            Format = format;
        }

        public string Format { get; }

        protected override TraceCapture Capture(ValueType obj)
        {
            return new TraceCapture { Text = string.Format($"{{0:{Format}}}", obj) };
        }
    }
}
