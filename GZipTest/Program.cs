using System;

namespace GZipTest
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var logger = new MultithreadingConsoleLogger();
            var validator = new CommandLineArgumentsValidator(logger);
            var validationResult = validator.Validate(args);
            
            if (validationResult.ValidationError != ValidationError.Success)
            {
                Console.WriteLine(ValidationErrorMessageProvider.GetErrorMessage(validationResult));
                return 1;
            }
            
            var taskProcessor = new TaskProcessor(logger);
            var task = taskProcessor.Start(validationResult.TaskParameters);
            task.Wait();
            
            return 0;
        }
    }
}
