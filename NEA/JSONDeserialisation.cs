using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEA
{
    internal class StationJSON // StationJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string name { get; set; }
        [JsonPropertyName("Zone")]
        public double zone { get; set; }
        [JsonPropertyName("Connections")]
        public List<ConnectionJSON> connections { get; set; }
    }
    internal class ConnectionJSON // ConnectionJSON class to match the JSON structure
    {
        [JsonPropertyName("Station")]
        public string station { get; set; }
        [JsonPropertyName("Time")]
        public double time { get; set; }
        [JsonPropertyName("Line")]
        public string line { get; set; }
        [JsonPropertyName("Platform")]
        public List<string> platform { get; set; }
    }
    internal class ZoneCostJSON
    {
        // Row refers to end zone - start zone, column represents start zone - 1. e.g. cost from zone 2 to 3 is located in [1][1] Source: https://londondrum.com/transport/adult-train-fares.php
        // Zone 10 represents stations that special fees are applied, e.g. Heathrow Airport T2&3, T4, T5 (Elizabeth Line) , Reading, Watford Junction
        [JsonPropertyName("ZoneCost")]
        public double[][] zoneCost { get; set; } // I used a jagged array here because it's easier to represent in JSON and reduce file size
    }
    internal class GeneralSettings
    {
        [JsonPropertyName("TransferPenalty")]
        public double transferPenalty { get; set; }
        [JsonPropertyName("TimetableFilePath")]
        public string timetableFilepath { get; set; }
        [JsonPropertyName("ZoneCostFilePath")]
        public string zonecostFilepath { get; set; }
        [JsonPropertyName("TouristAttraction")]
        public bool touristAttraction {  get; set; }
        [JsonPropertyName("ReturnStart")]
        public bool returnStart {  get; set; }
        [JsonPropertyName("ExportFilePath")]
        public string exportFilepath { get; set; }
    }
    internal class AlgorithmSettings
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
    internal class SettingsJSON
    {
        [JsonPropertyName("General")]
        public GeneralSettings general { get; set; }
        [JsonPropertyName("Algorithm")]
        public AlgorithmSettings algorithm { get; set; }
    }
    internal class LineColour
    {
        [JsonPropertyName("Foreground")]
        public string Foreground { get; set; }
        [JsonPropertyName("Background")]
        public string Background { get; set; }
    }
    internal class LineColourJSON
    {
        [JsonPropertyName("Colour")]
        public Dictionary<string, LineColour> colour { get; set; }
    }
    internal class DataEditor
    {
        public static Dictionary<string, Station> LoadStation(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                List<StationJSON> stationJsonList = JsonSerializer.Deserialize<List<StationJSON>>(json);
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
                Console.WriteLine($"Station file: {filePath} not vaild.");
                Console.ReadKey();
                System.Environment.Exit(0);
                return null;
            }
        }

        public static void AddTouristAttractions(string filePath, Dictionary<string, Station> stations)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                List<StationJSON> stationJsonList = JsonSerializer.Deserialize<List<StationJSON>>(json);
                foreach (StationJSON stationJson in stationJsonList)
                {
                    Station fromStation = new Station(stationJson.name, stationJson.zone);
                    stations.Add(stationJson.name, fromStation);
                    foreach (ConnectionJSON connection in stationJson.connections)
                    {
                        Station toStation = stations[connection.station];
                        fromStation.Connections.Add(new Connection(toStation, connection.time, connection.line, connection.platform)); // Add foward connection
                        toStation.Connections.Add(new Connection(fromStation, connection.time, connection.line, connection.platform)); // Add reverse connection
                    }
                }
            }
            catch
            {
                Console.WriteLine($"Tourist attraction file: {filePath} not valid.");
                Console.ReadKey();
                System.Environment.Exit(0);
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
                Console.WriteLine($"ZoneCost file: {filePath} not vaild.");
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
                Console.WriteLine($"Settings file: {filePath} not vaild.");
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
                Console.WriteLine($"Settings file : {filePath} not vaild.");
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
                    Console.WriteLine($"Colour file: {filePath} not valid.");
                    Console.ReadKey();
                    System.Environment.Exit(0);
                    return null;
                }
            }

        }
    }
}