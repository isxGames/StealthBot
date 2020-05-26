using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using LavishScriptAPI;
using InnerSpaceAPI;
using StealthBotIpc;

namespace StealthBotUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        //Maintain a SbServer instance
        SbIpcServer server = new SbIpcServer();
        readonly int MAX_LOGMESSAGES = 100;

        EventHandler<GenericExceptionEventArgs> Handle_ExceptionThrown;
        EventHandler<SbStateObjectEventArgs> Handle_SbStateObjectReceived;

        bool _isDisposed = false;

        public MainWindow()
        {
            //If I shouldn't be running, just exit.
            if (!_checkStartupRequirements())
            {
                Close();
                return;
            }

            InitializeComponent();

            //get an eventhandler for stateobject received
            Handle_SbStateObjectReceived = new EventHandler<SbStateObjectEventArgs>(server_SbStateObjectReceived);
            server.SbStateObjectReceived += Handle_SbStateObjectReceived;
            //Eventhandler for exceptions too
            Handle_ExceptionThrown = new EventHandler<GenericExceptionEventArgs>(server_ExceptionThrown);
            server.ExceptionThrown += Handle_ExceptionThrown;
        }

        ~MainWindow()
        {
            _dispose(true);
        }

        #region IDisposable implementors
        void _dispose(bool isFinalizing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                if (isFinalizing)
                {

                }

                //Dispose the server
                server.Dispose();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _dispose(false);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handle exceptions from SbIpcServer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void server_ExceptionThrown(object sender, GenericExceptionEventArgs e)
        {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                //add all exception messages to the main log
                foreach (string s in e.ExceptionDetails)
                {
                    _addMainLogMessage(s);
                }
            }
            else
            {
                Dispatcher.Invoke(Handle_ExceptionThrown, sender, e);
            }
        }

        /// <summary>
        /// Handle new state objects received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void server_SbStateObjectReceived(object sender, SbStateObjectEventArgs e)
        {
            if (Thread.CurrentThread == Dispatcher.Thread)
            {
                //Process the stateobject
                _processSbStateObject(e.SbStateObject);
            }
            else
            {
                Dispatcher.Invoke(Handle_SbStateObjectReceived, sender, e);
            }
        }
        #endregion

        #region Window Event Handlers
        private void Window_Closed(object sender, EventArgs e)
        {
            Dispose();
        }
        #endregion

        /// <summary>
        /// Process a received SbStateObject.
        /// </summary>
        /// <param name="sbStateObject"></param>
        void _processSbStateObject(SbStateObject sbStateObject)
        {
            //Get the matching tabitem, if any
            SbTabItem matchingTabItem = (SbTabItem)_getTabItem(sbStateObject);

            //First, see if I'm initializing. If so I need to destroy any existing tab then
            //recreate a tab.
            if (sbStateObject.Initializing)
            {
                //If we have a matching tab it needs to die
                if (matchingTabItem != null)
                {
                    tabControlBotTabs.Items.Remove(matchingTabItem);
                    matchingTabItem = null;
                }
            }

            //If the matching tab is null (not found) we need to create a new one
            if (matchingTabItem == null)
            {
                //Create a new tab item using the sessionname as the header
                if (sbStateObject.CharacterName == string.Empty)
                {
                    matchingTabItem = new SbTabItem(sbStateObject.SessionName);
                }
                else
                {
                    matchingTabItem = new SbTabItem(sbStateObject.CharacterName);
                }

                //Add the new tab item.
                tabControlBotTabs.Items.Add((TabItem)matchingTabItem);
            }
            else
            {
                //See if we have to update the name
                string header = (string)matchingTabItem.HeaderContent.HeaderLabel.Content;
                if (header== sbStateObject.SessionName &&
                    sbStateObject.CharacterName != string.Empty)
                {
                    matchingTabItem.HeaderContent.HeaderLabel.Content = sbStateObject.CharacterName;
                    matchingTabItem.HeaderContent.HeaderLabel.Content = sbStateObject.CharacterName;
                }
            }

            //Update the Status page
            _updateStatusPage(matchingTabItem, sbStateObject);
            //update the Core lists
            _updateCoreListsPage(matchingTabItem, sbStateObject);
            //Update the Behavior lists
            _updateBehaviorListsPage(matchingTabItem, sbStateObject);

            //Add any log messages
            _addLogMessages(matchingTabItem, sbStateObject);

            //Re-draw the tab control
            tabControlBotTabs.InvalidateVisual();
        }

        void _updateStatusPage(SbTabItem matchingTabItem, SbStateObject sbStateObject)
        {
            //Health stats...
            matchingTabItem.ShieldProgressBar.Value = sbStateObject.ShieldPct;
            matchingTabItem.HeaderContent.ShieldProgressBar.Value = sbStateObject.ShieldPct;
            matchingTabItem.ShieldProgressBar.ToolTip = String.Format("{0} / 100", sbStateObject.ShieldPct);

            matchingTabItem.ArmorProgressBar.Value = sbStateObject.ArmorPct;
            matchingTabItem.HeaderContent.ArmorProgressBar.Value = sbStateObject.ArmorPct;
            matchingTabItem.ArmorProgressBar.ToolTip = String.Format("{0} / 100", sbStateObject.ArmorPct);

            matchingTabItem.StructureProgressBar.Value = sbStateObject.StructurePct;
            matchingTabItem.HeaderContent.StructureProgressBar.Value = sbStateObject.StructurePct;
            matchingTabItem.StructureProgressBar.ToolTip = String.Format("{0} / 100", sbStateObject.StructurePct);

            matchingTabItem.CapacitorProgressBar.Value = sbStateObject.CapacitorPct;
            matchingTabItem.HeaderContent.CapacitorProgressBar.Value = sbStateObject.CapacitorPct;
            matchingTabItem.CapacitorProgressBar.ToolTip = String.Format("{0} / 100", sbStateObject.CapacitorPct);

            //Ship name/type and runtime...
            if (sbStateObject.ShipName != string.Empty)
            {
                matchingTabItem.ShipNameTextBox.Text = sbStateObject.ShipName;
            }
            if (sbStateObject.ShipType != string.Empty)
            {
                matchingTabItem.ShipTypeTextBox.Text = sbStateObject.ShipType;
            }
            matchingTabItem.RuntimeTextBox.Text = sbStateObject.ElapsedRuntime;
        }

        void _updateCoreListsPage(SbTabItem matchingTabItem, SbStateObject sbStateObject)
        {
            //Clear the lists and update them
            matchingTabItem.QueuedTargetsListBox.Items.Clear();
            if (sbStateObject.QueuedTargets != null)
            {
                foreach (String s in sbStateObject.QueuedTargets)
                {
                    matchingTabItem.QueuedTargetsListBox.Items.Add(s);
                }
            }

            matchingTabItem.LockedTargetsListBox.Items.Clear();
            if (sbStateObject.LockedTargets != null)
            {
                foreach (String s in sbStateObject.LockedTargets)
                {
                    matchingTabItem.LockedTargetsListBox.Items.Add(s);
                }
            }

            matchingTabItem.DestinationQueueListBox.Items.Clear();
            if (sbStateObject.DestinationQueue != null)
            {
                foreach (String s in sbStateObject.DestinationQueue)
                {
                    matchingTabItem.DestinationQueueListBox.Items.Add(s);
                }
            }

            matchingTabItem.ThreatsListBox.Items.Clear();
            if (sbStateObject.Threats != null)
            {
                foreach (String s in sbStateObject.Threats)
                {
                    matchingTabItem.ThreatsListBox.Items.Add(s);
                }
            }
        }

        void _updateBehaviorListsPage(SbTabItem matchingTabItem, SbStateObject sbStateObject)
        {
            //Asteroids in/out of range
            matchingTabItem.AsteroidsInRangeListBox.Items.Clear();
            if (sbStateObject.AsteroidsInRange != null)
            {
                foreach (string s in sbStateObject.AsteroidsInRange)
                {
                    matchingTabItem.AsteroidsInRangeListBox.Items.Add(s);
                }
            }

            matchingTabItem.AsteroidsOutOfRangeListBox.Items.Clear();
            if (sbStateObject.AsteroidsOutOfRange != null)
            {
                foreach (string s in sbStateObject.AsteroidsOutOfRange)
                {
                    matchingTabItem.AsteroidsOutOfRangeListBox.Items.Add(s);
                }
            }

            //update the data in the datagridviews
            if (sbStateObject.ItemsMined_Moved != null)
            {
                _updateDataGridView(sbStateObject.ItemsMined_Moved, matchingTabItem.ItemsMined_MovedDataGridView);
            }
            if (sbStateObject.Ammo_CrystalsUsed != null)
            {
                _updateDataGridView(sbStateObject.Ammo_CrystalsUsed, matchingTabItem.Ammo_CrystalsUsedDataGridView);
            }
        }

        /// <summary>
        /// Update a destination DataGridView's columns, rows, and cells using a source DataGridView for data/references
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        void _updateDataGridView(Dictionary<string, int> source, System.Windows.Forms.DataGridView destination)
        {
            //Iterate all keys in the source dictionary.
            foreach (string key in source.Keys.ToList())
            {
                //Iterate all rows in the destination datagridview to find a match
                bool matchFound = false;
                foreach (System.Windows.Forms.DataGridViewRow row in destination.Rows)
                {
                    //If we already have this key added
                    if (row.Cells.Count > 1 &&
                        ((string)row.Cells[0].Value) == key)
                    {
                        //update its value
                        matchFound = true;
                        row.Cells[1].Value = source[key];
                        break;
                    }
                }

                //If we didn't find a match
                if (!matchFound)
                {
                    //Add the pair as a new row
                    System.Windows.Forms.DataGridViewRow row = new System.Windows.Forms.DataGridViewRow();
                    row.CreateCells(destination);
                    row.Cells[0].Value = key;
                    row.Cells[1].Value = source[key];
                    destination.Rows.Add(row);
                }
            }
        }

        /// <summary>
        /// Try to get a TabItem from the instance tab controls.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        SbTabItem _getTabItem(SbStateObject sbStateObject)
        {
            //Iterate and search for the matching tabitem, checking for Character Name if valid otherwise session name
            //foreach (TabItem tabItem in tabControlBotTabs.Items)
            for (int idx = 0; idx < tabControlBotTabs.Items.Count; idx++)
            {
                SbTabItem sbTabItem = tabControlBotTabs.Items[idx] as SbTabItem;
                if (sbTabItem == null)
                {
                    continue;
                }

                string header = (string)sbTabItem.HeaderContent.HeaderLabel.Content;
                if (sbStateObject.CharacterName != string.Empty && header == sbStateObject.CharacterName)
                {
                    //Quickly iterate the list again, finding and removing any items with a header matching the session
                    for (int innerIdx = 0; innerIdx < tabControlBotTabs.Items.Count; innerIdx++)
                    {
                        SbTabItem innerTabItem = tabControlBotTabs.Items[innerIdx] as SbTabItem;
                        if (innerTabItem == null)
                        {
                            continue;
                        }

                        string innerHeader = (string)innerTabItem.HeaderContent.HeaderLabel.Content;
                        if (sbStateObject.SessionName == innerHeader)
                        {
                            tabControlBotTabs.Items.RemoveAt(innerIdx);
                            break;
                        }
                    }
                    return sbTabItem;
                }
                else if (header == sbStateObject.SessionName)
                {
                    return sbTabItem;
                }
            }

            //Return null if not found
            return null;
        }

        /// <summary>
        /// Add a log message to the main log box, clearing old messages
        /// </summary>
        /// <param name="message"></param>
        void _addMainLogMessage(string message)
        {
            //Add the message
            listBoxMainLogMessages.Items.Add(message);

            //If we have > max messages, remove the first ones
            while (listBoxMainLogMessages.Items.Count > MAX_LOGMESSAGES)
            {
                listBoxMainLogMessages.Items.RemoveAt(0);
            }

            //Set the selectedindex to the last possible item to auto-scroll
            if (listBoxMainLogMessages.Items.Count > 0)
            {
                listBoxMainLogMessages.ScrollIntoView(listBoxMainLogMessages.Items[listBoxMainLogMessages.Items.Count - 1]);
            }

            //Invalidate to show changes
            listBoxMainLogMessages.InvalidateVisual();
        }

        /// <summary>
        /// Add all log messages for the specified stateobject to a specified session tab.
        /// </summary>
        /// <param name="sbStateObject"></param>
        void _addLogMessages(SbTabItem sessionTabItem, SbStateObject sbStateObject)
        {
            //Make sure we don't have a null list.
            if (sbStateObject.LogMessages == null)
            {
                return;
            }

            //Add the messages
            ListBox logMessageListBox = sessionTabItem.LogMessageListBox;
            foreach (string s in sbStateObject.LogMessages)
            {
                logMessageListBox.Items.Add(s);
            }
            //Prune any extra
            while (logMessageListBox.Items.Count > 100)
            {
                logMessageListBox.Items.RemoveAt(0);
            }

            //Set the selected item for scrollage
            if (logMessageListBox.Items.Count > 1)
            {
                logMessageListBox.ScrollIntoView(logMessageListBox.Items[logMessageListBox.Items.Count - 1]);
            }
        }

        /// <summary>
        /// Check the conditions that must be present for this app to run.
        /// </summary>
        /// <returns>true if we can continue, otherwise false</returns>
        bool _checkStartupRequirements()
        {
            //Don't bother enforcing this if we're in a test environment.
#if !DEBUG
            //Make sure we're in Innerspace. If we are it'll have a valid build number.
            if (InnerSpace.BuildNumber == 0)
            {
                //Notify the user of the problem
                MessageBox.Show("StealthBotUI must be started in InnerSpace,\npreferrably in the uplink.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
#endif
            return true;
        }
    }
}
