﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;

namespace DevHome.Common.Services;
public interface IExtensionService
{
    Task<IEnumerable<IExtensionWrapper>> GetInstalledExtensionsAsync(bool includeDisabledExtensions = false);

    Task<IEnumerable<IExtensionWrapper>> GetInstalledExtensionsAsync(Microsoft.Windows.DevHome.SDK.ProviderType providerType, bool includeDisabledExtensions = false);

    Task<IEnumerable<IExtensionWrapper>> GetAllExtensionsAsync();

    Task SignalStopExtensionsAsync();

    Task<IEnumerable<AppExtension>> GetInstalledAppExtensionsAsync();

    public event EventHandler OnExtensionsChanged;

    public void EnableExtension(string extensionUniqueId);

    public void DisableExtension(string extensionUniqueId);
}
