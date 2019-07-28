using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace GZipTest
{
    public class MultithreadingConsoleLogger : ILogger
    {
        public void Write(string message, [CallerFilePath]string caller = "")
        {
            Console.WriteLine($"{Path.GetFileNameWithoutExtension(caller)}:{Environment.CurrentManagedThreadId}: {message}");
        }
    }
}