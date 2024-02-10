using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace WebMaestro.Models
{
    public abstract class ModelBase : ObservableObject
    {
        private bool isDirty;

        [JsonIgnore]
        public bool IsDirty
        {
            get => this.isDirty;
            internal set => this.SetProperty(ref this.isDirty, value);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(IsDirty), System.StringComparison.Ordinal))
            {
                return;
            }

            this.IsDirty = true;
            base.OnPropertyChanged(e);
        }
    }
}
