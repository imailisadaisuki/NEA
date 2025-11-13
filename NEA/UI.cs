using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class PathInstruction
    {
        public string instruction { get; set; }
        public bool isLine { get; set; }
        public PathInstruction(string instruction, bool isLine = false)
            => (this.instruction, this.isLine) = (instruction, isLine);

        private string NormalizeLineName(string line) =>
            line.Split('(')[0].Trim(); // e.g. "Central Line (Epping)" -> "Central Line"

        private string ConvertColourRGB(ConsoleColor colour)
        {
            switch (colour)
            {
                case ConsoleColor.Black:
                    return "000000";
                case ConsoleColor.DarkBlue:
                    return "000080";
                case ConsoleColor.DarkGreen:
                    return "008000";
                case ConsoleColor.DarkCyan:
                    return "008080";
                case ConsoleColor.DarkRed:
                    return "800000";
                case ConsoleColor.DarkMagenta:
                    return "800080";
                case ConsoleColor.DarkYellow:
                    return "808000";
                case ConsoleColor.Gray:
                    return "C0C0C0";
                case ConsoleColor.DarkGray:
                    return "808080";
                case ConsoleColor.Blue:
                    return "0000FF";
                case ConsoleColor.Green:
                    return "00FF00";
                case ConsoleColor.Cyan:
                    return "00FFFF";
                case ConsoleColor.Red:
                    return "FF0000";
                case ConsoleColor.Magenta:
                    return "FF00FF";
                case ConsoleColor.Yellow:
                    return "FFFF00";
                default:
                    return "FFFFFF";
            }
        }

        public (string, string) GetRGBColour(Dictionary<string, (ConsoleColor, ConsoleColor)> lineColors)
        {
            string trimmed = NormalizeLineName(instruction);
            var (fg, bg) = lineColors[trimmed];
            return (ConvertColourRGB(fg), ConvertColourRGB(bg));
        }

        public void DisplaySegment(Dictionary<string, (ConsoleColor, ConsoleColor)> lineColors)
        {
            string trimmed = NormalizeLineName(instruction);
            if (isLine && lineColors.ContainsKey(trimmed))
            {
                var (fg, bg) = lineColors[trimmed];
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
                Console.Write(instruction);
                Console.ResetColor();
                Console.WriteLine();
            }
            else
                Console.WriteLine(instruction);
        }
    }
    internal class UI
    {
        private readonly AntColonyOptimiser optimiser;
        private readonly Dictionary<string, Station> stations;
        private readonly SettingsJSON settings;
        private readonly Dictionary<string, (ConsoleColor, ConsoleColor)> tubeColours;
        public UI(AntColonyOptimiser optimiser, Dictionary<string, Station> stations, SettingsJSON settings, Dictionary<string, (ConsoleColor, ConsoleColor)> tubeColours)
            => (this.optimiser, this.stations, this.settings, this.tubeColours) = (optimiser, stations, settings, tubeColours);

        private string AutoCompleteInput()
        {
            string input = "";
            string match = "";
            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input = input.Substring(0, input.Length - 1);
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return input;
                }
                else if (key.Key == ConsoleKey.Tab && match != null)
                {
                    input = match;
                }
                else if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
                {
                    input += key.KeyChar;
                }
                match = stations.Keys.FirstOrDefault(name => name.StartsWith(input, StringComparison.OrdinalIgnoreCase)); // Get first match
                Console.SetCursorPosition(cursorLeft, cursorTop); // Clear line
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(cursorLeft, cursorTop);
                if (match != null && input != "" && match.Length > input.Length) // Reprint input + suggestion
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(input);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(match.Substring(input.Length));
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(input);
                }
                Console.SetCursorPosition(cursorLeft + input.Length, cursorTop); // Reset cursor for next loop
            }
        }

        private void GetStations(List<Station> targetStations, bool startStation)
        {
            while (true)
            {
                Console.Write(startStation ? "Enter name of starting location (Tab for auto-complete): " : "Enter location name (Tab for auto-complete): ");
                if (stations.TryGetValue(AutoCompleteInput(), out Station station))
                {
                    if (!targetStations.Contains(station))
                    {
                        targetStations.Add(station);
                        return;
                    }
                    Console.WriteLine("Location already added. Please enter a different location.");
                }
                else
                    Console.WriteLine("Location not found. Please try again.");
            }
        }

        private double GetDouble(string prompt, double min, double max)
        {
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out double value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"Invalid input. Please enter a number between {min} and {max}.");
            }
        }

        private int GetOption(List<string> options)
        {
            Console.Clear();
            Console.WriteLine("Press Up/Down arrow or W/S key to choose an option. Press Enter to confirm.\n");
            options.ForEach(opt => Console.WriteLine($"   {opt}")); // Print options
            Console.CursorLeft = 0;
            Console.CursorTop = 2;
            Console.Write("->");
            int option = 1;
            while (true)
            {
                ConsoleKeyInfo choice = Console.ReadKey(true);
                if ((choice.Key == ConsoleKey.DownArrow || choice.Key == ConsoleKey.S) && option < options.Count)
                {
                    Console.CursorTop = option + 1;
                    Console.CursorLeft = 0;
                    Console.Write("  ");
                    option++;
                    Console.CursorTop = option + 1;
                    Console.CursorLeft = 0;
                    Console.Write("->");
                }
                else if ((choice.Key == ConsoleKey.UpArrow || choice.Key == ConsoleKey.W) && option > 1)
                {
                    Console.CursorTop = option + 1;
                    Console.CursorLeft = 0;
                    Console.Write("  ");
                    option--;
                    Console.CursorTop = option + 1;
                    Console.CursorLeft = 0;
                    Console.Write("->");

                }
                else if (choice.Key == ConsoleKey.Enter)
                {
                    Console.Clear();
                    return option;
                }
            }
        }

        private void DisplayInstructions(List<Station> targetStations)
        {
            Dictionary<(Station, Station), DecomposedConnection> connections = optimiser.DecomposedPath(targetStations);
            Console.Clear();
            List<PathInstruction> fullInstructions;
            List<Station> orderedStation = new List<Station>();
            while (true)
            {
                int option = GetOption(new List<string> { "Manually ordered", "Auto optimisation based on time", "Auto optimisation based on cost", "Exit" });
                switch (option)
                {
                    case 1:
                        Console.WriteLine($"Manually ordered:");
                        orderedStation = new List<Station>(targetStations);
                        if (settings.general.returnStart) orderedStation.Add(targetStations[0]);
                        break;
                    case 2:
                        Console.WriteLine("Ant Colony Optimisation based on time:");
                        orderedStation = optimiser.AntColonyOptimisation(targetStations, connections, true);
                        break;
                    case 3:
                        Console.WriteLine("Ant Colony Optimisation based on cost:");
                        orderedStation = optimiser.AntColonyOptimisation(targetStations, connections, false);
                        break;
                    case 4:
                        return;
                }
                fullInstructions = optimiser.GetFullPath(orderedStation, connections);
                foreach (PathInstruction segment in fullInstructions)
                    segment.DisplaySegment(tubeColours);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
                option = GetOption(new List<string> { "Export file as PDF", "Return to optimisation choices" });
                if (option == 1)
                    PDFExport.Export(fullInstructions, settings.general.exportFilepath, tubeColours);
            }

        }

        private void StationSearch()
        {
            int numOfStations;
            List<Station> targetStations = new List<Station>();
            while (true)
            {
                Console.Write("Enter number of stations to find path (2-20): ");
                if (int.TryParse(Console.ReadLine(), out int option) && option >= 2 && option <= 20)
                {
                    numOfStations = option;
                    break;
                }
                Console.WriteLine("Invalid input. Please enter a number between 2 and 20.");
            }
            GetStations(targetStations, true); // Get first station from user input
            for (int i = 0; i < numOfStations - 1; i++)
            {
                GetStations(targetStations, false); // Get target stations from user input
            }
            DisplayInstructions(targetStations);
        }

        private void TimetableSettings()
        {
            int option;
            while (true)
            {
                option = GetOption(new List<string> { "Weekdays Peak", "Weekdays Offpeak", "Weekends" });
                switch (option)
                {
                    case 1:
                        settings.general.timetableFilepath = "undergroundWDP.json";
                        settings.general.zonecostFilepath = "zonecostP.json";
                        break;
                    case 2:
                        settings.general.timetableFilepath = "undergroundWDOP.json";
                        settings.general.zonecostFilepath = "zonecostOP.json";
                        break;
                    case 3:
                        settings.general.timetableFilepath = "undergroundWE.json";
                        settings.general.zonecostFilepath = "zonecostOP.json";
                        break;
                }
            }
        }

        private void GeneralSettings()
        {
            int option;
            while (true)
            {
                option = GetOption(new List<string> { "Walking Speed", "Timetable", "Applying tourist attraction", "Set name of export PDF", "Back to settings menu" });
                switch (option)
                {
                    case 1:
                        settings.general.transferPenalty = 10 + GetOption(new List<string> { "Very fast", "Fast", "Average", "Slow", "Very Slow" }); // Each option represents 1 minutes increment
                        continue;
                    case 2:
                        TimetableSettings();
                        continue;
                    case 3:
                        settings.general.touristAttraction = GetOption(new List<string> { "Apply tourist attraction", "Do not apply tourist attraction" }) == 1 ? true : false;
                        continue;
                    case 4:
                        settings.general.returnStart = GetOption(new List<string> { "Return to start station", "Do not return to start station" }) == 1 ? true : false;
                        continue;
                    case 5:
                        settings.general.exportFilepath = PDFExport.SetFileName();
                        continue;
                    case 6:
                        return;
                }
            }
        }

        private void AlgorithmSettings()
        {
            int option;
            while (true)
            {
                option = GetOption(new List<string> { "Execution Time", "Initial pheromones concentration", "Influence of pheromone", "Influence of heuristic", "Pheromone evaporation rate", "Pheromone deposit factor", "Back to settings menu" });
                switch (option)
                {
                    case 1:
                        settings.algorithm.executionTime = GetDouble("Enter execution time in seconds (1-600): ", 1, 600);
                        continue;
                    case 2:
                        settings.algorithm.ipc = GetDouble("Enter initial pheromones concentration (0.1-2): ", 0.1, 2);
                        continue;
                    case 3:
                        settings.algorithm.alpha = GetDouble("Enter alpha (influence of pheromone) (1-5): ", 1, 5);
                        continue;
                    case 4:
                        settings.algorithm.beta = GetDouble("Enter beta (influence of heuristic) (1-5): ", 1, 5);
                        continue;
                    case 5:
                        settings.algorithm.rho = GetDouble("Enter rho (pheromone evaporation rate) (0.1-2): ", 0.1, 2);
                        continue;
                    case 6:
                        settings.algorithm.q = GetDouble("Enter Q (pheromone deposit factor) (1-100): ", 1, 100);
                        continue;
                    case 7:
                        return;
                }
            }
        }

        private void Settings()
        {
            int option;
            while (true)
            {
                option = GetOption(new List<string> { "General settings", "Algorithm settings (Advanced)", "Save and return", "Return without saving" });
                switch (option)
                {
                    case 1:
                        GeneralSettings();
                        continue;
                    case 2:
                        AlgorithmSettings();
                        continue;
                    case 3:
                        Program.ApplySettings();
                        return;
                    case 4:
                        return;
                }
            }
        }

        public void Run()
        {
            int option;
            while (true)
            {
                option = GetOption(new List<string> { "Find route", "Settings", "Exit" });
                switch (option)
                {
                    case 1:
                        StationSearch(); // Start the station search process
                        continue;
                    case 2:
                        Settings();
                        continue;
                    case 3:
                        System.Environment.Exit(0); // Exit the program
                        return;

                }
            }
        }
    }
}
