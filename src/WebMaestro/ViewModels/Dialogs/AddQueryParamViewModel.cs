using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.ComponentModel.DataAnnotations;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class AddQueryParamViewModel : ObservableValidator, IModalDialogViewModel
    {
        public AddQueryParamViewModel()
        {

        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => dialogResult;
            private set => SetProperty(ref dialogResult, value);
        }

        private string key;
        [Required]
        public string Key
        {
            get => key;
            set
            {
                SetProperty(ref key, value);
                OkCommand.NotifyCanExecuteChanged();
            }
        }

        private string value;
        [Required]
        public string Value
        {
            get => value;
            set
            {
                SetProperty(ref this.value, value);
                OkCommand.NotifyCanExecuteChanged();
            }
        }

        private bool isEditing;
        public bool IsEditing
        {
            get => isEditing;
            set => SetProperty(ref isEditing, value);
        }

        private RelayCommand okCommand;
        public RelayCommand OkCommand => okCommand ??= new RelayCommand(Ok, CanExecuteOk);

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
