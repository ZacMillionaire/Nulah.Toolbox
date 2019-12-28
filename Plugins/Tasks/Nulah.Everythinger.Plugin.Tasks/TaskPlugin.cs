using Nulah.Everythinger.Plugins.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Nulah.Everythinger.Plugins.Tasks
{
    public class TaskPlugin : Plugin
    {
        public override string PluginName => "Nulah.Plugins.Tasks";
        public override string MenuName => "Tasks";
        private UserControl _control;
        public override UserControl LoadControl()
        {
            if (_control == null)
            {
                _control = new TaskControl();
            }

            return _control;
        }
    }
}
