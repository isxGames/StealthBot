using System;
using System.IO;
using System.Text;
using System.Xml;
using LavishScriptAPI;
using LavishVMAPI;

[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]

namespace StealthBot.Core
{
	public class Loader
	{
		private readonly EventHandler<LSEventArgs> _stealthBotUpdateCompleted, _stealthBotUpdated, _missionDatabaseUpdateCompleted, _npcBountiesUpdateCompleted, _possibleEwarNpcNamesUpdateCompleted;
		private volatile bool _isStealthBotUpdateComplete, _wasStealthBotUpdated, _isMissionDatabaseUpdateComplete, _isNpcBountiesUpdateCompleted, _isPossibleEwarNpcNamesUpdateComplete;

		private readonly string _productVersion;
		private readonly string[] _args;

		public bool LoadedSuccessfully;
		public string LoadErrorMessage;

		private static readonly uint _minimumInnerSpaceBuild = 5866;
		private static readonly DateTime _minimumIsxeveVersionDate = new DateTime(2013, 09, 11);
		private static readonly int _minimumIsxeveVersionBuild = 0002;

		public Loader(string productVersion, string[] args)
		{
		    _stealthBotUpdateCompleted = StealthBotUpdateCompleted;
			_stealthBotUpdated = StealthBotUpdated;
			_missionDatabaseUpdateCompleted = MissionDatabaseUpdateCompleted;
			_npcBountiesUpdateCompleted = NpcBountiesUpdateCompleted;
			_possibleEwarNpcNamesUpdateCompleted = PossibleEwarNpcNamesUpdateCompleted;

		    _productVersion = productVersion;
            for (var indexOf = _productVersion.IndexOf('.'); indexOf >= 0; indexOf = _productVersion.IndexOf('.'))
                _productVersion = _productVersion.Remove(indexOf, 1);

            _args = args;
		}

		public void Load()
		{
			Console.WriteLine("public void load()");
			if (!IsRunningInInnerSpace())
			{
				LoadErrorMessage = "Error: StealthBot must be started through InnerSpace, not by directly running the program.";
				return;
			}

			CheckForUpdates();

			if (_wasStealthBotUpdated)
				return;

			LoadErrorMessage = PerformSafetyChecks();

			if (LoadErrorMessage != null)
				return;

			LoadedSuccessfully = _isStealthBotUpdateComplete && !_wasStealthBotUpdated;
		}

		private static string PerformSafetyChecks()
		{
			if (!IsIsxeveLoaded())
				return "Error: ISXEVE not detected. StealthBot requires ISXEVE to run.";

			if (!IsMinimumIsxeveVersionLoaded())
			{
				var stringBuilder = new StringBuilder();
				
				var minimumIsxeveVersion = string.Format("{0}.{1}", _minimumIsxeveVersionDate.ToString("yyyyMMdd"), _minimumIsxeveVersionBuild);
				var errorVersionLine = "Error: The loaded ISXEVE is out of date. ISXEVE version {0} or later is required for StealthBot to run well.";
				var formattedVersionLine = string.Format(errorVersionLine, minimumIsxeveVersion);

				stringBuilder.AppendLine(formattedVersionLine);
				stringBuilder.AppendLine(@"Official test builds can be found here: http://www.isxgames.com/isxeve/test/");
				stringBuilder.AppendLine("Other test builds can frequently be found in the #isxeve topic or by asking in #isxeve.");
				stringBuilder.AppendLine("If you have any questions, contact stealthy in #stealthbot or at support@stealthsoftware.net");

				return stringBuilder.ToString();
			}

			if (!CheckInnerSpaceVersion())
				return "Error: You are running an outdated build of InnerSpace. Please ensure you're running development builds and patch InnerSpace.";

			return null;
		}

