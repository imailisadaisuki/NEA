using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NEA
{
    internal class AntColonyOptimiser
    {
        private static Random random = new Random();
        private readonly Dictionary<string, Station> stations;
        private readonly SettingsJSON settings;
        private readonly double[][] zoneCost;
        public AntColonyOptimiser(Dictionary<string, Station> stations, SettingsJSON settings, double[][] zoneCost) => (this.stations, this.settings, this.zoneCost) = (stations, settings, zoneCost);

        private bool IsLineChange(Station current, Connection connection)
            => !current.previousLines.Contains(connection.line) || connection.line == "Walk";

        private void Dijkstra(Station start)
        {
            PriorityQueue<Station> pq = new PriorityQueue<Station>();
            start.travelTime = 0;
            start.previousLines = new HashSet<string>(start.Connections.Select(c => c.line)); // Initialise previous lines for the start station
            pq.Enqueue(start, 0);
            double transferPenalty = settings.general.transferPenalty;
            bool transferred; // Flag to check if a transfer has occurred
            while (!pq.IsEmpty())
            {
                
                Station current = pq.Dequeue();
                if (current.visited) continue; // Skip if already visited
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
                        if (connection.line == "Walk") newTime -= transferPenalty; // As transfer penality of "Walk" will be counted twice
                        neighbour.travelTime = newTime;
                        neighbour.previousStation = current;
                        if (transferred) // If a transfer occurred, reset previous lines to the current station's lines
                        {
                            neighbour.previousLines = new HashSet<string>(current.Connections
                                .Where(c => c.station == neighbour)
                                .Select(c => c.line));
                        }
                        else
                        {
                            neighbour.previousLines = new HashSet<string>(current.previousLines); // Copy previous lines from the current station
                            neighbour.previousLines.IntersectWith(new HashSet<string>( // Intersect with current lines to keep only the lines that connect to the neighbour
                                current.Connections
                                .Where(c => c.station == neighbour)
                                .Select(c => c.line)));
                        }
                        pq.Enqueue(neighbour, newTime);
                    }
                }
            }
        }

        private List<PathInstruction> GetShortestPath(Station end) // I avoided using stacks as paths need to be read repeatively, stack cannot achieve this function (when poped, the last element is removed)
        {
            List<PathInstruction> path = new List<PathInstruction>();
            Station current = end;
            int stopCount = 0;
            Station prev = current.previousStation;
            HashSet<string> connectionLines = current.getPrevConnectionLine();
            while (prev != null)
            {
                HashSet<string> currentLines = new HashSet<string>(connectionLines); // Clone a new set to avoid modifying the original set
                HashSet<string> prevConnectionLines = current.getPrevConnectionLine();
                connectionLines.IntersectWith(prevConnectionLines); // Remove lines that are not connecting from the previous station to the current station
                if (connectionLines.Count == 0) // If no lines connecting from one station to current station, a transfer occurred
                {
                    AddTransfer(path, current.name, currentLines, stopCount, false);
                    connectionLines = new HashSet<string>(prevConnectionLines);
                    stopCount = 0;
                }
                path.Add(new PathInstruction(current.name));
                stopCount++;
                current = prev;
                prev = current.previousStation;
            }
            AddTransfer(path, current.name, connectionLines, stopCount, true); // Add starting segment
            path.Reverse(); // Reverse the path to start from the beginning
            return path;
        }

        private void AddTransfer(List<PathInstruction> path, string stationName, HashSet<string> lines, int stopCount, bool startStation)
        {
            path.Add(new PathInstruction($"\nRide {stopCount} stop(s) via:"));
            string platformInfo = $"Platform number(s): {string.Join(", ", stations[stationName].getPlatformNumber(lines))}";
            path.Add(new PathInstruction(platformInfo));
            foreach (string line in lines)
                path.Add(new PathInstruction(line, true));
            path.Add(new PathInstruction("Possible line(s):"));
            path.Add(new PathInstruction(startStation ? $"Start from {stationName}" : $"\nTransfer at {stationName}"));
        }

        private double GetCost(Station End) // Calculate cost based on zone number
        {
            double minZone = double.MaxValue;
            double maxZone = -1;
            for (Station current = End; current != null; current = current.previousStation)
            {
                if (current.zone < minZone) minZone = current.zone; // There are some stations which are located between 2 zones, represented by a decimal number.
                if (current.zone > maxZone) maxZone = current.zone; // e.g. 1.5 represents between stations between zone 1 and 2
            }
            return zoneCost[(int)Math.Ceiling(minZone) - 1][(int)Math.Floor(maxZone) - (int)Math.Ceiling(minZone)];
        }

        public Dictionary<(Station, Station), DecomposedConnection> DecomposedPath(List<Station> targetStations) // Decompose the full graph into K(n)
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

        public List<PathInstruction> GetFullPath(List<Station> path, Dictionary<(Station, Station), DecomposedConnection> connections)
        {
            double totalTime = 0;
            double totalCost = 0;
            List<PathInstruction> fullInstructions = new List<PathInstruction>();
            for (int i = 0; i < path.Count - 1; i++)
            {
                DecomposedConnection connection = connections[(path[i], path[i + 1])]; // Get the connection between the current and next station
                totalTime += connection.estimatedTime;
                fullInstructions.Add(new PathInstruction(new string('-', Console.WindowWidth)));
                fullInstructions.Add(new PathInstruction($"Route from {path[i].name} to {path[i + 1].name}:"));
                foreach (PathInstruction segment in connection.path)
                    fullInstructions.Add(segment);
                fullInstructions.Add(new PathInstruction($"\nSubpath estimated time: {Math.Round(connection.estimatedTime)} minutes"));
                if (connection.cost == 0)
                {
                    fullInstructions.Add(new PathInstruction("Special fares applied"));
                }
                else
                {
                    totalCost += connection.cost;
                    fullInstructions.Add(new PathInstruction($"Cost of subpath: {connection.cost:C2}"));
                }
                fullInstructions.Add(new PathInstruction(new string('-', Console.WindowWidth)));
            }
            fullInstructions.Add(new PathInstruction($"Total estimated time: {Math.Round(totalTime)} minutes"));
            fullInstructions.Add(new PathInstruction($"Total cost excluding special fares: {totalCost:C2}"));
            return fullInstructions;

        }

        private void ConstructPath(Dictionary<(Station, Station), double> pheromones, Dictionary<(Station, Station), double> distance, Dictionary<(Station, Station), DecomposedConnection> connections, bool timeOrCost)
        {
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
                    distance[key] = 0.8 * connections[key].cost + 0.2 * connections[key].estimatedTime; // Add a small weight to time to avoid route with 0 cost & choose a faster route when costs are the same
            }
        }

        private Station GetNextStation(List<Station> unvisited, Dictionary<(Station, Station), double> pheromones, Station current, Dictionary<(Station, Station), double> distance, double alpha, double beta)
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
            foreach (KeyValuePair<Station, double> kvp in probabilities)
            {
                cumulativeProbability += kvp.Value;
                if (rand < cumulativeProbability)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        private void UpdatePheromones(Dictionary<(Station, Station), DecomposedConnection> connections, List<(List<Station> path, double length)> allAntsPaths, Dictionary<(Station, Station), double> pheromones, double rho, double Q)
        {
            foreach ((Station, Station) key in connections.Keys)
                pheromones[key] *= 1 - rho; // Evaporate pheromones
            foreach ((List<Station> antPath, double length) in allAntsPaths)
            {
                double pheromoneDeposit = Q / length; // Deposit pheromones based on path length
                for (int i = 0; i < antPath.Count - 1; i++)
                    pheromones[(antPath[i], antPath[i + 1])] += pheromoneDeposit; // Update pheromones for each segment of the path
            }
        }

        public List<Station> AntColonyOptimisation(List<Station> targetStations, Dictionary<(Station, Station), DecomposedConnection> connections, bool timeOrCost)
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
            bool returnStart = settings.general.returnStart;
            List<Station> bestPath = null;
            double bestLength = double.MaxValue;
            Dictionary<(Station, Station), double> pheromones = new Dictionary<(Station, Station), double>();
            Dictionary<(Station, Station), double> distance = new Dictionary<(Station, Station), double>();
            ConstructPath(pheromones, distance, connections, timeOrCost);
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
                        Station chosen = GetNextStation(unvisited, pheromones, current, distance, alpha, beta);
                        path.Add(chosen);
                        unvisited.Remove(chosen);
                        totalLength += distance[(current, chosen)]; // Add the distance to the path length
                        current = chosen; // Move to the next station
                    }
                    if (returnStart)
                    {
                        path.Add(targetStations[0]);
                        totalLength += distance[(current, targetStations[0])];
                    }
                    if (totalLength < bestLength) // Track best path
                    {
                        bestLength = totalLength;
                        bestPath = new List<Station>(path);
                    }
                    allAntsPaths.Add((path, totalLength));
                }
                UpdatePheromones(connections, allAntsPaths, pheromones, rho, Q);
            }
            return bestPath;
        }
    }
}
