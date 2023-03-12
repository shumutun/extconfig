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
        private static readonly Regex _envVariableRegex = new Regex(@"\$\{([-_,A-Z,0-9]+)\}");

        private static readonly JsonMergeSettings _jsonMergeSettings = new JsonMergeSettings
        {
            MergeNullValueHandling = MergeNullValueHandling.Ignore,
            MergeArrayHandling = MergeArrayHandling.Union
        };

        public static T? Build<T>(string filePattern)
            where T : class
        {
            var filePath = Directory.GetFiles(Directory.GetCurrentDirectory(), filePattern).SingleOrDefault();
            if (filePath == null)
                throw new ArgumentException("No files found matching the file pattern");
            var obj = JObject.Parse(File.ReadAllText(filePath));
            return Build<T>(obj);
        }

        public static T? Build<T>(IConfigurationSection config)
            where T : class
        {
            var obj = Serialize(config);
            return Build<T>(obj);
        }

        private static JObject Serialize(IConfiguration config)
        {
            var obj = new JObject();
            foreach (var child in config.GetChildren())
                obj.Add(child.Key, Serialize(child));
            if (obj.HasValues || config is not IConfigurationSection section)
                return obj;
            if (section.Value == null)
                throw new ArgumentException("The config is empty");
            return JObject.Parse(section.Value);
        }

        private static T? Build<T>(JObject obj)
            where T : class
        {
            IncludeSubConfigs(obj);
            var variablesSource = obj.Value<string>(_variablesSource);
            if (variablesSource == null)
                throw new ArgumentNullException(_variablesSource);
            Transform(obj, VariablesSource.GetConfigVariables(variablesSource));
            return obj.ToObject<T>();
        }

        private static void IncludeSubConfigs(JObject obj)
        {
            if (!obj.TryGetValue(_include, out JToken? include))
                return;
            var filepath = include.Value<string>();
            if (filepath == null)
                throw new ArgumentNullException(_include);
            var includeObj = JObject.Parse(File.ReadAllText(filepath));
            IncludeSubConfigs(includeObj);
            obj.Merge(includeObj, _jsonMergeSettings);
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
                            else
                                EnvVariableSubstitution(arrayItem, environmentVariables);
                        break;
                    }
                    default:
                    {
                        EnvVariableSubstitution(item.Value, environmentVariables);
                        break;
                    }
                }
        }

        private static void EnvVariableSubstitution(JToken? item, IConfigVariables environmentVariables)
        {
            if (item?.Type != JTokenType.String)
                return;
            var value = item.Value<string>()!;
            if (!_envVariableRegex.IsMatch(value))
                return;
            foreach (Match match in _envVariableRegex.Matches(value))
                if (match != null)
                {
                    var envName = match.Groups[0].Value;
                    var envValue = environmentVariables.GetEnviromentVariable(match.Groups[1].Value);
                    value = value.Replace(envName, envValue);
                }

            item.Replace(JToken.FromObject(value));
        }
    }
}