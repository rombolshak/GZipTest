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

            pipe.Write();
            Assert.False(readGuard.IsLocked);
            
            pipe.Write(); 
            pipe.Write(); // maxElements reached
            pipe.Write();
            Assert.True(writeGuard.IsLocked);
            
            pipe.Read();
            Assert.False(writeGuard.IsLocked);
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