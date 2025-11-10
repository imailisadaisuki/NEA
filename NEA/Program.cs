using System;
using System.Collections.Generic;

namespace NEA
{
    internal class Program 
    {
        public const string settingsFilePath = "settings.json"; // Path to the JSON file containing user settings
        public const string tubeColoursFilePath = "tubecolours.json"; // Path to the JSON file containing tube line colours
        public const string touristAttractionFilePath = "touristAttraction.json"; // Path to the JSON file containing tourist attractions
        public static Dictionary<string, Station> stations;
        public static SettingsJSON settings;
        public static double[][] zoneCost;
        public static Dictionary<string, (ConsoleColor, ConsoleColor)> tubeColours;
        public static void ApplySettings()
        {
            DataEditor.SaveSettings(settingsFilePath, settings); // Save settings to JSON file
            Load();
        }
        private static void Load()
        {
            settings = DataEditor.LoadSettings(settingsFilePath); // Load settings from JSON file
            stations = DataEditor.LoadStation(settings.general.timetableFilepath); // Load stations from JSON file
            if (settings.general.touristAttraction)
                DataEditor.AddTouristAttractions(touristAttractionFilePath, stations); // Add tourist attractions to stations if enabled in settings
            tubeColours = DataEditor.LoadColours(tubeColoursFilePath); // Load tube line colours from JSON file
            zoneCost = DataEditor.LoadZoneCost(settings.general.zonecostFilepath); // Load zone cost from JSON file
        }
        static void Main(string[] args)
        {
            Load();
            AntColonyOptimiser optimiser = new AntColonyOptimiser(stations, settings, zoneCost);
            UI ui = new UI(optimiser, stations, settings, tubeColours);
            ui.Run();
        }
    }
}