using System;

namespace Rollem.TaskRunnerService.Models
{
    internal class TaskLog
    {
        public string TaskName { get; set; }
        public DateTime LastRun { get; set; }
        public DateTime NextRun { get; set; }
    }
}