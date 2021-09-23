using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Railwayrobotics.Applesprayer.Brain.Plumbing.Client
{
    public interface IModuleClient
    {
        Task CreateClient();
        Task OpenAsync(CancellationToken cancellationToken);
        Task CloseAsync(CancellationToken cancellationToken);
        Task SendEventAsync(string outputName, Message message);
        Task SetInputMessageHandlerAsync(string inputName, MessageHandler messageHandler, object userContext);
        Task SetMethodHandlerAsync(string methodName, MethodCallback methodHandler, object userContext);
        Task<Twin> GetTwinAsync(CancellationToken cancellationToken);
        Task SetPropertyUpdateHandlerAsync(DesiredPropertyUpdateCallback callback);
        Task SendEventBatchAsync(string outputName, IEnumerable<Message> messages);
    }
}
