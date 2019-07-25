using System;

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

            var isModeValid = TryParseProcessorMode(args[0], out var mode);
            if (!isModeValid)
            {
                return ParseResult.Error(ValidationResult.UnknownMode);
            }
            
            return ParseResult.Successful(new TaskParameters(mode));
        }

        private bool TryParseProcessorMode(string value, out ProcessorMode mode)
        {
            if (value.Equals("compress", StringComparison.InvariantCultureIgnoreCase))
            {
                mode = ProcessorMode.Compress;
                return true;
            }
            
            if (value.Equals("decompress", StringComparison.InvariantCultureIgnoreCase))
            {
                mode = ProcessorMode.Decompress;
                return true;
            }

            mode = default(ProcessorMode);
            return false;
        }
    }
}