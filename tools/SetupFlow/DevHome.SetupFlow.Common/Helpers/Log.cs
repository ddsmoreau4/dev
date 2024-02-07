﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security.Principal;
using DevHome.Logging;

namespace DevHome.SetupFlow.Common.Helpers;

#nullable enable

public class Log
{
    private static bool RunningAsAdmin
    {
        get
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    private static readonly ComponentLogger _logger = new(RunningAsAdmin ? "SetupFlow-Elevated" : "SetupFlow", "SetupFlow");

    public static Logger? Logger => _logger.Logger;

    // Component names to prepend to log strings
    public static class Component
    {
        public static readonly string Configuration = nameof(Configuration);
        public static readonly string AppManagement = nameof(AppManagement);
        public static readonly string DevDrive = nameof(DevDrive);
        public static readonly string RepoConfig = nameof(RepoConfig);

        public static readonly string Orchestrator = nameof(Orchestrator);
        public static readonly string MainPage = nameof(MainPage);
        public static readonly string Loading = nameof(Loading);
        public static readonly string Review = nameof(Review);
        public static readonly string Summary = nameof(Summary);

        public static readonly string IPCClient = nameof(IPCClient);
        public static readonly string IPCServer = nameof(IPCServer);
        public static readonly string Elevated = nameof(Elevated);
    }
}
