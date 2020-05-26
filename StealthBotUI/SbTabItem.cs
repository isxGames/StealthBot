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
using System.Windows.Forms.Integration;

namespace StealthBotUI
{
    public class SbTabItem : TabItem
    {
        public SbTabItemHeader HeaderContent;

        public Grid ContentGrid;
        public ListBox LogMessageListBox;
        public TabControl PagesTabControl;

        #region Status page
        public TabItem StatusPageTabItem;
        public Grid StatusPageGrid;

        //ProgressBars for ship readout
        public ProgressBar ShieldProgressBar, ArmorProgressBar, StructureProgressBar, CapacitorProgressBar;
        //Textboxes for ship stuff
        public TextBox ShipNameTextBox, ShipTypeTextBox, RuntimeTextBox;
        #endregion

        #region Core Lists page
        public TabItem CoreListsPageTabItem;
        public Grid CoreListsGrid;

        public ListBox QueuedTargetsListBox, LockedTargetsListBox, DestinationQueueListBox, ThreatsListBox;
        #endregion

        #region Behavior Lists page
        public TabItem BehaviorListsPageTabItem;
        public Grid BehaviorListsGrid;

        public ListBox AsteroidsInRangeListBox, AsteroidsOutOfRangeListBox;
        public WindowsFormsHost ItemsMined_MovedDataGridViewHost, Ammo_CrystalsUsedDataGridViewHost;
        public System.Windows.Forms.DataGridView ItemsMined_MovedDataGridView, Ammo_CrystalsUsedDataGridView;
        #endregion

        public SbTabItem(string header)
            : base()
        {
            //Set the content for the header
            HeaderContent = new SbTabItemHeader(header);
            Header = HeaderContent.ContentGrid;

            //Initialize the tabitem
            _initializeComponents(header);    
        }

        void _initializeComponents(string header)
        {
            //set the positioning stuff
            //Header = header;
            Width = 150;
            Height = 100;

            //Grid for holding everything
            ContentGrid = new Grid();

            //Second TabControl for holding session-specific pages
            PagesTabControl = new TabControl();
            PagesTabControl.Height = 428;
            PagesTabControl.HorizontalAlignment = HorizontalAlignment.Left;
            PagesTabControl.Margin = new Thickness(6, 11, 0, 0);
            PagesTabControl.VerticalAlignment = VerticalAlignment.Top;
            PagesTabControl.Width = 815;
            PagesTabControl.TabStripPlacement = Dock.Left;

            //Initialize the status page
            _initializeStatusPage();
            //Initialize the Core Lists page
            _initializeCoreListsPage();
            //Initialize the Behavior Lists page
            _initializeBehaviorListsPage();

            ContentGrid.Children.Add(PagesTabControl);

            //Create the log listbox
            LogMessageListBox = new ListBox();
            LogMessageListBox.Height = 250;
            LogMessageListBox.Margin = new Thickness(6, 0, 6, 6);
            LogMessageListBox.VerticalAlignment = VerticalAlignment.Bottom;
            LogMessageListBox.FontFamily = new FontFamily("Consolas");
            LogMessageListBox.FontSize = 12;

            //Add the listbox to the grid
            ContentGrid.Children.Add(LogMessageListBox);
            //Set the content reference
            Content = ContentGrid;

        }

