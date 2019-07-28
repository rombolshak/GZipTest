using System;

namespace GZipTest
{
    public class TaskParameters
    {
        public TaskParameters(ProcessorMode mode, string sourceFullPath, string destinationFullPath)
        {
            Mode = mode;
            SourceFullPath = sourceFullPath;
            DestinationFullPath = destinationFullPath;
            ParallelismDegree = Environment.ProcessorCount;
            MaxElementsInPipe = ParallelismDegree * 2;
            ChunkSize = 1024 * 1024; // 1MB
        }

        public ProcessorMode Mode { get; }
        
        public string SourceFullPath { get; }
        
        public string DestinationFullPath { get; }
        
        public int ParallelismDegree { get; }
        
        public int ChunkSize { get; }
        
        public int MaxElementsInPipe { get; }
    }

    public enum ProcessorMode
    {
        Compress,
        Decompress
    }
}