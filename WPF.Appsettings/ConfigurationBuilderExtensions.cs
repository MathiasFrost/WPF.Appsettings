using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace WPF.Appsettings;

/// <summary> TODOC </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    ///     Add appsettings.json files that is embedded in the application matching the regex:
    ///     <code> (?&lt;app_name&gt;(?:\w+\.?)+)\.appsettings(?&lt;environment&gt;\.\w+)?\.json </code>
    /// </summary>
    /// <remarks>
    ///     Note that environment-sensitive appsettings files should be controlled at build time for WPF apps, like so:
    ///     <code>
    ///  	&lt;ItemGroup&gt;
    /// 	        &lt;EmbeddedResource Include="appsettings.json" /&gt;
    /// 	        &lt;EmbeddedResource Include="appsettings.Development.json" Condition="'$(WPF_ENVIRONMENT)' == 'Development'" /&gt;
    /// 	        &lt;EmbeddedResource Include="appsettings.secrets.json" /&gt;
    ///    &lt;/ItemGroup&gt;
    ///  </code>
    /// </remarks>
    public static IConfigurationBuilder AddAppsettings(this ConfigurationBuilder builder)
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null) return builder;

        string[] names = assembly.GetManifestResourceNames();

        void AddJsonFile(string filename)
        {
            string? assemblyName = assembly.GetName().Name;
            if (assemblyName == null) return;

            string? name = names.FirstOrDefault(str => str == $"{assemblyName}.{filename}");
            if (name == null) return;

            Stream? stream = assembly.GetManifestResourceStream(name);
            if (stream == null) return;

            builder.AddJsonStream(stream);
        }

        AddJsonFile("appsettings.json");
        string? wpfEnv = names.Select(static name => name.Split('.'))
            .Where(static parts => parts.Last() == "json" && parts.Length > 1 && parts[^3] == "appsettings")
            .Select(static parts => parts[^2])
            .FirstOrDefault();

        if (wpfEnv != null) AddJsonFile($"appsettings.{wpfEnv}.json");
        AddJsonFile("appsettings.secrets.json");

        return builder;
    }
}