		private static bool IsMinimumIsxeveVersionLoaded()
		{
			string isxeveVersion = null;

			try
			{
				LavishScript.DataParse("${ISXEVE.Version}", ref isxeveVersion);
			}
			catch (Exception)
			{
				isxeveVersion = null;
			}

			if (string.IsNullOrEmpty(isxeveVersion))
				return false;

			var fragments = isxeveVersion.Split('.');

			if (fragments.Length != 2)
				return false;

			var dateString = fragments[0];
			var versionString = fragments[1];

			DateTime? date;
			try
			{
				date = DateTime.ParseExact(dateString, "yyyyMMdd", null);
			}
			catch (FormatException)
			{
				return false;
			}

			int version;
			if (!int.TryParse(versionString, out version))
				return false;

			if (date > _minimumIsxeveVersionDate)
				return true;

			if (date == _minimumIsxeveVersionDate && version >= _minimumIsxeveVersionBuild)
				return true;

			return false;
		}

		private static bool IsRunningInInnerSpace()
		{
			return InnerSpaceAPI.InnerSpace.BuildNumber > 0;
		}

		private static bool IsIsxeveLoaded()
		{
			var isxEveLoaded = false;

			try
			{
				LavishScript.DataParse("${ISXEVE(exists)}", ref isxEveLoaded);
			}
			catch { }

			return isxEveLoaded;
		}

		private static bool CheckInnerSpaceVersion()
		{
			return InnerSpaceAPI.InnerSpace.BuildNumber >= _minimumInnerSpaceBuild;
		}

		private void CheckForUpdates()
		{
			// Disable updates after STealthy code release -- CT
			return;
			// UpdateStealthBot();
			//UpdateMissionDatabase();
			//UpdateNpcBounties();
			//UpdatePossibleEwarNpcNames();

			//If we updated file, relaunch stealthbot.
			if (!_wasStealthBotUpdated) 
				return;

			var command = new StringBuilder(String.Format("TimedCommand 10 \"dotnet {0} stealthbot\"", AppDomain.CurrentDomain.FriendlyName));

			if (_args.Length > 0)
				command.Append(" true");

			LavishScript.ExecuteCommand(command.ToString());
		}

		private void UpdatePossibleEwarNpcNames()
		{
			LavishScript.Events.AttachEventTarget("PossibleEwarNpcNames_OnUpdateComplete", _possibleEwarNpcNamesUpdateCompleted);

			var possibleEwarNpcNamesVersion = ReadPossibleEwarNpcNamesVersion();
			LavishScript.ExecuteCommand(
				String.Format("dotnet {0} isxGamesPatcher {0} {1} http://stealthsoftware.net/software/data/isxGamesPatcher_PossibleEwarNpcNames.xml",
				"PossibleEwarNpcNames", possibleEwarNpcNamesVersion));

			var sanityCounter = 300;
			while (!_isPossibleEwarNpcNamesUpdateComplete || --sanityCounter < 0)
			{
				Frame.Wait(false);
			}

			LavishScript.Events.DetachEventTarget("PossibleEwarNpcNames_OnUpdateComplete", _possibleEwarNpcNamesUpdateCompleted);
		}

		private void UpdateNpcBounties()
		{
			var npcBountiesPath = string.Format("{0}\\{1}", Path.Combine(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "StealthBot"), "Data"), "NpcBounties.bin");

			if (File.Exists(npcBountiesPath))
			{
				var fileInfo = new FileInfo(npcBountiesPath);
				if (fileInfo.Length > 0) return;
			}

			LavishScript.Events.AttachEventTarget("npcBounties_OnUpdateComplete", _npcBountiesUpdateCompleted);

			LavishScript.ExecuteCommand(
				String.Format("dotnet {0} isxGamesPatcher {0} {1} http://stealthsoftware.net/software/data/isxGamesPatcher_NpcBounties.xml",
				"NpcBounties", 0));

			var sanityCounter = 300;
			while (!_isNpcBountiesUpdateCompleted || --sanityCounter < 0)
			{
				Frame.Wait(false);
			}

			LavishScript.Events.DetachEventTarget("npcBounties_OnUpdateComplete", _npcBountiesUpdateCompleted);
		}

