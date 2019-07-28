using System.IO;
using System.IO.Compression;

namespace GZipTest
{
    public class ChunksCompressor : IChunksProcessor
    {
        public ChunksCompressor(IPipe inputPipe, IPipe outputPipe, ILogger logger)
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
                    using (var gzipStream = new GZipStream(processedStream, CompressionMode.Compress, leaveOpen: true))
                    {
                        gzipStream.Write(chunk.Bytes, 0, chunk.Bytes.Length);
                    }

                    _outputPipe.Write(new Chunk { Bytes = processedStream.ToArray(), Index = chunk.Index });
                    processedStream.Position = 0;
                    _logger.Write($"Compressed chunk #{chunk.Index}");
                }
                catch (PipeClosedException)
                {
                    _logger.Write("Compressing complete");
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