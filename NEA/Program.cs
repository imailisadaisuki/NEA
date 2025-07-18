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
                    double newTime = current.time2Travel + connection.time;
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
        static void AddStation(Dictionary<string, Station> stations, string name, double zone) => stations.Add(name, new Station(name, zone));
        static Dictionary<string, Station> Zone1Prototype()
        {
            Dictionary<string, Station> stations = new Dictionary<string, Station>();
            AddStation(stations, "Aldgate", 1);
            AddStation(stations, "Aldgate East", 1);
            AddStation(stations, "Angel", 1);
            AddStation(stations, "Baker Street", 1);
            AddStation(stations, "Bank", 1);
            AddStation(stations, "Barbican", 1);
            AddStation(stations, "Battersea Power Station", 1);
            AddStation(stations, "Bayswater", 1);
            AddStation(stations, "Blackfriars", 1);
            AddStation(stations, "Bond Street", 1);
            AddStation(stations, "Borough", 1);
            AddStation(stations, "Cannon Street", 1);
            AddStation(stations, "Chancery Lane", 1);
            AddStation(stations, "Charing Cross", 1);
            AddStation(stations, "Covent Garden", 1);
            AddStation(stations, "Earl's Court", 1.5);
            AddStation(stations, "Edgware Road (Bakerloo)", 1);
            AddStation(stations, "Edgware Road (Circle and Hammersmith & City)", 1);
            AddStation(stations, "Edgware Road (Circle Terminal)", 1);
            AddStation(stations, "Elephant & Castle", 1.5); 
            AddStation(stations, "Embankment", 1);
            AddStation(stations, "Euston (Underground)", 1);
            AddStation(stations, "Euston Square", 1);
            AddStation(stations, "Farringdon", 1);
            AddStation(stations, "Gloucester Road", 1);
            AddStation(stations, "Goodge Street", 1);
            AddStation(stations, "Great Portland Street", 1);
            AddStation(stations, "Green Park", 1);
            AddStation(stations, "High Street Kensington", 1);
            AddStation(stations, "Holborn", 1);
            AddStation(stations, "Hyde Park Corner", 1);
            AddStation(stations, "Kennington", 1.5);
            AddStation(stations, "King's Cross & St Pancras International", 1);
            AddStation(stations, "Knightsbridge", 1);
            AddStation(stations, "Lambeth North", 1);
            AddStation(stations, "Lancaster Gate", 1);
            AddStation(stations, "Leicester Square", 1);
            AddStation(stations, "Liverpool Street", 1);
            AddStation(stations, "London Bridge", 1);
            AddStation(stations, "Mansion House", 1);
            AddStation(stations, "Marble Arch", 1);
            AddStation(stations, "Marylebone", 1);
            AddStation(stations, "Monument", 1);
            AddStation(stations, "Moorgate", 1);
            AddStation(stations, "Nine Elms", 1);
            AddStation(stations, "Notting Hill Gate", 1.5);
            AddStation(stations, "Old Street", 1);
            AddStation(stations, "Oxford Circus", 1);
            AddStation(stations, "Paddington (Bakerloo, Circle and District)", 1);
            AddStation(stations, "Paddington (Circle and Hammersmith & City)", 1);
            AddStation(stations, "Piccadilly Circus", 1);
            AddStation(stations, "Pimlico", 1);
            AddStation(stations, "Queensway", 1);
            AddStation(stations, "Regent's Park", 1);
            AddStation(stations, "Russell Square", 1);
            AddStation(stations, "St. Paul's", 1);
            AddStation(stations, "South Kensington", 1);
            AddStation(stations, "Southwark", 1);
            AddStation(stations, "St. James's Park", 1);
            AddStation(stations, "Sloane Square", 1);
            AddStation(stations, "Temple", 1);
            AddStation(stations, "Tottenham Court Road", 1);
            AddStation(stations, "Tower Hill", 1);
            AddStation(stations, "Tower Gateway", 1);
            AddStation(stations, "Vauxhall", 1.5);
            AddStation(stations, "Victoria", 1);
            AddStation(stations, "Warren Street", 1);
            AddStation(stations, "Waterloo", 1);
            AddStation(stations, "Westminster", 1);
            AddStation(stations, "Paddington (Elizabeth)", 1);
            AddStation(stations, "Bond Street (Elizabeth)", 1);
            AddStation(stations, "Tottenham Court Road (Elizabeth)", 1);
            AddStation(stations, "Farringdon (Elizabeth)", 1);
            AddStation(stations, "Liverpool Street (Elizabeth)", 1);
            AddStation(stations, "Euston (Overground)", 1);
            AddStation(stations, "Liverpool Street (Overground)", 1);

            AddStation(stations, "Shoreditch High Street", 1);
            AddStation(stations, "Abbey Road", 2.5);
            AddStation(stations, "All Saints", 2);
            AddStation(stations, "Archway", 2.5);
            AddStation(stations, "Arsenal", 2);
            AddStation(stations, "Barons Court", 2);
            AddStation(stations, "Battersea Park", 2);
            AddStation(stations, "Beksize Park", 2);
            AddStation(stations, "Bermondsey", 2);
            AddStation(stations, "Bethnal Green", 2);
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
            AddStation(stations, "Maida Vale", 2);
            AddStation(stations, "Manor House", 2.5);
            AddStation(stations, "Mile End", 2);
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
            AddStation(stations, "Queen's Park", 2);
            AddStation(stations, "Ravenscourt Park", 2);
            AddStation(stations, "Rectory Road", 2);
            AddStation(stations, "Rotherhithe", 2.5);
            AddStation(stations, "Royal Oak", 2);
            AddStation(stations, "Shadwell", 2);
            AddStation(stations, "Shadwell (Overground)", 2);
            AddStation(stations, "Shepherd's Bush", 2);
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

            AddStation(stations, "Acton Central", 3);
            AddStation(stations, "Acton Main Line", 3);
            AddStation(stations, "Acton Town", 3);
            AddStation(stations, "Balham", 3);
            AddStation(stations, "Beckton", 3);
            AddStation(stations, "Beckton Park", 3);
            AddStation(stations, "Blackhorse Road", 3);
            AddStation(stations, "Bounds Green", 3.5);
            AddStation(stations, "Brent Cross", 3);
            AddStation(stations, "Bruce Grove", 3);
            AddStation(stations, "Chiswick Park", 3);
            AddStation(stations, "Colliers Wood", 3);
            AddStation(stations, "Crouch Hill", 3);
            AddStation(stations, "Crystal Palace", 3.5);
            AddStation(stations, "Custom House", 3);
            AddStation(stations, "Cyprus", 3);
            AddStation(stations, "Dollis Hill", 3);
            AddStation(stations, "Ealing Broadway", 3);
            AddStation(stations, "Ealing Common", 3);
            AddStation(stations, "East Finchley", 3);
            AddStation(stations, "East Ham", 3.5);
            AddStation(stations, "Forest Gate", 3);
            AddStation(stations, "Forest Hill", 3);
            AddStation(stations, "Gallions Reach", 3);
            AddStation(stations, "Golders Green", 3);
            AddStation(stations, "Gunnersbury", 3);
            AddStation(stations, "Hanger Lane", 3);
            AddStation(stations, "Harlesden", 3);
            AddStation(stations, "Harringay Green Lanes", 3);
            AddStation(stations, "Hendon Central", 3.5);
            AddStation(stations, "Highgate", 3);
            AddStation(stations, "Honor Oak Park", 3);
            AddStation(stations, "Kew Gardens", 3.5);
            AddStation(stations, "King George V", 3);
            AddStation(stations, "Leyton", 3);
            AddStation(stations, "Leyton Midland Road", 3);
            AddStation(stations, "Leytonstone", 3.5);
            AddStation(stations, "Leytonstone High Road", 3);
            AddStation(stations, "London City Airport", 3);
            AddStation(stations, "Manor Park", 3.5);
            AddStation(stations, "Maryland", 3);
            AddStation(stations, "Neasden", 3);
            AddStation(stations, "North Ealing", 3);
            AddStation(stations, "Northfields", 3);
            AddStation(stations, "Park Royal", 3);
            AddStation(stations, "Plaistow", 3);
            AddStation(stations, "Pontoon Dock", 3);
            AddStation(stations, "Prince Regent", 3);
            AddStation(stations, "Royal Albert", 3);
            AddStation(stations, "Royal Victoria", 3);
            AddStation(stations, "Seven Sisters", 3);
            AddStation(stations, "South Acton", 3);
            AddStation(stations, "South Ealing", 3);
            AddStation(stations, "South Tottenham", 3);
            AddStation(stations, "South Wimbledon", 3.5);
            AddStation(stations, "Southfields", 3);
            AddStation(stations, "St James Street", 3);
            AddStation(stations, "Stratford Hill", 3);
            AddStation(stations, "Stonebridge Park", 3);
            AddStation(stations, "Sydenham", 3);
            AddStation(stations, "Tooting Bec", 3);
            AddStation(stations, "Tooting Broadway", 3);
            AddStation(stations, "Tottenham Hale", 3);
            AddStation(stations, "Turnpike Lane", 3);
            AddStation(stations, "Upton Park", 3);
            AddStation(stations, "Walthamstow Central", 3);
            AddStation(stations, "Walthamstow Queen's Road", 3);
            AddStation(stations, "Wanstead Park", 3);
            AddStation(stations, "West Acton", 3);
            AddStation(stations, "West Ealing", 3);
            AddStation(stations, "West Silvertown", 3);
            AddStation(stations, "White Hart Lane", 3);
            AddStation(stations, "Wimbledon Park", 3);
            AddStation(stations, "Woodgreen", 3);
            AddStation(stations, "Woodgrange Park", 3.5);

            AddStation(stations, "Abbey Wood", 4);
            AddStation(stations, "Alperton", 4);
            AddStation(stations, "Arnos Grove", 4);
            AddStation(stations, "Barking Riverside", 4);
            AddStation(stations, "Barkingside", 4);
            AddStation(stations, "Boston Manor", 4);
            AddStation(stations, "Burnt Oak", 4);
            AddStation(stations, "Chigwell", 4);
            AddStation(stations, "Colindale", 4);
            AddStation(stations, "Edmonton Green", 4);
            AddStation(stations, "Fairlop", 4);
            AddStation(stations, "Falconwood", 4);
            AddStation(stations, "Finchley Central", 4);
            AddStation(stations, "Gants Hill", 4);
            AddStation(stations, "Goodmayes", 4);
            AddStation(stations, "Grange Hill", 4);
            AddStation(stations, "Greenford", 4);
            AddStation(stations, "Hainault", 4);
            AddStation(stations, "Highams Park", 4);
            AddStation(stations, "Hounslow Central", 4);
            AddStation(stations, "Hounslow East", 4);
            AddStation(stations, "Ilford", 4);
            AddStation(stations, "Kenton", 4);
            AddStation(stations, "Kingsbury", 4);
            AddStation(stations, "Mill Hill East", 4);
            AddStation(stations, "Morden", 4);
            AddStation(stations, "Newbury Park", 4);
            AddStation(stations, "North Wembley", 4);
            AddStation(stations, "Northwick Park", 4);
            AddStation(stations, "Norwood Junction", 4);
            AddStation(stations, "Osterley", 4);
            AddStation(stations, "Penge West", 4);
            AddStation(stations, "Perivale", 4);
            AddStation(stations, "Preston Road", 4);
            AddStation(stations, "Queensbury", 4);
            AddStation(stations, "Ravensbourne", 4);
            AddStation(stations, "Redbridge", 4);
            AddStation(stations, "Richmond", 4);
            AddStation(stations, "Roding Valley", 4);
            AddStation(stations, "Seven Kings", 4);
            AddStation(stations, "Silver Street", 4);
            AddStation(stations, "Snaresbrook", 4);
            AddStation(stations, "South Greenford", 4);
            AddStation(stations, "South Kenton", 4);
            AddStation(stations, "South Woodford", 4);
            AddStation(stations, "Southall", 4);
            AddStation(stations, "Southgate", 4);
            AddStation(stations, "Sudbury Hill", 4);
            AddStation(stations, "Sudbury Town", 4);
            AddStation(stations, "Totteridge & Whetstone", 4);
            AddStation(stations, "Upney", 4);
            AddStation(stations, "Wanstead", 4);
            AddStation(stations, "Wembley Central", 4);
            AddStation(stations, "Wembley Park", 4);
            AddStation(stations, "West Finchley", 4);
            AddStation(stations, "Wood Street", 4);
            AddStation(stations, "Woodford", 4);
            AddStation(stations, "Woodside Park", 4);
            AddStation(stations, "Woolwich", 4);
            AddStation(stations, "Woolwich Arsenal", 4);

            AddStation(stations, "Becontree", 5);
            AddStation(stations, "Buckhurst Hill", 5);
            AddStation(stations, "Busg Hill Park", 5);
            AddStation(stations, "Chadwell Heath", 5);
            AddStation(stations, "Chingford", 5);
            AddStation(stations, "Cockfosters", 5);
            AddStation(stations, "Dagenham East", 5);
            AddStation(stations, "Dagenham Heathway", 5);
            AddStation(stations, "Eastcote", 5);
            AddStation(stations, "Edgware", 5);
            AddStation(stations, "Enfield Town", 5);
            AddStation(stations, "Harrow & Wealdstone", 5);
            AddStation(stations, "Harrow-on-the-Hill", 5);
            AddStation(stations, "Hatton Cross", 5.5);
            AddStation(stations, "Hayes & Harlington", 5);
            AddStation(stations, "Headstone Lane", 5);
            AddStation(stations, "High Barnet", 5);
            AddStation(stations, "Hounslow West", 5);
            AddStation(stations, "North Harrow", 5);
            AddStation(stations, "Northolt", 5);
            AddStation(stations, "Oakwood", 5);
            AddStation(stations, "Pinner", 5);
            AddStation(stations, "Rayners Lane", 5);
            AddStation(stations, "Ruislip Gardens", 5);
            AddStation(stations, "South Harrow", 5);
            AddStation(stations, "South Ruislip", 5);
            AddStation(stations, "Southbury", 5);
            AddStation(stations, "Stanmore", 5);
            AddStation(stations, "West Croydon", 5);
            AddStation(stations, "West Harrow", 5);

            AddStation(stations, "Debden", 6);
            AddStation(stations, "Elm Park", 6);
            AddStation(stations, "Emerson Park", 6);
            AddStation(stations, "Epping", 6);
            AddStation(stations, "Gidea Park", 6);
            AddStation(stations, "Harold Wood", 6);
            AddStation(stations, "Hatch End", 6);
            AddStation(stations, "Heathrow Terminals 2 & 3 (Elizabeth)", 10); //Special fares applied
            AddStation(stations, "Heathrow Terminals 2 & 3 (Piccadilly)", 6);
            AddStation(stations, "Heathrow Terminal 4 (Elizabeth)", 10); //Special fares applied
            AddStation(stations, "Heathrow Terminal 4 (Piccadilly)", 6);
            AddStation(stations, "Heathrow Terminal 5 (Elizabeth)", 10); //Special fares applied
            AddStation(stations, "Heathrow Terminal 5 (Piccadilly)", 6);
            AddStation(stations, "Hillingdon", 6);
            AddStation(stations, "Hornchurch", 6);
            AddStation(stations, "Ickenham", 6);
            AddStation(stations, "Loughton", 6);
            AddStation(stations, "Moor Park", 6);
            AddStation(stations, "Northwood", 6);
            AddStation(stations, "Northwood Hills", 6);
            AddStation(stations, "Ruislip", 6);
            AddStation(stations, "Ruislip Manor", 6);
            AddStation(stations, "Theydon Bois", 6);
            AddStation(stations, "Turkey Street", 6);
            AddStation(stations, "Upminster Bridge", 6);
            AddStation(stations, "Uxbridge", 6);
            AddStation(stations, "West Drayton", 6);
            AddStation(stations, "West Ruislip", 6);

            AddStation(stations, "Carpenders Park", 7);
            AddStation(stations, "Chorleywood", 7);
            AddStation(stations, "Croxley", 7);
            AddStation(stations, "Rickmansworth", 7);
            AddStation(stations, "Theobalds Grove", 7);
            AddStation(stations, "Watford", 7);
            AddStation(stations, "Bushey", 8);
            AddStation(stations, "Chalfont & Latimer", 8);
            AddStation(stations, "Cheshunt", 8);
            AddStation(stations, "Watford High Street", 8);
            AddStation(stations, "Amersham", 9);
            AddStation(stations, "Brentwood", 9);
            AddStation(stations, "Chesham", 9);

            AddStation(stations, "Watford Junction", -1);
            AddStation(stations, "Shenfield", -1);
            AddStation(stations, "Iver", -1);
            AddStation(stations, "Langley", -1);
            AddStation(stations, "Slough", -1);
            AddStation(stations, "Burnham", -1);
            AddStation(stations, "Taplow", -1);
            AddStation(stations, "Maidenhead", -1);
            AddStation(stations, "Twyford", -1);
            AddStation(stations, "Reading", -1);

            //add connections, source: https://tfl.gov.uk/corporate/publications-and-reports/working-timetables
            //adding Bakerloo
            string line = "Bakerloo (Elephant & Castle direction)";
            stations["Watford Junction"].Connections.Add(new Connection(stations["Watford High Street"], 3, line));
            stations["Watford High Street"].Connections.Add(new Connection(stations["Bushey"], 2.5, line));
            stations["Bushey"].Connections.Add(new Connection(stations["Carpenders Park"], 3, line));
            stations["Carpenders Park"].Connections.Add(new Connection(stations["Hatch End"], 3, line));
            stations["Hatch End"].Connections.Add(new Connection(stations["Headstone Lane"], 2, line));
            stations["Headstone Lane"].Connections.Add(new Connection(stations["Harrow & Wealdstone"], 2.5, line));
            stations["Harrow & Wealdstone"].Connections.Add(new Connection(stations["Kenton"], 2.5, line));
            stations["Kenton"].Connections.Add(new Connection(stations["South Kenton"], 2, line));
            stations["South Kenton"].Connections.Add(new Connection(stations["North Wembley"], 2, line));
            stations["North Wembley"].Connections.Add(new Connection(stations["Wembley Central"], 2, line));
            stations["Wembley Central"].Connections.Add(new Connection(stations["Stonebridge Park"], 2.5, line));
            stations["Stonebridge Park"].Connections.Add(new Connection(stations["Harlesden"], 2.5, line));
            stations["Harlesden"].Connections.Add(new Connection(stations["Willesden Junction"], 2, line));
            stations["Willesden Junction"].Connections.Add(new Connection(stations["Kensal Green"], 2.5, line));
            stations["Kensal Green"].Connections.Add(new Connection(stations["Queen's Park"], 3.5, line));
            stations["Queen's Park"].Connections.Add(new Connection(stations["Kilburn Park"], 1.5, line));
            stations["Kilburn Park"].Connections.Add(new Connection(stations["Maida Vale"], 2, line));
            stations["Maida Vale"].Connections.Add(new Connection(stations["Warwick Avenue"], 1.5, line));
            stations["Warwick Avenue"].Connections.Add(new Connection(stations["Paddington (Bakerloo, Circle and District)"], 2, line));
            stations["Paddington (Bakerloo, Circle and District)"].Connections.Add(new Connection(stations["Edgware Road (Bakerloo)"], 1.5, line));
            stations["Edgware Road (Bakerloo)"].Connections.Add(new Connection(stations["Marylebone"], 1.5, line));
            stations["Marylebone"].Connections.Add(new Connection(stations["Baker Street"], 1.5, line));
            stations["Baker Street"].Connections.Add(new Connection(stations["Regent's Park"], 1.5, line));
            stations["Regent's Park"].Connections.Add(new Connection(stations["Oxford Circus"], 2.5, line));
            stations["Oxford Circus"].Connections.Add(new Connection(stations["Piccadilly Circus"], 2, line));
            stations["Piccadilly Circus"].Connections.Add(new Connection(stations["Charing Cross"], 1.5, line));
            stations["Charing Cross"].Connections.Add(new Connection(stations["Embankment"], 1, line));
            stations["Embankment"].Connections.Add(new Connection(stations["Waterloo"], 2, line));
            stations["Waterloo"].Connections.Add(new Connection(stations["Lambeth North"], 1.5, line));
            stations["Lambeth North"].Connections.Add(new Connection(stations["Elephant & Castle"], 3, line));

            line = "Bakerloo (Watford Junction direction)";
            stations["Elephant & Castle"].Connections.Add(new Connection(stations["Lambeth North"], 2, line));
            stations["Lambeth North"].Connections.Add(new Connection(stations["Waterloo"], 2, line));
            stations["Waterloo"].Connections.Add(new Connection(stations["Embankment"], 1.5, line));
            stations["Embankment"].Connections.Add(new Connection(stations["Charing Cross"], 1, line));
            stations["Charing Cross"].Connections.Add(new Connection(stations["Piccadilly Circus"], 2, line));
            stations["Piccadilly Circus"].Connections.Add(new Connection(stations["Oxford Circus"], 2, line));
            stations["Oxford Circus"].Connections.Add(new Connection(stations["Regent's Park"], 2, line));
            stations["Regent's Park"].Connections.Add(new Connection(stations["Baker Street"], 2, line));
            stations["Baker Street"].Connections.Add(new Connection(stations["Marylebone"], 1, line));
            stations["Marylebone"].Connections.Add(new Connection(stations["Edgware Road (Bakerloo)"], 1, line));
            stations["Edgware Road (Bakerloo)"].Connections.Add(new Connection(stations["Paddington (Bakerloo, Circle and District)"], 2, line));
            stations["Paddington (Bakerloo, Circle and District)"].Connections.Add(new Connection(stations["Warwick Avenue"], 2, line));
            stations["Warwick Avenue"].Connections.Add(new Connection(stations["Maida Vale"], 1.5, line));
            stations["Maida Vale"].Connections.Add(new Connection(stations["Kilburn Park"], 2, line));
            stations["Kilburn Park"].Connections.Add(new Connection(stations["Queen's Park"], 2.5, line));
            stations["Queen's Park"].Connections.Add(new Connection(stations["Kensal Green"], 3, line));
            stations["Kensal Green"].Connections.Add(new Connection(stations["Willesden Junction"], 3, line));
            stations["Willesden Junction"].Connections.Add(new Connection(stations["Harlesden"], 2, line));
            stations["Harlesden"].Connections.Add(new Connection(stations["Stonebridge Park"], 2.5, line));
            stations["Stonebridge Park"].Connections.Add(new Connection(stations["Wembley Central"], 2.5, line));
            stations["Wembley Central"].Connections.Add(new Connection(stations["North Wembley"], 2, line));
            stations["North Wembley"].Connections.Add(new Connection(stations["South Kenton"], 2, line));
            stations["South Kenton"].Connections.Add(new Connection(stations["Kenton"], 2, line));
            stations["Kenton"].Connections.Add(new Connection(stations["Harrow & Wealdstone"], 2.5, line));
            stations["Harrow & Wealdstone"].Connections.Add(new Connection(stations["Headstone Lane"], 3, line));
            stations["Headstone Lane"].Connections.Add(new Connection(stations["Hatch End"], 2, line));
            stations["Hatch End"].Connections.Add(new Connection(stations["Carpenders Park"], 3, line));
            stations["Carpenders Park"].Connections.Add(new Connection(stations["Bushey"], 3, line));
            stations["Bushey"].Connections.Add(new Connection(stations["Watford High Street"], 2.5, line));
            stations["Watford High Street"].Connections.Add(new Connection(stations["Watford Junction"], 4, line));


            //adding Central
            line = "Central (Epping direction)";
            stations["West Ruislip"].Connections.Add(new Connection(stations["Ruislip Gardens"], 2.5, line));
            stations["Ruislip Gardens"].Connections.Add(new Connection(stations["South Ruislip"], 1.5, line));
            stations["South Ruislip"].Connections.Add(new Connection(stations["Northolt"], 2.75, line));
            stations["Northolt"].Connections.Add(new Connection(stations["Greenford"], 2.25, line));
            stations["Greenford"].Connections.Add(new Connection(stations["Perivale"], 2, line));
            stations["Perivale"].Connections.Add(new Connection(stations["Hanger Lane"], 2.5, line));
            stations["Hanger Lane"].Connections.Add(new Connection(stations["North Acton"], 3.5, line));
            stations["North Acton"].Connections.Add(new Connection(stations["East Acton"], 1.75, line));
            stations["East Acton"].Connections.Add(new Connection(stations["White City"], 3, line));
            stations["White City"].Connections.Add(new Connection(stations["Shepherd's Bush"], 2.75, line));
            stations["Shepherd's Bush"].Connections.Add(new Connection(stations["Holland Park"], 1.75, line));
            stations["Holland Park"].Connections.Add(new Connection(stations["Notting Hill Gate"], 1.5, line));
            stations["Notting Hill Gate"].Connections.Add(new Connection(stations["Queensway"], 1.5, line));
            stations["Queensway"].Connections.Add(new Connection(stations["Lancaster Gate"], 1.5, line));
            stations["Lancaster Gate"].Connections.Add(new Connection(stations["Marble Arch"], 2.5, line));
            stations["Marble Arch"].Connections.Add(new Connection(stations["Bond Street"], 1.25, line));
            stations["Bond Street"].Connections.Add(new Connection(stations["Oxford Circus"], 1.5, line));
            stations["Oxford Circus"].Connections.Add(new Connection(stations["Tottenham Court Road"], 1.5, line));
            stations["Tottenham Court Road"].Connections.Add(new Connection(stations["Holborn"], 2, line));
            stations["Holborn"].Connections.Add(new Connection(stations["Chancery Lane"], 1.25, line));
            stations["Chancery Lane"].Connections.Add(new Connection(stations["St. Paul's"], 1.75, line));
            stations["St. Paul's"].Connections.Add(new Connection(stations["Bank"], 2, line));
            stations["Bank"].Connections.Add(new Connection(stations["Liverpool Street"], 2.25, line));
            stations["Liverpool Street"].Connections.Add(new Connection(stations["Bethnal Green"], 3, line));
            stations["Bethnal Green"].Connections.Add(new Connection(stations["Mile End"], 2.25, line));
            stations["Mile End"].Connections.Add(new Connection(stations["Stratford"], 3.5, line));
            stations["Stratford"].Connections.Add(new Connection(stations["Leyton"], 2.75, line));
            stations["Leyton"].Connections.Add(new Connection(stations["Leytonstone"], 2.5, line));
            stations["Leytonstone"].Connections.Add(new Connection(stations["Snaresbrook"], 2.25, line));
            stations["Snaresbrook"].Connections.Add(new Connection(stations["South Woodford"], 2, line));
            stations["South Woodford"].Connections.Add(new Connection(stations["Woodford"], 2.25, line));
            stations["Woodford"].Connections.Add(new Connection(stations["Buckhurst Hill"], 2.5, line));
            stations["Buckhurst Hill"].Connections.Add(new Connection(stations["Loughton"], 2.5, line));
            stations["Loughton"].Connections.Add(new Connection(stations["Debden"], 2.5, line));
            stations["Debden"].Connections.Add(new Connection(stations["Theydon Bois"], 3.5, line));
            stations["Theydon Bois"].Connections.Add(new Connection(stations["Epping"], 2.75, line));

            line = "Central (Hainault via Newbury Park direction)";
            stations["Ealing Broadway"].Connections.Add(new Connection(stations["West Acton"], 2.25, line));
            stations["West Acton"].Connections.Add(new Connection(stations["North Acton"], 3, line));
            stations["North Acton"].Connections.Add(new Connection(stations["East Acton"], 1.75, line));
            stations["East Acton"].Connections.Add(new Connection(stations["White City"], 3, line));
            stations["White City"].Connections.Add(new Connection(stations["Shepherd's Bush"], 2.75, line));
            stations["Shepherd's Bush"].Connections.Add(new Connection(stations["Holland Park"], 1.75, line));
            stations["Holland Park"].Connections.Add(new Connection(stations["Notting Hill Gate"], 1.5, line));
            stations["Notting Hill Gate"].Connections.Add(new Connection(stations["Queensway"], 1.5, line));
            stations["Queensway"].Connections.Add(new Connection(stations["Lancaster Gate"], 1.5, line));
            stations["Lancaster Gate"].Connections.Add(new Connection(stations["Marble Arch"], 2.5, line));
            stations["Marble Arch"].Connections.Add(new Connection(stations["Bond Street"], 1.25, line));
            stations["Bond Street"].Connections.Add(new Connection(stations["Oxford Circus"], 1.5, line));
            stations["Oxford Circus"].Connections.Add(new Connection(stations["Tottenham Court Road"], 1.5, line));
            stations["Tottenham Court Road"].Connections.Add(new Connection(stations["Holborn"], 2, line));
            stations["Holborn"].Connections.Add(new Connection(stations["Chancery Lane"], 1.25, line));
            stations["Chancery Lane"].Connections.Add(new Connection(stations["St. Paul's"], 1.75, line));
            stations["St. Paul's"].Connections.Add(new Connection(stations["Bank"], 2, line));
            stations["Bank"].Connections.Add(new Connection(stations["Liverpool Street"], 2.25, line));
            stations["Liverpool Street"].Connections.Add(new Connection(stations["Bethnal Green"], 3, line));
            stations["Bethnal Green"].Connections.Add(new Connection(stations["Mile End"], 2.25, line));
            stations["Mile End"].Connections.Add(new Connection(stations["Stratford"], 3.5, line));
            stations["Stratford"].Connections.Add(new Connection(stations["Leyton"], 2.75, line));
            stations["Leyton"].Connections.Add(new Connection(stations["Leytonstone"], 2.5, line));
            stations["Leytonstone"].Connections.Add(new Connection(stations["Wanstead"], 2.25, line));
            stations["Wanstead"].Connections.Add(new Connection(stations["Redbridge"], 2, line));
            stations["Redbridge"].Connections.Add(new Connection(stations["Gants Hill"], 2, line));
            stations["Gants Hill"].Connections.Add(new Connection(stations["Newbury Park"], 3, line));
            stations["Newbury Park"].Connections.Add(new Connection(stations["Barkingside"], 1.75, line));
            stations["Barkingside"].Connections.Add(new Connection(stations["Fairlop"], 1.75, line));
            stations["Fairlop"].Connections.Add(new Connection(stations["Hainault"], 2, line));

            line = "Central (Hainault direction)";
            stations["Woodford"].Connections.Add(new Connection(stations["Roding Valley"], 2.25, line));
            stations["Roding Valley"].Connections.Add(new Connection(stations["Chigwell"], 3, line));
            stations["Chigwell"].Connections.Add(new Connection(stations["Grange Hill"], 2.25, line));
            stations["Grange Hill"].Connections.Add(new Connection(stations["Hainault"], 2.25, line));

            line = "Central (West Ruislip direction)";
            stations["Epping"].Connections.Add(new Connection(stations["Theydon Bois"], 2.75, line));
            stations["Theydon Bois"].Connections.Add(new Connection(stations["Debden"], 3.5, line));
            stations["Debden"].Connections.Add(new Connection(stations["Loughton"], 3, line));
            stations["Loughton"].Connections.Add(new Connection(stations["Buckhurst Hill"], 2.5, line));
            stations["Buckhurst Hill"].Connections.Add(new Connection(stations["Woodford"], 2.75, line));
            stations["Woodford"].Connections.Add(new Connection(stations["South Woodford"], 2.25, line));
            stations["South Woodford"].Connections.Add(new Connection(stations["Snaresbrook"], 2, line));
            stations["Snaresbrook"].Connections.Add(new Connection(stations["Leytonstone"], 2.75, line));
            stations["Leytonstone"].Connections.Add(new Connection(stations["Leyton"], 2.25, line));
            stations["Leyton"].Connections.Add(new Connection(stations["Stratford"], 2.75, line));
            stations["Stratford"].Connections.Add(new Connection(stations["Mile End"], 3.5, line));
            stations["Mile End"].Connections.Add(new Connection(stations["Bethnal Green"], 2.75, line));
            stations["Bethnal Green"].Connections.Add(new Connection(stations["Liverpool Street"], 3.25, line));
            stations["Liverpool Street"].Connections.Add(new Connection(stations["Bank"], 2, line));
            stations["Bank"].Connections.Add(new Connection(stations["St. Paul's"], 1.75, line));
            stations["St. Paul's"].Connections.Add(new Connection(stations["Chancery Lane"], 2, line));
            stations["Chancery Lane"].Connections.Add(new Connection(stations["Holborn"], 1.25, line));
            stations["Holborn"].Connections.Add(new Connection(stations["Tottenham Court Road"], 1.75, line));
            stations["Tottenham Court Road"].Connections.Add(new Connection(stations["Oxford Circus"], 1.25, line));
            stations["Oxford Circus"].Connections.Add(new Connection(stations["Bond Street"], 1.5, line));
            stations["Bond Street"].Connections.Add(new Connection(stations["Marble Arch"], 1.5, line));
            stations["Marble Arch"].Connections.Add(new Connection(stations["Lancaster Gate"], 1.75, line));
            stations["Lancaster Gate"].Connections.Add(new Connection(stations["Queensway"], 2, line));
            stations["Queensway"].Connections.Add(new Connection(stations["Notting Hill Gate"], 1.5, line));
            stations["Notting Hill Gate"].Connections.Add(new Connection(stations["Holland Park"], 1.5, line));
            stations["Holland Park"].Connections.Add(new Connection(stations["Shepherd's Bush"], 1.75, line));
            stations["Shepherd's Bush"].Connections.Add(new Connection(stations["White City"], 3.25, line));
            stations["White City"].Connections.Add(new Connection(stations["East Acton"], 2.75, line));
            stations["East Acton"].Connections.Add(new Connection(stations["North Acton"], 2, line));
            stations["North Acton"].Connections.Add(new Connection(stations["Hanger Lane"], 3.25, line));
            stations["Hanger Lane"].Connections.Add(new Connection(stations["Perivale"], 2.5, line));
            stations["Perivale"].Connections.Add(new Connection(stations["Greenford"], 2.25, line));
            stations["Greenford"].Connections.Add(new Connection(stations["Northolt"], 2.25, line));
            stations["Northolt"].Connections.Add(new Connection(stations["South Ruislip"], 2.5, line));
            stations["South Ruislip"].Connections.Add(new Connection(stations["Ruislip Gardens"], 1.75, line));
            stations["Ruislip Gardens"].Connections.Add(new Connection(stations["West Ruislip"], 2.5, line));

            line = "Central (Ealing Broadway direction)";
            stations["Hainault"].Connections.Add(new Connection(stations["Fairlop"], 2, line));
            stations["Fairlop"].Connections.Add(new Connection(stations["Barkingside"], 1.75, line));
            stations["Barkingside"].Connections.Add(new Connection(stations["Newbury Park"], 2, line));
            stations["Newbury Park"].Connections.Add(new Connection(stations["Gants Hill"], 3, line));
            stations["Gants Hill"].Connections.Add(new Connection(stations["Redbridge"], 1.75, line));
            stations["Redbridge"].Connections.Add(new Connection(stations["Wanstead"], 2, line));
            stations["Wanstead"].Connections.Add(new Connection(stations["Leytonstone"], 2.75, line));
            stations["Leytonstone"].Connections.Add(new Connection(stations["Leyton"], 2.25, line));
            stations["Leyton"].Connections.Add(new Connection(stations["Stratford"], 2.75, line));
            stations["Stratford"].Connections.Add(new Connection(stations["Mile End"], 3.5, line));
            stations["Mile End"].Connections.Add(new Connection(stations["Bethnal Green"], 2.75, line));
            stations["Bethnal Green"].Connections.Add(new Connection(stations["Liverpool Street"], 3.25, line));
            stations["Liverpool Street"].Connections.Add(new Connection(stations["Bank"], 2, line));
            stations["Bank"].Connections.Add(new Connection(stations["St. Paul's"], 1.75, line));
            stations["St. Paul's"].Connections.Add(new Connection(stations["Chancery Lane"], 2, line));
            stations["Chancery Lane"].Connections.Add(new Connection(stations["Holborn"], 1.25, line));
            stations["Holborn"].Connections.Add(new Connection(stations["Tottenham Court Road"], 1.75, line));
            stations["Tottenham Court Road"].Connections.Add(new Connection(stations["Oxford Circus"], 1.25, line));
            stations["Oxford Circus"].Connections.Add(new Connection(stations["Bond Street"], 1.5, line));
            stations["Bond Street"].Connections.Add(new Connection(stations["Marble Arch"], 1.5, line));
            stations["Marble Arch"].Connections.Add(new Connection(stations["Lancaster Gate"], 1.75, line));
            stations["Lancaster Gate"].Connections.Add(new Connection(stations["Queensway"], 2, line));
            stations["Queensway"].Connections.Add(new Connection(stations["Notting Hill Gate"], 1.5, line));
            stations["Notting Hill Gate"].Connections.Add(new Connection(stations["Holland Park"], 1.5, line));
            stations["Holland Park"].Connections.Add(new Connection(stations["Shepherd's Bush"], 1.75, line));
            stations["Shepherd's Bush"].Connections.Add(new Connection(stations["White City"], 3.25, line));
            stations["White City"].Connections.Add(new Connection(stations["East Acton"], 2.75, line));
            stations["East Acton"].Connections.Add(new Connection(stations["North Acton"], 2, line));
            stations["North Acton"].Connections.Add(new Connection(stations["West Acton"], 2.75, line));
            stations["West Acton"].Connections.Add(new Connection(stations["Ealing Broadway"], 2.75, line));

            line = "Central (Woodford direction)";
            stations["Hainault"].Connections.Add(new Connection(stations["Grange Hill"], 2, line));
            stations["Grange Hill"].Connections.Add(new Connection(stations["Chigwell"], 2, line));
            stations["Chigwell"].Connections.Add(new Connection(stations["Roding Valley"], 3.5, line));
            stations["Roding Valley"].Connections.Add(new Connection(stations["Woodford"], 2.5, line));

            //adding Circle

            //add Hammersmith & City

            //add District
            
            //add Jubilee

            //add Metropolitan

            //add Northern

            //add Piccadilly

            //add Victoria

            //add Waterloo & City

            //add Elizabeth

            //add other connections (walk)
            line = "Walk";
            //AddConnection(stations["Euston"], stations["Euston Square"], 1, line);
            //AddConnection(stations["Edgware Road"], stations["Edgware Road (Bakerloo)"], 0, line);
            //AddConnection(stations["Edgware Road"], stations["Edgware Road (Circle and Hammersmith & City)"], 0, line);
            //AddConnection(stations["Edgware Road"], stations["Edgware Road (Circle Terminal)"], 0, line);
            //AddConnection(stations["Edgware Road (Bakerloo)"], stations["Edgware Road (Circle and Hammersmith & City)"], 0, line);
            //AddConnection(stations["Edgware Road (Circle and Hammersmith & City)"], stations["Edgware Road (Circle Terminal)"], 0, line);
            //AddConnection(stations["Edgware Road (Bakerloo)"], stations["Edgware Road (Circle Terminal)"], 0, line);
            //AddConnection(stations["Paddington"], stations["Paddington (Bakerloo, Circle and District)"], 0, line);
            //AddConnection(stations["Paddington"], stations["Paddington (Circle and Hammersmith & City)"], 0, line);
            //AddConnection(stations["Paddington"], stations["Paddington (Elizabeth)"], 0, line);
            //AddConnection(stations["Paddington (Bakerloo, Circle and District)"], stations["Paddington (Circle and Hammersmith & City)"], 0, line);
            //AddConnection(stations["Paddington (Circle and Hammersmith & City)"], stations["Paddington (Elizabeth)"], 0, line);
            //AddConnection(stations["Paddington (Bakerloo, Circle and District)"], stations["Paddington (Elizabeth)"], 0, line);
            //AddConnection(stations["Bond Street"], stations["Bond Street (Elizabeth)"], 0, line);
            //AddConnection(stations["Tottenham Court Road"], stations["Tottenham Court Road (Elizabeth)"], 0, line);
            //AddConnection(stations["Farringdon"], stations["Farringdon (Elizabeth)"], 0, line);
            //AddConnection(stations["Liverpool Street"], stations["Liverpool Street (Elizabeth)"], 0, line);
            //AddConnection(stations["Moorgate"], stations["Liverpool Street (Elizabeth)"], 1, line);
            //AddConnection(stations["Tower Hill"], stations["Tower Gateway"], 0, line);
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

