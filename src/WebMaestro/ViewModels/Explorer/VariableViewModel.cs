using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using WebMaestro.Models;

namespace WebMaestro.ViewModels.Explorer
{
    public class VariableViewModel : ObservableObject
    {
        private readonly VariableModel variableModel;

        public VariableViewModel(VariableModel variableModel)
        {
            this.variableModel = variableModel;

            this.DeleteCommand = new RelayCommand(Delete);
            this.EditCommand = new RelayCommand(Edit);
        }

        public string Name
        {
            get { return this.variableModel.Name; }
        }

        public string Value
        {
            get { return this.variableModel.Value; }
        }

        public ICommand DeleteCommand { get; }

        private void Delete()
        {

        }

        public ICommand EditCommand { get; }

        private void Edit()
        {
            
        }
    }
}