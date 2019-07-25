namespace GZipTest
{
    public class TaskParameters
    {
        public TaskParameters(ProcessorMode mode, string sourceFullPath)
        {
            Mode = mode;
            SourceFullPath = sourceFullPath;
        }

        public ProcessorMode Mode { get; }
        
        public string SourceFullPath { get; }
    }

    public enum ProcessorMode
    {
        Compress,
        Decompress
    }
}