		private void UpdateStealthBot()
		{
			LavishScript.Events.AttachEventTarget("stealthbot_OnFileUpdated", _stealthBotUpdated);
			LavishScript.Events.AttachEventTarget("stealthbot_OnUpdateComplete", _stealthBotUpdateCompleted);

#if DEBUG
			LavishScript.ExecuteCommand(
				String.Format("dotnet {0} isxGamesPatcher {0} {1} http://stealthsoftware.net/software/stealthbot-test/isxGamesPatcher_StealthBot-Test.xml",
				              "StealthBot", _productVersion));
#else
			LavishScript.ExecuteCommand(
				String.Format("dotnet {0} isxGamesPatcher {0} {1} http://stealthsoftware.net/software/stealthbot/isxGamesPatcher_StealthBot.xml",
				"StealthBot", _productVersion));
#endif

			//wait for UpdateComplete
			var sanityCounter = 300;    //5 seconds @ 60fps, 10 seconds at 30fps
			while (!_isStealthBotUpdateComplete || --sanityCounter < 0)
			{
				Frame.Wait(false);
			}

			LavishScript.Events.DetachEventTarget("stealthbot_OnFileUpdated", _stealthBotUpdated);
			LavishScript.Events.DetachEventTarget("stealthbot_OnUpdateComplete", _stealthBotUpdateCompleted);
		}

		private void UpdateMissionDatabase()
		{
			LavishScript.Events.AttachEventTarget("missiondatabase_OnUpdateComplete", _missionDatabaseUpdateCompleted);

			var missionDatabaseVersion = ReadMissionDatabaseVersion();
			LavishScript.ExecuteCommand(
				String.Format("dotnet {0} isxGamesPatcher {0} {1} http://stealthsoftware.net/software/data/isxGamesPatcher_MissionDatabase.xml",
				"MissionDatabase", missionDatabaseVersion));

			var sanityCounter = 300;
			while (!_isMissionDatabaseUpdateComplete || --sanityCounter < 0)
			{
				Frame.Wait(false);
			}

			LavishScript.Events.DetachEventTarget("missiondatabase_OnUpdateComplete", _missionDatabaseUpdateCompleted);
		}

		private void StealthBotUpdateCompleted(object sender, LSEventArgs e)
		{
			_isStealthBotUpdateComplete = true;
		}

		private void StealthBotUpdated(object sender, LSEventArgs e)
		{
			_wasStealthBotUpdated = true;
		}

		private void MissionDatabaseUpdateCompleted(object sender, LSEventArgs e)
		{
			_isMissionDatabaseUpdateComplete = true;
		}

		private int ReadMissionDatabaseVersion()
		{
			var xmlDocument = new XmlDocument();

			var path = string.Format("{0}\\{1}", Path.Combine(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "StealthBot"), "Data"), "MissionDatabase.xml");

			if (!File.Exists(path))
				return 0;

			xmlDocument.Load(path);

			var missionsNode = xmlDocument.SelectSingleNode("/Missions");

			if (missionsNode == null)
				return 0;

			var versionAttribute = missionsNode.Attributes["MissionDatabaseVersion"];

			if (versionAttribute == null)
				return 0;

			var versionString = versionAttribute.Value;

			var returnValue = 0;

			int.TryParse(versionString, out returnValue);

			return returnValue;
		}

		private int ReadPossibleEwarNpcNamesVersion()
		{
			var xmlDocument = new XmlDocument();

			var path = string.Format("{0}\\{1}", Path.Combine(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath), "StealthBot"), "Data"), "PossibleEwarNpcNames.xml");

			if (!File.Exists(path))
				return 0;

			xmlDocument.Load(path);

			var missionsNode = xmlDocument.SelectSingleNode("/PossibleEwarNpcNames");

			if (missionsNode == null)
				return 0;

			var versionAttribute = missionsNode.Attributes["Version"];

			if (versionAttribute == null)
				return 0;

			var versionString = versionAttribute.Value;

			var returnValue = 0;

			int.TryParse(versionString, out returnValue);

			return returnValue;
		}

		private void NpcBountiesUpdateCompleted(object sender, LSEventArgs e)
		{
			_isNpcBountiesUpdateCompleted = true;
		}

		private void PossibleEwarNpcNamesUpdateCompleted(object sender, LSEventArgs e)
		{
			_isPossibleEwarNpcNamesUpdateComplete = true;
		}
	}
}
