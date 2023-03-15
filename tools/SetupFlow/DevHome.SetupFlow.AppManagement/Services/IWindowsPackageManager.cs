﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Threading.Tasks;
using DevHome.SetupFlow.AppManagement.Models;
using Microsoft.Management.Deployment;

namespace DevHome.SetupFlow.AppManagement.Services;

/// <summary>
/// Interface for interacting with the WinGet package manager.
/// More details: https://github.com/microsoft/winget-cli/blob/master/src/Microsoft.Management.Deployment/PackageManager.idl
/// </summary>
public interface IWindowsPackageManager
{
    /// <summary>
    /// Gets a composite catalog for all remote and local catalogs.
    /// </summary>
    public IWinGetCatalog AllCatalogs
    {
        get;
    }

    /// <summary>
    /// Gets a composite catalog for the predefined <c>winget</c> and local catalogs.
    /// </summary>
    public IWinGetCatalog WinGetCatalog
    {
        get;
    }

    /// <summary>
    /// Opens all predefined catalogs.
    /// </summary>
    /// <exception cref="CatalogConnectionException">Exception thrown if a catalog connection failed</exception>
    public Task ConnectToAllCatalogsAsync();

    /// <summary>
    /// Install a winget package
    /// </summary>
    /// <param name="package">Package to install</param>
    public Task InstallPackageAsync(WinGetPackage package);

    /// <summary>
    /// Checks if a package is obtained from the specified catalog
    /// </summary>
    /// <param name="package">Target package</param>
    /// <param name="catalog">Target catalog</param>
    /// <returns>True if the package is obtained from the specified catalog</returns>
    public bool IsPackageFromCatalog(IWinGetPackage package, PredefinedPackageCatalog catalog);
}
