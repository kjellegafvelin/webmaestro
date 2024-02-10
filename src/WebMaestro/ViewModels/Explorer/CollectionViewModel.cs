using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System.Threading.Tasks;
using System.Windows.Input;
using WebMaestro.Models;
using WebMaestro.Services;
using WebMaestro.ViewModels.Explorer;

namespace WebMaestro.ViewModels.Explorer
{
    internal sealed class CollectionViewModel : ObservableObject
    {
        private readonly CollectionModel collectionModel;
        private readonly IDialogService dialogService;
        private readonly CollectionsService collectionsService;

        public CollectionViewModel(CollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;
            
            this.Environments = new EnvironmentsViewModel(this.collectionModel.Environments);
            this.Variables = new VariablesViewModel(this.collectionModel.Variables);
            this.Requests = new RequestsViewModel(collectionModel);
            
            this.RemoveCommand = new AsyncRelayCommand(Remove);

            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
            this.collectionsService = Ioc.Default.GetRequiredService<CollectionsService>();
        }

        public string Name
        {
            get { return collectionModel.Name; }
            set { this.SetProperty(collectionModel.Name, value, x => collectionModel.Name = x); }
        }

        public ICommand RemoveCommand { get; }
        private async Task Remove()
        {
            var explorer = Ioc.Default.GetRequiredService<ExplorerViewModel>();
            
            var settings = new MessageBoxSettings()
            {
                Caption = "WebMaestro",
                MessageBoxText = $"'{ collectionModel.Name }' will be removed.",
                Button = System.Windows.MessageBoxButton.OKCancel,
                Icon = System.Windows.MessageBoxImage.Warning
            };

            if (this.dialogService.ShowMessageBox(explorer, settings) == System.Windows.MessageBoxResult.OK)
            {
                await this.collectionsService.RemoveCollection(this.collectionModel);
            }
        }

        public EnvironmentsViewModel Environments { get; }

        public VariablesViewModel Variables { get; }

        public RequestsViewModel Requests { get; }
    }
}
