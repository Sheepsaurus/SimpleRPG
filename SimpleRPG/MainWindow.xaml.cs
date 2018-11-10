using System.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleRPG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Player player;
        public MainWindow()
        {
            InitializeComponent();
            player = new Player
            { 
                CurrentHitPoints = 10,
                MaximumHitPoints = 10,
                Gold = 20,
                ExperiencePoints = 0,
                Level = 1
            };

            hitpointsValueLabel.Content = player.CurrentHitPoints.ToString();
            goldValueLabel.Content = player.Gold.ToString();
            experienceValueLabel.Content = player.ExperiencePoints.ToString();
            levelValueLabel.Content = player.Level.ToString();

            Location location = new Location(1, "Home", "This is your house");
        }
    }
}
