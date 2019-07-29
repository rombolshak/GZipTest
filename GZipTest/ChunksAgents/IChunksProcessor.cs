using System.Threading;

namespace GZipTest.ChunksAgents
{
    public interface IChunksProcessor
    {
        void Start(CancellationToken token);
    }
}