        void _initializeStatusPage()
        {
            //Add the Status page
            StatusPageTabItem = new TabItem();
            StatusPageTabItem.Width = 100;
            StatusPageTabItem.Header = "Status";
            PagesTabControl.Items.Add(StatusPageTabItem);

            //Set the status page's grid up
            StatusPageGrid = new Grid();
            #region Readout Labels
            //Add the labels
            Label shieldLabel = new Label();
            shieldLabel.Content = "Shield";
            shieldLabel.HorizontalAlignment = HorizontalAlignment.Left;
            shieldLabel.Margin = new Thickness(6, 6, 0, 384);
            StatusPageGrid.Children.Add(shieldLabel);

            Label armorLabel = new Label();
            armorLabel.Content = "Armor";
            armorLabel.HorizontalAlignment = HorizontalAlignment.Left;
            armorLabel.Margin = new Thickness(6, 40, 0, 350);
            StatusPageGrid.Children.Add(armorLabel);

            Label structLabel = new Label();
            structLabel.Content = "Structure";
            structLabel.HorizontalAlignment = HorizontalAlignment.Left;
            structLabel.Margin = new Thickness(6, 74, 0, 316);
            StatusPageGrid.Children.Add(structLabel);

            Label capLabel = new Label();
            capLabel.Content = "Capacitor";
            capLabel.HorizontalAlignment = HorizontalAlignment.Left;
            capLabel.Margin = new Thickness(6, 108, 0, 282);
            StatusPageGrid.Children.Add(capLabel);
            #endregion

            //Now for the progress bars
            #region Readout Progress Bars
            ShieldProgressBar = new ProgressBar();
            ShieldProgressBar.Height = 28;
            ShieldProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            ShieldProgressBar.Margin = new Thickness(70, 6, 0, 0);
            ShieldProgressBar.VerticalAlignment = VerticalAlignment.Top;
            ShieldProgressBar.Width = 627;
            ShieldProgressBar.Foreground = Brushes.Green;
            StatusPageGrid.Children.Add(ShieldProgressBar);

            ArmorProgressBar = new ProgressBar();
            ArmorProgressBar.Height = 28;
            ArmorProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            ArmorProgressBar.Margin = new Thickness(70, 40, 0, 0);
            ArmorProgressBar.VerticalAlignment = VerticalAlignment.Top;
            ArmorProgressBar.Width = 627;
            ArmorProgressBar.Foreground = Brushes.Blue;
            StatusPageGrid.Children.Add(ArmorProgressBar);

            StructureProgressBar = new ProgressBar();
            StructureProgressBar.Height = 28;
            StructureProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            StructureProgressBar.Margin = new Thickness(70, 74, 0, 0);
            StructureProgressBar.VerticalAlignment = VerticalAlignment.Top;
            StructureProgressBar.Width = 627;
            StructureProgressBar.Foreground = Brushes.Red;
            StatusPageGrid.Children.Add(StructureProgressBar);

            CapacitorProgressBar = new ProgressBar();
            CapacitorProgressBar.Height = 28;
            CapacitorProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            CapacitorProgressBar.Margin = new Thickness(70, 108, 0, 0);
            CapacitorProgressBar.VerticalAlignment = VerticalAlignment.Top;
            CapacitorProgressBar.Width = 627;
            CapacitorProgressBar.Foreground = Brushes.Yellow;
            StatusPageGrid.Children.Add(CapacitorProgressBar);

            Label shipNameLabel = new Label();
            shipNameLabel.Content = "Ship";
            shipNameLabel.Height = 28;
            shipNameLabel.HorizontalAlignment = HorizontalAlignment.Left;
            shipNameLabel.Margin = new Thickness(11, 142, 0, 0);
            shipNameLabel.VerticalAlignment = VerticalAlignment.Top;
            StatusPageGrid.Children.Add(shipNameLabel);

            ShipNameTextBox = new TextBox();
            ShipNameTextBox.Height = 23;
            ShipNameTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            ShipNameTextBox.Margin = new Thickness(70, 142, 0, 0);
            ShipNameTextBox.VerticalAlignment = VerticalAlignment.Top;
            ShipNameTextBox.IsReadOnly = true;
            ShipNameTextBox.Width = 120;
            StatusPageGrid.Children.Add(ShipNameTextBox);

            Label shipTypeLabel = new Label();
            shipTypeLabel.Content = "Type";
            shipTypeLabel.Height = 28;
            shipTypeLabel.HorizontalAlignment = HorizontalAlignment.Left;
            shipTypeLabel.Margin = new Thickness(270, 142, 0, 0);
            shipTypeLabel.VerticalAlignment = VerticalAlignment.Top;
            StatusPageGrid.Children.Add(shipTypeLabel);

            ShipTypeTextBox = new TextBox();
            ShipTypeTextBox.Height = 23;
            ShipTypeTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            ShipTypeTextBox.Margin = new Thickness(311, 142, 0, 0);
            ShipTypeTextBox.VerticalAlignment = VerticalAlignment.Top;
            ShipTypeTextBox.IsReadOnly = true;
            ShipTypeTextBox.Width = 120;
            StatusPageGrid.Children.Add(ShipTypeTextBox);

            Label runtimeLabel = new Label();
            runtimeLabel.Content = "Runtime";
            runtimeLabel.HorizontalAlignment = HorizontalAlignment.Left;
            runtimeLabel.Height = 28;
            runtimeLabel.Margin = new Thickness(517, 142, 0, 0);
            runtimeLabel.VerticalAlignment = VerticalAlignment.Top;
            StatusPageGrid.Children.Add(runtimeLabel);

            RuntimeTextBox = new TextBox();
            RuntimeTextBox.Height = 23;
            RuntimeTextBox.HorizontalAlignment = HorizontalAlignment.Left;
            RuntimeTextBox.Margin = new Thickness(577, 142, 0, 0);
            RuntimeTextBox.VerticalAlignment = VerticalAlignment.Top;
            RuntimeTextBox.IsReadOnly = true;
            RuntimeTextBox.Width = 120;
            StatusPageGrid.Children.Add(RuntimeTextBox);
            #endregion

            //SEt the status pagae content grid
            StatusPageTabItem.Content = StatusPageGrid;
        }

