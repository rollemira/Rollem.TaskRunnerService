using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using log4net.Config;
using Newtonsoft.Json;
using Topshelf;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(c =>
            {
                //configure and use log4net
                XmlConfigurator.Configure();
                c.UseLog4Net();

                //Service data
                c.Service<ServiceController>(s =>
                {
                    s.ConstructUsing(name => new ServiceController());
                    s.WhenStarted(sc => sc.Start());
                    s.WhenStopped(sc => sc.Stop());
                });
                c.RunAsPrompt();
                c.OnException(e => HostLogger.Get(typeof(ServiceController)).Fatal(e));

                //Display stuff
                c.SetDisplayName("Rollem.TaskRunnerService");
                c.SetServiceName("Rollem.TaskRunnerService");
                c.SetDescription("Service that runs tasks on intervals.");
            });
        }
    }

    internal class ServiceController
    {
        private readonly LogWriter _logger = HostLogger.Get(typeof(ServiceController));
        private readonly Timer _timer;
        private readonly TaskManager _taskManager;

        public ServiceController()
        {
            _timer = new Timer(1000 * 20) //every 20 seconds
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
            _logger.Debug("Service stopped.");
            return true;
        }
    }

    internal class TaskManager
    {
        private readonly LogWriter _logger;
        private static readonly string _configFilePath = Path.Combine(Environment.CurrentDirectory, "TaskConfig.json");
        private static readonly List<BaseTask> _tasks = new List<BaseTask>();

        public TaskManager(LogWriter logger)
        {
            _logger = logger;
            LoadTasksFromConfig();
        }

        public void ExecutePastDueJobs(DateTime now)
        {
            var tasksToRun = _tasks.Where(t => !t.NextRun.HasValue || t.NextRun.Value <= now).ToList();
            tasksToRun.ForEach(t => t.Execute(_logger, now));
        }

        private void LoadTasksFromConfig()
        {
            if (_tasks.Count == 0)
            {
                _logger.DebugFormat("Loading config from {0}", _configFilePath);
                string configJson = File.ReadAllText(_configFilePath);
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

                    _tasks.Add(task);
                    _logger.DebugFormat("FileTask: {0} has been loaded.", t.TaskName);
                });
            }
        }
    }

    internal abstract class BaseTask
    {
        public BaseTask(string taskName, int intervalInMinutes)
        {
            TaskName = taskName;
            IntervalInMinutes = intervalInMinutes;
        }

        public string TaskName { get; private set; }
        public int IntervalInMinutes { get; private set; }
        public DateTime? NextRun { get; set; }
        protected abstract void ExecuteInternal(LogWriter log);
        public void Execute(LogWriter log, DateTime now)
        {
            //TODO: Move this to TaskManager
            //execute on a new thread
            Task.Factory.StartNew(() =>
            {
                log.DebugFormat("Starting task: {0}", TaskName);
                ExecuteInternal(log); 
                log.DebugFormat("{0} completed", TaskName);
                NextRun = now.AddMinutes(IntervalInMinutes);
            });
        }
    }

    internal class FileTask : BaseTask
    {
        public FileTask(string taskName, int intervalInMinutes)
            : base(taskName, intervalInMinutes)
        {
        }

        public string FileLocation { get; set; }

        protected override void ExecuteInternal(LogWriter log)
        {
            
        }
    }

    internal class TaskConfig
    {
        public int DefaultIntervalInMinutes { get; set; }
        public List<FileTaskConfig> FileTasks { get; set; }
    }

    internal class FileTaskConfig
    {
        public int IntervalInMinutes { get; set; }
        public string TaskName { get; set; }
        public string FileLocation { get; set; }
    }
}
