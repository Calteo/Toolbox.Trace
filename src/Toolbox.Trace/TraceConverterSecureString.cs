using System.Security;

namespace Toolbox.Trace
{
    class TraceConverterSecureString : TraceConverter<SecureString>
    {
        protected override TraceCapture Capture(SecureString obj)
        {
            return new TraceCapture { Text = $"'***'" };
        }
    }
}
