using CommunityToolkit.Mvvm.DependencyInjection;
using ControlzEx.Theming;
using System.Linq;
using System.Windows;
using WebMaestro.Services;
using WebMaestro.ViewModels;

namespace WebMaestro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");

            Settings.Load();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            MainViewModel? vm = Ioc.Default.GetService<MainViewModel>();
            FileService? fileService = Ioc.Default.GetService<FileService>();

            if (vm != null && fileService != null)
            {
                fileService.SaveAppState(vm.AppState);
                fileService.SaveOpenTabs(vm.ViewModels.ToList());
            }

            Settings.Save();
            base.OnExit(e);
        }
    }
}
