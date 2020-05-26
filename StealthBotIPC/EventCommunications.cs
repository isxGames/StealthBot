using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LavishScriptAPI;

namespace StealthBot.Core
{
    internal class EventCommunications : ModuleBase
    {
        public static readonly string RelayGroup = "SB_Sessions";
        public event EventHandler<Fleet_NeedPickupEventArgs> OnSB_Event_Fleet_NeedPickup;
        public event EventHandler<Fleet_NeedAssistEventArgs> OnSB_Event_Fleet_NeedAssist;
        public event EventHandler<Fleet_NeedTankEventArgs> OnSB_Event_Fleet_NeedTank;
        public event EventHandler<Fleet_NotificationEventArgs> OnSB_Event_Fleet_TankReady;
        public event EventHandler<Fleet_NotificationEventArgs> OnSB_Event_Fleet_TankNotReady;
		public event EventHandler<Info_UpdateFleetSkillsEventArgs> OnSB_Event_Info_UpdateFleetSkills;
		public event EventHandler<Fleet_NotificationEventArgs> OnSB_Event_Info_NeedFleetSkills;
		public event EventHandler<Notify_AcceptFleetInviteEventArgs> OnSB_Event_Notify_AcceptFleetInvite;
		public event EventHandler<Fleet_NeedPickupEventArgs> OnSB_Event_Fleet_DidPickup;
		public event EventHandler<Fleet_NeedPickupEventArgs> OnSB_Event_Fleet_AcknowledgePickupRequest;
        public event EventHandler<Notify_UpdateConfigurationFileEventArgs> OnSB_Event_Notify_UpdateConfigurationFile;

