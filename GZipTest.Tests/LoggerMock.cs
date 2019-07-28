using System;

namespace GZipTest.Tests
{
    public class LoggerMock : ILogger
    {
        public void Write(string message, string caller = "")
        {
            Console.WriteLine(message);
        }
    }
}