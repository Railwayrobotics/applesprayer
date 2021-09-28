using BN.Edge.Robot.Device.Module.Anaylsis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Railwayrobotics.Applesprayer.Brain.Plumbing;
using Railwayrobotics.Applesprayer.Brain.Plumbing.Client;
using Railwayrobotics.Applesprayer.Brain.Services;
using System.Threading.Tasks;

namespace Railwayrobotics.Applesprayer.Brain
{
    public class Program
    {
        public static Task Main(string[] args) =>
            CreateHost(args)
            .RunAsync();

        public static IHost CreateHost(string[] args, IModuleClient moduleClient = null)
            => IotModuleBuilder.CreateHost(args, ConfigureSpecificServices, moduleClient);

        private static void ConfigureSpecificServices(IServiceCollection services)
        {
            services.AddSingleton<IGpioService, GpioService>();
            services.AddSingleton<AppleActorService>();
            services.AddHostedService<AppleService>();
        }
    }
}
