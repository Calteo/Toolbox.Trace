using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Toolbox.Trace.Test
{
    [TestClass]
    public class ObjectFileTraceListenerTests
    {
        private Regex GetPattern(TraceSource source, ObjectFileTraceListener listner, string method, TraceEventType eventType, int id, string text)
        {
            var pattern = "^";

            pattern += @"\[[0-9:. ]+\] ";
            pattern += @"<P=\d+> ";
            pattern += @"<T=\d+> ";
            pattern += $@".+: {eventType}\[{id}\] - ";

            pattern += $"{method.Replace("(", @"\(").Replace(")", @"\)")} ";

            pattern += $"- {text}$";

            return new Regex(pattern, RegexOptions.Singleline);
        }

        private string GetMethodName([CallerMemberName]string methodName = null)
        {
            return methodName;
        }

        private List<string> GetLines(ObjectFileTraceListener listener)
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(listener.Filename))
            {
                while (reader.Peek() >= 0)
                {
                    lines.Add(reader.ReadLine());
                }
            }
            return lines;
        }

        private List<Regex> GetPatterns(string methodName)
        {
            var name = $"{GetType().FullName}.{methodName}.txt";
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);

            if (stream == null)
                throw new ArgumentException($"resouce '{name}' not found.", nameof(methodName));

            var lines = new List<Regex>();
            using (var reader = new StreamReader(stream))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine()
                        .Replace(".", @"\.")
                        .Replace("*", @".*")
                        .Replace("[", @"\[").Replace("]", @"\]")
                        .Replace("(", @"\(").Replace(")", @"\)")
                        .Replace("<", @"\<").Replace(">", @"\>");
                    lines.Add(new Regex("^" + line + "$"));
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
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId,
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const string text = "some information";

            source.TraceInformation(text);
           
            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }

        private void AssertLines(List<Regex> expectedLines, List<string> actualLines)
        {
            Assert.AreEqual(expectedLines.Count, actualLines.Count);
            for (var i = 0; i < expectedLines.Count; i++)
            {
                Assert.IsTrue(expectedLines[i].IsMatch(actualLines[i]), $"line[{i}]='{actualLines[i]}' pattern='{expectedLines[i]}'");
            }
        }

        [TestMethod]
        public void TraceInformationFormat()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId,
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const string format = "some information is {0}";
            const int number = 42;

            source.TraceInformation(format, number);
            
            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }


        [TestMethod]
        public void TraceInformationFromMethod()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const string text = "some information";

            TraceInformationMethod(source, text);

            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }

        private void TraceInformationMethod(TraceSource source, string text)
        {
            source.TraceInformation(text);
        }


        [TestMethod]
        public void TraceInformationFromAction()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const string text = "some information";

            var action = new Action<TraceSource>(s => s.TraceInformation(text));

            action(source);

            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }

        [TestMethod]
        public void TraceEvent()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId,
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const int number = 42;
            const TraceEventType eventType = TraceEventType.Verbose;

            source.TraceEvent(eventType, number);

            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }

        [TestMethod]
        public void TraceEventMessage()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId,
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const int number = 42;
            const TraceEventType eventType = TraceEventType.Verbose;
            const string message = "some message";

            source.TraceEvent(eventType, number, message);

            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }

        [TestMethod]
        public void TraceEventMessageFormat()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId,
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const int id = 42;
            const TraceEventType eventType = TraceEventType.Verbose;
            const string format = "some information is {0}";
            const int number = 859874;

            source.TraceEvent(eventType, id, format, number);

            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }

        [TestMethod]
        public void TraceDataSingle()
        {
            var cut = new ObjectFileTraceListener
            {
                Name = "object",
                Filename = $"Trace-{GetMethodName()}.txt",
                TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId,
            };

            var source = new TraceSource(nameof(ObjectFileTraceListenerTests), SourceLevels.All)
            {
                Listeners = { cut }
            };

            const int id = 42;
            const TraceEventType eventType = TraceEventType.Verbose;
            const string message = "some message";
            const int number = 787;

            var data = new Data { Text = message, Id = number };

            source.TraceData(eventType, id, data);

            source.Close();

            var expectedLines = GetPatterns(GetMethodName());
            var actualLines = GetLines(cut);

            AssertLines(expectedLines, actualLines);
        }
    }
}
