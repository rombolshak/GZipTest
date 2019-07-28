using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GZipTest
{
    public class TaskProcessor
    {
        public Task Start(TaskParameters parameters)
        {
            var inputFile = File.OpenRead(parameters.SourceFullPath);
            var outputFile = File.Create(parameters.DestinationFullPath);
            var inputPipe = new Pipe(parameters.MaxElementsInPipe);
            var outputPipe = new Pipe(parameters.MaxElementsInPipe);
            
            var writer = new ChunksWriter(outputPipe);
            IChunksReader reader = null;
            IEnumerable<IChunksProcessor> processors = null;
            
            switch (parameters.Mode)
            {
                case ProcessorMode.Compress:
                    reader = new ChunksReader(inputPipe, parameters.ChunkSize);
                    processors = Enumerable.Range(0, parameters.ParallelismDegree).Select(
                        _ => new ChunksCompressor(inputPipe, outputPipe));
                    break;
                
                case ProcessorMode.Decompress:
                    reader = new CompressedChunksReader(inputPipe, parameters.ChunkSize);
                    processors = Enumerable.Range(0, parameters.ParallelismDegree).Select(
                        _ => new ChunksDecompressor(inputPipe, outputPipe));
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
                    
            return Task.StartInParallel(actions);
        }
    }
}