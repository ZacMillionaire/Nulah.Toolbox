using System;
using System.Windows.Controls;

namespace Nulah.Everythinger.Plugins.Core
{
    public abstract class Plugin : IPlugin
    {
        public abstract string PluginName { get; }
        public abstract string MenuName { get; }

        public abstract UserControl LoadControl();

        /// <summary>
        /// Runtime generated guid
        /// </summary>
        public Guid Id { get; private set; }
        public void ClosePlugin() { }

        public Plugin()
        {
            Id = Guid.NewGuid();
        }
    }

    public interface IPlugin
    {
        string PluginName { get; }
        string MenuName { get; }
        Guid Id { get; }
        UserControl LoadControl();
        void ClosePlugin();
    }
}
