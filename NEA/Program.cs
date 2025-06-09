using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    internal class Program
    {
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
                    if (current.getPrevConnectionLine() != connection.line && current.previousStation != null)
                    {
                        newTime += 2; // Add transfer time if changing lines
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
        static List<string> GetShortestPath(Station end)
        {
            List<string> path = new List<string>();
            Station current = end;
            string connectionLine = current.getPrevConnectionLine();
            while (current.previousStation != null)
            {
                if (current.getPrevConnectionLine() == connectionLine)
                    path.Add(current.name); // did not change lines
                else
                {
                    path.Add($"Transfer from {connectionLine} to {current.getPrevConnectionLine()} in {current.name}");
                    connectionLine = current.getPrevConnectionLine(); //  changes line
                }
                current = current.previousStation;
            }
            path.Add($"Take the {connectionLine} Line from {current.name}"); // add the starting station
            path.Reverse();
            return path;
        }
        static void Main(string[] args)
        {
            Station waterloo = new Station("Waterloo");
            Station embankment = new Station("Embankment");
            Station charingCross = new Station("Charing Cross");

            waterloo.Connections.Add(new Connection(embankment, 2, "Northern"));
            embankment.Connections.Add(new Connection(waterloo, 2, "Northern"));
            embankment.Connections.Add(new Connection(charingCross, 1, "Bakerloo"));
            charingCross.Connections.Add(new Connection(embankment, 1, "Bakerloo"));
            Dijkstra(waterloo);
            GetShortestPath(charingCross).ForEach(Console.WriteLine);
            Console.WriteLine($"Estimated time = {charingCross.time2Travel} minutes");
            Console.ReadLine();
        }
    }
}
