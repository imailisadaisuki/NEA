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
        public double time { get; set; }
        public string line { get; set; }
        public Connection(Station station, double time, string line) => (this.station, this.time, this.line) = (station, time, line);
    }
    public class Station
    {
        public string name { get; set; }
        public double zone { get; set; }
        public List<Connection> Connections = new List<Connection>();
        public double time2Travel = int.MaxValue;
        public Station previousStation = null;
        public bool visited = false;
        public Station(string name, double zone) => (this.name, this.zone) = (name, zone);
        public List<string> getPrevConnectionLine() =>
            previousStation.Connections
           .Where(c => c.station == this)
           .Select(c => c.line)
           .ToList();
    }

    public class DecomposedConnection
    {
        public Station startStation { get; set; }
        public Station endStation { get; set; }
        public double estimatedTime { get; set; }
        public double cost { get; set; }
        public Stack<string> path = new Stack<string>();
        public DecomposedConnection(Station startStation, Station endStation, double estimatedTime, double cost, Stack<string> path) =>
            (this.startStation, this.endStation, this.estimatedTime, this.cost, this.path) =
            (startStation, endStation, estimatedTime, cost, path);
    }
}
