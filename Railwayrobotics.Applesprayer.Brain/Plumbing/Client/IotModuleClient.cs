using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Railwayrobotics.Applesprayer.Brain.Plumbing.Client
{
    public class IotModuleClient : IModuleClient
    {
        private readonly ITransportSettings[] _settings;
        private ModuleClient _moduleClient;
        private ModuleClient GetModuleClient => _moduleClient ?? throw new Exception("Client not initiated. Bug");

        public IotModuleClient(ITransportSettings[] settings)
        {
            _settings = settings;
        }

        public async Task CreateClient()
        {
            if (_settings == null)
                throw new ArgumentNullException(nameof(_settings));

            _moduleClient = await ModuleClient.CreateFromEnvironmentAsync(_settings);
        }

        public Task SetPropertyUpdateHandlerAsync(DesiredPropertyUpdateCallback callback) => GetModuleClient.SetDesiredPropertyUpdateCallbackAsync(callback, GetModuleClient);
        public Task SendEventAsync(string outputName, Message message) => GetModuleClient.SendEventAsync(outputName, message);
        public Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext) => GetModuleClient.SetInputMessageHandlerAsync(inputName, messageHandler, userContext);
        public Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler, object userContext) => GetModuleClient.SetMethodHandlerAsync(methodName, methodHandler, userContext);
        public Task OpenAsync(CancellationToken cancellationToken) => GetModuleClient.OpenAsync(cancellationToken);
        public Task CloseAsync(CancellationToken cancellationToken) => GetModuleClient.CloseAsync(cancellationToken);
        public Task<Twin> GetTwinAsync(CancellationToken cancellationToken) => GetModuleClient.GetTwinAsync(cancellationToken);
        public Task SendEventBatchAsync(string outputName, IEnumerable<Message> messages) => GetModuleClient.SendEventBatchAsync(outputName, messages);
    }
}