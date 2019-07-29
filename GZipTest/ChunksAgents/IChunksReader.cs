using System.IO;
using System.Threading;

namespace GZipTest.ChunksAgents
{
    public interface IChunksReader
    {
        void ReadFromStream(Stream inputStream, CancellationToken token);
    }
}