using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace WebMaestro.Models
{
    internal partial class AppStateModel : ObservableObject
    {
        [ObservableProperty]
        private int mainWindowTop;

        [ObservableProperty]
        private int mainWindowLeft;

        [ObservableProperty]
        private int mainWindowWidth = 1280;

        [ObservableProperty]
        private int mainWindowHeight = 1024;

        [ObservableProperty]
        private WindowState mainWindowState = WindowState.Maximized;

    }
}
