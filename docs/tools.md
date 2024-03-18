# Tools

Dev Home adds functionality through a set of tools. Each tool provides a page in the Dev Home navigation view. Currently, all tools come from in-package assemblies. Third party or out-of-process tools are not supported at this time.

Tools utilize data and functionality from out-of-process [extensions](./extensions.md). This is done through the Extension SDK API. 

## Writing a Tool

1. Create a new directory with your tool's name under `tools` with three subdirectories `src`, `test`, and `uitest`
1. Create a new `WinUI 3 Class Library` project in your `src` directory
1. Create the `Strings\en-us` directories under `src`. Add `Resources.resw` and include the following code:
    ```xml
    <data name="NavigationPane.Content" xml:space="preserve">
      <value>[Name of your tool that will appear in navigation menu]</value>
      <comment>[Extra information about the name of your tool that may help translation]</comment>
    </data>
    ```
1. Add a project reference from `DevHome` to your project
1. Add a project reference from your project to `DevHome.Common.csproj` project under [/common/](/common)
1. Create your XAML View and ViewModel. Your View class must inherit from `ToolPage` and implement [tool interface requirements](#tool-requirements).
1. Update [NavConfig.jsonc](/src/NavConfig.jsonc) with your tool. Specifications for the [NavConfig.json schema](navconfig.md).

### Tool requirements

Each tool must define a custom page view extending from the [`ToolPage`](../common/ToolPage.cs) abstract class, and implement it like in this example:

```cs
public class SampleToolPage : ToolPage
{
    public override string ShortName => "SampleTool";

    public SampleToolPage()
    {
        ViewModel = Application.Current.GetService<SampleToolViewModel>();
        InitializeComponent();
    }
}
```

The Dev Home framework will look at all types in its assembly for any inheriting from `ToolPage`:

On a found type, the framework will use:
  - ShortName property to get the name of the tool

#### Method definition

This section contains a more detailed description of each of the interface methods.

ShortName

```cs
public abstract string ShortName { get; }
```

Returns the name of the tool. This is used for the navigation menu text.

### Code organization

[`toolpage.cs`](../common/ToolPage.cs)
Contains the interface definition for Dev Home tools.

### Navigation

In order to allow navigation to your tool, your page and ViewModel must be registered with the PageService. If your tool only contains one page, it is automatically registered for you since you added your page to `NavConfig.jsonc`. However, you may have other sub-pages you wish to register.

In order to do so, you must create an extension method for the PageService inside your tool. See examples in [Settings](../settings/DevHome.Settings/Extensions/PageExtensions.cs) or [Extensions](../tools/ExtensionLibrary/DevHome.ExtensionLibrary/Extensions/PageExtensions.cs). Then, call your extension from the [PageService](../src/Services/PageService.cs).

#### Navigating away from your tool

If you want a user action (such as clicking a link) to navigate away from your tool and your project does not otherwise rely on the destination project, do not create a PackageReference to the destination project from yours. Instead, add the destination page to `INavigationService.KnownPageKeys` and use it for navigation, like in this example:
```cs
navigationService.NavigateTo(KnownPageKeys.<DestinationPage>);
```

This keeps the dependency tree simple, and prevents circular references when two projects want to navigate to each other.

## Existing tools

[Dashboard](./tools/Dashboard.md)
