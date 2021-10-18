using Newtonsoft.Json;
using System;

namespace Railwayrobotics.FileSync.Models
{
    public class ModuleConfiguration
    {
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        [JsonProperty("sync")]
        public int Sync { get; set; }

        [JsonIgnore]
        public int DesiredSync { get; set; }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static ModuleConfiguration Deserialize(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<ModuleConfiguration>(json);
            }
            catch(Exception e)
            {
                throw new ArgumentException("Failed to deserialize json: " + json, e);
            }
        }
    }
}
