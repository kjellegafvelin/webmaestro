using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using WebMaestro.Models;

namespace WebMaestro.ViewModels.Explorer
{
    internal class EnvironmentViewModel : ObservableObject
    {
        private readonly EnvironmentModel environmentModel;
        public EnvironmentViewModel(EnvironmentModel environmentModel)
        {
            this.environmentModel = environmentModel;
            this.DeleteCommand = new RelayCommand(Delete);
        }
        public string Name => environmentModel.Name;
        public ICommand DeleteCommand { get; }
        private void Delete() {}
    }
}
