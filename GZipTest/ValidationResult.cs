namespace GZipTest
{
    public enum ValidationResult
    {
        Success,
        TooManyArguments,
        TooFewArguments,
        UnknownMode,
        SourceNotExists,
        PathIsIncorrect,
        SecurityViolation,
        PathNotFound,
        PathAlreadyExists
    }
}