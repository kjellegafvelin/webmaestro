using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Environment;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class AddCollectionViewModel : ObservableValidator, IModalDialogViewModel
    {
        private bool? dialogResult;

        public bool? DialogResult
        {
            get { return dialogResult; }
            private set { this.SetProperty(ref this.dialogResult, value); }
        }

        private readonly IDialogService dialogService;

        public AddCollectionViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            this.SelectFolderCommand = new RelayCommand(SelectFolder);
            this.OkCommand = new RelayCommand(Ok, CanExexuteOk);
        }

        public ICommand SelectFolderCommand { get; }
        private void SelectFolder()
        {
            var settings = new FolderBrowserDialogSettings()
            {
                //SelectedPath = Environment.GetFolderPath(SpecialFolder.LocalApplicationData),
                Description = "Browse",
                ShowNewFolderButton = true
            };

            if (dialogService.ShowFolderBrowserDialog(this, settings) == true)
            {
                this.Location = settings.SelectedPath;
            }
        }

        public RelayCommand OkCommand { get; }
        private void Ok()
        {
            if (this.HasErrors)
            {
                var error = this.GetErrors().First();

                var settings = new MessageBoxSettings()
                {
                    Caption = "Error",
                    Icon = System.Windows.MessageBoxImage.Error,
                    MessageBoxText = error.ErrorMessage,
                    Button = System.Windows.MessageBoxButton.OK
                };

                _ = dialogService.ShowMessageBox(this, settings);
                return;
            }

            DialogResult = true;
        }

        private bool CanExexuteOk()
        {
            this.ValidateAllProperties();

            return !this.HasErrors;
        }

        private string collectionName;

        [Required]
        public string CollectionName
        {
            get => collectionName;
            set
            {
                this.SetProperty(ref collectionName, value, true);
                this.OkCommand.NotifyCanExecuteChanged();
            }
        }

        private string location;

        [Required]
        public string Location
        {
            get => location;
            set
            {
                this.SetProperty(ref location, value, true);
                this.OkCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
