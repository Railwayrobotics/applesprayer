using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Railwayrobotics.Applesprayer.Brain.Plumbing.Client;
using Railwayrobotics.Applesprayer.Brain.Services;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BN.Edge.Robot.Device.Module.Anaylsis
{
    public class AppleService : BackgroundService
    {
        private readonly AppleActorService _appleActorService;
        private readonly IGpioService _gpioService;
        private readonly ILogger _logger;
        private readonly IModuleClient _moduleClient;

        private const string InputQueueName = "imageDetected";
        private const string GpioTopicName = "gpio";
        private const int GpioPin = 18;

        public AppleService(IModuleClient moduleClient, AppleActorService appleActorService, IGpioService gpioService, ILogger<AppleService> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _appleActorService = appleActorService;
            _gpioService = gpioService;
            _logger = logger;
            _moduleClient = moduleClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SetInputMessageHandler();

            _logger.LogInformation(nameof(AppleService) + " looping");

            while (!stoppingToken.IsCancellationRequested)
            {
                await _appleActorService.ScheduledCheck();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            _logger.LogInformation(nameof(AppleService) + " loop stopped on cancellation");
        }

        private async Task SetInputMessageHandler()
        {
            _logger.LogInformation(nameof(AppleService) + " setting up message handler");
            await _moduleClient.SetInputMessageHandlerAsync(InputQueueName, QueueMessageHandler, _moduleClient);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating and opening client");

            await Exec(_moduleClient.CreateClient(), nameof(_moduleClient.CreateClient));
            await Exec(_moduleClient.OpenAsync(cancellationToken), nameof(_moduleClient.OpenAsync));
            
            _logger.LogInformation("Client initialized");

            await _gpioService.Setup(GpioTopicName, GpioPin);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _moduleClient?.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }

        private async Task Exec(Task setup, string serviceName)
        {
            await setup;
            _logger.LogInformation(serviceName + " successfully setup");
        }

        private async Task<MessageResponse> QueueMessageHandler(Message message, object userContext)
        {
            try
            {
                var rawMessage = TryGetRawContent(message);
                if (rawMessage == null)
                    _logger.LogWarning("Message received was null");
                else
                    await _appleActorService.Event(rawMessage);

            }
            catch(Exception e)
            {
                _logger.LogError(e, "Exception while handling event: " + e.Message);
            }

            return MessageResponse.Completed;
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
