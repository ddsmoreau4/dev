﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Microsoft.Windows.Widgets.Hosts;
using static DevHome.Dashboard.Services.WidgetHostingService;

namespace DevHome.Dashboard.Services;

public interface IWidgetHostingService
{
    public Task<bool> EnsureWidgetServiceAsync();

    public WidgetServiceStates GetWidgetServiceState();

    public Task<WidgetHost> GetWidgetHostAsync();

    public Task<WidgetCatalog> GetWidgetCatalogAsync();
}
