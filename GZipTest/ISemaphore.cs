namespace GZipTest
{
    public interface ISemaphore
    {
        void Wait();
        void Release();
    }
}