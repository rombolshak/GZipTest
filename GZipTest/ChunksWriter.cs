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

        public void WriteToStream(Stream outputStream)
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
                    
                    outputStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
                }
                catch (PipeClosedException)
                {
                    foreach (var chunk in unorderedChunks.OrderBy(c => c.Index))
                    {
                        outputStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
                    }
                    
                    outputStream.Flush();
                    break;
                }
            }
        }
        
        private readonly IPipe _pipe;
    }
}