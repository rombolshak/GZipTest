using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GZipTest
{
    public class TaskProcessor
    {
        public TaskProcessor(ILogger logger)
        {
            _logger = logger;
        }

        public Task Start(TaskParameters parameters)
        {
            _logger.Write($"Starting task with parameters: {parameters}");
            
            var inputFile = File.OpenRead(parameters.SourceFullPath);
            var outputFile = File.Create(parameters.DestinationFullPath);
            var inputPipe = new Pipe(parameters.MaxElementsInPipe, _logger);
            var outputPipe = new Pipe(parameters.MaxElementsInPipe, _logger);
            
            var writer = new ChunksWriter(outputPipe, _logger);
            IChunksReader reader = null;
            IEnumerable<IChunksProcessor> processors = null;
            
            switch (parameters.Mode)
            {
                case ProcessorMode.Compress:
                    reader = new ChunksReader(inputPipe, parameters.ChunkSize, _logger);
                    processors = Enumerable.Range(0, parameters.ParallelismDegree).Select(
                        _ => new ChunksCompressor(inputPipe, outputPipe, _logger));
                    break;
                
                case ProcessorMode.Decompress:
                    reader = new CompressedChunksReader(inputPipe, parameters.ChunkSize, _logger);
                    processors = Enumerable.Range(0, parameters.ParallelismDegree).Select(
                        _ => new ChunksDecompressor(inputPipe, outputPipe, _logger));
                    break;
            }

            var actions = new Action[]
            {
                () =>
                {
                    (reader ?? throw new ArgumentNullException()).ReadFromStream(inputFile);
                    inputFile.Close();
                },
                () =>
                {
                    writer.WriteToStream(outputFile, writeChunksLengths: parameters.Mode == ProcessorMode.Compress);
                    outputFile.Close();
                },
            }.Concat((processors ?? throw new ArgumentNullException())
                .Select<IChunksProcessor, Action>(processor => processor.Start)).ToArray();
                    
            return Task.StartInParallel(actions, _logger);
        }
        
        private readonly ILogger _logger;
    }
}