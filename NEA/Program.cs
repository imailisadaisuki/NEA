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
            PriorityQueue<Station> queue = new PriorityQueue<Station>();
            start.time2Travel = 0;
            queue.Enqueue(start, 0);
            while (!queue.IsEmpty())
            {
                Station current = queue.Dequeue();
                if (current.visited) continue;
                current.visited = true;
                foreach (var connection in current.Connections)
                {
                    int newTime = current.time2Travel + connection.time;
                    if (newTime < connection.station.time2Travel)
                    {
                        connection.station.time2Travel = newTime;
                        connection.station.previousStation = current;
                        queue.Enqueue(connection.station, newTime);
                    }
                }
            }
        }
        static void Main(string[] args)
        {
        }
    }
}
