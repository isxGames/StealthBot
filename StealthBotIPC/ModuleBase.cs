using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LavishScriptAPI;
using System.Diagnostics;
using EVE.ISXEVE;
using StealthBot.BehaviorModules;

namespace StealthBot.Core
{
    //Contain the pulse shit that every class will use
    public abstract class ModuleBase
    {
        //Number of seconds we wait before pulsing again
        internal int PulseFrequency = 1;
		internal int PulseCounter = -1;
        internal bool IsEnabled;
 
        Stopwatch pulseProfilingTimer = new Stopwatch();
		Stopwatch methodProfilingTimer = new Stopwatch();
		string methodProfilingName = string.Empty;

        public string ObjectName = string.Empty;

		static DateTime _startOfRunSession = DateTime.Now, _startOfNextSession = DateTime.Now;
		static bool _runSessionStarted = false, _sessionWaitStarted = false;

        //Cache EVETime shit
        internal string EVETime;

        internal ModuleBase()
        {
            //Default to "enabled"
            IsEnabled = true;
            //Add this to the list of modules 
            ModuleManager.Modules.Add(this);
        }

        //Handler for Pulse
        internal virtual void Pulse()
        {
            throw new NotImplementedException();
        }

        internal virtual void Initialize()
        {
            throw new NotImplementedException();
        }
        internal virtual void CleanUp()
        {
            throw new NotImplementedException();
        }

		internal bool ShouldPulse()
		{
			if (IsEnabled)
			{
				PulseCounter--;
				if (PulseCounter <= 0)
				{
					PulseCounter = PulseFrequency;
					return true;
				}
			}
			return false;
		}

        internal double Distance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt(
                Math.Pow(x1 - x2, 2) +
                Math.Pow(y1 - y2, 2) +
                Math.Pow(z1 - z2, 2)
                );
        }

        internal double DistanceTo(double x, double y, double z)
        {
            return Math.Sqrt(
                Math.Pow(x - Core.StealthBot._Me.ToEntity.X, 2) +
                Math.Pow(y - Core.StealthBot._Me.ToEntity.Y, 2) +
                Math.Pow(z - Core.StealthBot._Me.ToEntity.Z, 2)
                );
        }

        #region Profiling
        internal void StartPulseProfiling()
        {
            pulseProfilingTimer.Start();
        }

        internal void EndPulseProfiling()
        {
            pulseProfilingTimer.Stop();
            //Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
            //    "EndPulseProfiling", String.Format("Ending pulse profiling for: {0}, Elapsed: {1}",
            //    ObjectName, pulseProfilingTimer.Elapsed)));
            pulseProfilingTimer.Reset();
        }

		internal void StartMethodProfiling(string methodName)
		{
			methodProfilingTimer.Start();
			methodProfilingName = methodName;
		}

