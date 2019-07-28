using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest
{
    public class Task
    {
        public static Task StartInParallel(Action[] actions)
        {
            var task = new Task();
            var handlesList = new List<WaitHandle>(actions.Length);
            foreach (var action in actions)
            {
                var handle = new EventWaitHandle(false, EventResetMode.ManualReset);
                var thread = new Thread(() =>
                {
                    action();
                    handle.Set();
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
        }
        
        private WaitHandle[] _waitHandles;
    }
}