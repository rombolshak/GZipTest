using System;
using System.IO;

namespace GZipTest
{
    public class ChunksReader
    {
        public ChunksReader(IPipe pipe, int chunkSize)
        {
            _pipe = pipe;
            _chunkSize = chunkSize;
        }

        public void ReadFromStream(Stream inputStream)
        {
            var buffer = new byte[_chunkSize];
            int bytesRead;
            while ((bytesRead = inputStream.Read(buffer, 0, _chunkSize)) > 0)
            {
                var chunkBytes = new byte[bytesRead];
                Buffer.BlockCopy(buffer, 0, chunkBytes, 0, bytesRead);
                _pipe.Write(new Chunk { Bytes = chunkBytes });
            }
        }
        
        private readonly IPipe _pipe;
        private readonly int _chunkSize;
    }
}