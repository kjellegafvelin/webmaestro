using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WebMaestro.Dialogs;
using WebMaestro.Models;
using WebMaestro.ViewModels.Dialogs;
using WebMaestro.Services;
using System.Linq;

namespace WebMaestro.ViewModels.Explorer
{
    internal sealed class EnvironmentsViewModel : ObservableObject
    {
        private readonly CollectionModel collectionModel;
        private readonly IDialogService dialogService;
        private readonly CollectionsService collectionsService;

        public EnvironmentsViewModel(CollectionModel collectionModel)
        {
            this.collectionModel = collectionModel;
            this.environmentModels = collectionModel.Environments;

            foreach (var environment in this.environmentModels)
            {
                this.Environments.Add(new EnvironmentModel()
                {
                    Name = environment.Name,
                    Variables = new ObservableCollection<VariableModel>(environment.Variables.Select(v => new VariableModel(v.Name, v.Value, v.Description)))
                });
            }

            this.dialogService = Ioc.Default.GetRequiredService<IDialogService>();
            this.collectionsService = Ioc.Default.GetRequiredService<CollectionsService>();

            this.AddCommand = new RelayCommand(Add);
            this.EditCommand = new RelayCommand(Edit);
        }

        private readonly ObservableCollection<EnvironmentModel> environmentModels;

        public ICommand AddCommand { get; }

        private void Add()
        {
            var env = new EnvironmentModel()
            {
                Name = "New Environment",
                Variables = new ObservableCollection<VariableModel>()
            };

            this.environmentModels.Add(env);
            this.Environments.Add(new EnvironmentModel()
            {
                Name = env.Name,
                Variables = new ObservableCollection<VariableModel>(env.Variables.Select(v => new VariableModel(v.Name, v.Value, v.Description)))
            });
        }

        public ICommand EditCommand { get; }

        public void Edit()
        {
            var explorer = Ioc.Default.GetRequiredService<ExplorerViewModel>();

            // Deep clone current environments to edit in dialog
            var editList = new ObservableCollection<EnvironmentModel>(this.environmentModels.Select(e => new EnvironmentModel()
            {
                Name = e.Name,
                Variables = new ObservableCollection<VariableModel>(e.Variables.Select(v => new VariableModel(v.Name, v.Value, v.Description)))
            }));

            var dlg = new EnvironmentEditorViewModel(editList);

            if (this.dialogService.ShowDialog(explorer, dlg) == true && dlg.DialogResult == true)
            {
                // Apply changes back to collection model and persist
                this.environmentModels.Clear();
                foreach (var e in dlg.Environments)
                {
                    this.environmentModels.Add(new EnvironmentModel()
                    {
                        Name = e.Name,
                        Variables = new ObservableCollection<VariableModel>(e.Variables.Select(v => new VariableModel(v.Name, v.Value, v.Description)))
                    });
                }

                // Save collection
                _ = this.collectionsService.SaveCollectionAsync(this.collectionModel);
            }
        }

        public ObservableCollection<EnvironmentModel> Environments { get; } = new();
    }
}
