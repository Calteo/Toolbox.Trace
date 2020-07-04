using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Trace
{
    /// <summary>
    /// Represens a capture of an object or property.
    /// </summary>
    public class TraceCapture
    {
        public string Name { get; internal set; }        
        public string Text { get; internal set; }

        public TraceCapture[] Children { get; set; }

    }
}
