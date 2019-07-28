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
            try
            {
                var task = taskProcessor.Start(validationResult.TaskParameters);
                Console.CancelKeyPress += (_, cancelEventArgs) =>
                {
                    cancelEventArgs.Cancel = true;
                    task.Abort();
                };
                
                task.Wait();
                logger.Write("Execution finished " + (task.IsErrorOccured ? "with errors or was aborted" : "successfully"));
                return task.IsErrorOccured ? 1 : 0;
            }
            catch (Exception e)
            {
                logger.Write(e.Message);
                return 1;
            }
        }
    }
}
