﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevHome.SetupFlow.Models;

namespace DevHome.SetupFlow.Services.WinGet.Operations;

internal interface IWinGetOperations
{
    /// <inheritdoc cref="IWinGetInstallOperation.InstallPackageAsync"/>"
    public Task<InstallPackageResult> InstallPackageAsync(IWinGetPackage package);

    /// <inheritdoc cref="IWinGetGetPackageOperation.GetPackagesAsync"/>"
    public Task<IList<IWinGetPackage>> GetPackagesAsync(IList<Uri> packageUris);

    /// <inheritdoc cref="IWinGetSearchOperation.SearchAsync"/>"
    public Task<IList<IWinGetPackage>> SearchAsync(string query, uint limit);
}
