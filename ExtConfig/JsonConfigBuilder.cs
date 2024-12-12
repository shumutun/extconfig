using System;
using ExtConfig.VariablesSources;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace ExtConfig
{
    public static class JsonConfigBuilder
    {
        private const string _include = "_include";
        private const string _variablesSource = "_variables_source";
        private static readonly Regex _envVariableRegex = new(@"\$\{([-_,A-Z,0-9]+)\}");

        private static readonly JsonMergeSettings _jsonMergeSettings = new()
        {
            MergeNullValueHandling = MergeNullValueHandling.Ignore,
            MergeArrayHandling = MergeArrayHandling.Union
        };

        #region JsonFile

        public static T? Build<T>(string filePattern)
            where T : class
        {
            var filePath = Directory.GetFiles(Directory.GetCurrentDirectory(), filePattern).SingleOrDefault();
            if (filePath is null)
                throw new ArgumentException("Not a file matching the file pattern found");
            var obj = JObject.Parse(File.ReadAllText(filePath));
            IncludeSubConfigs(obj);
            var variablesSource = obj.Value<string>(_variablesSource);
            Transform(obj, VariablesSource.GetConfigVariables(variablesSource));
            return obj.ToObject<T>();
        }

        private static void Transform(JObject obj, IConfigVariables environmentVariables)
        {
            foreach (var item in obj)
                switch (item.Value?.Type)
                {
                    case JTokenType.Object:
                    {
                        Transform((JObject)item.Value, environmentVariables);
                        break;
                    }
                    case JTokenType.Array:
                    {
                        foreach (var arrayItem in item.Value)
                            if (arrayItem is JObject jObject)
                                Transform(jObject, environmentVariables);
                            else if (!TrySubstituteEnvVariable(arrayItem, environmentVariables))
                                arrayItem.Remove();
                        break;
                    }
                    default:
                    {
                        if(!TrySubstituteEnvVariable(item.Value, environmentVariables))
                            item.Value?.Remove();
                        break;
                    }
                }
        }

        private static void IncludeSubConfigs(JObject obj)
        {
            if (!obj.TryGetValue(_include, out var include))
                return;
            var filepath = include.Value<string>();
            if (filepath is null)
                throw new ArgumentNullException(_include);
            var includeObj = JObject.Parse(File.ReadAllText(filepath));
            IncludeSubConfigs(includeObj);
            obj.Merge(includeObj, _jsonMergeSettings);
        }

        private static bool TrySubstituteEnvVariable(JToken? item, IConfigVariables environmentVariables)
        {
            if (item?.Type != JTokenType.String)
                return true;
            var value = item.Value<string>()!;
            value = ProcSubstitutions(value, environmentVariables);
            if (value is not null)
                item.Replace(JToken.FromObject(value));
            return value is not null;
        }

        #endregion

        #region AppSettings

        public static T? Build<T>(IConfigurationSection config)
            where T : class
        {
            var variablesSource = config.GetSection(_variablesSource);
            Transform(config, VariablesSource.GetConfigVariables(variablesSource.Value));
            return config.Get<T>();
        }

        private static void Transform(IConfigurationSection section, IConfigVariables environmentVariables)
        {
            foreach (var subSection in section.GetChildren())
                Transform(subSection, environmentVariables);
            if (section.Value is not null)
                section.Value = ProcSubstitutions(section.Value, environmentVariables);
        }

        #endregion

        private static string? ProcSubstitutions(string value, IConfigVariables environmentVariables)
        {
            if (!_envVariableRegex.IsMatch(value))
                return value;
            foreach (Match match in _envVariableRegex.Matches(value))
                if (match is not null)
                {
                    var env = match.Groups[0].Value;
                    value = value.Replace(env, environmentVariables.TryGetEnvironmentVariable(match.Groups[1].Value, out var envValue) ? envValue : string.Empty);
                }

            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}