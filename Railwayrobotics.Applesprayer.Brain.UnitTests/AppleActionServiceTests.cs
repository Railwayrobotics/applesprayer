using FakeItEasy;
using Microsoft.Extensions.Logging;
using Railwayrobotics.Applesprayer.Brain.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Railwayrobotics.Applesprayer.Brain.UnitTests
{
    public class AppleActionServiceTests
    {
        private IGpioService _gpioService;
        private AppleActorService _instance;
        private string apple_detected_1 => ReadFile(nameof(apple_detected_1));
        private string person_detected_1 => ReadFile(nameof(person_detected_1));

        public AppleActionServiceTests()
        {
            _gpioService = A.Fake<IGpioService>();
            _instance = new AppleActorService(_gpioService, A.Fake<ILogger<AppleActorService>>());
        }

        [Fact]
        public async Task AppleEvent_IsOff_TurnedOn()
        {
            await _instance.Event(apple_detected_1);
            A.CallTo(() => _gpioService.TurnOn()).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task AppleEvent_IsOn_StillOn()
        {
            A.CallTo(() => _gpioService.IsOn()).Returns(true);

            await _instance.Event(apple_detected_1);
            A.CallTo(() => _gpioService.TurnOn()).MustNotHaveHappened();
        }

        [Fact]
        public async Task ScheduledCheck_NoDetections_NotTurnedOff()
        {
            A.CallTo(() => _gpioService.IsOn()).Returns(false);

            await _instance.ScheduledCheck();

            A.CallTo(() => _gpioService.TurnOn()).MustNotHaveHappened();
            A.CallTo(() => _gpioService.TurnOff()).MustNotHaveHappened();
        }

        [Fact]
        public async Task ScheduledCheck_DetectionActive_NotTurnedOff()
        {
            A.CallTo(() => _gpioService.IsOn()).Returns(true);
            _instance.SetLastDetected(DateTime.UtcNow);

            await _instance.ScheduledCheck();

            A.CallTo(() => _gpioService.TurnOn()).MustNotHaveHappened();
            A.CallTo(() => _gpioService.TurnOff()).MustNotHaveHappened();
        }

        [Fact]
        public async Task ScheduledCheck_DetectionActiveAndOld_TurnOff()
        {
            A.CallTo(() => _gpioService.IsOn()).Returns(true);
            _instance.SetLastDetected(DateTime.UtcNow.AddSeconds(-1));

            await _instance.ScheduledCheck();

            A.CallTo(() => _gpioService.TurnOn()).MustNotHaveHappened();
            A.CallTo(() => _gpioService.TurnOff()).MustHaveHappened();
        }


        [Fact]
        public async Task PersonEvent_IsOff_TurnedOn()
        {
            await _instance.Event(person_detected_1);
            A.CallTo(() => _gpioService.TurnOn()).MustNotHaveHappened();
        }

        private string ReadFile(string prefix) => File.ReadAllText(prefix + ".json");

    }
}
