namespace WPF.Appsettings;

public sealed class ConfigurationSection
{
    public string Path { get; }

    public ConfigurationSection(string path)
    {
        Path = path;
    }

    public string GetString() => Appsettings.Value[Path];
}