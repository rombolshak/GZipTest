using System.Threading;
using Xunit;

namespace GZipTest.Tests
{
    public class ChunksProcessorTests
    {
        [Fact]
        public void TestCompressDecompress()
        {
            var logger = new LoggerMock();
            var inputPipe = new Pipe(8);
            var middlePipe = new Pipe(8);
            var outputPipe = new Pipe(8);
            var compressor = new ChunksCompressor(inputPipe, middlePipe, logger);
            var decompressor = new ChunksDecompressor(middlePipe, outputPipe, logger);
            var bytes = new byte[] { 0x11, 0x22, 0x11, 0x42 };
            inputPipe.Open();
            
            var compressorThread = new Thread(() => compressor.Start());
            compressorThread.Start();
            inputPipe.Write(new Chunk { Bytes = bytes });
            var compressedChunk = middlePipe.Read();
            Assert.NotEqual(bytes, compressedChunk.Bytes);
            
            var decompressorThread = new Thread(() => decompressor.Start());
            decompressorThread.Start();
            inputPipe.Write(new Chunk { Bytes = bytes });
            inputPipe.Close();
            
            var result = outputPipe.Read();
            Assert.Equal(bytes, result.Bytes);

            compressorThread.Join();
            decompressorThread.Join();
        }
    }
}