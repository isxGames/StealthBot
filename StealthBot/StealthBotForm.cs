// ReSharper disable LocalizableElement
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using LavishScriptAPI;
using StealthBot.Core;
using StealthBot.Core.CustomEventArgs;
using StealthBot.Core.Interfaces;

namespace StealthBot
{
    public partial class StealthBotForm : Form
    {
        private bool _areTabsLocked = true;
        private readonly string _stealthBotVersionAndBuild;

        private bool _authStarted, _authCompleted, _autoStarted, _isReadyToExit;

        private readonly Dictionary<string, long> _idsByName = new Dictionary<string, long>();
        private int _pilotCacheIndex = 0, _miningCargoFullEvents;

        private EventHandler<LogEventArgs> _logMessageEventHandler;
        private EventHandler _configLoadedEventHandler;
        private EventHandler<LSEventArgs> _onPulse;
        private EventHandler<PairEventArgs<string, int>> _statisticsOnAddIceOreMinedEventHandler;
        private EventHandler<PairEventArgs<string, int>> _statisticsOnCrystalsUsedEventHandler;
        private EventHandler<TimeSpanEventArgs> _statisticsOnMiningCargoFullEventHandler;
        private readonly EventHandler<ManuallyAddPilotEventArgs> _manuallyAddEntryFormManuallyAddEntry;
        private EventHandler<__err_retn> _authenticationCompleted;
        private EventHandler<WalletStatisticsUpdatedEventArgs> _walletStatisticsUpdated;
        private EventHandler _exitDelegate;

        private readonly AutoResetEvent _exitResetEvent = new AutoResetEvent(true);
    	private readonly AutoResetEvent _pulseResetEvent = new AutoResetEvent(true);
        private Thread _exitThread;

        private readonly Dictionary<string, BotModes> _botModesByText = new Dictionary<string, BotModes>();
        private readonly Dictionary<string, HaulerModes> _haulerModesByText = new Dictionary<string, HaulerModes>();
        private readonly Dictionary<string, LocationTypes> _locationTypesByText = new Dictionary<string, LocationTypes>();

        private readonly Auth _auth;

        public StealthBotForm(params string[] args)
        {
            InitializeComponent();

			//Do something to initialize StealthBot
        	var instance = Core.StealthBot.Instance;
			if (string.IsNullOrEmpty(instance.ModuleName)) return;

            _auth = Auth.CreateAuth(Core.StealthBot.Logging);
            LavishScript.ExecuteCommand("stealthbotLoaded:Set[TRUE]");

            ButtonPause.Visible = false;

            _stealthBotVersionAndBuild = String.Format("StealthBot v{0} ({1})", Application.ProductVersion, Core.StealthBot.Build);
            Text = _stealthBotVersionAndBuild;

            AttachEventHandlers();

            LoadUsernameAndPassword();

            AddComboBoxItems();

            if (args.Length <= 0) return;

            bool autoStart;
            if (!bool.TryParse(args[0], out autoStart)) return;

            _autoStarted = autoStart;
            if (_autoStarted)
            {
                ButtonStartResume_Click(this, new EventArgs());
            }
        }

        #region UI Control EventHandlers
        private void ConfigurationTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush textBrush;

            var g = e.Graphics;

            // Get the item from the collection.
            var tabPage = configurationTabControl.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            var tabBounds = configurationTabControl.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {
                textBrush = new SolidBrush(Color.Black);
                var foregroundBrush = new SolidBrush(Color.White);

                g.FillRectangle(foregroundBrush, e.Bounds);
            }
            else
            {
                textBrush = new SolidBrush(Color.Black);
                Brush brush = new LinearGradientBrush(e.Bounds, Color.White, Color.DarkGray, LinearGradientMode.Horizontal);

                g.FillRectangle(brush, e.Bounds);
            }

            var stringFlags = new StringFormat
                                  {
                                      Alignment = StringAlignment.Center,
                                      LineAlignment = StringAlignment.Center
                                  };
            g.DrawString(tabPage.Text, e.Font, textBrush, tabBounds, stringFlags);
        }

        private void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //If it's locked and we selected any tab other than the main tab, display a message and return.
            var tabControl = ((TabControl)sender);

            if (!_areTabsLocked || tabControl.SelectedIndex == 0) return;

