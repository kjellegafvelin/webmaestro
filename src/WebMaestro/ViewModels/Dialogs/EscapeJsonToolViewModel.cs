using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Text;
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
                //// Escape JSON by first deserializing to validate and then serializing without indentation
                //var jsonObj = JsonSerializer.Deserialize<object>(this.Source);
                //if (jsonObj != null)
                //{
                //    var escapedJson = JsonSerializer.Serialize(jsonObj, new JsonSerializerOptions() { WriteIndented = false });
                
                //    // Replace double quotes with escaped double quotes
                //    this.Target = escapedJson.Replace("\"", "\\\""); 
                //}
                this.Target = EscapeJsonString(this.Source);
            }
            catch { }
        }

        private static string EscapeJsonString(string input)
        {
            if (input == null)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (char c in input)
            {
                switch (c)
                {
                    case '\"':
                        sb.Append("\\\"");
                        break;
                    case '\\':
                        sb.Append("\\\\");
                        break;
                    case '/':
                        sb.Append("\\/");
                        break;
                    case '\b':
                        sb.Append("\\b");
                        break;
                    case '\f':
                        sb.Append("\\f");
                        break;
                    case '\n':
                        sb.Append("\\n");
                        break;
                    case '\r':
                        sb.Append("\\r");
                        break;
                    case '\t':
                        sb.Append("\\t");
                        break;
                    default:
                        if (char.IsControl(c))
                        {
                            sb.AppendFormat("\\u{0:X4}", (int)c); // Unicode escaping for other control characters
                        }
                        else
                        {
                            sb.Append(c);
                        }
                        break;
                }
            }

            return sb.ToString();
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