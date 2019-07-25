namespace GZipTest
{
    public class CommandLineArgumentsParser
    {
        public ParseResult Parse(string[] args)
        {
            if (args.Length > 3)
            {
                return ParseResult.Error(ValidationResult.TooManyArguments);
            }

            if (args.Length < 3)
            {
                return ParseResult.Error(ValidationResult.TooFewArguments);
            }
            
            return ParseResult.Successful();
        }
    }
}