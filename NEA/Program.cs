using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class Program
    {
        static void AddConnection(Station a, Station b, int time, string line)
        {
            a.Connections.Add(new Connection(b, time, line));
            b.Connections.Add(new Connection(a, time, line));
        }
        static void AddStation(Dictionary<string, Station> stations, string name, int zone) => stations.Add(name, new Station(name, zone));
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
            AddStation(stations, "Earl's Court", 1); //Also Zone 2
            AddStation(stations, "Edgware Road", 1);
            AddStation(stations, "Edgware Road (Bakerloo)", 1);
            AddStation(stations, "Edgware Road (Circle and Hammersmith & City)", 1);
            AddStation(stations, "Edgware Road (Circle Terminal)", 1);
            AddStation(stations, "Elephant & Castle", 1); //Also Zone 2
            AddStation(stations, "Embankment", 1);
            AddStation(stations, "Euston", 1);
            AddStation(stations, "Euston Square", 1);
            AddStation(stations, "Farringdon", 1);
            AddStation(stations, "Gloucester Road", 1);
            AddStation(stations, "Goodge Street", 1);
            AddStation(stations, "Great Portland Street", 1);
            AddStation(stations, "Green Park", 1);
            AddStation(stations, "High Street Kensington", 1);
            AddStation(stations, "Holborn", 1);
            AddStation(stations, "Hyde Park Corner", 1);
            AddStation(stations, "Kennington", 1); //Also Zone 2
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
            AddStation(stations, "Notting Hill Gate", 1); //Also Zone 2
            AddStation(stations, "Old Street", 1);
            AddStation(stations, "Oxford Circus", 1);
            AddStation(stations, "Paddington", 1);
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
            AddStation(stations, "Tower Gateway", 1); //DLR
            AddStation(stations, "Vauxhall", 1); //Also Zone 2
            AddStation(stations, "Victoria", 1);
            AddStation(stations, "Warren Street", 1);
            AddStation(stations, "Waterloo", 1);
            AddStation(stations, "Westminster", 1);

            //Elizebeth Line stations
            AddStation(stations, "Paddington (Elizabeth)", 1);
            AddStation(stations, "Bond Street (Elizabeth)", 1);
            AddStation(stations, "Tottenham Court Road (Elizabeth)", 1);
            AddStation(stations, "Farringdon (Elizabeth)", 1);
            AddStation(stations, "Liverpool Street (Elizabeth)", 1);

            //add connections, source: https://tfl.gov.uk/corporate/publications-and-reports/working-timetables
            //adding Bakerloo
            string line = "Bakerloo";
            AddConnection(stations["Paddington (Bakerloo, Circle and District)"], stations["Edgware Road (Bakerloo)"], 2, line);
            AddConnection(stations["Edgware Road (Bakerloo)"], stations["Marylebone"], 1, line);
            AddConnection(stations["Marylebone"], stations["Baker Street"], 2, line);
            AddConnection(stations["Baker Street"], stations["Regent's Park"], 2, line);
            AddConnection(stations["Regent's Park"], stations["Oxford Circus"], 2, line);
            AddConnection(stations["Oxford Circus"], stations["Piccadilly Circus"], 2, line);
            AddConnection(stations["Piccadilly Circus"], stations["Charing Cross"], 2, line);
            AddConnection(stations["Charing Cross"], stations["Embankment"], 1, line);
            AddConnection(stations["Embankment"], stations["Waterloo"], 2, line);
            AddConnection(stations["Waterloo"], stations["Lambeth North"], 2, line);
            AddConnection(stations["Lambeth North"], stations["Elephant & Castle"], 2, line);

            //adding Central
            line = "Central";
            AddConnection(stations["Liverpool Street"], stations["Bank"], 2, line);
            AddConnection(stations["Bank"], stations["St. Paul's"], 2, line);
            AddConnection(stations["St. Paul's"], stations["Chancery Lane"], 2, line);
            AddConnection(stations["Chancery Lane"], stations["Holborn"], 1, line);
            AddConnection(stations["Holborn"], stations["Tottenham Court Road"], 2, line);
            AddConnection(stations["Tottenham Court Road"], stations["Oxford Circus"], 1, line);
            AddConnection(stations["Oxford Circus"], stations["Bond Street"], 2, line);
            AddConnection(stations["Bond Street"], stations["Marble Arch"], 1, line);
            AddConnection(stations["Marble Arch"], stations["Lancaster Gate"], 2, line);
            AddConnection(stations["Lancaster Gate"], stations["Queensway"], 2, line);
            AddConnection(stations["Queensway"], stations["Notting Hill Gate"], 2, line);

            //adding Circle
            line = "Circle";
            AddConnection(stations["Edgware Road (Circle Terminal)"], stations["Paddington (Bakerloo, Circle and District)"], 2, line);
            AddConnection(stations["Paddington (Bakerloo, Circle and District)"], stations["Bayswater"], 2, line);
            AddConnection(stations["Bayswater"], stations["Notting Hill Gate"], 2, line);
            AddConnection(stations["Notting Hill Gate"], stations["High Street Kensington"], 2, line);
            AddConnection(stations["High Street Kensington"], stations["Gloucester Road"], 2, line);
            AddConnection(stations["Gloucester Road"], stations["South Kensington"], 2, line);
            AddConnection(stations["South Kensington"], stations["Sloane Square"], 2, line);
            AddConnection(stations["Sloane Square"], stations["Victoria"], 2, line);
            AddConnection(stations["Victoria"], stations["St. James's Park"], 2, line);
            AddConnection(stations["St. James's Park"], stations["Westminster"], 2, line);
            AddConnection(stations["Westminster"], stations["Embankment"], 2, line);
            AddConnection(stations["Embankment"], stations["Temple"], 2, line);
            AddConnection(stations["Temple"], stations["Blackfriars"], 1, line);
            AddConnection(stations["Blackfriars"], stations["Mansion House"], 2, line);
            AddConnection(stations["Mansion House"], stations["Cannon Street"], 1, line);
            AddConnection(stations["Cannon Street"], stations["Monument"], 1, line);
            AddConnection(stations["Monument"], stations["Tower Hill"], 2, line);
            AddConnection(stations["Tower Hill"], stations["Aldgate"], 2, line);
            AddConnection(stations["Aldgate"], stations["Liverpool Street"], 2, line);
            AddConnection(stations["Liverpool Street"], stations["Moorgate"], 2, line);
            AddConnection(stations["Moorgate"], stations["Barbican"], 2, line);
            AddConnection(stations["Barbican"], stations["Farringdon"], 1, line);
            AddConnection(stations["Farringdon"], stations["King's Cross & St Pancras International"], 3, line);
            AddConnection(stations["King's Cross & St Pancras International"], stations["Euston Square"], 2, line);
            AddConnection(stations["Euston Square"], stations["Great Portland Street"], 1, line);
            AddConnection(stations["Great Portland Street"], stations["Baker Street"], 2, line);
            AddConnection(stations["Baker Street"], stations["Edgware Road (Circle and Hammersmith & City)"], 2, line);
            AddConnection(stations["Edgware Road (Circle and Hammersmith & City)"], stations["Paddington (Circle and Hammersmith & City)"], 2, line);

            //add Hammersmith & City
            line = "Hammersmith & City";
            AddConnection(stations["Aldgate East"], stations["Liverpool Street"], 2, line);
            AddConnection(stations["Liverpool Street"], stations["Moorgate"], 2, line);
            AddConnection(stations["Moorgate"], stations["Barbican"], 2, line);
            AddConnection(stations["Barbican"], stations["Farringdon"], 1, line);
            AddConnection(stations["Farringdon"], stations["King's Cross & St Pancras International"], 3, line);
            AddConnection(stations["King's Cross & St Pancras International"], stations["Euston Square"], 2, line);
            AddConnection(stations["Euston Square"], stations["Great Portland Street"], 1, line);
            AddConnection(stations["Great Portland Street"], stations["Baker Street"], 2, line);
            AddConnection(stations["Baker Street"], stations["Edgware Road (Circle and Hammersmith & City)"], 2, line);
            AddConnection(stations["Edgware Road (Circle and Hammersmith & City)"], stations["Paddington (Circle and Hammersmith & City)"], 2, line);

            //add District
            line = "District";
            AddConnection(stations["Aldgate East"], stations["Tower Hill"], 2, line);
            AddConnection(stations["Tower Hill"], stations["Monument"], 2, line);
            AddConnection(stations["Monument"], stations["Cannon Street"], 1, line);
            AddConnection(stations["Cannon Street"], stations["Mansion House"], 1, line);
            AddConnection(stations["Mansion House"], stations["Blackfriars"], 2, line);
            AddConnection(stations["Blackfriars"], stations["Temple"], 2, line);
            AddConnection(stations["Temple"], stations["Embankment"], 2, line);
            AddConnection(stations["Embankment"], stations["Westminster"], 2, line);
            AddConnection(stations["Westminster"], stations["St. James's Park"], 2, line);
            AddConnection(stations["St. James's Park"], stations["Victoria"], 2, line);
            AddConnection(stations["Victoria"], stations["Sloane Square"], 2, line);
            AddConnection(stations["Sloane Square"], stations["South Kensington"], 2, line);
            AddConnection(stations["South Kensington"], stations["Gloucester Road"], 2, line);
            AddConnection(stations["Gloucester Road"], stations["Earl's Court"], 2, line);
            //branch
            AddConnection(stations["Edgware Road (Circle and Hammersmith & City)"], stations["Paddington (Bakerloo, Circle and District)"], 2, line);
            AddConnection(stations["Paddington (Bakerloo, Circle and District)"], stations["Bayswater"], 2, line);
            AddConnection(stations["Bayswater"], stations["Notting Hill Gate"], 2, line);
            AddConnection(stations["Notting Hill Gate"], stations["High Street Kensington"], 2, line);
            AddConnection(stations["High Street Kensington"], stations["Earl's Court"], 3, line);

            //add Jubilee
            line = "Jubilee";
            AddConnection(stations["Baker Street"], stations["Bond Street"], 2, line);
            AddConnection(stations["Bond Street"], stations["Green Park"], 2, line);
            AddConnection(stations["Green Park"], stations["Westminster"], 2, line);
            AddConnection(stations["Westminster"], stations["Waterloo"], 2, line);
            AddConnection(stations["Waterloo"], stations["Southwark"], 1, line);
            AddConnection(stations["Southwark"], stations["London Bridge"], 2, line);

            //add Metropolitan
            line = "Metropolitan";
            AddConnection(stations["Aldgate"], stations["Liverpool Street"], 2, line);
            AddConnection(stations["Liverpool Street"], stations["Moorgate"], 2, line);
            AddConnection(stations["Moorgate"], stations["Barbican"], 2, line);
            AddConnection(stations["Barbican"], stations["Farringdon"], 1, line);
            AddConnection(stations["Farringdon"], stations["King's Cross & St Pancras International"], 3, line);
            AddConnection(stations["King's Cross & St Pancras International"], stations["Euston Square"], 2, line);
            AddConnection(stations["Euston Square"], stations["Great Portland Street"], 1, line);
            AddConnection(stations["Great Portland Street"], stations["Baker Street"], 2, line);

            //add Northern
            //add Northern (Charing Cross branch)
            line = "Northern (Charing Cross branch)";
            AddConnection(stations["Euston"], stations["Warren Street"], 1, line);
            AddConnection(stations["Warren Street"], stations["Goodge Street"], 1, line);
            AddConnection(stations["Goodge Street"], stations["Tottenham Court Road"], 1, line);
            AddConnection(stations["Tottenham Court Road"], stations["Leicester Square"], 1, line);
            AddConnection(stations["Leicester Square"], stations["Charing Cross"], 2, line);
            AddConnection(stations["Charing Cross"], stations["Embankment"], 1, line);
            AddConnection(stations["Embankment"], stations["Waterloo"], 2, line);
            AddConnection(stations["Waterloo"], stations["Kennington"], 3, line);
            AddConnection(stations["Kennington"], stations["Nine Elms"], 2, line);
            AddConnection(stations["Nine Elms"], stations["Battersea Power Station"], 2, line);
            //add Northern (Bank branch)
            line = "Northern (Bank branch)";
            AddConnection(stations["Euston"], stations["King's Cross & St Pancras International"], 2, line);
            AddConnection(stations["King's Cross & St Pancras International"], stations["Angel"], 2, line);
            AddConnection(stations["Angel"], stations["Old Street"], 2, line);
            AddConnection(stations["Old Street"], stations["Moorgate"], 2, line);
            AddConnection(stations["Moorgate"], stations["Bank"], 2, line);
            AddConnection(stations["Bank"], stations["London Bridge"], 2, line);
            AddConnection(stations["London Bridge"], stations["Borough"], 2, line);
            AddConnection(stations["Borough"], stations["Elephant & Castle"], 2, line);
            AddConnection(stations["Elephant & Castle"], stations["Kennington"], 2, line);

            //add Piccadilly
            line = "Piccadilly";
            AddConnection(stations["King's Cross & St Pancras International"], stations["Russell Square"], 2, line);
            AddConnection(stations["Russell Square"], stations["Holborn"], 2, line);
            AddConnection(stations["Holborn"], stations["Covent Garden"], 2, line);
            AddConnection(stations["Covent Garden"], stations["Leicester Square"], 1, line);
            AddConnection(stations["Leicester Square"], stations["Piccadilly Circus"], 2, line);
            AddConnection(stations["Piccadilly Circus"], stations["Green Park"], 1, line);
            AddConnection(stations["Green Park"], stations["Hyde Park Corner"], 2, line);
            AddConnection(stations["Hyde Park Corner"], stations["Knightsbridge"], 2, line);
            AddConnection(stations["Knightsbridge"], stations["South Kensington"], 3, line);
            AddConnection(stations["South Kensington"], stations["Gloucester Road"], 2, line);
            AddConnection(stations["Gloucester Road"], stations["Earl's Court"], 2, line);

            //add Victoria
            line = "Victoria";
            AddConnection(stations["King's Cross & St Pancras International"], stations["Euston"], 2, line);
            AddConnection(stations["Euston"], stations["Warren Street"], 2, line);
            AddConnection(stations["Warren Street"], stations["Oxford Circus"], 2, line);
            AddConnection(stations["Oxford Circus"], stations["Green Park"], 2, line);
            AddConnection(stations["Green Park"], stations["Victoria"], 2, line);
            AddConnection(stations["Victoria"], stations["Pimlico"], 2, line);
            AddConnection(stations["Pimlico"], stations["Vauxhall"], 1, line);

            //add Waterloo & City
            line = "Waterloo & City (Saturday service not operating)";
            AddConnection(stations["Waterloo"], stations["Bank"], 4, line);

            //add Elizabeth
            line = "Elizabeth";
            AddConnection(stations["Paddington (Elizabeth)"], stations["Bond Street (Elizabeth)"], 4, line);
            AddConnection(stations["Bond Street (Elizabeth)"], stations["Tottenham Court Road (Elizabeth)"], 2, line);
            AddConnection(stations["Tottenham Court Road (Elizabeth)"], stations["Farringdon (Elizabeth)"], 3, line);
            AddConnection(stations["Farringdon (Elizabeth)"], stations["Liverpool Street (Elizabeth)"], 2, line);

            //add other connections (walk)
            line = "Walk";
            AddConnection(stations["Euston"], stations["Euston Square"], 1, line);
            AddConnection(stations["Edgware Road"], stations["Edgware Road (Bakerloo)"], 0, line);
            AddConnection(stations["Edgware Road"], stations["Edgware Road (Circle and Hammersmith & City)"], 0, line);
            AddConnection(stations["Edgware Road"], stations["Edgware Road (Circle Terminal)"], 0, line);
            AddConnection(stations["Edgware Road (Bakerloo)"], stations["Edgware Road (Circle and Hammersmith & City)"], 0, line);
            AddConnection(stations["Edgware Road (Circle and Hammersmith & City)"], stations["Edgware Road (Circle Terminal)"], 0, line);
            AddConnection(stations["Edgware Road (Bakerloo)"], stations["Edgware Road (Circle Terminal)"], 0, line);
            AddConnection(stations["Paddington"], stations["Paddington (Bakerloo, Circle and District)"], 0, line);
            AddConnection(stations["Paddington"], stations["Paddington (Circle and Hammersmith & City)"], 0, line);
            AddConnection(stations["Paddington"], stations["Paddington (Elizabeth)"], 0, line);
            AddConnection(stations["Paddington (Bakerloo, Circle and District)"], stations["Paddington (Circle and Hammersmith & City)"], 0, line);
            AddConnection(stations["Paddington (Circle and Hammersmith & City)"], stations["Paddington (Elizabeth)"], 0, line);
            AddConnection(stations["Paddington (Bakerloo, Circle and District)"], stations["Paddington (Elizabeth)"], 0, line);
            AddConnection(stations["Bond Street"], stations["Bond Street (Elizabeth)"], 0, line);
            AddConnection(stations["Tottenham Court Road"], stations["Tottenham Court Road (Elizabeth)"], 0, line);
            AddConnection(stations["Farringdon"], stations["Farringdon (Elizabeth)"], 0, line);
            AddConnection(stations["Liverpool Street"], stations["Liverpool Street (Elizabeth)"], 0, line);
            AddConnection(stations["Moorgate"], stations["Liverpool Street (Elizabeth)"], 1, line);
            AddConnection(stations["Moorgate"], stations["Liverpool Street"], 0, line);
            AddConnection(stations["Tower Hill"], stations["Tower Gateway"], 0, line);

            return stations;
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
        static void Main(string[] args)
        {
            Dictionary<string, Station> stations = Zone1Prototype();
            Station start, end;
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter starting station:");
                    start = stations[Console.ReadLine()];
                    break;
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Invalid station.");
                }
            }
            Console.Clear();
            while (true)
            {
                try
                {
                    Console.WriteLine("Enter destination:");
                    end = stations[Console.ReadLine()];
                    break;
                }
                catch
                {
                    Console.Clear();
                    Console.WriteLine("Invalid station.");
                }
            }
            Console.Clear();
            Dijkstra(start);
            GetShortestPath(end).ForEach(Console.Write);
            Console.WriteLine($"\nEstimated time = {end.time2Travel} minutes");
            Console.ReadLine();
        }
    }
}
