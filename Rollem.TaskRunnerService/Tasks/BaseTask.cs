using System;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Tasks
{
    internal abstract class BaseTask
    {
        public BaseTask(string taskName, int intervalInMinutes)
        {
            TaskName = taskName;
            IntervalInMinutes = intervalInMinutes;
        }

        public string TaskName { get; private set; }
        public int IntervalInMinutes { get; private set; }
        public DateTime? NextRun { get; set; }
        protected abstract void ExecuteInternal(LogWriter log);
        public void Execute(LogWriter log, DateTime now)
        {
            NextRun = now.AddMinutes(IntervalInMinutes);
            log.DebugFormat("Starting task: {0}", TaskName);
            ExecuteInternal(log); 
            log.DebugFormat("{0} completed", TaskName);
        }
    }
}