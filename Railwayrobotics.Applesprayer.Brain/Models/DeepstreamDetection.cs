using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Railwayrobotics.Applesprayer.Brain.Models
{
    public class DeepstreamDetection
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("@timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("sensorId")]
        public string SensorId { get; set; }

        [JsonProperty("objects")]
        public string[] Objects { get; set; }

        public List<DeepstreamDetectionObject> GetObjects(string lookingFor = null)
        {
            var result = new List<DeepstreamDetectionObject>();

            if (Objects == null || Objects.Length == 0)
                return result;

            foreach(var item in Objects) 
            {
                if (lookingFor != null && !item.Contains(lookingFor))
                    continue;

                var splitted = item.Split("|");
                if (splitted.Count() != 6)
                    throw new ArgumentException("Expected to have 6 elements separated by |. Got: " + item);

                result.Add(new DeepstreamDetectionObject
                {
                    TrackedId = splitted[0].Trim(),
                    ObjectName = splitted[5].Trim()
                });
            }

            return result;
        }
    }

    public class DeepstreamDetectionObject
    {
        public string TrackedId { get; set; }
        public string ObjectName { get; set; }
    }
}