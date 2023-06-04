# WPF.Appsettings - _WPF_

![Logo](https://raw.githubusercontent.com/MathiasFrost/WPF.Appsettings/main/logo.png)

NuGet aiming to make it easier to use [Microsoft.Extensions.Logging](https://www.nuget.org/packages/Microsoft.Extensions.Configuration) in WPF applications

## Usage

Define `WPF_ENVIRONMENT` as an environment variable. For development this is typically `Development`:

```xml

<ItemGroup>
	<EmbeddedResource Include="appsettings.json" />
	<EmbeddedResource Include="appsettings.Development.json" Condition="'$(WPF_ENVIRONMENT)' == 'Development'" />
	<EmbeddedResource Include="appsettings.secrets.json" />
</ItemGroup>
```

You can then run this code to load one or two `appsettings.json` files with this code:

```csharp
public sealed partial class App
{
    private static IConfiguration Configuration { get; } = new ConfigurationBuilder().AddAppsettings().Build();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

		// From here, configuration is used exactly like in ASP.NET Core (minus the dependency injection of course)
        Debug.WriteLine(Configuration["Logging:LogLevel:Default"]);
    }
}
```

All `appsettings.json` files matching the following regex will be loaded:

```regexp
(?<app_name>(?:\w+\.?)+)\.appsettings(?<environment>\.\w+)?\.json
```

## Load order

1. `appsettings.json`
2. `appsettings.{WPF_ENVIRONMENT}.json`
3. `appsettings.local.json`

## Committing

Important to run this before committing _(assuming you have GPG key set up)_

```shell
git config commit.gpgsign true
```