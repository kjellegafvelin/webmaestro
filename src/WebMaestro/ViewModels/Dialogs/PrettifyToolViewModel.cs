using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Xml;

namespace WebMaestro.ViewModels.Dialogs
{
    internal enum PrettifyTypes
    {
        Json,
        XML
    }

    internal partial class PrettifyToolViewModel : ObservableObject, IModalDialogViewModel
    {
        [ObservableProperty]
        private bool? dialogResult;

        [ObservableProperty]
        private string source;

        [ObservableProperty]
        private string target;

        [ObservableProperty]
        private PrettifyTypes prettifyType;

        [RelayCommand]
        private void Prettify()
        {
            if (this.PrettifyType == PrettifyTypes.XML)
            {
                    try
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.LoadXml(this.Source);

                        using (var sw = new StringWriter())
                        using (var xw = new XmlTextWriter(sw)
                        {
                            Formatting = System.Xml.Formatting.Indented
                        })
                        {
                            xmlDoc.WriteContentTo(xw);
                            xw.Flush();
                            this.Target = sw.ToString();
                        }
                    }
                    catch { }
            }
            else if (this.PrettifyType == PrettifyTypes.Json)
            {
                    try
                    {
                        var jsonObj = JsonSerializer.Deserialize<object>(this.Source);

                        this.Target = JsonSerializer.Serialize<object>(jsonObj, new JsonSerializerOptions() { WriteIndented = true });
                    }
                    catch { }
            }
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