            ShowError("All tabs are locked until authentication is complete.");
            tabControl.SelectedIndex = 0;
        }
        #endregion

        #region EventHandler Methods
        private void StealthBot_SaveAndExit(object sender, EventArgs e)
        {
            Close();
        }

        private void Auth_AuthenticationComplete(object sender, __err_retn e)
        {
            if (InvokeRequired)
            {
                _exitResetEvent.WaitOne();
                Invoke(_authenticationCompleted, sender, e);
                _exitResetEvent.Set();
                return;
            }

            //use a bunch of =='s instead of !=
            if (!e.DidAuthenticationFail ||
                e.AuthenticationResult != "Successful")
            {
                _authCompleted = false;
                _authStarted = false;

                if (e.DidAuthenticationFail)
                {
                    Core.StealthBot.Logging.LogMessage("StealthBotForm", "Authentication", LogSeverityTypes.Standard,
                        "Authentication failed. Reason: {0}. Contact a StealthBot administrator for help.", e.AuthenticationResult);
                }
                else
                {
                    Core.StealthBot.Logging.LogMessage("StealthBotForm", "Authentication", LogSeverityTypes.Standard,
                        "Authentication failed. Reason: __err_fail. Contact a StealthBot administrator for help.");
                }
            }
            else
            {
                var isTestBuild = false;
#if DEBUG
                isTestBuild = true;
#endif

                if ((isTestBuild && e.CanUseTestBuilds) || !isTestBuild)
                {
                    Core.StealthBot.Logging.LogMessage("StealthBotForm", "Authentication", LogSeverityTypes.Standard, "Authentication successful.");
                    _authCompleted = true;
                    _areTabsLocked = false;
                    ButtonStartResume_Click(this, new EventArgs());
                }
                else
                {
                    if (isTestBuild && !e.CanUseTestBuilds)
                    {
                        Core.StealthBot.Logging.LogMessage("StealthBotForm", "Authentication", LogSeverityTypes.Standard,
                            "Authentication failed. This is a test build of StealthBot and you are not an authorized tester.");
                    }
                }
            }
        }

        private void Statistics_OnMiningCargoFull(object sender, TimeSpanEventArgs e)
        {
            if (InvokeRequired)
            {
                _exitResetEvent.WaitOne();
                Invoke(_statisticsOnMiningCargoFullEventHandler, sender, e);
                _exitResetEvent.Set();
                return;
            }

            textBoxNumDropOffs.Text = (++_miningCargoFullEvents).ToString();
            textBoxAverageTimePerFull.Text = e.Span.ToString();
        }

        private void Statistics_OnCrystalsUsed(object sender, PairEventArgs<string, int> e)
        {
            if (InvokeRequired)
            {
                _exitResetEvent.WaitOne();
                Invoke(_statisticsOnCrystalsUsedEventHandler, sender, e);
                _exitResetEvent.Set();
                return;
            }

            foreach (DataGridViewRow row in dataGridViewItemsConsumed.Rows)
            {
                if (((string)row.Cells[0].Value) == e.First)
                {
                    row.Cells[1].Value = (int)(row.Cells[1].Value) + e.Second;
                    return;
                }
            }

            var newRow = new DataGridViewRow();
            newRow.CreateCells(dataGridViewItemsConsumed);
            newRow.Cells[0].Value = e.First;
            newRow.Cells[1].Value = e.Second;
            dataGridViewItemsConsumed.Rows.Add(newRow);

            if (Core.StealthBot.Instance.Ammo_CrystalsUsed == null)
                Core.StealthBot.Instance.Ammo_CrystalsUsed = dataGridViewItemsConsumed;
        }

        private void Statistics_OnAddIceOreMined(object sender, PairEventArgs<string, int> e)
        {
            if (InvokeRequired)
            {
                _exitResetEvent.WaitOne();
                Invoke(_statisticsOnAddIceOreMinedEventHandler, sender, e);
                _exitResetEvent.Set();
                return;
            }

            foreach (DataGridViewRow row in dataGridViewItemsAcquired.Rows)
            {
                if (((string)row.Cells[0].Value) == e.First)
                {
                    row.Cells[1].Value = (int)(row.Cells[1].Value) + e.Second;
                    return;
                }
            }

            DataGridViewRow newRow = new DataGridViewRow();
            newRow.CreateCells(dataGridViewItemsAcquired);
            newRow.Cells[0].Value = e.First;
            newRow.Cells[1].Value = e.Second;
            dataGridViewItemsAcquired.Rows.Add(newRow);

            if (Core.StealthBot.Instance.ItemsMined_Moved == null)
                Core.StealthBot.Instance.ItemsMined_Moved = dataGridViewItemsAcquired;
        }

        private void Configuration_ConfigLoaded(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(_configLoadedEventHandler, sender, e);
                return;
            }

            lock (this)
            {
                try
                {
                    LoadConfigurationValues();

                    Text = String.Format("{0} :: {1} :: {2}", _stealthBotVersionAndBuild, Core.StealthBot.MeCache.Name, Core.StealthBot.ConfigurationManager.ActiveConfigName);
                	notifyIcon1.Text = Core.StealthBot.MeCache.Name;
                }
                catch (Exception ex)
                {
                    LogException("Configuration_ConfigLoaded", ex);
                }
            }
        }

        private void Logging_LogMessage(object sender, LogEventArgs e)
        {
            if (Disposing || IsDisposed || _isReadyToExit)
                return;

            if (e.Severity == LogSeverityTypes.Debug ||
                (!Core.StealthBot.IsDebug && e.Severity == LogSeverityTypes.Debug) ||
                e.Severity == LogSeverityTypes.Trace ||
                e.Severity == LogSeverityTypes.Profiling)
                return;

            if (InvokeRequired)
            {
                Invoke(_logMessageEventHandler, this, e);
                return;
            }

            listBox_logMessages.Items.Add(e.FormattedMessage);
            while (listBox_logMessages.Items.Count > 100)
            {
                listBox_logMessages.Items.RemoveAt(0);
            }
            Invalidate();
            listBox_logMessages.SelectedIndex = listBox_logMessages.Items.Count - 1;
        }

        private void Pulse(object sender, LSEventArgs e)
        {
			//Execution CANNOT be blocked in this method or EVE will lock.

			//Any blocking in this method, which is called from the game thread, blocks all execution of the game process.
			//As such, any blocking waiting for something else will result in a deadlock because whatever
			//	is being waited on cannot execute to finish.

            if (InvokeRequired)
            {
                try
                {
                	_pulseResetEvent.Reset();
                    Invoke(_onPulse, sender, e);
                	_pulseResetEvent.Set();
                }
                catch (Exception ex)
                {
                    string ObjectName = "StealthBotForm", methodName = "OnPulse";
                    Core.StealthBot.Logging.LogMessage(ObjectName, methodName, LogSeverityTypes.Debug,
                        "Caught exception while handling OnPulse:");
                    Core.StealthBot.Logging.LogMessage(ObjectName, methodName, LogSeverityTypes.Debug,
                        "Message: {0}", ex.Message);
                    Core.StealthBot.Logging.LogMessage(ObjectName, methodName, LogSeverityTypes.Debug,
                        "Stack Trace: {0}", ex.StackTrace);
                    Core.StealthBot.Logging.LogMessage(ObjectName, methodName, LogSeverityTypes.Debug,
                        "Inner Exception: {0}", ex.InnerException);
                }
                return;
            }

            if (radioButtonDisplayPilotCache.Checked)
            {
                if (Core.StealthBot.PilotCache.IsInitialized)
                {
                    var cachedPilots = Core.StealthBot.PilotCache.CachedPilotsById.Values.ToList();
                    for (; _pilotCacheIndex < cachedPilots.Count; _pilotCacheIndex++)
                    {
                        var cachedPilot = cachedPilots[_pilotCacheIndex];

                        //If ISXEVE breaks we can get invalid data here.
                        if (cachedPilot.Name != null &&
                            !_idsByName.ContainsKey(cachedPilot.Name))
                        {
                            _idsByName.Add(cachedPilot.Name, cachedPilot.CharID);
                            listBoxSearchResults.Items.Add(cachedPilot.Name);
                        }
                        else
                        {
                            Core.StealthBot.PilotCache.CachedPilotsById.Remove(cachedPilot.CharID);
                            cachedPilots.RemoveAt(_pilotCacheIndex);
                            _pilotCacheIndex--;
                        }
                    }
                }
            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                for (; _pilotCacheIndex < Core.StealthBot.CorporationCache.CachedCorporations.Count; _pilotCacheIndex++)
                {
                    var cachedCorporation = Core.StealthBot.CorporationCache.CachedCorporations[_pilotCacheIndex];
                    if (cachedCorporation.Name != null && !_idsByName.ContainsKey(cachedCorporation.Name))
                    {
                        _idsByName.Add(cachedCorporation.Name, cachedCorporation.CorporationId);
                        listBoxSearchResults.Items.Add(cachedCorporation.Name);
                    }
                    else
                    {
                        Core.StealthBot.CorporationCache.RemoveCorporation(cachedCorporation.CorporationId);
                        _pilotCacheIndex--;
                    }
                }
            }
            else if (radioButtonDisplayAllianceCache.Checked)
            {
                for (; _pilotCacheIndex < Core.StealthBot.AllianceCache.CachedAlliances.Count; _pilotCacheIndex++)
                {
                    var cachedAlliance = Core.StealthBot.AllianceCache.CachedAlliances[_pilotCacheIndex];

                    if (string.IsNullOrEmpty(cachedAlliance.Name))
                    {
                        if (Core.StealthBot.AllianceCache.IsDatabaseReady)
                            Core.StealthBot.AllianceCache.RegenerateAllianceDatabase();

                        _pilotCacheIndex = 0;
                        break;
                    }

                    if (!_idsByName.ContainsKey(cachedAlliance.Name))
                    {
                        _idsByName.Add(cachedAlliance.Name, cachedAlliance.AllianceId);
                        listBoxSearchResults.Items.Add(cachedAlliance.Name);
                    }
                }
            }

            for (var index = 0; index < listBoxAgents.Items.Count; index++)
            {
                var item = (string)listBoxAgents.Items[index];
                if (!Core.StealthBot.Config.MissionConfig.Agents.Contains(item))
                {
                    listBoxAgents.Items.RemoveAt(index);
                    index--;
                }
            }

            for (var index = 0; index < listBoxResearchAgents.Items.Count; index++)
            {
                var item = (string)listBoxResearchAgents.Items[index];
                if (!Core.StealthBot.Config.MissionConfig.ResearchAgents.Contains(item))
                {
                    listBoxResearchAgents.Items.RemoveAt(index);
                    index--;
                }
            }

            //Update the elapsed runtime
            textBoxElapsedRunningTime.Text = Core.StealthBot.Instance.RunTime.Elapsed.ToString();
        }

        private void WalletStatisticsUpdated(object sender, WalletStatisticsUpdatedEventArgs e)
        {
            if (InvokeRequired)
            {
                _exitResetEvent.WaitOne();
                Invoke(_walletStatisticsUpdated, sender, e);
                _exitResetEvent.Set();
                return;
            }

            //Update the wallet balance stuff
            if (Core.StealthBot.MeCache.WalletBalance == double.MinValue) return;

            textBoxWalletBalanceChange.Text = e.IskDeltaThisSession.ToString("N");
            if (e.IskDeltaThisSession > 0)
                textBoxWalletBalanceChange.ForeColor = Color.Green;
            else if (e.IskDeltaThisSession < 0)
                textBoxWalletBalanceChange.ForeColor = Color.Red;
            else
                textBoxWalletBalanceChange.ForeColor = Color.Black;

            textBoxIskPerHour.Text = e.AverageIskPerHour.ToString("N");
            if (e.AverageIskPerHour > 0)
                textBoxIskPerHour.ForeColor = Color.Green;
            else if (e.AverageIskPerHour < 0)
                textBoxIskPerHour.ForeColor = Color.Red;
            else
                textBoxIskPerHour.ForeColor = Color.Black;
        }
        #endregion

        private void LogException(string methodName, Exception e)
        {
            Core.StealthBot.Logging.LogMessage("StealthBotForm", methodName, LogSeverityTypes.Standard,
                                               "Caught exception:\n{0}\nStackTrace:\n{1}\nInnerException:\n{2}",
                                               e.Message, e.StackTrace, e.InnerException);
        }

        private void DoExit(object stateInfo)
        {
            _exitResetEvent.WaitOne();
        	_pulseResetEvent.WaitOne();

            if (Core.StealthBot.Instance != null)
                Core.StealthBot.Instance.Dispose();

            _isReadyToExit = true;

            Invoke(_exitDelegate, this, new EventArgs());
        }

        private void DetachStealthBotToInterfaceEvents()
        {
            Core.StealthBot.Logging.MessageLogged -= _logMessageEventHandler;
            Core.StealthBot.ConfigurationManager.ConfigLoaded -= _configLoadedEventHandler;
            Core.StealthBot.OnPulse -= _onPulse;
            _auth.AuthenticationComplete -= _authenticationCompleted;
            Core.StealthBot.Statistics.WalletStatisticsUpdated -= _walletStatisticsUpdated;
            Core.StealthBot.Statistics.OnAddIceOreMined -= _statisticsOnAddIceOreMinedEventHandler;
            Core.StealthBot.Statistics.OnCrystalsUsed -= _statisticsOnCrystalsUsedEventHandler;
            Core.StealthBot.Statistics.OnDropoff -= _statisticsOnMiningCargoFullEventHandler;
            Core.StealthBot.SaveAndExit -= StealthBot_SaveAndExit;
        }

        private void LoadConfigurationValues()
        {
            var config = Core.StealthBot.Config;

            //Main tab
            if (_botModesByText.Any(entry => entry.Value == config.MainConfig.ActiveBehavior))
                comboBoxBotMode.SelectedItem = _botModesByText.FirstOrDefault(entry => entry.Value == config.MainConfig.ActiveBehavior).Key;
            checkBoxDisable3dRender.Checked = config.MainConfig.Disable3DRender;
            checkBoxDisableUIRender.Checked = config.MainConfig.DisableUiRender;
            checkBoxDisableTextureLoading.Checked = config.MainConfig.DisableTextureLoading;

            listBoxConfigProfiles.Items.Clear();
            
            foreach (var configProfile in Core.StealthBot.ConfigurationManager.ConfigProfilesByName.Keys)
            {
                listBoxConfigProfiles.Items.Add(configProfile);
            }

            //Defense tab
            textBoxMinShieldPct.Text = config.DefenseConfig.MinimumShieldPct.ToString();
            textBoxMinCapPct.Text = config.DefenseConfig.MinimumCapPct.ToString();
            textBoxMinArmorPct.Text = config.DefenseConfig.MinimumArmorPct.ToString();
            textBoxResumeShieldPct.Text = config.DefenseConfig.ResumeShieldPct.ToString();
            textBoxResumeCapPct.Text = config.DefenseConfig.ResumeCapPct.ToString();
            textBoxMinNumDrones.Text = config.DefenseConfig.MinimumNumDrones.ToString();

            checkBoxRunOnBlacklist.Checked = config.DefenseConfig.RunOnBlacklistedPilot;
            checkBoxRunOnNonWhitelisted.Checked = config.DefenseConfig.RunOnNonWhitelistedPilot;
            checkBoxRunOnLowTank.Checked = config.DefenseConfig.RunOnLowTank;
            checkBoxRunOnLowCapacitor.Checked = config.DefenseConfig.RunOnLowCap;
            checkBoxRunOnTargetJammed.Checked = config.DefenseConfig.RunIfTargetJammed;
            checkBoxRunOnLowAmmo.Checked = config.DefenseConfig.RunOnLowAmmo;
            checkBoxRunOnLowDrones.Checked = config.DefenseConfig.RunOnLowDrones;

            checkBoxWaitAfterFleeing.Checked = config.DefenseConfig.WaitAfterFleeing;
            textBoxMinutesToWait.Text = config.DefenseConfig.MinutesToWait.ToString();

            textBoxMinimumPilotStanding.Text = config.SocialConfig.MinimumPilotStanding.ToString();
            textBoxMinimumCorpStanding.Text = config.SocialConfig.MinimumCorpStanding.ToString();
            textBoxMinimumAllianceStanding.Text = config.SocialConfig.MinimumAllianceStanding.ToString();

            checkBoxRunOnMeToPilot.Checked = config.DefenseConfig.RunOnMeToPilot;
            checkBoxRunOnMeToCorp.Checked = config.DefenseConfig.RunOnMeToCorp;
            checkBoxRunOnCorpToPilot.Checked = config.DefenseConfig.RunOnCorpToPilot;
            checkBoxRunOnCorpToCorp.Checked = config.DefenseConfig.RunOnCorpToCorp;
            checkBoxRunOnCorpToAlliance.Checked = config.DefenseConfig.RunOnCorpToAlliance;
            checkBoxRunOnAllianceToAlliance.Checked = config.DefenseConfig.RunOnAllianceToAlliance;

            checkBoxPreferStationSafespots.Checked = config.DefenseConfig.PreferStationsOverSafespots;
            checkBoxAlwaysShieldBoost.Checked = config.DefenseConfig.AlwaysShieldBoost;
            checkBoxAlwaysRunTank.Checked = config.DefenseConfig.AlwaysRunTank;
            checkBoxDisableStandingsChecks.Checked = config.DefenseConfig.DisableStandingsChecks;
            checkBoxUseChatReading.Checked = config.SocialConfig.UseChatReading;

            //Whitelist/Blacklist
            listBoxWhitelistPilots.Items.Clear();
            foreach (var pilot in config.SocialConfig.PilotWhitelist)
            {
                listBoxWhitelistPilots.Items.Add(pilot);
            }

            listBoxWhitelistCorps.Items.Clear();
            foreach (var corp in config.SocialConfig.CorpWhitelist)
            {
                listBoxWhitelistCorps.Items.Add(corp);
            }

            listBoxWhitelistAlliances.Items.Clear();
            foreach (var alliance in config.SocialConfig.AllianceWhitelist)
            {
                listBoxWhitelistAlliances.Items.Add(alliance);
            }

            listBoxBlacklistPilots.Items.Clear();
            foreach (var s in config.SocialConfig.PilotBlacklist)
            {
                listBoxBlacklistPilots.Items.Add(s);
            }

            listBoxBlacklistCorps.Items.Clear();
            foreach (var s in config.SocialConfig.CorpBlacklist)
            {
                listBoxBlacklistCorps.Items.Add(s);
            }

            listBoxBlacklistAlliances.Items.Clear();
            foreach (var s in config.SocialConfig.AllianceBlacklist)
            {
                listBoxBlacklistAlliances.Items.Add(s);
            }

            //Bookmarks
            textBoxSafeBookmarkPrefix.Text = config.MovementConfig.SafeBookmarkPrefix;
            textBoxAsteroidBeltBookmarkPrefix.Text = config.MovementConfig.AsteroidBeltBookmarkPrefix;
            textBoxIceBeltBookmarkPrefix.Text = config.MovementConfig.IceBeltBookmarkPrefix;
            textBoxTemporaryBeltBookMark.Text = config.MovementConfig.TemporaryBeltBookMarkPrefix;
            textBoxTemporaryCanPrefix.Text = config.MovementConfig.TemporaryCanBookMarkPrefix;
            textBoxSalvagingPrefix.Text = config.MovementConfig.SalvagingPrefix;

            //Movement
            checkBoxBounceWarp.Checked = config.MovementConfig.UseBounceWarp;
            textBoxMaxSlowboatTime.Text = config.MovementConfig.MaxSlowboatTime.ToString();
            checkBoxUseTempBeltBookmarks.Checked = config.MovementConfig.UseTempBeltBookmarks;

            checkBoxOnlyUseBookMarkedBelts.Checked = config.MovementConfig.OnlyUseBeltBookmarks;
            checkBoxMoveToRandomBelts.Checked = config.MovementConfig.UseRandomBeltOrder;
            checkBoxUseBeltSubsets.Checked = config.MovementConfig.UseBeltSubsets;
            textBoxNumBeltsInSubset.Text = config.MovementConfig.NumBeltsInSubset.ToString();
            comboBoxBeltSubsetMode.SelectedItem = config.MovementConfig.BeltSubsetMode.ToString();

            textBoxPropModMinCapPct.Text = config.MovementConfig.PropModMinCapPct.ToString();
            textBoxPropModResumeCapPct.Text = config.MovementConfig.PropModResumeCapPct.ToString();

            checkBoxUseCustomOrbitDistance.Checked = config.MovementConfig.UseCustomOrbitDistance;
            textBoxOrbitDistance.Text = config.MovementConfig.CustomOrbitDistance.ToString();
            checkBoxKeepAtRange.Checked = config.MovementConfig.UseKeepAtRangeInsteadOfOrbit;

            //Cargo
            if (_locationTypesByText.Any(entry => entry.Value == config.CargoConfig.DropoffLocation.LocationType))
                comboBoxDropoffType.SelectedItem = _locationTypesByText.First(entry => entry.Value == config.CargoConfig.DropoffLocation.LocationType).Key;
            textBoxDropoffBookmarkLabel.Text = config.CargoConfig.DropoffLocation.BookmarkLabel;
            textBoxDropoffID.Text = config.CargoConfig.DropoffLocation.EntityID.ToString();
            textBoxDropoffHangarDivision.Text = config.CargoConfig.DropoffLocation.HangarDivision.ToString();

			if (_locationTypesByText.Any(entry => entry.Value == config.CargoConfig.PickupLocation.LocationType))
				comboBoxPickupType.SelectedItem = _locationTypesByText.First(entry => entry.Value == config.CargoConfig.PickupLocation.LocationType).Key;
            textBoxPickupName.Text = config.CargoConfig.PickupLocation.BookmarkLabel;
            textBoxPickupID.Text = config.CargoConfig.PickupLocation.EntityID.ToString();
            textPickupHangarDivision.Text = config.CargoConfig.PickupLocation.HangarDivision.ToString();

            textBoxPickupSystemBookmark.Text = config.CargoConfig.PickupSystemBookmark;

            textBoxCargoFullThreshold.Text = config.CargoConfig.CargoFullThreshold.ToString();

            textBoxJetcanNameFormat.Text = config.CargoConfig.CanNameFormat;
            checkBoxAlwaysPopCans.Checked = config.CargoConfig.AlwaysPopCans;

            //Max Runtime
            checkBoxUseMaxRuntime.Checked = config.MaxRuntimeConfig.UseMaxRuntime;
            textBoxMaxRuntime.Text = config.MaxRuntimeConfig.MaxRuntimeMinutes.ToString();
            checkBoxResumeAfter.Checked = config.MaxRuntimeConfig.ResumeAfterWaiting;
            textBoxResumeAfter.Text = config.MaxRuntimeConfig.ResumeWaitMinutes.ToString();
            checkBoxUseRandomWaits.Checked = config.MaxRuntimeConfig.UseRandomWaits;

            checkBoxUseRelaunching.Checked = config.MaxRuntimeConfig.UseRelaunching;
            textBoxCharacterSetToLaunch.Text = config.MaxRuntimeConfig.CharacterSetToRelaunch;
            checkBoxRelaunchAfterDowntime.Checked = config.MaxRuntimeConfig.RelaunchAfterDowntime;

            //Fleet
            checkBoxDoFleetInvites.Checked = config.FleetConfig.DoFleetInvites;
            checkBoxOnlyHaulForListedMembers.Checked = config.FleetConfig.OnlyHaulForSkipList;

            listBoxFleetCharIDs.Items.Clear();
            foreach (var charID in config.FleetConfig.BuddyCharIDsToInvite)
            {
                listBoxFleetCharIDs.Items.Add(charID.ToString());
            }

            listBoxFleetCharIDsToSkip.Items.Clear();
            foreach (var charID in config.FleetConfig.FleetCharIDsToSkip)
            {
                listBoxFleetCharIDsToSkip.Items.Add(charID.ToString());
            }

            //Alerts
            checkBoxUseAlerts.Checked = config.AlertConfig.UseAlerts;

            checkBoxAlertLocalUnsafe.Checked = config.AlertConfig.AlertOnLocalUnsafe;
            checkBoxAlertLocalChat.Checked = config.AlertConfig.AlertOnLocalChat;
            checkBoxAlertFactionSpawn.Checked = config.AlertConfig.AlertOnFactionSpawn;
            checkBoxAlertLowAmmo.Checked = config.AlertConfig.AlertOnLowAmmo;
            checkBoxAlertFreighterNoPickup.Checked = config.AlertConfig.AlertOnFreighterNoPickup;
            checkBoxAlertPlayerNear.Checked = config.AlertConfig.AlertOnPlayerNear;
            checkBoxAlertLongRandomWait.Checked = config.AlertConfig.AlertOnLongRandomWait;
            checkBoxAlertTargetJammed.Checked = config.AlertConfig.AlertOnTargetJammed;
            checkBoxAlertFlee.Checked = config.AlertConfig.AlertOnFlee;
            checkBoxAlertWarpJammed.Checked = config.AlertConfig.AlertOnWarpJammed;

            //Mining
            checkedListBoxIcePriorities.Items.Clear();
            checkedListBoxOrePriorities.Items.Clear();

            var oreTypes = (from string s in Core.StealthBot.Config.MiningConfig.PriorityByOreType
                            orderby Core.StealthBot.Config.MiningConfig.PriorityByOreType.IndexOf(s)
                            select s).ToList();
            var iceTypes = (from string s in Core.StealthBot.Config.MiningConfig.PriorityByIceType
                            orderby Core.StealthBot.Config.MiningConfig.PriorityByIceType.IndexOf(s)
                            select s).ToList();

            foreach (var s in oreTypes)
            {
                checkedListBoxOrePriorities.Items.Add(s,
                    Core.StealthBot.Config.MiningConfig.StatusByOre[s]);
            }

            foreach (var s in iceTypes)
            {
                checkedListBoxIcePriorities.Items.Add(s,
                    Core.StealthBot.Config.MiningConfig.StatusByIce[s]);
            }

            checkBoxUseMiningDrones.Checked = config.MiningConfig.UseMiningDrones;
            checkBoxStripMine.Checked = config.MiningConfig.StripMine;
            checkBoxDistributeLasers.Checked = config.MiningConfig.DistributeLasers;
            checkBoxShortCycle.Checked = config.MiningConfig.ShortCycle;
            checkBoxIceMining.Checked = config.MiningConfig.IsIceMining;
            textBoxNumCrystalsToCarry.Text = config.MiningConfig.NumCrystalsToCarry.ToString();
            textBoxMinDistanceToPlayers.Text = config.MiningConfig.MinDistanceToPlayers.ToString();
            boostLocationLabelTextBox.Text = config.MiningConfig.BoostOrcaBoostLocationLabel;

            //Hauler
            if (_haulerModesByText.Any(entry => entry.Value == config.HaulingConfig.HaulerMode))
                comboBoxHaulerMode.SelectedItem = _haulerModesByText.First(entry => entry.Value == config.HaulingConfig.HaulerMode).Key;

            //Missions
            checkBoxIgnoreMissionDeclineTimer.Checked = config.MissionConfig.IgnoreMissionDeclineTimer;
            checkBoxRunCourierMissions.Checked = config.MissionConfig.RunCourierMissions;
            checkBoxRunTradeMissions.Checked = config.MissionConfig.RunTradeMissions;
            checkBoxRunEncounterMissions.Checked = config.MissionConfig.RunEncounterMissions;
            checkBoxRunMiningMissions.Checked = config.MissionConfig.RunMiningMissions;

            checkBoxAvoidLowsecMissions.Checked = config.MissionConfig.AvoidLowSec;

            checkBoxDoOreMiningMissions.Checked = config.MissionConfig.DoOreMiningMissions;
            checkBoxDoIceMiningMissions.Checked = config.MissionConfig.DoIceMiningMissions;
            checkBoxDoGasMiningMissions.Checked = config.MissionConfig.DoGasMiningMissions;

            checkBoxKillEmpireFactions.Checked = config.MissionConfig.DoEmpireKillMissions;
            checkBoxKillPirateFactions.Checked = config.MissionConfig.DoPirateKillMissions;

            checkBoxDoStorylineMissions.Checked = config.MissionConfig.DoStorylineMissions;
            checkBoxDoChainCouriers.Checked = config.MissionConfig.DoChainCouriers;

            listBoxAgents.Items.Clear();
            foreach (var agent in config.MissionConfig.Agents)
            {
                listBoxAgents.Items.Add(agent);
            }

            listBoxResearchAgents.Items.Clear();
            foreach (var agent in config.MissionConfig.ResearchAgents)
            {
                listBoxResearchAgents.Items.Add(agent);
            }

            listBoxMissionBacklist.Items.Clear();
            foreach (var mission in config.MissionConfig.MissionBlacklist)
            {
                listBoxMissionBacklist.Items.Add(mission);
            }

            //Ratting
            checkBoxChainBelts.Checked = config.RattingConfig.ChainBelts;
            textBoxMinChainBounty.Text = config.RattingConfig.MinimumChainBounty.ToString();
            checkBoxOnlyChainSolo.Checked = config.RattingConfig.OnlyChainWhenAlone;
            runCosmicAnomaliesCheckBox.Checked = config.RattingConfig.IsAnomalyMode;

            anomalyStatusByNameCheckedListBox.Items.Clear();
            foreach (var pair in config.RattingConfig.StatusByAnomalyType)
            {
                anomalyStatusByNameCheckedListBox.Items.Add(pair.First, pair.Second);
            }

            //Salvaging
            checkBoxEnableSalvagingBM.Checked = config.SalvageConfig.CreateSalvageBookmarks;
            checkBoxSalvageToCorp.Checked = config.SalvageConfig.SaveBookmarksForCorporation;
        }

        private void ShowError(string errorText)
        {
            MessageBox.Show(errorText, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SetButtonsAndLabels()
        {
            if (ButtonStartResume.Text.Equals("Auth"))
                ButtonStartResume.Text = "Start";
            if (!ButtonPause.Visible)
                ButtonPause.Visible = true;
            if (!comboBoxBotMode.Visible)
                comboBoxBotMode.Visible = true;
            if (!checkBoxUseRandomWaits.Visible)
                checkBoxUseRandomWaits.Visible = true;
            if (!checkBoxDisable3dRender.Visible)
                checkBoxDisable3dRender.Visible = true;
            if (!checkBoxDisableUIRender.Visible)
                checkBoxDisableUIRender.Visible = true;
            if (!label35.Visible)
                label35.Visible = true;
            if (!checkBoxDisableTextureLoading.Visible)
                checkBoxDisableTextureLoading.Visible = true;

            Core.StealthBot.Logging.LogMessage("StealthBotForm", "Start", LogSeverityTypes.Standard, "Starting or resuming.");

            if (_autoStarted)
                ButtonStartResume_Click(this, new EventArgs());
        }

        private void AttachEventHandlers()
        {
            _logMessageEventHandler = Logging_LogMessage;
            Core.StealthBot.Logging.MessageLogged += _logMessageEventHandler;

            _configLoadedEventHandler = Configuration_ConfigLoaded;
            Core.StealthBot.ConfigurationManager.ConfigLoaded += _configLoadedEventHandler;

            _onPulse = Pulse;
            Core.StealthBot.OnPulse += _onPulse;

            _authenticationCompleted = Auth_AuthenticationComplete;
            _auth.AuthenticationComplete += _authenticationCompleted;

            _walletStatisticsUpdated = WalletStatisticsUpdated;
            Core.StealthBot.Statistics.WalletStatisticsUpdated += _walletStatisticsUpdated;

            _statisticsOnAddIceOreMinedEventHandler = Statistics_OnAddIceOreMined;
            Core.StealthBot.Statistics.OnAddIceOreMined += _statisticsOnAddIceOreMinedEventHandler;

            _statisticsOnCrystalsUsedEventHandler = Statistics_OnCrystalsUsed;
            Core.StealthBot.Statistics.OnCrystalsUsed += _statisticsOnCrystalsUsedEventHandler;

            _statisticsOnMiningCargoFullEventHandler = Statistics_OnMiningCargoFull;
            Core.StealthBot.Statistics.OnDropoff += _statisticsOnMiningCargoFullEventHandler;

            Core.StealthBot.SaveAndExit += StealthBot_SaveAndExit;

            _exitDelegate = StealthBot_SaveAndExit;
        }

        private void LoadUsernameAndPassword()
        {
            var directory = Path.Combine(Core.StealthBot.Directory, "Config");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            LoadUsernameAndPasswordFromFile(directory);
        }

        private void LoadUsernameAndPasswordFromFile(string directory)
        {
            var fileName = Path.Combine(directory, "SBLogin.txt");

            if (!File.Exists(fileName)) return;

            using (var sr = new StreamReader(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                TextBoxAuthEmailAddress.Text = sr.ReadLine();
                TextBoxAuthPassword.Text = sr.ReadLine();
            }
        }

        private void AddComboBoxItems()
        {
            switch (Core.StealthBot.Build)
            {
                case StealthBotBuilds.Stable:
                    buildsToPatchComboBox.Items.Add("Testing");
                    break;
                case StealthBotBuilds.Testing:
                    buildsToPatchComboBox.Items.Add("Stable");
                    break;
            }

            //Bot modes
            _botModesByText.Add("Mining", BotModes.Mining);
            _botModesByText.Add("Hauling", BotModes.Hauling);
            _botModesByText.Add("Ratting", BotModes.Ratting);
            _botModesByText.Add("Missioning", BotModes.Missioning);
            _botModesByText.Add("Boost Orca", BotModes.BoostOrca);
            _botModesByText.Add("Boost Can Orca", BotModes.BoostCanOrca);
            _botModesByText.Add("Freighting", BotModes.Freighting);
            _botModesByText.Add("Jump Stability Test", BotModes.JumpStabilityTest);
            foreach (var value in _botModesByText.Keys)
            {
                comboBoxBotMode.Items.Add(value);
            }

            comboBoxBeltSubsetMode.DataSource = Enum.GetNames(typeof (BeltSubsetModes));

        	var station = "Station";
			_locationTypesByText.Add(station, LocationTypes.Station);
			comboBoxDropoffType.Items.Add(station);
			comboBoxPickupType.Items.Add(station);

        	var corpHangarArray = "Corp. Hangar Array";
			_locationTypesByText.Add(corpHangarArray, LocationTypes.CorpHangarArray);
			comboBoxDropoffType.Items.Add(corpHangarArray);
			comboBoxPickupType.Items.Add(corpHangarArray);

        	var stationCorpHangar = "Station Corp. Hangar";
        	_locationTypesByText.Add(stationCorpHangar, LocationTypes.StationCorpHangar);
            comboBoxDropoffType.Items.Add(stationCorpHangar);
            comboBoxPickupType.Items.Add(stationCorpHangar);

            _locationTypesByText.Add("Anchored Container", LocationTypes.AnchoredContainer);

        	var jetcan = "Jetcan";
        	_locationTypesByText.Add(jetcan, LocationTypes.Jetcan);
            comboBoxDropoffType.Items.Add(jetcan);

        	var shipBay = "Ship Bay";
        	_locationTypesByText.Add(shipBay, LocationTypes.ShipBay);
            comboBoxDropoffType.Items.Add(shipBay);
            comboBoxPickupType.Items.Add(shipBay);

            //Hauler Modes
            _haulerModesByText.Add("Cycle Fleet", HaulerModes.CycleFleetMembers);
            _haulerModesByText.Add("Wait For Request", HaulerModes.WaitForRequestEvent);
            foreach (var value in _haulerModesByText.Keys)
            {
                comboBoxHaulerMode.Items.Add(value);
            }
        }

        #region Configuration Control Event Handlers
        #region Main Tab
        private void ButtonAddProfile_Click(object sender, EventArgs e)
        {
            var inputForm = new TextInputForm("Enter name of new profile");
            var result = inputForm.ShowDialog();

            if (result != DialogResult.OK)
                return;

            var profileName = inputForm.InputText;

            var invalidChar = '\0';
            var foundInvalidChar = false;

            foreach (var possibleInvalidChar in Path.GetInvalidFileNameChars())
            {
                if (!profileName.Contains(possibleInvalidChar)) continue;

                foundInvalidChar = true;
                invalidChar = possibleInvalidChar;
                break;
            }

            if (foundInvalidChar)
            {
                ShowError(string.Format("Profile name contains invalid character \'{0}\'.", invalidChar));
            }
            else
            {
            	_exitResetEvent.WaitOne();
            	Core.StealthBot.ConfigurationManager.AddConfigProfile(profileName, true);
            	_exitResetEvent.Set();
            }
        }

        private void ButtonRemoveProfile_Click(object sender, EventArgs e)
        {
            if (listBoxConfigProfiles.SelectedIndex == -1)
            {
                ShowError("Select a profile to remove.");
                return;
            }

            var selectedProfile = listBoxConfigProfiles.SelectedItem.ToString();
            if (selectedProfile == Core.StealthBot.ConfigurationManager.ActiveConfigName)
            {
                ShowError("Select a profile other than the currently loaded profile.");
                return;
            }

        	_exitResetEvent.WaitOne();
            Core.StealthBot.ConfigurationManager.RemoveConfigProfile(selectedProfile);
        	_exitResetEvent.Set();
            listBoxConfigProfiles.Items.Remove(selectedProfile);
        }

        private void ButtonRenameProfile_Click(object sender, EventArgs e)
        {
            if (listBoxConfigProfiles.SelectedIndex == -1)
            {
                ShowError("Select a profile to rename.");
                return;
            }

            var inputForm = new TextInputForm("Enter new profile name");
            var result = inputForm.ShowDialog();

            if (result != DialogResult.OK)
                return;

            var profileName = inputForm.InputText;

            var invalidChar = '\0';
            var foundInvalidChar = false;

            foreach (var possibleInvalidChar in Path.GetInvalidFileNameChars())
            {
                if (!profileName.Contains(possibleInvalidChar)) continue;

                foundInvalidChar = true;
                invalidChar = possibleInvalidChar;
                break;
            }

            var selectedProfile = listBoxConfigProfiles.SelectedItem.ToString();
            if (foundInvalidChar)
            {
                ShowError(string.Format("Profile name contains invalid character \'{0}\'.", invalidChar));
            }
            else
            {
            	_exitResetEvent.WaitOne();
                Core.StealthBot.ConfigurationManager.RenameConfigProfile(selectedProfile, profileName);
            	_exitResetEvent.Set();
            }
        }

        private void ButtonCopyProfile_Click(object sender, EventArgs e)
        {
            if (listBoxConfigProfiles.SelectedIndex == -1)
            {
                ShowError("Select a profile to copy.");
                return;
            }

            var selectedProfile = listBoxConfigProfiles.SelectedItem.ToString();

        	_exitResetEvent.WaitOne();
            Core.StealthBot.ConfigurationManager.CopyConfigProfile(selectedProfile);
        	_exitResetEvent.Set();
        }

        private void ButtonLoadProfile_Click(object sender, EventArgs e)
        {
            if (listBoxConfigProfiles.SelectedIndex == -1)
            {
                ShowError("Select a profile to load.");
                return;
            }

            var selectedProfile = listBoxConfigProfiles.SelectedItem.ToString();
            if (selectedProfile == Core.StealthBot.ConfigurationManager.ActiveConfigName)
            {
                ShowError("Select a profile other than the currently loaded profile.");
                return;
            }

			_exitResetEvent.WaitOne();
			Core.StealthBot.ConfigurationManager.LoadConfigProfile(selectedProfile);
        	_exitResetEvent.Set();
        }

        private void ButtonSaveProfiles_Click(object sender, EventArgs e)
        {
            if (Core.StealthBot.Config != null && !string.IsNullOrEmpty(Core.StealthBot.ConfigurationManager.ActiveConfigName))
            {
            	_exitResetEvent.WaitOne();
                Core.StealthBot.ConfigurationManager.SaveConfigProfile(Core.StealthBot.ConfigurationManager.ActiveConfigName);
            	_exitResetEvent.Set();
            }
        }

        private void ComboBoxBotMode_SelectedIndexChanged(object sender, EventArgs e)
        {
        	Core.StealthBot.Config.MainConfig.ActiveBehavior = _botModesByText[((ComboBox) sender).Text];
        }

        private void CheckBoxDisable3dRender_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MainConfig.Disable3DRender = ((CheckBox) sender).Checked;
        }

        private void CheckBoxDisableTextureLoading_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MainConfig.DisableTextureLoading = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDisableUIRender_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MainConfig.DisableUiRender = ((CheckBox)sender).Checked;
        }
        #endregion

        #region Defense Tab
        private void CheckBoxRunOnLowTank_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnLowTank = ((CheckBox)sender).Checked;
        }

        private void TextBoxMinArmorPct_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.DefenseConfig.MinimumArmorPct = value;
            }
        }

        private void TextBoxMinShieldPct_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.DefenseConfig.MinimumShieldPct = value;
            }
        }

        private void TextBoxResumeShieldPct_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.DefenseConfig.ResumeShieldPct = value;
            }
        }

        private void CheckBoxRunOnLowCapacitor_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnLowCap = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnLowAmmo_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnLowAmmo = ((CheckBox)sender).Checked;
        }

        private void TextBoxMinCapPct_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.DefenseConfig.MinimumCapPct = value;
            }
        }

        private void TextBoxResumeCapPct_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.DefenseConfig.ResumeCapPct = value;
            }
        }

        private void CheckBoxRunOnLowDrones_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnLowDrones = ((CheckBox)sender).Checked;
        }

        private void TextBoxMinNumDrones_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.DefenseConfig.MinimumNumDrones = value;
            }
        }

        private void CheckBoxRunOnTargetJammed_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunIfTargetJammed = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnNonWhitelisted_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnNonWhitelistedPilot = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnBlacklist_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnBlacklistedPilot = ((CheckBox)sender).Checked;
        }

        private void CheckBoxWaitAfterFleeing_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.WaitAfterFleeing = ((CheckBox)sender).Checked;
        }

        private void TextBoxMinutesToWait_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.DefenseConfig.MinutesToWait = value;
            }
        }

        private void CheckBoxRunOnMeToPilot_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnMeToPilot = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnMeToCorp_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnMeToCorp = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnCorpToAlliance_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnCorpToAlliance = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnCorpToPilot_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnCorpToPilot = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnCorpToCorp_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnCorpToCorp = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunOnAllianceToAlliance_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.RunOnAllianceToAlliance = ((CheckBox)sender).Checked;
        }

        private void TextBoxMinimumAllianceStanding_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.SocialConfig.MinimumAllianceStanding = value;
            }
        }

        private void TextBoxMinimumCorpStanding_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.SocialConfig.MinimumCorpStanding = value;
            }
        }

        private void TextBoxMinimumPilotStanding_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.SocialConfig.MinimumPilotStanding = value;
            }
        }

        private void CheckBoxPreferStationSafespots_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.PreferStationsOverSafespots = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlwaysShieldBoost_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.AlwaysShieldBoost = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlwaysRunTank_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.AlwaysRunTank = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDisableStandingsChecks_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.DefenseConfig.DisableStandingsChecks = ((CheckBox)sender).Checked;
        }

        private void CheckBoxUseChatReading_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.SocialConfig.UseChatReading = ((CheckBox)sender).Checked;
        }
        #endregion

        #region Whitelist/Blacklist
        private void ButtonRemoveWhitelistPilot_Click(object sender, EventArgs e)
        {
            if (listBoxWhitelistPilots.SelectedItem == null)
            {
                ShowError("Please select a pilot to remove from the whitelist.");
            }
            else
            {
                var selectedItem = listBoxWhitelistPilots.SelectedItem.ToString();
                
                listBoxWhitelistPilots.Items.Remove(selectedItem);
                Core.StealthBot.Config.SocialConfig.PilotWhitelist.Remove(selectedItem);
            }
        }

        private void ButtonRemoveWhitelistCorp_Click(object sender, EventArgs e)
        {
            if (listBoxWhitelistCorps.SelectedItem == null)
            {
                ShowError("Please select a corporation to remove from the whitelist.");
            }
            else
            {
                var selectedItem = listBoxWhitelistCorps.SelectedItem.ToString();
                
                listBoxWhitelistCorps.Items.Remove(selectedItem);
                Core.StealthBot.Config.SocialConfig.CorpWhitelist.Remove(selectedItem);
            }
        }

        private void ButtonRemoveWhitelistAlliance_Click(object sender, EventArgs e)
        {
            if (listBoxWhitelistAlliances.SelectedItem == null)
            {
                ShowError("Please select an alliance to remove from the whitelist.");
            }
            else
            {
                var selectedItem = listBoxWhitelistAlliances.SelectedItem.ToString();

                listBoxWhitelistAlliances.Items.Remove(selectedItem);
                Core.StealthBot.Config.SocialConfig.AllianceWhitelist.Remove(selectedItem);
            }
        }

        private void ButtonRemoveBlacklistPilot_Click(object sender, EventArgs e)
        {
            if (listBoxBlacklistPilots.SelectedItem == null)
            {
                ShowError("Please select a pilot to remove from the blacklist.");
            }
            else
            {
                var selectedItem = listBoxBlacklistPilots.SelectedItem.ToString();

                listBoxBlacklistPilots.Items.Remove(selectedItem);
                Core.StealthBot.Config.SocialConfig.PilotBlacklist.Remove(selectedItem);
            }
        }

        private void ButtonRemoveBlacklistCorp_Click(object sender, EventArgs e)
        {
            if (listBoxBlacklistCorps.SelectedItem == null)
            {
                ShowError("Please select a corporation to remove from the blacklist.");
            }
            else
            {
                var selectedItem = listBoxBlacklistCorps.SelectedItem.ToString();

                listBoxBlacklistCorps.Items.Remove(selectedItem);
                Core.StealthBot.Config.SocialConfig.CorpBlacklist.Remove(selectedItem);
            }
        }

        private void ButtonRemoveBlacklistAlliance_Click(object sender, EventArgs e)
        {
            if (listBoxBlacklistAlliances.SelectedItem == null)
            {
                ShowError("Please select an alliance to remove from the blacklist.");
            }
            else
            {
                var selectedItem = listBoxBlacklistAlliances.SelectedItem.ToString();
                
                listBoxBlacklistAlliances.Items.Remove(selectedItem);
                Core.StealthBot.Config.SocialConfig.AllianceBlacklist.Remove(selectedItem);
            }
        }

        private void ButtonManuallyAdd_Click(object sender, EventArgs e)
        {
            var form = new ManuallyAddPilotForm();
            form.ShowDialog();
        }

        private void ButtonAddWhitelistPilot_Click(object sender, EventArgs e)
        {
            if (listBoxSearchResults.SelectedIndex == -1)
            {
               ShowError("Please select a pilot.");
                return;
            }

            var name = (string)listBoxSearchResults.SelectedItem;
            var id = _idsByName[name];
            var nameToAdd = Core.StealthBot.PilotCache.CachedPilotsById[id].Name;

            if (!listBoxWhitelistPilots.Items.Contains(nameToAdd))
                listBoxWhitelistPilots.Items.Add(nameToAdd);
            if (!Core.StealthBot.Config.SocialConfig.PilotWhitelist.Contains(nameToAdd))
                Core.StealthBot.Config.SocialConfig.PilotWhitelist.Add(nameToAdd);
        }

        private void ButtonAddBlacklistPilot_Click_1(object sender, EventArgs e)
        {
            if (listBoxSearchResults.SelectedIndex == -1)
            {
                ShowError("Please select a pilot.");
                return;
            }

            var name = (string)listBoxSearchResults.SelectedItem;
            var id = _idsByName[name];
            var nameToAdd = Core.StealthBot.PilotCache.CachedPilotsById[id].Name;

            if (!listBoxBlacklistPilots.Items.Contains(nameToAdd))
                listBoxBlacklistPilots.Items.Add(nameToAdd);
            if (!Core.StealthBot.Config.SocialConfig.PilotBlacklist.Contains(nameToAdd))
                Core.StealthBot.Config.SocialConfig.PilotBlacklist.Add(nameToAdd);
        }

        private void ButtonAddWhitelistCorp_Click(object sender, EventArgs e)
        {
            if (listBoxSearchResults.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var name = (string)listBoxSearchResults.SelectedItem;
            var id = _idsByName[name];
            var nameToAdd = string.Empty;

            if (radioButtonDisplayPilotCache.Checked)
            {
                nameToAdd = Core.StealthBot.CorporationCache.CachedCorporationsById[
                    Core.StealthBot.PilotCache.CachedPilotsById[id].CorpID].Name;
            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                nameToAdd = Core.StealthBot.CorporationCache.CachedCorporationsById[id].Name;
            }

            if (nameToAdd == string.Empty)
            {
                ShowError("No valid corporation found in cache for selected pilot.");
                return;
            }
            if (!listBoxWhitelistCorps.Items.Contains(nameToAdd))
                listBoxWhitelistCorps.Items.Add(nameToAdd);
            if (!Core.StealthBot.Config.SocialConfig.CorpWhitelist.Contains(nameToAdd))
                Core.StealthBot.Config.SocialConfig.CorpWhitelist.Add(nameToAdd);
        }

        private void ButtonAddBlacklistCorp_Click(object sender, EventArgs e)
        {
            if (listBoxSearchResults.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var name = (string)listBoxSearchResults.SelectedItem;
            var id = _idsByName[name];
            var nameToAdd = string.Empty;

            if (radioButtonDisplayPilotCache.Checked)
            {
                nameToAdd = Core.StealthBot.CorporationCache.CachedCorporationsById[
                    Core.StealthBot.PilotCache.CachedPilotsById[id].CorpID].Name;

            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                nameToAdd = Core.StealthBot.CorporationCache.CachedCorporationsById[id].Name;
            }

            if (nameToAdd == string.Empty)
            {
                MessageBox.Show("No valid corporation found in cache for selected pilot.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!listBoxBlacklistCorps.Items.Contains(nameToAdd))
                listBoxBlacklistCorps.Items.Add(nameToAdd);
            if (!Core.StealthBot.Config.SocialConfig.CorpBlacklist.Contains(nameToAdd))
                Core.StealthBot.Config.SocialConfig.CorpBlacklist.Add(nameToAdd);
        }

        private void ButtonAddWhitelistAlliance_Click_1(object sender, EventArgs e)
        {
            if (listBoxSearchResults.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var name = (string)listBoxSearchResults.SelectedItem;
            var id = (int)_idsByName[name];
            var nameToAdd = string.Empty;

            if (radioButtonDisplayAllianceCache.Checked)
            {
                if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(id))
                {
                    nameToAdd = Core.StealthBot.AllianceCache.CachedAlliancesById[id].Name;
                }
                else
                {
                    ShowError("Could not find valid alliance entry for the selected alliance.");
                    return;
                }
            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                if (Core.StealthBot.CorporationCache.CachedCorporationsById.ContainsKey(id))
                {
                    var allianceId = Core.StealthBot.CorporationCache.CachedCorporationsById[id].MemberOfAlliance;
                    if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(allianceId))
                    {
                        nameToAdd = Core.StealthBot.AllianceCache.CachedAlliancesById[allianceId].Name;
                    }
                    else
                    {
                        if (allianceId > 0)
                        {
                            ShowError(string.Format("No Alliance entry for ID {0} was found, cannot blacklist. Delete the EVEDB_Alliances.xml and let it rebuild.",
                                allianceId));
                        }
                        else
                        {
                            ShowError(string.Format("Corporation {0} ({1}) does not appear to be part of an alliance.",
                                Core.StealthBot.CorporationCache.CachedCorporationsById[id].Name, id));
                        }
                        return;
                    }
                }
            }
            else if (radioButtonDisplayPilotCache.Checked)
            {
                if (Core.StealthBot.PilotCache.CachedPilotsById.ContainsKey(id))
                {
                    var allianceId = Core.StealthBot.PilotCache.CachedPilotsById[id].AllianceID;
                    if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(allianceId))
                    {
                        nameToAdd = Core.StealthBot.AllianceCache.CachedAlliancesById[allianceId].Name;
                    }
                    else
                    {
                        if (allianceId > 0)
                        {
                            ShowError(string.Format("No Alliance entry for ID {0} was found, cannot blacklist. Delete the EVEDB_Alliances.xml and let it rebuild.",
                                allianceId));
                        }
                        else
                        {
                            ShowError(string.Format("Pilot {0} ({1}) does not appear to be part of an alliance.",
                                Core.StealthBot.PilotCache.CachedPilotsById[id].Name, id));
                        }
                        return;
                    }
                }
            }

            if (!listBoxWhitelistAlliances.Items.Contains(nameToAdd))
                listBoxWhitelistAlliances.Items.Add(nameToAdd);
            if (!Core.StealthBot.Config.SocialConfig.AllianceWhitelist.Contains(nameToAdd))
                Core.StealthBot.Config.SocialConfig.AllianceWhitelist.Add(nameToAdd);
        }

        private void ButtonAddBlacklistAlliance_Click(object sender, EventArgs e)
        {
            if (listBoxSearchResults.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an entry.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var name = (string)listBoxSearchResults.SelectedItem;
            var id = (int)_idsByName[name];
            var nameToAdd = string.Empty;

            if (radioButtonDisplayAllianceCache.Checked)
            {
                if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(id))
                {
                    nameToAdd = Core.StealthBot.AllianceCache.CachedAlliancesById[id].Name;
                }
                else
                {
                    ShowError("Could not find valid alliance entry for the selected alliance.");
                    return;
                }
            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                if (Core.StealthBot.CorporationCache.CachedCorporationsById.ContainsKey(id))
                {
                    var allianceId = Core.StealthBot.CorporationCache.CachedCorporationsById[id].MemberOfAlliance;
                    if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(allianceId))
                    {
                        nameToAdd = Core.StealthBot.AllianceCache.CachedAlliancesById[allianceId].Name;
                    }
                    else
                    {
                        if (allianceId > 0)
                        {
                            ShowError(string.Format("No Alliance entry for ID {0} was found, cannot blacklist. Delete the EVEDB_Alliances.xml and let it rebuild.",
                                allianceId));
                        }
                        else
                        {
                            ShowError(string.Format("Corporation {0} ({1}) does not appear to be part of an alliance.",
                                Core.StealthBot.CorporationCache.CachedCorporationsById[id].Name, id));
                        }
                        return;
                    }
                }
            }
            else if (radioButtonDisplayPilotCache.Checked)
            {
                if (Core.StealthBot.PilotCache.CachedPilotsById.ContainsKey(id))
                {
                    var allianceId = Core.StealthBot.PilotCache.CachedPilotsById[id].AllianceID;
                    if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(allianceId))
                    {
                        nameToAdd = Core.StealthBot.AllianceCache.CachedAlliancesById[allianceId].Name;
                    }
                    else
                    {
                        if (allianceId > 0)
                        {
                            ShowError(string.Format("No Alliance entry for ID {0} was found, cannot blacklist. Delete the EVEDB_Alliances.xml and let it rebuild.",
                                allianceId));
                        }
                        else
                        {
                            ShowError(string.Format("Pilot {0} ({1}) does not appear to be part of an alliance.",
                                Core.StealthBot.PilotCache.CachedPilotsById[id].Name, id));
                        }
                        return;
                    }
                }
            }

            if (!listBoxBlacklistAlliances.Items.Contains(nameToAdd))
                listBoxBlacklistAlliances.Items.Add(nameToAdd);
            if (!Core.StealthBot.Config.SocialConfig.AllianceBlacklist.Contains(nameToAdd))
                Core.StealthBot.Config.SocialConfig.AllianceBlacklist.Add(nameToAdd);
        }

        private void RadioButtonDisplayPilotCache_CheckedChanged(object sender, EventArgs e)
        {
            //Reset the cachedpilot index on check changed
            _pilotCacheIndex = 0;

            //Clear the localPilots listbox and the lookup table
            listBoxSearchResults.Items.Clear();
            _idsByName.Clear();

            //Flip visibilities
            if (radioButtonDisplayAllianceCache.Checked)
            {
                buttonAddBlacklistPilot.Visible = false;
                buttonAddWhitelistPilot.Visible = false;
                buttonAddWhitelistCorp.Visible = false;
                buttonAddBlacklistCorp.Visible = false;
            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                buttonAddWhitelistPilot.Visible = false;
                buttonAddBlacklistPilot.Visible = false;
                buttonAddWhitelistCorp.Visible = true;
                buttonAddBlacklistCorp.Visible = true;
            }
            else if (radioButtonDisplayPilotCache.Checked)
            {
                buttonAddWhitelistPilot.Visible = true;
                buttonAddBlacklistPilot.Visible = true;
                buttonAddWhitelistCorp.Visible = true;
                buttonAddBlacklistCorp.Visible = true;
            }
        }

        private void TextBoxSearchCache_TextChanged(object sender, EventArgs e)
        {
            var searchString = textBoxSearchCache.Text;
            if (searchString != string.Empty)
            {
                for (var idx = 0; idx < listBoxSearchResults.Items.Count; idx++)
                {
                    if (listBoxSearchResults.Items[idx].ToString().StartsWith(searchString, StringComparison.InvariantCultureIgnoreCase))
                    {
                        listBoxSearchResults.SelectedIndex = idx;
                        break;
                    }
                }
            }
        }
        #endregion

        #region Bookmarks

        private void TextBoxSafeBookmarkPrefix_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.SafeBookmarkPrefix = ((TextBox)sender).Text;
        }

        private void TextBoxSalvagingPrefix_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.SalvagingPrefix = ((TextBox)sender).Text;
        }

        private void TextBoxAsteroidBeltBookmarkPrefix_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.AsteroidBeltBookmarkPrefix = ((TextBox)sender).Text;
        }

        private void TextBoxIceBeltBookmarkPrefix_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.IceBeltBookmarkPrefix = ((TextBox)sender).Text;
        }

        private void TextBoxTemporaryBeltBookMark_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.TemporaryBeltBookMarkPrefix = ((TextBox)sender).Text;
        }

        private void TextBoxTemporaryCanPrefix_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.TemporaryCanBookMarkPrefix = ((TextBox)sender).Text;
        }
        #endregion

        #region Movement
        private void CheckBoxBounceWarp_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.UseBounceWarp = ((CheckBox)sender).Checked;
        }

        private void TextBoxMaxSlowboatTime_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MovementConfig.MaxSlowboatTime = value;
            }
        }

        private void CheckBoxUseTempBeltBookmarks_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.UseTempBeltBookmarks = ((CheckBox)sender).Checked;
        }

        private void CheckBoxOnlyUseBookMarkedBelts_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.OnlyUseBeltBookmarks = ((CheckBox)sender).Checked;
        }

        private void CheckBoxMoveToRandomBelts_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.UseRandomBeltOrder = ((CheckBox)sender).Checked;
        }

        private void CheckBoxUseBeltSubsets_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.UseBeltSubsets = ((CheckBox)sender).Checked;
        }

        private void TextBoxNumBeltsInSubset_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MovementConfig.NumBeltsInSubset = value;
            }
        }

        private void ComboBoxBeltSubsetMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)((ComboBox)sender).SelectedItem)
            {
                case "First":
                    Core.StealthBot.Config.MovementConfig.BeltSubsetMode = BeltSubsetModes.First;
                    break;
                case "Middle":
                    Core.StealthBot.Config.MovementConfig.BeltSubsetMode = BeltSubsetModes.Middle;
                    break;
                case "Last":
                    Core.StealthBot.Config.MovementConfig.BeltSubsetMode = BeltSubsetModes.Last;
                    break;
                default: break;
            }
        }

        private void TextBoxPropModMinCapPct_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MovementConfig.PropModMinCapPct = value;
            }
        }

        private void TextBoxPropModResumeCapPct_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MovementConfig.PropModResumeCapPct = value;
            }
        }

        private void TextBoxStartBookMark_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.JumpStabilityTestStartBookmark = ((TextBox)sender).Text;
        }

        private void TextBoxEndBookMark_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.JumpStabilityTestEndBookmark = ((TextBox)sender).Text;
        }

        private void CheckBoxUseCustomOrbitDistance_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.UseCustomOrbitDistance = ((CheckBox)sender).Checked;
        }

        private void TextBoxOrbitDistance_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MovementConfig.CustomOrbitDistance = value;
            }
        }

        private void CheckBoxKeepAtRange_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MovementConfig.UseKeepAtRangeInsteadOfOrbit = ((CheckBox) sender).Checked;
        }
        #endregion

        #region Cargo
        private void ComboBoxDropoffType_SelectedIndexChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.CargoConfig.DropoffLocation.LocationType = _locationTypesByText[((ComboBox)sender).Text];
        }

        private void TextBoxDropoffBookmarkLabel_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.CargoConfig.DropoffLocation.BookmarkLabel = ((TextBox)sender).Text;
        }

        private void TextBoxDropoffID_TextChanged(object sender, EventArgs e)
        {
            long value;
            if (long.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.CargoConfig.DropoffLocation.EntityID = value;
            }
        }

        private void TextBoxDropoffHangarDivision_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (!int.TryParse(((TextBox) sender).Text, out value)) return;

            if (value < 1 || value > 7)
                ShowError("Hangar Division must be between 1 and 7, inclusive.");
            else
                Core.StealthBot.Config.CargoConfig.DropoffLocation.HangarDivision = value;
        }

        private void ComboBoxPickupType_SelectedIndexChanged(object sender, EventArgs e)
        {
			Core.StealthBot.Config.CargoConfig.PickupLocation.LocationType = _locationTypesByText[((ComboBox)sender).Text];
        }

        private void TextBoxPickupName_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.CargoConfig.PickupLocation.BookmarkLabel = ((TextBox)sender).Text;
        }

        private void TextBoxPickupID_TextChanged(object sender, EventArgs e)
        {
            long value;
            if (long.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.CargoConfig.PickupLocation.EntityID = value;
            }
        }

        private void TextPickupHangarDivision_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (!int.TryParse(((TextBox)sender).Text, out value)) return;

            if (value < 1 || value > 7)
                ShowError("Hangar Division must be between 1 and 7, inclusive.");
            else
                Core.StealthBot.Config.CargoConfig.PickupLocation.HangarDivision = value;
        }

        private void TextBoxPickupSystemBookmark_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.CargoConfig.PickupSystemBookmark = ((TextBox) sender).Text;
        }

        private void TextBoxCargoFullThreshold_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.CargoConfig.CargoFullThreshold = value;
            }
        }

        private void TextBoxJetcanNameFormat_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.CargoConfig.CanNameFormat = ((TextBox) sender).Text;
        }

        private void CheckBoxAlwaysPopCans_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.CargoConfig.AlwaysPopCans = ((CheckBox) sender).Checked;
        }
        #endregion

        #region Max Runtime
        private void CheckBoxUseMaxRuntime_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MaxRuntimeConfig.UseMaxRuntime = ((CheckBox) sender).Checked;
        }

        private void TextBoxMaxRuntime_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MaxRuntimeConfig.MaxRuntimeMinutes = value;
            }
        }

        private void CheckBoxResumeAfter_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MaxRuntimeConfig.ResumeAfterWaiting = ((CheckBox) sender).Checked;
        }

        private void TextBoxResumeAfter_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MaxRuntimeConfig.ResumeWaitMinutes = value;
            }
        }

        private void CheckBoxUseRandomWaits_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MaxRuntimeConfig.UseRandomWaits = ((CheckBox) sender).Checked;
        }

        private void CheckBoxUseRelaunching_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MaxRuntimeConfig.UseRelaunching = ((CheckBox) sender).Checked;
        }

        private void TextBoxCharacterSetToLaunch_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MaxRuntimeConfig.CharacterSetToRelaunch = ((TextBox) sender).Text;
        }

        private void CheckBoxRelaunchAfterDowntime_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MaxRuntimeConfig.RelaunchAfterDowntime = ((CheckBox) sender).Checked;
        }
        #endregion

        #region Fleet
        private void CheckBoxDoFleetInvites_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.FleetConfig.DoFleetInvites = ((CheckBox) sender).Checked;
        }

        private void CheckBoxOnlyHaulForListedMembers_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.FleetConfig.OnlyHaulForSkipList = ((CheckBox) sender).Checked;
        }

        private void ButtonFleetAddCharID_Click(object sender, EventArgs e)
        {
            Int64 charId;
            if (Int64.TryParse(textBoxFleetCharID.Text, out charId))
            {
                if (!Core.StealthBot.Config.FleetConfig.BuddyCharIDsToInvite.Contains(charId))
                {
                    listBoxFleetCharIDs.Items.Add(charId);
                    Core.StealthBot.Config.FleetConfig.BuddyCharIDsToInvite.Add(charId);
                    textBoxFleetCharID.Clear();
                }
                else
                {
                    ShowError(String.Format("You have already added CharID {0}.", charId));
                }
            }
            else
            {
                ShowError(String.Format("Unable to parse CharID {0}. Make sure it is numeric and has no text.",
                    textBoxFleetCharID.Text));
            }
        }

        private void ButtonFleetAddSkipCharID_Click(object sender, EventArgs e)
        {
            Int64 charID;
            if (Int64.TryParse(textBoxFleetCharIDSkip.Text, out charID))
            {
                if (!Core.StealthBot.Config.FleetConfig.FleetCharIDsToSkip.Contains(charID))
                {
                    listBoxFleetCharIDsToSkip.Items.Add(charID);
                    Core.StealthBot.Config.FleetConfig.FleetCharIDsToSkip.Add(charID);
                    textBoxFleetCharIDSkip.Clear();
                }
                else
                {
                    ShowError(String.Format("You have already added CharID {0}.", charID));
                }
            }
            else
            {
                ShowError(String.Format("Unable to parse CharID {0}. Make sure it is numeric and has no text.",
                    textBoxFleetCharID.Text));
            }
        }

        private void ListBoxFleetCharIDs_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            ButtonFleetRemoveCharID_Click(this, new EventArgs());
        }

        private void ButtonFleetRemoveCharID_Click(object sender, EventArgs e)
        {
            if (listBoxFleetCharIDs.SelectedIndex == -1)
            {
                ShowError("Please select a CharID to remove.");
                return;
            }

            Int64 charID;
            if (!Int64.TryParse(listBoxFleetCharIDs.SelectedItem.ToString(), out charID))
                return;

            Core.StealthBot.Config.FleetConfig.BuddyCharIDsToInvite.Remove(charID);
            listBoxFleetCharIDs.Items.Remove(listBoxFleetCharIDs.SelectedItem);

            if (listBoxFleetCharIDs.Items.Count > 0)
                listBoxFleetCharIDs.SelectedIndex = 0;
        }

        private void ButtonFleetRemoveSkipCharID_Click(object sender, EventArgs e)
        {
            if (listBoxFleetCharIDsToSkip.SelectedIndex == -1)
            {
                ShowError("Please select a CharID to remove.");
                return;
            }

            Int64 charID;
            if (!Int64.TryParse(listBoxFleetCharIDsToSkip.SelectedItem.ToString(), out charID))
                return;

            Core.StealthBot.Config.FleetConfig.FleetCharIDsToSkip.Remove(charID);
            listBoxFleetCharIDsToSkip.Items.Remove(listBoxFleetCharIDsToSkip.SelectedItem);

            if (listBoxFleetCharIDsToSkip.Items.Count > 0)
                listBoxFleetCharIDsToSkip.SelectedIndex = 0;
        }

        private void ListBoxFleetCharIDsToSkip_KeyUp(object sender, KeyEventArgs e)
        {
            ButtonFleetRemoveSkipCharID_Click(this, new EventArgs());
        }

        private void TextBoxFleetCharID_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            ButtonFleetAddCharID_Click(this, e);
        }

        private void TextBoxFleetCharIDSkip_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            ButtonFleetAddSkipCharID_Click(this, e);
        }
        #endregion 

        #region Alerts
        private void CheckBoxUseAlerts_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.UseAlerts = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertLocalUnsafe_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnLocalUnsafe = ((CheckBox) sender).Checked;
        }

        private void CheckBoxAlertLocalChat_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnLocalChat = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertFactionSpawn_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnFactionSpawn = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertLowAmmo_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnLowAmmo = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertFreighterNoPickup_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnFreighterNoPickup = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertPlayerNear_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnPlayerNear = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertLongRandomWait_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnLongRandomWait = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertTargetJammed_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnTargetJammed = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertFlee_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnFlee = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAlertWarpJammed_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.AlertConfig.AlertOnWarpJammed = ((CheckBox)sender).Checked;
        }
        #endregion

        #region Mining
        private void CheckedListBoxOrePriorities_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var name = (string)((CheckedListBox)sender).Items[e.Index];

            Core.StealthBot.Config.MiningConfig.StatusByOre[name] = e.NewValue == CheckState.Checked;
        }

        private void CheckedListBoxIcePriorities_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var listBox = (CheckedListBox) sender;
            var name = (string)listBox.Items[e.Index];

            Core.StealthBot.Config.MiningConfig.StatusByIce[name] = e.NewValue == CheckState.Checked;
        }

        private void ButtonOreIncreasePriority_Click(object sender, EventArgs e)
        {
            if (checkedListBoxOrePriorities.SelectedIndex == -1)
            {
                MessageBox.Show("Select an ore to increase the priority of.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var idx = checkedListBoxOrePriorities.SelectedIndex;
            if (idx == 0)
            {
                MessageBox.Show("Cannot further increase priority of selected ore.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var temp = string.Empty;
            temp = Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx];
            Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx] =
                Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx - 1];
            Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx - 1] = temp;


            object oldOre = null;
            oldOre = checkedListBoxOrePriorities.Items[idx];
            var isChecked = checkedListBoxOrePriorities.CheckedItems.Contains(oldOre);
            checkedListBoxOrePriorities.Items.Remove(oldOre);
            checkedListBoxOrePriorities.Items.Insert(idx - 1, oldOre);
            checkedListBoxOrePriorities.SetItemChecked(idx - 1, isChecked);
            checkedListBoxOrePriorities.SelectedIndex = idx - 1;
        }

        private void ButtonOreLowerPriority_Click(object sender, EventArgs e)
        {
            if (checkedListBoxOrePriorities.SelectedIndex == -1)
            {
                MessageBox.Show("Select an ore to decrease the priority of.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var idx = checkedListBoxOrePriorities.SelectedIndex;
            if (idx + 1 == checkedListBoxOrePriorities.Items.Count)
            {
                MessageBox.Show("Cannot further decrease priority of selected ice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //Swap the two items in the priority list
            var temp = string.Empty;
            temp = Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx];
            Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx] =
                Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx + 1];
            Core.StealthBot.Config.MiningConfig.PriorityByOreType[idx + 1] = temp;

            object oldOre = null;
            oldOre = checkedListBoxOrePriorities.Items[idx];
            var isChecked = checkedListBoxOrePriorities.CheckedItems.Contains(
                oldOre);
            checkedListBoxOrePriorities.Items.Remove(oldOre);
            checkedListBoxOrePriorities.Items.Insert(idx + 1, oldOre);
            checkedListBoxOrePriorities.SetItemChecked(idx + 1, isChecked);
            checkedListBoxOrePriorities.SelectedIndex = idx + 1;
        }

        private void ButtonIceIncreasePriority_Click(object sender, EventArgs e)
        {
            if (checkedListBoxIcePriorities.SelectedIndex == -1)
            {
                MessageBox.Show("Select an ice to increase the priority of.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var idx = checkedListBoxIcePriorities.SelectedIndex;
            if (idx == 0)
            {
                MessageBox.Show("Cannot further increase priority of selected ice.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var temp = string.Empty;
            temp = Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx];
            Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx] =
                Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx - 1];
            Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx - 1] = temp;


            object oldIce = null;
            oldIce = checkedListBoxIcePriorities.Items[idx];
            var isChecked = checkedListBoxIcePriorities.CheckedItems.Contains(oldIce);
            checkedListBoxIcePriorities.Items.Remove(oldIce);
            checkedListBoxIcePriorities.Items.Insert(idx - 1, oldIce);
            checkedListBoxIcePriorities.SetItemChecked(idx - 1, isChecked);
            checkedListBoxIcePriorities.SelectedIndex = idx - 1;
        }

        private void ButtonIceDecreasePriority_Click(object sender, EventArgs e)
        {
            if (checkedListBoxIcePriorities.SelectedIndex == -1)
            {
                MessageBox.Show("Select an ice to decrease the priority of.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var idx = checkedListBoxIcePriorities.SelectedIndex;
            if (idx + 1 == checkedListBoxIcePriorities.Items.Count)
            {
                MessageBox.Show("Cannot further decrease selected ice priority.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var temp = string.Empty;
            temp = Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx];
            Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx] =
                Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx + 1];
            Core.StealthBot.Config.MiningConfig.PriorityByIceType[idx + 1] = temp;


            object oldIce = null;
            oldIce = checkedListBoxIcePriorities.Items[idx];
            var isChecked = checkedListBoxIcePriorities.CheckedItems.Contains(oldIce);
            checkedListBoxIcePriorities.Items.Remove(oldIce);
            checkedListBoxIcePriorities.Items.Insert(idx + 1, oldIce);
            checkedListBoxIcePriorities.SetItemChecked(idx + 1, isChecked);
            checkedListBoxIcePriorities.SelectedIndex = idx + 1;
        }

        private void CheckBoxUseMiningDrones_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MiningConfig.UseMiningDrones = ((CheckBox)sender).Checked;
        }

        private void CheckBoxStripMine_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MiningConfig.StripMine = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDistributeLasers_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MiningConfig.DistributeLasers = ((CheckBox)sender).Checked;
        }

        private void CheckBoxShortCycle_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MiningConfig.ShortCycle = ((CheckBox)sender).Checked;
        }

        private void CheckBoxIceMining_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MiningConfig.IsIceMining = ((CheckBox)sender).Checked;
        }

        private void TextBoxNumCrystalsToCarry_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MiningConfig.NumCrystalsToCarry = value;
            }
        }

        private void TextBoxMinDistanceToPlayers_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.MiningConfig.MinDistanceToPlayers = value;
            }
        }

        private void BoostLocationLabelTextBox_TextChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MiningConfig.BoostOrcaBoostLocationLabel = ((TextBox) sender).Text;
        }
        #endregion

        #region Hauler
        private void ComboBoxHaulerMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.HaulingConfig.HaulerMode = _haulerModesByText[((ComboBox)sender).Text];
        }
        private void TextboxCycleFleetDelay_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.HaulingConfig.HaulerCycleFleetDelay = value;
            }
        }
        #endregion

        #region Missions
        private void CheckBoxRunCourierMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.RunCourierMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunTradeMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.RunTradeMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunEncounterMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.RunEncounterMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxRunMiningMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.RunMiningMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxAvoidLowsecMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.AvoidLowSec = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDoOreMiningMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.DoOreMiningMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDoIceMiningMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.DoIceMiningMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDoGasMiningMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.DoGasMiningMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxKillEmpireFactions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.DoEmpireKillMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxKillPirateFactions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.DoPirateKillMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDoStorylineMissions_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.DoStorylineMissions = ((CheckBox)sender).Checked;
        }

        private void CheckBoxDoChainCouriers_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.DoChainCouriers = ((CheckBox)sender).Checked;
        }

        private void ButtonAgentAdd_Click(object sender, EventArgs e)
        {
            var agentName = textBoxAgentName.Text;

			if (string.IsNullOrWhiteSpace(agentName)) return;

            if (!Core.StealthBot.Config.MissionConfig.Agents.Contains(agentName))
            {
                Core.StealthBot.Config.MissionConfig.Agents.Add(agentName);
                listBoxAgents.Items.Add(agentName);
                textBoxAgentName.Clear();
                textBoxAgentName.Focus();
            }
            else
            {
                ShowError(String.Format("Agent \"{0}\" already added!", agentName));
            }
        }

        private void ButtonRemoveAgent_Click(object sender, EventArgs e)
        {
            if (listBoxAgents.SelectedIndex == -1)
            {
                MessageBox.Show("You must select an agent to remove.");
            }
            else
            {
                var agent = (string)listBoxAgents.SelectedItem;
                Core.StealthBot.Config.MissionConfig.Agents.Remove(agent);
                listBoxAgents.Items.Remove(agent);

                if (listBoxAgents.Items.Count > 0)
                    listBoxAgents.SelectedIndex = 0;
            }
        }

        private void ButtonAgentIncreasePriority_Click(object sender, EventArgs e)
        {
            if (listBoxAgents.SelectedIndex == -1)
            {
                ShowError("Select an agent to increase the priority of.");
                return;
            }

            var idx = listBoxAgents.SelectedIndex;
            if (idx == 0)
            {
                ShowError("Cannot further increase priority of selected agent.");
                return;
            }

            var temp = string.Empty;
            temp = Core.StealthBot.Config.MissionConfig.Agents[idx];
            Core.StealthBot.Config.MissionConfig.Agents[idx] =
                Core.StealthBot.Config.MissionConfig.Agents[idx - 1];
            listBoxAgents.Items[idx] = listBoxAgents.Items[idx - 1];
            Core.StealthBot.Config.MissionConfig.Agents[idx - 1] = temp;
            listBoxAgents.Items[idx - 1] = temp;
            listBoxAgents.SelectedIndex = idx - 1;
        }

        private void ButtonDecreaseAgentPriority_Click(object sender, EventArgs e)
        {
            if (listBoxAgents.SelectedIndex == -1)
            {
                ShowError("Select an agent to decrease the priority of.");
                return;
            }

            int idx = listBoxAgents.SelectedIndex;
            if (idx + 1 == listBoxAgents.Items.Count)
            {
                ShowError("Cannot further decrease priority of selected agent.");
                return;
            }

            var temp = string.Empty;
            temp = Core.StealthBot.Config.MissionConfig.Agents[idx];
            Core.StealthBot.Config.MissionConfig.Agents[idx] =
                Core.StealthBot.Config.MissionConfig.Agents[idx + 1];
            listBoxAgents.Items[idx] = listBoxAgents.Items[idx + 1];
            Core.StealthBot.Config.MissionConfig.Agents[idx + 1] = temp;
            listBoxAgents.Items[idx + 1] = temp;
            listBoxAgents.SelectedIndex = idx + 1;
        }

        private void ButtonAddResearchAgent_Click(object sender, EventArgs e)
        {
            var agentName = textBoxResearchAgentName.Text;

			if (string.IsNullOrWhiteSpace(agentName)) return;

            if (!Core.StealthBot.Config.MissionConfig.ResearchAgents.Contains(agentName))
            {
                Core.StealthBot.Config.MissionConfig.ResearchAgents.Add(agentName);
                listBoxResearchAgents.Items.Add(agentName);
                textBoxResearchAgentName.Clear();
                textBoxResearchAgentName.Focus();
            }
            else
            {
                ShowError(String.Format("Research agent \"{0}\" already added!", agentName));
            }
        }

        private void ButtonRemoveRAgent_Click(object sender, EventArgs e)
        {
            if (listBoxResearchAgents.SelectedIndex == -1)
            {
                MessageBox.Show("You must select a research agent to remove.");
            }
            else
            {
                var agent = (string)listBoxResearchAgents.SelectedItem;
                Core.StealthBot.Config.MissionConfig.ResearchAgents.Remove(agent);
                listBoxResearchAgents.Items.Remove(agent);

                if (listBoxResearchAgents.Items.Count > 0)
                    listBoxResearchAgents.SelectedIndex = 0;
            }
        }

        private void ButtonIncreaseRAgentPriority_Click(object sender, EventArgs e)
        {
            if (listBoxResearchAgents.SelectedIndex == -1)
            {
                ShowError("Select a research agent to increase the priority of.");
                return;
            }

            var idx = listBoxResearchAgents.SelectedIndex;
            if (idx == 0)
            {
                ShowError("Cannot further increase priority of selected research agent.");
                return;
            }

            var temp = string.Empty;
            temp = Core.StealthBot.Config.MissionConfig.ResearchAgents[idx];
            Core.StealthBot.Config.MissionConfig.ResearchAgents[idx] =
                Core.StealthBot.Config.MissionConfig.ResearchAgents[idx - 1];
            listBoxResearchAgents.Items[idx] = listBoxResearchAgents.Items[idx - 1];
            Core.StealthBot.Config.MissionConfig.ResearchAgents[idx - 1] = temp;
            listBoxResearchAgents.Items[idx - 1] = temp;
            listBoxResearchAgents.SelectedIndex = idx - 1;
        }

        private void ButtonDereaseRAgentPriority_Click(object sender, EventArgs e)
        {
            if (listBoxResearchAgents.SelectedIndex == -1)
            {
                ShowError("Select a research agent to decrease the priority of.");
                return;
            }

            var idx = listBoxResearchAgents.SelectedIndex;
            if (idx + 1 == listBoxResearchAgents.Items.Count)
            {
                ShowError("Cannot further decrease priority of selected research agent.");
                return;
            }

            var temp = string.Empty;
            temp = Core.StealthBot.Config.MissionConfig.ResearchAgents[idx];
            Core.StealthBot.Config.MissionConfig.ResearchAgents[idx] =
                Core.StealthBot.Config.MissionConfig.ResearchAgents[idx + 1];
            listBoxResearchAgents.Items[idx] = listBoxResearchAgents.Items[idx + 1];
            Core.StealthBot.Config.MissionConfig.ResearchAgents[idx + 1] = temp;
            listBoxResearchAgents.Items[idx + 1] = temp;
            listBoxResearchAgents.SelectedIndex = idx + 1;
        }

        private void ButtonAddBlacklistedMission_Click(object sender, EventArgs e)
        {
            var mission = textBoxBlacklistedMission.Text;

			if (string.IsNullOrWhiteSpace(mission)) return;

            if (!listBoxMissionBacklist.Items.Contains(mission))
            {
                listBoxMissionBacklist.Items.Add(mission);
                Core.StealthBot.Config.MissionConfig.MissionBlacklist.Add(mission);
            }

            textBoxBlacklistedMission.Clear();
            textBoxBlacklistedMission.Focus();
        }

        private void ButtonRemoveBlacklistedMission_Click(object sender, EventArgs e)
        {
            var index = listBoxMissionBacklist.SelectedIndex;

            if (index < 0)
            {
                ShowError("Select a blacklisted mission to remove.");
                return;
            }

            var mission = (string)listBoxMissionBacklist.Items[index];

            listBoxMissionBacklist.Items.RemoveAt(index);
            Core.StealthBot.Config.MissionConfig.MissionBlacklist.Remove(mission);

            if (listBoxMissionBacklist.Items.Count > index)
            {
                listBoxMissionBacklist.SelectedIndex = index;
            }
        }

        private void TextBoxAgentName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            ButtonAgentAdd_Click(this, new EventArgs());
        }

        private void TextBoxResearchAgentName_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            ButtonAddResearchAgent_Click(this, new EventArgs());
        }

        private void TextBoxBlacklistedMission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            ButtonAddBlacklistedMission_Click(this, new EventArgs());
        }

        private void ListBoxAgents_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            ButtonRemoveAgent_Click(this, new EventArgs());
        }

        private void ListBoxResearchAgents_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            ButtonRemoveRAgent_Click(this, new EventArgs());
        }

        private void ListBoxMissionBacklist_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            ButtonRemoveBlacklistedMission_Click(this, new EventArgs());
        }

        private void CheckBoxIgnoreMissionDeclineTimer_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.MissionConfig.IgnoreMissionDeclineTimer = ((CheckBox)sender).Checked;
        }
        #endregion

        #region Ratting
        private void CheckBoxChainBelts_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.RattingConfig.ChainBelts = ((CheckBox) sender).Checked;
        }

        private void TextBoxMinChainBounty_TextChanged(object sender, EventArgs e)
        {
            int value;
            if (int.TryParse(((TextBox)sender).Text, out value))
            {
                Core.StealthBot.Config.RattingConfig.MinimumChainBounty = value;
            }
        }

        private void CheckBoxOnlyChainSolo_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.RattingConfig.OnlyChainWhenAlone = ((CheckBox)sender).Checked;
        }

        private void CheckBoxIsAnomalyMode_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.RattingConfig.IsAnomalyMode = ((CheckBox)sender).Checked;
        }

        private void AnomalyStatusByNameCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var listBox = (CheckedListBox)sender;
            var name = (string)listBox.Items[e.Index];

            var pair = Core.StealthBot.Config.RattingConfig.StatusByAnomalyType.First(t => t.First == name);
            pair.Second = e.NewValue == CheckState.Checked;
        }
        #endregion

        #region Salvaging

        private void CheckBoxSaveBookmarksForCorporation_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.SalvageConfig.SaveBookmarksForCorporation = ((CheckBox)sender).Checked;
        }

        private void CheckBoxEnableSalvagingBM_CheckedChanged(object sender, EventArgs e)
        {
            Core.StealthBot.Config.SalvageConfig.CreateSalvageBookmarks = ((CheckBox)sender).Checked;
        }

        #endregion
        #endregion

        #region Other Control Event Handlers
        private void ButtonStartResume_Click(object sender, EventArgs e)
        {
            if (TextBoxAuthEmailAddress.Text == string.Empty)
            {
                ShowError("Please enter your email address for authentication.");
                return;
            }

            if (TextBoxAuthEmailAddress.Text.Contains('\'') ||
                TextBoxAuthEmailAddress.Text.Contains('\"'))
            {
                ShowError("Please remove all quotation marks from your email address.");
                return;
            }

            if (TextBoxAuthPassword.Text == string.Empty)
            {
                ShowError("Please enter your password for authentication.");
                return;
            }

            if (TextBoxAuthPassword.Text.Contains('\'') ||
                TextBoxAuthPassword.Text.Contains('\"'))
            {
                ShowError("Please remove all quotation marks from your password.");
                return;
            }

            if (!_authStarted)
            {
                Core.StealthBot.Logging.LogMessage("StealthBotForm", "Authentication", LogSeverityTypes.Standard,
                    "Authenticating...");

                _auth.TryLogin(TextBoxAuthEmailAddress.Text, TextBoxAuthPassword.Text);
                _authStarted = true;
            }

            if (!_authCompleted)
                return;

            Core.StealthBot.Instance.IsEnabled = true;

            if (ButtonStartResume.Text.Equals("Resume") ||
                ButtonStartResume.Text.Equals("Start"))
            {
                ButtonPause.BackColor = Color.Green;
                ButtonStartResume.Text = "Resume";
                Core.StealthBot.JustLoadConfig = false;
                Core.StealthBot.Logging.LogMessage("StealthBotForm", "Start", LogSeverityTypes.Standard,
                    "Enabling full pulse");
            }
            else
            {
                SetButtonsAndLabels();
            }
        }

        private void ButtonPause_Click(object sender, EventArgs e)
        {
            if (Core.StealthBot.Instance.IsEnabled)
            {
                ButtonPause.BackColor = Color.Orange;
                Core.StealthBot.Instance.IsEnabled = false;
                Core.StealthBot.Logging.LogMessage("StealthBotForm", "Pause", LogSeverityTypes.Standard,
                    "Pausing");
            }
        }

        private void ButtonChangeBuild_Click(object sender, EventArgs e)
        {
            Core.StealthBot.Instance.IsEnabled = false;

            if (!Core.StealthBot.CanExit) return;

#if DEBUG
            LavishScript.ExecuteCommand(
                String.Format("dotnet {0} isxGamesPatcher {0} {1} http://stealthsoftware.net/software/stealthbot/isxGamesPatcher_StealthBot.xml",
                              Application.ProductName, 0));
#else
			LavishScript.ExecuteCommand(
				String.Format("dotnet {0} isxGamesPatcher {0} {1} http://stealthsoftware.net/software/stealthbot-test/isxGamesPatcher_StealthBot-Test.xml",
				Application.ProductName, 0));
#endif

            LavishScript.ExecuteCommand("TimedCommand 75 dotnet sb${Session} stealthbot");
            Close();
        }

        private void LstBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            //
            // Draw the background of the ListBox control for each item.
            // Create a new Brush and initialize to a Black colored brush
            // by default.
            //
            e.DrawBackground();
            var myBrush = Brushes.Black;
            if (e.Index < 0)
            {
                return;
            }

            Int64 id = -1;
            var value = (string)((ListBox)sender).Items[e.Index];
            if (_idsByName.ContainsKey(value))
            {
                id = _idsByName[value];
            }
            else
            {
                //Missing Corp or Alliance
                if (radioButtonDisplayCorporationCache.Checked)
                {
                    Core.StealthBot.CorporationCache.GetCorporationInfo(id);
                    return;
                }
            }
            CachedStanding standing = null;

            var alliance = string.Empty;
            var corp = string.Empty;
            var name = string.Empty;

            if (radioButtonDisplayPilotCache.Checked)
            {
                var cp = Core.StealthBot.PilotCache.CachedPilotsById[id];
                standing = cp.Standing;

                alliance = cp.Alliance;
                corp = cp.Corp;
                name = cp.Name;

                if (cp.CorpID == Core.StealthBot.MeCache.CorporationId)
                {
                    myBrush = Brushes.Green;
                }
                else if (Core.StealthBot.MeCache.AllianceId > 0 && cp.AllianceID > 0)
                {
                    if (cp.Alliance == Core.StealthBot.MeCache.Alliance)
                    {
                        myBrush = Brushes.Blue;
                    }
                }
            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                if (Core.StealthBot.CorporationCache.CachedCorporationsById.ContainsKey(id))
                {
                    CachedCorporation cc = Core.StealthBot.CorporationCache.CachedCorporationsById[id];
                    //Core.StealthBot.Logging.LogMessage(ObjectName, new StealthBot.Core.LogEventArgs(StealthBot.Core.LogSeverityTypes.Debug,
                    //    String.Format("DrawItem: Corp - {0} ({1})}",
                    //    cc.Name, cc.CorporationID)));
                    int corpID = -1, allianceID = -1;
                    if (cc.CorporationId > 0)
                        corpID = cc.CorporationId;
                    if (cc.MemberOfAlliance > 0)
                        allianceID = cc.MemberOfAlliance;
                    //standing = new StealthBot.Core.CachedStanding(
                    //    Core.StealthBot.Me.StandingTo(-1, corpID, allianceID));
                    //Core.StealthBot.Logging.LogMessage(ObjectName, new StealthBot.Core.LogEventArgs(StealthBot.Core.LogSeverityTypes.Debug,
                    //    "DrawItem: Corp - got standings"));

                    corp = cc.Name;
                    if (cc.MemberOfAlliance > 0)
                    {
                        if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey(cc.MemberOfAlliance))
                        {
                            alliance = Core.StealthBot.AllianceCache.CachedAlliancesById[cc.MemberOfAlliance].Name;
                        }
                        else
                        {
                            Core.StealthBot.AllianceCache.RegenerateAllianceDatabase();
                        }
                    }

                    if (Core.StealthBot.MeCache.CorporationId == id)
                    {
                        myBrush = Brushes.Green;
                    }
                    else if (Core.StealthBot.MeCache.AllianceId == cc.MemberOfAlliance)
                    {
                        myBrush = Brushes.Blue;
                    }
                }
            }
            else if (radioButtonDisplayAllianceCache.Checked)
            {
                //standing = new StealthBot.Core.CachedStanding(
                //    Core.StealthBot.Me.StandingTo(-1, -1, id));
                //Core.StealthBot.Logging.LogMessage(ObjectName, new StealthBot.Core.LogEventArgs(StealthBot.Core.LogSeverityTypes.Debug,
                //    "DrawItem: Alliance - got standing"));
                if (Core.StealthBot.MeCache.AllianceId == id)
                {
                    myBrush = Brushes.Blue;
                }
                if (Core.StealthBot.MeCache.AllianceId > 0)
                {
                    if (Core.StealthBot.AllianceCache.CachedAlliancesById.ContainsKey((int)id))
                    {
                        alliance = Core.StealthBot.AllianceCache.CachedAlliancesById[(int)id].Name;
                    }
                }
            }

            //
            // Determine the color of the brush to draw each item based on 
            // the index of the item to draw.
            //

            if (standing != null)
            {
                if (standing.AllianceToAlliance >= 5 || standing.CorpToAlliance >= 5 || standing.CorpToCorp >= 5 ||
                    standing.CorpToPilot >= 5 || standing.MeToCorp >= 5 || standing.MeToPilot >= 5)
                {
                    myBrush = Brushes.Blue;
                }
                else if (standing.AllianceToAlliance > 0 || standing.CorpToAlliance > 0 || standing.CorpToCorp > 0 ||
                    standing.CorpToPilot > 0 || standing.MeToCorp > 0 || standing.MeToPilot > 0)
                {
                    myBrush = Brushes.LightBlue;
                }
                else if (standing.AllianceToAlliance < 0 || standing.CorpToAlliance < 0 || standing.CorpToCorp < 0 ||
                    standing.CorpToPilot < 0 || standing.MeToCorp < 0 || standing.MeToPilot < 0)
                {
                    myBrush = Brushes.OrangeRed;
                }
                else if (standing.AllianceToAlliance <= -5 || standing.CorpToAlliance <= -5 || standing.CorpToCorp <= -5 ||
                    standing.CorpToPilot <= -5 || standing.MeToCorp <= -5 || standing.MeToPilot <= -5)
                {
                    myBrush = Brushes.Red;
                }
            }

            if (radioButtonDisplayAllianceCache.Checked)
            {
                if (Core.StealthBot.Config.SocialConfig.AllianceBlacklist.Contains(alliance))
                {
                    myBrush = Brushes.DarkRed;
                }
                else if (Core.StealthBot.Config.SocialConfig.AllianceWhitelist.Contains(alliance))
                {
                    myBrush = Brushes.DarkBlue;
                }
            }
            else if (radioButtonDisplayCorporationCache.Checked)
            {
                if (Core.StealthBot.Config.SocialConfig.AllianceBlacklist.Contains(alliance))
                {
                    myBrush = Brushes.DarkRed;
                }
                else if (Core.StealthBot.Config.SocialConfig.AllianceWhitelist.Contains(alliance))
                {
                    myBrush = Brushes.DarkBlue;
                }

                if (Core.StealthBot.Config.SocialConfig.CorpWhitelist.Contains(corp))
                {
                    myBrush = Brushes.DarkBlue;
                }
                else if (Core.StealthBot.Config.SocialConfig.CorpBlacklist.Contains(corp))
                {
                    myBrush = Brushes.DarkRed;
                }
            }
            else if (radioButtonDisplayPilotCache.Checked)
            {
                if (Core.StealthBot.Config.SocialConfig.AllianceBlacklist.Contains(alliance))
                {
                    myBrush = Brushes.DarkRed;
                }
                else if (Core.StealthBot.Config.SocialConfig.AllianceWhitelist.Contains(alliance))
                {
                    myBrush = Brushes.DarkBlue;
                }

                if (Core.StealthBot.Config.SocialConfig.CorpWhitelist.Contains(corp))
                {
                    myBrush = Brushes.DarkBlue;
                }
                else if (Core.StealthBot.Config.SocialConfig.CorpBlacklist.Contains(corp))
                {
                    myBrush = Brushes.DarkRed;
                }

                if (Core.StealthBot.Config.SocialConfig.PilotBlacklist.Contains(corp))
                {
                    myBrush = Brushes.DarkRed;
                }
                else if (Core.StealthBot.Config.SocialConfig.PilotWhitelist.Contains(corp))
                {
                    myBrush = Brushes.DarkBlue;
                }
            }
            //
            // Draw the current item text based on the current 
            // Font and the custom brush settings.
            //
            e.Graphics.DrawString(((ListBox)sender).Items[e.Index].ToString(),
                e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            //
            // If the ListBox has focus, draw a focus rectangle 
            // around the selected item.
            //  
            e.DrawFocusRectangle();
        }

        private void StealthBotForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            lock (this)
            {
                if (!_isReadyToExit)
                {
                    e.Cancel = true;

                    DetachStealthBotToInterfaceEvents();

                    if (_exitThread == null)
                    {
                        _exitThread = new Thread(DoExit);
                        _exitThread.Start();
                    }
                    return;
                }

                if (TextBoxAuthEmailAddress != null &&
                    TextBoxAuthEmailAddress.IsHandleCreated &&
                    TextBoxAuthEmailAddress.Text != string.Empty &&
                    !TextBoxAuthEmailAddress.Text.Contains('\'') &&
                    !TextBoxAuthEmailAddress.Text.Contains('\"') &&
                    TextBoxAuthPassword.Text != string.Empty &&
                    !TextBoxAuthPassword.Text.Contains('\'') &&
                    !TextBoxAuthPassword.Text.Contains('\"'))
                {
                    var directory = String.Format("{0}\\stealthbot\\config",
                        Path.GetDirectoryName(Application.ExecutablePath));
                    var fileName = String.Format("{0}\\SBLogin.txt", directory);
                    using (var sw = new StreamWriter(File.Create(fileName)))
                    {
                        sw.WriteLine(TextBoxAuthEmailAddress.Text);
                        sw.WriteLine(TextBoxAuthPassword.Text);
                    }
                }
            }
            _exitResetEvent.Set();
            _exitResetEvent.Close();
        }
        #endregion

		private void StealthBotForm_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				notifyIcon1.Visible = true;
				Hide();
			}
			else if (WindowState == FormWindowState.Normal)
			{
				notifyIcon1.Visible = false;
			}
		}

		private void NotifyIcon1_Click(object sender, EventArgs e)
		{
			Show();
			WindowState = FormWindowState.Normal;
		}
	}
}
// ReSharper restore LocalizableElement