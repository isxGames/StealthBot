using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EVE.ISXEVE;

namespace StealthBot.Core
{
    /// <summary>
    /// This class is designed to handle all pulsing of modules in one easy, self-contained class.
    /// </summary>
    public sealed class ModuleManager : ModuleBase, IDisposable
    {
        public bool IsConfigLoaded = false;

        //Cached result of Is3DDisplayOn and IsUIDisplayOn
        bool _is3DDisplayOn = true, _isUIDisplayOn = true;

        #region IDisposable members
        bool _isDisposed = false;
        #endregion

        //Module lists for iteration during Pulse
        public static List<ModuleBase> Modules = new List<ModuleBase>();
        public static List<ModuleBase> CachesToPulse = new List<ModuleBase>();
        public static List<ModuleBase> EntityCachesToPulse = new List<ModuleBase>();
        public static List<ModuleBase> ModulesToPulse = new List<ModuleBase>();

        public static List<IDisposable> ModulesToDispose = new List<IDisposable>();

        internal ModuleManager()
        {
            //Set the object name
            ObjectName = "ModuleManager";
            //Set the pulse frequency, default to fastest until otherwise set
            PulseFrequency = 3;
            //make sure it's enabled
            IsEnabled = true;
            //Don't need to add this to a list of modules to pulse; it's pulsed from StealthBot.
        }

        #region IDisposable implementors
        ~ModuleManager()
        {
            _dispose(false);
        }

        public void Dispose()
        {
            _dispose(true);
        }

        void _dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                //If not finalizing...
                if (disposing)
                {

                }

                //re-enable 3D display if necessary
                if (!_is3DDisplayOn)
                {
                    Core.StealthBot.EVE.Toggle3DDisplay();
                }

                //Do the same for UI display
                if (!_isUIDisplayOn)
                {
                    Core.StealthBot.EVE.ToggleUIDisplay();
                }

