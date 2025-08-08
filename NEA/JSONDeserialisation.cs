using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEA
{
    public class StationJson // StationJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string name { get; set; }
        [JsonPropertyName("Zone")]
        public double zone { get; set; }
        [JsonPropertyName("Connections")]
        public List<ConnectionJson> connections { get; set; }
    }

    public class ConnectionJson // ConnectionJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string station { get; set; }
        [JsonPropertyName("Time")]
        public double time { get; set; }
        [JsonPropertyName("Line")]
        public string line { get; set; }
    }
}