﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmDialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WebMaestro.Helpers;
using WebMaestro.Messages;
using WebMaestro.Models;
using WebMaestro.Services;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        private readonly DispatcherTimer dispatcherTimer = new();
        private readonly HttpClient httpClient = new();
        private readonly FileService fileService;

        public MainViewModel(FileService fileService)
        {
            this.fileService = fileService;
            WeakReferenceMessenger.Default.Register<OpenRequestMessage>(this, HandleOpenRequestMessage);

            dispatcherTimer.Tick += async (_, _) => await this.GetPublicIPAddress();
            dispatcherTimer.Interval = TimeSpan.FromSeconds(5);
            dispatcherTimer.Start();

            this.Initialize();
        }

        private void Initialize()
        {
            this.AppState = this.fileService.LoadAppState();

            Task.Run(() =>
            {
                var docs = this.fileService.LoadOpenTabs();
                foreach (var doc in docs)
                {
                    if (doc.Type == nameof(WebViewModel))
                    {
                        var req = fileService.ReadTempFile<RequestModel>(doc.File);
                        if (req != null)
                        {
                            this.ViewModels.Add(new WebViewModel(doc.Id, req, Guid.Empty));
                        }
                    }
                    else if (doc.Type == nameof(WebServerViewModel))
                    {
                        this.ViewModels.Add(new WebServerViewModel());
                    }
                }
            });
        }

        private async Task GetPublicIPAddress()
        {
            dispatcherTimer.Interval = TimeSpan.FromMinutes(30);

            try
            {
                var ipAddress = await httpClient.GetStringAsync("https://api.getwebmaestro.com/GetCallerIP");
                this.PublicIPAddress = ipAddress;
            }
            catch
            {
                this.PublicIPAddress = "<Not connected to Internet>";
            }
        }

        private void HandleOpenRequestMessage(object recipient, OpenRequestMessage message)
        {
            if (this.ViewModels.Any(x => x.Id == message.Id))
            {
                this.SelectedTabItemIndex = this.ViewModels.IndexOf(this.ViewModels.First(x => x.Id == message.Id));
            }
            else
            {
                var req = FileHelpers.ReadJsonFile<RequestModel>(message.Filename);
                this.ViewModels.Add(new WebViewModel(message.Id, req!, message.CollectionId) 
                { 
                    Filename = message.Filename, 
                    Tooltip = $"{message.CollectionId} - {message.Filename}" 
                });
            }
        }

        public ObservableCollection<TabItemViewModel> ViewModels { get; } = new();

        [ObservableProperty]
        private bool isServerStarted;

        [ObservableProperty]
        private int selectedTabItemIndex = -1;

        [ObservableProperty]
        private TabItemViewModel? selectedTabItem;

        [ObservableProperty]
        private string publicIPAddress = "Getting IP address...";

        public AppStateModel AppState { get; set; } = new();

        [RelayCommand]
        private void NewRequest()
        {

            var req = new RequestModel();

            req.Headers.Add(new HeaderModel("Cache-Control", "no-cache,no-store"));
            req.Headers.Add(new HeaderModel("Pragma", "no-cache"));
            req.Headers.Add(new HeaderModel("Accept", "*/*"));
            req.Headers.Add(new HeaderModel("Accept-Encoding", "gzip, deflate"));
            req.Headers.Add(new HeaderModel("User-Agent", "WebMaestro"));

            var wvm = new WebViewModel(); // System.Guid.Empty, req, System.Guid.Empty);
            
            this.ViewModels.Add(wvm);
        }

        [RelayCommand]
        private void NewServer()
        {
            this.ViewModels.Add(new WebServerViewModel());
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }

        [RelayCommand]
        private async Task Save()
        {
            await this.SelectedTabItem!.Save();
        }

        [RelayCommand]
        private void Prettify()
        {
            var dialogService = Ioc.Default.GetRequiredService<IDialogService>();

            var vm = new PrettifyToolViewModel();
            dialogService.ShowDialog(this, vm);
        }

        [RelayCommand]
        private void EscapeJson()
        {
            var dialogService = Ioc.Default.GetRequiredService<IDialogService>();

            var vm = new EscapeJsonToolViewModel();
            dialogService.ShowDialog(this, vm);
        }

        [RelayCommand]
        private void ImportRawHttp()
        {
            var dialogService = Ioc.Default.GetRequiredService<IDialogService>();

            var vm = new ImportRawHttpViewModel();
            if (dialogService.ShowDialog(this, vm) == true)
            {
                foreach (var req in vm.Requests)
                {
                    var wvm = new WebViewModel(System.Guid.Empty, req, System.Guid.Empty);
                    this.ViewModels.Add(wvm);
                }
            }
        }

        [RelayCommand]
        private void CopyPublicIPAddress()
        {
            if (this.PublicIPAddress != null)
            {
                Clipboard.SetText(this.PublicIPAddress, TextDataFormat.UnicodeText);
            }
        }
    }
}
