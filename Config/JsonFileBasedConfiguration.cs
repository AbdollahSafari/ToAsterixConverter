using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToAsterixConverter.Config;

public abstract class JsonFileBasedConfiguration
{

    public static string ConfigDirectoryName { get; } = "Config";
    private static readonly List<JsonFileBasedConfiguration> Configurations = new List<JsonFileBasedConfiguration>();
    public static TConfig Read<TConfig>(bool recreate = false) where TConfig : JsonFileBasedConfiguration, new()
    {
        var config = Configurations.FirstOrDefault(u => u is TConfig) as TConfig;
        if (config is null || recreate)
        {
            config = ReadConfigFromFile<TConfig>();
            if (config is null)
            {
                config = new TConfig();
                config.InitialDefaults();
                config.Save();
            }
            //try
            //{
            //    config = ReadConfigFromFile<TConfig>();
            //}
            //catch (Exception exception)
            //{
            //    if (exception is DirectoryNotFoundException
            //        || exception is FileNotFoundException)
            //    {
            //        config = new TConfig();
            //        config.InitialDefaults();
            //        config.Save();
            //    }
            //    else
            //        throw;
            //}


            if (config is not null)
            {
                Configurations.Add(config);
            }
        }

        return config;

    }
    private static JsonFileBasedConfigurationAttribute? GetConfigAttribute(Type configType)
    {
        var type = configType;
        var attributes = type.GetCustomAttributes(typeof(JsonFileBasedConfigurationAttribute), false);
        if (attributes.Any())
        {
            return attributes.First() as JsonFileBasedConfigurationAttribute;
        }
        return null;

    }
    private static JsonFileBasedConfigurationAttribute? GetConfigAttribute<TConfig>()
        where TConfig : JsonFileBasedConfiguration
    {

        return GetConfigAttribute(typeof(TConfig));

    }

    private static string GetConfigDirectory()
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigDirectoryName);
    }
    private static string? GetFilePath(Type configType)
    {
        var attribute = GetConfigAttribute(configType);
        if (attribute is null)
            return null;
        return Path.Combine(GetConfigDirectory(), attribute.FileName);
    }
    private static string? GetFilePath<TConfig>() where TConfig : JsonFileBasedConfiguration
    {
        var attribute = GetConfigAttribute<TConfig>();
        if (attribute is null)
            return null;
        return Path.Combine(GetConfigDirectory(), attribute.FileName);
    }
    private static TConfig? ReadConfigFromFile<TConfig>() where TConfig : JsonFileBasedConfiguration
    {
        var filePath = GetFilePath<TConfig>();
        if (!File.Exists(filePath))
            return null;
        return JsonConvert.DeserializeObject<TConfig>(File.ReadAllText(filePath), new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects
        });
    }

    public void Save()
    {
        var path = GetFilePath(this.GetType());
        var dir = GetConfigDirectory();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        if (!string.IsNullOrWhiteSpace(path))
        {
            var serialized = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            });
            File.WriteAllText(path, serialized);
        }
    }

    public abstract void InitialDefaults();
}