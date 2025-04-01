using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using MvvmDialogs;
using WebMaestro.Models;
using WebMaestro.Services;
using WebMaestro.ViewModels;
using WebMaestro.ViewModels.Dialogs;
using WebMaestro.ViewModels.Explorer;

namespace WebMaestro
{
    internal class ViewModelLocator
    {
        public ViewModelLocator()
        {
            //var mainVM = new MainViewModel();
            var options = OptionsModel.Create();
            
            var sp = new ServiceCollection()
                .AddSingleton<MainViewModel>()
                //.AddSingleton(mainVM)
                .AddSingleton<ExplorerViewModel>()
                .AddSingleton<IDialogService, DialogService>()
                .AddSingleton<CollectionsService>()
                .AddSingleton<FileService>()
                .AddSingleton(options)

                .AddTransient<AddCollectionViewModel>()
                .AddTransient<AddHeaderViewModel>()
                .AddTransient<AddCookieViewModel>()

                .BuildServiceProvider();

            Ioc.Default.ConfigureServices(sp);

        }

        public ExplorerViewModel ExplorerControl => Ioc.Default.GetService<ExplorerViewModel>();
        
        public MainViewModel MainView => Ioc.Default.GetService<MainViewModel>();
    }
}
