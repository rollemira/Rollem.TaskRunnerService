using System;
using System.Threading;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Tasks
{
    internal abstract class BaseTask
    {
        private readonly LogWriter _logger = HostLogger.Get(typeof(BaseTask));

        public BaseTask(string taskName, int intervalInMinutes, int timeoutInMinutes)
        {
            TaskName = taskName;
            IntervalInMinutes = intervalInMinutes;
            TimeoutInMinutes = timeoutInMinutes;
        }

        public string TaskName { get; private set; }
        public int IntervalInMinutes { get; private set; }
        public int TimeoutInMinutes { get; set; }
        public DateTime? NextRun { get; set; }
        protected abstract void ExecuteInternal(CancellationToken token);

        public void Execute(DateTime now, CancellationToken token)
        {
            NextRun = now.AddMinutes(IntervalInMinutes);
            _logger.DebugFormat("Starting task: {0}", TaskName);
            ExecuteInternal(token);
            _logger.DebugFormat("Task: {0} completed", TaskName);
            _logger.DebugFormat("Task: {0} next run time will be {1}", TaskName, NextRun);
        }
    }
}