// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading.Tasks;
using DevHome.SetupFlow.Common.Helpers;

namespace DevHome.SetupFlow.Services;

/// <summary>
/// Class responsible for initializing the App Management system in the setup flow.
/// </summary>
public class AppManagementInitializer : IAppManagementInitializer
{
    private readonly IWindowsPackageManager _wpm;
    private readonly ICatalogDataSourceLoader _catalogDataSourceLoader;

    public AppManagementInitializer(
        IWindowsPackageManager wpm,
        ICatalogDataSourceLoader catalogDataSourceLoader)
    {
        _wpm = wpm;
        _catalogDataSourceLoader = catalogDataSourceLoader;
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Initializing app management");

        // Initialize catalogs from all data sources
        await InitializeCatalogsAsync();

        // Ensure AppInstaller is registered
        if (await TryRegisterAppInstallerAsync())
        {
            await InitializeInternalAsync();
        }

        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Completed app management initialization");
    }

    /// <inheritdoc />
    public async Task ReinitializeAsync()
    {
        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Reinitializing app management");
        await InitializeInternalAsync();
        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Completed app management reinitialization");
    }

    /// <summary>
    /// Initialize app management services
    /// </summary>
    private async Task InitializeInternalAsync()
    {
        try
        {
            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Ensuring app management initialization");

            // Initialize windows package manager after AppInstaller is registered
            await _wpm.InitializeAsync();

            // Load catalogs from all data sources
            await LoadCatalogsAsync();

            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Finished ensuring app management initialization");
        }
        catch (Exception e)
        {
            Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Unable to correctly initialize app management at the moment. Further attempts will be performed later.", e);
        }
    }

    /// <summary>
    /// Initialize catalogs from all data sources (e.g. Restore packages, etc ...)
    /// </summary>
    private async Task InitializeCatalogsAsync()
    {
        Log.Logger?.ReportInfo(Log.Component.AppManagement, "Initialize catalogs from all data sources");
        await _catalogDataSourceLoader.InitializeAsync();
    }

    /// <summary>
    /// Loading catalogs from all data sources(e.g. Restore packages, etc ...)
    /// </summary>
    private async Task LoadCatalogsAsync()
    {
        Log.Logger?.ReportInfo($"Loading catalogs from all data sources at launch time to reduce the wait time when this information is requested");
        await foreach (var dataSourceCatalogs in _catalogDataSourceLoader.LoadCatalogsAsync())
        {
            Log.Logger?.ReportInfo($"Loaded {dataSourceCatalogs.Count} catalogs [{string.Join(", ", dataSourceCatalogs.Select(c => c.Name))}]");
        }
    }

    /// <summary>
    /// Try to register AppInstaller
    /// </summary>
    /// <returns>True if AppInstaller is registered, false otherwise</returns>
    private async Task<bool> TryRegisterAppInstallerAsync()
    {
        Log.Logger?.ReportInfo(Log.Component.AppManagement, "Ensuring AppInstaller is registered ...");

        // If WinGet COM Server is available, then AppInstaller is registered
        if (await _wpm.IsAvailableAsync())
        {
            Log.Logger?.ReportInfo(Log.Component.AppManagement, "AppInstaller is already registered");
            return true;
        }

        Log.Logger?.ReportInfo(Log.Component.AppManagement, "WinGet COM Server is not available. AppInstaller might be staged but not registered, attempting to register it to fix the issue");
        if (await _wpm.RegisterAppInstallerAsync())
        {
            if (await _wpm.IsAvailableAsync())
            {
                Log.Logger?.ReportInfo(Log.Component.AppManagement, "AppInstaller was registered successfully");
                return true;
            }

            Log.Logger?.ReportError(Log.Component.AppManagement, "WinGet COM Server is not available after AppInstaller registration");
        }
        else
        {
            Log.Logger?.ReportError(Log.Component.AppManagement, "AppInstaller was not registered");
        }

        return false;
    }
}
