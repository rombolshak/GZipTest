namespace GZipTest.Tests
{
    public class LoggerMock : ILogger
    {
        public void Write(string message, string caller = "")
        {
        }

        public void WriteError(string message, string caller = "")
        {
        }
    }
}