namespace Rollem.TaskRunnerService.Models
{
    internal class FileTaskConfig
    {
        public int IntervalInMinutes { get; set; }
        public int TimeoutInMinutes { get; set; }
        public string TaskName { get; set; }
        public string FileLocation { get; set; }
    }
}