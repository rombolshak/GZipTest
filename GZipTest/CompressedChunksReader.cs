using System;
using System.IO;

namespace GZipTest
{
    public class CompressedChunksReader
    {
        public CompressedChunksReader(IPipe pipe, int chunkSize)
        {
            _pipe = pipe;
            _chunkSize = chunkSize;
        }

        public void ReadFromStream(Stream inputStream)
        {
            _pipe.Open();
            var lengthBuffer = new byte[4];
            var buffer = new byte[0];
            var maxChunkLength = 0;
            var index = 0;
            int bytesRead;
            while ((bytesRead = inputStream.Read(lengthBuffer, 0, 4)) > 0)
            {
                if (bytesRead < 4)
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
                _pipe.Write(new Chunk { Bytes = chunkBytes, Index = index });
                index++;
            }

            _pipe.Close();
        }
        
        private readonly IPipe _pipe;
        private readonly int _chunkSize;
    }
}