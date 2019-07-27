using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GZipTest
{
    public class ChunksWriter
    {
        public ChunksWriter(IPipe pipe)
        {
            _pipe = pipe;
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
                    if (chunk.Index != index)
                    {
                        unorderedChunks.Add(chunk);
                        var found = unorderedChunks.FirstOrDefault(c => c.Index == index);
                        if (found == null)
                        {
                            continue;
                        }

                        chunk = found;
                    }
                    
                    WriteChunk(outputStream, chunk, writeChunksLengths);
                }
                catch (PipeClosedException)
                {
                    foreach (var chunk in unorderedChunks.OrderBy(c => c.Index))
                    {
                        WriteChunk(outputStream, chunk, writeChunksLengths);
                    }
                    
                    outputStream.Flush();
                    break;
                }
            }
        }

        private static void WriteChunk(Stream outputStream, Chunk chunk, bool writeChunksLengths)
        {
            if (writeChunksLengths)
            {
                outputStream.Write(BitConverter.GetBytes(chunk.Bytes.Length), 0,  sizeof(int));
            }
            
            outputStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
        }

        private readonly IPipe _pipe;
    }
}