using System.Runtime.CompilerServices;

namespace GZipTest
{
    public interface ILogger
    {
        void Write(string message, [CallerFilePath]string caller = "");
    }
}