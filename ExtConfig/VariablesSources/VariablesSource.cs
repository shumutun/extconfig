using ExtConfig.VariablesSources.AsureKeyVault;
using ExtConfig.VariablesSources.EnviromentVariable;
using System;

namespace ExtConfig.VariablesSources
{
    public static class VariablesSource
    {
        private const string _enviromentvariables = "enviromentvariables";
        private const string _asurekeyvault = "asurekeyvault:";


        public static IConfigVariables GetConfigVariables(string variablesSource)
        {
            if (variablesSource == null || variablesSource.ToLower() == _enviromentvariables)
                return new EnvVariables();

            if (variablesSource.ToLower().StartsWith(_asurekeyvault))
                if (Uri.TryCreate(variablesSource.Substring(_asurekeyvault.Length), UriKind.Absolute, out Uri akvUri))
                    return new AsureKeyVaultVariables(akvUri);

            throw new NotImplementedException();
        }
    }
}