        void _initializeCoreListsPage()
        {
            //Initialize the items
            CoreListsPageTabItem = new TabItem();
            CoreListsPageTabItem.Width = 100;
            CoreListsPageTabItem.Header = "Core Lists";
            PagesTabControl.Items.Add(CoreListsPageTabItem);
            CoreListsGrid = new Grid();

            //Start with Queue Targets
            Label queueTargetsLabel = new Label();
            queueTargetsLabel.Content = "Queued Targets";
            queueTargetsLabel.Height = 28;
            queueTargetsLabel.HorizontalAlignment = HorizontalAlignment.Left;
            queueTargetsLabel.Margin = new Thickness(6, 6, 0, 0);
            queueTargetsLabel.VerticalAlignment = VerticalAlignment.Top;
            CoreListsGrid.Children.Add(queueTargetsLabel);

            QueuedTargetsListBox = new ListBox();
            QueuedTargetsListBox.Height = 100;
            QueuedTargetsListBox.Margin = new Thickness(4, 37, 0, 0);
            QueuedTargetsListBox.VerticalAlignment = VerticalAlignment.Top;
            QueuedTargetsListBox.Width = 345;
            QueuedTargetsListBox.HorizontalAlignment = HorizontalAlignment.Left;
            CoreListsGrid.Children.Add(QueuedTargetsListBox);

            //Next, Locked Targets
            Label lockedTargetsLabel = new Label();
            lockedTargetsLabel.Content = "Locked Targets";
            lockedTargetsLabel.Height = 28;
            lockedTargetsLabel.HorizontalAlignment = HorizontalAlignment.Left;
            lockedTargetsLabel.Margin = new Thickness(354, 6, 0, 0);
            lockedTargetsLabel.VerticalAlignment = VerticalAlignment.Top;
            CoreListsGrid.Children.Add(lockedTargetsLabel);

            LockedTargetsListBox = new ListBox();
            LockedTargetsListBox.Height = 100;
            LockedTargetsListBox.Margin = new Thickness(0, 37, 4, 0);
            LockedTargetsListBox.HorizontalAlignment = HorizontalAlignment.Right;
            LockedTargetsListBox.Width = 345;
            LockedTargetsListBox.VerticalAlignment = VerticalAlignment.Top;
            CoreListsGrid.Children.Add(LockedTargetsListBox);

            //thirdly, DestinationQueue
            Label destinationQueueLabel = new Label();
            destinationQueueLabel.Content = "DestinationQueue";
            destinationQueueLabel.Height = 28;
            destinationQueueLabel.HorizontalAlignment = HorizontalAlignment.Left;
            destinationQueueLabel.Margin = new Thickness(6, 143, 0, 0);
            destinationQueueLabel.VerticalAlignment = VerticalAlignment.Top;
            CoreListsGrid.Children.Add(destinationQueueLabel);

            DestinationQueueListBox = new ListBox();
            DestinationQueueListBox.Height = 100;
            DestinationQueueListBox.Margin = new Thickness(4, 175, 0, 0);
            DestinationQueueListBox.HorizontalAlignment = HorizontalAlignment.Left;
            DestinationQueueListBox.VerticalAlignment = VerticalAlignment.Top;
            DestinationQueueListBox.Width = 345;
            CoreListsGrid.Children.Add(DestinationQueueListBox);

            //fourth, Threats
            Label threatsLabel = new Label();
            threatsLabel.Content = "Threats";
            threatsLabel.Height = 28;
            threatsLabel.HorizontalAlignment = HorizontalAlignment.Left;
            threatsLabel.Margin = new Thickness(354, 143, 0, 0);
            threatsLabel.VerticalAlignment = VerticalAlignment.Top;
            CoreListsGrid.Children.Add(threatsLabel);

            ThreatsListBox = new ListBox();
            ThreatsListBox.Height = 100;
            ThreatsListBox.HorizontalAlignment = HorizontalAlignment.Right;
            ThreatsListBox.Margin = new Thickness(0, 175, 4, 0);
            ThreatsListBox.VerticalAlignment = VerticalAlignment.Top;
            ThreatsListBox.Width = 345;
            CoreListsGrid.Children.Add(ThreatsListBox);

            //Add the UIElement to the content grid and set content reference
            CoreListsPageTabItem.Content = CoreListsGrid;
        }

