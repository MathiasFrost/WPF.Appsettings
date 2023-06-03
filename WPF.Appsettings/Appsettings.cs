using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace WPF.Appsettings;

public static class Appsettings
{
    public static readonly JsonObject Root = EmptyJsonObject();

    static Appsettings()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null) return;

        string[] names = assembly.GetManifestResourceNames();

        JsonObject ParseJsonFile(string filename)
        {
            string? assemblyName = assembly.GetName().Name;
            if (assemblyName == null) return EmptyJsonObject();

            var name = names.FirstOrDefault(str => str == $"{assemblyName}.{filename}");
            if (name == null) return EmptyJsonObject();

            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null) return EmptyJsonObject();

            var res = JsonSerializer.Deserialize<JsonObject>(stream);
            return res ?? EmptyJsonObject();
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

        Root = ParseJsonFile("appsettings.json");
        var envJson = wpfEnv == null ? EmptyJsonObject() : ParseJsonFile($"appsettings.{wpfEnv}.json");
        var localRootJson = ParseJsonFile("appsettings.local.json");
        var localEnvJson = wpfEnv == null ? EmptyJsonObject() : ParseJsonFile($"appsettings.{wpfEnv}.local.json");

        Root.Merge(envJson);
        Root.Merge(localRootJson);
        Root.Merge(localEnvJson);
    }

    private static JsonObject EmptyJsonObject() => JsonNode.Parse("{}")!.AsObject();

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

    /// TODOC
    private static JsonNode Copy(this JsonNode jsonNode) => JsonNode.Parse(jsonNode.ToJsonString())!;

    /// Layer b on top of a
    private static void Merge(this JsonNode? a, JsonNode? b)
    {
        var typeA = a.GetJsonType();
        var typeB = b.GetJsonType();
        switch (typeA)
        {
            case JsonType.Object when typeB is JsonType.Object:
                var objA = a!.AsObject();
                var objB = b!.AsObject();
                foreach (var (keyB, valB) in objB)
                {
                    var valA = objA[keyB];
                    if (objA.ContainsKey(keyB))
                    {
                        typeA = valA.GetJsonType();
                        typeB = valB.GetJsonType();
                        switch (typeA)
                        {
                            case JsonType.Value when objA.Remove(keyB): // If a is value, replace a with b
                                objA.TryAdd(keyB, valB!.Copy());
                                break;
                            case JsonType.Object when typeB is JsonType.Object: // If both are objects merge them recursively
                                valA.Merge(valB);
                                break;
                            case JsonType.Array when typeB is JsonType.Array: // If both are arrays merge them recursively
                                valA.Merge(valB);
                                break;
                            default: // If types are different, just overwrite
                                if (objA.Remove(keyB)) objA.TryAdd(keyB, valB!.Copy());
                                break;
                        }
                    }
                    else // If a does not have b, simply add b to a
                    {
                        objA.Add(keyB, valB!.Copy());
                    }
                }

                break;
            case JsonType.Array when typeB is JsonType.Array:
                var arrA = a!.AsArray();
                var arrB = b!.AsArray();
                var i = 0;
                foreach (var elB in arrB)
                {
                    if (arrA.Count > i)
                    {
                        var elA = arrA[i];
                        typeA = elA.GetJsonType();
                        typeB = elB.GetJsonType();
                        switch (typeA)
                        {
                            case JsonType.Value: // If a is value, replace a with b
                                arrA[i] = elB!.Copy();
                                break;
                            case JsonType.Object when typeB is JsonType.Object: // If both are objects merge them recursively
                                elA.Merge(elB);
                                break;
                            case JsonType.Array when typeB is JsonType.Array: // If both are arrays merge them recursively
                                elA.Merge(elB);
                                break;
                            default: // If types are different, just overwrite
                                if (elB != null) arrA[i] = elB.Copy();
                                break;
                        }
                    }
                    else // If a does not have b, simply add b to a
                    {
                        arrA.Add(elB!.Copy());
                    }
                }

                break;
        }
    }

    public static JsonNode GetPropertyValue(this JsonObject jsonObject, string key) =>
        jsonObject.TryGetPropertyValue(key, out var value) && value != null
            ? value
            : throw new InvalidOperationException($"Could not find property {jsonObject.GetPath()}.{key}");

    public static T GetValue<T>(this JsonObject jsonObject, string key) =>
        jsonObject.GetPropertyValue(key).GetValue<T>();

    public static JsonObject GetObject(this JsonObject jsonObject, string key) =>
        jsonObject.GetPropertyValue(key).AsObject();

    public static JsonArray GetArray(this JsonObject jsonObject, string key) =>
        jsonObject.GetPropertyValue(key).AsArray();
}