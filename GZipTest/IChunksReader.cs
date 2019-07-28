using System.IO;
using System.Threading;

namespace GZipTest
{
    public interface IChunksReader
    {
        void ReadFromStream(Stream inputStream, CancellationToken token);
    }
}