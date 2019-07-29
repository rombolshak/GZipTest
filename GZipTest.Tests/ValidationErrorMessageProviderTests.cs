using System;
using System.Linq;
using GZipTest.ArgumentsValidation;
using Xunit;

namespace GZipTest.Tests
{
    public class ValidationErrorMessageProviderTests
    {
        [Fact]
        public void AllValidationErrorsMustBeDescribed()
        {
            foreach (var error in 
                Enum.GetValues(typeof(ValidationError)).Cast<ValidationError>()
                .Except(new[] { ValidationError.Success }))
            {
                var validationResult = ValidationResult.Error(error);
                Assert.False(string.IsNullOrEmpty(ValidationErrorMessageProvider.GetErrorMessage(validationResult)), error.ToString());
            }
        }
    }
}