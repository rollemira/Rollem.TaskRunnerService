using System;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Tasks
{
    internal abstract class BaseTask
    {
        private readonly LogWriter _logger = HostLogger.Get(typeof(BaseTask));

        public string TaskName { get; private set; }
        public int IntervalInMinutes { get; private set; }
        public int TimeoutInMinutes { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? Finish { get; set; }
        public string Status { get; set; }
        public DateTime? NextRun { get; set; }

        protected abstract Task ExecuteInternal(CancellationToken token);

        public virtual void OutputResults(DateTime now, Task task)
        {
            LastRun = now;
            Finish = DateTime.Now;
            _logger.InfoFormat("Task: {0} completed", TaskName);
            _logger.InfoFormat("Task: {0} next run time will be {1}", TaskName, NextRun);
        }

        public Task Execute(DateTime now, CancellationToken token)
        {
            NextRun = now.AddMinutes(IntervalInMinutes);
            _logger.InfoFormat("Starting task: {0}", TaskName);
            return ExecuteInternal(token);
        }
    }
}