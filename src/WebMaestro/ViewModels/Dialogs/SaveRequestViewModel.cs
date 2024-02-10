using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using WebMaestro.Models;
using WebMaestro.Services;
using System.Linq;
using System;
using MvvmDialogs.FrameworkDialogs.MessageBox;
using System.Windows;
using System.Threading.Tasks;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class SaveRequestViewModel : ObservableValidator, IModalDialogViewModel
    {
        private readonly IDialogService dialogService;
        private readonly CollectionsService collectionsService;

        public SaveRequestViewModel()
        {
            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();

            this.collectionsService = Ioc.Default.GetRequiredService<CollectionsService>();
            this.Collections = this.collectionsService.Collections;
        }

        private bool? dialogResult;
        public bool? DialogResult
        {
            get => this.dialogResult;
            private set => SetProperty(ref dialogResult, value);
        }
        public ObservableCollection<CollectionModel> Collections { get; set; }

        private CollectionModel collection;
        [Required]
        public CollectionModel Collection
        {
            get => collection;
            set => SetProperty(ref collection, value);
        }

        private string name;
        [Required]
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
                OkCommand.NotifyCanExecuteChanged();
            }
        }

        private RelayCommand okCommand;

        public RelayCommand OkCommand => okCommand ??= new RelayCommand(Ok, CanExecuteOk);

        private void Ok()
        {
            if (this.HasErrors)
            {
                return;
            }

            if (Collection.Files.Any(x => x.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)))
            {
                var settings = new MessageBoxSettings()
                {
                    Caption = "Error",
                    MessageBoxText = $"A request with the name '{ Name }' already exists. Please select another name for the request.",
                    Icon = MessageBoxImage.Error,
                    Button = MessageBoxButton.OK
                };
                this.dialogService.ShowMessageBox(this, settings);
                return;
            }

            DialogResult = true;
        }

        private bool CanExecuteOk()
        {
            this.ValidateAllProperties();

            return !this.HasErrors;
        }

        private AsyncRelayCommand createNewCollectioCommand;
        public AsyncRelayCommand CreateNewCollectionCommand => createNewCollectioCommand ??= new(CreateNewCollection);

        private async Task CreateNewCollection()
        {
            var vm = new AddCollectionViewModel(this.dialogService);

            if (dialogService.ShowDialog(this, vm) == true)
            {
                await this.collectionsService.CreateCollectionAsync(vm.CollectionName, vm.Location);
            }
        }

    }
}
