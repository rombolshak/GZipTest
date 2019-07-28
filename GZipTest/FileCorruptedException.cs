using System;

namespace GZipTest
{
    public class FileCorruptedException : Exception
    {
        public override string Message => "Input archive is corrupted";
    }
}