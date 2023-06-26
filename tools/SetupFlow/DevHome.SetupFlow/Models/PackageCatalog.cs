﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Collections.Generic;

namespace DevHome.SetupFlow.Models;

// TODO Rename this class to PackageCollection to avoid confusion with the COM PackageCatalog class
// https://github.com/microsoft/devhome/issues/636

/// <summary>
/// Model class for a package catalog. A package catalog contains a list of
/// packages provided from the same source
/// </summary>
public class PackageCatalog
{
    public string Name
    {
        init; get;
    }

    public string Description
    {
        init; get;
    }

    public IReadOnlyCollection<IWinGetPackage> Packages
    {
        init; get;
    }
}
