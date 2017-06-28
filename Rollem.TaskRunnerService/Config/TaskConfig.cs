using System.Collections.Generic;

namespace Rollem.TaskRunnerService.Config
{
    internal class TaskConfig
    {
        public int DefaultIntervalInMinutes { get; set; }
        public List<FileTaskConfig> FileTasks { get; set; }
    }
}