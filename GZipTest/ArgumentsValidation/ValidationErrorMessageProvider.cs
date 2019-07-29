using System;

namespace GZipTest.ArgumentsValidation
{
    public static class ValidationErrorMessageProvider
    {
        public static string GetErrorMessage(ValidationResult result)
        {
            switch (result.ValidationError)
            {
                case ValidationError.Success:
                    return string.Empty;
                case ValidationError.TooManyArguments:
                    return "Too many arguments. Syntax: GZipTest.exe [compress|decompress] [input_filepath] [output_filepath]";
                case ValidationError.TooFewArguments:
                    return "Insufficient arguments. Syntax: GZipTest.exe [compress|decompress] [input_filepath] [output_filepath]";
                case ValidationError.UnknownMode:
                    return $"Unknown mode {result.AdditionalValidationData} specified. Valid values are 'compress' or 'decompress'";
                case ValidationError.SourceNotExists:
                    return $"Input file {result.AdditionalValidationData} does not exists";
                case ValidationError.PathIsIncorrect:
                    return $"Specified path {result.AdditionalValidationData} is incorrect. Please check for not allowed symbols " +
                           "and/or path length which should be less than 248 symbols for directory and 260 in total";
                case ValidationError.SecurityViolation:
                    return $"Security access violation for file {result.AdditionalValidationData}";
                case ValidationError.PathNotFound:
                    return $"File or directory specified ({result.AdditionalValidationData}) does not exists";
                case ValidationError.PathAlreadyExists:
                    return $"File {result.AdditionalValidationData} is already exists";
                case ValidationError.Other:
                    return $"Error occured while checking file {result.AdditionalValidationData}";
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}