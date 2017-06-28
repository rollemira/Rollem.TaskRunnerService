using System;
using System.Diagnostics;
using System.IO;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Tasks
{
    internal class FileTask : BaseTask
    {
        public FileTask(string taskName, int intervalInMinutes)
            : base(taskName, intervalInMinutes)
        {
        }

        public string FileLocation { get; set; }

        protected override void ExecuteInternal(LogWriter log)
        {
            var file = FixUpFileLocation(FileLocation);
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = file
            };
            Process process = new Process()
            {
                StartInfo = startInfo
            };
            process.Start();

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