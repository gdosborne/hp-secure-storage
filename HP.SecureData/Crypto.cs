using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace HP.Palette.Security {
    /// <summary>
    /// Cryptography
    /// </summary>
    public sealed class Crypto {
        /// <summary>
        /// Gets the application identifier.
        /// </summary>
        /// <value>
        /// The application identifier.
        /// </value>
        private string AppID {
            get {
                var desc = ((AssemblyDescriptionAttribute)Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(AssemblyDescriptionAttribute))).Description;
                return $"{Assembly.GetExecutingAssembly().GetName().Name}/{desc}";
            }
        }

        //NOTE - this file is only encrypted to keep prying eyes from
        //seeing this when looking at the file system - it is not truely
        //secure as anyone can reverse engineer the code just by
        //decompiling the executable

        //pros and cons over password vault
        //pros      stores unlimited number of items - pwv is 10 items per app
        //          user must know algorythm and password (PublicKeyToken of app)
        //cons      can be view from another users login if the above info is known

        /// <summary>
        /// Reads all from local data file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private Dictionary<string, string> ReadAllFromLocalDataFile(string fileName) {
            var result = new Dictionary<string, string>();
            if (File.Exists(fileName)) {
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None)) {
                    using (var reader = new BinaryReader(fs)) {
                        var data = reader.ReadBytes(Convert.ToInt32(fs.Length));
                        var decrypted = Decrypt(System.Text.Encoding.ASCII.GetString(data), AppID);
                        if (!string.IsNullOrEmpty(decrypted)) {
                            var doc = XDocument.Parse(decrypted);
                            var root = doc.Root;
                            root.Elements().ToList().ForEach(el => {
                                if (el.Name.LocalName.Equals("value") && el.Attribute("key") != null) {
                                    result.Add(el.Attribute("key").Value, el.Value);
                                }
                            });
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Writes all to local data file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="items">The items.</param>
        private void WriteAllToLocalDataFile(string fileName, Dictionary<string, string> items) {
            //if (File.Exists(fileName)) {
                var root = new XElement("values");
                items.ToList().ForEach(x => {
                    root.Add(new XElement("value", new XAttribute("key", x.Key), x.Value));
                });
                var doc = new XDocument(root);
                var enc = Encrypt(doc.ToString(), AppID);
                var data = System.Text.Encoding.ASCII.GetBytes(enc);
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)) {
                    using (var writer = new BinaryWriter(fs)) {
                        writer.Write(data);
                    }
                }
            //}
        }

        /// <summary>
        /// Determines whether the specified file name has value.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <returns>
        ///   <c>true</c> if the specified file name has value; otherwise, <c>false</c>.
        /// </returns>
        public bool HasValue(string fileName, string itemName) {
            var items = ReadAllFromLocalDataFile(fileName);
            return items.ContainsKey(itemName);
        }

        /// <summary>
        /// Removes the value.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="itemName">Name of the item.</param>
        public void RemoveValue(string fileName, string itemName) {
            var items = ReadAllFromLocalDataFile(fileName);
            if (items.ContainsKey(itemName)) {
                items.Remove(itemName);
                WriteAllToLocalDataFile(fileName, items);
            }
        }

        /// <summary>
        /// Reads from local data file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public string ReadFromLocalDataFile(string fileName, string itemName, string defaultValue = default) {
            var items = ReadAllFromLocalDataFile(fileName);
            if (items.ContainsKey(itemName)) {
                return items[itemName];
            }
            return defaultValue;
        }

        /// <summary>
        /// Writes to local data file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="value">The value.</param>
        public void WriteToLocalDataFile(string fileName, string itemName, object value) {
            var items = ReadAllFromLocalDataFile(fileName);
            if (items.ContainsKey(itemName)) {
                items[itemName] = value.ToString();
            }
            else {
                items.Add(itemName, value.ToString());
            }
            WriteAllToLocalDataFile(fileName, items);
        }

        /// <summary>
        /// Encrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="pass">The pass.</param>
        /// <returns></returns>
        public string Encrypt(string input, string pass) {
            var sap = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            var hap = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var aesHash = hap.CreateHash();
            try {
                var hash = new byte[32];
                aesHash.Append(CryptographicBuffer.CreateFromByteArray(System.Text.Encoding.UTF8.GetBytes(pass)));
                CryptographicBuffer.CopyToByteArray(aesHash.GetValueAndReset(), out var temp);

                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);

                var aes = sap.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(hash));

                var buffer = CryptographicBuffer.CreateFromByteArray(System.Text.Encoding.UTF8.GetBytes(input));
                var encrypted = CryptographicBuffer.EncodeToBase64String(CryptographicEngine.Encrypt(aes, buffer, null));

                return encrypted;
            }
            catch (Exception) {
                return null;
            }
        }

        /// <summary>
        /// Decrypts the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="pass">The pass.</param>
        /// <returns></returns>
        public string Decrypt(string input, string pass) {
            var sap = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesEcbPkcs7);
            var hap = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var aesHash = hap.CreateHash();
            try {
                var hash = new byte[32];
                aesHash.Append(CryptographicBuffer.CreateFromByteArray(System.Text.Encoding.UTF8.GetBytes(pass)));
                CryptographicBuffer.CopyToByteArray(aesHash.GetValueAndReset(), out var temp);

                Array.Copy(temp, 0, hash, 0, 16);
                Array.Copy(temp, 0, hash, 15, 16);

                var aes = sap.CreateSymmetricKey(CryptographicBuffer.CreateFromByteArray(hash));

                var buffer = CryptographicBuffer.DecodeFromBase64String(input);
                CryptographicBuffer.CopyToByteArray(CryptographicEngine.Decrypt(aes, buffer, null), out var Decrypted);
                var decrypted = System.Text.Encoding.UTF8.GetString(Decrypted, 0, Decrypted.Length);

                return decrypted;
            }
            catch (Exception) {
                return null;
            }
        }
    }
}
