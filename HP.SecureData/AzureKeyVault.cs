using Azure.Core;
using Azure.Security.KeyVault.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HP.Palette.Security {
    public class AzureKeyVault {
        public AzureKeyVault() {
            //IAzure azure = Azure.Authenticate("my.azureauth").WithDefaultSubscription();
            //var tc = TokenCredential
            //Client = new KeyClient()
        }

        private Uri vaultUri = new Uri(@"https://palettekeyvault.vault.azure.net/", UriKind.Absolute);

        public KeyClient Client { get; private set; } = default;

    }
}