        public EventCommunications() : base()
        {
			IsEnabled = false;
			ObjectName = "EventCommunications";

            //Register the events we'll be using
            LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Fleet_NeedAssist.ToString());
            LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Fleet_NeedPickup.ToString());
			LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Fleet_WillPickup.ToString());
			LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Notify_PickupRequestAcknowledged.ToString());
            LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Fleet_NeedTank.ToString());
            LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Fleet_TankReady.ToString());
            LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Fleet_TankNotReady.ToString());
			LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Info_NeedFleetSkills.ToString());
			LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Info_UpdateFleetSkills.ToString());
			LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Notify_AcceptFleetInvite.ToString());
            LavishScript.Events.RegisterEvent(SB_Events.SB_Event_Notify_UpdateConfigurationFile.ToString());

            LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Fleet_NeedAssist.ToString(),
                _handle_SB_Event_Fleet_NeedAssist);
            LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Fleet_NeedPickup.ToString(),
                _handle_SB_Event_Fleet_NeedPickup);
			LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Fleet_WillPickup.ToString(),
				_handle_SB_Event_Fleet_DidPickup);
			LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Notify_PickupRequestAcknowledged.ToString(),
				_handle_SB_Event_Notify_AcknowledgePickupRequest);
            LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Fleet_NeedTank.ToString(),
                _handle_SB_Event_Fleet_NeedTank);
            LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Fleet_TankReady.ToString(),
                _handle_SB_Event_Fleet_TankReady);
            LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Fleet_TankNotReady.ToString(),
                _handle_SB_Event_Fleet_TankNotReady);
			LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Info_NeedFleetSkills.ToString(),
				_handle_SB_Event_Info_NeedFleetSkills);
			LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Info_UpdateFleetSkills.ToString(),
				_handle_SB_Event_Info_UpdateFleetSkills);
			LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Notify_AcceptFleetInvite.ToString(),
				_handle_SB_Event_Notify_AcceptFleetInvite);
            LavishScript.Events.AttachEventTarget(SB_Events.SB_Event_Notify_UpdateConfigurationFile.ToString(),
                _handle_SB_Event_Notify_UpdateConfigurationFile);

            //Join the stealthbot relay group
            LavishScript.ExecuteCommand(String.Format("Uplink RelayGroup -join {0}", RelayGroup));
            //Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
            //    "Ctor", String.Format("Joined RelayGroup {0}", RelayGroup)));
        }

        ~EventCommunications()
        {
            LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Fleet_NeedAssist.ToString(),
                _handle_SB_Event_Fleet_NeedAssist);
            LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Fleet_NeedPickup.ToString(),
                _handle_SB_Event_Fleet_NeedPickup);
			LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Fleet_WillPickup.ToString(),
				_handle_SB_Event_Fleet_DidPickup);
			LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Notify_PickupRequestAcknowledged.ToString(),
				_handle_SB_Event_Notify_AcknowledgePickupRequest);
            LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Fleet_NeedTank.ToString(),
                _handle_SB_Event_Fleet_NeedTank);
            LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Fleet_TankReady.ToString(),
                _handle_SB_Event_Fleet_TankReady);
            LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Fleet_TankNotReady.ToString(),
                _handle_SB_Event_Fleet_TankNotReady);
			LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Info_NeedFleetSkills.ToString(),
				_handle_SB_Event_Info_NeedFleetSkills);
			LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Info_UpdateFleetSkills.ToString(),
				_handle_SB_Event_Info_UpdateFleetSkills);
			LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Notify_AcceptFleetInvite.ToString(),
				_handle_SB_Event_Notify_AcceptFleetInvite);
            LavishScript.Events.DetachEventTarget(SB_Events.SB_Event_Notify_UpdateConfigurationFile.ToString(),
                _handle_SB_Event_Notify_UpdateConfigurationFile);
		}

		#region Event Handling Methods
		private void _handle_SB_Event_Fleet_NeedAssist(object sender, LSEventArgs e)
        {
            string methodName = "_handle_Fleet_NeedAssist";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
            Fleet_NeedAssistEventArgs fe = new Fleet_NeedAssistEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Received NeedAssist from {0}.", fe.SendingFleetMemberID)));
            if (OnSB_Event_Fleet_NeedAssist != null)
            {
                OnSB_Event_Fleet_NeedAssist(sender, fe);
            }
        }

        private void _handle_SB_Event_Fleet_NeedPickup(object sender, LSEventArgs e)
        {
            string methodName = "_handle_Fleet_NeedPickup";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));

			Fleet_NeedPickupEventArgs fe = new Fleet_NeedPickupEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Received NeedPickup from {0}.", fe.SendingFleetMemberID)));
			if (OnSB_Event_Fleet_NeedPickup != null)
			{
				OnSB_Event_Fleet_NeedPickup(this, fe);
			}
        }

		private void _handle_SB_Event_Fleet_DidPickup(object sender, LSEventArgs e)
		{
            string methodName = "_handle_Fleet_DidPickup";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			Fleet_NeedPickupEventArgs fe = new Fleet_NeedPickupEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Received DidPickup from {0}.", fe.SendingFleetMemberID)));
			if (OnSB_Event_Fleet_DidPickup != null)
			{
				OnSB_Event_Fleet_DidPickup(this, fe);
			}
		}

		private void _handle_SB_Event_Notify_AcknowledgePickupRequest(object sender, LSEventArgs e)
		{
            string methodName = "_handle_Notify_AckPickupRequest";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			Fleet_NeedPickupEventArgs fe = new Fleet_NeedPickupEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Received AckPickupRequest from {0}.", fe.SendingFleetMemberID)));
			if (OnSB_Event_Fleet_AcknowledgePickupRequest != null)
			{
				OnSB_Event_Fleet_AcknowledgePickupRequest(this, fe);
			}
		}

        private void _handle_SB_Event_Fleet_NeedTank(object sender, LSEventArgs e)
        {
            string methodName = "_handle_Fleet_NeedTank";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			Fleet_NeedTankEventArgs fe = new Fleet_NeedTankEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Received NeedTank from {0}.", fe.SendingFleetMemberID)));
			if (OnSB_Event_Fleet_NeedTank != null)
			{
				OnSB_Event_Fleet_NeedTank(sender, fe);
			}
        }

        private void _handle_SB_Event_Fleet_TankReady(object sender, LSEventArgs e)
        {
            string methodName = "_handle_Fleet_TankReady";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
            Fleet_NotificationEventArgs fe = new Fleet_NotificationEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Received TankReady from {0}.", fe.SendingFleetMemberID)));
            if (OnSB_Event_Fleet_TankReady != null)
            {
                OnSB_Event_Fleet_TankReady(sender, fe);
            }
        }

        private void _handle_SB_Event_Fleet_TankNotReady(object sender, LSEventArgs e)
        {
            string methodName = "_handle_Fleet_TankNotReady";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
            Fleet_NotificationEventArgs fe = new Fleet_NotificationEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Received TankNotReady from {0}.", fe.SendingFleetMemberID)));
            if (OnSB_Event_Fleet_TankNotReady != null)
            {
                OnSB_Event_Fleet_TankNotReady(sender, fe);
            }
        }

		private void _handle_SB_Event_Info_NeedFleetSkills(object sender, LSEventArgs e)
		{
            string methodName = "_handle_Info_NeedFleetSkills";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			Fleet_NotificationEventArgs eventArgs = new Fleet_NotificationEventArgs(e);

			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				methodName, string.Format("Received NeedFleetSkills from {0}.",
				eventArgs.SendingFleetMemberID)));
			if (OnSB_Event_Info_NeedFleetSkills != null)
			{
				OnSB_Event_Info_NeedFleetSkills(this, eventArgs);
			}
		}

		private void _handle_SB_Event_Info_UpdateFleetSkills(object sender, LSEventArgs e)
		{
            string methodName = "_handle_Info_UpdateFleetSkills";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			Info_UpdateFleetSkillsEventArgs eventArgs = new Info_UpdateFleetSkillsEventArgs(e);

			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				methodName, String.Format("Received UpdateFleetSkills from {0}.",
				eventArgs.SendingFleetMemberID)));
			if (OnSB_Event_Info_UpdateFleetSkills != null)
			{
				OnSB_Event_Info_UpdateFleetSkills(this, eventArgs);
			}
		}

		private void _handle_SB_Event_Notify_AcceptFleetInvite(object sender, LSEventArgs e)
		{
            string methodName = "_handle_Notify_AcceptFleetInvite";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			Notify_AcceptFleetInviteEventArgs eventArgs = new Notify_AcceptFleetInviteEventArgs(e);

			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				methodName, String.Format("Received AcceptFleetInvite from {0} ({1})",
				eventArgs.AcceptFrom, eventArgs.SendingFleetMemberID)));
			if (OnSB_Event_Notify_AcceptFleetInvite != null)
			{
				OnSB_Event_Notify_AcceptFleetInvite(this, eventArgs);
			}
		}

        private void _handle_SB_Event_Notify_UpdateConfigurationFile(object sender, LSEventArgs e)
        {
            string methodName = "_handle_Notify_UpdateConfigurationFile";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
            Notify_UpdateConfigurationFileEventArgs eventArgs = new Notify_UpdateConfigurationFileEventArgs(e);

            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, "Received UpdateConfigurationFile"));
            //Make sure we're on our uplink
            string uplinkName = string.Empty;
            LavishScript.DataParse<string>("${SettingXML[InnerSpace.XML].Set[Remote].GetString[Name].Escape}", ref uplinkName);
            if (uplinkName == eventArgs.UplinkName &&
                OnSB_Event_Notify_UpdateConfigurationFile != null)
            {
                OnSB_Event_Notify_UpdateConfigurationFile(this, eventArgs);
            }
        }
		#endregion

		#region Event Sending methods
		/// <summary>
		/// Signal to listeners that we need the fleet to assist with a given target.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
		public void Send_SB_Event_Fleet_NeedAssist(int sendingFleetMemberID, int solarSystemID)
        {
            string methodName = "Send_Fleet_NeedAssist";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3}]\"",
				RelayGroup, SB_Events.SB_Event_Fleet_NeedAssist.ToString(),
				sendingFleetMemberID.ToString(), solarSystemID.ToString());
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                methodName, string.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
        }

		/// <summary>
		/// Signal listeners that we need a fleet tank at a given location.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
		/// <param name="isBookMark"></param>
		/// <param name="destination"></param>
        public void Send_SB_Event_Fleet_NeedTank(int sendingFleetMemberID, int solarSystemID, bool isBookMark,
			object destination)
        {
            string methodName = "Send_Fleet_NeedTank";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3},{4},{5}]\"",
				RelayGroup, SB_Events.SB_Event_Fleet_NeedTank.ToString(),
				sendingFleetMemberID.ToString(), solarSystemID.ToString(), isBookMark.ToString(), destination.ToString());
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                methodName, string.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
        }

		/// <summary>
		/// Signal to listeners that we are a tank that is in position.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
        public void Send_SB_Event_Fleet_TankReady(int sendingFleetMemberID, int solarSystemID)
        {
            string methodName = "Send_Fleet_TankReady";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3}]\"",
				RelayGroup, SB_Events.SB_Event_Fleet_TankReady.ToString(), sendingFleetMemberID, solarSystemID);
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                methodName, String.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
        }

		/// <summary>
		/// Signal to listeners that we are no longer in position or actively tanking.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
        public void Send_SB_Event_Fleet_TankNotReady(int sendingFleetMemberID, int solarSystemID)
        {
            string methodName = "Send_Fleet_TankNotReady";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3}]\"",
				RelayGroup, SB_Events.SB_Event_Fleet_TankNotReady.ToString(), sendingFleetMemberID.ToString(),
				solarSystemID.ToString());
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                methodName, string.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
        }

		/// <summary>
		/// Signal to listeners that we need a pickup.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
		/// <param name="targetCanEntityID"></param>
		/// <param name="sendingFleetMemberEntityID"></param>
        public void Send_SB_Event_Fleet_NeedPickup(int sendingFleetMemberID, int solarSystemID, int targetCanEntityID, int sendingFleetMemberEntityID, string sendingFleetMemberName)
        {
            string methodName = "Send_Fleet_NeedPickup";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3},{4},{5},{6}]\"",
				RelayGroup, SB_Events.SB_Event_Fleet_NeedPickup.ToString(), sendingFleetMemberID.ToString(),
				solarSystemID.ToString(), targetCanEntityID.ToString(), sendingFleetMemberEntityID.ToString(), sendingFleetMemberName);
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, string.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
        }

		/// <summary>
		/// Signal to listeners that we will pickup a specific can.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
		/// <param name="targetCanEntityID"></param>
		/// <param name="sendingFleetMemberEntityID"></param>
		public void Send_SB_Event_Fleet_WillPickup(int sendingFleetMemberID, int solarSystemID, int targetCanEntityID, int sendingFleetMemberEntityID, string sendingFleetMemberName)
		{
            string methodName = "Send_Fleet_WillPickup";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3},{4},{5},{6}]\"",
				RelayGroup, SB_Events.SB_Event_Fleet_WillPickup.ToString(), sendingFleetMemberID.ToString(),
				solarSystemID.ToString(), targetCanEntityID.ToString(), sendingFleetMemberEntityID.ToString(), sendingFleetMemberName);
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
		}

		/// <summary>
		/// Signal to listeners that we need their fleet skills.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
		public void Send_SB_Event_Info_NeedFleetSkills(int sendingFleetMemberID, int solarSystemID)
		{
            string methodName = "Send_Info_NeedFleetSkills";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"{0}\" \"Event[{1}]:Execute[{2},{3}]\"",
				RelayGroup, SB_Events.SB_Event_Info_NeedFleetSkills.ToString(),
				sendingFleetMemberID, solarSystemID);
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				methodName, String.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
		}

		/// <summary>
		/// Signal to listeners what our fleet skills are.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
        /// <param name="inBoostShip"></param>
		/// <param name="leadership"></param>
		/// <param name="wingCommand"></param>
		/// <param name="fleetCommand"></param>
		/// <param name="miningDirector"></param>
		/// <param name="miningForeman"></param>
		/// <param name="armoredWarfare"></param>
		/// <param name="skirmishWarfare"></param>
		/// <param name="informationWarfare"></param>
		/// <param name="siegeWarfare"></param>
		/// <param name="warfareLinkSpecialist"></param>
		public void Send_SB_Event_Info_UpdateFleetSkills(int sendingFleetMemberID, int solarSystemID, int inBoostShip, int leadership,
			int wingCommand, int fleetCommand, int miningDirector, int miningForeman, int armoredWarfare,
			int skirmishWarfare, int informationWarfare, int siegeWarfare, int warfareLinkSpecialist)
		{
            string methodName = "Send_UpdateFleetSkills";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"{0}\" \"Event[{1}]:Execute[{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}]\"",
				RelayGroup, SB_Events.SB_Event_Info_UpdateFleetSkills.ToString(),
				sendingFleetMemberID, solarSystemID, inBoostShip, leadership, wingCommand, fleetCommand, miningDirector,
				miningForeman, armoredWarfare, skirmishWarfare, informationWarfare, siegeWarfare, warfareLinkSpecialist);
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				methodName, String.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
		}

		/// <summary>
		/// Signal to listeners that they should accept a fleet invite from the sender.
		/// </summary>
		/// <param name="sendingFleetMemberID"></param>
		/// <param name="solarSystemID"></param>
		/// <param name="invitedBy"></param>
		public void Send_SB_Event_Notify_AcceptFleetInvite(int sendingFleetMemberID, int solarSystemID, string invitedBy)
		{
            string methodName = "Send_Notify_AcceptFleetInvite";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));
			string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3},{4}]\"",
				RelayGroup, SB_Events.SB_Event_Notify_AcceptFleetInvite.ToString(),
				sendingFleetMemberID, solarSystemID, invitedBy);
			Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
				methodName, String.Format("Sending {0}", eventString)));
			LavishScript.ExecuteCommand(eventString);
		}

        /// <summary>
        /// Signal to listeners that they need to update a configuration file reference.
        /// </summary>
        /// <param name="oldFileName"></param>
        /// <param name="newFileName"></param>
        public void Send_SB_Event_Notify_UpdateConfigurationFile(string oldFileName, string newFileName)
        {
            string methodName = "Send_Notify_UpdateConfigurationFile";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));

            string uplinkName = string.Empty;
            LavishScript.DataParse<string>("${SettingXML[InnerSpace.XML].Set[Remote].GetString[Name].Escape}", ref uplinkName);
            string eventString = String.Format("relay \"other {0}\" \"Event[{1}]:Execute[{2},{3},{4}]\"",
                RelayGroup, SB_Events.SB_Event_Notify_UpdateConfigurationFile.ToString(),
                uplinkName, oldFileName, newFileName);
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Minor,
                methodName, String.Format("Sending {0}", eventString)));
            LavishScript.ExecuteCommand(eventString);
        }
		#endregion
	}

    public class Fleet_NotificationEventArgs : LSEventArgs
    {
        public int SendingFleetMemberID { get; private set; }
        public int SolarSystemID { get; private set; }
		protected string _objectName { get; set; }

        public Fleet_NotificationEventArgs(int sendingFleetMemberID, int solarSystemID)
        {
			_objectName = "NotificationEventArgs";
            SendingFleetMemberID = sendingFleetMemberID;
            SolarSystemID = solarSystemID;
        }

		//Call a function to parse the values of the args
		public Fleet_NotificationEventArgs(LSEventArgs copy)
		{
			_objectName = "NotificationEventArgs";
			_initialize(copy);
		}

		private void _initialize(LSEventArgs copy)
		{
            string methodName = "_initialize";
			int sendingFleetMemberID = -1, solarSystemID = -1;
			if (!int.TryParse(copy.Args[0], out sendingFleetMemberID))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse sending fleet member ID {0}",
					copy.Args[0])));
			}
			if (!int.TryParse(copy.Args[1], out solarSystemID))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse sending solar system ID {0}",
					copy.Args[1])));
			}
			SendingFleetMemberID = sendingFleetMemberID;
			SolarSystemID = solarSystemID;
		}
    }

	public sealed class Fleet_NeedPickupEventArgs : Fleet_NotificationEventArgs
	{
		public int TargetCanEntityID { get; private set; }
		public int SendingFleetMemberEntityID { get; private set; }
        public string SendingFleetMemberName { get; private set; }

		public Fleet_NeedPickupEventArgs(int sendingFleetMemberID, int solarSystemID, int targetCanEntityID, int sendingFleetMemberEntityID, string sendingFleetMemberName) :
			base(sendingFleetMemberID, solarSystemID)
		{
			_objectName = "NeedPickupEventArgs";
			TargetCanEntityID = targetCanEntityID;
			SendingFleetMemberEntityID = sendingFleetMemberEntityID;
            SendingFleetMemberName = sendingFleetMemberName;
		}

		public Fleet_NeedPickupEventArgs(LSEventArgs copy)
			: base(copy)
		{
			_objectName = "NeedPickupEventArgs";
			_initialize(copy);
		}

		private void _initialize(LSEventArgs copy)
		{
            string methodName = "_initialize";
			int targetCanEntityID = -1, sendingFleetMemberEntityID = -1;
			if (!int.TryParse(copy.Args[2], out targetCanEntityID))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse target can entity ID {0}", copy.Args[2])));
			}
			if (!int.TryParse(copy.Args[3], out sendingFleetMemberEntityID))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse sending fleet member entity ID {0}", copy.Args[3])));
			}
			TargetCanEntityID = targetCanEntityID;
			SendingFleetMemberEntityID = sendingFleetMemberEntityID;
		}
	}

    public sealed class Fleet_NeedAssistEventArgs : Fleet_NotificationEventArgs
    {
        public int TargetEntityID { get; private set; }

		public Fleet_NeedAssistEventArgs(int sendingFleetMemberID, int solarSystemID, int targetEntityID) : 
			base(sendingFleetMemberID, solarSystemID)
        {
			_objectName = "NeedAssistEventArgs";
            TargetEntityID = targetEntityID;
        }

		public Fleet_NeedAssistEventArgs(LSEventArgs copy)
			: base(copy)
		{
			_objectName = "NeedAssistEventArgs";
			_initialize(copy);
		}

		private void _initialize(LSEventArgs copy)
		{
            string methodName = "_initialize";
			int targetEntityID = -1;
			if (!int.TryParse(copy.Args[2], out targetEntityID))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse target entity ID {0}", copy.Args[2])));
			}
			TargetEntityID = targetEntityID;
		}
    }

    public sealed class Fleet_NeedTankEventArgs : Fleet_NotificationEventArgs
    {
        public bool DestinationIsBookMark { get; private set; }
        public object DestinationID { get; private set; }

        public Fleet_NeedTankEventArgs(int sendingFleetMemberID, int solarSystemID, bool destinationIsBookMark, object destinationID) :
			base(sendingFleetMemberID, solarSystemID)
        {
			_objectName = "NeedTankEventArg";
            DestinationIsBookMark = destinationIsBookMark;
            DestinationID = destinationID;
        }

		public Fleet_NeedTankEventArgs(LSEventArgs copy)
			: base(copy)
		{
			_objectName = "NeedTankEventArgs";
			_initialize(copy);
		}

		private void _initialize(LSEventArgs copy)
		{
            string methodName = "_initialize";
			int nearestEntityID = -1;
			bool isBookmark;
			string bookMarkLabel = string.Empty;

			if (!bool.TryParse(copy.Args[2], out isBookmark))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse \'is bookmark\' {0}", copy.Args[2])));
			}
			if (isBookmark)
			{
				bookMarkLabel = copy.Args[3];
			}
			else
			{
				if (!int.TryParse(copy.Args[3], out nearestEntityID))
				{
					Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
						methodName, String.Format("Unable to parse nearest entity ID {0}", copy.Args[3])));
				}
			}

			if (isBookmark)
			{
				DestinationIsBookMark = true;
				DestinationID = bookMarkLabel;
			}
			else
			{
				DestinationIsBookMark = false;
				DestinationID = nearestEntityID;
			}
		}
    }

	public sealed class Info_UpdateFleetSkillsEventArgs : Fleet_NotificationEventArgs
	{
        public int InBoostShip { get; private set; }
		public int Leadership { get; private set; }
		public int WingCommand { get; private set; }
		public int FleetCommand { get; private set; }
		public int MiningDirector { get; private set; }
		public int MiningForeman { get; private set; }
		public int ArmoredWarfare { get; private set; }
		public int SkirmishWarfare { get; private set; }
		public int InformationWarfare { get; private set; }
		public int SiegeWarfare { get; private set; }
		public int WarfareLinkSpecialist { get; private set; }

		public Info_UpdateFleetSkillsEventArgs(int sendingFleetMemberID, int solarSystemID, int inBoostShip, int leadership,
			int wingCommand, int fleetCommand, int miningDirector, int miningForeman, int armoredWarfare,
			int skirmishWarfare, int informationWarfare, int siegeWarfare, int warfareLinkSpecialist) :
			base(sendingFleetMemberID, solarSystemID)
		{
			_objectName = "UpdateFleetSkillsEventArgs";
            InBoostShip = inBoostShip;
			Leadership = leadership;
			WingCommand = wingCommand;
			FleetCommand = fleetCommand;
			MiningDirector = miningDirector;
			MiningForeman = miningForeman;
			ArmoredWarfare = armoredWarfare;
			SkirmishWarfare = skirmishWarfare;
			InformationWarfare = informationWarfare;
			SiegeWarfare = siegeWarfare;
			WarfareLinkSpecialist = warfareLinkSpecialist;
		}

		public Info_UpdateFleetSkillsEventArgs(LSEventArgs copy)
			: base(copy)
		{
			_objectName = "UpdateFleetSkillsEventArgs";
			_initialize(copy);
		}

		private void _initialize(LSEventArgs copy)
		{
            string methodName = "_initialize";
			int inBoostShip = -1, leadership = -1, wingCommand = -1, fleetCommand = -1, miningDirector = -1, miningForeman = -1, armoredWarfare = -1,
			skirmishWarfare = -1, informationWarfare = -1, siegeWarfare = -1, warfareLinkSpecialist = -1;

            if (!int.TryParse(copy.Args[2], out inBoostShip))
            {
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse in boost ship {0}", copy.Args[2])));
            }
			if (!int.TryParse(copy.Args[3], out leadership))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse leadership level {0}", copy.Args[2])));
			}
			if (!int.TryParse(copy.Args[4], out wingCommand))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse wing command level {0}", copy.Args[3])));
			}
			if (!int.TryParse(copy.Args[5], out fleetCommand))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse fleet command level {0}", copy.Args[4])));
			}
			if (!int.TryParse(copy.Args[6], out miningDirector))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse mining director level {0}", copy.Args[5])));
			}
			if (!int.TryParse(copy.Args[7], out miningForeman))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse mining foreman level {0}", copy.Args[6])));
			}
			if (!int.TryParse(copy.Args[8], out armoredWarfare))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse armor warfare level {0}", copy.Args[7])));
			}
			if (!int.TryParse(copy.Args[9], out skirmishWarfare))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse skirmish warfare level {0}", copy.Args[8])));
			}
			if (!int.TryParse(copy.Args[10], out informationWarfare))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse information warfare level {0}", copy.Args[9])));
			}
			if (!int.TryParse(copy.Args[11], out siegeWarfare))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse siege warfare level {0}", copy.Args[10])));
			}
			if (!int.TryParse(copy.Args[12], out warfareLinkSpecialist))
			{
				Core.StealthBot.Logging.OnLogMessage(_objectName, new LogEventArgs(LogSeverityTypes.Standard,
					methodName, String.Format("Unable to parse warfare link specialist level {0}", copy.Args[11])));
			}

            InBoostShip = inBoostShip;
			Leadership = leadership;
			WingCommand = wingCommand;
			FleetCommand = fleetCommand;
			MiningDirector = miningDirector;
			MiningForeman = miningForeman;
			ArmoredWarfare = armoredWarfare;
			SkirmishWarfare = skirmishWarfare;
			InformationWarfare = informationWarfare;
			SiegeWarfare = siegeWarfare;
			WarfareLinkSpecialist = warfareLinkSpecialist;
		}
	}

	public sealed class Notify_AcceptFleetInviteEventArgs : Fleet_NotificationEventArgs
	{
		public string AcceptFrom { get; private set; }

		public Notify_AcceptFleetInviteEventArgs(int sendingFleetMemberID, int solarSystemID, string acceptFrom) :
			base(sendingFleetMemberID, solarSystemID)
		{
			_objectName = "Notify_AcceptFleetInviteEventArgs";
			AcceptFrom = acceptFrom;
		}

		public Notify_AcceptFleetInviteEventArgs(LSEventArgs copy)
			: base(copy)
		{
			_objectName = "Notify_AcceptFleetInviteEventArgs";
			_initialize(copy);
		}

		private void _initialize(LSEventArgs copy)
		{
			AcceptFrom = copy.Args[2];
		}
	}

    public sealed class Notify_UpdateConfigurationFileEventArgs : Fleet_NotificationEventArgs
    {
        public string UplinkName { get; private set; }
        public string OldFileName { get; private set; }
        public string NewFileName { get; private set; }

        public Notify_UpdateConfigurationFileEventArgs(LSEventArgs copy)
            : base(copy)
        {
            _objectName = "Notify_UpdateConfigurationFileEventArgs";
            _initialize(copy);
        }

        private void _initialize(LSEventArgs copy)
        {
            UplinkName = copy.Args[2];
            OldFileName = copy.Args[3];
            NewFileName = copy.Args[4];
        }
    }

	/// <summary>
	/// The various events StealthBot can send and receive.
	/// </summary>
    public enum SB_Events
    {
		/// <summary>
		/// Signal to the fleet that we need a hauler to pickup a can.
		/// </summary>
        SB_Event_Fleet_NeedPickup,
		/// <summary>
		/// Signal to the fleet that we picked up a requested can.
		/// </summary>
		SB_Event_Fleet_WillPickup,
		/// <summary>
		/// Signal to the fleet that we need assistance with a target.
		/// </summary>
        SB_Event_Fleet_NeedAssist,
		/// <summary>
		/// Signal to the fleet that we need a tank at a specific location.
		/// </summary>
        SB_Event_Fleet_NeedTank,
		/// <summary>
		/// Signal to the fleet that a tank is in position at a specific location.
		/// </summary>
        SB_Event_Fleet_TankReady,
		/// <summary>
		/// Signal to the fleet that a tank that was in position is no longer in position.
		/// </summary>
        SB_Event_Fleet_TankNotReady,
		/// <summary>
		/// Signal to the listeners that we need their fleet-related skills.
		/// </summary>
		SB_Event_Info_NeedFleetSkills,
		/// <summary>
		/// Signal to the listeners the levels of our fleet-related skills.
		/// </summary>
		SB_Event_Info_UpdateFleetSkills,
		/// <summary>
		/// Signal to the listeners that they should accept a fleet invite from the sender.
		/// </summary>
		SB_Event_Notify_AcceptFleetInvite,
		/// <summary>
		/// Signal to listeners that we are going to try to pickup a specific can.
		/// </summary>
		SB_Event_Notify_PickupRequestAcknowledged,
        /// <summary>
        /// Signal to listeners that they need to update a configuration file
        /// </summary>
        SB_Event_Notify_UpdateConfigurationFile
    }
}
