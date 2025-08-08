using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;

namespace NEA
{
    internal class Program
    {
        public static Random random = new Random();
        public const string filePath = "underground.json"; // Path to the JSON file containing station data
        public static readonly double[,] zoneCost = { 
            //row refers to end zone, column represents start zone. Source: https://londondrum.com/transport/adult-train-fares.php
            // Zone 10 represents stations that special fees apply, such as Heathrow Airport T2&3, T4, T5 (Elizabeth Line) , Reading, Watford Junction, etc.
            { 2.9, 3.5, 3.8, 4.6, 5.2, 5.8, 6.7, 8.2, 8.3, 0 },
            { 0, 2.1, 2.3, 3, 3.2, 3.6, 4.9, 5.6, 8.3, 0 },
            { 0, 0, 2.1, 2.3, 3, 3.2, 4, 4.8, 8.3, 0 },
            { 0, 0, 0, 2.1, 2.3, 3, 3.2, 4, 4.1, 0 },
            { 0, 0, 0, 0, 2.1, 2.3, 2.9, 3.2, 3.5, 0 },
            { 0, 0, 0, 0, 0, 2.1, 2.2, 2.9, 3, 0 },
            { 0, 0, 0, 0, 0, 0, 2, 2.2, 2.3, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 2, 2.2, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        static Dictionary<string, Station> LoadFromJson(string filePath)
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
        static void ResetStations(Dictionary<string, Station> stations)
        {
            foreach (Station station in stations.Values)
            {
                station.gScore = int.MaxValue;
                station.previousStation = null;
                station.visited = false;
                station.previousLines.Clear(); // Reset previous lines for each station
            }
        }
        static bool IsLineChange(Station current, Connection connection)
            => !current.previousLines.Contains(connection.line) && connection.line != "Walk";
        static double Heuristic(Station from, Station to)
            => Math.Abs(from.zone - to.zone);
        static void AStar(Station start, Station goal)
        {
            PriorityQueue<Station> openSet = new PriorityQueue<Station>();
            start.gScore = 0;
            start.previousLines = new HashSet<string>(start.Connections.Select(c => c.line)); // Initialize previous lines for the start station
            openSet.Enqueue(start, 0);
            while (!openSet.IsEmpty())
            {
                Station current = openSet.Dequeue();
                if (current == goal) break;
                if (current.visited) continue;
                current.visited = true;
                foreach (Connection connection in current.Connections)
                {
                    Station neighbor = connection.station;
                    double tentativeGScore = current.gScore + connection.time;
                    if (IsLineChange(current, connection))
                    {
                        tentativeGScore += 13; // Transfer penalty
                    }
                    neighbor.previousLines = new HashSet<string>( // Save all lines for current leg
                        current.Connections
                        .Where(c => c.station == neighbor)
                        .Select(c => c.line));
                    if (tentativeGScore < neighbor.gScore)
                    {
                        neighbor.gScore = tentativeGScore;
                        neighbor.previousStation = current;
                        neighbor.previousLines = new HashSet<string> { connection.line };
                        openSet.Enqueue(neighbor, tentativeGScore + Heuristic(neighbor, goal));
                    }
                }
            }
        }
        static Stack<string> GetShortestPath(Station end)
        {
            Stack<string> path = new Stack<string>();
            Station current = end;
            int stopCount = 0;
            Station prev = current.previousStation;
            HashSet<string> connectionLines = new HashSet<string>(current.getPrevConnectionLine());
            while (prev != null)
            {
                HashSet<string> currentLines = new HashSet<string>(connectionLines); //clone a new set to avoid modifying the original set
                HashSet<string> prevConnectionLines = new HashSet<string>(current.getPrevConnectionLine());
                connectionLines.IntersectWith(prevConnectionLines); // Remove lines that are not connecting from the previous station to the current station
                if (connectionLines.Count == 0) // If no lines connecting from one station to current station , a transfer occurred
                {
                    AddTransfer(path, current.name, currentLines, stopCount, false);
                    connectionLines = new HashSet<string>(prevConnectionLines);
                    stopCount = 0;
                }
                path.Push(current.name);
                stopCount++;
                current = prev;
                prev = current.previousStation;
            }
            AddTransfer(path, current.name, connectionLines, stopCount, true); // Add starting segment
            return path;
        }
        static void AddTransfer(Stack<string> path, string stationName, HashSet<string> lines, int stopCount, bool startStation)
        {
            path.Push($"\nRide {stopCount} stop(s) via:");
            foreach (string line in lines)
                path.Push(line);
            path.Push("Possible line(s):");
            path.Push(startStation ? $"Start from {stationName}" : $"\nTransfer at {stationName}");
        }
        static double GetCost(Station End) //calculate cost based on Zone number
        {
            double minZone = double.MaxValue;
            double maxZone = -1;
            for (Station current = End; current != null; current = current.previousStation)
            {
                if (Math.Ceiling(current.zone) < minZone) minZone = current.zone; // There are some stations which are located between 2 zones, represented by a decimal number.
                if (Math.Floor(current.zone) > maxZone) maxZone = current.zone; // e.g. 1.5 represents between station between zone 1 and 2
            }
            return zoneCost[(int)minZone - 1, (int)maxZone - 1];
        }
        static Dictionary<(Station, Station), DecomposedConnection> DecomposedPath(List<Station> targetStations, Dictionary<string, Station> stations) // Decompose the full graph into K(n)
        {
            Dictionary<(Station, Station), DecomposedConnection> decomposedConnections = new Dictionary<(Station, Station), DecomposedConnection>();
            foreach (Station station in targetStations)
            {
                foreach (Station target in targetStations)
                {
                    if (target == station) continue; // Skip the same station
                    ResetStations(stations);
                    AStar(station, target); // Run A* algorithm to find the shortest path from station to target
                    decomposedConnections.Add((station, target),
                        new DecomposedConnection(
                        station,
                        target,
                        target.gScore,
                        GetCost(target),
                        GetShortestPath(target))); // Store the connection in a dictionary for quick access
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
                while (connection.path.Count > 0)
                {
                    Console.WriteLine(connection.path.Pop());
                }
                Console.WriteLine($"\nSubpath estimated time: {connection.estimatedTime} minutes");
                Console.WriteLine(connection.cost == 0 ? "Special fares applied" : $"Cost of subpath: £{connection.cost}");
                Console.WriteLine(new string('-', Console.WindowWidth));
            }
            Console.WriteLine($"Total estimated time: {totalTime} minutes");
            Console.WriteLine($"Total cost: £{totalCost}");
        }
        static List<Station> AntColonyOptimisation(List<Station> targetStations, Dictionary<(Station, Station), DecomposedConnection> connections, int iterations)
        /* Reference: https://learn.microsoft.com/en-us/archive/msdn-magazine/2012/february/test-run-ant-colony-optimization ,
        https://www.youtube.com/watch?v=oXb2nC-e_EA ,
        https://medium.com/data-science/ant-colony-optimization-in-action-6d9106de60af */
        {
            int ants = 50;
            double alpha = 1;  // influence of pheromone
            double beta = 5;   // influence of heuristic
            double rho = 0.5;  // pheromone evaporation rate
            double Q = 100;    // pheromone deposit factor
            List<Station> bestPath = null;
            double bestLength = double.MaxValue;
            Dictionary<(Station, Station), double> pheromones = new Dictionary<(Station, Station), double>();
            Dictionary<(Station, Station), double> distance = new Dictionary<(Station, Station), double>();
            foreach ((Station, Station) key in connections.Keys)
            {
                pheromones[key] = 1; // Initialize pheromone levels
                distance[key] = connections[key].estimatedTime; // Use estimated time as distance
            }
            for (int iteration = 0; iteration < iterations; iteration++)
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
                        double rand = random.NextDouble() * sum; // Implicitly normalise probabilities
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
                    if (totalLength < bestLength) // Track best
                    {
                        bestLength = totalLength;
                        bestPath = new List<Station>(path);
                    }
                    allAntsPaths.Add((path, totalLength));
                }
                foreach ((Station, Station) key in pheromones.Keys.ToList())
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
        static string AutoCompleteInput(Dictionary<string, Station> stations)
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
                else if (key.Key == ConsoleKey.Tab)
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
        static Station GetStation(Dictionary<string, Station> stations)
        {
            while (true)
            {
                try
                {
                    Console.Write("Enter station: ");
                    return stations[AutoCompleteInput(stations)];
                }
                catch (KeyNotFoundException)
                {
                    Console.WriteLine("Station not found. Please try again.");
                }
            }
        }
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            Dictionary<string, Station> stations = LoadFromJson(filePath); // Load stations from JSON file
            List<Station> targetStations = new List<Station>();
            Console.Write("Enter number of stations to find path (2-10): ");
            int numOfStations = int.Parse(Console.ReadLine());
            for (int i = 0; i < numOfStations; i++)
            {
                targetStations.Add(GetStation(stations)); // Get target stations from user input
            }
            stopwatch.Start();
            Dictionary<(Station, Station), DecomposedConnection> connections = DecomposedPath(targetStations, stations);
            Console.WriteLine("Ant Colony Optimisation:");
            List<Station> acPath = AntColonyOptimisation(targetStations, connections, 5000);
            DisplayFullPath(acPath, connections);
            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadLine();
        }
    }
}