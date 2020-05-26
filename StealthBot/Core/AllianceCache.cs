using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Xml;
using System.Threading;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
    internal class AllianceCache : ModuleBase, IAllianceCache
    {
// ReSharper disable ConvertToConstant.Local
// ReSharper disable InconsistentNaming
		private readonly string Filename = "AllianceCache.bin";
// ReSharper restore InconsistentNaming
// ReSharper restore ConvertToConstant.Local

        private List<CachedAlliance> _cachedAlliances = new List<CachedAlliance>(); 
        public ReadOnlyCollection<CachedAlliance> CachedAlliances { get { return _cachedAlliances.AsReadOnly(); } }

        private readonly Dictionary<int, CachedAlliance> _cachedAlliancesById = new Dictionary<int, CachedAlliance>();
        public Dictionary<int, CachedAlliance> CachedAlliancesById { get { return _cachedAlliancesById; } } 

        private readonly string _allianceDatabaseFileName, _dataDirectory;

        public bool IsDatabaseReady { get; private set; }
        public bool IsDatabaseBuilding { get; private set; }

        //Callbacks and AutoResetEvents for load/save of the files
        private readonly FileReadCallback<CachedAlliance> _fileReadCallback;
        private readonly FileWriteCallback _fileWriteCallback;

        private readonly IFileManager _fileManager;

        public AllianceCache(IFileManager fileManager)
        {
			ModuleName = "AllianceCache";

            _fileManager = fileManager;

            _fileReadCallback = FileReadCompleted;
            _fileWriteCallback = FileWriteCompleted;

        	_dataDirectory = Path.Combine(StealthBot.Directory, "Data");
            _allianceDatabaseFileName = Path.Combine(_dataDirectory, Filename);
        }

        public override bool Initialize()
        {
            var methodName = "Initialize";
			LogTrace(methodName);

            if (!Directory.Exists(_dataDirectory))
            {
                Directory.CreateDirectory(_dataDirectory);
            }

			IsCleanedUpOutOfFrame = false;
            if (!IsInitialized)
            {
                if (!_isInitializing)
                {
                    _isInitializing = true;
                    LoadAllianceDatabase();
                }
            }

            return IsInitialized;
        }

        public override bool OutOfFrameCleanup()
        {
            var methodName = "OutOfFrameCleanup";
			LogTrace(methodName);

            if (!IsCleanedUpOutOfFrame)
            {
                if (!_isCleaningUp)
                {
                    _isCleaningUp = true;
                    SaveAllianceDatabase();
                }
            }

            return IsCleanedUpOutOfFrame;
        }

        #region Save / Load
        private void SaveAllianceDatabase()
        {
            _fileManager.QueueOverwriteSerialize(_allianceDatabaseFileName, _cachedAlliances, _fileWriteCallback);
        }

        private void FileReadCompleted(List<CachedAlliance> results)
        {
            var methodName = "FileReadCompleted";
            LogTrace(methodName);

			if (results.Count > 0)
			{
                lock (_cachedAlliances)
				{
                    _cachedAlliances = results;
                    foreach (var cachedAlliance in _cachedAlliances)
					{
                        if (!_cachedAlliancesById.ContainsKey(cachedAlliance.AllianceId))
						{
                            _cachedAlliancesById.Add(cachedAlliance.AllianceId, cachedAlliance);
						}
					}
				}
				IsDatabaseReady = true;
				IsInitialized = true;
			}
			else
			{
				IsInitialized = true;
				RegenerateAllianceDatabase();
			}
        }

        private void FileWriteCompleted()
        {
            IsCleanedUpOutOfFrame = true;
        }

        private void LoadAllianceDatabase()
        {
            var methodName = "LoadAllianceDatabase";
			LogTrace(methodName);

            if (File.Exists(_allianceDatabaseFileName))
            {
                _fileManager.QueueDeserialize(_allianceDatabaseFileName, _fileReadCallback);
            }
            else if (!IsDatabaseBuilding)
            {
                _cachedAlliances = new List<CachedAlliance>();
                IsInitialized = true;

                RegenerateAllianceDatabase();
            }
        }
        #endregion

		public void RegenerateAllianceDatabase()
		{
		    var methodName = "RegenerateAllianceDatabase";
			LogTrace(methodName);

			if (IsDatabaseBuilding)
				return;

			IsDatabaseBuilding = true;
			IsDatabaseReady = false;

			if (File.Exists(_allianceDatabaseFileName))
			{
				File.Delete(_allianceDatabaseFileName);
			}
			ThreadPool.QueueUserWorkItem(PopulateAllianceDatabaseFromEveApi);
			LogMessage(methodName, LogSeverityTypes.Debug, "Building alliance database.");
		}

        private void PopulateAllianceDatabaseFromEveApi(object stateInfo)
        {
            var methodName = "GenerateAllianceDB";
			LogTrace(methodName);

            //Core.StealthBot.Logging.LogMessage(Core.StealthBot.AllianceDB, new LogEventArgs(LogSeverityTypes.Debug,
            //    "GenerateAllianceDB", "Starting the download of the alliance database."));
			using (var downloadClient = new WebClient())
			{
				downloadClient.Proxy = null;
				try
				{
					using (var downloadStream = downloadClient.OpenRead("http://api.eve-online.com/eve/AllianceList.xml.aspx"))
					using (var downloadStreamReader = new StreamReader(downloadStream))
					{
						var xDoc = new XmlDocument();
					    try
					    {
                            xDoc.Load(downloadStreamReader);
					    }
					    catch (XmlException xe)
					    {
					        LogException(xe, methodName, "Caught exception while parsing Alliance API response:");
                            return;
					    }

						var xNodeList = xDoc.SelectNodes("/eveapi/result/rowset/row");

						foreach (XmlNode xNode in xNodeList)
						{
							var tempCachedAlliance = new CachedAlliance();
							tempCachedAlliance.AllianceId = Convert.ToInt32(xNode.Attributes["allianceID"].Value);
							tempCachedAlliance.Name = xNode.Attributes["name"].Value;
							tempCachedAlliance.Ticker = xNode.Attributes["shortName"].Value;

                            if (!_cachedAlliances.Contains(tempCachedAlliance))
							{
                                _cachedAlliances.Add(tempCachedAlliance);
							}
                            if (!_cachedAlliancesById.ContainsKey(tempCachedAlliance.AllianceId))
							{
                                _cachedAlliancesById.Add(tempCachedAlliance.AllianceId, tempCachedAlliance);
							}
						}
						IsDatabaseReady = true;
						IsInitialized = true;
						IsDatabaseBuilding = false;
					}
				}
				catch (WebException ex)
				{
					//Check for server unavailable -- eveonline.com is down
					if (ex.Message == "The remote server returned an error: (503) Server Unavailable." ||
                        ex.Message == "Remote server returned: (503) The server is not available." ||
                        ex.Message.Contains("The server committed a protocol violation."))
					{
						return;
					}
					IsDatabaseReady = false;
					IsInitialized = true;
					IsDatabaseBuilding = false;
					throw;
				}
			}
            //Core.StealthBot.Logging.LogMessage(Core.StealthBot.AllianceDB, new LogEventArgs(LogSeverityTypes.Debug,
            //    "GenerateAllianceDB", "Done downloading alliance database."));
        }
    }
}
