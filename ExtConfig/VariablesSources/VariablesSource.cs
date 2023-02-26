using ExtConfig.VariablesSources.EnviromentVariable;
using System;

namespace ExtConfig.VariablesSources
{
    public static class VariablesSource
    {
        private const string _environmentVariables = "enviromentvariables";
        
        public static IConfigVariables GetConfigVariables(string variablesSource)
        {
            if (variablesSource == null || variablesSource.ToLower() == _environmentVariables)
                return new EnvVariables();
            throw new NotImplementedException();
        }
    }
}
