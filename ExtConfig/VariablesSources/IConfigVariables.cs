namespace ExtConfig.VariablesSources
{
    public interface IConfigVariables
    {
        bool TryGetEnvironmentVariable(string name, out string? value);
    }
}
