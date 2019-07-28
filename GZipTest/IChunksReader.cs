using System.IO;

namespace GZipTest
{
    public interface IChunksReader
    {
        void ReadFromStream(Stream inputStream);
    }
}