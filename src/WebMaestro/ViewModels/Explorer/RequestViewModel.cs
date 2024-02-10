using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System.Windows.Input;
using WebMaestro.Messages;
using WebMaestro.Models;
using WebMaestro.Services;

namespace WebMaestro.ViewModels.Explorer
{
    public class RequestViewModel : ObservableObject
    {
        private readonly IDialogService dialogService;
        private readonly ExplorerViewModel explorer;
        private readonly CollectionsService collectionsService;
        private readonly CollectionFileModel collectionFileModel;
        private readonly CollectionModel collectionModel;

        public RequestViewModel(CollectionFileModel collectionFileModel, CollectionModel collectionModel)
        {
            this.collectionFileModel = collectionFileModel;
            this.collectionModel = collectionModel;

            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
            this.explorer = Ioc.Default.GetRequiredService<ExplorerViewModel>();
            this.collectionsService = Ioc.Default.GetRequiredService<CollectionsService>();
        }

        public string Name => collectionFileModel.Name;

        public string Url => collectionFileModel.Url;

        public string Filename => collectionFileModel.FileName;

        public HttpMethods Method => collectionFileModel.HttpMethod;

        private RelayCommand openCommand;
        public RelayCommand OpenCommand => this.openCommand ??= new(Open);

        private void Open()
        {
            var msg = new OpenRequestMessage()
            {
                Id = collectionFileModel.Id,
                Filename = collectionFileModel.FileName,
                CollectionId = collectionModel.Id
            };

            _ = WeakReferenceMessenger.Default.Send(msg);
        }

        private RelayCommand deleteCommand;
        public RelayCommand DeleteCommand => this.deleteCommand ??= new(Delete);

        private void Delete()
        {
            var settings = new MessageBoxSettings()
            {
                Caption = "WebMaestro",
                MessageBoxText = $"'{ this.Name }' will be deleted permanently.",
                Icon = System.Windows.MessageBoxImage.Warning,
                Button = System.Windows.MessageBoxButton.OKCancel
            };

            if (this.dialogService.ShowMessageBox(this.explorer, settings) == System.Windows.MessageBoxResult.OK)
            {
                this.collectionsService.DeleteRequestAsync(this.collectionModel, this.collectionFileModel);
            }
        }
    }
}