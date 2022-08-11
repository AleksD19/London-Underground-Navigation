using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace LondonUnderground_Navigation
{
    public class Station
    {
        public Station Parent;
        public string StationName;
        public int stationId;
        public float xCoor;
        public float yCoor;
        public float fCost;
        public float gCost;
        public float HCost;

        public Station(int id, float x, float y, string name)
        {
            SetStationId(id);
            SetXCoor(x);
            SetYCoor(y);
            StationName = name;
        }

        public void SetStationId(int id)
        {
            stationId = id;
        }

        public void SetXCoor(float x)
        {
            xCoor = x;
        }
        public void SetYCoor(float y)
        {
            yCoor = y;
        }
    }


    class Program
    {

        public struct Connection
        {
            public Station station;
            public int time;
            public int line;

        }

        // Graph
        public static Dictionary<Station, List<Connection>> StationDictionary = new Dictionary<Station, List<Connection>>();
        // All possible Stations
        public static List<Station> AllStations = new List<Station>();
        //Stores the best path/Most efficient
        public static List<Station> WholePath = new List<Station>();


        static void Main(string[] args)
        {

            ReadLatLon();
            ReadConnections();

            //Stations 1-302
            //Change to string input
            Search(AllStations[1], AllStations[10]);


            foreach (Station item in WholePath)
            {
                //Print out the path from the chosen locations
                Console.WriteLine(item.StationName);
            }

            Console.ReadLine();
        }


        public static void ReadLatLon()
        {
            using (var reader = new StreamReader(@"StationFiles\london.stations.csv"))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    Station newStat = new Station(int.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), values[3]);

                    AllStations.Add(newStat);

                }
            }
        }

        public static Station ReturnStation(int stationID)
        {
            foreach (Station item in AllStations)
            {
                if (item.stationId == stationID)
                {
                    // Returns a station object corresponding to the station Id
                    return item;
                }
            }
            return null;

        }

        public static void ReadConnections()
        {
            using (var reader1 = new StreamReader(@"StationFiles\london.connections.csv"))
            {
                while (!reader1.EndOfStream)
                {
                    //Holds a line from the csv file
                    var line = reader1.ReadLine();
                    // Seperates the line into values at delimiter
                    var values = line.Split(',');


                    foreach (Station item in AllStations)
                    {
                        // Linking 1st station to second
                        if (int.Parse(values[0]) == item.stationId)
                        {
                            // Check if key is already contained in Dictionary
                            if (StationDictionary.ContainsKey(item))
                            {
                                //Only the connection is added to the key
                                Connection con;
                                con.station = ReturnStation(int.Parse(values[1]));
                                con.time = int.Parse(values[3]);
                                con.line = int.Parse(values[2]);
                                StationDictionary[item].Add(con);

                            }
                            else
                            {
                                // If key doesnt exist new key is created and connection are added.
                                List<Connection> conList = new List<Connection>();
                                Connection con1;
                                con1.station = ReturnStation(int.Parse(values[1]));
                                con1.time = int.Parse(values[3]);
                                con1.line = int.Parse(values[2]);
                                conList.Add(con1);
                                StationDictionary.Add(item, conList);
                            }
                        }
                        //Linking 2nd station to first
                        if (int.Parse(values[1]) == item.stationId)
                        {

                            if (StationDictionary.ContainsKey(item))
                            {
                                Connection con2;
                                con2.station = ReturnStation(int.Parse(values[0]));
                                con2.time = int.Parse(values[3]);
                                con2.line = int.Parse(values[2]);
                                StationDictionary[item].Add(con2);
                            }
                            else
                            {
                                List<Connection> conList1 = new List<Connection>();
                                Connection con3;

                                con3.station = ReturnStation(int.Parse(values[0]));
                                con3.time = int.Parse(values[3]);
                                con3.line = int.Parse(values[2]);
                                conList1.Add(con3);
                                StationDictionary.Add(item, conList1);


                            }

                        }
                    }
                }
            }
        }

        //............................ //Searching  Algorithm  Starts From Here//..............................................

        public static float SetHeuristic(Station current, Station end)
        {
            float Xdiff = Math.Abs(current.xCoor - end.xCoor);
            float Ydiff = Math.Abs(current.yCoor - end.yCoor);
            float Distance = (float)Math.Sqrt((Xdiff * Xdiff) + (Ydiff * Ydiff));
            return Distance; // Correct Scale to Be Added.
        }

        public static void ReconstructPath(Station current, Station start)
        {
            // Find the path
            while (current != null)
            {
                WholePath.Add(current);
                if (current == start)
                {
                    break;
                }
                current = current.Parent;
            }
        }

        public static void Search(Station start, Station end)
        {

            List<Station> OpenList = new List<Station>();
            List<Station> ClosedList = new List<Station>();

            OpenList.Add(start);
            start.gCost = 0;
            start.HCost = 0;
            start.fCost = 0;

            while (OpenList.Count > 0)
            {
                Station currentStation = GetSmallestFCostStation(OpenList);

                if (currentStation == end)
                {
                    ReconstructPath(currentStation, start);
                    WholePath.Reverse();

                    break;
                }

                OpenList.Remove(currentStation);
                ClosedList.Add(currentStation);

                List<Connection> Neighbours = new List<Connection>();
                GetAllNeighbours(currentStation, Neighbours);

                foreach (Connection neighbour in Neighbours)
                {

                    if (ClosedList.Contains(neighbour.station))
                    {
                        //ignore      
                        continue;
                    }

                    float tempGScore = currentStation.gCost + neighbour.time;

                    if (OpenList.Contains(neighbour.station) == false)
                    {
                        OpenList.Add(neighbour.station);
                    }
                    else if (tempGScore >= neighbour.station.gCost)
                    {
                        //Bad option ignore
                        continue;
                    }

                    neighbour.station.Parent = currentStation;
                    neighbour.station.gCost = tempGScore;
                    neighbour.station.fCost = neighbour.station.gCost + SetHeuristic(neighbour.station, end);
                }
            }
        }

        public static void GetAllNeighbours(Station current, List<Connection> neighbourList)
        {
            foreach (Connection con in StationDictionary[current])
            {
                //Returns all neighbours of a node
                neighbourList.Add(con);
            }
        }

        public static Station GetSmallestFCostStation(List<Station> openlist)
        {
            Station lowestFcostStation = openlist[0];
            foreach (Station stat in openlist)
            {
                if (stat.fCost < lowestFcostStation.fCost)
                {
                    //Finds the station with the lowest FCost from a list
                    lowestFcostStation = stat;
                }
            }
            return lowestFcostStation;
        }
    }

}
