using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Rollem.TaskRunnerService.Config;
using Rollem.TaskRunnerService.Tasks;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService
{
    internal class TaskManager : IDisposable
    {
        private readonly LogWriter _logger;
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskConfig.json");
        private static readonly List<BaseTask> TasksCache = new List<BaseTask>();
        private static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();
        private static CancellationToken _token;
        private bool _disposed = false;
        private SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);

        public TaskManager(LogWriter logger)
        {
            _logger = logger;
            _token = TokenSource.Token;
            LoadTasksFromConfig();
        }

        public void ExecutePastDueJobs(DateTime now)
        {
            var tasksToRun = TasksCache.Where(t => !t.NextRun.HasValue || t.NextRun.Value <= now).ToList();
            tasksToRun.ForEach(t =>
            {
                //spin the tasks in a new thread
                var process = Task.Factory.StartNew(() =>
                {
                    //check for cancellation
                    if (_token.IsCancellationRequested)
                    {
                        _token.ThrowIfCancellationRequested();
                    }

                    t.Execute(_logger, now);

                    //check for cancellation
                    if (_token.IsCancellationRequested)
                    {
                        _token.ThrowIfCancellationRequested();
                    }
                });
                //wait for thread complete
                try
                {
                    process.Wait();
                }
                catch (AggregateException e)
                {
                    if (e.InnerExceptions.Any(x => x is TaskCanceledException))
                    {
                        _logger.DebugFormat("Task: {0} was cancelled for shutdown.", t.TaskName);
                        TokenSource.Dispose();
                    }
                    else
                    {
                        throw;
                    }
                }
            });
        }

        private void LoadTasksFromConfig()
        {
            if (TasksCache.Count == 0)
            {
                _logger.DebugFormat("Loading config from {0}", ConfigFilePath);
                string configJson = File.ReadAllText(ConfigFilePath);
                if (string.IsNullOrEmpty(configJson))
                    throw new ApplicationException("TaskConfig.json was not found.");

                _logger.Debug("Config loaded.");
                var config = JsonConvert.DeserializeObject<TaskConfig>(configJson);
                _logger.Debug("Reading config.");
                
                _logger.Debug("Loading file tasks.");
                config.FileTasks.ForEach(t =>
                {
                    var task = (FileTask)Activator.CreateInstance(typeof(FileTask), 
                        t.TaskName, 
                        (t.IntervalInMinutes != 0) ? t.IntervalInMinutes : config.DefaultIntervalInMinutes);

                    task.FileLocation = t.FileLocation;

                    TasksCache.Add(task);
                    _logger.DebugFormat("FileTask: {0} has been loaded and will run every {1} minute(s).", task.TaskName, task.IntervalInMinutes);
                });
                _logger.DebugFormat("{0} tasks loaded.", TasksCache.Count);
            }
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
                TokenSource.Cancel();
            }

            _disposed = true;
        }
    }
}