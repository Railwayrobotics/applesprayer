using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Railwayrobotics.Applesprayer.Brain.Plumbing.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BN.Edge.Robot.Device.Module.Anaylsis
{
    public class AppleService : BackgroundService
    {
        private readonly ILogger _logger;

        public AppleService(IModuleClient moduleClient, ILogger<AppleService> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(nameof(AppleService) + " is working");
                await Task.Delay(TimeSpan.FromSeconds(30));
            }
        }
    }
}
