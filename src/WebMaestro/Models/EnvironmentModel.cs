using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace WebMaestro.Models
{
    public partial class EnvironmentModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private Authentication authentication = new();

        public ObservableCollection<VariableModel> Variables { get; set; } = new();
    }
}
