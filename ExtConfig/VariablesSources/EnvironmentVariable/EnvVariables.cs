using System;

namespace ExtConfig.VariablesSources.EnvironmentVariable
{
    public class EnvVariables : IConfigVariables
    {
        public bool TryGetEnvironmentVariable(string name, out string? value)
        {
            value = Environment.GetEnvironmentVariable(name);
            return value is not null;
        }
    }
}
