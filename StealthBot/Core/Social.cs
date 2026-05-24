using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using EVE.ISXEVE;
using LavishScriptAPI;
using StealthBot.Core.Interfaces;

namespace StealthBot.Core
{
	/* Social: Contain all social interaction, such as local checking, standings, etc */
    internal sealed class Social : ModuleBase, ISocial
    {
		//List of pilots for social iteration
		public bool IsLocalSafe { get; private set; }

	    private bool _isChatHandlerAttached;
		private readonly List<EVE_OnChannelMessageEventArgs> _localMessages = new List<EVE_OnChannelMessageEventArgs>();

        private readonly List<Pilot> _pilots = new List<Pilot>();
        public ReadOnlyCollection<Pilot> LocalPilots
        {
            get { return _pilots.AsReadOnly(); }
        }

        private readonly IIsxeveProvider _isxeveProvider;

		internal Social(IIsxeveProvider isxeveProvider)
		{
		    _isxeveProvider = isxeveProvider;

		    ModuleManager.ModulesToPulse.Add(this);
			ModuleName = "Social";
			PulseFrequency = 1;
		}

		public override bool Initialize()
		{
			if (!IsInitialized)
			{
				IsInitialized = true;
			}
			return IsInitialized;
		}

		public override bool OutOfFrameCleanup()
		{
			if (!IsCleanedUpOutOfFrame)
			{
				IsCleanedUpOutOfFrame = true;

                if (_isChatHandlerAttached)
                    LavishScript.Events.DetachEventTarget(EVE.ISXEVE.EVE.OnChannelMessageEvent, ChannelMessageReceived);
			}
			return IsCleanedUpOutOfFrame;
		}

		public override void InFrameCleanup()
		{
			foreach (var pilot in _pilots)
			{
				pilot.Invalidate();
			}
			_pilots.Clear();
		}

		public override void Pulse()
		{
			var methodName = "Pulse";
			LogTrace(methodName);

			if (!ShouldPulse()) 
				return;
			//StartPulseProfiling();

            if (StealthBot.Config.SocialConfig.UseChatReading)
            {
                if (!_isChatHandlerAttached)
                {
                    _isChatHandlerAttached = true;
                    LavishScript.Events.AttachEventTarget(EVE.ISXEVE.EVE.OnChannelMessageEvent, ChannelMessageReceived);
                }
            }
            else
            {
                if (_isChatHandlerAttached)
                {
                    _isChatHandlerAttached = false;
                    LavishScript.Events.DetachEventTarget(EVE.ISXEVE.EVE.OnChannelMessageEvent, ChannelMessageReceived);
                }
            }

			//Lock local messages to block the event adding any
			lock (_localMessages)
			{
				//If we have any local messages, fire the alert
				if (_localMessages.Count > 0)
				{
					StealthBot.Alerts.LocalChat(_localMessages[0].CharName, _localMessages[0].MessageText);
				}

				//Iterate any messages and log them then clear the list
				foreach (var channelMessageEventArgs in _localMessages)
				{
					LogMessage(methodName, LogSeverityTypes.Critical, "Channel {0} message: <{1}> {2}", 
						channelMessageEventArgs.ChannelID, channelMessageEventArgs.CharName, channelMessageEventArgs.MessageText);
				}
				_localMessages.Clear();
			}

			//StartMethodProfiling("GetLocalPilots");
		    _pilots.Clear();
            var pilots = _isxeveProvider.Eve.GetLocalPilots();
		    if (pilots != null)
		        _pilots.AddRange(pilots);
			//EndMethodProfiling();

			//StartMethodProfiling("IteratePilots");
			foreach (var pilot in _pilots)
			{
			    var charId = pilot.CharID;

			    if (charId == StealthBot.MeCache.CharId) continue;
			    if (!StealthBot.PilotCache.IsInitialized) continue;

			    if (StealthBot.PilotCache.CachedPilotsById.ContainsKey(charId))
				{
					var tempCachedPilot = StealthBot.PilotCache.CachedPilotsById[charId];
					//If I'm already getting the standing, might as well set it.
					tempCachedPilot.Standing = !StealthBot.Config.DefenseConfig.DisableStandingsChecks
					                           	? new CachedStanding(pilot.Standing)
					                           	: new CachedStanding();

					var corp = pilot.Corp;
					var corpId = corp.ID;
					if (corpId >= 0 &&
					    (tempCachedPilot.Corp.Length == 0 ||
					     tempCachedPilot.CorpID != corpId))
					{
						tempCachedPilot.CorpID = corpId;
                        tempCachedPilot.Corp = GetCorporationName(corpId);
					}

					var allianceId = pilot.AllianceID;
					if (allianceId >= 0 &&
					    (tempCachedPilot.Alliance.Length == 0 ||
					     tempCachedPilot.AllianceID != allianceId) &&
					    StealthBot.AllianceCache.IsDatabaseReady)
					{
						//Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
						//    "Pulse", String.Format("Setting pilot {0}'s Alliance to {1} ({2})",
						//    lp.Name, lp.Alliance, lp.AllianceID)));
						tempCachedPilot.AllianceID = allianceId;
					    tempCachedPilot.Alliance = GetAllianceName(allianceId);
					}

					if (allianceId < 0)
					{
						tempCachedPilot.AllianceID = allianceId;
						tempCachedPilot.Alliance = string.Empty;
					}
				}
				else
			    {
			        var corporationName = GetCorporationName(pilot.Corp.ID);
			        var allianceName = GetAllianceName(pilot.AllianceID);

                    StealthBot.PilotCache.AddPilot(pilot, corporationName, allianceName);
				}
			}
			//EndMethodProfiling();

			//StartMethodProfiling("IsLocalSafe");
			IsLocalSafe = DetermineIfLocalIsSafe();
			//EndMethodProfiling();

			//EndPulseProfiling();
		}

