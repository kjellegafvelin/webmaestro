using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace WebMaestro.Models
{
    internal class OptionsModel : ObservableObject
    {
        /// <summary>
        /// Creates a <see cref="OptionsModel"/> object with default setttings.
        /// </summary>
        /// <returns><see cref="OptionsModel"/></returns>
        public static OptionsModel Create()
        {
            var options = new OptionsModel();
            options.collectionsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WebMaestro");

            return options;
        }

        private string collectionsPath;
        public string CollectionsPath
        {
            get => collectionsPath;
            set => SetProperty(ref collectionsPath, value);
        }

    }
}
