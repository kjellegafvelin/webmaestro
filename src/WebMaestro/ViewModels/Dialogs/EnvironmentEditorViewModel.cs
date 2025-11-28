using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.ObjectModel;
using WebMaestro.Models;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;
using System;

namespace WebMaestro.ViewModels.Dialogs
{
    internal partial class EnvironmentEditorViewModel : ObservableObject, IModalDialogViewModel
    {
        public EnvironmentEditorViewModel(ObservableCollection<EnvironmentModel> environments)
        {
            this.Environments = environments ?? new ObservableCollection<EnvironmentModel>();

            this.Environments.CollectionChanged += Environments_CollectionChanged;
            RegisterEnvironmentHandlers();
            // Select first environment if available
            if (this.Environments.Count > 0) { this.SelectedEnvironment = this.Environments[0]; }
            Validate();
        }

        private readonly Dictionary<VariableModel, string> variableNameCache = new();

        private void CacheVariable(VariableModel v)
        {
            if (!variableNameCache.ContainsKey(v))
            {
                variableNameCache[v] = v.Name;
            }
        }

        private void RemoveCachedVariable(VariableModel v)
        {
            _ = variableNameCache.Remove(v);
        }

        [ObservableProperty]
        private bool? dialogResult;

        public ObservableCollection<EnvironmentModel> Environments { get; }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(CloneEnvironmentCommand))]
        [NotifyCanExecuteChangedFor(nameof(RemoveEnvironmentCommand))]
        [NotifyCanExecuteChangedFor(nameof(AddVariableCommand))]
        [NotifyCanExecuteChangedFor(nameof(RemoveVariableCommand))]
        private EnvironmentModel? selectedEnvironment;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(RemoveVariableCommand))]
        private VariableModel? selectedVariable;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsValidationMessageVisible))]
        [NotifyCanExecuteChangedFor(nameof(OkCommand))]
        private string? validationMessage;

        public bool IsValidationMessageVisible => !string.IsNullOrEmpty(ValidationMessage);

        [RelayCommand]
        private void AddEnvironment()
        {
            var env = new EnvironmentModel() { Name = "New Environment" };
            var source = Environments.FirstOrDefault(e => e.Variables != null && e.Variables.Count > 0);
            env.Variables = source != null
                ? new ObservableCollection<VariableModel>(source.Variables.Select(v => new VariableModel(v.Name, string.Empty, v.Description)))
                : new ObservableCollection<VariableModel>();
            Environments.Add(env);
            SelectedEnvironment = env;
            RegisterHandlersForEnvironment(env);
            foreach (var v in env.Variables) CacheVariable(v);
            Validate();
        }

        [RelayCommand(CanExecute = nameof(CanCloneEnvironment))]
        private void CloneEnvironment()
        {
            if (SelectedEnvironment == null) return;
            var src = SelectedEnvironment;
            var clone = new EnvironmentModel()
            {
                Name = GetUniqueEnvironmentName(src.Name + " - Copy"),
                Variables = new ObservableCollection<VariableModel>(src.Variables.Select(v => new VariableModel(v.Name, v.Value, v.Description)))
            };
            Environments.Add(clone);
            SelectedEnvironment = clone;
            RegisterHandlersForEnvironment(clone);
            foreach (var v in clone.Variables) CacheVariable(v);
            Validate();
        }

        private bool CanCloneEnvironment() => SelectedEnvironment != null;

        private string GetUniqueEnvironmentName(string baseName)
        {
            var existing = new HashSet<string>(Environments.Select(e => e.Name), StringComparer.OrdinalIgnoreCase);
            var name = baseName; var idx = 1;
            while (existing.Contains(name)) { name = baseName + " (" + idx++ + ")"; }
            return name;
        }

        [RelayCommand(CanExecute = nameof(CanRemoveEnvironment))]
        private void RemoveEnvironment()
        {
            if (SelectedEnvironment == null) return;
            foreach (var v in SelectedEnvironment.Variables) RemoveCachedVariable(v);
            UnregisterHandlersForEnvironment(SelectedEnvironment);
            Environments.Remove(SelectedEnvironment);
            SelectedEnvironment = Environments.FirstOrDefault();
            Validate();
        }

        private bool CanRemoveEnvironment() => SelectedEnvironment != null;

        [RelayCommand(CanExecute = nameof(CanAddVariable))]
        private void AddVariable()
        {
            if (!CanAddVariable()) return;
            var name = GetUniqueVariableName("newVar");
            foreach (var env in Environments)
            {
                var vm = new VariableModel(name, string.Empty, string.Empty);
                env.Variables.Add(vm);
                vm.PropertyChanged += VariableModel_PropertyChanged;
                CacheVariable(vm);
            }
            SelectedVariable = SelectedEnvironment!.Variables.FirstOrDefault(v => v.Name == name);
            Validate();
        }

        [RelayCommand(CanExecute = nameof(CanRemoveVariable))]
        private void RemoveVariable()
        {
            if (!CanRemoveVariable()) return;
            var nameToRemove = SelectedVariable!.Name;
            foreach (var env in Environments)
            {
                var toRemove = env.Variables.FirstOrDefault(v => v.Name == nameToRemove);
                if (toRemove != null)
                {
                    toRemove.PropertyChanged -= VariableModel_PropertyChanged;
                    RemoveCachedVariable(toRemove);
                    env.Variables.Remove(toRemove);
                }
            }
            SelectedVariable = SelectedEnvironment!.Variables.FirstOrDefault();
            Validate();
        }

        private string GetUniqueVariableName(string baseName)
        {
            var existing = new HashSet<string>(Environments.SelectMany(e => e.Variables).Select(v => v.Name), StringComparer.OrdinalIgnoreCase);
            var name = baseName; var idx = 1;
            while (existing.Contains(name)) { name = baseName + idx++; }
            return name;
        }

        [RelayCommand(CanExecute = nameof(CanOk))]
        private void Ok()
        {
            Validate();
            if (!string.IsNullOrEmpty(ValidationMessage)) return;
            DialogResult = true;
        }

        private bool CanOk() => string.IsNullOrEmpty(ValidationMessage);

        [RelayCommand]
        private void Cancel() => DialogResult = false;

        private void Environments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EnvironmentModel env in e.NewItems)
                {
                    RegisterHandlersForEnvironment(env);
                    foreach (var v in env.Variables) CacheVariable(v);
                }
            }
            if (e.OldItems != null)
            {
                foreach (EnvironmentModel env in e.OldItems)
                {
                    foreach (var v in env.Variables) RemoveCachedVariable(v);
                    UnregisterHandlersForEnvironment(env);
                }
            }
            Validate();
        }

        private void RegisterEnvironmentHandlers()
        {
            foreach (var env in Environments) RegisterHandlersForEnvironment(env);
            foreach (var v in Environments.SelectMany(e => e.Variables)) CacheVariable(v);
        }

        private void RegisterHandlersForEnvironment(EnvironmentModel env)
        {
            if (env == null) return;
            env.Variables.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (VariableModel v in e.NewItems)
                    {
                        v.PropertyChanged += VariableModel_PropertyChanged;
                        CacheVariable(v);
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (VariableModel v in e.OldItems)
                    {
                        v.PropertyChanged -= VariableModel_PropertyChanged;
                        RemoveCachedVariable(v);
                    }
                }
                Validate();
            };
            foreach (var v in env.Variables)
            {
                v.PropertyChanged += VariableModel_PropertyChanged;
                CacheVariable(v);
            }
        }

        private void UnregisterHandlersForEnvironment(EnvironmentModel env)
        {
            if (env == null) return;
            foreach (var v in env.Variables)
            {
                v.PropertyChanged -= VariableModel_PropertyChanged;
                RemoveCachedVariable(v);
            }
        }

        private void VariableModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is VariableModel vm && e.PropertyName == nameof(VariableModel.Name))
            {
                if (variableNameCache.TryGetValue(vm, out var oldName))
                {
                    var newName = vm.Name;
                    if (!string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Propagate rename to all environments
                        foreach (var other in Environments.SelectMany(env => env.Variables))
                        {
                            if (!ReferenceEquals(other, vm) && other.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase))
                            {
                                other.Name = newName; // triggers its own PropertyChanged
                                variableNameCache[other] = newName;
                            }
                        }
                        variableNameCache[vm] = newName;
                    }
                }
            }
            Validate();
        }

        private void Validate()
        {
            foreach (var env in Environments)
            {
                var dup = env.Variables.GroupBy(v => v.Name, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1);
                if (dup != null)
                {
                    ValidationMessage = $"Duplicate variable name '{dup.Key}' found in environment '{env.Name}'.";
                    return;
                }
            }
            ValidationMessage = string.Empty;
        }

        private bool CanAddVariable() => SelectedEnvironment != null;
        private bool CanRemoveVariable() => SelectedEnvironment != null && SelectedVariable != null;
    }
}
