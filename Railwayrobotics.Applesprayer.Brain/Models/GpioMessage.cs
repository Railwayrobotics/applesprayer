using Newtonsoft.Json;

namespace Railwayrobotics.Applesprayer.Brain.Models
{
    public class GpioMessage
    {
        [JsonProperty("output_pin")]
        public int Pin { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }

        public static GpioMessage SetGpioLow(int pin) 
        {
            return new GpioMessage
            {
                Pin = pin,
                Value = 0
            };
        }

        public static GpioMessage SetGpioHigh(int pin)
        {
            return new GpioMessage
            {
                Pin = pin,
                Value = 1
            };
        }
    }
}
