namespace GZipTest.ArgumentsValidation
{
    public enum ValidationError
    {
        Success,
        TooManyArguments,
        TooFewArguments,
        UnknownMode,
        SourceNotExists,
        PathIsIncorrect,
        SecurityViolation,
        PathNotFound,
        PathAlreadyExists,
        Other
    }
}