using Xunit;

namespace GZipTest.Tests
{
    public class InputPipeTests
    {
        [Fact]
        public void TestPipeWorkflow()
        {
            const int maxElements = 2;
            var readGuard = new SemaphoreMock(0);
            var writeGuard = new SemaphoreMock(maxElements);
            
            var pipe = new InputPipe<SemaphoreMock>(readGuard, writeGuard);

            pipe.Read();
            Assert.True(readGuard.IsLocked);
            Assert.False(writeGuard.IsLocked);

            pipe.Write(new Chunk());
            Assert.False(readGuard.IsLocked);
            Assert.False(writeGuard.IsLocked);
            
            pipe.Write(new Chunk()); 
            pipe.Write(new Chunk()); // maxElements reached
            pipe.Write(new Chunk());
            Assert.True(writeGuard.IsLocked);
            Assert.False(readGuard.IsLocked);
            
            pipe.Read();
            Assert.False(writeGuard.IsLocked);
            Assert.False(readGuard.IsLocked);
        }
        
        private class SemaphoreMock : ISemaphore
        {
            public SemaphoreMock(int initialCount)
            {
                _count = initialCount;
            }
            
            public bool IsLocked => _count < 0;
            
            public void Wait()
            {
                _count--;
            }

            public void Release()
            {
                _count++;
            }
            
            private int _count;
        }
    }
}