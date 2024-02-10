using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using WebMaestro.Importers;
using WebMaestro.Models;

namespace WebMaestro.ViewModels.Dialogs
{
    internal partial class ImportRawHttpViewModel : ObservableObject, IModalDialogViewModel
    {
        public ImportRawHttpViewModel()
        {

        }

        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private string source;

        public RequestModel Request { get; set; }

        [RelayCommand]
        private void Import()
        {
            try
            {
                var importer = new RawHttpImporter();
                this.Request = importer.Import(this.Source);

                this.DialogResult = true;
            }
            catch(Exception)
            {
                var dialogService = Ioc.Default.GetRequiredService<IDialogService>();
                var settings = new MessageBoxSettings()
                {
                    Caption = "Error",
                    MessageBoxText = "Failed to parse the request.",
                    Button = System.Windows.MessageBoxButton.OK,
                    Icon = System.Windows.MessageBoxImage.Error
                };

                dialogService.ShowMessageBox(this, settings);
            }
        }
    }
}
