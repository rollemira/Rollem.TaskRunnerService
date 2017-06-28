using System;
using System.IO;
using Newtonsoft.Json;
using Rollem.TaskRunnerService.Models;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService.Services
{
    internal class ConfigService
    {
        private static readonly LogWriter Logger = HostLogger.Get(typeof(ConfigService));
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TaskConfig.json");

        public static TaskConfig ReadConfig()
        {
            Logger.DebugFormat("Loading config from {0}", ConfigFilePath);
            var configJson = File.ReadAllText(ConfigFilePath);
            
            if (string.IsNullOrEmpty(configJson))
                throw new ApplicationException("TaskConfig.json was not found.");

            var result = JsonConvert.DeserializeObject<TaskConfig>(configJson);
            Logger.Debug("Config loaded.");
            return result;
        }
    }
}