        void _initializeBehaviorListsPage()
        {
            //Initialize the page items
            BehaviorListsPageTabItem = new TabItem();
            BehaviorListsPageTabItem.Header = "Behavior Lists";
            BehaviorListsPageTabItem.Width = 100;
            PagesTabControl.Items.Add(BehaviorListsPageTabItem);
            BehaviorListsGrid = new Grid();

            //Asteroids in range
            Label asteroidsInRangeLabel = new Label();
            asteroidsInRangeLabel.Content = "Asteroids In Range";
            asteroidsInRangeLabel.Height = 28;
            asteroidsInRangeLabel.HorizontalAlignment = HorizontalAlignment.Left;
            asteroidsInRangeLabel.Margin = new Thickness(6, 6, 0, 0);
            asteroidsInRangeLabel.VerticalAlignment = VerticalAlignment.Top;
            BehaviorListsGrid.Children.Add(asteroidsInRangeLabel);

            AsteroidsInRangeListBox = new ListBox();
            AsteroidsInRangeListBox.Height = 100;
            AsteroidsInRangeListBox.HorizontalAlignment = HorizontalAlignment.Left;
            AsteroidsInRangeListBox.VerticalAlignment = VerticalAlignment.Top;
            AsteroidsInRangeListBox.Margin = new Thickness(4, 37, 0, 0);
            AsteroidsInRangeListBox.Width = 345;
            BehaviorListsGrid.Children.Add(AsteroidsInRangeListBox);

            //Asteroids out of range
            Label asteroidsOutOfRangeLabel = new Label();
            asteroidsOutOfRangeLabel.Content = "Asteroids Out of Range";
            asteroidsOutOfRangeLabel.Height = 28;
            asteroidsOutOfRangeLabel.HorizontalAlignment = HorizontalAlignment.Left;
            asteroidsOutOfRangeLabel.Margin = new Thickness(354, 6, 0, 0);
            asteroidsOutOfRangeLabel.VerticalAlignment = VerticalAlignment.Top;
            BehaviorListsGrid.Children.Add(asteroidsOutOfRangeLabel);

            AsteroidsOutOfRangeListBox = new ListBox();
            AsteroidsOutOfRangeListBox.Height = 100;
            AsteroidsOutOfRangeListBox.HorizontalAlignment = HorizontalAlignment.Right;
            AsteroidsOutOfRangeListBox.VerticalAlignment = VerticalAlignment.Top;
            AsteroidsOutOfRangeListBox.Margin = new Thickness(0, 37, 4, 0);
            AsteroidsOutOfRangeListBox.Width = 345;
            BehaviorListsGrid.Children.Add(AsteroidsOutOfRangeListBox);

            //Labels for the DataGridViewHosts
            Label itemsMined_MovedLabel = new Label();
            itemsMined_MovedLabel.Content = "Items Mined / Moved";
            itemsMined_MovedLabel.HorizontalAlignment = HorizontalAlignment.Left;
            itemsMined_MovedLabel.VerticalAlignment = VerticalAlignment.Top;
            itemsMined_MovedLabel.Margin = new Thickness(6, 143, 0, 0);
            BehaviorListsGrid.Children.Add(itemsMined_MovedLabel);

            Label ammo_crystalsUsed = new Label();
            ammo_crystalsUsed.Content = "Ammo / Crystals Used";
            ammo_crystalsUsed.HorizontalAlignment = HorizontalAlignment.Left;
            ammo_crystalsUsed.VerticalAlignment = VerticalAlignment.Top;
            ammo_crystalsUsed.Margin = new Thickness(356, 143, 0, 0);
            BehaviorListsGrid.Children.Add(ammo_crystalsUsed);

            //Initialize the WindowsFormHosts for the DataGridViews, set their children to their DataGridViews,
            //and add them to the grid.
            System.Windows.Forms.DataGridViewCell templateCell = new System.Windows.Forms.DataGridViewTextBoxCell();
            ItemsMined_MovedDataGridView = new System.Windows.Forms.DataGridView();
            ItemsMined_MovedDataGridView.AllowUserToAddRows = false;
            ItemsMined_MovedDataGridView.AllowUserToDeleteRows = false;
            ItemsMined_MovedDataGridView.Columns.Add(new System.Windows.Forms.DataGridViewColumn() {
                Name = "Items Mined / Moved", CellTemplate = templateCell, Width = 175 });
            ItemsMined_MovedDataGridView.Columns.Add(new System.Windows.Forms.DataGridViewColumn() { 
                Name = "Quantity", CellTemplate = templateCell });
            ItemsMined_MovedDataGridViewHost = new WindowsFormsHost();
            ItemsMined_MovedDataGridViewHost.VerticalAlignment = VerticalAlignment.Top;
            ItemsMined_MovedDataGridViewHost.HorizontalAlignment = HorizontalAlignment.Left;
            ItemsMined_MovedDataGridViewHost.Margin = new Thickness(4, 177, 0, 0);
            ItemsMined_MovedDataGridViewHost.Height = 100;
            ItemsMined_MovedDataGridViewHost.Width = 345;
            ItemsMined_MovedDataGridViewHost.Child = ItemsMined_MovedDataGridView;
            BehaviorListsGrid.Children.Add(ItemsMined_MovedDataGridViewHost);

            Ammo_CrystalsUsedDataGridView = new System.Windows.Forms.DataGridView();
            Ammo_CrystalsUsedDataGridView.AllowUserToAddRows = false;
            Ammo_CrystalsUsedDataGridView.AllowUserToDeleteRows = false;
            Ammo_CrystalsUsedDataGridView.Columns.Add(new System.Windows.Forms.DataGridViewColumn() { 
                Name = "Ammo / Crystal Type", CellTemplate = templateCell, Width = 175 });
            Ammo_CrystalsUsedDataGridView.Columns.Add(new System.Windows.Forms.DataGridViewColumn() { 
                Name = "Quantity", CellTemplate = templateCell });
            Ammo_CrystalsUsedDataGridViewHost = new WindowsFormsHost();
            Ammo_CrystalsUsedDataGridViewHost.VerticalAlignment = VerticalAlignment.Top;
            Ammo_CrystalsUsedDataGridViewHost.HorizontalAlignment = HorizontalAlignment.Left;
            Ammo_CrystalsUsedDataGridViewHost.Margin = new Thickness(354, 177, 0, 0);
            Ammo_CrystalsUsedDataGridViewHost.Height = 100;
            Ammo_CrystalsUsedDataGridViewHost.Width = 345;
            Ammo_CrystalsUsedDataGridViewHost.Child = Ammo_CrystalsUsedDataGridView;
            BehaviorListsGrid.Children.Add(Ammo_CrystalsUsedDataGridViewHost);

            BehaviorListsPageTabItem.Content = BehaviorListsGrid;
        }
    }
}
