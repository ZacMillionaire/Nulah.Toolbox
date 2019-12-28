using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Nulah.WPF.Toolbox.Utilities;

namespace Nulah.Everythinger.Plugins.Core.Models
{
    class PluginControlViewModel : ObservableViewObject
    {
        private ObservableCollection<PluginViewModel> _plugins;

        public ObservableCollection<PluginViewModel> Plugins
        {
            get { return _plugins; }
            set
            {
                _plugins = value;
                RaisePropertyChangedEvent("Plugins");
            }
        }

        private UserControl _loadedPanel;
        private PluginViewModel _activePlugin { get; set; }

        public UserControl LoadedPanel
        {
            get { return _loadedPanel; }
            set
            {
                _loadedPanel = value;
                RaisePropertyChangedEvent("LoadedPanel");
            }
        }

        private readonly PluginLoader _pluginLoader;

        public PluginControlViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                __DesignModeCtor();
            }
            else
            {
                _pluginLoader = new PluginLoader();

                Plugins = new ObservableCollection<PluginViewModel>(_pluginLoader.GetPlugins()
                    .Select(x => new PluginViewModel
                    {
                        Name = x.MenuName,
                        PluginName = x.PluginName,
                        Id = x.Id
                    })
                );

                _activePlugin = Plugins.First();
                ChangeControl(_pluginLoader.First.Id);
            }
        }

        public ICommand ChangeControlCommand
        {
            get
            {
                return new DelegateCommand<Guid>(ChangeControl);
            }
        }

        public void ChangeControl(Guid pluginId)
        {
            _activePlugin.IsActive = false;
            _activePlugin = Plugins.First(x => x.Id == pluginId);
            _activePlugin.IsActive = true;

            LoadedPanel = _pluginLoader.GetPlugin(pluginId).LoadControl();
        }

        private void __DesignModeCtor()
        {
            Plugins = new ObservableCollection<PluginViewModel>(new List<PluginViewModel>
            {
                new PluginViewModel {
                    IsActive = true,
                    Name = "short"
                },
                new PluginViewModel{
                    Name = "Way too long a plugin name what the fuck"
                },
                new PluginViewModel(),
                new PluginViewModel(),
                new PluginViewModel()
            });
        }
    }
}
