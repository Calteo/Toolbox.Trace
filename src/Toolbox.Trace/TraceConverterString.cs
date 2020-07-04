namespace Toolbox.Trace
{
    class TraceConverterString : TraceConverter<string>
    {
        protected override TraceCapture Capture(string obj)
        {
            return new TraceCapture { Text = $"'{obj}'" };
        }
    }
}
