using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    public class Connection
    {
        public Station station { get; set; }
        public int time { get; set; }
        public string line { get; set; }
        public Connection(Station station, int time, string line) => (this.station, this.time, this.line) = (station, time, line);
    }
    public class Station 
    {
        public string name { get; set; }
        public int zone { get; set; }
        public List<Connection> Connections = new List<Connection>();
        public int time2Travel = int.MaxValue;
        public Station previousStation = null;
        public bool visited = false;
        public Station (string name, int zone) => (this.name, this.zone) = (name, zone);
        public List<string> getPrevConnectionLine()
        {
            List<string> lines = new List<string>();
            foreach (Connection connection in Connections)
            {
                if (connection.station == previousStation)
                {
                    lines.Add(connection.line);
                }
            }
            return lines;
        }
    }
    public class DecomposedConnection
    {
        public Station startStation { get; set; }
        public Station endStation { get; set; }
        public int estimatedTime { get; set; }
        public double cost { get; set; }
        public List<string> path = new List<string>();
        public DecomposedConnection(Station startStation, Station endStation, int estimatedTime, double cost, List<string> path) =>
            (this.startStation, this.endStation, this.estimatedTime, this.cost, this.path) =
            (startStation, endStation, estimatedTime, cost, path);
    }
}
