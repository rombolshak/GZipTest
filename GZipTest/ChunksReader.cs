using System;
using System.IO;
using System.Threading;

namespace GZipTest
{
    public class ChunksReader : IChunksReader
    {
        public ChunksReader(IPipe pipe, int chunkSize, ILogger logger)
        {
            _pipe = pipe;
            _chunkSize = chunkSize;
            _logger = logger;
        }

        public void ReadFromStream(Stream inputStream, CancellationToken token)
        {
            _pipe.Open();
            var buffer = new byte[_chunkSize];
            var index = 0;
            int bytesRead;

            try
            {
                while (!token.IsCancellationRequested && 
                       (bytesRead = inputStream.Read(buffer, 0, _chunkSize)) > 0)
                {
                    var chunkBytes = new byte[bytesRead];
                    Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
                    _pipe.Write(new Chunk { Bytes = chunkBytes, Index = index });
                    _logger.Write($"Read chunk #{index}");
                    index++;
                }

                _logger.Write("Reading complete");
                _pipe.Close();
            }
            catch (Exception e)
            {
                _logger.WriteError("Reading failed with error: " + e.Message);
                _pipe.Close();
                throw;
            }
            
        }
        
        private readonly IPipe _pipe;
        private readonly int _chunkSize;
        private readonly ILogger _logger;
    }
}