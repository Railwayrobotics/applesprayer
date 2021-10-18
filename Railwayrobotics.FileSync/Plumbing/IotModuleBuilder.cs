using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Devices.Client.Transport.Mqtt;
using Microsoft.Azure.Devices.Client;
using Railwayrobotics.Applesprayer.Brain.Plumbing.Client;
using BN.Edge.Robot.Device.Module.Anaylsis;

namespace Railwayrobotics.Applesprayer.Brain.Plumbing
{
    public static class IotModuleBuilder
    {
        public static IHost CreateHost(string[] args, Action<IServiceCollection> configureServices, IModuleClient predefinedClient = null)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    AddCommon(services, predefinedClient);
                    configureServices(services);
                })
                 .ConfigureLogging(builder =>
                 {
                     builder.AddConsole();
                 })
                 .UseConsoleLifetime()
                 .Build();
        }

        private static void AddCommon(IServiceCollection services, IModuleClient predefinedClient)
        {
            services.AddTransportSettings();

            if (predefinedClient != null)
                services.AddSingleton(predefinedClient);
            else
                services.AddSingleton<IModuleClient, IotModuleClient>();

            services.AddTransient<ILogger>(c => c.GetService<ILogger<FileService>>());
        }

        public static void AddTransportSettings(this IServiceCollection serviceCollection)
        {
            var mqttSetting = new MqttTransportSettings(TransportType.Mqtt_Tcp_Only);
            serviceCollection.AddSingleton(new ITransportSettings[] { mqttSetting });
        }
    }
}
