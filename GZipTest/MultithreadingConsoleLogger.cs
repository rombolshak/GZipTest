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

        public void WriteError(string message, [CallerFilePath]string caller = "")
        {
            Console.Error.WriteLine();
            Console.Error.WriteLine("====================");
            Console.Error.WriteLine(message);
            Console.Error.WriteLine("====================");
            Console.Error.WriteLine();
        }
    }
}