        private string GetAllianceName(int allianceId)
        {
            if (allianceId <= 0) return string.Empty;

            if (!StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(allianceId))
            {
                if (StealthBot.AllianceCache.IsDatabaseReady)
                    StealthBot.AllianceCache.RegenerateAllianceDatabase();

                return string.Empty;
            }

            var alliance = StealthBot.AllianceCache.CachedAlliancesById[allianceId];
            return alliance.Name;
        }

        private string GetCorporationName(Int64 corpId)
        {
            if (corpId <= 0) return string.Empty;

            if (!StealthBot.CorporationCache.CachedCorporationsById.ContainsKey(corpId))
            {
                StealthBot.CorporationCache.GetCorporationInfo(corpId);

                return string.Empty;
            }

            var corp = StealthBot.CorporationCache.CachedCorporationsById[corpId];
            return corp.Name;
        }

        /// <summary>
		/// Process received local messages
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ChannelMessageReceived(object sender, LSEventArgs e)
		{
			var methodName = "ChannelMessageReceived";
			LogTrace(methodName);

			var channelMessageEventArgs = new EVE_OnChannelMessageEventArgs(e);

			//Lock _localMessages for thread synch
			lock (_localMessages)
			{
				//Only watch for local messages. Here's how the IDs work.
				//Channel ID is the ID of the solarsystem for local, the ID of your corporation or alliance for those two channels,
				//or its own oddball number for other channels. So, check for the solar system ID.
				if (channelMessageEventArgs.ChannelID == StealthBot.MeCache.SolarSystemId)
				{
					_localMessages.Add(channelMessageEventArgs);
				}
			}
		}

