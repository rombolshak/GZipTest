using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    public class GuardedPipe<T>: IPipe where T: ISemaphore
    {
        public GuardedPipe(T readGuard, T writeGuard, ILogger logger)
        {
            _readGuard = readGuard;
            _writeGuard = writeGuard;
            _logger = logger;
        }
        
        public Chunk Read()
        {
            AcquireReadLock();
            var chunk = _queue.Dequeue();
            _logger.Write($"Read lock acquired, got chunk #{chunk.Index} if {chunk.Bytes.Length} bytes");
            _writeGuard.Release();
            return chunk;
        }

        public void Write(Chunk chunk)
        {
            _writeGuard.Wait(millisecondsTimeout: int.MaxValue);
            _logger.Write($"Write lock acquired, storing chunk #{chunk.Index} of {chunk.Bytes.Length} bytes");
            _queue.Enqueue(chunk);
            _readGuard.Release();
        }

        public void Open()
        {
            Interlocked.Increment(ref _registeredWriters);
            _logger.Write("Pipe opened");
            _wasEverOpened = true;
        }

        public void Close()
        {
            Interlocked.Decrement(ref _registeredWriters);
            _logger.Write("Pipe closed");
        }

        private void AcquireReadLock()
        {
            while (true)
            {
                _readGuard.Wait(millisecondsTimeout: 500);
                if (_queue.Count == 0)
                {
                    if (_registeredWriters == 0 && _wasEverOpened)
                    {
                        throw new PipeClosedException();
                    }

                    continue;
                }

                break;
            }
        }

        private readonly Queue<Chunk> _queue = new Queue<Chunk>();
        private readonly T _readGuard;
        private readonly T _writeGuard;
        private readonly ILogger _logger;
        private int _registeredWriters = 0;
        private bool _wasEverOpened;
    }
}