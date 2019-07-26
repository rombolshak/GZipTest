namespace GZipTest
{
    public class InputPipe<T>: IPipe where T: ISemaphore
    {
        public InputPipe(T readGuard, T writeGuard)
        {
            _readGuard = readGuard;
            _writeGuard = writeGuard;
        }
        
        public Chunk Read()
        {
            _readGuard.Wait();
            _writeGuard.Release();
            return null;
        }

        public void Write(Chunk chunk)
        {
            _writeGuard.Wait();
            _readGuard.Release();
        }

        private readonly T _readGuard;
        private readonly T _writeGuard;
    }

    public class InputPipe : InputPipe<SemaphoreSlimAdapter>
    {
        public InputPipe(SemaphoreSlimAdapter readGuard, SemaphoreSlimAdapter writeGuard) : base(readGuard, writeGuard)
        {
        }
    }
}