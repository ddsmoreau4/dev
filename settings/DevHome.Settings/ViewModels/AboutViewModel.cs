﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using CommunityToolkit.Mvvm.ComponentModel;
using DevHome.Common.Extensions;
using DevHome.Common.Services;
using Microsoft.UI.Xaml;

namespace DevHome.Settings.ViewModels;

public partial class AboutViewModel : ObservableObject
{
    [ObservableProperty]
    private string _versionDescription;

    public AboutViewModel()
    {
        _versionDescription = GetVersionDescription();
    }

    private static string GetVersionDescription()
    {
        var appInfoService = Application.Current.GetService<IAppInfoService>();
        var version = appInfoService.GetAppVersion();

        return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }
}
