using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace NEA
{
    public struct Connection
    {
        public Station station;
        public int time;
        public string line;
    }
    public class Station 
    {
        public string name;
        public int zone;
        public List<Connection> Connections = new List<Connection>();
        public int time2Travel = int.MaxValue;
        public Station previousStation = null;
        public bool visited = false;
        public Station (string name) => this.name = name;
    }
}
