using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Railwayrobotics.Applesprayer.Brain.Plumbing.Client;
using Railwayrobotics.FileSync.Models;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BN.Edge.Robot.Device.Module.Anaylsis
{
    public class FileService : BackgroundService
    {
        private readonly ModuleConfiguration _config;
        private readonly ILogger _logger;
        private readonly IModuleClient _moduleClient;

        public FileService(ModuleConfiguration config, IModuleClient moduleClient, ILogger<FileService> logger, IHostApplicationLifetime hostApplicationLifetime)
        {
            _config = config;
            _logger = logger;
            _moduleClient = moduleClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(nameof(FileService) + " looping");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30));

                if (_config == null || _config.Sync == _config.DesiredSync)
                    continue;


            }

            _logger.LogInformation(nameof(FileService) + " loop stopped on cancellation");
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating and opening client");

            await Exec(_moduleClient.CreateClient(), nameof(_moduleClient.CreateClient));
            await Exec(_moduleClient.OpenAsync(cancellationToken), nameof(_moduleClient.OpenAsync));
            
            _logger.LogInformation("Client initialized");

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _moduleClient?.CloseAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }

        private async Task PropertyUpdateCallbackAsync(TwinCollection desiredTwinProperties, object userContext)
        {
            var updateId = GetUpdatedId();
            _logger.Log(LogLevel.Information, "triggered", updateId);

            try
            {
                ModuleClient moduleClient = userContext as ModuleClient;
                if (moduleClient == null)
                    throw new InvalidOperationException("UserContext doesn't contain expected values");

                var reportedTwinCollection = UpdateReportedConfig(desiredTwinProperties, updateId);

                await moduleClient.UpdateReportedPropertiesAsync(reportedTwinCollection);
                Log(LogLevel.Information, "updated reported properties (length=" + _config.GetJson().Length + ")", updateId);
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, "failed " + ex.Message + ". Stack: " + ex.StackTrace, updateId, ex);
                throw ex;
            }
        }

        public TwinCollection UpdateReportedConfig(TwinCollection desiredTwinProperties, string updateId)
        {
            if (desiredTwinProperties == null)
                throw new ArgumentNullException(nameof(desiredTwinProperties));

            var json = desiredTwinProperties.ToJson();
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(nameof(desiredTwinProperties) + "." + nameof(json));

            var desiredConfiguration = ModuleConfiguration.Deserialize(json);

            Log(LogLevel.Information, "Updating config (length=" + json.Length + ")", updateId);

            _config.Uri = desiredConfiguration.Uri;
            _config.Volume = desiredConfiguration.Volume;
            _config.DesiredSync = desiredConfiguration.Sync;
            
            Log(LogLevel.Information, "updated config", updateId);

            return new TwinCollection(_config.GetJson());
        }

        private void Log(LogLevel logLevel, string message, string updateId, Exception e = null)
        {
            message = $"[{updateId}] {nameof(PropertyUpdateCallbackAsync)}: " + message;

            if (e != null)
                _logger.Log(logLevel, e, message);
            else
                _logger.Log(logLevel, message);
        }

        public static string GetUpdatedId() => Guid.NewGuid().ToString().Split("-")[0];

        private async Task Exec(Task setup, string serviceName)
        {
            await setup;
            _logger.LogInformation(serviceName + " successfully setup");
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
