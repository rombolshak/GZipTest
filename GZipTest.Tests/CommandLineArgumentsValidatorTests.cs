using System;
using System.IO;
using System.Threading;
using GZipTest.ArgumentsValidation;
using Xunit;

namespace GZipTest.Tests
{
    public class CommandLineArgumentsValidatorTests
    {
        public CommandLineArgumentsValidatorTests()
        {
            File.Create(ExistingSource).Dispose();
            File.Delete(Destination);
            Thread.Sleep(100); // wait while OS performs file operations
        }

        [Fact]
        public void AcceptsExactlyThreeArguments()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            var result = validator.Validate(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationError.Success, result.ValidationError);

            result = validator.Validate(new[] { "", "", "", "" });
            Assert.Equal(ValidationError.TooManyArguments, result.ValidationError);

            result = validator.Validate(new[] { "", "" });
            Assert.Equal(ValidationError.TooFewArguments, result.ValidationError);
        }

        [Fact]
        public void FirstArgIsProcessMode()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            var result = validator.Validate(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationError.Success, result.ValidationError);
            Assert.Equal(ProcessorMode.Compress, result.TaskParameters.Mode);

            result = validator.Validate(new[] { "decompress", ExistingSource, Destination });
            Assert.Equal(ValidationError.Success, result.ValidationError);
            Assert.Equal(ProcessorMode.Decompress, result.TaskParameters.Mode);

            result = validator.Validate(new[] { "random-string", ExistingSource, Destination });
            Assert.Equal(ValidationError.UnknownMode, result.ValidationError);
        }

        [Fact]
        public void SourceMustBeCorrectPath()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            var result = validator.Validate(new[] { "compress", "incorrect:file^name", Destination });
            Assert.Equal(ValidationError.PathIsIncorrect, result.ValidationError);
        }

        [Fact]
        public void SourceFileMustExist()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            var result = validator.Validate(new[] { "compress", "not-exists.txt", Destination });
            Assert.Equal(ValidationError.SourceNotExists, result.ValidationError);

            result = validator.Validate(new[] { "compress", "A:\\file.mp3", Destination });
            Assert.Equal(ValidationError.SourceNotExists, result.ValidationError);
        }

        [Fact]
        public void SourceFileRelativePathConvertsToFull()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            var result = validator.Validate(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationError.Success, result.ValidationError);
            
            var fullPath = Path.Combine(Environment.CurrentDirectory, ExistingSource);
            Assert.Equal(fullPath,result.TaskParameters.SourceFullPath);
            
            result = validator.Validate(new[] { "compress", fullPath, Destination });
            Assert.Equal(fullPath,result.TaskParameters.SourceFullPath);
        }

        [Fact]
        public void DestinationMustBeCorrectPath()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            var result = validator.Validate(new[] { "compress", ExistingSource, "   " });
            Assert.Equal(ValidationError.PathIsIncorrect, result.ValidationError);
        }

        [Fact]
        public void DestinationMustNotExist()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            File.Create("111.txt").Dispose();
            var result = validator.Validate(new[] { "compress", ExistingSource, "111.txt" });
            File.Delete("111.txt");
            Assert.Equal(ValidationError.PathAlreadyExists, result.ValidationError);
        }

        [Fact]
        public void DestinationFileRelativePathConvertsToFull()
        {
            var validator = new CommandLineArgumentsValidator(new LoggerMock());
            var result = validator.Validate(new[] { "compress", ExistingSource, Destination });
            Assert.Equal(ValidationError.Success, result.ValidationError);
            
            var fullPath = Path.Combine(Environment.CurrentDirectory, Destination);
            Assert.Equal(fullPath,result.TaskParameters.DestinationFullPath);
            
            result = validator.Validate(new[] { "compress", ExistingSource, fullPath });
            Assert.Equal("", result.AdditionalValidationData);
            Assert.Equal(ValidationError.Success, result.ValidationError);
            Assert.Equal(fullPath,result.TaskParameters.DestinationFullPath);
        }
        
        private const string ExistingSource = "in.txt";
        private const string Destination = "out.txt";
    }
}