using ExtConfig.VariablesSources;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExtConfig
{
    public static class JsonConfigBuilder
    {
        private const string _include = "_include";
        private const string _variables_source = "_variables_source";
        private static readonly Regex _envVariableRegex = new Regex(@"\$\{([-_,A-Z,0-9]+)\}");

        private static readonly JsonMergeSettings _jsonMergeSettings = new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Ignore,
            MergeArrayHandling = MergeArrayHandling.Union
        };

        public static T Build<T>(string filePattern)
            where T : class
        {
            var filePath = Directory.GetFiles(Directory.GetCurrentDirectory(), filePattern).SingleOrDefault();
            var obj = JObject.Parse(File.ReadAllText(filePath));
            return Build<T>(obj, VariablesSource.GetConfigVariables(obj.Value<string>(_variables_source)));
        }

        public static T Build<T>(JObject obj, IConfigVariables enviromentVariables)
            where T : class
        {
            IncludeSubConfigs(obj);
            Transform(obj, enviromentVariables);
            return obj.ToObject<T>();
        }

        private static void IncludeSubConfigs(JObject obj)
        {
            if (obj.TryGetValue(_include, out JToken include))
            {
                var filepath = include.Value<string>();
                var includeObj = JObject.Parse(File.ReadAllText(filepath));
                IncludeSubConfigs(includeObj);
                obj.Merge(includeObj, _jsonMergeSettings);
            }
        }

        private static void Transform(JObject obj, IConfigVariables enviromentVariables)
        {
            foreach (var item in obj)
                if (item.Value?.Type == JTokenType.Object)
                    Transform((JObject)item.Value, enviromentVariables);
                else if (item.Value?.Type == JTokenType.Array)
                    foreach (var arrayItem in item.Value)
                        if (arrayItem is JObject)
                            Transform((JObject)arrayItem, enviromentVariables);
                        else
                            EnvVariableSubstitution(arrayItem, enviromentVariables);
                else
                    EnvVariableSubstitution(item.Value, enviromentVariables);
        }

        private static void EnvVariableSubstitution(JToken item, IConfigVariables enviromentVariables)
        {
            if (item?.Type != JTokenType.String)
                return;
            var val = item.Value<string>();
            if (_envVariableRegex.IsMatch(val))
            {
                foreach (Match match in _envVariableRegex.Matches(val))
                    if (match != null)
                    {
                        var envName = match.Groups[0].Value;
                        var envValue = enviromentVariables.GetEnviromentVariable(match.Groups[1].Value);
                        val = val.Replace(envName, envValue);
                    }
                item.Replace(JToken.FromObject(val));
            }
        }
    }
}
