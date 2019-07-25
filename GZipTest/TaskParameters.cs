namespace GZipTest
{
    public class TaskParameters
    {
        public TaskParameters(ProcessorMode mode)
        {
            Mode = mode;
        }

        public ProcessorMode Mode { get; }
    }

    public enum ProcessorMode
    {
        Compress,
        Decompress
    }
}