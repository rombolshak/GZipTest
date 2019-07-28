using System.Threading;

namespace GZipTest
{
    public interface IChunksProcessor
    {
        void Start(CancellationToken token);
    }
}