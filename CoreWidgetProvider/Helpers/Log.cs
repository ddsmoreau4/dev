﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using DevHome.Logging;

#nullable enable

namespace CoreWidgetProvider.Helpers;

public class Log
{
    private static readonly ComponentLogger _logger = new("CoreWidgetProvider");

    public static Logger? Logger() => _logger.Logger;
}
#nullable disable
