﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Diagnostics.Tracing;
using DevHome.Telemetry;
using Microsoft.Diagnostics.Telemetry;
using Microsoft.Diagnostics.Telemetry.Internal;

namespace DevHome.Common.TelemetryEvents;

[EventData]
public class ExceptionEvent : EventBase
{
    public override PartA_PrivTags PartA_PrivTags => PrivTags.ProductAndServiceUsage;

    public int HResult { get; }

    public string Message
    {
        get; private set;
    }

    public ExceptionEvent(int hresult)
    {
        HResult = hresult;
        Message = string.Empty;
    }

    public ExceptionEvent(int hresult, string message)
    {
        HResult = hresult;
        Message = message;
    }

    public override void ReplaceSensitiveStrings(Func<string, string> replaceSensitiveStrings)
    {
        // Only storing HRESULT. No sensitive information.
    }
}
