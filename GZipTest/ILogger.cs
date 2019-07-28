using System.Runtime.CompilerServices;

namespace GZipTest
{
    public interface ILogger
    {
        void Write(string message, [CallerFilePath]string caller = "");
        void WriteError(string message, [CallerFilePath]string caller = "");
    }
}