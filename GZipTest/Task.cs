using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    public class Task
    {
        private Task(CancellationTokenSource cancellationTokenSource, ILogger logger)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _logger = logger;
            _timer.Start();
        }
        
        public bool IsErrorOccured { get; private set; }
        
        public static Task StartInParallel(
            Action[] actions, 
            CancellationTokenSource cancellationTokenSource,
            ILogger logger)
        {
            var task = new Task(cancellationTokenSource, logger);
            var handlesList = new List<WaitHandle>(actions.Length);
            foreach (var action in actions)
            {
                var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                var thread = new Thread(() =>
                {
                    try
                    {
                        action();
                    }
                    catch (Exception)
                    {
                        task.Abort();
                    }
                    finally
                    {
                        handle.Set();
                    }
                });
                
                handlesList.Add(handle);
                thread.Start();
            }

            task._waitHandles = handlesList.ToArray();
            return task;
        }

        public void Wait()
        {
            WaitHandle.WaitAll(_waitHandles);
            _timer.Stop();
            _logger.Write("");
            _logger.Write($"Task finished in {_timer.Elapsed}");
        }

        public void Abort()
        {
            _logger.Write("");
            _logger.Write("Aborting task...");
            _logger.Write($"Execution lasted for {_timer.Elapsed}");
            _cancellationTokenSource.Cancel();
            IsErrorOccured = true;
        }
        
        private WaitHandle[] _waitHandles;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ILogger _logger;
        private readonly Stopwatch _timer = new Stopwatch();
    }
}