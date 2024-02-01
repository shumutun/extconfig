namespace ExtConfig.VariablesSources
{
    public interface IConfigVariables
    {
        string GetEnvironmentVariable(string name);
    }
}
