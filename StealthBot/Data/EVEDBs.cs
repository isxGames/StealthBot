using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using ProtoBuf;
using StealthBot.Core;
using StealthBot.Core.Interfaces;

namespace StealthBot.Data
{
    //Provide a means of accessing the EVE DBs.
	[ProtoContract()]
    internal class EVEDBs
    {
        private string ConfigPath;
        private string EVEDBSolarSystemsPath;
        private string EVEDBBountiesPath;

		[ProtoMember(1)]
        public List<SolarSystem> SolarSystems;
		[ProtoMember(2)]
        public List<RatBountyPair> Bounties;

	    private readonly ILogging _logging;

        private EVEDBs(ILogging logging)
        {
            _logging = logging;
            ConfigPath = Directory.GetCurrentDirectory();
            ConfigPath += "\\config\\";

            //Load the dbs
            LoadEveDbSpawns();
            Load_EVEDB_SolarSystems();
        }

        internal void LoadEveDbSpawns()
        {
            var OldEVEDBSpawnsPath = @"C:\Program Files\InnerSpace\Scripts\evebot-test\Data\EVEDB_Spawns.xml";
            EVEDBBountiesPath = ConfigPath + "EVEDB_Bounties.bin";
            //XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<RatBountyPair>));

            if (!File.Exists(EVEDBBountiesPath) && File.Exists(OldEVEDBSpawnsPath))
            {
                using (TextReader tr = new StreamReader(OldEVEDBSpawnsPath))
                {
                    var xdoc = new XmlDocument();
                    try
                    {
                        xdoc.Load(tr);
                    }
                    catch (XmlException xe)
                    {
                        _logging.LogException("EVEDBs", xe, "LoadEveDbSpawns", "Caught exception while loading EVEBot Spawns database: ");
                        return;
                    }

                    var xNodeList = xdoc.SelectNodes("/InnerSpaceSettings/Set/Set");

                    foreach (XmlNode xNode in xNodeList)
                    {
                        RatBountyPair tempPair = new RatBountyPair();
                        tempPair.Name = xNode.Attributes["Name"].Value;
                        tempPair.Bounty = Convert.ToInt32(
                            xNode.ChildNodes[0].InnerXml);
                        Bounties.Add(tempPair);
                    }
                }
            }
            else if (File.Exists(EVEDBBountiesPath))
            {
                using (Stream tr = File.Open(EVEDBBountiesPath, FileMode.OpenOrCreate))
                {
					//Try/catch block as a dirtyhack to fix a smartassembly crash
					//Bounties = (List<RatBountyPair>)xmlSerializer.Deserialize(tr);
					Bounties = Serializer.Deserialize<List<RatBountyPair>>(tr);
                }
            }
        }

