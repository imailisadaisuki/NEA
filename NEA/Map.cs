using System;
using System.Collections.Generic;
using System.Linq;

namespace NEA
{
    internal class Connection
    {
        public Station station { get; set; }
        public double time { get; set; }
        public string line { get; set; }
        public List<string> platform { get; set; }
        public Connection(Station station, double time, string line, List<string> platform) 
            => (this.station, this.time, this.line, this.platform) = (station, time, line, platform);
    }
    internal class Station
    {
        public string name { get; set; }
        public double zone { get; set; }
        public List<Connection> Connections = new List<Connection>();
        // Variables for Dijkstra's algorithm
        public double travelTime = int.MaxValue; 
        public Station previousStation = null;
        public bool visited = false;
        public HashSet<string> previousLines = new HashSet<string>();
        public Station(string name, double zone) 
            => (this.name, this.zone) = (name, zone);
        public HashSet<string> getPrevConnectionLine() 
            => previousStation == null ? new HashSet<string>()
            : previousStation.Connections
            .Where(c => c.station == this)
            .Select(c => c.line)
            .ToHashSet();
        public List<string> getPlatformNumber(HashSet<string> lines)
        {
            HashSet<string> platforms = new HashSet<string>();
            platforms.UnionWith(Connections
                .Where(c => lines.Contains(c.line))
                .SelectMany(c => c.platform)); // Remove repeated platforms
            return platforms.OrderBy(p => p).ToList();
        }
        public void ResetStation()
        {
            travelTime = int.MaxValue;
            previousStation = null;
            visited = false;
            previousLines.Clear();
        }
    }
    internal class DecomposedConnection
    {
        public Station startStation { get; set; }
        public Station endStation { get; set; }
        public double estimatedTime { get; set; }
        public double cost { get; set; }
        public List<PathInstruction> path = new List<PathInstruction>();
        public DecomposedConnection(Station startStation, Station endStation, double estimatedTime, double cost, List<PathInstruction> path) 
            => (this.startStation, this.endStation, this.estimatedTime, this.cost, this.path) 
            = (startStation, endStation, estimatedTime, cost, path);
    }
}
