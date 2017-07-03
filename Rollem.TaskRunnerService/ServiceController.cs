using Rollem.TaskRunnerService.Services;
using System;
using System.Timers;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService
{
    internal class ServiceController
    {
        private const int TimerFireInSeconds = 10;
        private readonly LogWriter _logger = HostLogger.Get(typeof(ServiceController));
        private readonly Timer _timer;
        private readonly TaskManagerService _taskManagerService;

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
            _timer.Stop();
            _taskManagerService.Dispose();
            _logger.InfoFormat("Service stopped.");
            return true;
        }
    }
}