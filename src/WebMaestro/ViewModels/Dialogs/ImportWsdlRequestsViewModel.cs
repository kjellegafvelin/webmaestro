using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System;
using System.IO;

namespace WebMaestro.ViewModels.Dialogs
{
    internal partial class ImportWsdlRequestsViewModel : ObservableObject, IModalDialogViewModel
    {
        private readonly IDialogService dialogService;

        public ImportWsdlRequestsViewModel(string location)
        {
            this.location = location;

            dialogService = Ioc.Default.GetRequiredService<IDialogService>();
        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => this.dialogResult;
            private set => SetProperty(ref dialogResult, value);
        }

        private string path;
        public string Path
        {
            get => path;
            set => SetProperty(ref path, value);
        }

        private string url;
        public string Url
        {
            get => url;
            set => SetProperty(ref url, value);
        }

        private bool isUrl = true;
        public bool IsUrl
        {
            get => isUrl;
            set => SetProperty(ref isUrl, value);
        }

        private string location;
        public string Location
        {
            get => location;
            set => SetProperty(ref location, value);
        }

        [RelayCommand]
        private void BrowseForFile()
        {
            var settings = new OpenFileDialogSettings()
            {
                Title = "Import Open API file",
                Filter = "Json (*.json)|*.json|Xml (*.xml)|*.xml"
            };

            if (dialogService.ShowOpenFileDialog(this, settings) == true)
            {
                this.Path = settings.FileName;
            }
        }

        [RelayCommand]
        private void BrowseForFolder()
        {
            var settings = new FolderBrowserDialogSettings()
            {
                SelectedPath = this.Location
            };

            if (dialogService.ShowFolderBrowserDialog(this, settings) == true)
            {
                this.Location = settings.SelectedPath;
            }
        }

        [RelayCommand]
        private void Ok()
        {
            var settings = new MessageBoxSettings()
            {
                Caption = "Error",
                Icon = System.Windows.MessageBoxImage.Error,
                Button = System.Windows.MessageBoxButton.OK
            };

            if (IsUrl)
            {
                if (string.IsNullOrWhiteSpace(this.Url) || !Uri.TryCreate(this.Url, UriKind.Absolute, out _))
                {
                    settings.MessageBoxText = "The specified Url is not valid. Please check it and try again.";
                    dialogService.ShowMessageBox(this, settings);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(this.Path) || !File.Exists(this.Path))
                {
                    settings.MessageBoxText = "The specified filename is not valid or the file does not exist. Please check it and try again.";
                    dialogService.ShowMessageBox(this, settings);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(this.Location) || !Directory.Exists(this.Location))
            {
                settings.MessageBoxText = "The specified location is not valid or the path does not exist. Please check it and try again.";
                dialogService.ShowMessageBox(this, settings);
                return;
            }

            this.DialogResult = true;
        }
    }
}
