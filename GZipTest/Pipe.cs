namespace GZipTest
{
    public class Pipe : GuardedPipe<SemaphoreSlimAdapter>
    {
        public Pipe(int maxElements, ILogger logger) 
            : base(
                readGuard: new SemaphoreSlimAdapter(0, maxElements), 
                writeGuard: new SemaphoreSlimAdapter(maxElements, maxElements),
                logger: logger)
        {
        }
    }
}