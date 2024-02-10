using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.ComponentModel.DataAnnotations;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class LicenseCheckerViewModel : ObservableValidator, IModalDialogViewModel
    {
        public LicenseCheckerViewModel()
        {

        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => dialogResult;
            private set => SetProperty(ref dialogResult, value);
        }

        private string serialKey;
        [Required]
        public string SerialKey
        {
            get => serialKey;
            set => SetProperty(ref serialKey, value);
        }

        private RelayCommand okCommand;
        public RelayCommand OkCommand => okCommand ??= new RelayCommand(Ok, CanExectueOk);

        private void Ok()
        {
            if (this.HasErrors)
            {
                return;
            }

            this.DialogResult = true;
        }

        private bool CanExectueOk()
        {
            this.ValidateAllProperties();

            return !this.HasErrors;
        }


    }
}
