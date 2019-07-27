namespace GZipTest
{
    public interface ISemaphore
    {
        void Wait(int millisecondsTimeout);
        void Release();
    }
}