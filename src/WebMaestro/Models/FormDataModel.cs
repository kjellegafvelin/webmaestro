using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace WebMaestro.Models
{
    public class FormDataModel : ObservableObject, IEditableObject
    {
        private string oldName;
        private string oldValue;
        private string oldFilePath;
        private string name;
        private string value;
        private string filePath;
        private bool isEnabled = true;

        public FormDataModel() { }

        public FormDataModel(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public FormDataModel(string name, string value, bool isEnabled) : this(name, value)
        {
            this.isEnabled = isEnabled;
        }

        public string Name
        {
            get => name;
            set => this.SetProperty(ref this.name, value);
        }

        public string Value
        {
            get => value;
            set => this.SetProperty(ref this.value, value);
        }

        public string FilePath
        {
            get => filePath;
            set => this.SetProperty(ref this.filePath, value);
        }

        [JsonIgnore]
        public bool IsFile => !string.IsNullOrEmpty(this.FilePath);

        public bool IsEnabled
        {
            get => isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        public void BeginEdit()
        {
            oldName ??= Name;
            oldValue ??= Value;
            oldFilePath ??= FilePath;
        }

        public void CancelEdit()
        {
            Name = oldName;
            Value = oldValue;
            FilePath = oldFilePath;
        }

        public void EndEdit()
        {
            oldName = null;
            oldValue = null;
            oldFilePath = null;
        }
    }
}
