using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WebMaestro.Models
{
    public partial class EnvironmentModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        public ObservableCollection<VariableModel> Variables { get; set; } = new();
    }
}
