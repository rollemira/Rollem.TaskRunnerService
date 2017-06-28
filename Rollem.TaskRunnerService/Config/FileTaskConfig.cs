namespace Rollem.TaskRunnerService.Config
{
    internal class FileTaskConfig
    {
        public int IntervalInMinutes { get; set; }
        public string TaskName { get; set; }
        public string FileLocation { get; set; }
    }
}