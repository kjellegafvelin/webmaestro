using CommunityToolkit.Mvvm.DependencyInjection;
using ControlzEx.Theming;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WebMaestro.Models;
using WebMaestro.Services;
using WebMaestro.ViewModels;
using WebMaestro.Views;

namespace WebMaestro
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            //ThemeManager.Current.ChangeTheme(Application.Current, "Light.Blue");

            //ThemeManager.Current.ThemeSyncMode = ThemeSyncMode.SyncWithAppMode;
            //ThemeManager.Current.SyncTheme();

            ViewModelLocator.Initialize();

            Settings.Load();
            
            try
            {
                // Load application state BEFORE creating the window
                var fileService = Ioc.Default.GetRequiredService<FileService>();
                var appState = await fileService.LoadAppStateAsync();
                
                // Validate window bounds to ensure window appears on visible screen
                ValidateWindowBounds(appState);
                
                // Create MainWindow
                var mainWindow = new MainWindow();
                var vm = (MainViewModel)mainWindow.DataContext;
                
                // Pass the loaded state to the view model
                vm.AppState = appState;
                
                // Apply window state from loaded settings BEFORE showing
                ApplyWindowState(mainWindow, appState);
                
                // Show the window
                mainWindow.Show();
                
                // Load tabs asynchronously AFTER window is shown (non-blocking)
                await vm.LoadOpenTabsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to load application state: {ex.Message}\n\nThe application will start with default settings.",
                    "Startup Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                
                // Fallback: show window with default state
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            
            base.OnStartup(e);
        }

        private void ValidateWindowBounds(AppStateModel appState)
        {
            // Ensure window dimensions are reasonable
            if (appState.MainWindowWidth < 800)
                appState.MainWindowWidth = 1280;
            
            if (appState.MainWindowHeight < 600)
                appState.MainWindowHeight = 1024;
            
            // Ensure window appears on a visible screen
            var screenWidth = SystemParameters.VirtualScreenWidth;
            var screenHeight = SystemParameters.VirtualScreenHeight;
            
            if (appState.MainWindowLeft < 0 || appState.MainWindowLeft > screenWidth - 100)
                appState.MainWindowLeft = 100;
            
            if (appState.MainWindowTop < 0 || appState.MainWindowTop > screenHeight - 100)
                appState.MainWindowTop = 100;
        }

        private void ApplyWindowState(MainWindow window, AppStateModel appState)
        {
            if (appState.MainWindowState == WindowState.Minimized)
            {
                // Don't start minimized, use Normal instead
                window.WindowState = WindowState.Normal;
            }
            else
            {
                window.WindowState = appState.MainWindowState;
            }
            
            // Only apply position and size if not maximized
            if (appState.MainWindowState != WindowState.Maximized)
            {
                window.Left = appState.MainWindowLeft;
                window.Top = appState.MainWindowTop;
                window.Width = appState.MainWindowWidth;
                window.Height = appState.MainWindowHeight;
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // OnExit is called after all windows are closed
            // At this point, we can only do synchronous cleanup
            Settings.Save();
            base.OnExit(e);
        }
    }
}
