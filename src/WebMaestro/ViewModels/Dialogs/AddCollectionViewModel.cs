using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.FolderBrowser;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
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

        // Base location (Documents\WebMaestro)
        private readonly string baseLocation;

        // If true, the user has manually changed the Location (via UI or SelectFolder)
        private bool locationManuallyChanged = false;

        // Suppress marking manual change when setting Location programmatically
        private bool suppressLocationManualFlag = false;

        public AddCollectionViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            this.SelectFolderCommand = new RelayCommand(SelectFolder);
            this.OkCommand = new RelayCommand(Ok, CanExexuteOk);

            // Initialize base location to Documents\WebMaestro
            baseLocation = Path.Combine(GetFolderPath(SpecialFolder.MyDocuments), "WebMaestro");

            // Set default Location to baseLocation without marking as manual
            suppressLocationManualFlag = true;
            this.Location = baseLocation;
            suppressLocationManualFlag = false;
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
                // user selected a folder -> mark as manual via Location setter
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

                // If location hasn't been manually changed by the user, update it to baseLocation\CollectionName
                if (!locationManuallyChanged)
                {
                    if (!string.IsNullOrWhiteSpace(collectionName))
                    {
                        suppressLocationManualFlag = true;
                        this.Location = Path.Combine(baseLocation, collectionName);
                        suppressLocationManualFlag = false;
                    }
                    else
                    {
                        // if name is cleared, revert to baseLocation
                        suppressLocationManualFlag = true;
                        this.Location = baseLocation;
                        suppressLocationManualFlag = false;
                    }
                }
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

                // If this assignment is not suppressed, consider it a manual change by the user
                if (!suppressLocationManualFlag)
                {
                    locationManuallyChanged = true;
                }

                this.OkCommand.NotifyCanExecuteChanged();
            }
        }
    }
}
