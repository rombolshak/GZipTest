using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GZipTest.ChunksAgents;

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

            var cancellationTokenSource = new CancellationTokenSource();
            var inputFile = File.OpenRead(parameters.SourceFullPath);
            var outputFile = File.Create(parameters.DestinationFullPath);
            var inputPipe = new Pipe(parameters.MaxElementsInPipe);
            var outputPipe = new Pipe(parameters.MaxElementsInPipe);

            var expectedChunksCount = GetExpectedChunksCount(parameters, inputFile, outputFile);

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
                    (reader ?? throw new ArgumentNullException()).ReadFromStream(inputFile, cancellationTokenSource.Token);
                    inputFile.Close();
                },
                () =>
                {
                    writer.WriteToStream(
                        outputFile, 
                        cancellationTokenSource.Token, 
                        expectedChunksCount,
                        writeChunksLengths: parameters.Mode == ProcessorMode.Compress);
                    outputFile.Close();
                },
            }.Concat((processors ?? throw new ArgumentNullException())
                .Select<IChunksProcessor, Action>(processor => () => processor.Start(cancellationTokenSource.Token))).ToArray();
                    
            return Task.StartInParallel(actions, cancellationTokenSource, _logger);
        }

        private int GetExpectedChunksCount(TaskParameters parameters, FileStream inputFile, FileStream outputFile)
        {
            int expectedChunksCount;
            if (parameters.Mode == ProcessorMode.Compress)
            {
                expectedChunksCount = (int) Math.Ceiling(inputFile.Length * 1.0 / parameters.ChunkSize);
                outputFile.Write(_magicHeader, 0, _magicHeader.Length);
                outputFile.Write(BitConverter.GetBytes(expectedChunksCount), 0, sizeof(int));
            }
            else
            {
                byte[] buffer = new byte[sizeof(int)];
                inputFile.Read(buffer, 0, buffer.Length);
                if (!buffer.SequenceEqual(_magicHeader))
                {
                    throw new Exception($"File {parameters.SourceFullPath} is not an archive");
                }

                inputFile.Read(buffer, 0, sizeof(int));
                expectedChunksCount = BitConverter.ToInt32(buffer, 0);
            }

            return expectedChunksCount;
        }

        private readonly ILogger _logger;
        private readonly byte[] _magicHeader = { 0x1a, 0x2b, 0x3c, 0x4d };
    }
}