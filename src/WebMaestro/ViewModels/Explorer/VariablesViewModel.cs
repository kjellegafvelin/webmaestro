using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WebMaestro.Models;

namespace WebMaestro.ViewModels.Explorer
{
    internal sealed class VariablesViewModel : ObservableObject
    {
        private readonly ObservableCollection<VariableModel> variableModels;

        public VariablesViewModel(ObservableCollection<VariableModel> variableModels)
        {

            this.AddCommand = new RelayCommand(Add);
            this.variableModels = variableModels;

            foreach (var variable in this.variableModels)
            {
                this.Variables.Add(new VariableViewModel(variable));
            }
        }

        public ICommand AddCommand { get; }

        private void Add()
        {

        }

        public ObservableCollection<VariableViewModel> Variables { get; } = new();
    }
}
