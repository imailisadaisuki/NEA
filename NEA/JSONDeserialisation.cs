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
    public class StationJSON // StationJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string name { get; set; }
        [JsonPropertyName("Zone")]
        public double zone { get; set; }
        [JsonPropertyName("Connections")]
        public List<ConnectionJSON> connections { get; set; }
    }
    public class ConnectionJSON // ConnectionJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string station { get; set; }
        [JsonPropertyName("Time")]
        public double time { get; set; }
        [JsonPropertyName("Line")]
        public string line { get; set; }
        [JsonPropertyName("Platform")]
        public HashSet<string> platform { get; set; }
    }
    public class ZoneCostJSON
    {
        // Row refers to end zone - start zone, column represents start zone - 1. e.g. cost from zone 2 to 3 is located in [1][1] Source: https://londondrum.com/transport/adult-train-fares.php
        // Zone 10 represents stations that special fees are applied, e.g. Heathrow Airport T2&3, T4, T5 (Elizabeth Line) , Reading, Watford Junction
        [JsonPropertyName("ZoneCost")]
        public double[][] zoneCost { get; set; } // I used a jagged array here because it's easier to represent in JSON and reduce file size
    }
    public class GeneralSettings
    {
        [JsonPropertyName("TransferPenalty")]
        public double transferPenalty { get; set; }
        [JsonPropertyName("TimetableFilePath")]
        public string timetableFilepath { get; set; }
        [JsonPropertyName("ZoneCostFilePath")]
        public string zonecostFilepath { get; set; }
        [JsonPropertyName("TouristAttraction")]
        public bool touristAttraction {  get; set; }
        [JsonPropertyName("ExportFilePath")]
        public string exportFilepath { get; set; }
    }
    public class AlgorithmSettings
    {
        [JsonPropertyName("ExecutionTime")]
        public double executionTime { get; set; }
        [JsonPropertyName("IPC")]
        public double ipc { get; set; }
        [JsonPropertyName("Alpha")]
        public double alpha { get; set; }
        [JsonPropertyName("Beta")]
        public double beta { get; set; }
        [JsonPropertyName("Rho")]
        public double rho { get; set; }
        [JsonPropertyName("Q")]
        public double q { get; set; }
    }
    public class SettingsJSON
    {
        [JsonPropertyName("General")]
        public GeneralSettings general { get; set; }
        [JsonPropertyName("Algorithm")]
        public AlgorithmSettings algorithm { get; set; }
    }
    public class LineColour
    {
        [JsonPropertyName("Foreground")]
        public string Foreground { get; set; }
        [JsonPropertyName("Background")]
        public string Background { get; set; }
    }
    public class LineColourJSON
    {
        [JsonPropertyName("Colour")]
        public Dictionary<string, LineColour> colour { get; set; }
    }
    public class DataEditor
    {
        public static Dictionary<string, Station> LoadStation(string filePath1, string filePath2, bool loadFile2)
        {
            try
            {
                string json = File.ReadAllText(filePath1);
                List<StationJSON> stationJsonList = JsonSerializer.Deserialize<List<StationJSON>>(json);
                if (loadFile2)
                {
                    string json2 = File.ReadAllText(filePath2);
                    List<StationJSON> stationJSONList2 = JsonSerializer.Deserialize<List<StationJSON>>(json2);
                    stationJsonList = stationJsonList.Union(stationJSONList2).ToList(); // Avoid duplicate stations
                }
                Dictionary<string, Station> stations = stationJsonList
                    .ToDictionary(s => s.name, s => new Station(s.name, s.zone)); // Create stations objects from the JSON data
                foreach (StationJSON stationJson in stationJsonList) // Create connections between the stations
                {
                    Station fromStation = stations[stationJson.name];
                    foreach (ConnectionJSON connection in stationJson.connections)
                        fromStation.Connections.Add(new Connection(stations[connection.station], connection.time, connection.line, connection.platform));
                }
                return stations;
            }
            catch
            {
                Console.WriteLine("Station file not vaild.");
                Console.ReadKey();
                System.Environment.Exit(0);
                return null;
            }
        }
        public static double[][] LoadZoneCost(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                ZoneCostJSON zoneCostData = JsonSerializer.Deserialize<ZoneCostJSON>(json);
                return zoneCostData.zoneCost;
            }
            catch
            {
                Console.WriteLine("ZoneCost file not vaild.");
                Console.ReadKey();
                System.Environment.Exit(0);
                return null;
            }
            
        }
        public static SettingsJSON LoadSettings(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                SettingsJSON settings = JsonSerializer.Deserialize<SettingsJSON>(json);
                return settings;
            }
            catch
            {
                Console.WriteLine("Settings file not vaild.");
                Console.ReadKey();
                System.Environment.Exit(0);
                return null;
            }
        }
        public static void SaveSettings(string filePath, SettingsJSON settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(filePath, json);
            }
            catch
            {
                Console.WriteLine("Settings file not vaild.");
                Console.ReadKey();
                System.Environment.Exit(0);
            }

        }
        public static Dictionary<string, (ConsoleColor, ConsoleColor)> LoadColours(string filePath)
        {
            while (true)
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    LineColourJSON colourStrings = JsonSerializer.Deserialize<LineColourJSON>(json);
                    return colourStrings.colour.ToDictionary(
                        kvp => kvp.Key,
                        kvp => ((ConsoleColor)Enum.Parse(typeof(ConsoleColor), kvp.Value.Foreground),
                                (ConsoleColor)Enum.Parse(typeof(ConsoleColor), kvp.Value.Background)) // Convert to dictionary of string, (foreground colour, background colour)
                    );
                }
                catch
                {
                    Console.WriteLine("Colour file not valid.");
                    Console.ReadKey();
                    System.Environment.Exit(0);
                    return null;
                }
            }

        }
    }
}