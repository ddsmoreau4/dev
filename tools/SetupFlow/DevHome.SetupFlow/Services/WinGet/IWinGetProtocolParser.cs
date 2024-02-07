﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using DevHome.SetupFlow.Models;

namespace DevHome.SetupFlow.Services.WinGet;

internal interface IWinGetProtocolParser
{
    /// <summary>
    /// Create a package uri from a package
    /// </summary>
    /// <param name="package">Package</param>
    /// <returns>Package uri</returns>
    public Uri CreatePackageUri(IWinGetPackage package);

    /// <summary>
    /// Create a winget catalog package uri from a package id
    /// </summary>
    /// <param name="packageId">Package id</param>
    /// <returns>Package uri</returns>
    public Uri CreateWinGetCatalogPackageUri(string packageId);

    /// <summary>
    /// Create a Microsoft store catalog package uri from a package id
    /// </summary>
    /// <param name="packageId">Package id</param>
    /// <returns>Package uri</returns>
    public Uri CreateMsStoreCatalogPackageUri(string packageId);

    /// <summary>
    /// Create a custom catalog package uri from a package id and catalog name
    /// </summary>
    /// <param name="packageId">Package id</param>
    /// <param name="catalogName">Catalog name</param>
    /// <returns>Package uri</returns>
    public Uri CreateCustomCatalogPackageUri(string packageId, string catalogName);

    /// <summary>
    /// Get the package id and catalog from a package uri
    /// </summary>
    /// <param name="packageUri">Input package uri</param>
    /// <returns>Package id and catalog, or null if the URI protocol is inaccurate</returns>
    public WinGetProtocolParserResult ParsePackageUri(Uri packageUri);

    /// <summary>
    /// Resolve a catalog from a parser result
    /// </summary>
    /// <param name="result">Parser result</param>
    /// <returns>Catalog</returns>
    public Task<WinGetCatalog> ResolveCatalogAsync(WinGetProtocolParserResult result);

    /// <summary>
    /// Create a package uri from a package id and catalog
    /// </summary>
    /// <param name="packageId">Package id</param>
    /// <param name="catalog">Catalog</param>
    /// <returns>Package uri</returns>
    public Uri CreatePackageUri(string packageId, WinGetCatalog catalog);
}
