using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace XmlDbTest
{
    public partial class Form1 : Form
    {
        public string ConfigPath;
        public string DatabaseFilePath;
        public string EVEBotDatabaseFilePath;

        public XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<ScriptedMission>));

        public List<ScriptedMission> Missions = new List<ScriptedMission>();
        public Form1()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, EventArgs e)
        {
            LoadDatabase();
        }

        internal void LoadDatabase()
        {
            ConfigPath = Directory.GetCurrentDirectory();
            ConfigPath += "\\config\\";
            DatabaseFilePath = ConfigPath + "EVEBotMissionDatabase.xml";
            EVEBotDatabaseFilePath = @"C:\Program Files\InnerSpace\Scripts\evebot-test\Data\Mission Database.xml";

            if (Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }

            if (File.Exists(EVEBotDatabaseFilePath) && !File.Exists(DatabaseFilePath))
            {
                using (TextReader tr = new StreamReader(EVEBotDatabaseFilePath))
                {
                    //Create and load the document
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.Load(tr);
                    //Get a node list
                    XmlNodeList xNodeList = xdoc.SelectNodes("/InnerSpaceSettings/Set/Set");

                    //Sweet. Iterate it, spit out output.
                    foreach (XmlNode node in xNodeList)
                    {
                        ScriptedMission tempMission = new ScriptedMission();
                        tempMission.Name = node.Attributes["Name"].Value;
                        listBoxItems.Items.Add(tempMission.Name);
                        foreach (XmlNode levelNode in node.ChildNodes)
                        {
                            ScriptedMissionLevel tempLevel = new ScriptedMissionLevel(
                                Convert.ToInt32(levelNode.Attributes["Name"].Value));
                            listBoxItems.Items.Add("\t" + tempLevel.Level.ToString());
                            foreach (XmlNode commandOrAllowedShipsNode in levelNode.ChildNodes)
                            {
                                if (commandOrAllowedShipsNode.Attributes["Name"].Value == "Commands")
                                {
                                    foreach (XmlNode commandNode in commandOrAllowedShipsNode.ChildNodes)
                                    {
                                        ScriptedMissionCommand tempCommand = new ScriptedMissionCommand();
                                        try
                                        {
                                            tempCommand.Action = commandNode.Attributes["Action"].Value;
                                            switch (tempCommand.Action)
                                            {
                                                //Don't need to do anything for ClearRoom or NextRoom
                                                case "ClearRoom":
                                                    listBoxItems.Items.Add("\t\tClearRoom");
                                                    break;
                                                case "NextRoom":
                                                    listBoxItems.Items.Add("\t\tNextRoom");
                                                    break;
                                                //CheckContainers needs a Target, GroupID, and optionally WreckName
                                                case "CheckContainers":
                                                    tempCommand.Target = commandNode.Attributes["Target"].Value;
                                                    tempCommand.GroupID = Convert.ToInt32(commandNode.Attributes["GroupID"].Value);
                                                    tempCommand.WreckName = commandNode.Attributes["WreckName"].Value;
                                                    listBoxItems.Items.Add(String.Format("\t\t{0}, {1}, {2}, {3}",
                                                        tempCommand.Action, tempCommand.Target, tempCommand.GroupID.ToString(),
                                                        tempCommand.WreckName));
                                                    break;
                                                //ApproachBreakOnCombat needs a target and optional a distance
                                                case "ApproachBreakOnCombat":
                                                    tempCommand.Target = commandNode.Attributes["Target"].Value;
                                                    tempCommand.Distance = Convert.ToInt32(commandNode.Attributes["_distance"].Value);
                                                    listBoxItems.Items.Add(String.Format("\t\t{0}, {1}, {2}",
                                                        tempCommand.Action, tempCommand.Target, tempCommand.Distance.ToString()));
                                                    break;
                                                //WaitAggro needs a timeout
                                                case "WaitAggro":
                                                    tempCommand.TimeOut = Convert.ToInt32(commandNode.Attributes["TimeOut"].Value);
                                                    listBoxItems.Items.Add(String.Format("\t\t{0}, {1}",
                                                        tempCommand.Action, tempCommand.TimeOut.ToString()));
                                                    break;
                                                //Kill only needs a target
                                                case "Kill":
                                                    tempCommand.Target = commandNode.Attributes["Target"].Value;
                                                    listBoxItems.Items.Add(String.Format("\t\t{0}, {1}",
                                                        tempCommand.Action, tempCommand.Target));
                                                    break;
                                                //Approach needs a target and optionally a distance
                                                case "Approach":
                                                    tempCommand.Target = commandNode.Attributes["Target"].Value;
                                                    tempCommand.Distance = Convert.ToInt32(commandNode.Attributes["_distance"].Value);
                                                    listBoxItems.Items.Add(String.Format("\t\t{0}, {1}, {2}",
                                                        tempCommand.Action, tempCommand.Target, tempCommand.Distance.ToString()));
                                                    break;
                                                //Waves doesn't need anything either
                                                case "Waves":
                                                    listBoxItems.Items.Add("\t\tWaves");
                                                    break;
                                                //ActivateGat needs a target
                                                case "ActivateGate":
                                                    tempCommand.Target = commandNode.Attributes["Target"].Value;
                                                    listBoxItems.Items.Add(String.Format("\t\t{0}, {1}",
                                                        tempCommand.Action, tempCommand.Target)); 
                                                    break;
                                                //That should be everything
                                            }
                                            //Add the command
                                            tempLevel.Commands.Add(tempCommand);
                                        }
                                        catch (Exception) { }
                                    }
                                }
                                if (commandOrAllowedShipsNode.Attributes["Name"].Value == "AllowedShips")
                                {
                                    foreach (XmlNode shipNode in commandOrAllowedShipsNode.ChildNodes)
                                    {
                                            tempLevel.AllowedShipGroupIDs.Add(Convert.ToInt32(
                                                shipNode.InnerXml));
                                            listBoxItems.Items.Add(String.Format("\t\t{0}, {1}",
                                                shipNode.Attributes["Name"].Value, shipNode.InnerXml));
                                    }
                                }
                            }
                            //Add the level to the mission
                            tempMission.Levels.Add(tempLevel);
                        }
                        if (tempMission.Name != null && tempMission.Name != string.Empty)
                        {
                            Missions.Add(tempMission);
                        }
                    }
                }
            }
            else if (File.Exists(DatabaseFilePath))
            {
                using (TextReader tr = new StreamReader(DatabaseFilePath))
                {
					try
					{
						Missions = (List<ScriptedMission>)xmlSerializer.Deserialize(tr);
					}
					catch (InvalidCastException)
					{

					}
                }
            }
        }
    }

    //Cache missions we have and their sec status, last decline, etc
    public class MissionCache
    {
        internal List<CachedMission> Missions = new List<CachedMission>();
        internal string ConfigPath;
        internal string CacheFilePath;
        internal XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<CachedMission>));

        internal MissionCache()
        {

        }

        ~MissionCache()
        {
            if (CacheFilePath == null)
            {
                return;
            }
            using (TextWriter tw = new StreamWriter(CacheFilePath))
            {
                xmlSerializer.Serialize(tw, Missions);
            }
        }

        internal bool IsCachedMissionByAgentAvailable(int agentID)
        {
            foreach (CachedMission cm in Missions)
            {
                if (cm.AgentID == agentID)
                {
                    return true;
                }
            }
            return false;
        }

        internal void LoadCache(string charName)
        {
            ConfigPath = Directory.GetCurrentDirectory();
            ConfigPath += "\\config\\";
            CacheFilePath = ConfigPath + charName + " Mission Cache.xml";

            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }

            if (File.Exists(CacheFilePath))
            {
                using (TextReader tr = new StreamReader(CacheFilePath))
                {
					try
					{
						Missions = (List<CachedMission>)xmlSerializer.Deserialize(tr);
					}
					catch (InvalidCastException)
					{

					}
                }
            }
            else
            {
                Missions = new List<CachedMission>();
            }
        }
    }

    //Object to describe our cached mission
    public class CachedMission
    {
        public int AgentID;
        public string Name;
        public int FactionID;
        public int TypeID;
        public float CargoVolume;
        public bool IsLowSecurity;

        public CachedMission() { }
        internal CachedMission(string name, int agentID)
        {
            AgentID = agentID;
            Name = name;
        }
    }

    //Describe the objectives of our mission; tell us what to do
    public class ScriptedMission
    {
        [XmlElement("Name")]
        public string Name;
        [XmlElement("Levels")]
        public List<ScriptedMissionLevel> Levels;

        internal ScriptedMission()
        {
            Levels = new List<ScriptedMissionLevel>();
        }
    }

    public class ScriptedMissionLevel
    {
        [XmlAttribute("Level")]
        public int Level;
        [XmlElement("Commands")]
        public List<ScriptedMissionCommand> Commands;
        [XmlElement("AllowedShipGroupIDs")]
        public List<int> AllowedShipGroupIDs;

        public ScriptedMissionLevel()
        {

        }

        internal ScriptedMissionLevel(int level)
        {
            Level = level;
            Commands = new List<ScriptedMissionCommand>();
            AllowedShipGroupIDs = new List<int>();
        }
    }

    public class ScriptedMissionCommand
    {
        [XmlAttribute("Action")]
        public string Action;
        [XmlAttribute("Target")]
        public string Target;
        [XmlAttribute("GroupID")]
        public int GroupID;
        [XmlAttribute("_distance")]
        public double Distance;
        [XmlAttribute("TimeOut")]
        public int TimeOut;
        [XmlAttribute("WreckName")]
        public string WreckName;

        public ScriptedMissionCommand()
        {

        }

        internal ScriptedMissionCommand(string action, string target, int groupID, double distance)
        {
            Action = action;
            Target = target;
            GroupID = groupID;
            Distance = distance;
        }
    }
}
