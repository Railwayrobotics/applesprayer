using System.Threading.Tasks;

namespace Railwayrobotics.Applesprayer.Brain.Services
{
    public interface IGpioService
    {
        Task Setup(string topicName, int pinNumber);
        bool IsOn();
        Task TurnOn();
        Task TurnOff();
    }
}
