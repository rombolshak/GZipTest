using System.Threading;

namespace GZipTest
{
    public class SemaphoreSlimAdapter : ISemaphore
    {
        public SemaphoreSlimAdapter(int initialCount, int maxCount)
        {
            _adaptee = new SemaphoreSlim(initialCount, maxCount);
        }
        
        public void Wait()
        {
            _adaptee.Wait();
        }

        public void Release()
        {
            _adaptee.Release();
        }

        private readonly SemaphoreSlim _adaptee;
    }
}