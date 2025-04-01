using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using WebMaestro.Core;

namespace WebMaestro.ViewModels
{
    public abstract partial class TabItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string filename;

        [ObservableProperty]
        private string tooltip;

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
