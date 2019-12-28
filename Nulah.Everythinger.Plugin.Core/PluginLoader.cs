using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Nulah.Everythinger.Plugins.Core
{
    class PluginLoader
    {
        private List<Plugin> _pluginCache;

        private Dictionary<Guid, Plugin> _loadedPlugins;

        public Plugin First { get; private set; }

        /// <summary>
        /// Returns a list of all plugins. Subsequent calls to this method will return the plugins found on application start
        /// </summary>
        /// <returns></returns>
        public List<Plugin> GetPlugins()
        {
            if (_pluginCache != null)
            {
                return _pluginCache;
            }

            _pluginCache = new List<Plugin>();

            var dirInfo = new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"));
            var dlls = dirInfo.GetFiles("*.dll", SearchOption.AllDirectories);

            foreach (var dll in dlls)
            {
                /*
                var loadContext = new PluginLoadContext(dll.FullName);
                var assembly = loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(dll.FullName)));
                */
                var assembly = Assembly.LoadFrom(dll.FullName);
                _pluginCache.AddRange(GetPluginsFromAssembly(assembly));
            }

            First = _pluginCache.First();
            _loadedPlugins = _pluginCache.ToDictionary(x => x.Id, x => x);

            return _pluginCache;
        }

        /// <summary>
        /// Returns a plugin by runtime Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Plugin GetPlugin(Guid id)
        {
            return _loadedPlugins[id];
        }

        private IEnumerable<Plugin> GetPluginsFromAssembly(Assembly assembly)
        {
            int count = 0;

            foreach (Type type in assembly.GetTypes())
            {
                if (typeof(Plugin).IsAssignableFrom(type))
                {
                    Plugin result = Activator.CreateInstance(type) as Plugin;
                    if (result != null)
                    {
                        count++;
                        yield return result;
                    }
                }
            }
            /*
            if (count == 0)
            {
                string availableTypes = string.Join(",", assembly.GetTypes().Select(t => t.FullName));
                throw new ApplicationException(
                    $"Can't find any type which implements ICommand in {assembly} from {assembly.Location}.\n" +
                    $"Available types: {availableTypes}");
            }*/
        }
    }
}
