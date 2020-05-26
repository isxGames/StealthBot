using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Serialization;

namespace GetAllianceCorpXmlDB
{
    public partial class MainForm : Form
    {
        public static XmlSerializer corporationCacheSerializer = new XmlSerializer(typeof(List<CachedCorporation>));
        public static XmlSerializer allianceCacheSerializer = new XmlSerializer(typeof(List<CachedAlliance>));
        public static List<CachedCorporation> CorporationCache = new List<CachedCorporation>();
        public static List<CachedAlliance> AllianceCache = new List<CachedAlliance>();
        public static bool corpBuildCanRun = false;
        public static List<int> CorporationIDs = new List<int>();
        public static event EventHandler AllianceRecordProcessed;
        public static EventHandler Handler_MainForm_AllianceRecordProcessed;
        public static EventHandler Handler_MainForm_CorpRecordProcessed;
        public static int TotalAllianceRecords = 0;
        public int AllianceRecordsProcessed = 0;
        public static event EventHandler CorpRecordProcessed;
        public static int TotalCorpRecords = 0;
        public int CorpRecordsProcessed = 0;
        private static MainForm _instance;

        public MainForm()
        {
            InitializeComponent();
            backgroundWorker1.RunWorkerAsync();
            backgroundWorker2.RunWorkerAsync();
            _instance = this;
            Handler_MainForm_AllianceRecordProcessed = new EventHandler(MainForm_AllianceRecordProcessed);
            Handler_MainForm_CorpRecordProcessed = new EventHandler(MainForm_CorpRecordProcessed);
            AllianceRecordProcessed += Handler_MainForm_AllianceRecordProcessed;
            CorpRecordProcessed += Handler_MainForm_CorpRecordProcessed;
        }

        void MainForm_CorpRecordProcessed(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(Handler_MainForm_CorpRecordProcessed, sender, e);
            else
            {
                CorpRecordsProcessed++;
                corporationDatabaseLabel.Text = string.Format("{0}/{1}", CorpRecordsProcessed, TotalCorpRecords);
            }
        }

        void MainForm_AllianceRecordProcessed(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(Handler_MainForm_AllianceRecordProcessed, sender, e);
            else
            {
                AllianceRecordsProcessed++;
                allianceDatabaseLabel.Text = string.Format("{0}/{1}", AllianceRecordsProcessed, TotalAllianceRecords);
            }
        }

        private static void OnAllianceRecordProcessed()
        {
            if (AllianceRecordProcessed != null)
                AllianceRecordProcessed(_instance, new EventArgs());
        }

        private static void OnCorpRecordProcessed()
        {
            if (CorpRecordProcessed != null)
                CorpRecordProcessed(_instance, new EventArgs());
        }

        public static void GetAllianceDatabase()
        {
            WebClient a_db_Download_Client = new WebClient();
            Stream a_db_stream = a_db_Download_Client.OpenRead(
                "http://api.eve.ablairwhm.info/eve/AllianceList.xml.aspx");

            string filepath = String.Format("{0}\\{1}",
                Directory.GetCurrentDirectory(), "EVEDB_Alliances.xml");

            if (!File.Exists(filepath))
            {
                File.Create(filepath);
            }
            else
            {
                using (TextReader tr = new StreamReader(filepath))
                {
					try
					{
						AllianceCache = (List<CachedAlliance>)allianceCacheSerializer.Deserialize(tr);
					}
					catch (InvalidCastException)
					{

					}
                }
            }

            using (StreamReader sr = new StreamReader(a_db_stream))
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(sr);

                XmlNodeList xNodeList = xDoc.SelectNodes("/eveapi/result/rowset/row");
                TotalAllianceRecords = xNodeList.Count;
                foreach (XmlNode xNode1 in xNodeList)
                {
                    CachedAlliance tempCachedAlliance = new CachedAlliance();
                    tempCachedAlliance.AllianceID = Convert.ToInt32(xNode1.Attributes["allianceID"].Value);
                    tempCachedAlliance.Name = xNode1.Attributes["name"].Value;
                    tempCachedAlliance.Ticker = xNode1.Attributes["shortName"].Value;

                    if (!AllianceCache.Contains(tempCachedAlliance))
                    {
                        AllianceCache.Add(tempCachedAlliance);
                    }

                    XmlNodeList corpIDs = xNode1.SelectNodes("rowset/row");
                    foreach (XmlNode xNode2 in corpIDs)
                    {
                        CorporationIDs.Add(Convert.ToInt32(xNode2.Attributes["corporationID"].Value));
                    }
                    OnAllianceRecordProcessed();
                }
            }
            using (TextWriter tw = new StreamWriter(filepath))
            {
                allianceCacheSerializer.Serialize(tw, AllianceCache);
            }
            TotalCorpRecords = CorporationIDs.Count;
            corpBuildCanRun = true;
        }

        public static void GetCorporationDatabase()
        {
            while (!corpBuildCanRun)
            {

            }

            string filepath = String.Format("{0}\\stealthbot\\{1}",
				Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "EVEDB_Corporations.xml");

            if (!File.Exists(filepath))
            {
                File.Create(filepath);
            }
            else
            {
                using (TextReader tr = new StreamReader(filepath))
                {
					try
					{
						CorporationCache = (List<CachedCorporation>)corporationCacheSerializer.Deserialize(tr);
					}
					catch (InvalidCastException)
					{

					}
                }
            }

            foreach (int i in CorporationIDs)
            {
                HttpWebRequest c_db_web_request = (HttpWebRequest)WebRequest.Create(
                    String.Format("http://api.eve.ablairwhm.info/corp/CorporationSheet.xml.aspx?corporationID={0}", i));
                HttpWebResponse c_db_web_response = (HttpWebResponse)c_db_web_request.GetResponse();

                Stream c_db_Stream = c_db_web_response.GetResponseStream();

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(c_db_Stream);

                XmlNode xElement = xDoc.SelectSingleNode("/eveapi/result");
                CachedCorporation tempCachedCorporation = new CachedCorporation();
                foreach (XmlNode xn in xElement.ChildNodes)
                {
                    switch (xn.Name)
                    {
                        case "corporationID":
                            tempCachedCorporation.CorporationID = Convert.ToInt32(xn.InnerXml);
                            break;
                        case "corporationName":
                            tempCachedCorporation.Name = xn.InnerXml;
                            break;
                        case "ticker":
                            tempCachedCorporation.Ticker = xn.InnerXml;
                            break;
                        case "allianceID":
                            tempCachedCorporation.MemberOfAlliance = Convert.ToInt32(xn.InnerXml);
                            break;
                    }
                }

                if (!CorporationCache.Contains(tempCachedCorporation))
                {
                    CorporationCache.Add(tempCachedCorporation);
                }

                OnCorpRecordProcessed();
            }
            using (TextWriter tw = new StreamWriter(filepath))
            {
                corporationCacheSerializer.Serialize(tw, CorporationCache);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            GetAllianceDatabase();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            GetCorporationDatabase();
        }
    }


    public class CachedCorporation
    {
        public int CorporationID, MemberOfAlliance;
        public string Name, Ticker;
        public CachedCorporation()
        {

        }
    }

    public class CachedAlliance
    {
        public int AllianceID;
        public string Name, Ticker;
        public CachedAlliance()
        {

        }
    }
}
