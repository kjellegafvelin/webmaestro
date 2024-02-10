using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.ComponentModel.DataAnnotations;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class AddVariableViewModel : ObservableValidator, IModalDialogViewModel
    {
        public AddVariableViewModel()
        {

        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => dialogResult;
            set => SetProperty(ref dialogResult, value);
        }

        private bool isEditing;
        public bool IsEditing
        {
            get => isEditing;
            set => SetProperty(ref isEditing, value);
        }

        private string name;
        [Required]
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
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
