using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

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

        public void Start(CancellationToken token)
        {
            _outputPipe.Open();
            var processedStream = new MemoryStream();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var chunk = _inputPipe.Read(token);
                    using (var gzipStream = new GZipStream(new MemoryStream(chunk.Bytes), CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(processedStream);
                    }

                    var processedBytes = processedStream.ToArray();
                    _outputPipe.Write(new Chunk { Bytes = processedBytes, Index = chunk.Index }, token);
                    processedStream.Position = 0;
                    processedStream.SetLength(0);
                    _logger.Write(
                        $"Decompressed chunk #{chunk.Index} from {chunk.Bytes.Length} bytes to {processedBytes.Length}");
                }
                catch (PipeClosedException)
                {
                    _logger.Write("Decompressing complete");
                    break;
                }
                catch (Exception e)
                {
                    _logger.WriteError("Decompressing failed with error: " + e.Message);
                    _outputPipe.Close();
                    throw;
                }
            }
            
            _outputPipe.Close();
        }
        
        private readonly IPipe _inputPipe;
        private readonly IPipe _outputPipe;
        private readonly ILogger _logger;
    }
}