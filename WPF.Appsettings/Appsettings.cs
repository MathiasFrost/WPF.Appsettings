using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WPF.Appsettings;

public static class Appsettings
{
    public static readonly Dictionary<string, string> Value = new();

    public static ConfigurationSection GetSection(string path)
    {
        return new ConfigurationSection(path);
    }

    static Appsettings()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null) return;

        string[] names = assembly.GetManifestResourceNames();
        Dictionary<string, string> value = new Dictionary<string, string>();

        void AddJsonFile(string filename)
        {
            string? assemblyName = assembly.GetName().Name;
            if (assemblyName == null) return;

            var name = names.FirstOrDefault(str => str == $"{assemblyName}.{filename}");
            if (name == null) return;

            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) return;

            var res = JsonSerializer.Deserialize<JsonObject>(stream);
            res!.Flatten(String.Empty, value);
        }

        string? wpfEnv;
        try
        {
            wpfEnv = "Development"; //Environment.GetEnvironmentVariable("FSHARP_ENVIRONMENT");
            if (String.IsNullOrWhiteSpace(wpfEnv)) wpfEnv = null;
        }
        catch (ArgumentNullException)
        {
            wpfEnv = null;
        }

        AddJsonFile("appsettings.json");
        if (wpfEnv != null) AddJsonFile($"appsettings.{wpfEnv}.json");
        AddJsonFile("appsettings.local.json");
        if (wpfEnv != null) AddJsonFile($"appsettings.{wpfEnv}.local.json");

        Value = value;
    }

    private enum JsonType : byte
    {
        Null,
        Value,
        Object,
        Array
    }

    /// Find out which prop type the JsonNode is
    private static JsonType GetJsonType(this JsonNode? jsonNode)
    {
        if (jsonNode == null) return JsonType.Null;
        try
        {
            jsonNode.AsValue();
            return JsonType.Value;
        }
        catch (InvalidOperationException)
        {
            try
            {
                jsonNode.AsObject();
                return JsonType.Object;
            }
            catch (InvalidOperationException)
            {
                try
                {
                    jsonNode.AsArray();
                    return JsonType.Array;
                }
                catch (InvalidOperationException)
                {
                    throw new Exception("JsonNode could not be cast to anything");
                }
            }
        }
    }

    private static bool GetString(this JsonNode? jsonNode, out string? res)
    {
        res = jsonNode?.AsValue().GetValue<dynamic>().ToString();
        return !String.IsNullOrWhiteSpace(res);
    }

    private static bool NoExisting(this Dictionary<string, string> dictionary, string key) => dictionary.All(pair => pair.Key != key);

    private static void Flatten(this JsonNode? jsonNode, string parent, Dictionary<string, string> dictionary)
    {
        switch (jsonNode.GetJsonType())
        {
            case JsonType.Value when parent != String.Empty && jsonNode.GetString(out string? s):
                dictionary[parent] = s!;
                break;
            case JsonType.Object:
                foreach (var (key, value) in jsonNode!.AsObject())
                {
                    switch (value.GetJsonType())
                    {
                        case JsonType.Value when value.GetString(out string? s):
                            dictionary[$"{parent}{key}"] = s!;
                            break;
                        case JsonType.Object:
                            value!.AsObject().Flatten($"{parent}{key}__", dictionary);
                            break;
                        case JsonType.Array:
                            var arr = value!.AsArray();
                            for (var i = 0; i < arr.Count; i++) arr[i].Flatten($"{parent}{key}__{i}", dictionary);
                            break;
                        case JsonType.Null:
                        default:
                            break;
                    }
                }

                break;
            case JsonType.Array:
                break;
            case JsonType.Null:
            default:
                break;
        }
    }
}