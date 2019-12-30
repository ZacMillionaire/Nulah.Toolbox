using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design;
namespace Nulah.Everythinger.Plugins.Core
{
    public static class ViewManager
    {
        private static ServiceContainer _registeredViews = new ServiceContainer();

        public static T GetView<T>()
        {
            var a = (T)_registeredViews.GetService(typeof(T));

            if (a != null)
            {
                return a;
            }

            throw new Exception($"No registered view for {typeof(T).FullName}");
        }

        public static void RegisterView<T>(T viewObject)
        {
            _registeredViews.AddService(typeof(T), viewObject);
        }
    }
}
