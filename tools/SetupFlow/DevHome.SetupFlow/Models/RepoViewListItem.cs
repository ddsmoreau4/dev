﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Windows.DevHome.SDK;

namespace DevHome.SetupFlow.Models;

/// <summary>
/// Class used to house the repo name and IsPrivate.  IsPrivate is used in views to determine
/// the glyph to show.  These are in a model for DataType in an ItemTemplate.
/// </summary>
public partial class RepoViewListItem : ObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the repo is a private repo.  If changed to "IsPublic" the
    /// values of the converters in the views need to change order.
    /// </summary>
    public bool IsPrivate { get; set; }

    /// <summary>
    /// Gets or sets the name of the repository
    /// </summary>
    public string RepoName { get; set; }

    public string OwningAccountName { get; set; }

    public string RepoDisplayName => Path.Join(OwningAccountName, RepoName);

    [ObservableProperty]
    private bool _isSelected;

    public RepoViewListItem()
    {
    }

    public RepoViewListItem(IRepository repo)
    {
        IsPrivate = repo.IsPrivate;
        RepoName = repo.DisplayName;
        OwningAccountName = repo.OwningAccountName;
    }
}
