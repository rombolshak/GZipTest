namespace GZipTest
{
    public interface IPipe
    {
        Chunk Read();
        void Write(Chunk chunk);
    }
}