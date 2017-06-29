using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Medallion.Shell;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Tasks
{
    internal class FileTask : BaseTask
    {
        private readonly LogWriter _logger = HostLogger.Get(typeof(FileTask));

        public FileTask(string taskName, int intervalInMinutes, int timeoutInMinutes)
            : base(taskName, intervalInMinutes, timeoutInMinutes)
        {
        }

        public string FileLocation { get; set; }

        protected override Task ExecuteInternal(CancellationToken token)
        {
            var file = FixUpFileLocation(FileLocation);

            var cmd = Command.Run(file, null, opts =>
            {
                opts
                    .StartInfo(i =>
                    {
                        i.RedirectStandardOutput = true;
                        i.WindowStyle = ProcessWindowStyle.Hidden;
                    });
            });

            //register the kill command to cancellation token
            // https://github.com/madelson/MedallionShell/issues/18
            token.Register(() => cmd.Kill());

            return cmd.Task;
        }

        public override void OutputResults(object taskResult)
        {
            var result = taskResult as CommandResult;

            if (result != null)
            {
                _logger.InfoFormat(Tokens.Formats.TaskLogFmt, TaskName, Tokens.TaskResults.Output, result.StandardOutput);

                if (!result.Success)
                {
                    _logger.ErrorFormat(Tokens.Formats.TaskLogFmt, TaskName, Tokens.TaskResults.Error, result.StandardError);
                    _logger.ErrorFormat(Tokens.Formats.TaskLogFmt, TaskName, Tokens.TaskResults.ExitCode, result.ExitCode);
                }   
            }

            base.OutputResults(result);
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