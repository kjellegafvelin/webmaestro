using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;

namespace WebMaestro.Core
{
    public class ModificationObserver : ObservableObject
    {
        public static ModificationObserver Create(INotifyPropertyChanged observable, bool isModified = false)
        {
            var observer = new ModificationObserver();

            observable.PropertyChanged += observer.Observable_PropertyChanged;

            observer.AttachProperties(observable);

            // Allows setting a dirty state from the beginning
            if (isModified)
            {
                observer.IsModified = true;
            }

            return observer;
        }

        private void AttachProperties(INotifyPropertyChanged obj)
        {
            var props = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);

            foreach (var prop in props)
            {
                var propType = prop.PropertyType.GetInterface(nameof(INotifyPropertyChanged));
                if (propType is null)
                {
                    continue;
                }

                if (prop.GetValue(obj) is INotifyCollectionChanged propCollection)
                {
                    propCollection.CollectionChanged += PropCollection_CollectionChanged;
                    foreach (var item in propCollection as IEnumerable)
                    {
                        if (item is INotifyPropertyChanged observable)
                        {
                            observable.PropertyChanged += PropValue_PropertyChanged;
                            AttachProperties(observable);
                        }
                    }
                }
                else if (prop.GetValue(obj) is INotifyPropertyChanged propValue)
                {
                    propValue.PropertyChanged += PropValue_PropertyChanged;
                    AttachProperties(propValue);
                }
            }
        }

        private void PropCollection_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            this.IsModified = true;

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems!)
                {
                    if (item is INotifyPropertyChanged observable)
                    {
                        observable.PropertyChanged += PropValue_PropertyChanged;
                        AttachProperties(observable);
                    }
                }
            }
        }

        private void PropValue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.IsModified = true;
        }

        private void Observable_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            this.IsModified = true;
        }

        private bool isModified;
        public bool IsModified
        {
            get => this.isModified;
            private set => this.SetProperty(ref this.isModified, value);
        }

        public void Reset()
        {
            this.IsModified = false;
        }
    }
}
