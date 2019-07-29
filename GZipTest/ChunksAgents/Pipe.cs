namespace GZipTest.ChunksAgents
{
    public class Pipe : GuardedPipe<SemaphoreSlimAdapter>
    {
        public Pipe(int maxElements) 
            : base(
                readGuard: new SemaphoreSlimAdapter(0, maxElements), 
                writeGuard: new SemaphoreSlimAdapter(maxElements, maxElements))
        {
        }
    }
}