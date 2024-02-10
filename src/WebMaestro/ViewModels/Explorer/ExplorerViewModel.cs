using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using WebMaestro.Models;
using WebMaestro.Services;
using WebMaestro.ViewModels.Dialogs;
using System.Linq;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using Microsoft.OpenApi.Any;
using System.Collections.Generic;
using WebMaestro.Importers;
using System;

namespace WebMaestro.ViewModels.Explorer
{
    internal partial class ExplorerViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;
        private readonly CollectionsService collectionsService;
        private readonly OptionsModel options;

        public ExplorerViewModel(IDialogService dialogService, CollectionsService collectionsService, OptionsModel options)
        {
            this.dialogService = dialogService;
            this.collectionsService = collectionsService;
            this.options = options;
            this.collectionsService.Collections.CollectionChanged += Collections_CollectionChanged;
        }

        private void Collections_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CollectionModel item in e.NewItems)
                    {
                        this.collections.Add(new CollectionViewModel(item));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.collections.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.collections.Clear();
                    foreach (CollectionModel item in e.NewItems)
                    {
                        this.collections.Add(new CollectionViewModel(item));
                    }
                    break;
                default:
                    break;
            }
        }

        private ObservableCollection<CollectionViewModel> collections = new();

        public ObservableCollection<CollectionViewModel> Collections
        {
            get
            {
                return this.collections;
            }
            set
            {
                SetProperty(ref this.collections, value);
            }
        }

        [RelayCommand]
        private async Task AddCollection()
        {
            var dlg = Ioc.Default.GetService<AddCollectionViewModel>();
            if (dialogService.ShowDialog(this, dlg) == true)
            {
                await this.collectionsService.CreateCollectionAsync(dlg.CollectionName, dlg.Location);
            }
        }

        [RelayCommand]
        private async Task OpenCollection()
        {
            var settings = new OpenFileDialogSettings()
            {
                Title = "Open existing collection",
                Filter = "WebMaestro Collection (*.coll)|*.coll"
            };

            if (dialogService.ShowOpenFileDialog(this, settings) == true)
            {
                await this.collectionsService.LoadCollectionAsync(settings.FileName);
            }
        }

        [RelayCommand]
        private async Task Init()
        {
            await this.collectionsService.LoadCollectionsAsync();
        }

        [RelayCommand]
        private async Task CreateFromOpenApi()
        {
            var vm = new ImportOpenApiRequestsViewModel(options.CollectionsPath);

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                Stream stream;
                OpenApiImporter importer = null;

                if (vm.IsUrl)
                {
                    var client = new HttpClient();

                    stream = await client.GetStreamAsync(vm.Url);

                    var url = new Uri(vm.Url);
                    var baseUrl = $"{ url.Scheme }://{ url.DnsSafeHost }";
                    importer = new OpenApiImporter(baseUrl);
                }
                else
                {
                    stream = File.OpenRead(vm.Path);
                    importer = new OpenApiImporter();
                }

                var mboxSetttings = new MessageBoxSettings()
                {
                    Caption = "Error",
                    Icon = System.Windows.MessageBoxImage.Error,
                    Button = System.Windows.MessageBoxButton.OK
                };

                importer.Import(stream);

                if (collectionsService.Collections.Any(x => x.Name.Equals(importer.Collection.Name, System.StringComparison.OrdinalIgnoreCase)))
                {
                    mboxSetttings.MessageBoxText = $"A collection with the name '{ importer.Collection.Name }' already exists.";
                    this.dialogService.ShowMessageBox(this, mboxSetttings);
                    return;
                }
                
                var existingPath = Path.Combine(vm.Location, importer.Collection.Name);

                if (Directory.Exists(existingPath))
                {
                    mboxSetttings.MessageBoxText = $"A folder with the name '{ existingPath }' already exists.";
                    this.dialogService.ShowMessageBox(this, mboxSetttings);
                    return;
                }
                

                var collection = await collectionsService.CreateCollectionAsync(importer.Collection, vm.Location);

                foreach (var req in importer.Requests)
                {
                    await collectionsService.AddRequestAsync(collection, req);
                }
            }
        }


        [RelayCommand]
        private async Task CreateFromWsdl()
        {
            var vm = new ImportWsdlRequestsViewModel(options.CollectionsPath);

            if (this.dialogService.ShowDialog(this, vm) == true)
            {
                Stream stream;
                WsdlImporter importer = null;

                if (vm.IsUrl)
                {
                    var client = new HttpClient();

                    stream = await client.GetStreamAsync(vm.Url);

                    var url = new Uri(vm.Url);
                    var baseUrl = $"{ url.Scheme }://{ url.DnsSafeHost }";
                    importer = new (baseUrl);
                }
                else
                {
                    stream = File.OpenRead(vm.Path);
                    importer = new ();
                }

                var mboxSetttings = new MessageBoxSettings()
                {
                    Caption = "Error",
                    Icon = System.Windows.MessageBoxImage.Error,
                    Button = System.Windows.MessageBoxButton.OK
                };

                importer.Import(stream);

                if (collectionsService.Collections.Any(x => x.Name.Equals(importer.Collection.Name, System.StringComparison.OrdinalIgnoreCase)))
                {
                    mboxSetttings.MessageBoxText = $"A collection with the name '{ importer.Collection.Name }' already exists.";
                    this.dialogService.ShowMessageBox(this, mboxSetttings);
                    return;
                }

                var existingPath = Path.Combine(vm.Location, importer.Collection.Name);

                if (Directory.Exists(existingPath))
                {
                    mboxSetttings.MessageBoxText = $"A folder with the name '{ existingPath }' already exists.";
                    this.dialogService.ShowMessageBox(this, mboxSetttings);
                    return;
                }


                var collection = await collectionsService.CreateCollectionAsync(importer.Collection, vm.Location);

                foreach (var req in importer.Requests)
                {
                    await collectionsService.AddRequestAsync(collection, req);
                }
            }
        }
    }
}
