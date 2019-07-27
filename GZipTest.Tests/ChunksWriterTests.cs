using System.Collections.Generic;
using System.IO;
using Xunit;

namespace GZipTest.Tests
{
    public class ChunksWriterTests
    {
        [Fact]
        public void WritesNoContentForEmptyPipe()
        {
            var pipe = new PipeMock(new Chunk[0]);
            var stream = new MemoryStream();
            var writer = new ChunksWriter(pipe);
            writer.WriteToStream(stream);
            
            Assert.Equal(0, stream.Length);
        }

        [Fact]
        public void WritesSingleChunkToStream()
        {
            var bytes = new byte[] { 0x00, 0x11, 0x22 };
            var pipe = new PipeMock(new[]
            {
                new Chunk { Bytes = bytes } 
            });
            var stream = new MemoryStream();
            var writer = new ChunksWriter(pipe);
            writer.WriteToStream(stream);
            
            Assert.Equal(bytes, stream.ToArray());
        }

        [Fact]
        public void WritesTwoChunksConsecutive()
        {
            var bytes = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66 };
            var pipe = new PipeMock(new[]
            {
                new Chunk { Bytes =  new byte[] { 0x00, 0x11, 0x22 }, Index = 0 },
                new Chunk { Bytes =  new byte[] { 0x33, 0x44, 0x55, 0x66 }, Index = 1 }
            });
            var stream = new MemoryStream();
            var writer = new ChunksWriter(pipe);
            writer.WriteToStream(stream);
            
            Assert.Equal(bytes, stream.ToArray());
        }

        [Fact]
        public void WritesChunksInTheRightOrder()
        {
            var bytes = new byte[] { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99 };
            var pipe = new PipeMock(new[]
            {
                new Chunk { Bytes =  new byte[] { 0x33, 0x44, 0x55, 0x66 }, Index = 1 },
                new Chunk { Bytes =  new byte[] { 0x77, 0x88, 0x99 }, Index = 2 },
                new Chunk { Bytes =  new byte[] { 0x00, 0x11, 0x22 }, Index = 0 }
            });
            var stream = new MemoryStream();
            var writer = new ChunksWriter(pipe);
            writer.WriteToStream(stream);
            
            Assert.Equal(bytes, stream.ToArray());
        }
        
        private class PipeMock : IPipe
        {
            public PipeMock(Chunk[] chunks)
            {
                _chunks = new Queue<Chunk>(chunks);
            }

            public Chunk Read()
            {
                return _chunks.Count > 0 ? _chunks.Dequeue() : throw new PipeClosedException();
            }

            public void Write(Chunk chunk)
            {
                throw new System.NotSupportedException();
            }

            public void Open()
            {
                throw new System.NotSupportedException();
            }

            public void Close()
            {
                throw new System.NotSupportedException();
            }
            
            private readonly Queue<Chunk> _chunks;
        }
    }
}