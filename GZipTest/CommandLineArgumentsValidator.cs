using System;
using System.IO;
using System.Security;

namespace GZipTest
{
    public class CommandLineArgumentsValidator
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

            var sourcePathValidationResult = TryGetFilePath(args[1], FileMode.Open, out var sourceFullPath);
            if (sourcePathValidationResult != ValidationResult.Success)
            {
                return ParseResult.Error(sourcePathValidationResult == ValidationResult.FileNotFound
                    ? ValidationResult.SourceNotExists
                    : sourcePathValidationResult);
            }
            
            return ParseResult.Successful(new TaskParameters(mode, sourceFullPath));
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

        private ValidationResult TryGetFilePath(string value, FileMode mode, out string fullPath)
        {
            fullPath = string.Empty;
            try
            {
                fullPath = Path.GetFullPath(value);
                using (File.Open(fullPath, mode))
                {
                    return ValidationResult.Success;
                }
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is NotSupportedException ||
                                       ex is PathTooLongException)
            {
                return ValidationResult.PathIsIncorrect;
            }
            catch (Exception ex) when (ex is SecurityException ||
                                       ex is UnauthorizedAccessException)
            {
                return ValidationResult.SecurityViolation;
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException ||
                                       ex is FileNotFoundException)
            {
                return ValidationResult.FileNotFound;
            }
        }
    }
}