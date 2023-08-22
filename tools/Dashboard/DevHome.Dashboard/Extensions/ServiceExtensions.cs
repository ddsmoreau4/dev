﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using DevHome.Dashboard.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DevHome.Settings.Extensions;
public static class ServiceExtensions
{
    public static IServiceCollection AddDashboard(this IServiceCollection services, HostBuilderContext context)
    {
        // View-models
        services.AddSingleton<DashboardViewModel>();

        return services;
    }
}
