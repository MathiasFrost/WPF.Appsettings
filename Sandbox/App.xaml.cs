using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace WPF.Appsettings.Sandbox;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public sealed partial class App
{
    private static IConfiguration Configuration { get; } = new ConfigurationBuilder().AddAppsettings().Build();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Debug.WriteLine(Configuration["Logging:LogLevel:Default"]);
        Debug.WriteLine(Configuration["ExternalAPI:Fitness.API:APIKey"]);
        Debug.WriteLine(JsonSerializer.Serialize(Configuration.GetSection("Logging").Get<Logging>()));
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global"), SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class Logging
    {
        public LogLevel LogLevel { get; init; } = new();
    }

    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global"), SuppressMessage("ReSharper", "UnusedMember.Global")]
    public sealed class LogLevel
    {
        public string Default { get; init; } = String.Empty;
    }
}