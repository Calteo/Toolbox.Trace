using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Toolbox.Trace.Test
{
    [TestClass]
    public class TraceSourceTests
    {
        private Regex GetPattern(TraceSource source, ObjectFileTraceListener listner, string method, string text)
        {
            var pattern = "^";

            pattern += @"\[[0-9:. ]+\] ";
            pattern += @"<P=\d+> ";
            pattern += @"<T=\d+> ";
            pattern += ".+: ";

            pattern += $"{method.Replace("(", @"\(").Replace(")", @"\)")} ";

            if (text != "")
                pattern += $"- {text}";

            pattern += "$";

            return new Regex(pattern, RegexOptions.Singleline);
        }

        private List<string> GetLines(ObjectFileTraceListener listner)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(listner.Filename))
            {
                while (reader.Peek() >= 0)
                {
                    lines.Add(reader.ReadLine());
                }
            }
            return lines;
        }

        [TestMethod]
        public void TraceInformation()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{nameof(TraceInformation)}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId,
            };

            var source = new TraceSource(nameof(TraceSourceTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const string text = "some information";

            source.TraceInformation(text);
            
            source.Close();

            var lines = GetLines(cut);
            var pattern = GetPattern(source, cut, new StackFrame().GetMethod().ToString(), text);

            Assert.AreEqual(1, lines.Count);
            Assert.IsTrue(pattern.IsMatch(lines[0]), $"matching line - '{lines[0]}'");
        }

        [TestMethod]
        public void TraceInformationFromMethod()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{nameof(TraceInformationFromMethod)}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId
            };

            var source = new TraceSource(nameof(TraceSourceTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const string text = "some information";

            var pattern = TraceInformationMethod(source, cut, text);

            source.Close();

            var lines = GetLines(cut);

            Assert.AreEqual(1, lines.Count);
            Assert.IsTrue(pattern.IsMatch(lines[0]), $"matching line - '{lines[0]}'");
        }

        private Regex TraceInformationMethod(TraceSource source, ObjectFileTraceListener listener, string text)
        {
            source.TraceInformation(text);
            return GetPattern(source, listener, new StackFrame().GetMethod().ToString(), text);
        }


        [TestMethod]
        public void TraceInformationFromFunc()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{nameof(TraceInformationFromFunc)}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId
            };

            var source = new TraceSource(nameof(TraceSourceTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const string text = "some information";
            
            var action = new Func<TraceSource, ObjectFileTraceListener, string, Regex>((s, l, t) => { s.TraceInformation(t); return GetPattern(s, l, new StackFrame().GetMethod().ToString(), t); });

            var pattern = action(source, cut, text);

            source.Close();

            var lines = GetLines(cut);

            Assert.AreEqual(1, lines.Count);
            Assert.IsTrue(pattern.IsMatch(lines[0]), $"matching line - '{lines[0]}'");
        }

    }
}
