using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WebMaestro.Dialogs;
using WebMaestro.Models;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.ViewModels.Explorer
{
    internal sealed class EnvironmentsViewModel : ObservableObject
    {
        private readonly ObservableCollection<EnvironmentModel> environmentModels;
        private readonly IDialogService dialogService;

        public EnvironmentsViewModel(ObservableCollection<EnvironmentModel> environmentModels)
        {
            this.AddCommand = new RelayCommand(Add);
            this.environmentModels = environmentModels;

            foreach (var environment in this.environmentModels)
            {
                this.Environments.Add(new EnvironmentViewModel(environment));
            }

            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();

            this.EditCommand = new RelayCommand(Edit);
        }

        public ICommand AddCommand { get; }

        private void Add()
        {

        }

        public ICommand EditCommand { get; }

        public void Edit()
        {
            var explorer = Ioc.Default.GetRequiredService<ExplorerViewModel>();
            var dlg = new EnvironmentEditorViewModel();

            _ = this.dialogService.ShowDialog(explorer, dlg);
        }

        public ObservableCollection<EnvironmentViewModel> Environments { get; } = new();
    }
}
