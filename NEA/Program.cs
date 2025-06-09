using System;
using System.Collections.Generic;
using System.Linq;
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
                foreach (var connection in current.Connections)
                {
                    int newTime = current.time2Travel + connection.time;
                    if (newTime < connection.station.time2Travel)
                    {
                        connection.station.time2Travel = newTime;
                        connection.station.previousStation = current;
                        pq.Enqueue(connection.station, newTime);
                    }
                }
            }
        }
        static void Main(string[] args)
        {
        }
    }
}
