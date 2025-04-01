using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Text.Json;
using System.Windows;

namespace WebMaestro.ViewModels.Dialogs
{
    internal partial class EscapeJsonToolViewModel : ObservableObject, IModalDialogViewModel
    {
        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private string source = string.Empty;

        [ObservableProperty]
        private string target = string.Empty;

        [RelayCommand]
        private void Escape()
        {
            try
            {
                // Escape JSON by first deserializing to validate and then serializing without indentation
                var jsonObj = JsonSerializer.Deserialize<object>(this.Source);
                if (jsonObj != null)
                {
                    var escapedJson = JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions() { WriteIndented = false });
                
                    // Replace double quotes with escaped double quotes
                    this.Target = escapedJson.Replace("\"", "\\\"");
                }
            }
            catch { }
        }

        [RelayCommand()]
        private void Copy()
        {
            if (this.Target != null)
            {
                Clipboard.SetText(Target, TextDataFormat.UnicodeText);
            }
        }

        [RelayCommand]
        private void Exit()
        {
            this.DialogResult = true;
        }
    }
}