using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace GZipTest.Tests
{
    public class ChunksReaderTests
    {
        [Fact]
        public void EmptyStreamProducesNoChunks()
        {
            var inputStream = new MemoryStream();
            var pipe = new PipeMock();
            
            var reader = new ChunksReader(pipe, chunkSize: 8);
            reader.ReadFromStream(inputStream);
            
            Assert.Empty(pipe.Chunks);
        }

        [Fact]
        public void SmallFileFitsInOneChunk()
        {
            var bytes = new byte[] { 0x11, 0x22, 0x33, 0x42 };
            var inputStream = new MemoryStream(bytes);
            var pipe = new PipeMock();

            var reader = new ChunksReader(pipe, chunkSize: 8);
            reader.ReadFromStream(inputStream);
            
            Assert.Single(pipe.Chunks);
            Assert.Equal(bytes, pipe.Chunks[0].Bytes);
        }

        [Fact]
        public void BigFileFitsInSeveralChunks()
        {
            var bytes = new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB };
            var inputStream = new MemoryStream(bytes);
            var pipe = new PipeMock();

            var reader = new ChunksReader(pipe, chunkSize: 8);
            reader.ReadFromStream(inputStream);
            
            Assert.Equal(2, pipe.Chunks.Count);
            Assert.Equal(8, pipe.Chunks[0].Bytes.Length);
            Assert.Equal(3, pipe.Chunks[1].Bytes.Length);
        }
        
        private class PipeMock : IPipe
        {
            public List<Chunk> Chunks { get; } = new List<Chunk>();
            
            public Chunk Read()
            {
                throw new NotSupportedException();
            }

            public void Write(Chunk chunk)
            {
                Chunks.Add(chunk);
            }
        }
    }
}