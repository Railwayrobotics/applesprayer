using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Railwayrobotics.Applesprayer.Brain.Models;
using Railwayrobotics.Applesprayer.Brain.Plumbing.Client;
using System.Text;
using System.Threading.Tasks;

namespace Railwayrobotics.Applesprayer.Brain.Services
{
    public class GpioService : IGpioService
    {
        private IModuleClient _moduleClient;
        private ILogger<GpioService> _logger;
        private bool _isOn;
        private string _topicName;
        private int _pin;

        public GpioService(IModuleClient moduleClient, ILogger<GpioService> logger)
        {
            _moduleClient = moduleClient;
            _logger = logger;
        }

        public async Task Setup(string topicName, int pinNumber)
        {
            _topicName = topicName;
            _pin = pinNumber;

            await TurnOff();
            _isOn = false;
        }

        public bool IsOn() => _isOn;

        public async Task TurnOff()
        {
            await SendMessageToGpio(GpioMessage.SetGpioHigh(_pin));
            _logger.LogInformation("Turned off");
            _isOn = false;
        }

        public async Task TurnOn()
        {
            await SendMessageToGpio(GpioMessage.SetGpioLow(_pin));
            _logger.LogInformation("Turned on");
            _isOn = true;
        }

        private Task SendMessageToGpio(GpioMessage message)
        {
            var dataBuffer = JsonConvert.SerializeObject(message);
            return _moduleClient.SendEventAsync(_topicName, new Message(Encoding.UTF8.GetBytes(dataBuffer)));
        }
    }
}
