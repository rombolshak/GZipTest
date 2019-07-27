namespace GZipTest
{
    public interface IPipe
    {
        Chunk Read();
        void Write(Chunk chunk);
        void Open();
        void Close();
    }
}