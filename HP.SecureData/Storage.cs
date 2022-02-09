using HP.Palette.Security.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace HP.Palette.Security {
    /// <summary>
    /// Secure Storage class
    /// </summary>
    public class Storage {
        /// <summary>
        /// Initializes a new instance of the <see cref="Storage"/> class.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        public Storage(string applicationName) {
            ApplicationName = applicationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Storage"/> class.
        /// </summary>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="maxLength">The maximum length.</param>
        public Storage(string applicationName, int maxLength)
            : this(applicationName) {
            MaximumValueLength = maxLength;
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <value>
        /// The name of the application.
        /// </value>
        public string ApplicationName { get; private set; } = default;

        public int MaximumValueLength { get; private set; } = 255;
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="tryBackupOnFailure">if set to <c>true</c> attempts to get key from backup source.</param>
        /// <param name="addToBackupIfExists">if set to <c>true</c> will validate and update the backup source if necessary.</param>
        /// <returns>Value of T</returns>
        public T GetValue<T>(string valueName, T defaultValue, bool tryBackupOnFailure, bool addToBackupIfExists) {

            //you can remove this and the APIKeyService.cs file
            // and replace it with a call to the AzureKeyVault.GetBingKey method to get the Bing Key
            var svc = new APIKeyService();
            var val = svc.GetRemoteKey<T>(valueName);
            //===============================================================+
            
            //if we don't get a value then look for a backup value in the Password Vault locally
            if (val == null) {
                if (tryBackupOnFailure && BackupValueExists(valueName)) {
                    val = GetBackupValue(valueName, defaultValue);
                }
            }
            else {
                //save the current azure value locally so we will have a backup
                if (addToBackupIfExists) {
                    var test = BackupValueExists(valueName)
                        ? GetBackupValue(valueName, default(T))
                        : (T)(object)null;
                    if (test != null && !test.Equals(val)) {
                        //remote value has been updated, so update backup
                        SetBackupValue(valueName, val);
                    }
                }
            }
            return val;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName">Name of the value.</param>
        /// <returns>Value of T</returns>
        public T GetValue<T>(string valueName) {
            return GetValue(valueName, default(T));
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="tryBackupOnFailure">if set to <c>true</c> [try backup on failure].</param>
        /// <returns>Value of T</returns>
        public T GetValue<T>(string valueName, T defaultValue, bool tryBackupOnFailure) {
            return GetValue(valueName, defaultValue, tryBackupOnFailure, false);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Value of T</returns>
        public T GetValue<T>(string valueName, T defaultValue) {
            return GetValue(valueName, defaultValue, true);
        }

        /// <summary>
        /// Sets the backup value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        /// <exception cref="StorageException">
        /// $"{typeof(T).FullName} must contain a static Parse method.
        /// or
        /// $"Setting of {typeof(T).FullName} value named {valueName} failed. (see inner exception for reason), ex
        /// </exception>
        public void SetBackupValue<T>(string valueName, T value) {
            if (!typeof(T).TypeHasParseMethod()) {
                throw new StorageException($"{typeof(T).FullName} must contain a static Parse method.");
            }
            var passwordVault = new PasswordVault();
            var stringValue = value.ToString();
            if (stringValue.Length > MaximumValueLength) {
                throw new StorageException("The length of the value cannot exceed 255 characters");
            }
            try {
                var cred = new PasswordCredential {
                    Resource = ApplicationName,
                    UserName = valueName,
                    Password = stringValue
                };
                passwordVault.Add(cred);
            }
            catch (Exception ex) {
                throw new StorageException($"Setting of {typeof(T).FullName} value named {valueName} failed. (see inner exception for reason)", ex);
            }
            finally {
                passwordVault = null;
            }
        }


        /// <summary>
        /// Gets the backup value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName">Name of the value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        /// <exception cref="StorageException">
        /// $"{typeof(T).FullName} must contain a static Parse method.
        /// or
        /// $"Retrieval of {typeof(T).FullName} value named {valueName} failed. (see inner exception for reason), ex
        /// </exception>
        public T GetBackupValue<T>(string valueName, T defaultValue) {
            if (!typeof(T).TypeHasParseMethod()) {
                throw new StorageException($"{typeof(T).FullName} must contain a static Parse method.");
            }
            var result = defaultValue;
            var passwordVault = new PasswordVault();
            var creds = default(IReadOnlyList<PasswordCredential>);
            try {
                creds = passwordVault.FindAllByResource(ApplicationName);

            }
            catch {
                //nothing in the password vault for the application
                //leave result the default
            }
            try {
                var cred = creds.FirstOrDefault(x => x.UserName == valueName);
                if (cred == null) {
                    //leave result the default
                }
                else {
                    cred.RetrievePassword();
                    if (typeof(T) == typeof(string)) {
                        result = (T)(object)cred.Password;
                    }
                    else {
                        var method = typeof(T).GetStaticParseMethod();
                        if (method != null) {
                            var val = method.Invoke(null, new object[] { cred.Password });
                            result = (T)val;
                        }
                    }
                }
            }
            catch (Exception ex) {
                throw new StorageException($"Retrieval of {typeof(T).FullName} value named {valueName} failed. (see inner exception for reason)", ex);
            }
            finally {
                passwordVault = null;
            }
            return result;
        }

        /// <summary>
        /// Backups the value exists.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        /// <exception cref="HP.Palette.Security.Exceptions.StorageException">Removal of the value named {valueName} failed. (see inner exception for reason)</exception>
        public bool BackupValueExists(string valueName) {
            var passwordVault = new PasswordVault();
            var result = false;
            try {
                var creds = passwordVault.FindAllByResource(ApplicationName);
                var cred = creds.FirstOrDefault(x => x.UserName == valueName);
                result = cred != null;
            }
            catch (Exception ex) {
                result = false;
            }
            finally {
                passwordVault = null;
            }
            return result;
        }

        /// <summary>
        /// Removes the backup value.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <exception cref="HP.Palette.Security.Exceptions.StorageException">Removal of the value named {valueName} failed. (see inner exception for reason)</exception>
        public async void RemoveBackupValueAsync(string valueName) {
            await Task.Yield();
            var passwordVault = new PasswordVault();
            try {
                var creds = passwordVault.FindAllByResource(ApplicationName);
                var cred = creds.FirstOrDefault(x => x.UserName == valueName);
                if (cred == null) {
                    return;
                }
                passwordVault.Remove(cred);
            }
            catch { }
            finally {
                passwordVault = null;
            }
        }
    }
}
