namespace GZipTest
{
    public class ParseResult
    {
        public ValidationResult ValidationResult { get; private set; }
        
        public TaskParameters TaskParameters { get; private set; }

        public static ParseResult Successful(TaskParameters taskParameters) => new ParseResult
        {
            ValidationResult = ValidationResult.Success,
            TaskParameters = taskParameters
        };

        public static ParseResult Error(ValidationResult validationResult) => new ParseResult
        {
            ValidationResult = validationResult
        };
    }
}