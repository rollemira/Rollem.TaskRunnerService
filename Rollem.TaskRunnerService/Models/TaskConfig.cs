using System.Collections.Generic;

namespace Rollem.TaskRunnerService.Models
{
    internal class TaskConfig
    {
        public int DefaultIntervalInMinutes { get; set; }
        public int DefaultTimeoutInMinutes { get; set; }
        public List<FileTaskConfig> FileTasks { get; set; }
    }
}