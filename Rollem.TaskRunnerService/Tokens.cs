namespace Rollem.TaskRunnerService
{
    public static class Tokens
    {
        public static class TaskResults
        {
            public const string Output = "Output";
            public const string Error = "Error";
            public const string ExitCode = "ExitCode";
        }
        public static class Formats
        {
            public const string TaskLogFmt = "[Task][{0}][{1}]: {2}";
        }
    }
}