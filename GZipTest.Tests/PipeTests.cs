using System.Threading;
using Xunit;

namespace GZipTest.Tests
{
    public class PipeTests
    {
        [Fact]
        public void TestPipeWorkflow()
        {
            const int maxElements = 2;
            var readGuard = new SemaphoreMock(0);
            var writeGuard = new SemaphoreMock(maxElements);
            
            var pipe = new GuardedPipe<SemaphoreMock>(readGuard, writeGuard, new LoggerMock());
            pipe.Open();

            new Thread(() => pipe.Read()).Start();
            Thread.Sleep(200);
            Assert.True(readGuard.IsLocked);
            Assert.False(writeGuard.IsLocked);

            pipe.Write(new Chunk { Bytes = new byte[0] });
            Thread.Sleep(200);
            Assert.False(readGuard.IsLocked);
            Assert.False(writeGuard.IsLocked);
            
            pipe.Write(new Chunk { Bytes = new byte[0] }); 
            pipe.Write(new Chunk { Bytes = new byte[0] }); // maxElements reached
            
            new Thread(() => pipe.Write(new Chunk { Bytes = new byte[0] })).Start();
            Thread.Sleep(200);
            Assert.True(writeGuard.IsLocked);
            Assert.False(readGuard.IsLocked);
            
            pipe.Read();
            Thread.Sleep(200);
            Assert.False(writeGuard.IsLocked);
            Assert.False(readGuard.IsLocked);

            pipe.Close();
            pipe.Read();
            pipe.Read();
            Assert.Throws<PipeClosedException>(() => pipe.Read());
        }
        
        private class SemaphoreMock : ISemaphore
        {
            public SemaphoreMock(int initialCount)
            {
                _semaphore = new SemaphoreSlim(initialCount);
            }

            public bool IsLocked { get; private set; }
            
            public void Wait(int timeout)
            {
                IsLocked = true;
                _semaphore.Wait(timeout);
                IsLocked = false;
            }

            public void Release()
            {
                _semaphore.Release();
            }

            private readonly SemaphoreSlim _semaphore;
        }
    }
}