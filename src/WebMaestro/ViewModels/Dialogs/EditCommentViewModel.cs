using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class EditCommentViewModel : ObservableObject, IModalDialogViewModel
    {
        public EditCommentViewModel()
        {

        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => dialogResult;
            private set => SetProperty(ref dialogResult, value);
        }

        private string comment;
        public string Comment
        {
            get => comment;
            set => SetProperty(ref comment, value);
        }

        private RelayCommand okCommand;
        public RelayCommand OkCommand => okCommand ??= new RelayCommand(Ok);

        private void Ok()
        {
            this.DialogResult = true;
        }
    }
}
