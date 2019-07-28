using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    public class ChunksDecompressor : IChunksProcessor
    {
        public ChunksDecompressor(IPipe inputPipe, IPipe outputPipe, ILogger logger)
        {
            _inputPipe = inputPipe;
            _outputPipe = outputPipe;
            _logger = logger;
        }

        public void Start()
        {
            _outputPipe.Open();
            var processedStream = new MemoryStream();
            while (true)
            {
                try
                {
                    var chunk = _inputPipe.Read();
                    using (var gzipStream = new GZipStream(new MemoryStream(chunk.Bytes), CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(processedStream);
                    }

                    _outputPipe.Write(new Chunk { Bytes = processedStream.ToArray(), Index = chunk.Index });
                    processedStream.Position = 0;
                    _logger.Write($"Decompressed chunk #{chunk.Index}");
                }
                catch (PipeClosedException)
                {
                    _logger.Write("Decompressing complete");
                    _outputPipe.Close();
                    break;
                }
            }
        }
        
        private readonly IPipe _inputPipe;
        private readonly IPipe _outputPipe;
        private readonly ILogger _logger;
    }
}