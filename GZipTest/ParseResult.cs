namespace GZipTest
{
    public class ParseResult
    {
        public ValidationResult ValidationResult { get; private set; }

        public static ParseResult Successful() => new ParseResult { ValidationResult = ValidationResult.Success };

        public static ParseResult Error(ValidationResult validationResult) =>
            new ParseResult { ValidationResult = validationResult };
    }
}