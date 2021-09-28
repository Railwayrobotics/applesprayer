using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Railwayrobotics.Applesprayer.Brain.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Railwayrobotics.Applesprayer.Brain.Services
{
    public class AppleActorService
    {
        private readonly IGpioService _gpioService;
        private readonly ILogger<AppleActorService> _logger;
        private const string AppleKeyword = "apple";
        private const int MillisecondsToKeepOn = 1000;
        private DateTime? _lastDetected;

        public AppleActorService(IGpioService gpioService, ILogger<AppleActorService> logger)
        {
            _gpioService = gpioService;
            _logger = logger;
        }

        public async Task Event(string rawMessage)
        {
            var deepstreamDetection = Deserialize(rawMessage);
            if (deepstreamDetection == null)
                return;

            var objects = deepstreamDetection.GetObjects(lookingFor: AppleKeyword);
            if (!objects.Any())
                return;

            SetLastDetected(deepstreamDetection);

            if (!_gpioService.IsOn())
                await _gpioService.TurnOn();
        }

        public async Task ScheduledCheck()
        {
            if (!_gpioService.IsOn())
                return;

            if(_lastDetected == null)
            {
                _logger.LogInformation("Detection never happen, but gpio is on. Turning off");
                await _gpioService.TurnOff();
                return;
            }

            if (DateTime.UtcNow > _lastDetected.Value.Add(TimeSpan.FromMilliseconds(MillisecondsToKeepOn)))
                await _gpioService.TurnOff();
        }

        private void SetLastDetected(DeepstreamDetection deepstreamDetection)
        {
            if (_lastDetected.HasValue && _lastDetected > deepstreamDetection.Timestamp)
                return;

            SetLastDetected(deepstreamDetection.Timestamp);
        }

        public void SetLastDetected(DateTime time)
        {
            _lastDetected = time;
        }

        private DeepstreamDetection Deserialize(string rawMessage)
        {
            try
            {
                return JsonConvert.DeserializeObject<DeepstreamDetection>(rawMessage);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to deserialize " + rawMessage);
                return null;
            }
        }
    }
}
