using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;

namespace GZipTest.Tests
{
    public class TaskProcessorTests
    {
        [Fact]
        public void OverallTest()
        {
            const string text = "Test test test, ho ho ho";
            const string compressedTxt = "compressed.txt";
            const string decompressedTxt = "decompressed.txt";
            File.WriteAllText("in.txt", text);
            var parameters = new TaskParameters(ProcessorMode.Compress, "in.txt", compressedTxt);
            var processor = new TaskProcessor(new LoggerMock());
            var task = processor.Start(parameters);
            task.Wait();
            
            Assert.True(File.Exists(compressedTxt));
            Assert.NotEqual(Encoding.UTF8.GetBytes(text), File.ReadAllBytes(compressedTxt));
            
            parameters = new TaskParameters(ProcessorMode.Decompress, compressedTxt, decompressedTxt);
            task = processor.Start(parameters);
            task.Wait();
            
            Assert.True(File.Exists(decompressedTxt));
            Assert.Equal(text, File.ReadAllText(decompressedTxt));
        }

        [Fact]
        public void TestFileWithSeveralChunks()
        {
            var text = string.Concat(Enumerable.Repeat("Test test test, ho ho ho. ", 100500));
            const string compressedTxt = "compressed.txt";
            const string decompressedTxt = "decompressed.txt";
            File.WriteAllText("in.txt", text);
            var parameters = new TaskParameters(ProcessorMode.Compress, "in.txt", compressedTxt);
            var processor = new TaskProcessor(new LoggerMock());
            var task = processor.Start(parameters);
            task.Wait();
            
            Assert.True(File.Exists(compressedTxt));
            Assert.NotEqual(Encoding.UTF8.GetBytes(text), File.ReadAllBytes(compressedTxt));
            
            parameters = new TaskParameters(ProcessorMode.Decompress, compressedTxt, decompressedTxt);
            task = processor.Start(parameters);
            task.Wait();
            
            Assert.True(File.Exists(decompressedTxt));
            Assert.False(task.IsErrorOccured);
            Assert.Equal(text, File.ReadAllText(decompressedTxt));
        }

        [Fact]
        public void TestErrorOnDecompress()
        {
            const string text = "Test test test, ho ho ho";
            const string compressedTxt = "compressed.gz";
            const string decompressedTxt = "decompressed.txt";
            File.WriteAllText("in.txt", text);
            var parameters = new TaskParameters(ProcessorMode.Compress, "in.txt", compressedTxt);
            var processor = new TaskProcessor(new LoggerMock());
            var task = processor.Start(parameters);
            task.Wait();
            
            File.AppendAllText(compressedTxt, "a0b4");
            
            parameters = new TaskParameters(ProcessorMode.Decompress, compressedTxt, decompressedTxt);
            task = processor.Start(parameters);
            task.Wait();
            
            Assert.True(task.IsErrorOccured);
        }

        [Fact]
        public void TestTaskAbortion()
        {
            var text = string.Concat(Enumerable.Repeat("Test test test, ho ho ho. ", 200500));
            const string compressedTxt = "compressed.txt";
            File.WriteAllText("in.txt", text);
            var parameters = new TaskParameters(ProcessorMode.Compress, "in.txt", compressedTxt);
            var processor = new TaskProcessor(new LoggerMock());
            var task = processor.Start(parameters);
            
            task.Abort();
            task.Wait();
            Assert.True(task.IsErrorOccured);
        }

        [Fact]
        public void TestMagicHeaderMissing()
        {
            const string text = "Test test test, ho ho ho. ";
            const string compressedTxt = "compressed.txt";
            File.WriteAllText("in.txt", text);
            var parameters = new TaskParameters(ProcessorMode.Decompress, "in.txt", compressedTxt);
            var processor = new TaskProcessor(new LoggerMock());
            Assert.Throws<Exception>(() => processor.Start(parameters).Wait());
        }
    }
}