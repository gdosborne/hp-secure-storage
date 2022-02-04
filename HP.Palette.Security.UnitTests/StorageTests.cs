
using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HP.Palette.Security.UnitTests {
    [TestClass]
    public class StorageTests {

        private const string TestAppName = "StorageUnitTests";
        private const string KeyBase = "MapKey";
        private const string AuthKey = "19DB5EED-3B4D-4033-8B94-7FA003CF9A8C";
        private const int MaxLength = 255;

        private Storage GetStorage() => new Storage(TestAppName);
        private Storage GetStorage(int maxLength) => new Storage(TestAppName, maxLength);

        private void CleanUp(params string[] keys) {
            foreach (var item in keys) {
                GetStorage().RemoveBackupValueAsync(item);
            }
        }

        private string RandomValue(int length) {
            var r = new Random();
            var result = new byte[length];
            for (var i = 0; i < length; i++) {
                var isEven = r.Next(1, 3) == 2; //1 or 2 - choose either number or letter
                var c = isEven ? r.Next(48, 57) : r.Next(97, 122);
                result[i] = (byte)(char)c;
            }
            return Encoding.ASCII.GetString(result);
        }

        [TestMethod]
        public void ApplicationNameTest() {
            var store = GetStorage();
            Assert.IsTrue(store.ApplicationName == TestAppName);
        }

        [TestMethod]
        public void AddBackupValueWhenRemoteExists_Test() {
            var store = GetStorage();
            var result = false;
            var name = $"{KeyBase}1";
            var key = default(string);
            try {
                key = store.GetValue(name, default(string), false, true);
            }
            catch { }
            try {
                var backupValue = store.GetBackupValue(name, default(string));
                result = key == backupValue;
            }
            catch {
                //backup value read failed
                store.SetBackupValue(name, key);
                var backupValue = store.GetBackupValue(name, default(string));
                result = key == backupValue;
            }
            finally {
                CleanUp(name);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void BackupValueIsMissing_Test() {
            var store = GetStorage();
            var result = true;
            var name = $"{KeyBase}4";
            try {
                result = store.BackupValueExists(name);
            }
            catch { result = false; }
            finally {
                CleanUp(name);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void BackupValueExists_Test() {
            var store = GetStorage();
            var result = true;
            var name = $"{KeyBase}5";
            try {
                store.SetBackupValue(name, AuthKey);
                result = store.BackupValueExists(name);
            }
            catch { result = false; }
            finally {
                CleanUp(name);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void SetBackupValue_Test() {
            var store = GetStorage();
            var result = true;
            var name = $"{KeyBase}6";
            try {
                store.SetBackupValue(name, AuthKey);
                var key = store.GetBackupValue(name, default(string));
                result = key == AuthKey;
            }
            catch { result = false; }
            finally {
                CleanUp(name);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void SetBackupInvalidLength_Test() {
            var store = GetStorage(MaxLength);
            var result = true;
            var name = $"{KeyBase}7";
            try {
                var value = RandomValue(store.MaximumValueLength + 1);
                store.SetBackupValue(name, value);
            }
            catch { result = false; }
            finally {
                CleanUp(name);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void SetBackupValidLength_Test() {
            var store = GetStorage(MaxLength);
            var result = true;
            var name = $"{KeyBase}8";
            try {
                var value = RandomValue(store.MaximumValueLength);
                store.SetBackupValue(name, value);
                var val = store.GetBackupValue(name, default(string));
                result = val == value;
            }
            catch { result = false; }
            finally {
                CleanUp(name);
                Assert.IsTrue(result);
            }
        }
    }
}
