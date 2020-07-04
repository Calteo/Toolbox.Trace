using System;

namespace Toolbox.Trace
{
    class TraceConverterValueType : TraceConverter<ValueType>
    {
        protected override TraceCapture Capture(ValueType obj)
        {
            return new TraceCapture { Text = obj.ToString() };
        }
    }
}
