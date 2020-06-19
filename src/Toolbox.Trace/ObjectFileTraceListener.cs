using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Toolbox.Trace
{
    public class ObjectFileTraceListener : ObjectTraceListener
    {
        public ObjectFileTraceListener()
        {
            Filename = "ObjectFileTraceListener.txt";
            Template = "[{Timestamp:G}] <P={ProcessId}> <T={ThreadId}> {Source}: {Method} - {Text}";
        }

        public ObjectFileTraceListener(string initData)
            : this()
        {            
        }        

        private static string[] SupportedAttributes { get; } = new[] 
        { 
            AttributeFilename, 
            AttributeAppend,
            AttributeTemplate,
        };

        protected override string[] GetSupportedAttributes()
        {
            return base.GetSupportedAttributes()?.Concat(SupportedAttributes).ToArray() ?? SupportedAttributes;
        }

        private T GetAttribute<T>(string key) 
        {
            var value = Attributes[key];

            if (value == null) return default;            

            return (T)Convert.ChangeType(value, typeof(T));
        }

        private void SetAttribute<T>(string key, T value)
        {
            if (value == null)
                Attributes.Remove(key);
            else
                Attributes[key] = Convert.ToString(value);
        }

        #region Filename
        private const string AttributeFilename = "filename";
        public string Filename
        {
            get => Attributes[AttributeFilename];
            set => Attributes[AttributeFilename] = value;
        }
        #endregion

        #region Filename
        private const string AttributeAppend = "append";
        public bool Append
        {
            get => GetAttribute<bool>(AttributeAppend);
            set => SetAttribute(AttributeAppend, value);
        }
        #endregion

        #region Template
        private const string AttributeTemplate = "template";
        public string Template
        {
            get => Attributes[AttributeTemplate];
            set { Attributes[AttributeTemplate] = value; ParseTemplate(); }
        }
        #endregion

        private List<Action<TraceItem>> TemplateLines { get; } = new List<Action<TraceItem>>();

        private void ParseTemplate()
        {
            TemplateLines.Clear();
            if (string.IsNullOrEmpty(Template)) return;

            var lines = Template.Replace("\r\n", "\n").Split('\n');
            foreach (var line in lines)
            {
                var blockMatch = PatternBlocks.Match(line);
                if (blockMatch.Success)
                {
                    switch (blockMatch.Groups["parameter"].Value)
                    {
                        case "objects":
                            TemplateLines.Add(t => WriteObjects(t, blockMatch.Groups["option"].Value, blockMatch.Groups["prefix"].Value, blockMatch.Groups["suffic"].Value));
                            break;
                        case "stacktrace":
                            TemplateLines.Add(t => WriteStackTrace(t, blockMatch.Groups["option"].Value, blockMatch.Groups["prefix"].Value, blockMatch.Groups["suffic"].Value));
                            break;
                        default:
                            throw new NotSupportedException($"block parameter {blockMatch.Groups["parameter"].Value}");
                    }
                }
                else
                {
                    var parameterMatches = PatternProperties.Matches(line);
                    if (parameterMatches.Count == 0)
                        TemplateLines.Add(t => Writer.WriteLine(line));
                    else
                    {
                        var properties = new HashSet<string>(typeof(TraceItem).GetProperties().Select(p => p.Name));

                        foreach (Match match in parameterMatches)
                        {
                            var parameter = match.Groups["parameter"].Value;
                            if (parameter == "object" || parameter == "stacktrace")
                                throw new NotSupportedException($"block parameter {parameter} not supported in mixed template line");
                            if (!properties.Contains(parameter))
                                throw new NotSupportedException($"parameter {parameter} not supported");
                        }

                        TemplateLines.Add(t => WriteProperties(t, line));
                    }
                }
            }
        }

        private void WriteProperties(TraceItem item, string template)
        {
            var text = PatternProperties.Replace(template, m => string.Format("{0" + m.Groups["option"].Value + "}", item.GetType().GetProperty(m.Groups["parameter"].Value).GetValue(item)));
            Writer.WriteLine(text);
        }

        private void WriteObjects(TraceItem item, string options, string prefix, string suffix)
        {
            
        }

        private void WriteStackTrace(TraceItem item, string options, string prefix, string suffix)
        {
        }

        private static readonly Regex PatternProperties = new Regex(@"{(?<parameter>\w+)((?<option>:[^}]+))?}", RegexOptions.Compiled);
        private static readonly Regex PatternBlocks = new Regex(@"^(?<prefix>.*){(?<parameter>(stacktrace|objects))(:(?<option>[^}]+))?}(?<suffix>.*)$", RegexOptions.Compiled);

        protected override TextWriter CreateWriter()
        {
            return new StreamWriter(Filename, Append);
        }

        protected override void Write(TraceItem item)
        {
            foreach (var action in TemplateLines)
            {
                action(item);
            }
        }
    }
}
