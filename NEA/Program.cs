using System;
using System.Collections.Generic;

namespace NEA
{
    internal class Program 
    {
        public const string settingsFilePath = "settings.json";
        public const string tubeColoursFilePath = "tubecolours.json";
        public const string touristAttractionFilePath = "touristAttraction.json";
        public static Dictionary<string, Station> stations;
        public static SettingsJSON settings;
        public static double[][] zoneCost;
        public static Dictionary<string, (ConsoleColor, ConsoleColor)> tubeColours;
        public static void ApplySettings()
        {
            DataEditor.SaveSettings(settingsFilePath, settings); // Save settings to JSON file
        }

        private static void Load()
        {
            settings = DataEditor.LoadSettings(settingsFilePath);
            stations = DataEditor.LoadStation(settings.general.timetableFilepath);
            if (settings.general.touristAttraction)
                DataEditor.AddTouristAttractions(touristAttractionFilePath, stations); // Add tourist attractions to stations if enabled in settings
            tubeColours = DataEditor.LoadColours(tubeColoursFilePath);
            zoneCost = DataEditor.LoadZoneCost(settings.general.zonecostFilepath);
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Load();
                AntColonyOptimiser optimiser = new AntColonyOptimiser(stations, settings, zoneCost);
                UI ui = new UI(optimiser, stations, settings, tubeColours);
                ui.Run();
            }
        }
    }
}