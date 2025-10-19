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
        public HashSet<string> platform { get; set; }
        public Connection(Station station, double time, string line, HashSet<string> platform) 
            => (this.station, this.time, this.line, this.platform) = (station, time, line, platform);
    }
    public class Station
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
        public HashSet<string> getPlatformNumber(HashSet<string> lines)
        {
            HashSet<string> platforms = new HashSet<string>();
            platforms.UnionWith(Connections
                .Where(c => lines.Contains(c.line))
                .SelectMany(c => c.platform));
            return platforms;
        }
        public void ResetStation()
        {
            travelTime = int.MaxValue;
            previousStation = null;
            visited = false;
            previousLines.Clear();
        }
    }
    public class PathInstruction 
    { 
        public string instruction { get; set; }
        public bool isLine { get; set; }
        public PathInstruction(string instruction, bool isLine = false)
            => (this.instruction, this.isLine) = (instruction, isLine);
        private string NormalizeLineName(string line) =>
            line.Split('(')[0].Trim(); // e.g. "Central Line (Epping)" -> "Central Line
        private string ConvertColourRGB(ConsoleColor colour)
        {
            switch (colour)
            {
                case ConsoleColor.Black:
                    return "000000";
                case ConsoleColor.DarkBlue:
                    return "000080";
                case ConsoleColor.DarkGreen:
                    return "008000";
                case ConsoleColor.DarkCyan:
                    return "008080";
                case ConsoleColor.DarkRed:
                    return "800000";
                case ConsoleColor.DarkMagenta:
                    return "800080";
                case ConsoleColor.DarkYellow:
                    return "808000";
                case ConsoleColor.Gray:
                    return "C0C0C0";
                case ConsoleColor.DarkGray:
                    return "808080";
                case ConsoleColor.Blue:
                    return "0000FF";
                case ConsoleColor.Green:
                    return "00FF00";
                case ConsoleColor.Cyan:
                    return "00FFFF";
                case ConsoleColor.Red:
                    return "FF0000";
                case ConsoleColor.Magenta:
                    return "FF00FF";
                case ConsoleColor.Yellow:
                    return "FFFF00";
                default:
                    return "FFFFFF";
            }
        }
        public (string, string) GetRGBColour(Dictionary<string, (ConsoleColor, ConsoleColor)> lineColors)
        {
            string trimmed = NormalizeLineName(instruction);
            var (fg, bg) = lineColors[trimmed];
            return (ConvertColourRGB(fg), ConvertColourRGB(bg));
        }
        public void DisplaySegment(Dictionary<string, (ConsoleColor, ConsoleColor)> lineColors)
        {
            string trimmed = NormalizeLineName(instruction);
            if (isLine && lineColors.ContainsKey(trimmed))
            {
                var (fg, bg) = lineColors[trimmed];
                Console.ForegroundColor = fg;
                Console.BackgroundColor = bg;
                Console.Write(instruction);
                Console.ResetColor();
                Console.WriteLine();
            }
            else
                Console.WriteLine(instruction);
        }

    }
    public class DecomposedConnection
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