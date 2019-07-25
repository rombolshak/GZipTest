using System.Collections.Generic;

namespace GZipTest
{
    public class TaskParameters
    {
        public TaskParameters(ProcessorMode mode, string sourceFullPath, string destinationFullPath)
        {
            Mode = mode;
            SourceFullPath = sourceFullPath;
            DestinationFullPath = destinationFullPath;
        }

        public ProcessorMode Mode { get; }
        
        public string SourceFullPath { get; }
        
        public string DestinationFullPath { get; }
    }

    public enum ProcessorMode
    {
        Compress,
        Decompress
    }
}