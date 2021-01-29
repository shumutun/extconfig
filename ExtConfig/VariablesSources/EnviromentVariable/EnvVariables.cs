using System;

namespace ExtConfig.VariablesSources.EnviromentVariable
{
    public class EnvVariables : IConfigVariables
    {
        public string GetEnviromentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? throw new ArgumentException($"Enviroment variabel {name} not found");
        }
    }
}
