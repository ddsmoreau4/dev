﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Windows;
using Windows.UI.ViewManagement;

namespace DevHome.Stub;

[SupportedOSPlatform("Windows10.0.21200.0")]
public class FontSizeManager
{
    private static readonly UISettings Settings;
    private static ResourceDictionary _dictionary;
    private static IDictionary<object, double> _defaultValues;

    static FontSizeManager()
    {
        Settings = new UISettings();
    }

    public static void Register(ResourceDictionary dictionary)
    {
        try
        {
            Settings.TextScaleFactorChanged -= Settings_TextScaleFactorChanged;
        }
        catch (Exception)
        {
            // TO-DO: Log exception
        }

        _dictionary = dictionary;
        _defaultValues = dictionary.Cast<DictionaryEntry>().Where(entry => entry.Value is double)
                                    .ToDictionary(entry => entry.Key, entry => (double)entry.Value);

        UpdateValues(Settings.TextScaleFactor);

        try
        {
            Settings.TextScaleFactorChanged += Settings_TextScaleFactorChanged;
        }
        catch (Exception)
        {
            // TO-DO: Log exception
        }
    }

    private static void Settings_TextScaleFactorChanged(UISettings sender, object args)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(() => UpdateValues(sender.TextScaleFactor));
            return;
        }

        UpdateValues(sender.TextScaleFactor);
    }

    private static void UpdateValues(double scaleFactor)
    {
        foreach (var key in _defaultValues.Keys)
        {
            _dictionary[key] = _defaultValues[key] * scaleFactor;
        }
    }
}
