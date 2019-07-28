using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace GZipTest
{
    public class Task
    {
        private Task(ILogger logger)
        {
            _logger = logger;
            _timer.Start();
        }
        
        public bool IsErrorOccured { get; private set; }
        
        public static Task StartInParallel(Action[] actions, ILogger logger)
        {
            var task = new Task(logger);
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
                    catch (Exception e)
                    {
                        logger.WriteError(e.Message);
                        task.IsErrorOccured = true;
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
        
        private WaitHandle[] _waitHandles;
        private readonly ILogger _logger;
        private readonly Stopwatch _timer = new Stopwatch();
    }
}