﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using DevHome.SetupFlow.Models;

namespace DevHome.SetupFlow.Services.WinGet;

internal interface IWinGetPackageInstaller
{
    /// <summary>
    /// Install a package from WinGet catalog
    /// </summary>
    /// <param name="catalog">Catalog from which to install the package</param>
    /// <param name="packageId">Package id to install</param>
    /// <returns>Result of the installation</returns>
    public Task<InstallPackageResult> InstallPackageAsync(WinGetCatalog catalog, string packageId);
}
