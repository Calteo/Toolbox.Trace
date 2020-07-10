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
    /// <summary>
    /// An <see cref="ObjectTraceListener"/> with output to a file in text format
    /// </summary>
    public class ObjectFileTraceListener : ObjectTraceListener
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFileTraceListener"/> class.
        /// </summary>
        public ObjectFileTraceListener()
        {
            Filename = $"{nameof(ObjectFileTraceListener)}.txt";
            Template = Properties.Resources.ObjectFileTraceListenerTemplate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFileTraceListener"/> class.
        /// </summary>
        /// <param name="initData">this is ignored</param>
        public ObjectFileTraceListener(string initData)
            : this()
        {            
        }        

        #region Filename
        private const string AttributeFilename = "filename";
        /// <summary>
        /// Gets or sets the filename of the output file.
        /// </summary>
        /// <remarks>
        /// This property can be set from the configuration file with the attribute 'filename'.
        /// </remarks>
        [SupportedAttribute(AttributeFilename)]       
        public string Filename
        {
            get => Attributes[AttributeFilename];
            set => Attributes[AttributeFilename] = value;
        }
        #endregion

        #region Append
        private const string AttributeAppend = "append";
        /// <summary>
        /// Get or sets if the outpuf file should be appended or overwritten.
        /// </summary>
        /// <remarks>
        /// This property can be set from the configuration file with the attribute 'append'.
        /// </remarks>
        [SupportedAttribute(AttributeAppend)]
        public bool Append
        {
            get => GetAttribute<bool>(AttributeAppend);
            set => SetAttribute(AttributeAppend, value);
        }
        #endregion

        #region Template
        private const string AttributeTemplate = "template";
        /// <summary>
        /// Gets or sets the template for writing the output of one <see cref="TraceItem"/>.
        /// </summary>
        /// <remarks>
        /// This property can be set from the configuration file with the attribute 'template'.
        /// </remarks>
        [SupportedAttribute(AttributeTemplate)]
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
                        case "Objects":
                            TemplateLines.Add(t => WriteObjects(t, blockMatch.Groups["option"].Value, blockMatch.Groups["prefix"].Value, blockMatch.Groups["suffic"].Value));
                            break;
                        case "StackTrace":
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
                            if (parameter == "Object" || parameter == "StackTrace")
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
            item.Objects?.ForEach(c => WriteCapture(c, prefix, suffix));
        }

        private void WriteCapture(TraceCapture capture, string prefix, string suffix)
        {
            Writer.WriteLine($"{prefix}{capture.Name} = {capture.Text}");
            capture.Children?.ForEach(c => WriteCapture(c, prefix + "    ", suffix));
        }

        private void WriteStackTrace(TraceItem item, string options, string prefix, string suffix)
        {
        }

        private static readonly Regex PatternProperties = new Regex(@"{(?<parameter>\w+)((?<option>:[^}]+))?}", RegexOptions.Compiled);
        private static readonly Regex PatternBlocks = new Regex(@"^(?<prefix>.*){(?<parameter>(StackTrace|Objects))(:(?<option>[^}]+))?}(?<suffix>.*)$", RegexOptions.Compiled);

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
