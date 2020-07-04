using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Trace
{
    class TraceConverterEnumerable : TraceConverter<IEnumerable>
    {
        protected override TraceCapture Capture(IEnumerable obj)
        {
            var capture = new TraceCapture { Text = $"{obj.GetType().FullName}" };
            var children = new List<TraceCapture>();
            var enumarator = obj.GetEnumerator();
            var index = 0;
            while (enumarator.MoveNext())
            {
                var childCapture = Listener.GetConverter(enumarator.Current).CaptureCore(enumarator.Current);
                childCapture.Name = $"[{index++}]";
                children.Add(childCapture);
            }
            capture.Children = children.ToArray();
            capture.Text += $" - {index} elements";            

            return capture;
        }
    }
}
