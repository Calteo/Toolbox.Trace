using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Toolbox.Trace
{
    public abstract class TraceConverterBase
    {
        protected internal abstract TraceCapture CaptureCore(object obj);
        protected internal abstract Type ConvertType { get; }

        protected internal ObjectTraceListener Listener { get; internal set; }
        
        protected TraceCapture[] GetChildren(object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            return properties.Select(p => Capture(obj, p)).ToArray();
        }

        private TraceCapture Capture(object obj, PropertyInfo property)
        {
            var value = property.GetValue(obj);

            var capture = value != null
                ? Listener.GetConverter(value).CaptureCore(value)
                : new TraceCapture { Text = "<null>" };
            capture.Name = property.Name;

            return capture;
        }

    }
}
