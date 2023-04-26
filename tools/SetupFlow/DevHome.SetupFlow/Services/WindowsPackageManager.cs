﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Threading.Tasks;
using DevHome.Services;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.SetupFlow.Common.WindowsPackageManager;
using DevHome.SetupFlow.Exceptions;
using DevHome.SetupFlow.Models;
using Microsoft.Extensions.Options;
using Microsoft.Management.Deployment;
using Windows.ApplicationModel.Store.Preview.InstallControl;

namespace DevHome.SetupFlow.Services;

/// <summary>
/// Windows package manager class is an entrypoint for using the WinGet COM API.
/// </summary>
public class WindowsPackageManager : IWindowsPackageManager
{
    private readonly WindowsPackageManagerFactory _wingetFactory;
    private readonly IAppInstallManagerService _appInstallManagerService;
    private readonly IOptions<SetupFlowOptions> _setupFlowOptions;

    // Custom composite catalogs
    private readonly Lazy<WinGetCompositeCatalog> _allCatalogs;
    private readonly Lazy<WinGetCompositeCatalog> _wingetCatalog;

    // Predefined catalog ids
    private readonly Lazy<string> _wingetCatalogId;
    private readonly Lazy<string> _msStoreCatalogId;

    // App installer
    private readonly Lazy<bool> _isCOMServerAvailable;
    private bool _appInstallerUpdateAvailable;

    public WindowsPackageManager(
        WindowsPackageManagerFactory wingetFactory,
        IAppInstallManagerService appInstallManagerService,
        IOptions<SetupFlowOptions> setupFlowOptions)
    {
        _wingetFactory = wingetFactory;
        _appInstallManagerService = appInstallManagerService;
        _setupFlowOptions = setupFlowOptions;

        // Lazy-initialize custom composite catalogs
        _allCatalogs = new (CreateAllCatalogs);
        _wingetCatalog = new (CreateWinGetCatalog);

        // Lazy-initialize predefined catalog ids
        _wingetCatalogId = new (() => GetPredefinedCatalogId(PredefinedPackageCatalog.OpenWindowsCatalog));
        _msStoreCatalogId = new (() => GetPredefinedCatalogId(PredefinedPackageCatalog.MicrosoftStore));

        _appInstallManagerService.ItemCompleted += OnAppInstallManagerItemCompleted;
        _isCOMServerAvailable = new (IsCOMServerAvailableInternal);
    }

    public string WinGetCatalogId => _wingetCatalogId.Value;

    public string MsStoreId => _msStoreCatalogId.Value;

    public IWinGetCatalog AllCatalogs => _allCatalogs.Value;

    public IWinGetCatalog WinGetCatalog => _wingetCatalog.Value;

    public event EventHandler AppInstallerUpdateCompleted;

    public async Task ConnectToAllCatalogsAsync()
    {
        Log.Logger?.ReportInfo(Log.Component.AppManagement, "Connecting to all catalogs");

        // Connect composite catalog for all local and remote catalogs to
        // enable searching for pacakges from any source
        await AllCatalogs.ConnectAsync();

        // Connect predefined winget catalog to enable loading
        // package with a known source (e.g. for restoring packages)
        await WinGetCatalog.ConnectAsync();
    }

    public async Task<InstallPackageResult> InstallPackageAsync(WinGetPackage package)
    {
        var packageManager = _wingetFactory.CreatePackageManager();
        var options = _wingetFactory.CreateInstallOptions();
        options.PackageInstallMode = PackageInstallMode.Silent;

        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Starting package install for {package.Id}");
        var installResult = await packageManager.InstallPackageAsync(package.CatalogPackage, options).AsTask();

        Log.Logger?.ReportInfo(
            Log.Component.AppManagement,
            $"Install result: Status={installResult.Status}, InstallerErrorCode={installResult.InstallerErrorCode}, RebootRequired={installResult.RebootRequired}");

        if (installResult.Status != InstallResultStatus.Ok)
        {
            throw new InstallPackageException(installResult.Status, installResult.InstallerErrorCode);
        }

        return new ()
        {
            RebootRequired = installResult.RebootRequired,
        };
    }

    public bool IsCOMServerAvailable => _isCOMServerAvailable.Value;

