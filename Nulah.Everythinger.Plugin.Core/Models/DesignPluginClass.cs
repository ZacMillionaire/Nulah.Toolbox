using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Nulah.Everythinger.Plugins.Core.Models
{
    public class DesignPluginClass : Plugin
    {
        public override string PluginName => "Test.Plugin";

        public override string MenuName => "Design Mode Plugin";
        public override UserControl LoadControl()
        {
            throw new NotImplementedException();
        }
    }
}
