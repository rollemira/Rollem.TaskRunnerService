using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Rollem.TaskRunnerService.Config;
using Rollem.TaskRunnerService.Tasks;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Services
{
    internal class TaskManagerService : IDisposable
    {
        private readonly LogWriter _logger = HostLogger.Get(typeof(TaskManagerService));
        private static readonly List<BaseTask> TasksCache = new List<BaseTask>();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private readonly CancellationToken _token;
        private bool _disposed = false;
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);

        public TaskManagerService()
        {
            _token = _tokenSource.Token;
            LoadTasksFromConfig();
        }

        public void ExecutePastDueJobs(DateTime now)
        {
            var tasksToRun = TasksCache.Where(t => !t.NextRun.HasValue || t.NextRun.Value <= now).ToList();
            tasksToRun.ForEach(t =>
            {
                //give task cancellation token
                t.Execute(now, _token);
            });
        }

        private void LoadTasksFromConfig()
        {
            if (TasksCache.Count == 0)
            {
               var config = ConfigService.ReadConfig();
               
                _logger.Debug("Reading config.");
                
                _logger.Debug("Loading file tasks.");
                LoadFileTasks(config);
                _logger.DebugFormat("{0} tasks loaded.", TasksCache.Count);
            }
        }

        private void LoadFileTasks(TaskConfig config)
        {
            config.FileTasks.ForEach(t =>
            {
                var task = (FileTask) Activator.CreateInstance(typeof (FileTask),
                    t.TaskName,
                    (t.IntervalInMinutes != 0) ? t.IntervalInMinutes : config.DefaultIntervalInMinutes,
                    (t.TimeoutInMinutes !=0) ? t.TimeoutInMinutes : config.DefaultTimeoutInMinutes);

                task.FileLocation = t.FileLocation;

                TasksCache.Add(task);
                _logger.DebugFormat("FileTask: {0} has been loaded and will run every {1} minute(s).", task.TaskName,
                    task.IntervalInMinutes);
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _handle.Dispose();
                //cancel any running tasks and dispose
                _tokenSource.Cancel(false);
            }

            _disposed = true;
        }
    }
}