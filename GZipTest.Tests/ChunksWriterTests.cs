using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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
            var writer = new ChunksWriter(pipe, new LoggerMock());
            writer.WriteToStream(stream, new CancellationToken(), 0);
            
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
            var writer = new ChunksWriter(pipe, new LoggerMock());
            writer.WriteToStream(stream, new CancellationToken(), 1);
            
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
            var writer = new ChunksWriter(pipe, new LoggerMock());
            writer.WriteToStream(stream, new CancellationToken(), 2);
            
            Assert.Equal(bytes, stream.ToArray());
        }

        [Fact]
        public void WritesChunksInTheRightOrder()
        {
            var bytes = new byte[]
                { 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xaa, 0xbb, 0xcc, 0xdd, 0xee, 0xff };
            var pipe = new PipeMock(new[]
            {
                new Chunk { Bytes =  new byte[] { 0x33, 0x44, 0x55, 0x66 }, Index = 1 },
                new Chunk { Bytes =  new byte[] { 0x77, 0x88, 0x99 }, Index = 2 },
                new Chunk { Bytes =  new byte[] { 0x00, 0x11, 0x22 }, Index = 0 },
                new Chunk { Bytes =  new byte[] { 0xdd, 0xee, 0xff }, Index = 4 },
                new Chunk { Bytes =  new byte[] { 0xaa, 0xbb, 0xcc }, Index = 3 }
            });
            var stream = new MemoryStream();
            var writer = new ChunksWriter(pipe, new LoggerMock());
            writer.WriteToStream(stream, new CancellationToken(), 5);
            
            Assert.Equal(bytes, stream.ToArray());
        }

        [Fact]
        public void TestChunksLengthsWriting()
        {
            var bytes = BitConverter.GetBytes(3).Concat(new byte[] { 0x00, 0x11, 0x22 })
                .Concat(BitConverter.GetBytes(4)).Concat(new byte[] { 0x33, 0x44, 0x55, 0x66 })
                .ToArray();
            var pipe = new PipeMock(new[]
            {
                new Chunk { Bytes =  new byte[] { 0x00, 0x11, 0x22 }, Index = 0 },
                new Chunk { Bytes =  new byte[] { 0x33, 0x44, 0x55, 0x66 }, Index = 1 }
            });
            var stream = new MemoryStream();
            var writer = new ChunksWriter(pipe, new LoggerMock());
            writer.WriteToStream(stream, new CancellationToken(), 2, writeChunksLengths: true);
            
            Assert.Equal(bytes, stream.ToArray());
        }

        [Fact]
        public void TestCorruptedExpectedChunks()
        {
            var bytes = BitConverter.GetBytes(3).Concat(new byte[] { 0x00, 0x11, 0x22 })
                .Concat(BitConverter.GetBytes(4)).Concat(new byte[] { 0x33, 0x44, 0x55, 0x66 })
                .ToArray();
            var pipe = new PipeMock(new[]
            {
                new Chunk { Bytes =  new byte[] { 0x00, 0x11, 0x22 }, Index = 0 },
                new Chunk { Bytes =  new byte[] { 0x33, 0x44, 0x55, 0x66 }, Index = 1 }
            });
            var stream = new MemoryStream();
            var writer = new ChunksWriter(pipe, new LoggerMock());
            Assert.Throws<FileCorruptedException>(
                () => writer.WriteToStream(
                    stream, 
                    new CancellationToken(), 
                    expectedChunksCount: 222,
                    writeChunksLengths: true));
        }
        
        private class PipeMock : IPipe
        {
            public PipeMock(Chunk[] chunks)
            {
                _chunks = new Queue<Chunk>(chunks);
            }

            public Chunk Read(CancellationToken token)
            {
                return _chunks.Count > 0 ? _chunks.Dequeue() : throw new PipeClosedException();
            }

            public void Write(Chunk chunk, CancellationToken token)
            {
                throw new NotSupportedException();
            }

            public void Open()
            {
                throw new NotSupportedException();
            }

            public void Close()
            {
                throw new NotSupportedException();
            }
            
            private readonly Queue<Chunk> _chunks;
        }
    }
}