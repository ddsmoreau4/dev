﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using DevHome.Telemetry;
using Microsoft.Diagnostics.Telemetry;
using Microsoft.Diagnostics.Telemetry.Internal;

namespace DevHome.Common.TelemetryEvents.SetupFlow;

[EventData]
public class FinalRepoResult
{
    public string ProviderName { get; }

    public AddKind AddKind { get; }

    public CloneLocationKind CloneLocationKind { get; }

    public FinalRepoResult(string providerName, AddKind addKind, CloneLocationKind cloneLocationKind)
    {
        ProviderName = providerName;
        AddKind = addKind;
        CloneLocationKind = cloneLocationKind;
    }
}

[EventData]
public class RepoToolFinalReposToAddEvent : EventBase
{
    public override PartA_PrivTags PartA_PrivTags => PrivTags.ProductAndServiceUsage;

    public List<FinalRepoResult> FinalAddedRepos { get; }

    public RepoToolFinalReposToAddEvent(List<FinalRepoResult> addedRepos)
    {
        FinalAddedRepos = new List<FinalRepoResult>();
        FinalAddedRepos.AddRange(addedRepos);
    }

    public override void ReplaceSensitiveStrings(Func<string, string> replaceSensitiveStrings)
    {
        // No sensitive strings to replace.
    }
}
