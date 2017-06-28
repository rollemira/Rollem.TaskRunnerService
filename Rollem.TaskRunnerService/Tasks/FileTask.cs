using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Medallion.Shell;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Tasks
{
    internal class FileTask : BaseTask
    {
        private readonly LogWriter _logger = HostLogger.Get(typeof(FileTask));
        private const string TaskLogFmt = "[Task][{0}][{1}]: {2}";

        public FileTask(string taskName, int intervalInMinutes, int timeoutInMinutes)
            : base(taskName, intervalInMinutes, timeoutInMinutes)
        {
        }

        public string FileLocation { get; set; }

        protected override void ExecuteInternal(CancellationToken token)
        {
            var file = FixUpFileLocation(FileLocation);

            var cmd = Command.Run(file, null, opts =>
            {
                opts
                    .Timeout(TimeSpan.FromMinutes(TimeoutInMinutes))
                    .StartInfo(i =>
                    {
                        i.RedirectStandardOutput = true;
                        i.WindowStyle = ProcessWindowStyle.Hidden;
                    });
            });

            cmd.Wait();

            if (!cmd.Result.Success)
            {
                _logger.ErrorFormat(TaskLogFmt, TaskName,  "Error", cmd.Result.StandardError);
                _logger.ErrorFormat(TaskLogFmt, TaskName, "ExitCode", cmd.Result.ExitCode);
            }

            _logger.DebugFormat(TaskLogFmt, TaskName, "Output", cmd.Result.StandardOutput);
        }

        private string FixUpFileLocation(string fileLocation)
        {
            //if they use a : we assume full path, otherwise we need to add
            //this location to it.
            return (!fileLocation.Contains(":")
                ? Path.Combine(Environment.CurrentDirectory, FileLocation)
                : fileLocation);
        }
    }
}