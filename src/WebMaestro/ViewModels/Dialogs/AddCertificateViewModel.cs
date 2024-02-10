using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System.ComponentModel.DataAnnotations;

namespace WebMaestro.ViewModels.Dialogs
{
    class AddCertificateViewModel : ObservableValidator, IModalDialogViewModel
    {
        private readonly IDialogService dialogService;

        public AddCertificateViewModel()
        {
            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => this.dialogResult;
            private set => SetProperty(ref dialogResult, value);
        }

        private string fileName;
        [Required]
        public string FileName
        {
            get => fileName;
            set
            {
                SetProperty(ref fileName, value);
                OkCommand.NotifyCanExecuteChanged();
            }
        }

        private bool hasPassword;
        public bool HasPassword
        {
            get => hasPassword;
            set => SetProperty(ref hasPassword, value);
        }

        private string password;
        public string Password
        {
            get => password;
            set => SetProperty(ref password, value);
        }

        private RelayCommand browseFileCommand;
        public RelayCommand BrowseFileCommand => browseFileCommand ??= new(BrowseFile);

        private void BrowseFile()
        {
            var settings = new OpenFileDialogSettings()
            {
                Title = "Select Client Certificate",
                Filter = "Certificate (*.pfx,*.cer)|*.pfx;*cer",
            };

            if (this.dialogService.ShowOpenFileDialog(this, settings) == true)
            {
                this.FileName = settings.FileName;
            }
        }


        private RelayCommand okCommand;
        public RelayCommand OkCommand => okCommand ??= new(Ok, CanExecuteOk);

        private void Ok()
        {
            if (this.HasErrors)
            {
                return;
            }

            this.DialogResult = true;
        }

        private bool CanExecuteOk()
        {
            this.ValidateAllProperties();

            return !this.HasErrors;
        }
    }
}