		internal void EndMethodProfiling()
		{
			methodProfilingTimer.Stop();
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"EndMethodProfiling", String.Format("Ending method profiling for: {0}, Elapsed: {1}",
				methodProfilingName, methodProfilingTimer.Elapsed)));
			methodProfilingTimer.Reset();
			methodProfilingName = string.Empty;
		}
        #endregion

        #region Exiting
        protected void ExitAndRelaunch()
		{
			Core.StealthBot.Instance.IsEnabled = false;
			if (Core.StealthBot.Config.MainConfig.UseRelaunching)
			{
				LavishScriptAPI.LavishScript.ExecuteCommand(
					String.Format("relay {0} \"TimedCommand 600 run isboxer -launch \"{1}\"\"",
					"${SettingXML[InnerSpace.XML].Set[Remote].GetString[Name].Escape}",
					Core.StealthBot.Config.MainConfig.CharacterSetToRelaunch));
			}
			LavishScriptAPI.LavishScript.ExecuteCommand(	//This code might not be correct, but the idea is.
				"uplink TimedCommand 200 kill ${Session}");	//We want to exit after 5 seconds so we don't undock forever,
			Core.StealthBot.OnSaveAndExit(this);			//which is going to get things blown up in low/nullsec.
		}

		protected void DowntimeExitAndRelaunch()
		{
			Core.StealthBot.Instance.IsEnabled = false;
			if (Core.StealthBot.Config.MainConfig.RelaunchAfterDowntime)
			{
				LavishScriptAPI.LavishScript.ExecuteCommand(
					String.Format("relay {0} \"TimedCommand 54000 run isboxer -launch \"{1}\"\"",
					"${SettingXML[InnerSpace.XML].Set[Remote].GetString[Name].Escape}",
					Core.StealthBot.Config.MainConfig.CharacterSetToRelaunch));
			}
			LavishScript.ExecuteCommand(
				"uplink TimedCommand 200 kill ${Session}");
			Core.StealthBot.OnSaveAndExit(this);
		}
        #endregion

        #region Game Time
        internal int GameHour
        {
            get
            {
                if (Core.StealthBot.EVE.Time != null)
                {
                    EVETime = Core.StealthBot.EVE.Time;
                }
				int hour = -1;
				int.TryParse(EVETime.Split(':')[0], out hour);
					
                return hour;
            }
        }

        internal int GameMinute
        {
            get
            {
                if (Core.StealthBot.EVE.Time != null)
                {
                    EVETime = Core.StealthBot.EVE.Time;
                }
				int minute = -1;
				int.TryParse(EVETime.Split(':')[1], out minute);

                return minute;
            }
        }

        internal bool IsDowntimeNear
        {
            get
            {
                return (GameHour == 10 && GameMinute >= 30);
            }
        }

        internal bool IsDowntimeImminent
        {
            get
            {
                return (GameHour == 10 && GameMinute >= 50);
            }
        }
        #endregion

        protected static bool CheckRuntime()
		{
            string methodName = "CheckRuntime";
            Core.StealthBot.Logging.OnLogMessage("BaseClass", new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));

			//Check should return true if we're not using MaxRuntime
			if (!Core.StealthBot.Config.MainConfig.UseMaxRuntime)
			{
				return true;
			}

			if (_runSessionStarted)
			{
				//If we're using max runtime and have exceeded the max runtime, switch to session wait and return false.
				//If we're still good on time, return true.
				if (Core.StealthBot.Config.MainConfig.UseMaxRuntime &&
					Core.StealthBot.TimeOfPulse.Subtract(_startOfRunSession).Minutes >= Core.StealthBot.Config.MainConfig.MaxRuntime)
				{
					Core.StealthBot.Logging.OnLogMessage("CheckRuntime", new LogEventArgs(LogSeverityTypes.Standard,
						methodName, String.Format("Max runtime of {0} minutes exceeded at {1}",
						Core.StealthBot.Config.MainConfig.MaxRuntime, DateTime.Now.ToString("hh:mm tt"))));
					_runSessionStarted = false;
					if (Core.StealthBot.Config.MainConfig.UseResumeAfter)
					{
						_sessionWaitStarted = true;
						Core.StealthBot.Logging.OnLogMessage("CheckRuntime", new LogEventArgs(LogSeverityTypes.Standard,
							methodName, String.Format("Resuming running in {0} minutes",
							Core.StealthBot.Config.MainConfig.ResumeAfter)));
						_startOfNextSession = DateTime.Now.AddMinutes(Core.StealthBot.Config.MainConfig.ResumeAfter);
					}
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				if (_sessionWaitStarted)
				{
					//If we've waited long enough and are using ResumeAfter, switch back to run session timer and return true
					if (Core.StealthBot.Config.MainConfig.UseResumeAfter &&
						Core.StealthBot.TimeOfPulse.CompareTo(_startOfNextSession) >= 0)
					{
						Core.StealthBot.Logging.OnLogMessage("CheckRuntime", new LogEventArgs(LogSeverityTypes.Standard,
							methodName, "Done waiting, resuming run."));
						_sessionWaitStarted = false;
						_runSessionStarted = true;
						_startOfRunSession = DateTime.Now;
						return true;
					}
					else
					{
						 //Haven't waited long enough, return false.
						return false;
					}
				}
				else
				{
					_runSessionStarted = true;
					_startOfRunSession = DateTime.Now;
					return true;
				}
			}
		}

		internal void DumpEntity(Entity entity)
		{
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "Alliance".PadRight(45), entity.Alliance.PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "AllianceID".PadRight(45), entity.AllianceID.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "AllianceTicker".PadRight(45), entity.AllianceTicker.PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "ArmorPct".PadRight(45), entity.ArmorPct.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "Alliance".PadRight(45), entity.Alliance.PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "BeingTargeted".PadRight(45), entity.BeingTargeted.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "CanLoot".PadRight(45), entity.CanLoot.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "CargoCapacity".PadRight(45), entity.CargoCapacity.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "CargoCount".PadRight(45), entity.CargoCount.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "Category".PadRight(45), entity.Category.PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "CategoryID".PadRight(45), entity.CategoryID.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "Corporation".PadRight(45), entity.Corporation.PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "CorporationID".PadRight(45), entity.CorporationID.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "CorporationTicker".PadRight(45), entity.CorporationTicker.PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "_distance".PadRight(45), entity.Distance.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "FollowRange".PadRight(45), entity.FollowRange.ToString().PadRight(45))));
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				"DumpEntity", String.Format("{0}: {1}", "ID".PadRight(45), entity.ID.ToString().PadRight(45))));
		}

        protected bool CanChangeTarget
        {
            get
            {
                return (!Core.StealthBot.Targeting.BlockTargetChangeNextFrame && !Core.StealthBot.Targeting.ChangedTargetThisFrame);
            }
        }

        /// <summary>
        /// Convert a boolean to an integer value. 1 for true, 0 for false.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected int BoolToInt(bool value)
        {
            if (value)
                return 1;
            else
                return 0;
        }
    }
}
