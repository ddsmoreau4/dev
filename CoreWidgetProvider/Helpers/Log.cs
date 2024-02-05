﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DevHome.Logging;

#nullable enable

namespace CoreWidgetProvider.Helpers;

public class Log
{
    private static readonly ComponentLogger _logger = new ("CoreWidgetProvider");

    public static Logger? Logger() => _logger.Logger;
}
#nullable disable
