﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHome.Common.Contracts;
using DevHome.Common.Extensions;
using DevHome.Common.Services;
using DevHome.Settings.Models;
using DevHome.Settings.Views;
using Microsoft.UI.Xaml;
using Microsoft.Windows.DevHome.SDK;
using Windows.Devices.Display.Core;
using Windows.Storage;
using Windows.System;

namespace DevHome.Settings.ViewModels;

public partial class ExtensionViewModel : ObservableRecipient
{
    private readonly Setting _setting;

    private readonly ExtensionsViewModel _extensionsViewModel;

    public ExtensionViewModel(Setting setting, ExtensionsViewModel extensionsViewModel)
    {
        _setting = setting;
        _extensionsViewModel = extensionsViewModel;
    }

    public string Path => _setting.Path;

    public string Header => _setting.Header;

    public string Description => _setting.Description;

    public bool HasToggleSwitch => _setting.HasToggleSwitch;

    public bool IsEnabled
    {
        get => _setting.IsExtensionEnabled;
        set => _setting.IsExtensionEnabled = value;
    }

    [RelayCommand]
    private void NavigateSettings()
    {
        _extensionsViewModel.Navigate(_setting.Path);
    }
}

public partial class ExtensionsViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<ExtensionViewModel> _settingsList = new ();

    public ExtensionsViewModel()
    {
        var pluginWrappers = Task.Run(() =>
        {
            var pluginService = Application.Current.GetService<IPluginService>();
            return pluginService.GetInstalledPluginsAsync(true);
        }).Result;

        var numberOfPlugins = pluginWrappers.Count();
        if (numberOfPlugins == 0)
        {
            return;
        }

        SettingsList.Clear();

        foreach (var pluginWrapper in pluginWrappers)
        {
            var setting = new Setting("Plugins/" + pluginWrapper.PluginClassId, pluginWrapper.PluginClassId, pluginWrapper.Name, string.Empty, true);
            SettingsList.Add(new ExtensionViewModel(setting, this));
        }
    }

    public void Navigate(string path)
    {
        // TODO: Navigate to Plugin's settings Adaptive Card
    }
}
