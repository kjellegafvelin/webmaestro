using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using WebMaestro.Models;
using WebMaestro.Services;
using System.Linq;

namespace WebMaestro.ViewModels.Explorer
{
    internal class RequestsViewModel : ObservableObject
    {
        private readonly CollectionModel collectionModel;
        private readonly CollectionsService collectionsService;
        private readonly ExplorerViewModel explorer;
        private readonly IDialogService dialogService;

        public RequestsViewModel(CollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;

            foreach (var file in this.collectionModel.Files.OrderBy(x => x.HttpMethod).OrderBy(x => x.Name))
            {
                this.Requests.Add(new RequestViewModel(file, collectionModel));
            }

            this.collectionsService = Ioc.Default.GetRequiredService<CollectionsService>();

            this.collectionModel.Files.CollectionChanged += Files_CollectionChanged;

            this.explorer = Ioc.Default.GetRequiredService<ExplorerViewModel>();
            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
        }

        private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (CollectionFileModel file in e.NewItems)
                    {
                        this.Requests.Add(new RequestViewModel(file, this.collectionModel));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.Requests.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.Requests.Clear();
                    foreach (CollectionFileModel file in e.NewItems)
                    {
                        this.Requests.Add(new RequestViewModel(file, this.collectionModel));
                    }
                    break;
                default:
                    break;
            }
        }

        public ObservableCollection<RequestViewModel> Requests { get; } = new();

        private ICommand addRequestCommand;

        public ICommand AddRequestCommand => addRequestCommand ??= new AsyncRelayCommand(AddRequest);

        private async Task AddRequest()
        {
            await this.collectionsService.AddRequestAsync(collectionModel);
        }

        private ICommand addExistingCommand;

        public ICommand AddExistingCommand => addExistingCommand ??= new AsyncRelayCommand(AddExisting);

        private async Task AddExisting()
        {
            var settings = new OpenFileDialogSettings()
            {
                Title = "Add Existing Item",
                Filter = "WebMaestro Request File (*.req)|*.req"
            };

            if (this.dialogService.ShowOpenFileDialog(this.explorer, settings) == true)
            {
                await this.collectionsService.AddExistingRequestAsync(collectionModel, settings.FileName);
            }
        }
    }
}
