using System.Threading;

namespace GZipTest
{
    public class SemaphoreSlimAdapter : ISemaphore
    {
        public SemaphoreSlimAdapter(int initialCount, int maxCount)
        {
            _adaptee = new SemaphoreSlim(initialCount, maxCount);
        }
        
        public void Wait(int millisecondsTimeout, CancellationToken token)
        {
            _adaptee.Wait(millisecondsTimeout, token);
        }

        public void Release()
        {
            _adaptee.Release();
        }

        private readonly SemaphoreSlim _adaptee;
    }
}