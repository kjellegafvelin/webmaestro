﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MvvmDialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WebMaestro.Helpers;
using WebMaestro.Messages;
using WebMaestro.Models;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        private Timer ipCheckTimer;

        public MainViewModel()
        {
            WeakReferenceMessenger.Default.Register<OpenRequestMessage>(this, HandleOpenRequestMessage);

            this.ipCheckTimer = new Timer(GetPublicIPAddress, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(30));
        }

        private void GetPublicIPAddress(object state)
        {
            var client = new HttpClient();

            try
            {
                var ipAddress = client.GetStringAsync("https://api64.ipify.org").GetAwaiter().GetResult();
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
                this.ViewModels.Add(new WebViewModel(message.Id, req, message.CollectionId));
            }
        }

        public ObservableCollection<TabItemViewModel> ViewModels { get; } = new();

        [ObservableProperty]
        private bool isServerStarted;

        [ObservableProperty]
        private int selectedTabItemIndex = -1;

        [ObservableProperty]
        private TabItemViewModel selectedTabItem;

        [ObservableProperty]
        private string publicIPAddress = "Getting IP address...";

        [RelayCommand]
        private void Init()
        {

        }

        [RelayCommand]
        private void NewRequest()
        {

            var req = new RequestModel();

            req.Headers.Add(new HeaderModel("Cache-Control", "no-cache,no-store"));
            req.Headers.Add(new HeaderModel("Pragma", "no-cache"));
            req.Headers.Add(new HeaderModel("Accept", "*/*"));
            req.Headers.Add(new HeaderModel("Accept-Encoding", "gzip, deflate"));
            req.Headers.Add(new HeaderModel("User-Agent", "WebMaestro"));

            var wvm = new WebViewModel(System.Guid.Empty, req, System.Guid.Empty);
            
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
            await this.SelectedTabItem.Save();
        }

        [RelayCommand]
        private void Prettify()
        {
            var dialogService = Ioc.Default.GetRequiredService<IDialogService>();
            
            var vm = new PrettifyToolViewModel();
            dialogService.ShowDialog(this, vm);
        }

        [RelayCommand]
        private void ImportRawHttp()
        {
            var dialogService = Ioc.Default.GetRequiredService<IDialogService>();

            var vm = new ImportRawHttpViewModel();
            if (dialogService.ShowDialog(this, vm) == true)
            {
                var wvm = new WebViewModel(System.Guid.Empty, vm.Request, System.Guid.Empty);

                this.ViewModels.Add(wvm);
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
