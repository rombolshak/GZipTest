using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GZipTest
{
    public class ChunksWriter
    {
        public ChunksWriter(IPipe pipe, ILogger logger)
        {
            _pipe = pipe;
            _logger = logger;
        }

        public void WriteToStream(Stream outputStream, bool writeChunksLengths = false)
        {
            var index = 0;
            var unorderedChunks = new List<Chunk>();
            while (true)
            {
                try
                {
                    var chunk = _pipe.Read();
                    _logger.Write($"Requested to write chunk #{chunk.Index}, expecting #{index}");
                    if (chunk.Index != index)
                    {
                        unorderedChunks.Add(chunk);
                        var found = unorderedChunks.FirstOrDefault(c => c.Index == index);
                        if (found == null)
                        {
                            _logger.Write("Chunk order is incorrect, waiting next");
                            continue;
                        }

                        _logger.Write($"Found chunk #{index} in queue, writing");
                        chunk = found;
                    }
                    
                    WriteChunk(outputStream, chunk, writeChunksLengths);
                    index++;
                }
                catch (PipeClosedException)
                {
                    foreach (var chunk in unorderedChunks.OrderBy(c => c.Index))
                    {
                        WriteChunk(outputStream, chunk, writeChunksLengths);
                    }
                    
                    _logger.Write("Writing complete");
                    outputStream.Flush();
                    break;
                }
            }
        }

        private void WriteChunk(Stream outputStream, Chunk chunk, bool writeChunksLengths)
        {
            _logger.Write($"Writing chunk #{chunk.Index} of {chunk.Bytes.Length} bytes");
            if (writeChunksLengths)
            {
                outputStream.Write(BitConverter.GetBytes(chunk.Bytes.Length), 0,  sizeof(int));
            }
            
            outputStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
        }

        private readonly IPipe _pipe;
        private readonly ILogger _logger;
    }
}