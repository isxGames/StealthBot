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

namespace StealthBotUI
{
    public class SbTabItemHeader
    {
        /// <summary>
        /// Grid for holding content
        /// </summary>
        public Grid ContentGrid;

        public Label HeaderLabel;

        public ProgressBar ShieldProgressBar, ArmorProgressBar, StructureProgressBar, CapacitorProgressBar;

        public SbTabItemHeader(string headerText)
        {
            _initializeComponents(headerText);
        }

        void _initializeComponents(string headerText)
        {
            ContentGrid = new Grid();
            ContentGrid.Width = 150;
            ContentGrid.Height = 100;
            ContentGrid.Margin = new Thickness(0, 0, 0, 0);
            ContentGrid.VerticalAlignment = VerticalAlignment.Center;
            ContentGrid.HorizontalAlignment = HorizontalAlignment.Center;

            //Header label
            HeaderLabel = new Label();
            HeaderLabel.Content = headerText;
            HeaderLabel.HorizontalAlignment = HorizontalAlignment.Center;
            HeaderLabel.Margin = new Thickness(0, 5, 0, 0);
            HeaderLabel.VerticalAlignment = VerticalAlignment.Top;
            ContentGrid.Children.Add(HeaderLabel);

            #region Progress Bars
            //Shield ProgressBar
            ShieldProgressBar = new ProgressBar();
            ShieldProgressBar.Foreground = Brushes.Green;
            ShieldProgressBar.Height = 10;
            ShieldProgressBar.Width = 130;
            ShieldProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            ShieldProgressBar.VerticalAlignment = VerticalAlignment.Top;
            ShieldProgressBar.Margin = new Thickness(10, 32, 0, 0);
            ContentGrid.Children.Add(ShieldProgressBar);

            //Armor ProgressBar
            ArmorProgressBar = new ProgressBar();
            ArmorProgressBar.Foreground = Brushes.Blue;
            ArmorProgressBar.Height = 10;
            ArmorProgressBar.Width = 130;
            ArmorProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            ArmorProgressBar.VerticalAlignment = VerticalAlignment.Top;
            ArmorProgressBar.Margin = new Thickness(10, 48, 0, 0);
            ContentGrid.Children.Add(ArmorProgressBar);

            //Structure ProgressBar
            StructureProgressBar = new ProgressBar();
            StructureProgressBar.Foreground = Brushes.Red;
            StructureProgressBar.Height = 10;
            StructureProgressBar.Width = 130;
            StructureProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            StructureProgressBar.VerticalAlignment = VerticalAlignment.Top;
            StructureProgressBar.Margin = new Thickness(10, 65, 0, 0);
            ContentGrid.Children.Add(StructureProgressBar);

            //Capacitor ProgressBar
            CapacitorProgressBar = new ProgressBar();
            CapacitorProgressBar.Foreground = Brushes.Yellow;
            CapacitorProgressBar.Height = 10;
            CapacitorProgressBar.Width = 130;
            CapacitorProgressBar.HorizontalAlignment = HorizontalAlignment.Left;
            CapacitorProgressBar.VerticalAlignment = VerticalAlignment.Top;
            CapacitorProgressBar.Margin = new Thickness(10, 81, 0, 0);
            ContentGrid.Children.Add(CapacitorProgressBar);
            #endregion
        }
    }
}
