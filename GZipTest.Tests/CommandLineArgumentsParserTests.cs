using Xunit;

namespace GZipTest.Tests
{
    public class CommandLineArgumentsParserTests
    {
        [Fact]
        public void AcceptsExactlyThreeArguments()
        {
            var parser = new CommandLineArgumentsParser();
            var result = parser.Parse(new[] { "compress", "", "" });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);

            result = parser.Parse(new[] { "", "", "", "" });
            Assert.Equal(ValidationResult.TooManyArguments, result.ValidationResult);

            result = parser.Parse(new[] { "", "" });
            Assert.Equal(ValidationResult.TooFewArguments, result.ValidationResult);
        }

        [Fact]
        public void FirstArgIsProcessMode()
        {
            var parser = new CommandLineArgumentsParser();
            var result = parser.Parse(new[] { "compress", "", "" });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            Assert.Equal(ProcessorMode.Compress, result.TaskParameters.Mode);
            
            result = parser.Parse(new[] { "decompress", "", "" });
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            Assert.Equal(ProcessorMode.Decompress, result.TaskParameters.Mode);

            result = parser.Parse(new[] { "random-string", "", "" });
            Assert.Equal(ValidationResult.UnknownMode, result.ValidationResult);
        }
    }
}