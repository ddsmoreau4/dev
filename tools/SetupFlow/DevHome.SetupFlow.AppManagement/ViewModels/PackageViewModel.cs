﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHome.Contracts.Services;
using DevHome.SetupFlow.AppManagement.Models;
using DevHome.SetupFlow.AppManagement.Services;
using DevHome.SetupFlow.ComInterop.Projection.WindowsPackageManager;
using DevHome.SetupFlow.Common.Services;
using DevHome.Telemetry;
using Microsoft.Internal.Windows.DevHome.Helpers.Restore;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.System;

namespace DevHome.SetupFlow.AppManagement.ViewModels;

/// <summary>
/// Delegate factory for creating package view models
/// </summary>
/// <param name="package">WinGet package</param>
/// <returns>Package view model</returns>
public delegate PackageViewModel PackageViewModelFactory(IWinGetPackage package);

/// <summary>
/// ViewModel class for the <see cref="Package"/> model.
/// </summary>
public partial class PackageViewModel : ObservableObject
{
    private static readonly BitmapImage DefaultLightPackageIconSource = new (new Uri("ms-appx:///DevHome.SetupFlow/Assets/DefaultLightPackageIcon.png"));
    private static readonly BitmapImage DefaultDarkPackageIconSource = new (new Uri("ms-appx:///DevHome.SetupFlow/Assets/DefaultDarkPackageIcon.png"));

    private readonly Lazy<BitmapImage> _packageDarkThemeIcon;
    private readonly Lazy<BitmapImage> _packageLightThemeIcon;
    private readonly Lazy<InstallPackageTask> _installPackageTask;

    private readonly ILogger _logger;
    private readonly ISetupFlowStringResource _stringResource;
    private readonly IWinGetPackage _package;
    private readonly IWindowsPackageManager _wpm;
    private readonly IThemeSelectorService _themeSelector;
    private readonly WindowsPackageManagerFactory _wingetFactory;

    /// <summary>
    /// Occurrs after the package selection changes
    /// </summary>
    public event EventHandler<PackageViewModel> SelectionChanged;

    /// <summary>
    /// Indicates if a package is selected
    /// </summary>
    [ObservableProperty]
    private bool _isSelected;

    public PackageViewModel(
        ILogger logger,
        ISetupFlowStringResource stringResource,
        IWindowsPackageManager wpm,
        IWinGetPackage package,
        IThemeSelectorService themeSelector,
        WindowsPackageManagerFactory wingetFactory)
    {
        _logger = logger;
        _stringResource = stringResource;
        _wpm = wpm;
        _package = package;
        _themeSelector = themeSelector;
        _wingetFactory = wingetFactory;
        _packageDarkThemeIcon = new Lazy<BitmapImage>(() => GetIconByTheme(RestoreApplicationIconTheme.Dark));
        _packageLightThemeIcon = new Lazy<BitmapImage>(() => GetIconByTheme(RestoreApplicationIconTheme.Light));
        _installPackageTask = new Lazy<InstallPackageTask>(CreateInstallTask);
    }

    public PackageUniqueKey UniqueKey => _package.UniqueKey;

    public IWinGetPackage Package => _package;

    public string Name => _package.Name;

    public BitmapImage Icon => _themeSelector.IsDarkTheme() ? _packageDarkThemeIcon.Value : _packageLightThemeIcon.Value;

    public string Version => _package.Version;

    public bool IsInstalled => _package.IsInstalled;

    public InstallPackageTask InstallPackageTask => _installPackageTask.Value;

    /// <summary>
    /// Gets the URI for the "Learn more" button
    /// </summary>
    /// <remarks>
    /// For packages from winget or custom catalogs:
    /// 1. Use package url
    /// 2. Else, use publisher url
    /// 3. Else, use "https://github.com/microsoft/winget-pkgs"
    ///
    /// For packages from ms store catalog:
    /// 1. Use package url
    /// 2. Else, use "ms-windows-store://pdp?productid={ID}"
    /// </remarks>
    /// <returns>Learn more button uri</returns>
    public Uri GetLearnMoreUri()
    {
        if (_package.PackageUrl != null)
        {
            return _package.PackageUrl;
        }

        if (_package.CatalogId == _wpm.MsStoreId)
        {
            return new Uri($"ms-windows-store://pdp/?productid={_package.Id}");
        }

        if (_package.PublisherUrl != null)
        {
            return _package.PublisherUrl;
        }

        return new Uri("https://github.com/microsoft/winget-pkgs");
    }

    partial void OnIsSelectedChanged(bool value) => SelectionChanged?.Invoke(null, this);

    /// <summary>
    /// Toggle package selection
    /// </summary>
    [RelayCommand]
    private void ToggleSelection() => IsSelected = !IsSelected;

    /// <summary>
    /// Gets the package icon based on the provided theme
    /// </summary>
    /// <param name="theme">Package icon theme</param>
    /// <returns>Package icon</returns>
    private BitmapImage GetIconByTheme(RestoreApplicationIconTheme theme)
    {
        return theme switch
        {
            // Get default dark theme icon if corresponding package icon was not found
            RestoreApplicationIconTheme.Dark =>
                _package.DarkThemeIcon == null ? DefaultDarkPackageIconSource : CreateBitmapImage(_package.DarkThemeIcon),

            // Get default light theme icon if corresponding package icon was not found
            _ => _package.LightThemeIcon == null ? DefaultLightPackageIconSource : CreateBitmapImage(_package.LightThemeIcon),
        };
    }

    private BitmapImage CreateBitmapImage(IRandomAccessStream stream)
    {
        var bitmapImage = new BitmapImage();
        bitmapImage.SetSource(stream);
        return bitmapImage;
    }

    private InstallPackageTask CreateInstallTask()
    {
        return _package.CreateInstallTask(_logger, _wpm, _stringResource, _wingetFactory);
    }

    /// <summary>
    /// Command for launching a 'Learn more' uri
    /// </summary>
    [RelayCommand]
    private async Task LearnMoreAsync()
    {
        await Launcher.LaunchUriAsync(GetLearnMoreUri());
    }
}
