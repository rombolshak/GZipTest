using System.Threading;

namespace GZipTest.ChunksAgents
{
    public interface ISemaphore
    {
        void Wait(int millisecondsTimeout, CancellationToken token);
        void Release();
    }
}