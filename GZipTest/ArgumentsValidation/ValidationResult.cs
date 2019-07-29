namespace GZipTest.ArgumentsValidation
{
    public class ValidationResult
    {
        public ValidationError ValidationError { get; private set; }

        public string AdditionalValidationData { get; private set; } = "";
        
        public TaskParameters TaskParameters { get; private set; }

        public static ValidationResult Successful(TaskParameters taskParameters) => new ValidationResult
        {
            ValidationError = ValidationError.Success,
            TaskParameters = taskParameters
        };

        public static ValidationResult Error(ValidationError validationError, string additionalData = "") => new ValidationResult
        {
            ValidationError = validationError,
            AdditionalValidationData = additionalData
        };
    }
}