using System;
using System.IO;
using System.Text;
using System.Xml;
using LavishScriptAPI;
using LavishVMAPI;
using InnerSpaceAPI;

[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]

namespace StealthBot.Core
{
	public class Loader
	{
		private EventHandler<LSEventArgs> _stealthBotUpdateCompleted, _stealthBotUpdated;
		private volatile bool _isStealthBotUpdateComplete, _wasStealthBotUpdated;

		private readonly string _productVersion;
		private readonly string[] _args;

		public bool LoadedSuccessfully;
		public string LoadErrorMessage;

		public Loader(string productVersion, string[] args)
		{
		    _stealthBotUpdateCompleted = StealthBotUpdateCompleted;
			_stealthBotUpdated = StealthBotUpdated;

		    _productVersion = productVersion;
            for (var indexOf = _productVersion.IndexOf('.'); indexOf >= 0; indexOf = _productVersion.IndexOf('.'))
                _productVersion = _productVersion.Remove(indexOf, 1);

            _args = args;
		}

		public void Load()
		{
			InnerSpace.Echo("Checking if running in InnerSpace...");
			Console.WriteLine("public void load()");
			if (!IsRunningInInnerSpace())
			{
				InnerSpace.Echo("Error: StealthBot must be started through InnerSpace, not by directly running the program.");
				LoadErrorMessage = "Error: StealthBot must be started through InnerSpace, not by directly running the program.";
				return;
			}

			CheckForUpdates();

			if (_wasStealthBotUpdated)
				return;
			InnerSpace.Echo("Main Load(): PreformSafetyCheck call");
			LoadErrorMessage = PerformSafetyChecks();

			if (LoadErrorMessage != null)
				return;
			InnerSpace.Echo("ISXEve Loaded Returned that is was, moving on to set LoadedSuccessfully.");

			LoadedSuccessfully = true;
		}

		private static string PerformSafetyChecks()
		{
			if (!IsIsxeveLoaded())
				return "Error: ISXEVE not detected. StealthBot requires ISXEVE to run.";

			return null;
		}

		private static bool IsRunningInInnerSpace()
		{
			return InnerSpaceAPI.InnerSpace.BuildNumber > 0;
		}

		private static bool IsIsxeveLoaded()
		{
			InnerSpace.Echo("IsIsxeveLoaded(): Start of the function");
			var isxEveLoaded = false;

			try
			{
				LavishScript.DataParse("${ISXEVE(exists)}", ref isxEveLoaded);
			}
			catch { }

			return isxEveLoaded;
		}

		private void CheckForUpdates()
		{
			InnerSpace.Echo("CheckForUpdates: No Need, No Longer Maintained");
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

		private void StealthBotUpdateCompleted(object sender, LSEventArgs e)
		{
			_isStealthBotUpdateComplete = true;
		}

		private void StealthBotUpdated(object sender, LSEventArgs e)
		{
			_wasStealthBotUpdated = true;
		}

	}
}
