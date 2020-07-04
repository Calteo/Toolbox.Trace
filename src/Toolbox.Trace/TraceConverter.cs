using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Trace
{
    public abstract class TraceConverter<T> : TraceConverterBase
    {

        protected internal override Type ConvertType => typeof(T);

        protected abstract TraceCapture Capture(T obj);

        protected internal override TraceCapture CaptureCore(object obj)
        {
            return Capture((T)obj);
        }
    }
}