    public async Task<bool> IsAppInstallerUpdateAvailableAsync()
    {
        try
        {
            Log.Logger?.ReportInfo(Log.Component.AppManagement, "Checking if AppInstaller has an update ...");
            _appInstallerUpdateAvailable = await _appInstallManagerService.IsAppUpdateAvailableAsync(_setupFlowOptions.Value.AppInstallerProductId);
            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"AppInstaller update available = {_appInstallerUpdateAvailable}");
            return _appInstallerUpdateAvailable;
        }
        catch (Exception e)
        {
            Log.Logger?.ReportError(Log.Component.AppManagement, "Failed to check if AppInstaller has an update, defaulting to false", e);
            return false;
        }
    }

    public async Task<bool> StartAppInstallerUpdateAsync()
    {
        try
        {
            Log.Logger?.ReportInfo(Log.Component.AppManagement, "Starting AppInstaller update ...");
            var updateStarted = await _appInstallManagerService.StartAppUpdateAsync(_setupFlowOptions.Value.AppInstallerProductId);
            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Start AppInstaller update = {updateStarted}");
            return updateStarted;
        }
        catch (Exception e)
        {
            Log.Logger?.ReportError(Log.Component.AppManagement, "Failed to start AppInstaller update", e);
            return false;
        }
    }

    /// <summary>
    /// Gets the id of the provided predefined catalog
    /// </summary>
    /// <param name="catalog">Predefined catalog</param>
    /// <returns>Catalog id</returns>
    private string GetPredefinedCatalogId(PredefinedPackageCatalog catalog)
    {
        var packageManager = _wingetFactory.CreatePackageManager();
        var packageCatalog = packageManager.GetPredefinedPackageCatalog(catalog);
        return packageCatalog.Info.Id;
    }

    /// <summary>
    /// Create a composite catalog that can be used for finding packages in all
    /// remote and local packages
    /// </summary>
    /// <returns>Catalog composed of all remote and local catalogs</returns>
    private WinGetCompositeCatalog CreateAllCatalogs()
    {
        var compositeCatalog = new WinGetCompositeCatalog(_wingetFactory);
        compositeCatalog.CompositeSearchBehavior = CompositeSearchBehavior.RemotePackagesFromAllCatalogs;
        var packageManager = _wingetFactory.CreatePackageManager();
        var catalogs = packageManager.GetPackageCatalogs();

        // Cannot use foreach or Linq for out-of-process IVector
        // Bug: https://github.com/microsoft/CsWinRT/issues/1205
        for (var i = 0; i < catalogs.Count; ++i)
        {
            compositeCatalog.AddPackageCatalog(catalogs[i]);
        }

        return compositeCatalog;
    }

    /// <summary>
    /// Create a composite catalog that can be used for finding packages in
    /// winget and local catalogs
    /// </summary>
    /// <returns>Catalog composed of winget and local catalogs</returns>
    private WinGetCompositeCatalog CreateWinGetCatalog()
    {
        return CreatePredefinedCatalog(PredefinedPackageCatalog.OpenWindowsCatalog);
    }

    /// <summary>
    /// Create a composite catalog that can be used for finding packages in
    /// a predefined and local catalogs
    /// </summary>
    /// <param name="predefinedPackageCatalog">Predefined package catalog</param>
    /// <returns>Catalog composed of the provided and local catalogs</returns>
    private WinGetCompositeCatalog CreatePredefinedCatalog(PredefinedPackageCatalog predefinedPackageCatalog)
    {
        var compositeCatalog = new WinGetCompositeCatalog(_wingetFactory);
        compositeCatalog.CompositeSearchBehavior = CompositeSearchBehavior.RemotePackagesFromAllCatalogs;
        var packageManager = _wingetFactory.CreatePackageManager();
        var catalog = packageManager.GetPredefinedPackageCatalog(predefinedPackageCatalog);
        compositeCatalog.AddPackageCatalog(catalog);
        return compositeCatalog;
    }

    private bool IsCOMServerAvailableInternal()
    {
        try
        {
            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Attempting to create a dummy out-of-proc {nameof(WindowsPackageManager)} COM object to test if the COM server is available");
            _wingetFactory.CreatePackageManager();
            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"{nameof(WindowsPackageManager)} COM object created successfully");
            return true;
        }
        catch (Exception e)
        {
            Log.Logger?.ReportError(Log.Component.AppManagement, $"Failed to create a {nameof(WindowsPackageManager)} COM object", e);
            return false;
        }
    }

    private void OnAppInstallManagerItemCompleted(AppInstallManager appInstallManager, AppInstallManagerItemEventArgs itemEventArgs)
    {
        var item = itemEventArgs?.Item;
        if (item?.ProductId == _setupFlowOptions.Value.AppInstallerProductId && item?.InstallType == AppInstallType.Update)
        {
            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"AppInstaller update completed");
            _appInstallerUpdateAvailable = false;
            AppInstallerUpdateCompleted?.Invoke(null, EventArgs.Empty);
        }
    }
}
