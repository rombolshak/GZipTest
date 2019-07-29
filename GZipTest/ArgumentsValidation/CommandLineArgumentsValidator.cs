using System;
using System.IO;
using System.Security;

namespace GZipTest.ArgumentsValidation
{
    public class CommandLineArgumentsValidator
    {
        public CommandLineArgumentsValidator(ILogger logger)
        {
            _logger = logger;
        }

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
                if (sourceValidationResult == ValidationError.PathNotFound)
                {
                    sourceValidationResult = ValidationError.SourceNotExists;
                }

                if (sourceValidationResult == ValidationError.PathAlreadyExists)
                {
                    sourceValidationResult = ValidationError.Other;
                }
                
                return ValidationResult.Error(sourceValidationResult, args[1]);
            }

            var destinationValidationResult = CheckFileByPath(args[2], FileMode.CreateNew, out var destinationFullPath);
            if (destinationValidationResult != ValidationError.Success)
            {
                return ValidationResult.Error(destinationValidationResult, args[2]);
            }
            
            File.Delete(destinationFullPath);
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
                _logger.Write(ex.Message);
                return ValidationError.PathIsIncorrect;
            }
            catch (Exception ex) when (ex is SecurityException ||
                                       ex is UnauthorizedAccessException)
            {
                _logger.Write(ex.Message);
                return ValidationError.SecurityViolation;
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException ||
                                       ex is FileNotFoundException)
            {
                _logger.Write(ex.Message);
                return ValidationError.PathNotFound;
            }
            catch (IOException ex)
            {
                _logger.Write(ex.Message);
                // thrown when Open existing file with CreateNew
                return ValidationError.PathAlreadyExists;
            }
            catch (Exception ex)
            {
                _logger.Write(ex.Message);
                return ValidationError.Other;
            }
        }
        
        private readonly ILogger _logger;
    }
}