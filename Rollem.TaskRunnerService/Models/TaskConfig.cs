using System.Collections.Generic;

namespace Rollem.TaskRunnerService.Models
{
    internal class TaskConfig
    {
        public int DefaultIntervalInMinutes { get; set; }
        public int DefaultTimeoutInMinutes { get; set; }
        public List<FileTaskConfig> FileTasks { get; set; }
    }

    internal class FileTaskConfig
    {
        public int? IntervalInMinutes { get; set; }
        public int? TimeoutInMinutes { get; set; }
        public string TaskName { get; set; }
        public string FileLocation { get; set; }
    }
}