using System;
using System.Runtime.InteropServices;
using System.Timers;
using Microsoft.Win32.SafeHandles;
using Rollem.TaskRunnerService.Services;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService
{
    internal class ServiceController : IDisposable
    {
        private const int TimerFireInSeconds = 10;
        private readonly SafeHandle _handle = new SafeFileHandle(IntPtr.Zero, true);
        private readonly LogWriter _logger = HostLogger.Get(typeof(ServiceController));
        private readonly TaskManagerService _taskManagerService;
        private readonly Timer _timer;
        private bool _disposed;

        public ServiceController()
        {
            _timer = new Timer(1000 * TimerFireInSeconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += ElapsedFired;

            _taskManagerService = new TaskManagerService();

            _logger.Debug("ServiceController constructed.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ElapsedFired(object sender, ElapsedEventArgs e)
        {
            _taskManagerService.ExecutePastDueJobs(DateTime.Now);
            _logger.Debug("Interval fired.");
        }

        public bool Start()
        {
            _timer.Start();
            _logger.InfoFormat("Service started.");
            return true;
        }

        public bool Stop()
        {
            Dispose(true);
            _timer.Stop();
            _logger.InfoFormat("Service stopped.");
            return true;
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _handle.Dispose();
                //cancel any running tasks and dispose
                _taskManagerService.Dispose();
            }

            _disposed = true;
        }
    }
}