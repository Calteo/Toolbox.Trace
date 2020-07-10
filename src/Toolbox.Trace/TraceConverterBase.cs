using System;
using System.Linq;
using System.Reflection;

namespace Toolbox.Trace
{
    public abstract class TraceConverterBase
    {
        protected internal abstract TraceCapture CaptureCore(object obj);
        protected internal abstract Type ConvertType { get; }

        protected internal ObjectTraceListener Listener { get; internal set; }
        
        protected TraceCapture[] GetChildren(object obj)
        {
            var properties = obj.GetType()
                                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                .Where(p => p.GetCustomAttribute<NotTraceableAttribute>(true) == null);

            return properties.Select(p => Capture(obj, p)).ToArray();
        }

        private TraceCapture Capture(object obj, PropertyInfo property)
        {
            var value = property.GetValue(obj);
                        
            var capture = value != null
                ? (GetConverter(property) ?? Listener.GetConverter(value)).CaptureCore(value)
                : new TraceCapture { Text = "<null>" };
            capture.Name = property.Name;

            return capture;
        }

        private TraceConverterBase GetConverter(PropertyInfo property)
        {
            var attribute = property.GetCustomAttribute<TraceConverterAttribute>(true);
            if (attribute == null) return null;

            return (TraceConverterBase)Activator.CreateInstance(attribute.ConvertType, attribute.Arguments);
        }
    }
}
