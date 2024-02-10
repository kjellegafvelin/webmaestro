using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using WebMaestro.Core;

namespace WebMaestro.ViewModels
{
    public abstract class TabItemViewModel : ObservableObject
    {
        private string name;

        public string Name
        {
            get => this.name;
            set => SetProperty(ref this.name, value);
        }
        public Guid Id { get; internal set; }

        public ModificationObserver Observer { get; protected set; }

        public async Task Save()
        {
            if (this.Observer?.IsModified == true && await OnSave())
            {
                Observer?.Reset();
            }
        }

        public abstract Task<bool> OnSave();
     
    }
}
