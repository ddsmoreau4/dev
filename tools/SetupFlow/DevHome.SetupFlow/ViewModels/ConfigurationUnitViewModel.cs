﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using DevHome.SetupFlow.Models;

namespace DevHome.SetupFlow.ViewModels;

public class ConfigurationUnitViewModel
{
    private readonly ConfigurationUnit _configurationUnit;

    public ConfigurationUnitViewModel(ConfigurationUnit configurationUnit)
    {
        _configurationUnit = configurationUnit;
    }

    public string Id => _configurationUnit.Id;

    public string Title => GetTitle();

    public string SubTitle => GetSubTitle();

    public string Type => _configurationUnit.Type;

    public string Description => _configurationUnit.Description;

    public string ModuleName => _configurationUnit.ModuleName;

    public string ModuleDescription => _configurationUnit.ModuleDescription;

    public string Author => _configurationUnit.Author;

    public string Intent => _configurationUnit.Intent;

    public IList<string> Dependencies => _configurationUnit.Dependencies;

    public IList<KeyValuePair<string, string>> Settings => _configurationUnit.Settings;

    public IList<KeyValuePair<string, string>> Metadata => _configurationUnit.Metadata;

    public string GetTitle()
    {
        return $"Resource: {Type}/{ModuleName}";
    }

    public string GetSubTitle()
    {
        return $"Description: {Description}";
    }
}
