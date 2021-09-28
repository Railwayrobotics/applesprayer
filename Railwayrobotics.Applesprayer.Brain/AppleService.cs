using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Railwayrobotics.Applesprayer.Brain.Plumbing.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BN.Edge.Robot.Device.Module.Anaylsis
{
    public class AppleService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IModuleClient _moduleClient;
        private const string InputQueueName = "imageDetected";

        public AppleService(IModuleClient moduleClient, ILogger<AppleService> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _moduleClient = moduleClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(nameof(AppleService) + " setting up message handler");
            await _moduleClient.SetInputMessageHandlerAsync(InputQueueName, QueueMessageHandler, _moduleClient);

            _logger.LogInformation(nameof(AppleService) + " looping");
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(nameof(AppleService) + " is alive");
                await Task.Delay(TimeSpan.FromSeconds(100));
            }

            _logger.LogInformation(nameof(AppleService) + " loop stopped on cancellation");
        }

        private Task<MessageResponse> QueueMessageHandler(Message message, object userContext)
        {
            var rawMessage = TryGetRawContent(message);
            if (rawMessage == null)
                _logger.LogWarning("Message received was null");
            else
                _logger.LogInformation("Message received: " + rawMessage);

            return Task.FromResult(MessageResponse.Completed);
        }

        private string TryGetRawContent(Message message) => Try(() => Encoding.UTF8.GetString(message.GetBytes()), nameof(TryGetRawContent));

        private T Try<T>(Func<T> action, string info)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message: " + ex.Message + ": " + info);
                return default;
            }
        }
    }
}
