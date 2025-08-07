using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.SqlServer.Server;

namespace NEA
{
    internal class Program
    {
        public const string filePath = "underground.json"; // Path to the JSON file containing station data
        public static readonly double[,] zoneCost = { //row refers to end zone, column represents start zone. Source: https://londondrum.com/transport/adult-train-fares.php
            { 2.9, 3.5, 3.8, 4.6, 5.2, 5.8, 6.7, 8.2, 8.3, 12.8 },
            { 0, 2.1, 2.3, 3, 3.2, 3.6, 4.9, 5.6, 8.3, 7.6 },
            { 0, 0, 2.1, 2.3, 3, 3.2, 4, 4.8, 8.3, 7.5 },
            { 0, 0, 0, 2.1, 2.3, 3, 3.2, 4, 4.1, 7.4 },
            { 0, 0, 0, 0, 2.1, 2.3, 2.9, 3.2, 3.5, 7.3 },
            { 0, 0, 0, 0, 0, 2.1, 2.2, 2.9, 3, 0 },
            { 0, 0, 0, 0, 0, 0, 2, 2.2, 2.3, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 2, 2.2, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
        };
        static void ResetStations(Dictionary<string, Station> stations)
        {
            foreach (Station station in stations.Values)
            {
                station.time2Travel = int.MaxValue;
                station.previousStation = null;
                station.visited = false;
            }
        }
        static bool IsLineChange(Station current, Connection connection)
        {
            if (current.previousStation == null)
                return false;
            return current.previousLine != connection.line;
        }
        static double Heuristic(Station from, Station to)
            => Math.Abs(from.zone - to.zone);
        static void AStar(Station start, Station goal)
        {
            PriorityQueue<Station> openSet = new PriorityQueue<Station>();
            start.time2Travel = 0;
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
                    double gScore = current.time2Travel + connection.time;
                    if (IsLineChange(current, connection) && !current.getPrevConnectionLine().Contains("Walk"))
                    {
                        gScore += 13;
                    }
                    if (gScore < connection.station.time2Travel)
                    {
                        neighbor.time2Travel = gScore;
                        neighbor.previousStation = current;
                        connection.station.previousLine = connection.line;
                        openSet.Enqueue(neighbor, gScore + Heuristic(neighbor, goal));
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
            if (startStation)
                path.Push($"Start from {stationName}");
            else
                path.Push($"\nTransfer at {stationName}");
        }
        static double GetCost(Station End) //calculate cost based on Zone number
        {
            double minZone = double.MaxValue;
            double maxZone = -1;
            for (Station current = End; current != null; current = current.previousStation)
            {
                if (Math.Ceiling(current.zone) < minZone) minZone = current.zone; //there are some station which is between 2 zones, represented by a decimal number.
                                                                                  //e.g. 1.5 represents between station between zone 1 and 2
                if (Math.Floor(current.zone) > maxZone) maxZone = current.zone;
            }
            return zoneCost[(int)minZone - 1, (int)maxZone - 1];
        }
        static List<DecomposedConnection> DecomposedPath(List<Station> targetStations, Dictionary<string, Station> stations) //Decompose the full graph into K(n)
        {
            List<DecomposedConnection> decomposedConnections = new List<DecomposedConnection>();
            foreach (Station station in targetStations)
            {
                foreach (Station target in targetStations)
                {
                    if (target == station) continue; // skip the same station
                    ResetStations(stations);
                    AStar(station, target); // Run A* algorithm to find the shortest path from station to target
                    decomposedConnections.Add(new DecomposedConnection(
                        station,
                        target,
                        target.time2Travel,
                        GetCost(target),
                        GetShortestPath(target)));
                }
            }
            return decomposedConnections;
        }
        static void DisplayFullPath(List<Station> path, List<DecomposedConnection> connections)
        {
            double totalTime = 0;
            double totalCost = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                foreach (DecomposedConnection connection in connections)
                {
                    if (connection.startStation == path[i] && connection.endStation == path[i + 1])
                    {
                        totalTime += connection.estimatedTime;
                        totalCost += connection.cost;
                        Console.WriteLine(new string('-', Console.WindowWidth));
                        Console.WriteLine($"Route from {path[i].name} to {path[i + 1].name}:");
                        while (connection.path.Count > 0)
                        {
                            Console.WriteLine(connection.path.Pop());
                        }
                        Console.WriteLine($"\nSubpath estimated time: {connection.estimatedTime} minutes");
                        Console.WriteLine($"Cost of subpath: £{connection.cost}");
                        Console.WriteLine(new string('-', Console.WindowWidth));
                        break;
                    }
                }
            }
            Console.WriteLine($"Total estimated time: {totalTime} minutes");
            Console.WriteLine($"Total cost: £{totalCost}");
        }
        static List<Station> NearestNeighbourTSP(List<Station> targetStations, List<DecomposedConnection> decomposedConnections)
        {
            double totalTime = 0;
            List<Station> visitedStation = new List<Station>();
            List<Station> path = new List<Station>();
            Station currentStation = targetStations[0];
            path.Add(currentStation);
            visitedStation.Add(currentStation);
            while (visitedStation.Count != targetStations.Count)
            {
                Station nearestStation = null;
                double minTime = int.MaxValue;
                foreach (Station station in targetStations)
                {
                    if (visitedStation.Contains(station)) continue; // skip visited stations
                    foreach (DecomposedConnection connection in decomposedConnections)
                    {
                        if (connection.startStation == currentStation && connection.estimatedTime < minTime && !visitedStation.Contains(connection.endStation))
                        {
                            nearestStation = connection.endStation;
                            minTime = connection.estimatedTime;
                        }
                    }
                }
                totalTime += minTime;
                path.Add(nearestStation);
                visitedStation.Add(nearestStation);
                currentStation = nearestStation;
            }
            return path;
        }
        static string AutoCompleteInput(Dictionary<string, Station> stations)
        {
            string input = "";
            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;
            string match = "";
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
                // Reprint input + suggestion
                if (match != null && match.Length > input.Length)
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
            Dictionary<string, Station> stations = StationLoader.LoadFromJson(filePath); // Load stations from JSON file
            List<Station> targetStations = new List<Station>();
            targetStations.Add(GetStation(stations));
            targetStations.Add(GetStation(stations));
            stopwatch.Start();
            List<DecomposedConnection> connections = DecomposedPath(targetStations, stations);
            Console.WriteLine("Manually ordered:");
            DisplayFullPath(targetStations, connections);
            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadLine();
        }
    }
}

