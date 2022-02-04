using HP.MVVM;
using HP.Palette.Security;
using System;
using System.IO;
using System.Text;
using Windows.UI.Core;

namespace HP.SecureStorage {
    public class MainPageView : ViewModelBase {
        public MainPageView() {
            storage = new Storage("HP.SecureStorage");
        }

        private readonly Storage storage = default;

        public override void Initialize() {
            base.Initialize();

            //this gives UI time tp draw itself before calling - just coding nonsense
            var tmr = new System.Threading.Timer(new System.Threading.TimerCallback(tmrCallBack));
            tmr.Change(Convert.ToInt32(TimeSpan.FromMilliseconds(250).TotalMilliseconds), 0);
        }

        private async void tmrCallBack(object state) {
            var t = (System.Threading.Timer)state;
            t.Dispose();

            //because this is a different thread need to marshal back to the UI thread
            var dispatcher = Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                // remote returns a phony key until primary strategy is in place
                
                // if remote value is missing, check backup
                var checkBackup = true;
                // if remote exists and backup is different than remote, update backup as safety
                var updateBackup = true;
                BingMapsAuthKey = storage.GetValue("BingMapsKey", default(string), checkBackup, updateBackup);
            });
            
        }

        #region BingMapsAuthKey Property
        private string _BingMapsAutKey = default;
        public string BingMapsAuthKey {
            get => _BingMapsAutKey;
            set {
                _BingMapsAutKey = value;
                InvokePropertyChanged(nameof(BingMapsAuthKey));
            }
        }
        #endregion

        #region Name Property
        private string _Name = default;
        public string Name {
            get => _Name;
            set {
                _Name = value;
                InvokePropertyChanged(nameof(Name));
            }
        }
        #endregion

        #region Value Property
        private string _Value = default;
        public string Value {
            get => _Value;
            set {
                _Value = value;
                InvokePropertyChanged(nameof(Value));
            }
        }
        #endregion

        #region SetCommand
        private DelegateCommand _SetCommand = default;
        public DelegateCommand SetCommand => _SetCommand ?? (_SetCommand = new DelegateCommand(Set, ValidateSetState));
        private bool ValidateSetState(object state) => true;
        private void Set(object state) {
            try {
                if (IsString) {
                    storage.SetBackupValue(Name, ValueExtensions.CastValue<string>(Value));
                }
                else if (IsLong) {
                    storage.SetBackupValue(Name, ValueExtensions.CastValue<long>(Value));
                }
                else if (IsDouble) {
                    storage.SetBackupValue(Name, ValueExtensions.CastValue<double>(Value));
                }
                else if (IsBoolean) {
                    storage.SetBackupValue(Name, ValueExtensions.CastValue<bool>(Value));
                }
                else if (IsCustom) {
                    //this is just for testing custom values
                    storage.SetBackupValue(Name, ValueExtensions.CastValue<CustomValue>(Value));
                }
            }
            catch (Exception ex) {
                SetError(ex);
            }
        }
        #endregion

        private void SetError(Exception ex) {
            var res = new StringBuilder();
            var exx = ex;
            var tab = 0;
            while (exx != null) {
                using (var sr = new StringReader(exx.ToString())) {
                    while (sr.Peek() > -1) {
                        res.AppendLine(new string(' ', tab * 4) + sr.ReadLine());
                    }
                }
                exx = exx.InnerException;
                tab++;
            }
            ErrorText = res.ToString();
        }

        #region GetCommand
        private DelegateCommand _GetCommand = default;
        public DelegateCommand GetCommand => _GetCommand ?? (_GetCommand = new DelegateCommand(Get, ValidateGetState));
        private bool ValidateGetState(object state) => true;
        private async void Get(object state) {
            Value = string.Empty;
            ErrorText = string.Empty;
            try {
                if (IsString) {
                    Value = storage.GetBackupValue(Name, default(string));
                }
                else {
                    var val = default(object);
                    if (IsLong) {
                        val = storage.GetBackupValue(Name, default(long));
                    }
                    else if (IsDouble) {
                        val = storage.GetBackupValue(Name, default(double));
                    }
                    else if (IsBoolean) {
                        val = storage.GetBackupValue(Name, default(bool));
                    }
                    else if (IsCustom) {
                        val = storage.GetBackupValue(Name, default(CustomValue));
                    }
                    Value = val.ToString();
                }
            }
            catch (Exception ex) {
                SetError(ex);
            }
        }
        #endregion

        #region IsString Property
        private bool _IsString = default;
        public bool IsString {
            get => _IsString;
            set {
                _IsString = value;
                InvokePropertyChanged(nameof(IsString));
            }
        }
        #endregion

        #region IsLong Property
        private bool _IsLong = default;
        public bool IsLong {
            get => _IsLong;
            set {
                _IsLong = value;
                InvokePropertyChanged(nameof(IsLong));
            }
        }
        #endregion

        #region IsDouble Property
        private bool _IsDouble = default;
        public bool IsDouble {
            get => _IsDouble;
            set {
                _IsDouble = value;
                InvokePropertyChanged(nameof(IsDouble));
            }
        }
        #endregion

        #region IsBoolean Property
        private bool _IsBoolean = default;
        public bool IsBoolean {
            get => _IsBoolean;
            set {
                _IsBoolean = value;
                InvokePropertyChanged(nameof(IsBoolean));
            }
        }
        #endregion

        #region IsCustom Property
        private bool _IsCustom = default;
        public bool IsCustom {
            get => _IsCustom;
            set {
                _IsCustom = value;
                InvokePropertyChanged(nameof(IsCustom));
            }
        }
        #endregion

        #region ErrorText Property
        private string _ErrorText = default;
        public string ErrorText {
            get => _ErrorText;
            set {
                _ErrorText = value;
                InvokePropertyChanged(nameof(ErrorText));
            }
        }
        #endregion
    }
}
