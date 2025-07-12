using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NEA
{
    internal class Program
    {
        public const string filePath = "underground.json"; // Path to the JSON file containing station data
        public static readonly double[,] zoneCost = { //row refers to end zone, column represents start zone. Source: https://londondrum.com/transport/adult-train-fares.php
            { 2.9, 3.5, 3.8, 4.6, 5.2, 5.8, 6.7, 8.2, 8.3 },
            { 0, 2.1, 2.3, 3, 3.2, 3.6, 4.9, 5.6, 8.3 },
            { 0, 0, 2.1, 2.3, 3, 3.2, 4, 4.8, 8.3 },
            { 0, 0, 0, 2.1, 2.3, 3, 3.2, 4, 4.1 },
            { 0, 0, 0, 0, 2.1, 2.3, 2.9, 3.2, 3.5 },
            { 0, 0, 0, 0, 0, 2.1, 2.2, 2.9, 3 },
            { 0, 0, 0, 0, 0, 0, 2, 2.2, 2.3 },
            { 0, 0, 0, 0, 0, 0, 0, 2, 2.2 }
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
        static bool IsLineChange(Station current, Connection connection) =>
current.previousStation != null && !current.getPrevConnectionLine().Contains(connection.line);
        static void Dijkstra(Station start)
        {
            PriorityQueue<Station> pq = new PriorityQueue<Station>();
            start.time2Travel = 0;
            pq.Enqueue(start, 0);
            while (!pq.IsEmpty())
            {
                Station current = pq.Dequeue();
                if (current.visited) continue;
                current.visited = true;
                foreach (Connection connection in current.Connections)
                {
                    int newTime = current.time2Travel + connection.time;
                    if (IsLineChange(current, connection) && !current.getPrevConnectionLine().Contains("Walk"))
                    {
                        newTime += 11; // Add transfer time if changing lines
                    }
                    if (newTime < connection.station.time2Travel)
                    {
                        connection.station.time2Travel = newTime;
                        connection.station.previousStation = current;
                        pq.Enqueue(connection.station, newTime);
                    }
                }
            }
        }
        static List<string> GetShortestPath(Station end) //backtrack the station from the destination
        {
            List<string> path = new List<string>();
            Station current = end;
            List<string> connectionLine = current.getPrevConnectionLine();
            string line;
            int stops = 0;
            while (true)
            {
                line = string.Join("/ ", connectionLine);
                for (int i = 0; i < connectionLine.Count; i++)
                {
                    if (connectionLine.Count == 0) break;
                    if (!current.getPrevConnectionLine().Contains(connectionLine[i]))
                    {
                        connectionLine.Remove(connectionLine[i]); // remove the line if it is not the previous connection line
                        i--;
                    }
                }
                if (current.previousStation == null)
                {
                    if (line == "Walk") // if walking, do not add the line
                        path.Add($"Walk from {current.name} to ");
                    else
                    {
                        path.Add($"Ride {stops} stop(s) via:\n");
                        path.Add($"Take the {line} Line from {current.name}\n"); // add the starting station
                    }
                    path.Reverse();
                    break;
                }
                if (connectionLine.Count == 0)
                {
                    if (line == "Walk") // if walking, do not add the line
                        path.Add($"\nWalk from {current.name} to ");
                    else
                    {
                        path.Add($"Ride {stops} stop(s) via:\n");
                        path.Add($"\nTransfer to {line} line in {current.name}\n");
                    }
                    connectionLine = current.getPrevConnectionLine(); // changes line
                    line = string.Join("/ ", connectionLine);
                    stops = 0;
                }
                path.Add(current.name + "\n");
                stops++;
                current = current.previousStation;
            }
            return path;
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
                ResetStations(stations);
                Dijkstra(station);
                foreach (Station target in targetStations)
                {
                    if (target == station) continue; // skip the same station
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
            int totalTime = 0;
            double totalCost = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                foreach (DecomposedConnection connection in connections)
                {
                    if (connection.startStation == path[i] && connection.endStation == path[i + 1])
                    {
                        totalTime += connection.estimatedTime;
                        totalCost += connection.cost;
                        Console.WriteLine($"Route from {path[i].name} to {path[i + 1].name}:");
                        connection.path.ForEach(Console.Write);
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
            int totalTime = 0;
            List<Station> visitedStation = new List<Station>();
            List<Station> path = new List<Station>();
            Station currentStation = targetStations[0];
            path.Add(currentStation);
            visitedStation.Add(currentStation);
            while (visitedStation.Count != targetStations.Count)
            {
                Station nearestStation = null;
                int minTime = int.MaxValue;
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
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Dictionary<string, Station> stations = StationLoader.LoadFromJson(filePath); // Load stations from JSON file
            List<Station> targetStations = new List<Station>();
            targetStations.Add(stations["Westminster"]);
            targetStations.Add(stations["Baker Street"]);
            targetStations.Add(stations["Aldgate"]);
            targetStations.Add(stations["Earl's Court"]);
            List<DecomposedConnection> connections = DecomposedPath(targetStations, stations);
            Console.WriteLine("Manually ordered:");
            DisplayFullPath(targetStations, connections);
            Console.WriteLine(new string('-', Console.WindowWidth));
            targetStations = NearestNeighbourTSP(targetStations, connections);
            Console.WriteLine("Nearest Neighbour TSP:");
            DisplayFullPath(targetStations, connections);
            stopwatch.Stop();
            Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadLine();
            convertToJSON(Zone1Prototype());
            Console.ReadLine();
        }

        //temp
        static void AddConnection(Station a, Station b, int time, string line)
        {
            a.Connections.Add(new Connection(b, time, line));
            b.Connections.Add(new Connection(a, time, line));
        }
        static void AddStation(Dictionary<string, Station> stations, string name, double zone) => stations.Add(name, new Station(name, zone));
        static Dictionary<string, Station> Zone1Prototype()
        {
            Dictionary<string, Station> stations = new Dictionary<string, Station>();
            AddStation(stations, "Shoreditch High Street", 1);
            AddStation(stations, "Abbey Road", 2.5);
            AddStation(stations, "All Saints", 2);
            AddStation(stations, "Archway", 2.5);
            AddStation(stations, "Arsenal", 2);
            AddStation(stations, "Barons Court", 2);
            AddStation(stations, "Battersea Park", 2);
            AddStation(stations, "Beksize Park", 2);
            AddStation(stations, "Bermondsey", 2);
            AddStation(stations, "Bethnal Green (Underground)", 2);
            AddStation(stations, "Bethnal Green (Overground)", 2);
            AddStation(stations, "Blackwall", 2);
            AddStation(stations, "Bow Church", 2);
            AddStation(stations, "Bow Road", 2);
            AddStation(stations, "Brixton", 2);
            AddStation(stations, "Brockley", 2);
            AddStation(stations, "Bromley-by-Bow", 2.5);
            AddStation(stations, "Brondesbury", 2);
            AddStation(stations, "Brondesbury Park", 2);
            AddStation(stations, "Caledonian Road", 2);
            AddStation(stations, "Caledonian Road and Barnsbury", 2);
            AddStation(stations, "Cambridge Heath", 2);
            AddStation(stations, "Camden Road", 2);
            AddStation(stations, "Camden Town", 2);
            AddStation(stations, "Canada Water", 2);
            AddStation(stations, "Canary Wharf", 2);
            AddStation(stations, "Canary Wharf (Jubilee and DLR)", 2);
            AddStation(stations, "Canary Wharf (Elizabeth)", 2);
            AddStation(stations, "Canning Town", 2.5);
            AddStation(stations, "Canonbury", 2);
            AddStation(stations, "Chalk Farm", 2);
            AddStation(stations, "Clapham Common", 2);
            AddStation(stations, "Clapham High Street", 2);
            AddStation(stations, "Clapham Junction", 2);
            AddStation(stations, "Clapham North", 2);
            AddStation(stations, "Clapham South", 2.5);
            AddStation(stations, "Clapton", 2.5);
            AddStation(stations, "Crossharbour", 2);
            AddStation(stations, "Cutty Sark", 2.5);
            AddStation(stations, "Dalston Junction", 2);
            AddStation(stations, "Dalston Kingsland", 2);
            AddStation(stations, "Denmark Hill", 2);
            AddStation(stations, "Deptford Bridge", 2.5);
            AddStation(stations, "Devons Road", 2);
            AddStation(stations, "East Acton", 2);
            AddStation(stations, "East India", 2.5);
            AddStation(stations, "East Putney", 2.5);
            AddStation(stations, "Elverson Road", 2.5);
            AddStation(stations, "Finchley Road", 2);
            AddStation(stations, "Finchley Road & Frognal", 2);
            AddStation(stations, "Finsbury Park", 2);
            AddStation(stations, "Fulham Broadway", 2);
            AddStation(stations, "Goldhawk Road", 2);
            AddStation(stations, "Gospel Oak", 2);
            AddStation(stations, "Greenwich", 2.5);
            AddStation(stations, "Hackney Central", 2);
            AddStation(stations, "Hackney Downs", 2);
            AddStation(stations, "Hackney Wick", 2);
            AddStation(stations, "Haggerston", 2);
            AddStation(stations, "Hammersmith", 2);
            AddStation(stations, "Hammersmith (Circle and Hammersmith & City)", 2);
            AddStation(stations, "Hammersmith (Piccadilly and District)", 2);
            AddStation(stations, "Hampstead", 2.5);
            AddStation(stations, "Hampstead Heath", 2);
            AddStation(stations, "Heron Quays", 2);
            AddStation(stations, "Highbury & Islington", 2);
            AddStation(stations, "Holland Park", 2);
            AddStation(stations, "Holloway Road", 2);
            AddStation(stations, "Homerton", 2);
            AddStation(stations, "Hoxton", 1.5);
            AddStation(stations, "Imperial Wharf", 2);
            AddStation(stations, "Island Garders", 2);
            AddStation(stations, "Kensal Green", 2);
            AddStation(stations, "Kensal Rise", 2);
            AddStation(stations, "Kensington (Olympia)", 2);
            AddStation(stations, "Kentish Town", 2);
            AddStation(stations, "Kentish Town West", 2);
            AddStation(stations, "Kilburn", 2);
            AddStation(stations, "Kilburn High Road", 2);
            AddStation(stations, "Kilburn Park", 2);
            AddStation(stations, "Ladbroke Grove", 2);
            AddStation(stations, "Langdon Park", 2);
            AddStation(stations, "Latimer Road", 2);
            AddStation(stations, "Lewisham", 2);
            AddStation(stations, "Limehouse", 2);
            AddStation(stations, "London Fields", 2);
            AddStation(stations, "Malda Vale", 2);
            AddStation(stations, "Manor House", 2.5);
            AddStation(stations, "Mile End ", 2);
            AddStation(stations, "Mornington Crescent", 2.5);
            AddStation(stations, "Mudchute", 2);
            AddStation(stations, "New Cross", 2);
            AddStation(stations, "New Cross Gate", 2);
            AddStation(stations, "North Acton", 2.5);
            AddStation(stations, "North Greenwich", 2.5);
            AddStation(stations, "Oval", 2);
            AddStation(stations, "Parsons Green", 2);
            AddStation(stations, "Poplar", 2);
            AddStation(stations, "Pudding Mill Lane", 2.5);
            AddStation(stations, "Putney Bridge", 2);
            AddStation(stations, "Queens Park", 2);
            AddStation(stations, "Ravenscourt Park", 2);
            AddStation(stations, "Rectory Road", 2);
            AddStation(stations, "Rotherhithe", 2.5);
            AddStation(stations, "Royal Oak", 2);
            AddStation(stations, "Shadwell", 2.5);
            AddStation(stations, "Shadwell (DLR)", 2);
            AddStation(stations, "Shadwell (Overground)", 2);
            AddStation(stations, "Shepherd's Bush", 2);
            AddStation(stations, "Shepherd's Bush (Central)", 2);
            AddStation(stations, "Shepherd's Bush (Overground)", 2);
            AddStation(stations, "Shepherd's Bush Market", 2);
            AddStation(stations, "South Hampstead", 2);
            AddStation(stations, "South Quay", 2);
            AddStation(stations, "St John's Wood", 2);
            AddStation(stations, "Stamford Brook", 2);
            AddStation(stations, "Star Lane", 2.5);
            AddStation(stations, "Stepney Green", 2);
            AddStation(stations, "Stockwell", 2);
            AddStation(stations, "Stratford", 2.5);
            AddStation(stations, "Stratford High Street", 2.5);
            AddStation(stations, "Stratford International", 2.5);
            AddStation(stations, "Surrey Quays", 2);
            AddStation(stations, "Swiss Cottage", 2);
            AddStation(stations, "Tufnell Park", 2);
            AddStation(stations, "Turnham Green", 2.5);
            AddStation(stations, "Upper Holloway", 2);
            AddStation(stations, "Wandsworth Road", 2);
            AddStation(stations, "Wapping", 2);
            AddStation(stations, "Warwick Avenue", 2);
            AddStation(stations, "West Brompton", 2);
            AddStation(stations, "West Ham", 2.5);
            AddStation(stations, "West Hampstead", 2);
            AddStation(stations, "West Hampstead (Jubilee)", 2);
            AddStation(stations, "West Hampstead (Overground)", 2);
            AddStation(stations, "West India Quay", 2);
            AddStation(stations, "West Kensington", 2);
            AddStation(stations, "Westbourne Park", 1);
            AddStation(stations, "Westferry", 2);
            AddStation(stations, "Whitechapel", 2);
            AddStation(stations, "White City", 2);
            AddStation(stations, "Willesden Green", 2.5);
            AddStation(stations, "Willesden Junction", 2.5);
            AddStation(stations, "Wood Lane", 2);
            return stations;
        }
        static void convertToJSON(Dictionary<string, Station> stations)
        {
            List<StationJson> stationJsonList = new List<StationJson>();
            foreach (var station in stations.Values)
            {
                StationJson stationJson = new StationJson
                {
                    name = station.name,
                    zone = (float)station.zone,
                    connections = station.Connections.Select(c => new ConnectionJson
                    {
                        station = c.station.name,
                        time = c.time,
                        line = c.line
                    }).ToList()
                };
                stationJsonList.Add(stationJson);
            }
            string json = JsonSerializer.Serialize(stationJsonList, new JsonSerializerOptions { WriteIndented = true });
            Console.WriteLine(json);
        }
    }

}

