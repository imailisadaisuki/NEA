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
        static void InsertZone1Station()
        {
            Station Aldgate = new Station("Aldgate", 1);
            Station AldgateEast = new Station("Aldgate East", 1);
            Station Angel = new Station("Angel", 1);
            Station BakerStreet = new Station("Baker Street", 1);
            Station Bank = new Station("Bank", 1);
            Station Barbican = new Station("Barbican", 1);
            Station BatterseaPowerStation = new Station("Battersea Power Station", 1);
            Station Bayswater = new Station("Bayswater", 1);
            Station Blackfriars = new Station("Blackfriars", 1);
            Station BondStreet = new Station("Bond Street", 1);
            Station Borough = new Station("Borough", 1);
            Station CannonStreet = new Station("Cannon Street", 1);
            Station ChanceryLane = new Station("Chancery Lane", 1);
            Station CharingCross = new Station("Charing Cross", 1);
            Station CoventGarden = new Station("Covent Garden", 1);
            Station EarlsCourt = new Station("Earl's Court", 1); //Also Zone 2
            Station EdgwareRoadB = new Station("Edgware Road (Bakerloo)", 1);
            Station EdgwareRoadCHC = new Station("Edgware Road (Circle and Hammersmith & City)", 1);
            Station ElephantAndCastle = new Station("Elephant & Castle", 1); //Also Zone 2
            Station Embankment = new Station("Embankment", 1);
            Station Euston = new Station("Euston", 1);
            Station EustonSquare = new Station("Euston Square", 1);
            Station Farringdon = new Station("Farringdon", 1);
            Station GloucesterRoad = new Station("Gloucester Road", 1);
            Station GoodgeStreet = new Station("Goodge Street", 1);
            Station GreatPortlandStreet = new Station("Great Portland Street", 1);
            Station GreenPark = new Station("Green Park", 1);
            Station HighStreetKensington = new Station("High Street Kensington", 1);
            Station Holborn = new Station("Holborn", 1);
            Station HydeParkCorner = new Station("Hyde Park Corner", 1);
            Station Kennington = new Station("Kennington", 1); //Also Zone 2
            Station KingsCross = new Station("King's Cross & St Pancras International", 1);
            Station Knightsbridge = new Station("Knightsbridge", 1);
            Station LambethNorth = new Station("Lambeth North", 1);
            Station LancasterGate = new Station("Lancaster Gate", 1);
            Station LeicesterSquare = new Station("Leicester Square", 1);
            Station LiverpoolStreet = new Station("Liverpool Street", 1);
            Station LondonBridge = new Station("London Bridge", 1);
            Station MansionHouse = new Station("Mansion House", 1);
            Station MarbleArch = new Station("Marble Arch", 1);
            Station Marylebone = new Station("Marylebone", 1);
            Station Monument = new Station("Monument", 1);
            Station Moorgate = new Station("Moorgate", 1);
            Station NineElms = new Station("Nine Elms", 1);
            Station NottingHillGate = new Station("Notting Hill Gate", 1); //Also Zone 2
            Station OldStreet = new Station("Old Street", 1);
            Station OxfordCircus = new Station("Oxford Circus", 1);
            Station PaddingtonBCD = new Station("Paddington (Bakerloo, Circle and District)", 1);
            Station PaddingtonCHC = new Station("Paddington (Circle and Hammersmith & City)", 1);
            Station PiccadillyCircus = new Station("Piccadilly Circus", 1);
            Station Pimlico = new Station("Pimlico", 1);
            Station Queensway = new Station("Queensway", 1);
            Station RegentsPark = new Station("Regent's Park", 1);
            Station RussellSquare = new Station("Russell Square", 1);
            Station StPauls = new Station("St. Paul's", 1);
            Station SouthKensington = new Station("South Kensington", 1);
            Station Southwark = new Station("Southwark", 1);
            Station StJamesPark = new Station("St. James's Park", 1);
            Station SloaneSquare = new Station("Sloane Square", 1);
            Station Temple = new Station("Temple", 1);
            Station TottenhamCourtRoad = new Station("Tottenham Court Road", 1);
            Station TowerHill = new Station("Tower Hill", 1);
            Station TowerGateway = new Station("Tower Gateway", 1); //DLR
            Station Vauxhall = new Station("Vauxhall", 1); //Also Zone 2
            Station Victoria = new Station("Victoria", 1);
            Station WarrenStreet = new Station("Warren Street", 1);
            Station Waterloo = new Station("Waterloo", 1);
            Station Westminster = new Station("Westminster", 1);
            //add connections, source: https://tfl.gov.uk/corporate/publications-and-reports/working-timetables
            //adding Bakerloo
            string line = "Bakerloo";
            AddConnection(PaddingtonBCD, EdgwareRoadB, 2, line);
            AddConnection(EdgwareRoadB, Marylebone, 1, line);
            AddConnection(Marylebone, BakerStreet, 1, line);
            AddConnection(BakerStreet, RegentsPark, 2, line);
            AddConnection(RegentsPark, OxfordCircus, 2, line);
            AddConnection(OxfordCircus, PiccadillyCircus, 2, line);
            AddConnection(PiccadillyCircus, CharingCross, 2, line);
            AddConnection(CharingCross, Embankment, 1, line);
            AddConnection(Embankment, Waterloo, 2, line);
            AddConnection(Waterloo, LambethNorth, 2, line);
            AddConnection(LambethNorth, ElephantAndCastle, 2, line);

            //adding Central
            line = "Central";
            AddConnection(LiverpoolStreet, Bank, 2, line);
            AddConnection(Bank, StPauls, 2, line);
            AddConnection(StPauls, ChanceryLane, 2, line);
            AddConnection(ChanceryLane, Holborn, 1, line);
            AddConnection(Holborn, TottenhamCourtRoad, 2, line);
            AddConnection(TottenhamCourtRoad, OxfordCircus, 1, line);
            AddConnection(OxfordCircus, BondStreet, 2, line);
            AddConnection(BondStreet, MarbleArch, 1, line);
            AddConnection(MarbleArch, LancasterGate, 2, line);
            AddConnection(LancasterGate, Queensway, 2, line);
            AddConnection(Queensway, NottingHillGate, 2, line);

            //adding Circle
            line = "Circle";
            AddConnection(EdgwareRoadCHC, PaddingtonBCD, 2, line);
            AddConnection(PaddingtonBCD, Bayswater, 2, line);
            AddConnection(Bayswater, NottingHillGate, 2, line);
            AddConnection(NottingHillGate, HighStreetKensington, 2, line);
            AddConnection(HighStreetKensington, GloucesterRoad, 2, line);
            AddConnection(GloucesterRoad, SouthKensington, 2, line);
            AddConnection(SouthKensington, SloaneSquare, 2, line);
            AddConnection(SloaneSquare, Victoria, 2, line);
            AddConnection(Victoria, StJamesPark, 2, line);
            AddConnection(StJamesPark, Westminster, 2, line);
            AddConnection(Westminster, Embankment, 2, line);
            AddConnection(Embankment, Temple, 2, line);
            AddConnection(Temple, Blackfriars, 1, line);
            AddConnection(Blackfriars, MansionHouse, 2, line);
            AddConnection(MansionHouse, CannonStreet, 1, line);
            AddConnection(CannonStreet, Monument, 1, line);
            AddConnection(Monument, TowerHill, 2, line);
            AddConnection(TowerHill, Aldgate, 2, line);
            AddConnection(Aldgate, LiverpoolStreet, 2, line);
            AddConnection(LiverpoolStreet, Moorgate, 2, line);
            AddConnection(Moorgate, Barbican, 2, line);
            AddConnection(Barbican, Farringdon, 1, line);
            AddConnection(Farringdon, KingsCross, 3, line);
            AddConnection(KingsCross, EustonSquare, 2, line);
            AddConnection(EustonSquare, GreatPortlandStreet, 1, line);
            AddConnection(GreatPortlandStreet, BakerStreet, 2, line);
            AddConnection(BakerStreet, EdgwareRoadCHC, 2, line);
            AddConnection(EdgwareRoadCHC, PaddingtonCHC, 2, line);

            //add Hammersmith & City
            line = "Hammersmith & City Line";
            AddConnection(AldgateEast, LiverpoolStreet, 2, line);
            AddConnection(LiverpoolStreet, Moorgate, 2, line);
            AddConnection(Moorgate, Barbican, 2, line);
            AddConnection(Barbican, Farringdon, 1, line);
            AddConnection(Farringdon, KingsCross, 3, line);
            AddConnection(KingsCross, EustonSquare, 2, line);
            AddConnection(EustonSquare, GreatPortlandStreet, 1, line);
            AddConnection(GreatPortlandStreet, BakerStreet, 2, line);
            AddConnection(BakerStreet, EdgwareRoadCHC, 2, line);
            AddConnection(EdgwareRoadCHC, PaddingtonCHC, 2, line);

            //add District
            line = "District";
            AddConnection(AldgateEast, TowerHill, 2, line);
            AddConnection(TowerHill, Monument, 2, line);
            AddConnection(Monument, CannonStreet, 1, line);
            AddConnection(CannonStreet, MansionHouse, 1, line);
            AddConnection(MansionHouse, Blackfriars, 2, line);
            AddConnection(Blackfriars, Temple, 2, line);
            AddConnection(Temple, Embankment, 2, line); 
            AddConnection(Embankment, Westminster, 2, line);
            AddConnection(Westminster, StJamesPark, 2, line);
            AddConnection(StJamesPark, Victoria, 2, line);
            AddConnection(Victoria, SloaneSquare, 2, line);
            AddConnection(SloaneSquare, SouthKensington, 2, line);
            AddConnection(SouthKensington, GloucesterRoad, 2, line);
            AddConnection(GloucesterRoad, EarlsCourt, 2, line);
            //branch
            AddConnection(EdgwareRoadCHC, PaddingtonBCD, 2, line);
            AddConnection(PaddingtonBCD, Bayswater, 2, line);
            AddConnection(Bayswater, NottingHillGate, 2, line);
            AddConnection(NottingHillGate, HighStreetKensington, 2, line);
            AddConnection(HighStreetKensington, EarlsCourt, 3, line);

            //add Jubilee
            line = "Jubilee";
            AddConnection(BakerStreet, BondStreet, 2, line);
            AddConnection(BondStreet, GreenPark, 2, line);
            AddConnection(GreenPark, Westminster, 2, line);
            AddConnection(Westminster, Waterloo, 2, line);
            AddConnection(Waterloo, Southwark, 1, line);
            AddConnection(Southwark, LondonBridge, 2, line);

            //add Metropolitan
            line = "Metropolitan";
            AddConnection(Aldgate, LiverpoolStreet, 2, line);
            AddConnection(LiverpoolStreet, Moorgate, 2, line);
            AddConnection(Moorgate, Barbican, 2, line);
            AddConnection(Barbican, Farringdon, 1, line);
            AddConnection(Farringdon, KingsCross, 3, line);
            AddConnection(KingsCross, EustonSquare, 2, line);
            AddConnection(EustonSquare, GreatPortlandStreet, 1, line);
            AddConnection(GreatPortlandStreet, BakerStreet, 2, line);

            //add Northern
            //add Northern (Charing Cross branch)
            line = "Northern (Charing Cross branch)";
            AddConnection(Euston, WarrenStreet, 1, line);
            AddConnection(WarrenStreet, GoodgeStreet, 1, line);
            AddConnection(GoodgeStreet, TottenhamCourtRoad, 1, line);
            AddConnection(TottenhamCourtRoad, LeicesterSquare, 1, line);
            AddConnection(LeicesterSquare, CharingCross, 2, line);
            AddConnection(CharingCross, Embankment, 1, line);
            AddConnection(Embankment, Waterloo, 2, line);
            AddConnection(Waterloo, Kennington, 3, line);
            AddConnection(Kennington, NineElms, 2, line);
            AddConnection(NineElms, BatterseaPowerStation, 2, line);
            //add Northern (Bank branch)
            line = "Northern (Bank branch)";
            AddConnection(Euston, KingsCross, 2, line);
            AddConnection(KingsCross, Angel, 2, line);
            AddConnection(Angel, OldStreet, 2, line);
            AddConnection(OldStreet, Moorgate, 2, line);
            AddConnection(Moorgate, Bank, 2, line);
            AddConnection(Bank, LondonBridge, 2, line);
            AddConnection(LondonBridge, Borough, 2, line);
            AddConnection(Borough, ElephantAndCastle, 2, line);
            AddConnection(ElephantAndCastle, Kennington, 2, line);

            //add Piccadilly
            line = "Piccadilly";
            AddConnection(KingsCross, RussellSquare, 2, line);
            AddConnection(RussellSquare, Holborn, 2, line);
            AddConnection(Holborn, CoventGarden, 2, line);
            AddConnection(CoventGarden, LeicesterSquare, 1, line);
            AddConnection(LeicesterSquare, PiccadillyCircus, 2, line);
            AddConnection(PiccadillyCircus, GreenPark, 2, line);
            AddConnection(GreenPark, HydeParkCorner, 2, line);
            AddConnection(HydeParkCorner, Knightsbridge, 2, line);
            AddConnection(Knightsbridge, SouthKensington, 3, line);
            AddConnection(SouthKensington, GloucesterRoad, 2, line);
            AddConnection(GloucesterRoad, EarlsCourt, 2, line);

            //add Victoria
            line = "Victoria";
            AddConnection(KingsCross, Euston, 2, line);
            AddConnection(Euston, WarrenStreet, 1, line);
            AddConnection(WarrenStreet, OxfordCircus, 2, line);
            AddConnection(OxfordCircus, GreenPark, 2, line);
            AddConnection(GreenPark, Victoria, 2, line);
            AddConnection(Victoria, Pimlico, 2, line);
            AddConnection(Pimlico, Vauxhall, 1, line);

            //add Waterloo & City
            line = "Waterloo & City (Saturday service not operating)";
            AddConnection(Waterloo, Bank, 4, line);

            //add Elizabeth
            Station PaddingtonE = new Station("Paddington (Elizabeth)", 1); // Elizabeth Line station
            Station BondStreetE = new Station("Bond Street (Elizabeth)", 1);
            Station TottenhamCourtRoadE = new Station("Tottenham Court Road (Elizabeth)", 1);
            Station FarringdonE = new Station("Farringdon (Elizabeth)", 1);
            Station LiverpoolStreetE = new Station("Liverpool Street (Elizabeth)", 1);
            //add Elizebeth (Paddigton / Heathrow Terminal 5 to Shenfield)
            string line1 = "Elizabeth (towards Shenfield)";
            string line2 = "Elizabeth (towards Paddington)";
            string line3 = "Elizabeth (towards Heathrow Terminal 5)";
            PaddingtonE.Connections.Add(new Connection(BondStreetE, 4, line1));
            BondStreetE.Connections.Add(new Connection(PaddingtonE, 4, line2));
            BondStreetE.Connections.Add(new Connection(PaddingtonE, 4, line3));
            BondStreetE.Connections.Add(new Connection(TottenhamCourtRoadE, 2, line1));
            TottenhamCourtRoadE.Connections.Add(new Connection(BondStreetE, 2, line2));
            TottenhamCourtRoadE.Connections.Add(new Connection(BondStreetE, 2, line3));
            TottenhamCourtRoadE.Connections.Add(new Connection(FarringdonE, 3, line1));
            FarringdonE.Connections.Add(new Connection(TottenhamCourtRoadE, 3, line2));
            FarringdonE.Connections.Add(new Connection(TottenhamCourtRoadE, 3, line3));
            FarringdonE.Connections.Add(new Connection(LiverpoolStreetE, 2, line1));
            LiverpoolStreetE.Connections.Add(new Connection(FarringdonE, 2, line2));
            LiverpoolStreetE.Connections.Add(new Connection(FarringdonE, 2, line3));
            //add Elizabeth (Reading / Maidenhead / Heathrow Terminal 4 to Abbey Wood)
            line1 = "Elizabeth (towards Abbey Wood)";
            line2 = "Elizabeth (towards Reading)";
            line3 = "Elizabeth (towards Maidenhead)";
            string line4 = "Elizabeth (towards Heathrow Terminal 4)";
            PaddingtonE.Connections.Add(new Connection(BondStreetE, 4, line1));
            BondStreetE.Connections.Add(new Connection(PaddingtonE, 4, line2));
            BondStreetE.Connections.Add(new Connection(PaddingtonE, 4, line3));
            BondStreetE.Connections.Add(new Connection(PaddingtonE, 4, line4));
            BondStreetE.Connections.Add(new Connection(TottenhamCourtRoadE, 2, line1));
            TottenhamCourtRoadE.Connections.Add(new Connection(BondStreetE, 2, line2));
            TottenhamCourtRoadE.Connections.Add(new Connection(BondStreetE, 2, line3));
            TottenhamCourtRoadE.Connections.Add(new Connection(BondStreetE, 2, line4));
            TottenhamCourtRoadE.Connections.Add(new Connection(FarringdonE, 3, line1));
            FarringdonE.Connections.Add(new Connection(TottenhamCourtRoadE, 3, line2));
            FarringdonE.Connections.Add(new Connection(TottenhamCourtRoadE, 3, line3));
            FarringdonE.Connections.Add(new Connection(TottenhamCourtRoadE, 3, line4));
            FarringdonE.Connections.Add(new Connection(LiverpoolStreetE, 2, line1));
            LiverpoolStreetE.Connections.Add(new Connection(FarringdonE, 2, line2));
            LiverpoolStreetE.Connections.Add(new Connection(FarringdonE, 2, line3));
            LiverpoolStreetE.Connections.Add(new Connection(FarringdonE, 2, line4));

            //add other connections (walk)
            line = "Walk";
            AddConnection(Euston, EustonSquare, 1, line);
            AddConnection(EdgwareRoadB, EdgwareRoadCHC, 1, line);
            AddConnection(PaddingtonBCD, PaddingtonCHC, 1, line);
            AddConnection(PaddingtonCHC, PaddingtonE, 1, line);
            AddConnection(PaddingtonBCD, PaddingtonE, 1, line);
            AddConnection(BondStreet, BondStreetE, 1, line);
            AddConnection(TottenhamCourtRoad, TottenhamCourtRoadE, 1, line);
            AddConnection(Farringdon, FarringdonE, 1, line);
            AddConnection(LiverpoolStreet, LiverpoolStreetE, 1, line);

            Dijkstra(PaddingtonE);
            GetShortestPath(LiverpoolStreetE).ForEach(Console.WriteLine);
            Console.WriteLine($"Estimated time = {LiverpoolStreetE.time2Travel} minutes");
            Console.ReadLine();
        }

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
                    if (!current.getPrevConnectionLine().Contains(connection.line) && current.previousStation != null)
                    {
                        newTime += 6; // Add transfer time if changing lines
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
            List<string> connectionLine = current.getPrevConnectionLine();
            string line = string.Join("/", connectionLine);
            while (current.previousStation != null)
            {
                for (int i = 0; i < connectionLine.Count; i++)
                {
                    if (!current.getPrevConnectionLine().Contains(connectionLine[i]))
                    {
                        if (connectionLine.Count == 1) line = connectionLine[0];
                        connectionLine.Remove(connectionLine[i]); // remove the line if it is not the previous connection line
                        i++;
                    }
                }
                if (connectionLine.Count != 0)
                    path.Add(current.name); // did not change lines
                else
                {
                    if (line == "Walk") // if walking, do not add the line
                        path.Add($"Walk to {current.name}");
                    else
                        path.Add($"Transfer to {line} in {current.name}");
                    connectionLine = current.getPrevConnectionLine(); // changes line
                    line = string.Join("/", connectionLine);
                }
                current = current.previousStation;
            }
            path.Add($"Take the {line} Line from {current.name}"); // add the starting station
            path.Reverse();
            return path;
        }
        static void Main(string[] args)
        {
            InsertZone1Station();
            
        }
    }
}
