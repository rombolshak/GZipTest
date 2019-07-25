using System;
using System.IO;
using Xunit;

namespace GZipTest.Tests
{
    public class CommandLineArgumentsValidatorTests
    {
        public CommandLineArgumentsValidatorTests()
        {
            File.Create(ExistingSource).Dispose();
        }

        [Fact]
        public void AcceptsExactlyThreeArguments()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);

            result = parser.Parse(new[] { "", "", "", "" });
            Assert.Equal(ValidationResult.TooManyArguments, result.ValidationResult);

            result = parser.Parse(new[] { "", "" });
            Assert.Equal(ValidationResult.TooFewArguments, result.ValidationResult);
        }

        [Fact]
        public void FirstArgIsProcessMode()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            Assert.Equal(ProcessorMode.Compress, result.TaskParameters.Mode);

            result = parser.Parse(new[] { "decompress", ExistingSource, Destination });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            Assert.Equal(ProcessorMode.Decompress, result.TaskParameters.Mode);

            result = parser.Parse(new[] { "random-string", ExistingSource, Destination });
            Assert.Equal(ValidationResult.UnknownMode, result.ValidationResult);
        }

        [Fact]
        public void SourceMustBeCorrectPath()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", "incorrect:file^name", Destination });
            Assert.Equal(ValidationResult.PathIsIncorrect, result.ValidationResult);
        }

        [Fact]
        public void SourceFileMustExist()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", "not-exists.txt", Destination });
            Assert.Equal(ValidationResult.SourceNotExists, result.ValidationResult);

            result = parser.Parse(new[] { "compress", "A:\\file.mp3", Destination });
            Assert.Equal(ValidationResult.SourceNotExists, result.ValidationResult);
        }

        [Fact]
        public void SourceFileRelativePathConvertsToFull()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            
            var fullPath = Path.Combine(Environment.CurrentDirectory, ExistingSource);
            Assert.Equal(fullPath,result.TaskParameters.SourceFullPath);
            
            result = parser.Parse(new[] { "compress", fullPath, Destination });
            Assert.Equal(fullPath,result.TaskParameters.SourceFullPath);
        }

        [Fact]
        public void DestinationMustBeCorrectPath()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", ExistingSource, "   " });
            Assert.Equal(ValidationResult.PathIsIncorrect, result.ValidationResult);
        }

        [Fact]
        public void DestinationMustNotExist()
        {
            var parser = new CommandLineArgumentsValidator();
            File.Create("111.txt").Dispose();
            var result = parser.Parse(new[] { "compress", ExistingSource, "111.txt" });
            File.Delete("111.txt");
            Assert.Equal(ValidationResult.PathAlreadyExists, result.ValidationResult);
        }

        [Fact]
        public void DestinationFileRelativePathConvertsToFull()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            
            var fullPath = Path.Combine(Environment.CurrentDirectory, Destination);
            Assert.Equal(fullPath,result.TaskParameters.DestinationFullPath);
            
            
            result = parser.Parse(new[] { "compress", ExistingSource, fullPath });
            Assert.Equal(fullPath,result.TaskParameters.DestinationFullPath);
        }
        
        private const string ExistingSource = "in.txt";
        private const string Destination = "out.txt";
    }
}