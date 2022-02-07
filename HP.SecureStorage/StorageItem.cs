using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HP.SecureStorage {
    internal class StorageItem : INotifyPropertyChanged {
        public static List<StorageItem> LoadAll(string filename) {
            return null;
        }
        
        public static StorageItem Create(string name, Type type, object value) {
            return new StorageItem {
                Name = name,
                Type = type,
                Value = value
            };
        }

        private StorageItem() { }

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void InvokePropertyChanged([CallerMemberName] string propertyName = default) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Name Property
        private string _Name = default;
        public string Name {
            get => _Name;
            set {
                _Name = value;
                InvokePropertyChanged();
            }
        }
        #endregion

        #region Type Property
        private Type _Type = default;
        public Type Type {
            get => _Type;
            set {
                _Type = value;
                InvokePropertyChanged();
            }
        }
        #endregion

        #region Value Property
        private object _Value = default;
        public object Value {
            get => _Value;
            set {
                _Value = value;
                InvokePropertyChanged();
            }
        }
        #endregion
    }
}