                //Dispose any IDisposable classes
                foreach (IDisposable idisposable in ModulesToDispose)
                {
                    idisposable.Dispose();
                }
            }
        }
        #endregion

        /// <summary>
        /// Initialize all ModuleBase modules.
        /// </summary>
        internal override void Initialize()
        {
            string methodName = "Initialize";

            foreach (ModuleBase module in Modules)
            {
                try
                {
                    //Don't initialize StealthBot since it calls all initializes, or
                    //ModuleManager since it's what's called by stealthbot and does all initializing
                    if (module is StealthBot ||
                        module is ModuleManager)
                    {
                        continue;
                    }
                    module.Initialize();
                }
                catch (NotImplementedException) { }
                catch (Exception e)
                {
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Caught exception while initializing {0}:",
                        module.ObjectName)));
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Message: {0}",
                        e.Message)));
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Stack Trace: {0}",
                        e.StackTrace)));
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Inner Exception: {0}",
                        e.InnerException)));
                }
            }
        }

        /// <summary>
        /// Clean up all ModuleBase modules.
        /// </summary>
        internal override void CleanUp()
        {
            string methodName = "CleanUp";

            foreach (ModuleBase module in Modules)
            {
                try
                {
                    if (module is StealthBot ||
                        module is ModuleManager)
                    {
                        continue;
                    }
                    module.CleanUp();
                }
                catch (NotImplementedException) { }
                catch (Exception e)
                {
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Caught exception while cleaning up {0}:",
                        module.ObjectName)));
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Message: {0}",
                        e.Message)));
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Stack Trace: {0}",
                        e.StackTrace)));
                    StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, String.Format("Inner Exception: {0}",
                        e.InnerException)));
                }
            }
        }

        internal override void Pulse()
        {
            string methodName = "Pulse";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, string.Empty));

            //Make sure we should be pulsing...
            if (ShouldPulse())
            {
                //First, pulse the Cache modules
                foreach (ModuleBase module in CachesToPulse)
                {
                    //StartMethodProfiling(string.Format("Pulse {0}", module.ObjectName));
                    _tryPulseModule(module);
                    //EndMethodProfiling();
                }

                //If we're critically moving we can only pulse core modules.
                if (Core.StealthBot.Movement.IsCriticalMoving)
                {
                    StartMethodProfiling(string.Format("Pulse {0}", "Movement"));
                    Core.StealthBot.Movement.Pulse();
                    EndMethodProfiling();
                    return;
                }

                //Return if EVE, MyShip are invalid and not InSpace but not InStation
                if (!Core.StealthBot._Me.InSpace && !Core.StealthBot._Me.InStation)
                {
                    Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                        methodName, "Ending pulse due to some form of loading or break (not InSpace and not InStation)."));
                    return;
                }

                //Do any important actions, i.e. toggling UI/3d display or closing chats
                //StartMethodProfiling("Important Pulse Actions");
                _doImportantPulseActions();
                //EndMethodProfiling();

                //If we're just supposed to load a config and not actually pulse things, return.
                if (Core.StealthBot.JustLoadConfig)
                {
                    return;
                }

                //Now pulse Entity caches
                foreach (ModuleBase module in EntityCachesToPulse)
                {
                    //StartMethodProfiling(string.Format("Pulse {0}", module.ObjectName));
                    _tryPulseModule(module);
                    //EndMethodProfiling();
                }

                //up next, actual core modules
                foreach (ModuleBase module in ModulesToPulse)
                {
                    //StartMethodProfiling(string.Format("Pulse {0}", module.ObjectName));
                    _tryPulseModule(module);
                    //EndMethodProfiling();
                }

                //Separately pulse the SbUiCommunication module
                //StartMethodProfiling("SbUiCommunication");
                _tryPulseModule(Core.StealthBot.SbUiCommunication);
                //EndMethodProfiling();
            }
        }

        #region Important Pulse Action methods
        /// <summary>
        /// Do any non-critical actions we need to do each pulse.
        /// </summary>
        void _doImportantPulseActions()
        {
            //Load pilot cache, configuration files, and set logfile prefix
            _loadConfigCacheEtc();

            //Close windows
            _closeEveWindows();

            //Toggle render states as necessary
            _toggleRender();

            //Update the pulse frequency to match what we're set to use in Config
            _updatePulseFrequency();
        }

        /// <summary>
        /// Load the pilot cache, set the logfile prefix, and load configuration files.
        /// </summary>
        void _loadConfigCacheEtc()
        {
            string methodName = "_loadCacheConfigEtc";

            //If we've got a valid Me and our name is readable...
            if (Core.StealthBot._Me.IsValid && Core.StealthBot._Me.Name.Length > 0)
            {
                //Set the LogFilePrefix if it's thus far unset
                if (Logging.LogFilePrefix.Equals(string.Empty))
                {
                    Logging.LogFilePrefix = Core.StealthBot._Me.Name;
                }

                //Load a config if we haven't already loaded one
                if (!IsConfigLoaded)
                {
                    //Go ahead and load config
                    //Config = Configuration.LoadConfiguration(_Me.Name);
                    if (Core.StealthBot.PilotCache == null)
                    {
                        Core.StealthBot.PilotCache = PilotCache.LoadPilotCache(Core.StealthBot._Me.Name);
                    }
                    Configuration.LoadConfigurationFiles();
                    //Configuration.OnConfigLoaded();
                    string versionString = "Stable";
#if DEBUG
                    versionString = "Testing";
#endif
                    Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Standard,
                        methodName, String.Format("Version: {0} {1}", System.Windows.Forms.Application.ProductVersion, versionString)));

                    //Also load missions
                    //EVEBotMissions.Cache.LoadCache(_Me.Name);

                    if (!Core.StealthBot.Config.DefenseConfig.DisableStandingsChecks)
                    {
                        Core.StealthBot.EVE.RefreshStandings();
                    }
                    IsConfigLoaded = true;
                }
            }
        }

        /// <summary>
        /// Close unnecessary EVE modal windows.
        /// </summary>
        void _closeEveWindows()
        {
            //First check for any "MessageBox" template windows
            EVEWindow infoWindow = new EVEWindow("MessageBox");
            if (infoWindow.IsValid)
            {
                //Fleet invitations are message boxes. Close it if we're not currently invited to a fleet
                if (!MeCache.Me.Fleet.Invited || MeCache.Me.Fleet.InvitationText == "NULL")
                {
                    infoWindow.Close();
                }
            }

            //Next check for any "Information" template windows
            infoWindow = new EVEWindow("ByCaption,Information");
            if (infoWindow.IsValid)
            {
                infoWindow.Close();
            }
        }

        /// <summary>
        /// Turn on or off 3D or UI rendering as necessary.
        /// </summary>
        void _toggleRender()
        {
            //first, toggle 3D rendering as necessary
            _is3DDisplayOn = Core.StealthBot.EVE.Is3DDisplayOn;
            if (Core.StealthBot.Config.MainConfig.Disable3DRender)
            {
                if (_is3DDisplayOn)
                    Core.StealthBot.EVE.Toggle3DDisplay();
            }
            else
            {
                if (!_is3DDisplayOn)
                    Core.StealthBot.EVE.Toggle3DDisplay();
            }

            //next, toggle UI rendering as necessary
            _isUIDisplayOn = Core.StealthBot.EVE.IsUIDisplayOn;
            if (Core.StealthBot.Config.MainConfig.DisableUIRender)
            {
                if (_isUIDisplayOn)
                    Core.StealthBot.EVE.ToggleUIDisplay();
            }
            else
            {
                if (!_isUIDisplayOn)
                    Core.StealthBot.EVE.ToggleUIDisplay();
            }
        }

        /// <summary>
        /// Update the pulse frequency to match what is set in the config
        /// </summary>
        void _updatePulseFrequency()
        {
            //Swithc the enum
            switch (Core.StealthBot.Config.MainConfig.PulseSpeed)
            {
                case PulseSpeeds.Average:
                    PulseFrequency = 4;
                    break;
                case PulseSpeeds.Fast:
                    PulseFrequency = 3;
                    break;
                case PulseSpeeds.Slow:
                    PulseFrequency = 5;
                    break;
                case PulseSpeeds.VerySlow:
                    PulseFrequency = 6;
                    break;
                case PulseSpeeds.VeryFast:
                    PulseFrequency = 2;
                    break;
                case PulseSpeeds.Hyper:
                    PulseFrequency = 2;
                    break;
            }
        }
        #endregion

        /// <summary>
        /// Try to execute the Pulse method of a given module, catching and noting any exceptions thrown.
        /// </summary>
        /// <param name="module"></param>
        void _tryPulseModule(ModuleBase module)
        {
            string methodName = "_tryPulseModule";
            Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Trace,
                methodName, module.ObjectName));

            //Try/Catch block for any unhandled exceptions in the odule
            try
            {
                module.Pulse();
            }
            catch (Exception e)
            {
                Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                    methodName, String.Format("Caught exception while pulsing {0}:",
                    module.ObjectName)));
                Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                    methodName, String.Format("Message: {0}",
                    e.Message)));
                Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                    methodName, String.Format("Stack Trace: {0}",
                    e.StackTrace)));
                Core.StealthBot.Logging.OnLogMessage(ObjectName, new LogEventArgs(LogSeverityTypes.Debug,
                    methodName, String.Format("Inner Exception: {0}",
                    e.InnerException)));
            }
        }
    }
}
