﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Diagnostics;
using System.Text.Json;
using DevHome.SetupFlow.Common.Contracts;
using DevHome.SetupFlow.Common.Elevation;
using DevHome.SetupFlow.ElevatedComponent;

namespace DevHome.SetupFlow.ElevatedServer;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        var mappedFileName = args[0];
        var initEventName = args[1];
        var completionSemaphoreName = args[2];
        var factory = new ElevatedComponentFactory(args, 3);

        try
        {
            IPCSetup.CompleteRemoteObjectInitialization<IElevatedComponentFactory>(0, factory, mappedFileName, initEventName, completionSemaphoreName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Remote factory initialization failed: {ex}");
            IPCSetup.CompleteRemoteObjectInitialization<IElevatedComponentFactory>(ex.HResult, null, mappedFileName, initEventName, completionSemaphoreName);
            throw;
        }
    }
}
