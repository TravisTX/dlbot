using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Topshelf;
using Microsoft.Extensions.DependencyInjection;

namespace DlBot
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var serviceProvider = DependencyBuilder.GetDependencies().BuildServiceProvider();

            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.RollingFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "log-{Date}.txt"))
                .WriteTo.ColoredConsole()
                .CreateLogger();

            Serilog.Log.Information("Starting dlBot");

            HostFactory.Run(x =>
            {
                try
                {
                    //http://topshelf.readthedocs.org/en/latest/configuration/config_api.html#service-recovery
                    x.Service<Service>(s =>
                    {
                        s.ConstructUsing(() => serviceProvider.GetService<Service>());
                        s.WhenStarted((tc, hostControl) => tc.Start(hostControl));
                        s.WhenStopped((tc, hostControl) => tc.Stop(hostControl));
                    });
                    x.UseSerilog();

                    x.SetDescription("DlBot Slack Bot");
                    x.SetDisplayName("DlBot");
                    x.SetServiceName("DlBot");

                    x.EnableServiceRecovery(r =>
                    {
                        r.RestartService(0); // restart the service after 0 minutes
                        //should this be true for crashed or non-zero exits
                        r.OnCrashOnly();
                        //number of days until the error count resets
                        r.SetResetPeriod(1);
                    });

                    x.StartAutomatically();
                    x.EnablePauseAndContinue();
                    x.EnableShutdown();

                    x.RunAsNetworkService();
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "");
                    throw;
                }
            });
        }
    }
}
