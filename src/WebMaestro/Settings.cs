using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMaestro
{
    class Settings
    {
        const string COLLECTION_KEY = "Collection_";

        public static Settings Current;

        public static void Load()
        {
            var settings = new Settings();

            RegistryKey key = RegistryHelper.GetUserRegistryKey();

            var keyNames = key.GetValueNames().Where(x => x.StartsWith(COLLECTION_KEY)).ToList();

            foreach (var keyName in keyNames)
            {
                var value = (string)key.GetValue(keyName);
                settings.Collections.Add(value);
            }

            Current = settings;
        }

        public static void Save()
        {

            RegistryKey key = RegistryHelper.GetUserRegistryKey();

            var keyNames = key.GetValueNames().Where(x => x.StartsWith(COLLECTION_KEY)).ToList();

            foreach (var keyName in keyNames)
            {
                key.DeleteValue(keyName);
            }

            for (int i = 0; i < Current.Collections.Count; i++)
            {
                key.SetValue(COLLECTION_KEY + i.ToString(), Current.Collections[i]);
            }



        }

        public List<string> Collections { get; set; } = new List<string>();

    }
}
