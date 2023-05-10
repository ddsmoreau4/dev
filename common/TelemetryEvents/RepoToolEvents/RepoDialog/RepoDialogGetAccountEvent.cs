﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using DevHome.Telemetry;
using Microsoft.Diagnostics.Telemetry;
using Microsoft.Diagnostics.Telemetry.Internal;

namespace DevHome.Common.TelemetryEvents.RepoToolEvents.RepoDialog;

[EventData]
public class RepoDialogGetAccountEvent : EventBase
{
    public override PartA_PrivTags PartA_PrivTags => PrivTags.ProductAndServiceUsage;

    public string ProviderName
    {
        get;
    }

    public bool AlreadyLoggedIn
    {
        get;
    }

    public RepoDialogGetAccountEvent(string providerName, bool alreadyLoggedIn)
    {
        ProviderName = providerName;
        AlreadyLoggedIn = alreadyLoggedIn;
    }

    public override void ReplaceSensitiveStrings(Func<string, string> replaceSensitiveStrings)
    {
        // No sensitive data held
    }
}
