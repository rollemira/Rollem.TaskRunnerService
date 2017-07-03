using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Medallion.Shell;
using Microsoft.Win32.SafeHandles;
using Rollem.TaskRunnerService.Bootstrap;
using Rollem.TaskRunnerService.Models;
using Rollem.TaskRunnerService.Tasks;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Services
{
    internal class TaskManagerService : IDisposable
    {
        private readonly LogWriter _logger = HostLogger.Get(typeof(TaskManagerService));
        private static readonly List<BaseTask> TasksCache = new List<BaseTask>();
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private bool _disposed = false;
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);

        public TaskManagerService()
        {
            //mappings
            Mapper.Initialize(cfg => cfg.AddProfile<AppMapProfile>());
            LoadTasksFromConfig();
        }

        public void ExecutePastDueJobs(DateTime now)
        {
            var tasksToRun = TasksCache.Where(t => !t.NextRun.HasValue || t.NextRun.Value <= now).ToList();
            tasksToRun.ForEach(t =>
            {
                //give task cancellation token
                var token = _tokenSource.Token;
                using (var task = t.Execute(now, token))
                {
                    task.Wait(1000 * t.TimeoutInMinutes, token);
                    t.OutputResults(now, task);
                    var item = Mapper.Map<TaskLog>(t);
                    ConfigService.OutputLog(item);
                }
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
                var task = Mapper.Map<FileTask>(t);
                TasksCache.Add(task);
                _logger.InfoFormat("FileTask: {0} has been loaded and will run every {1} minute(s).", task.TaskName,
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
                _tokenSource.Dispose();
            }

            _disposed = true;
        }
    }
}