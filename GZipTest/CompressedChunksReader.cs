using System;
using System.IO;
using System.Threading;

namespace GZipTest
{
    public class CompressedChunksReader : IChunksReader
    {
        public CompressedChunksReader(IPipe pipe, int chunkSize, ILogger logger)
        {
            _pipe = pipe;
            _chunkSize = chunkSize;
            _logger = logger;
        }

        public void ReadFromStream(Stream inputStream, CancellationToken token)
        {
            _pipe.Open();
            var lengthBuffer = new byte[sizeof(int)];
            var buffer = new byte[0];
            var maxChunkLength = 0;
            var index = 0;
            int bytesRead;

            try
            {
                while (!token.IsCancellationRequested &&
                       (bytesRead = inputStream.Read(lengthBuffer, 0, lengthBuffer.Length)) > 0)
                {
                    if (bytesRead < lengthBuffer.Length)
                    {
                        throw new FileCorruptedException();
                    }

                    var chunkLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (chunkLength < 0 || chunkLength > _chunkSize * 10)
                    {
                        throw new FileCorruptedException();
                    }

                    if (chunkLength > maxChunkLength)
                    {
                        _logger.Write($"Increasing reading buffer to {chunkLength}");
                        maxChunkLength = chunkLength;
                        buffer = new byte[maxChunkLength];
                    }

                    bytesRead = inputStream.Read(buffer, 0, chunkLength);
                    if (bytesRead != chunkLength)
                    {
                        throw new FileCorruptedException();
                    }

                    var chunkBytes = new byte[chunkLength];
                    Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
                    _pipe.Write(new Chunk { Bytes = chunkBytes, Index = index }, token);
                    _logger.Write($"Read compressed chunk #{index} of {chunkLength} bytes");
                    index++;
                }

                _pipe.Close();
            }
            catch (Exception e)
            {
                _logger.WriteError("Archive reading failed with error: " + e.Message);
                _pipe.Close();
                throw;
            }
        }
        
        private readonly IPipe _pipe;
        private readonly int _chunkSize;
        private readonly ILogger _logger;
    }
}