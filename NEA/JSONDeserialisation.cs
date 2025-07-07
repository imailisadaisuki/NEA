using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEA
{
    public class StationJson //StationJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string name { get; set; }
        [JsonPropertyName("Zone")]
        public int zone { get; set; }
        [JsonPropertyName("Connections")]
        public List<ConnectionJson> connections { get; set; }
    }

    public class ConnectionJson //ConnectionJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string station { get; set; }
        [JsonPropertyName("Time")]
        public int time { get; set; }
        [JsonPropertyName("Line")]
        public string line { get; set; }
    }
    public static class StationLoader
    {
        public static Dictionary<string, Station> LoadFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            List<StationJson> stationJsonList = JsonSerializer.Deserialize<List<StationJson>>(json);
            Dictionary<string, Station> stations = new Dictionary<string, Station>();
            foreach (StationJson stationJson in stationJsonList) //create stations objects from the JSON data
            {
                stations[stationJson.name] = new Station(stationJson.name, stationJson.zone);
            }
            foreach (StationJson stationJson in stationJsonList) //create connections between the stations
            {
                Station fromStation = stations[stationJson.name];
                foreach (ConnectionJson connection in stationJson.connections)
                {
                    fromStation.Connections.Add(new Connection(stations[connection.station], connection.time, connection.line));
                }
            }
            return stations;
        }
    }
}
