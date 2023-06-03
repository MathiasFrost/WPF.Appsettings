using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace WPF.Appsettings.Sandbox;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        foreach (var (key, value) in Appsettings.Value)
        {
            Debug.WriteLine($"{key} {value}");  
        }   
        Debug.WriteLine(Appsettings.GetSection("Logging__LogLevel__Default").GetString());
    }
}