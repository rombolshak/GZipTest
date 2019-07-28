using System;
using System.IO;
using System.Security;

namespace GZipTest
{
    public class CommandLineArgumentsValidator
    {
        public ValidationResult Validate(string[] args)
        {
            if (args.Length > 3)
            {
                return ValidationResult.Error(ValidationError.TooManyArguments);
            }

            if (args.Length < 3)
            {
                return ValidationResult.Error(ValidationError.TooFewArguments);
            }

            var isModeValid = TryParseProcessorMode(args[0], out var mode);
            if (!isModeValid)
            {
                return ValidationResult.Error(ValidationError.UnknownMode, args[0]);
            }

            var sourceValidationResult = CheckFileByPath(args[1], FileMode.Open, out var sourceFullPath);
            if (sourceValidationResult != ValidationError.Success)
            {
                return ValidationResult.Error(sourceValidationResult == ValidationError.PathNotFound
                    ? ValidationError.SourceNotExists
                    : sourceValidationResult, args[1]);
            }

            var destinationValidationResult = CheckFileByPath(args[2], FileMode.CreateNew, out var destinationFullPath);
            if (destinationValidationResult != ValidationError.Success)
            {
                return ValidationResult.Error(destinationValidationResult, args[2]);
            }
            
            return ValidationResult.Successful(new TaskParameters(mode, sourceFullPath, destinationFullPath));
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

        private ValidationError CheckFileByPath(string value, FileMode mode, out string fullPath)
        {
            fullPath = string.Empty;
            try
            {
                fullPath = Path.GetFullPath(value);
                using (File.Open(fullPath, mode))
                {
                    return ValidationError.Success;
                }
            }
            catch (Exception ex) when (ex is ArgumentException ||
                                       ex is NotSupportedException ||
                                       ex is PathTooLongException)
            {
                return ValidationError.PathIsIncorrect;
            }
            catch (Exception ex) when (ex is SecurityException ||
                                       ex is UnauthorizedAccessException)
            {
                return ValidationError.SecurityViolation;
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException ||
                                       ex is FileNotFoundException)
            {
                return ValidationError.PathNotFound;
            }
            catch (IOException)
            {
                // thrown when Open existing file with CreateNew
                return ValidationError.PathAlreadyExists;
            }
        }
    }
}