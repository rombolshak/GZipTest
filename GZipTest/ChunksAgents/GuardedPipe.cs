using System.Collections.Generic;
using System.Threading;

namespace GZipTest.ChunksAgents
{
    public class GuardedPipe<T>: IPipe where T: ISemaphore
    {
        public GuardedPipe(T readGuard, T writeGuard)
        {
            _readGuard = readGuard;
            _writeGuard = writeGuard;
        }
        
        public Chunk Read(CancellationToken token)
        {
            AcquireReadLock(token);
            _writeGuard.Release();
            lock (_queue)
            {
                return _queue.Dequeue();
            }
        }

        public void Write(Chunk chunk, CancellationToken token)
        {
            _writeGuard.Wait(millisecondsTimeout: int.MaxValue, token: token);
            lock (_queue)
            {
                _queue.Enqueue(chunk);
            }
            
            _readGuard.Release();
        }

        public void Open()
        {
            Interlocked.Increment(ref _registeredWriters);
            _wasEverOpened = true;
        }

        public void Close()
        {
            Interlocked.Decrement(ref _registeredWriters);
        }

        private void AcquireReadLock(CancellationToken token)
        {
            while (true)
            {
                _readGuard.Wait(millisecondsTimeout: 500, token: token);
                lock (_queue)
                {
                    if (_queue.Count == 0)
                    {
                        if (_registeredWriters == 0 && _wasEverOpened)
                        {
                            throw new PipeClosedException();
                        }

                        continue;
                    }
                }

                break;
            }
        }

        private readonly Queue<Chunk> _queue = new Queue<Chunk>();
        private readonly T _readGuard;
        private readonly T _writeGuard;
        private int _registeredWriters = 0;
        private bool _wasEverOpened;
    }
}