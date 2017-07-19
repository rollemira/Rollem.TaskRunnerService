using System;
using log4net.Config;
using Microsoft.Win32;
using Topshelf;
using Topshelf.Logging;

namespace Rollem.TaskRunnerService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(c =>
            {
                //configure and use log4net
                XmlConfigurator.Configure();
                c.UseLog4Net();

                c.BeforeInstall(() =>
                {
                    //check out the .net framework version
                    var logger = HostLogger.Get(typeof(ServiceController));

                    if (!CheckNet45OrLater(logger))
                    {
                        var ex = new ApplicationException("Must have .NET Framework 4.6 installed!");
                        logger.Fatal(ex.Message);
                        throw ex;
                    }
                });

                //Service data
                c.Service<ServiceController>(s =>
                {
                    s.ConstructUsing(name => new ServiceController());
                    s.WhenStarted(sc => sc.Start());
                    s.WhenStopped(sc => sc.Stop());
                });
                c.EnableServiceRecovery(s => { s.RestartService(1); });
                c.RunAsPrompt();
                c.StartAutomaticallyDelayed();
                c.OnException(e => HostLogger.Get(typeof(ServiceController)).Fatal(e));

                //Display stuff
                c.SetDisplayName("RollemTaskRunnerService");
                c.SetServiceName("RollemTaskRunnerService");
                c.SetDescription("Service that runs tasks on intervals.");
            });
        }

        // Checking the version using >= will enable forward compatibility, 
        // however you should always compile your code on newer versions of
        // the framework to ensure your app works the same.
        private static string CheckFor45DotVersion(int releaseKey)
        {
            if (releaseKey >= 393295)
                return "4.6 or later";
            if (releaseKey >= 379893)
                return "4.5.2 or later";
            if (releaseKey >= 378675)
                return "4.5.1 or later";
            if (releaseKey >= 378389)
                return "4.5 or later";
            // This line should never execute. A non-null release key should mean
            // that 4.5 or later is installed.
            return "No 4.5 or later version detected";
        }

        private static bool CheckNet45OrLater(LogWriter logger)
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    var version = "Version: " + CheckFor45DotVersion((int) ndpKey.GetValue("Release"));
                    logger.Debug(version);
                    return version.IndexOf("4.6", StringComparison.Ordinal) >= 0;
                }

                return false; //not installed
            }
        }
    }
}