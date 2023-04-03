﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Threading.Tasks;
using DevHome.SetupFlow.AppManagement.Exceptions;
using DevHome.SetupFlow.AppManagement.Services;
using DevHome.SetupFlow.ComInterop.Projection.WindowsPackageManager;
using DevHome.SetupFlow.Common.Models;
using DevHome.SetupFlow.Common.Services;
using DevHome.SetupFlow.ElevatedComponent;
using DevHome.Telemetry;
using Microsoft.Management.Deployment;
using Windows.Foundation;

namespace DevHome.SetupFlow.AppManagement.Models;

public class InstallPackageTask : ISetupTask
{
    private readonly ILogger _logger;
    private readonly IWindowsPackageManager _wpm;
    private readonly WinGetPackage _package;
    private readonly ISetupFlowStringResource _stringResource;
    private readonly WindowsPackageManagerFactory _wingetFactory;
    private readonly Lazy<bool> _requiresElevation;

    private InstallPackageException _installPackageException;

    public bool RequiresAdmin => _requiresElevation.Value;

    // As we don't have this information available for each package before
    // installation in the WinGet COM API, simply assume that any package
    // installation may need a reboot by default.
    public bool RequiresReboot { get; set; } = true;

    // May potentially be moved to a central list in the future.
    public bool WasInstallSuccessful
    {
        get; private set;
    }

    public bool DependsOnDevDriveToBeInstalled
    {
        get;
    }

    public InstallPackageTask(
        ILogger logger,
        IWindowsPackageManager wpm,
        ISetupFlowStringResource stringResource,
        WindowsPackageManagerFactory wingetFactory,
        WinGetPackage package)
    {
        _logger = logger;
        _wpm = wpm;
        _stringResource = stringResource;
        _wingetFactory = wingetFactory;
        _package = package;
        _requiresElevation = new (RequiresElevation);
    }

    public TaskMessages GetLoadingMessages()
    {
        return new TaskMessages
        {
            Executing = _stringResource.GetLocalized(StringResourceKey.InstallingPackage, _package.Name),
            Error = _stringResource.GetLocalized(StringResourceKey.InstallPackageError, _package.Name),
            Finished = _stringResource.GetLocalized(StringResourceKey.InstalledPackage, _package.Name),
            NeedsReboot = _stringResource.GetLocalized(StringResourceKey.InstalledPackageReboot, _package.Name),
        };
    }

    public ActionCenterMessages GetErrorMessages()
    {
        return new ()
        {
            PrimaryMessage = _stringResource.GetLocalized(StringResourceKey.InstallPackageError, _package.Name),
        };
    }

    public ActionCenterMessages GetRebootMessage()
    {
        return new ()
        {
            PrimaryMessage = _stringResource.GetLocalized(StringResourceKey.InstalledPackageReboot, _package.Name),
        };
    }

    IAsyncOperation<TaskFinishedState> ISetupTask.Execute()
    {
        return Task.Run(async () =>
        {
            try
            {
                var installResult = await _wpm.InstallPackageAsync(_package);
                RequiresReboot = installResult.RebootRequired;
                WasInstallSuccessful = true;
                return TaskFinishedState.Success;
            }
            catch (InstallPackageException e)
            {
                _installPackageException = e;
                _logger.LogError(nameof(InstallPackageTask), LogLevel.Local, $"Failed to install package with status {e.Status} and installer error code {e.InstallerErrorCode}");
                return TaskFinishedState.Failure;
            }
            catch (Exception e)
            {
                _logger.LogError(nameof(InstallPackageTask), LogLevel.Local, $"Exception thrown while installing package: {e.Message}");
                return TaskFinishedState.Failure;
            }
        }).AsAsyncOperation();
    }

    IAsyncOperation<TaskFinishedState> ISetupTask.ExecuteAsAdmin(IElevatedComponentFactory elevatedComponentFactory)
    {
        return Task.Run(async () =>
        {
            var packageInstaller = elevatedComponentFactory.CreatePackageInstaller();
            var installResult = await packageInstaller.InstallPackage(_package.Id, _package.CatalogName);
            WasInstallSuccessful = installResult.InstallSucceeded;
            return installResult.InstallSucceeded ? TaskFinishedState.Success : TaskFinishedState.Failure;
        }).AsAsyncOperation();
    }

    private string GetErrorReason()
    {
        return _installPackageException?.Status switch
        {
            InstallResultStatus.BlockedByPolicy =>
                _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorBlockedByPolicy),
            InstallResultStatus.InternalError =>
                _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorInternalError),
            InstallResultStatus.DownloadError =>
                _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorDownloadError),
            InstallResultStatus.InstallError =>
                _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorInstallError, $"0x{_installPackageException.InstallerErrorCode.ToString("X", CultureInfo.InvariantCulture)}"),
            InstallResultStatus.NoApplicableInstallers =>
                _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorNoApplicableInstallers),
            _ => _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorUnknownError),
        };
    }

    private bool RequiresElevation()
    {
        var options = _wingetFactory.CreateInstallOptions();
        options.PackageInstallScope = PackageInstallScope.User;
        return _package.RequiresElevation(options);
    }
}
