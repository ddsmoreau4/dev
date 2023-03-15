﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using DevHome.Common.Services;
using DevHome.SetupFlow.AppManagement.Services;
using DevHome.SetupFlow.AppManagement.ViewModels;
using DevHome.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace DevHome.SetupFlow.UnitTest;
public class BaseSetupFlowTest
{
    protected Mock<IWindowsPackageManager>? WindowsPackageManager { get; private set; }

    protected IHost? TestHost { get; private set; }

    [TestInitialize]
    public void TestInitialize()
    {
        WindowsPackageManager = new Mock<IWindowsPackageManager>();
        TestHost = CreateTestHost();
    }

    private IHost CreateTestHost(Action<IServiceCollection>? spAction = null)
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Configure string resource
                var stringResource = new Mock<IStringResource>();
                stringResource
                    .Setup(sr => sr.GetLocalized(It.IsAny<string>(), It.IsAny<object[]>()))
                    .Returns((string key, object[] args) => key);

                // Common services
                services.AddSingleton<ILogger>(new Mock<ILogger>().Object);
                services.AddSingleton<IStringResource>(stringResource.Object);

                // App-management view models
                services.AddTransient<PackageViewModel>();
                services.AddTransient<PackageCatalogViewModel>();
                services.AddTransient<SearchViewModel>();

                // App-management services
                services.AddSingleton<IWindowsPackageManager>(WindowsPackageManager!.Object);
                services.AddSingleton<WinGetPackageJsonDataSource>();

                // Custom services
                spAction?.Invoke(services);
            }).Build();
    }
}
