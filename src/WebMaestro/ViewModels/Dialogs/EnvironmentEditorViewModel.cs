using CommunityToolkit.Mvvm.ComponentModel;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebMaestro.ViewModels.Dialogs
{
    internal class EnvironmentEditorViewModel : ObservableObject, IModalDialogViewModel
    {
        public EnvironmentEditorViewModel()
        {
                
        }

        public bool? DialogResult { get; }
    }
}
