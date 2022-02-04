using HP.Palette.Security.Exceptions;
using System.IO;
using System.Reflection;

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
            var dataPaths = Windows.Storage.AppDataPaths.GetDefault();
            var programDataPath = dataPaths.RoamingAppData;
            var path = Path.Combine(programDataPath, Assembly.GetExecutingAssembly().GetName().Name);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            DataFileName = Path.Combine(path, ".securebin");
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
        /// <summary>
        /// Gets the name of the data file.
        /// </summary>
        /// <value>
        /// The name of the data file.
        /// </value>

        public string DataFileName { get; private set; } = default;
        /// <summary>
        /// Gets the maximum length of the value.
        /// </summary>
        /// <value>
        /// The maximum length of the value.
        /// </value>
       
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
        public object GetValue(string valueName, object defaultValue) {
            var crypto = new Crypto();
            return crypto.ReadFromLocalDataFile(DataFileName, valueName);
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="valueName">Name of the value.</param>
        /// <returns>Value of T</returns>
        //public T GetValue<T>(string valueName) {
        //    return GetValue(valueName, default(T));
        //}

        public object GetValue(string valueName) {
            return GetValue(valueName, default);
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
        public void SetValue(string valueName, object value) {
            var crypto = new Crypto();
            crypto.WriteToLocalDataFile(DataFileName, valueName, value);
        }

        /// <summary>
        /// Backups the value exists.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <returns></returns>
        /// <exception cref="HP.Palette.Security.Exceptions.StorageException">Removal of the value named {valueName} failed. (see inner exception for reason)</exception>
        public bool ValueExists(string valueName) {
            var crypto = new Crypto();
            return crypto.HasValue(DataFileName, valueName);
        }

        /// <summary>
        /// Removes the backup value.
        /// </summary>
        /// <param name="valueName">Name of the value.</param>
        /// <exception cref="HP.Palette.Security.Exceptions.StorageException">Removal of the value named {valueName} failed. (see inner exception for reason)</exception>
        public void RemoveValue(string valueName) {
            var crypto = new Crypto();
            crypto.RemoveValue(DataFileName, valueName);
        }
    }
}
