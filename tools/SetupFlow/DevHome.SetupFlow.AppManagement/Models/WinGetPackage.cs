﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using DevHome.SetupFlow.AppManagement.Services;
using Microsoft.Management.Deployment;
using Windows.Storage.Streams;

namespace DevHome.SetupFlow.AppManagement.Models;

/// <summary>
/// Model class for a Windows Package Manager package.
/// </summary>
public class WinGetPackage : IWinGetPackage
{
    private readonly CatalogPackage _package;
    private readonly Lazy<Uri> _packageUrl;
    private readonly Lazy<Uri> _publisherUrl;

    public WinGetPackage(CatalogPackage package)
    {
        _package = package;
        _packageUrl = new (() => GetMetadataValue(metadata => new Uri(metadata.PackageUrl), null));
        _publisherUrl = new (() => GetMetadataValue(metadata => new Uri(metadata.PublisherUrl), null));
    }

    public CatalogPackage CatalogPackage => _package;

    public string Id => _package.Id;

    public string CatalogId => _package.DefaultInstallVersion.PackageCatalog.Info.Id;

    public string Name => _package.Name;

    public string Version => _package.DefaultInstallVersion.Version;

    public IRandomAccessStream LightThemeIcon
    {
        get; set;
    }

    public IRandomAccessStream DarkThemeIcon
    {
        get; set;
    }

    public Uri PackageUrl => _packageUrl.Value;

    public Uri PublisherUrl => _publisherUrl.Value;

    public async Task InstallAsync(IWindowsPackageManager wpm)
    {
        await wpm.InstallPackageAsync(this);
    }

    /// <summary>
    /// Gets the package metadata from the current culture name (e.g. 'en-US')
    /// </summary>
    /// <typeparam name="T">Type of the return value</typeparam>
    /// <param name="metadataFunction">Function called with the package metadata as input</param>
    /// <param name="defaultValue">Default value returned if the package metadata threw an exception</param>
    /// <returns>Metadata function result or default value</returns>
    private T GetMetadataValue<T>(Func<CatalogPackageMetadata, T> metadataFunction, T defaultValue)
    {
        try
        {
            var locale = Thread.CurrentThread.CurrentCulture.Name;
            var metadata = _package.DefaultInstallVersion.GetCatalogPackageMetadata(locale);
            return metadataFunction(metadata);
        }
        catch
        {
            return defaultValue;
        }
    }
}
