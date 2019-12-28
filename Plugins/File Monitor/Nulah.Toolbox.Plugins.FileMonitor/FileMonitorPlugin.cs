using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using Nulah.Everythinger.Plugins.Core;

namespace Nulah.Toolbox.Plugins.FileMonitor
{
    public class FileMonitorPlugin : Plugin
    {
        public override string PluginName => "Nulah.Plugins.FileMonitor";
        public override string MenuName => "File Monitor";
        private UserControl _control;
        public override UserControl LoadControl()
        {
            if (_control == null)
            {
                _control = new FileMonitorControl();
            }

            return _control;
        }
    }
}
