using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Trace
{
    public class TraceConverterObject : TraceConverter<object>
    {
        protected override TraceCapture Capture(object obj)
        {
            return new TraceCapture 
            {
                Text = obj.GetType().FullName,
                Children = GetChildren(obj)
            };
        }
    }
}
