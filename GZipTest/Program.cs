﻿using System;

namespace GZipTest
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var validator = new CommandLineArgumentsValidator();
            var validationResult = validator.Validate(args);
            
            if (validationResult.ValidationError != ValidationError.Success)
            {
                Console.WriteLine(ValidationErrorMessageProvider.GetErrorMessage(validationResult));
                return 1;
            }
            
            return 0;
        }
    }
}
