using System;

namespace ExtConfig.VariablesSources.EnvironmentVariable
{
    public class EnvVariables : IConfigVariables
    {
        public string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? throw new ArgumentException($"Environment variable {name} not found");
        }
    }
}
