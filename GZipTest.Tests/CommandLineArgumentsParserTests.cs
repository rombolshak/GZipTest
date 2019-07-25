using Xunit;

namespace GZipTest.Tests
{
    public class CommandLineArgumentsParserTests
    {
        [Fact]
        public void AcceptsExactlyThreeArguments()
        {
            var parser = new CommandLineArgumentsParser();
            var result = parser.Parse(new[] {"", "", ""});
            Assert.Equal(ValidationResult.Success, result.ValidationResult);
            
            result = parser.Parse(new[] {"", "", "", ""});
            Assert.Equal(ValidationResult.TooManyArguments, result.ValidationResult);
            
            result = parser.Parse(new[] {"", ""});
            Assert.Equal(ValidationResult.TooFewArguments, result.ValidationResult);
        }
    }
}