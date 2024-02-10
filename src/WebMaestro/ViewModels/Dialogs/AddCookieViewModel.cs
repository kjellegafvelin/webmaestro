#nullable enable

using MvvmDialogs;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WebMaestro.ViewModels.Dialogs
{
    internal partial class AddCookieViewModel : ObservableValidator, IModalDialogViewModel
    {
        public AddCookieViewModel()
        {

        }

        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        private string? name;

        [ObservableProperty]
        [Required]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        private string? value;

        [ObservableProperty]
        private bool isEdit;

        [RelayCommand(CanExecute = nameof(OkExecute))]
        private void Ok()
        {
            if (this.HasErrors)
            {
                return;
            }

            this.DialogResult = true;
        }

        private bool OkExecute()
        {
            return this.Name != null && this.Value != null;
        }



    }
}
