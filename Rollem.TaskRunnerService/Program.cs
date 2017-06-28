using log4net.Config;
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
                c.EnableServiceRecovery(s =>
                {
                    s.RestartService(1);
                });
                c.RunAsLocalSystem();
                c.StartAutomaticallyDelayed();
                c.OnException(e => HostLogger.Get(typeof(ServiceController)).Fatal(e));

                //Display stuff
                c.SetDisplayName("RollemTaskRunnerService");
                c.SetServiceName("RollemTaskRunnerService");
                c.SetDescription("Service that runs tasks on intervals.");
            });
        }
    }
}
