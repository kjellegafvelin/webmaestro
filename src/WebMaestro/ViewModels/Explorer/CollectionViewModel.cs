using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using WebMaestro.Models;
using WebMaestro.Services;
using WebMaestro.ViewModels.Dialogs;

namespace WebMaestro.ViewModels.Explorer
{
    internal sealed partial class CollectionViewModel : ObservableObject
    {
        private readonly CollectionModel collectionModel;
        private readonly IDialogService dialogService;
        private readonly CollectionsService collectionsService;

        public CollectionViewModel(CollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;

            this.Requests = new RequestsViewModel(collectionModel);

            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
            this.collectionsService = Ioc.Default.GetRequiredService<CollectionsService>();
        }

        public string Name
        {
            get { return collectionModel.Name; }
            set { this.SetProperty(collectionModel.Name, value, x => collectionModel.Name = x); }
        }

        [RelayCommand]
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

        public RequestsViewModel Requests { get; }

        //private RelayCommand editCommand;
        //public ICommand EditCommand => editCommand ??= new RelayCommand(Edit);


        [RelayCommand]
        private async Task Edit()
        {
            var explorer = Ioc.Default.GetRequiredService<ExplorerViewModel>();

            // Deep clone current environments to edit in dialog
            var editList = new ObservableCollection<EnvironmentModel>(this.collectionModel.Environments.Select(CloneEnvironment));

            var dlg = new EnvironmentEditorViewModel(editList);

            if (this.dialogService.ShowDialog(explorer, dlg) == true && dlg.DialogResult == true)
            {
                // Apply changes back to collection model and persist
                this.collectionModel.Environments.Clear();
                foreach (var e in dlg.Environments)
                {
                    this.collectionModel.Environments.Add(CloneEnvironment(e));
                }

                // Save collection
                await this.collectionsService.SaveCollectionAsync(this.collectionModel);
            }
        }

        private static EnvironmentModel CloneEnvironment(EnvironmentModel source)
        {
            return new EnvironmentModel()
            {
                Name = source.Name,
                Variables = new ObservableCollection<VariableModel>(source.Variables.Select(v => new VariableModel(v.Name, v.Value, v.Description))),
                Authentication = CloneAuthentication(source.Authentication)
            };
        }

        private static Authentication CloneAuthentication(Authentication source)
        {
            if (source == null)
            {
                return new Authentication();
            }

            return new Authentication()
            {
                Type = source.Type,
                Key = source.Key,
                Value = source.Value,
                Username = source.Username,
                Password = source.Password,
                ApiKeyLocation = source.ApiKeyLocation,
                Token = source.Token
            };
        }

    }
}
