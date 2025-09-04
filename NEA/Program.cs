using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NEA
{
    internal class Program
    {
        public static Random random = new Random();
        public const string settingsFilePath = "settings.json"; // Path to the JSON file containing user settings
        public static Dictionary<string, Station> stations;
        public static SettingsJSON settings;
        static bool IsLineChange(Station current, Connection connection)
            => !current.previousLines.Contains(connection.line);
        static void Dijkstra(Station start)
        {
            PriorityQueue<Station> openSet = new PriorityQueue<Station>();
            start.travelTime = 0;
            start.previousLines = new HashSet<string>(start.Connections.Select(c => c.line)); // Initialize previous lines for the start station
            openSet.Enqueue(start, 0);
            double transferPenalty = settings.general.transferPenalty;
            bool transferred; // Flag to check if a transfer has occurred
            while (!openSet.IsEmpty())
            {
                Station current = openSet.Dequeue();
                if (current.visited) continue;
                current.visited = true;
                foreach (Connection connection in current.Connections)
                {
                    Station neighbour = connection.station;
                    double newTime = current.travelTime + connection.time;
                    transferred = false; // Reset transfer flag for each neighbour
                    if (IsLineChange(current, connection))
                    {
                        newTime += transferPenalty; // Transfer penalty
                        transferred = true; // Set transfer flag if a line change occurs
                    }
                    if (newTime < neighbour.travelTime)
                    {
                        neighbour.travelTime = newTime;
                        neighbour.previousStation = current;
                        if (transferred) // If a transfer occurred, reset previous lines to the current station's lines
                        {
                            neighbour.previousLines = new HashSet<string>(neighbour.Connections.Select(c => c.line));
                        }
                        else
                        {
                            neighbour.previousLines = new HashSet<string>(current.previousLines); // Copy previous lines from the current station
                            neighbour.previousLines.IntersectWith(new HashSet<string>( // Intersect with current lines to keep only the lines that connect to the neighbour
                                current.Connections
                                .Where(c => c.station == neighbour)
                                .Select(c => c.line)));
                        }
                        openSet.Enqueue(neighbour, newTime);
                    }
                }
            }
        }
        static List<string> GetShortestPath(Station end) // I avoided using stacks as paths need to be read repeatively, stack cannot achieve this function (when poped, the last element is removed)
        {
            List<string> path = new List<string>();
            Station current = end;
            int stopCount = 0;
            Station prev = current.previousStation;
            HashSet<string> connectionLines = current.getPrevConnectionLine();
            while (prev != null)
            {
                HashSet<string> currentLines = new HashSet<string>(connectionLines); // Clone a new set to avoid modifying the original set
                HashSet<string> prevConnectionLines = current.getPrevConnectionLine();
                connectionLines.IntersectWith(prevConnectionLines); // Remove lines that are not connecting from the previous station to the current station
                if (connectionLines.Count == 0) // If no lines connecting from one station to current station , a transfer occurred
                {
                    AddTransfer(path, current.name, currentLines, stopCount, false);
                    connectionLines = new HashSet<string>(prevConnectionLines);
                    stopCount = 0;
                }
                path.Add(current.name);
                stopCount++;
                current = prev;
                prev = current.previousStation;
            }
            AddTransfer(path, current.name, connectionLines, stopCount, true); // Add starting segment
            path.Reverse(); // Reverse the path to start from the beginning
            return path;
        }
        static void AddTransfer(List<string> path, string stationName, HashSet<string> lines, int stopCount, bool startStation)
        {
            path.Add($"\nRide {stopCount} stop(s) via:");
            string platformInfo = $"Platform number(s): {string.Join(", ", stations[stationName].getPlatformNumber(lines))}";
            path.Add(platformInfo);
            foreach (string line in lines)
                path.Add(line);
            path.Add("Possible line(s):");
            path.Add(startStation ? $"Start from {stationName}" : $"\nTransfer at {stationName}");
        }
        static double GetCost(Station End) // Calculate cost based on zone number
        {
            double[][] zoneCost = DataEditor.LoadZoneCost(settings.general.zonecostFilepath);
            double minZone = double.MaxValue;
            double maxZone = -1;
            for (Station current = End; current != null; current = current.previousStation)
            {
                if (current.zone < minZone) minZone = current.zone; // There are some stations which are located between 2 zones, represented by a decimal number.
                if (current.zone > maxZone) maxZone = current.zone; // e.g. 1.5 represents between station between zone 1 and 2
            }
            return zoneCost[(int) Math.Ceiling(minZone) - 1][(int) Math.Floor(maxZone) - (int) Math.Ceiling(minZone)];
        }
        static Dictionary<(Station, Station), DecomposedConnection> DecomposedPath(List<Station> targetStations) // Decompose the full graph into K(n)
        {
            Dictionary<(Station, Station), DecomposedConnection> decomposedConnections = new Dictionary<(Station, Station), DecomposedConnection>(); // Store the connection in a dictionary for quick access
            foreach (Station station in targetStations)
            {
                foreach (Station s in stations.Values) // Reset all stations before running Dijkstra's algorithm
                    s.ResetStation();
                Dijkstra(station);
                foreach (Station target in targetStations)
                {
                    if (target == station) continue; // Skip the same station
                    decomposedConnections.Add((station, target),
                        new DecomposedConnection(
                        station,
                        target,
                        target.travelTime,
                        GetCost(target),
                        GetShortestPath(target))); 
                }
            }
            return decomposedConnections;
        }
        static void DisplayFullPath(List<Station> path, Dictionary<(Station, Station), DecomposedConnection> connections)
        {
            double totalTime = 0;
            double totalCost = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                DecomposedConnection connection = connections[(path[i], path[i + 1])]; // Get the connection between the current and next station
                totalTime += connection.estimatedTime;
                totalCost += connection.cost;
                Console.WriteLine(new string('-', Console.WindowWidth));
                Console.WriteLine($"Route from {path[i].name} to {path[i + 1].name}:");
                connection.path.ForEach(Console.WriteLine); // Print the path segments
                Console.WriteLine($"\nSubpath estimated time: {connection.estimatedTime} minutes");
                Console.WriteLine(connection.cost == 0 ? "Special fares applied" : $"Cost of subpath: £{connection.cost}");
                Console.WriteLine(new string('-', Console.WindowWidth));
            }
            Console.WriteLine($"Total estimated time: {totalTime} minutes");
            Console.WriteLine($"Total cost: £{totalCost}");
        }
        static List<Station> AntColonyOptimisation(List<Station> targetStations, Dictionary<(Station, Station), DecomposedConnection> connections, bool timeOrCost)
        /* Reference: https://learn.microsoft.com/en-us/archive/msdn-magazine/2012/february/test-run-ant-colony-optimization ,
        https://www.youtube.com/watch?v=oXb2nC-e_EA ,
        https://medium.com/data-science/ant-colony-optimization-in-action-6d9106de60af,
        https://www.sciencedirect.com/science/article/pii/S2667305324000863,
        https://www.youtube.com/watch?v=u7bQomllcJw&t=988s */
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int ants = targetStations.Count;
            double maxTime = settings.algorithm.executionTime * 1000; // Convert to milliseconds
            double alpha = settings.algorithm.alpha;  // influence of pheromone
            double beta = settings.algorithm.beta;   // influence of heuristic
            double rho = settings.algorithm.rho;  // pheromone evaporation rate
            double Q = settings.algorithm.q;    // pheromone deposit factor
            List<Station> bestPath = null;
            double bestLength = double.MaxValue;
            Dictionary<(Station, Station), double> pheromones = new Dictionary<(Station, Station), double>();
            Dictionary<(Station, Station), double> distance = new Dictionary<(Station, Station), double>();
            foreach ((Station, Station) key in connections.Keys)
                pheromones[key] = settings.algorithm.ipc; // Initialize pheromone concentration
            if (timeOrCost)
            {
                foreach ((Station, Station) key in connections.Keys)
                    distance[key] = connections[key].estimatedTime; // Use estimated time as distance
            }
            else
            {
                foreach ((Station, Station) key in connections.Keys)
                    distance[key] = connections[key].cost; // Use cost as distance
            }
            while (stopwatch.ElapsedMilliseconds < maxTime) // Run until the execution time is reached
            {
                List<(List<Station> path, double length)> allAntsPaths = new List<(List<Station>, double)>();
                for (int ant = 0; ant < ants; ant++)
                {
                    List<Station> unvisited = new List<Station>(targetStations);
                    Station current = targetStations[0]; // Start from the first station
                    List<Station> path = new List<Station> { current };
                    unvisited.Remove(current);
                    double totalLength = 0;
                    while (unvisited.Count > 0)
                    {
                        double sum = 0; // Sum of probabilities for normalisation
                        // Applying formula: P(i,j) = (tau(i,j)^alpha * eta(i,j)^beta) / sum(tau(i,k)^alpha * eta(i,k)^beta) for all k in unvisited
                        Dictionary<Station, double> probabilities = new Dictionary<Station, double>();
                        foreach (Station next in unvisited)
                        {
                            double tau = Math.Pow(pheromones[(current, next)], alpha);
                            double eta = Math.Pow(1 / distance[(current, next)], beta);
                            probabilities[next] = tau * eta;
                            sum += probabilities[next];
                        }
                        double rand = random.NextDouble() * sum; // Implicitly normalise probabilities (dividing sum(tau(i,k)^alpha * eta(i,k)^beta))
                        double cumulativeProbability = 0;
                        Station chosen = null;
                        foreach (KeyValuePair<Station, double> kvp in probabilities)
                        {
                            cumulativeProbability += kvp.Value;
                            if (rand < cumulativeProbability)
                            {
                                chosen = kvp.Key;
                                break;
                            }
                        }
                        path.Add(chosen);
                        unvisited.Remove(chosen);
                        totalLength += distance[(current, chosen)]; // Add the distance to the path length
                        current = chosen; // Move to the next station
                    }
                    if (totalLength < bestLength) // Track best path
                    {
                        bestLength = totalLength;
                        bestPath = new List<Station>(path);
                    }
                    allAntsPaths.Add((path, totalLength));
                }
                foreach ((Station, Station) key in connections.Keys)
                {
                    pheromones[key] *= (1 - rho); // Evaporate pheromones
                }
                foreach ((List<Station> antPath, double length) in allAntsPaths)
                {
                    double pheromoneDeposit = Q / length; // Deposit pheromones based on path length
                    for (int i = 0; i < antPath.Count - 1; i++)
                    {
                        pheromones[(antPath[i], antPath[i + 1])] += pheromoneDeposit; // Update pheromones for each segment of the path
                    }
                }
            }
            return bestPath;
        }
        static string AutoCompleteInput()
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
                if (match != null && match.Length > input.Length) // Reprint input + suggestion
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
        static Station GetStation()
        {
            while (true)
            {
                Console.Write("Enter station name (Tab for auto-complete): ");
                if (stations.TryGetValue(AutoCompleteInput(), out Station station))
                    return station;
                Console.WriteLine("Station not found. Please try again.");
            }
        }
        static double GetDouble(string prompt, double min, double max)
        {
            double value;
            while (true)
            {
                Console.Write(prompt);
                if (double.TryParse(Console.ReadLine(), out value) && value >= min && value <= max)
                    return value;
                Console.WriteLine($"Invalid input. Please enter a number between {min} and {max}.");
            }
        }
        static int GetOption(List<string> options)
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
        static void StationSearch()
        {
            int option, numOfStations;
            List<Station> targetStations = new List<Station>();
            while (true)
            {
                Console.Write("Enter number of stations to find path (2-20): ");
                if (int.TryParse(Console.ReadLine(), out option) && option >= 2 && option <= 20)
                {
                    numOfStations = option;
                    break;
                }
                Console.WriteLine("Invalid input. Please enter a number between 2 and 20.");
            }
            for (int i = 0; i < numOfStations; i++)
            {
                targetStations.Add(GetStation()); // Get target stations from user input
            }
            Dictionary<(Station, Station), DecomposedConnection> connections = DecomposedPath(targetStations); 
            Console.Clear();
            while (true)
            {
                option = GetOption(new List<string> { "Manually ordered", "Auto optimisation based on time", "Auto optimisation based on cost", "Exit" });
                switch (option)
                {
                    case 1:
                        Console.WriteLine($"Manually ordered:");
                        DisplayFullPath(targetStations, connections);
                        Console.ReadLine();
                        continue;
                    case 2:
                        Console.WriteLine("Ant Colony Optimisation based on time:");
                        List<Station> acoTimePath = AntColonyOptimisation(targetStations, connections, true);
                        DisplayFullPath(acoTimePath, connections);
                        Console.ReadLine();
                        continue;
                    case 3:
                        Console.WriteLine("Ant Colony Optimisation based on cost:");
                        List<Station> acoCostPath = AntColonyOptimisation(targetStations, connections, false);
                        DisplayFullPath(acoCostPath, connections);
                        Console.ReadLine(); 
                        continue;
                    case 4:
                        return;
                }
            }
        }
        static void TimetableSettings()
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
        static void GeneralSettings()
        {
            int option;
            while (true)
            {
                option = GetOption(new List<string> { "Walking Speed", "Timetable (not implemented)", "Back to settings menu" });
                switch (option)
                {
                    case 1:
                        settings.general.transferPenalty = 10 + GetOption(new List<string> { "Very fast", "Fast", "Average", "Slow", "Very Slow" }); // Each option represents 1 minutes increment
                        continue;
                    case 2:
                        TimetableSettings();
                        continue;
                    case 3:
                        return;
                }
            }
        }
        static void AlgorithmSettings()
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
        static void Settings()
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
                        DataEditor.SaveSettings(settingsFilePath, settings); // Save settings to JSON file
                        return;
                    case 4:
                        return;
                }
            }
        }
        static void Main(string[] args)
        {
            int option;
            while (true)
            {
                settings = DataEditor.LoadSettings(settingsFilePath); // Load settings from JSON file
                stations = DataEditor.LoadStation(settings.general.timetableFilepath); // Load stations from JSON file
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