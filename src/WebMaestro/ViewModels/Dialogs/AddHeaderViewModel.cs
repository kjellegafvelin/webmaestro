using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.ComponentModel.DataAnnotations;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class AddHeaderViewModel : ObservableValidator, IModalDialogViewModel
    {
        public AddHeaderViewModel()
        {

        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => this.dialogResult;
            private set => SetProperty(ref dialogResult, value);
        }

        private string name;

        [Required]
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref this.name, value);
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

        private string description;

        public string Description
        {
            get => description;
            set => SetProperty(ref description, value);
        }

        private bool isEdit;

        public bool IsEdit
        {
            get => isEdit;
            set => SetProperty(ref isEdit, value);
        }

        private RelayCommand okCommand;

        public RelayCommand OkCommand => okCommand ??= new RelayCommand(Ok, CanExecuteOk);

        private void Ok()
        {
            if (this.HasErrors)
            {
                return;
            }

            DialogResult = true;
        }

        private bool CanExecuteOk()
        {
            this.ValidateAllProperties();

            return !this.HasErrors;
        }

    }
}
