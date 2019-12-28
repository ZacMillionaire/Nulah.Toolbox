using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Nulah.WPF.Toolbox.Utilities;

namespace Nulah.Everythinger.Plugins.Core.Models
{
    internal class PluginViewModel : ObservableViewObject
    {
        public string Name { get; set; }
        public string PluginName { get; set; }
        public Guid Id { get; set; }
        private bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; RaisePropertyChangedEvent("IsActive"); }
        }

        public PluginViewModel()
        {
            Name = "Design View Name";
            PluginName = "DESIGN MODE VIEW MODEL";
            Id = Guid.Empty;
        }
    }
}
