using Nulah.Everythinger.Plugins.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Nulah.Everythinger.Plugins.Core
{
    /// <summary>
    /// Interaction logic for PluginMenuControl.xaml
    /// </summary>
    public partial class PluginMenuControl : UserControl
    {
        public PluginMenuControl()
        {
            InitializeComponent();
            /*
            ViewModel = new PluginViewModel();
            ViewModel.Plugins = new ObservableCollection<Plugin>(GetPlugins());

            this.PluginPanel.Content = ViewModel.Plugins.First().LoadPlugin();
            */

            /*
            ((PluginViewModel)this.DataContext).Plugins = new ObservableCollection<Plugin>(GetPlugins());
            this.PluginPanel.Content = ((PluginViewModel)this.DataContext).Plugins.First().LoadPlugin();
            */
        }
    }
}
