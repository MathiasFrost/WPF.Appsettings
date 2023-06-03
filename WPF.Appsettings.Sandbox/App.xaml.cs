using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using WPF.Appsettings;

namespace WPF.Appsettings.Sandbox;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        Debug.WriteLine(Appsettings.Root);

        Debug.WriteLine(Appsettings.Root.GetObject("Logging").GetObject("LogLevel").GetValue<string>("Default"));
        foreach (var b in Appsettings.Root.GetArray("Array").Select(node => node!.GetValue<byte>()))
        {
            Debug.WriteLine(b);
        }
    }
}