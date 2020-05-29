using System;

namespace ExtConfig.EnviromentVariables
{
    public class EnvVariables : IEnvVariables
    {
        public string GetEnviromentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name) ?? throw new ArgumentException($"Enviroment variabel {name} not found");
        }
    }
}
