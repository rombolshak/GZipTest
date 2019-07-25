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
            var result = parser.Parse(new[] { "compress", ExistingSource, "" });
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
            var result = parser.Parse(new[] { "compress", ExistingSource, "" });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            Assert.Equal(ProcessorMode.Compress, result.TaskParameters.Mode);

            result = parser.Parse(new[] { "decompress", ExistingSource, "" });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            Assert.Equal(ProcessorMode.Decompress, result.TaskParameters.Mode);

            result = parser.Parse(new[] { "random-string", ExistingSource, "" });
            Assert.Equal(ValidationResult.UnknownMode, result.ValidationResult);
        }

        [Fact]
        public void SourceMustBeCorrectPath()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", "incorrect:file^name", "" });
            Assert.Equal(ValidationResult.PathIsIncorrect, result.ValidationResult);
        }

        [Fact]
        public void SourceFileMustExist()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", "not-exists.txt", "" });
            Assert.Equal(ValidationResult.SourceNotExists, result.ValidationResult);

            result = parser.Parse(new[] { "compress", "A:\\file.mp3", "" });
            Assert.Equal(ValidationResult.SourceNotExists, result.ValidationResult);
        }

        [Fact]
        public void SourceFileRelativePathConvertsToFull()
        {
            var parser = new CommandLineArgumentsValidator();
            var result = parser.Parse(new[] { "compress", ExistingSource, "" });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            Assert.Equal(
                Path.Combine(Environment.CurrentDirectory, ExistingSource),
                result.TaskParameters.SourceFullPath);
        }
        
        private const string ExistingSource = "a.txt";
    }
}