		//Determine if the system is safe.
		private bool DetermineIfLocalIsSafe()
		{
			var methodName = "DetermineIfLocalIsSafe";
			LogTrace(methodName);

			//If I'm the only person in system
            //if (StealthBot.MeCache.SolarSystemId < 0 || _isxeveProvider.Eve.ChatChannel(StealthBot.MeCache.SolarSystemId).PilotCount == 1)
			//	return true;

			foreach (var pilot in _pilots)
			{
				var charId = pilot.CharID;

                if (charId == StealthBot.MeCache.CharId)
                    continue;

				if (!StealthBot.PilotCache.CachedPilotsById.ContainsKey(charId))
				{
					LogMessage(methodName, LogSeverityTypes.Standard, "Error: could not find entry in PilotCache for pilot {0} ({1}).",
                               pilot.Name, charId);
					return false;
				}

				var tempCachedPilot = StealthBot.PilotCache.CachedPilotsById[charId];

				if (StealthBot.Config.DefenseConfig.RunOnNonWhitelistedPilot)
				{
					//Note: If they're in ANY whitelist, they're ok unless excluded by a blacklist.

					if ((tempCachedPilot.AllianceID < 0 || !StealthBot.Config.SocialConfig.AllianceWhitelist.Contains(tempCachedPilot.Alliance)) &&
						!StealthBot.Config.SocialConfig.CorpWhitelist.Contains(tempCachedPilot.Corp) &&
						!StealthBot.Config.SocialConfig.PilotWhitelist.Contains(tempCachedPilot.Name))
					{
						LogMessage(methodName, LogSeverityTypes.Critical, "Player not whitelisted: {0}, ID: {1}, Corp: {2}, Alliance: {3}",
								   tempCachedPilot.Name, tempCachedPilot.CharID, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						StealthBot.Alerts.LocalUnsafe(tempCachedPilot.Name, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						return false;
					}
				}
				if (StealthBot.Config.DefenseConfig.RunOnBlacklistedPilot)
				{
					//If we find 'em in any blacklist, we're not safe.
					var isPlayerAllianceBlacklisted = StealthBot.Config.SocialConfig.AllianceBlacklist.Contains(tempCachedPilot.Alliance);
					if (isPlayerAllianceBlacklisted)
					{
                        LogMessage(methodName, LogSeverityTypes.Debug, "LocalPilot {0}, MTP: {1}, MTC: {2}, CTP: {3}, CTC: {4}, CTA: {5}, ATA: {6}",
                            tempCachedPilot.Name, tempCachedPilot.Standing.MeToPilot, tempCachedPilot.Standing.MeToCorp, tempCachedPilot.Standing.CorpToPilot,
                            tempCachedPilot.Standing.CorpToCorp, tempCachedPilot.Standing.CorpToAlliance, tempCachedPilot.Standing.AllianceToAlliance);
						LogMessage(methodName, LogSeverityTypes.Critical, "Player is alliance blacklisted: {0}, ID: {1}, Corp: \"{2}\", Alliance: \"{3}\"",
						           tempCachedPilot.Name, tempCachedPilot.CharID, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						StealthBot.Alerts.LocalUnsafe(tempCachedPilot.Name, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						return false;
					}

					var isPlayerCorpBlacklisted = StealthBot.Config.SocialConfig.CorpBlacklist.Contains(tempCachedPilot.Corp);
					if (isPlayerCorpBlacklisted)
					{
						LogMessage(methodName, LogSeverityTypes.Critical, "Player is corporation blacklisted: {0}, ID: {1}, Corp: \"{2}\", Alliance: \"{3}\"",
						           tempCachedPilot.Name, tempCachedPilot.CharID, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						StealthBot.Alerts.LocalUnsafe(tempCachedPilot.Name, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						return false;
					}

					var isPlayerPiotBlacklisted = StealthBot.Config.SocialConfig.PilotBlacklist.Contains(tempCachedPilot.Name);
					if (isPlayerPiotBlacklisted)
					{
						LogMessage(methodName, LogSeverityTypes.Critical, "Player is pilot blacklisted: {0}, ID: {1}, Corp: \"{2}\", Alliance: \"{3}\"",
						           tempCachedPilot.Name, tempCachedPilot.CharID, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						StealthBot.Alerts.LocalUnsafe(tempCachedPilot.Name, tempCachedPilot.Corp, tempCachedPilot.Alliance);
						return false;
					}
				}

				//If not checking standings, return/continue
				if (StealthBot.Config.DefenseConfig.DisableStandingsChecks)
					continue;

				//Temporary Standing to check
				var cachedStanding = tempCachedPilot.Standing;
				//I've got two ways of doing this:
				//1) Check if all *ToPilot is lower than minimum pilot standing.
				//2) Check if ANY *ToPilot is lower than min pilot standing.
				//#1 could caues issues when: dunno, can't think of any
				//#2 could cause issues when someone is blue to corp but not to you, thereby being "danger"

				if ((StealthBot.Config.DefenseConfig.RunOnCorpToPilot && cachedStanding.CorpToPilot < StealthBot.Config.SocialConfig.MinimumPilotStanding &&
				     StealthBot.Config.DefenseConfig.RunOnMeToPilot && cachedStanding.MeToPilot < StealthBot.Config.SocialConfig.MinimumPilotStanding) ||
				    (StealthBot.Config.DefenseConfig.RunOnMeToPilot && cachedStanding.MeToPilot < StealthBot.Config.SocialConfig.MinimumPilotStanding))
				{
					LogMessage(methodName, LogSeverityTypes.Critical, "Pilot {0}'s CorpToPilot ({1}) or MeToPilot ({2}) standing is below minimum pilot standing of {3}!",
					    tempCachedPilot.Name, cachedStanding.CorpToPilot, cachedStanding.MeToPilot, StealthBot.Config.SocialConfig.MinimumPilotStanding);
					StealthBot.Alerts.LocalUnsafe(tempCachedPilot.Name, tempCachedPilot.Corp, tempCachedPilot.Alliance);
					return false;
				}

				//Check the *ToCorp now
				if ((StealthBot.Config.DefenseConfig.RunOnCorpToCorp && cachedStanding.CorpToCorp < StealthBot.Config.SocialConfig.MinimumCorpStanding &&
				     StealthBot.Config.DefenseConfig.RunOnMeToCorp && cachedStanding.MeToCorp < StealthBot.Config.SocialConfig.MinimumCorpStanding) ||
				    (StealthBot.Config.DefenseConfig.RunOnMeToCorp && cachedStanding.MeToCorp < StealthBot.Config.SocialConfig.MinimumCorpStanding))
				{
					LogMessage(methodName, LogSeverityTypes.Critical, "Pilot {0}'s CorpToCorp ({1}) or MeToCorp ({2}) standing is below minimum corp standing of {3}!",
					    tempCachedPilot.Name, cachedStanding.CorpToCorp, cachedStanding.MeToCorp, StealthBot.Config.SocialConfig.MinimumCorpStanding);
					StealthBot.Alerts.LocalUnsafe(tempCachedPilot.Name, tempCachedPilot.Corp, tempCachedPilot.Alliance);
					return false;
				}
				//And now check *ToAlliance
				if ((StealthBot.Config.DefenseConfig.RunOnAllianceToAlliance && cachedStanding.AllianceToAlliance < StealthBot.Config.SocialConfig.MinimumAllianceStanding && 
                    StealthBot.Config.DefenseConfig.RunOnCorpToAlliance && cachedStanding.CorpToAlliance < StealthBot.Config.SocialConfig.MinimumAllianceStanding) ||
				    (StealthBot.Config.DefenseConfig.RunOnCorpToAlliance && cachedStanding.CorpToAlliance < StealthBot.Config.SocialConfig.MinimumAllianceStanding))
				{
					LogMessage(methodName, LogSeverityTypes.Critical, "Pilot {0}'s CorpToAlliance ({1}) or AllianceToAlliance ({2}) standing is below minimum alliance standing of {3}!",
					    tempCachedPilot.Name, cachedStanding.CorpToAlliance, cachedStanding.AllianceToAlliance, StealthBot.Config.SocialConfig.MinimumAllianceStanding);
					StealthBot.Alerts.LocalUnsafe(tempCachedPilot.Name, tempCachedPilot.Corp, tempCachedPilot.Alliance);
					return false;
				}
			}
			return true;
		}
	}
}
