using System;

namespace GZipTest.ChunksAgents
{
    public class FileCorruptedException : Exception
    {
        public override string Message => "Input archive is corrupted";
    }
}