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
        private readonly TaskManager _taskManager;

        public ServiceController()
        {
            _timer = new Timer(1000 * TimerFireInSeconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += ElapsedFired;

            _taskManager = new TaskManager(_logger);

            _logger.Debug("ServiceController constructed.");
        }

        private void ElapsedFired(object sender, ElapsedEventArgs e)
        {
            _taskManager.ExecutePastDueJobs(DateTime.Now);
            _logger.Debug("Interval fired.");
        }

        public bool Start()
        {
            _timer.Start();
            _logger.Debug("Service started.");
            return true;
        }

        public bool Stop()
        {
            _timer.Stop();
            _taskManager.Dispose();
            _logger.Debug("Service stopped.");
            return true;
        }
    }
}