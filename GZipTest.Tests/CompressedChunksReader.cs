using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GZipTest.ChunksAgents;
using Xunit;

namespace GZipTest.Tests
{
    public class CompressedChunksReaderTests
    {
        [Fact]
        public void EmptyStreamProducesNoChunks()
        {
            var pipe = new PipeMock();
            var reader = new CompressedChunksReader(pipe, 4, new LoggerMock());
            var stream = new MemoryStream();
            reader.ReadFromStream(stream, new CancellationToken());
            Assert.Empty(pipe.Chunks);
        }

        [Fact]
        public void TestSingleChunk()
        {
            var pipe = new PipeMock();
            var reader = new CompressedChunksReader(pipe, 4, new LoggerMock());
            var bytes = new byte[] { 0x12, 0x34 };
            var stream = new MemoryStream(BitConverter.GetBytes(bytes.Length).Concat(bytes).ToArray());
            reader.ReadFromStream(stream, new CancellationToken());

            Assert.Single(pipe.Chunks);
            Assert.Equal(bytes, pipe.Chunks[0].Bytes);
        }

        [Fact]
        public void TestSeveralChunks()
        {
            var pipe = new PipeMock();
            var reader = new CompressedChunksReader(pipe, 4, new LoggerMock());
            var bytes1 = new byte[] { 0x12, 0x34 };
            var bytes2 = new byte[] { 0x56, 0x78, 0x90, 0xAB, 0xCD };
            var stream = new MemoryStream(
                BitConverter.GetBytes(bytes1.Length).Concat(bytes1)
                    .Concat(BitConverter.GetBytes(bytes2.Length)).Concat(bytes2)
                    .ToArray());
            reader.ReadFromStream(stream, new CancellationToken());

            Assert.Equal(2, pipe.Chunks.Count);
            
            Assert.Equal(bytes1, pipe.Chunks[0].Bytes);
            Assert.Equal(0, pipe.Chunks[0].Index);
            
            Assert.Equal(bytes2, pipe.Chunks[1].Bytes);
            Assert.Equal(1, pipe.Chunks[1].Index);
        }

        [Fact]
        public void TestCorruptedLengthHeader()
        {
            var pipe = new PipeMock();
            var reader = new CompressedChunksReader(pipe, 4, new LoggerMock());
            var bytes1 = new byte[] { 0x12, 0x34 };
            var stream = new MemoryStream(bytes1);
            
            Assert.Throws<FileCorruptedException>(() => reader.ReadFromStream(stream, new CancellationToken()));
        }

        [Fact]
        public void TestIncorrectLengthHeader()
        {
            var pipe = new PipeMock();
            var reader = new CompressedChunksReader(pipe, 4, new LoggerMock());
            var bytes1 = new byte[] { 0x12, 0x34 };
            var bytes2 = new byte[] { 0x56, 0x78, 0x90, 0xAB, 0xCD };
            var stream = new MemoryStream(
                BitConverter.GetBytes(bytes1.Length).Concat(bytes1)
                    .Concat(bytes2)
                    .ToArray());
            
            Assert.Throws<FileCorruptedException>(() => reader.ReadFromStream(stream, new CancellationToken()));
        }

        [Fact]
        public void TestMissingBody()
        {
            var pipe = new PipeMock();
            var reader = new CompressedChunksReader(pipe, 4, new LoggerMock());
            var bytes = new byte[] { 0x12, 0x34 };
            var stream = new MemoryStream(BitConverter.GetBytes(bytes.Length + 1).Concat(bytes).ToArray());
            
            Assert.Throws<FileCorruptedException>(() => reader.ReadFromStream(stream, new CancellationToken()));
        }
        
        private class PipeMock : IPipe
        {
            public List<Chunk> Chunks { get; } = new List<Chunk>();
            
            public Chunk Read(CancellationToken token)
            {
                throw new NotSupportedException();
            }

            public void Write(Chunk chunk, CancellationToken token)
            {
                Chunks.Add(chunk);
            }

            public void Open()
            {
            }

            public void Close()
            {
            }
        }
    }
}