        internal void Load_EVEDB_SolarSystems()
        {
            var methodName = "Load_EVEDB_SolarSystems";

            string OldEVEDBSolarSystemsPath = @"C:\Program Files\InnerSpace\Scripts\evebot-test\Data\EVEDB_Stations.xml";
            EVEDBSolarSystemsPath = ConfigPath + "EVEDB_SolarSystems.bin";
            //XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<SolarSystem>));

            //If we don't have the new db, we're gonna have to make it from the old one.
            if (!File.Exists(EVEDBSolarSystemsPath) && File.Exists(OldEVEDBSolarSystemsPath))
            {
                using (TextReader tr = new StreamReader(OldEVEDBSolarSystemsPath))
                {
                    SolarSystems = new List<SolarSystem>();

                    //Create and load the xdoc
                    var xdoc = new XmlDocument();

                    try
                    {
                        xdoc.Load(tr);
                    }
                    catch (XmlException xe)
                    {
                        _logging.LogException("EVEDBs", xe, methodName, "Caught exception while loading EVEBot Stations database:");
                        return;
                    }

                    //Get a list of nodes in the document.
                    var xNodeList = xdoc.SelectNodes("/InnerSpaceSettings/Set/Set");

                    //Start iterating.
                    //The attribute Name of the outer node is the station ID.
                        //The element with attribute Name=solarSystemID is the Solar System ID.
                        //The element with the attribute Name="stationName" is the station name.
                    foreach (XmlNode xNode in xNodeList)
                    {
                        //Since this shit is so backwards, we need to get a temp Station from the stationID.
                        //Later we'll add the nameto the station and check it against solar systems.
                        Station tempStation = new Station();
                        tempStation.ID = Convert.ToInt32(xNode.Attributes["Name"].Value);
                        SolarSystem tempSolarSystem = new SolarSystem();

                        //Iterate the child nodes of the current node. One of them is the station name, the other
                        //is the solarSystemID.
                        foreach (XmlNode xChildNode in xNode.ChildNodes)
                        {
                            //See if it's SolarSystemID or StationName
                            if (xChildNode.Attributes["Name"].Value == "solarSystemID")
                            {
                                tempSolarSystem.SolarSystemID = Convert.ToInt32(xChildNode.InnerXml);
                            }
                            if (xChildNode.Attributes["Name"].Value == "stationName")
                            {
                                tempStation.Name = xChildNode.InnerXml;
                            }
                        }
                        //Ok, we have tempSolarSystem and tempStation fully set. See if we need to add a new solar system,
                        //or if we just need to add Station to an existing system.
                        int tempSolarSystemIndex = ContainsSolarSystem(tempSolarSystem.SolarSystemID);
                        if (tempSolarSystemIndex >= 0)
                        {
                            //We already contain a solar system with that ID. Add to it.
                            SolarSystems[tempSolarSystemIndex].Stations.Add(tempStation);
                        }
                        else
                        {
                            //Well shit. Add te solar system, after adding the staion to the solar system.
                            tempSolarSystem.Stations.Add(tempStation);
                            SolarSystems.Add(tempSolarSystem);
                        }
                    }
                }
            }
            else if (File.Exists(EVEDBSolarSystemsPath))
            {
                //Thank god, just deserialize the db.
                using (Stream tr = File.Open(EVEDBSolarSystemsPath, FileMode.OpenOrCreate))
                {
					//SolarSystems = xmlSerializer.Deserialize(tr) as List<SolarSystem>;
					SolarSystems = Serializer.Deserialize<List<SolarSystem>>(tr);
                }
            }
        }

        //Determine if we aready have an entry for the solar system. If we do, return its index. If not, return -1.
        internal int ContainsSolarSystem(int solarSystemID)
        {
            int idx = -1;
            foreach (SolarSystem ss in SolarSystems)
            {
                idx++;
                if (ss.SolarSystemID == solarSystemID)
                {
                    return idx;
                }
            }
            return -1;
        }
    }

    //Hold all the stations and stationIDs by solar system
	[ProtoContract()]
    public class SolarSystem
    {
        //[XmlAttribute("SolarSystemID")]
		[ProtoMember(1)]
        public int SolarSystemID;
        //[XmlElement("Stations")]
		[ProtoMember(2)]
        public List<Station> Stations = new List<Station>();

        public SolarSystem() { }

        internal SolarSystem(int solarSystemID)
        {
            SolarSystemID = solarSystemID;
        }
    }

    //Hold a station's name and ID
	[ProtoContract()]
    public class Station
    {
        //[XmlAttribute("Name")]
		[ProtoMember(1)]
        public string Name;
        //[XmlAttribute("ID")]
		[ProtoMember(2)]
        public int ID;

        public Station() { }

        internal Station(string name, int id)
        {
            Name = name;
            ID = id;
        }
    }

    //Describe a rat/bounty pair
	[ProtoContract()]
    public class RatBountyPair
    {
        //[XmlAttribute("Name")]
		[ProtoMember(1)]
        public string Name;
        //[XmlAttribute("Bounty")]
		[ProtoMember(2)]
        public int Bounty;

        public RatBountyPair() { }

        internal RatBountyPair(string ratName, int bounty)
        {
            Name = ratName;
            Bounty = bounty;
        }
    }
}
