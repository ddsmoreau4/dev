﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using DevHome.SetupFlow.Common.Extensions;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.SetupFlow.Common.WindowsPackageManager;
using DevHome.SetupFlow.Exceptions;
using DevHome.SetupFlow.Models;
using Microsoft.Management.Deployment;
using Windows.Win32.Foundation;

namespace DevHome.SetupFlow.Services.WinGet;

/// <summary>
/// Installs a package using the Windows Package Manager (WinGet).
/// </summary>
internal sealed class WinGetPackageInstaller : IWinGetPackageInstaller
{
    private readonly WindowsPackageManagerFactory _wingetFactory;
    private readonly IWinGetPackageFinder _packageFinder;

    public WinGetPackageInstaller(WindowsPackageManagerFactory wingetFactory, IWinGetPackageFinder packageFinder)
    {
        _wingetFactory = wingetFactory;
        _packageFinder = packageFinder;
    }

    /// <inheritdoc />
    public async Task<InstallPackageResult> InstallPackageAsync(WinGetCatalog catalog, string packageId)
    {
        if (catalog == null)
        {
            throw new CatalogNotInitializedException();
        }

        // 1. Find package
        var package = await _packageFinder.GetPackageAsync(catalog, packageId);
        if (package == null)
        {
            Log.Logger?.ReportError(Log.Component.AppManagement, $"Install aborted for package {packageId} because it was not found in the provided catalog {catalog.GetDescriptiveName()}");
            throw new FindPackagesException(FindPackagesResultStatus.CatalogError);
        }

        // 2. Install package
        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Starting package installation for {packageId} from catalog {catalog.GetDescriptiveName()}");
        var installResult = await InstallPackageInternalAsync(package);
        var extendedErrorCode = installResult.ExtendedErrorCode?.HResult ?? HRESULT.S_OK;
        var installErrorCode = installResult.GetValueOrDefault(res => res.InstallerErrorCode, HRESULT.S_OK); // WPM API V4

        // 3. Report install result
        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Install result: Status={installResult.Status}, InstallerErrorCode={installErrorCode}, ExtendedErrorCode={extendedErrorCode}, RebootRequired={installResult.RebootRequired}");
        if (installResult.Status != InstallResultStatus.Ok)
        {
            throw new InstallPackageException(installResult.Status, extendedErrorCode, installErrorCode);
        }

        Log.Logger?.ReportInfo(Log.Component.AppManagement, $"Completed package installation for {packageId} from catalog {catalog.GetDescriptiveName()}");
        return new InstallPackageResult()
        {
            ExtendedErrorCode = extendedErrorCode,
            RebootRequired = installResult.RebootRequired,
        };
    }

    /// <summary>
    /// Install a package from a catalog.
    /// </summary>
    /// <param name="package">Package to install</param>
    /// <returns>Install result</returns>
    private async Task<InstallResult> InstallPackageInternalAsync(CatalogPackage package)
    {
        var installOptions = _wingetFactory.CreateInstallOptions();
        installOptions.PackageInstallMode = PackageInstallMode.Silent;
        var packageManager = _wingetFactory.CreatePackageManager();
        return await packageManager.InstallPackageAsync(package, installOptions).AsTask();
    }
}
