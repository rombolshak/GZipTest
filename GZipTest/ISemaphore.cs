using System.Threading;

namespace GZipTest
{
    public interface ISemaphore
    {
        void Wait(int millisecondsTimeout, CancellationToken token);
        void Release();
    }
}