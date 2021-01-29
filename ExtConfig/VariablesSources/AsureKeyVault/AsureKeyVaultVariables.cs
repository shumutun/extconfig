using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System;

namespace ExtConfig.VariablesSources.AsureKeyVault
{
    public class AsureKeyVaultVariables : IConfigVariables
    {
        private readonly SecretClient _secretClient;
        public AsureKeyVaultVariables(Uri azureKeyVaultUrl)
        {
            _secretClient = new SecretClient(azureKeyVaultUrl, new DefaultAzureCredential());
        }

        public string GetEnviromentVariable(string name)
        {
            var secret = _secretClient.GetSecret(name);
            return secret.Value.Value;
        }
    }
}
