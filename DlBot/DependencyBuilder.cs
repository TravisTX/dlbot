using DlBot.Models;
using DlBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DlBot
{
    // Separated so that it can be used for both the windows service and the aspnet app
    public class DependencyBuilder
    {
        public static IServiceCollection GetDependencies()
        {
            var serviceCollection = new ServiceCollection()
                .AddOptions()
                .AddLogging()
                .AddSingleton<SlackService, SlackService>()
                .AddTransient<SprintCalculatingService, SprintCalculatingService>()
                .AddTransient<TfsService, TfsService>()
                .AddTransient<Service, Service>()
                .AddTransient<BotUser, BotUser>()
                .Configure<SettingsModel>(GetConfiguration());
            return serviceCollection;
        }

        private static IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
            return builder.Build();
        }

    }
}
