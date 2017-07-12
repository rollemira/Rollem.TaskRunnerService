using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Rollem.TaskRunnerService.Models;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Services
{
    internal class ConfigService
    {
        private static readonly LogWriter Logger = HostLogger.Get(typeof(ConfigService));
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigFilePath;
        private static readonly string RunLogFilePath;

        static ConfigService()
        {
            ConfigFilePath = Path.Combine(BaseDirectory, "TaskConfig.json");
            RunLogFilePath = Path.Combine(BaseDirectory, "RunLog.json");
        }

        public static TaskConfig ReadConfig()
        {
            Logger.DebugFormat("Loading config from {0}", ConfigFilePath);
            var configJson = File.ReadAllText(ConfigFilePath);

            if (string.IsNullOrEmpty(configJson))
                throw new ApplicationException("TaskConfig.json was not found.");

            var result = JsonConvert.DeserializeObject<TaskConfig>(configJson);

            //set any defaults
            result.FileTasks.ForEach(t =>
            {
                t.IntervalInMinutes = t.IntervalInMinutes ?? result.DefaultIntervalInMinutes;
                t.TimeoutInMinutes = t.TimeoutInMinutes ?? result.DefaultIntervalInMinutes;
            });

            Logger.Debug("Config loaded.");
            return result;
        }

        public static void OutputLog(TaskLog item)
        {
            var logItems = new List<TaskLog>();

            //get current log items
            string logJson = null;
            if (File.Exists(RunLogFilePath))
                logJson = File.ReadAllText(RunLogFilePath);
            if (!string.IsNullOrEmpty(logJson))
                logItems = JsonConvert.DeserializeObject<List<TaskLog>>(logJson);

            //replace or add this one
            var existingItem = logItems.FirstOrDefault(i => i.TaskName == item.TaskName);
            if (existingItem != null)
            {
                var index = logItems.IndexOf(existingItem);
                logItems[index] = item;
            }
            else
            {
                logItems.Add(item);
            }

            //write to file
            var outJson = JsonConvert.SerializeObject(logItems, Formatting.Indented);
            File.WriteAllText(RunLogFilePath, outJson);
        }
    }
}