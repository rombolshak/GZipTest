using System.Threading;

namespace GZipTest
{
    public interface IPipe
    {
        Chunk Read(CancellationToken token);
        void Write(Chunk chunk, CancellationToken token);
        void Open();
        void Close();
    }
}