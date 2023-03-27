﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Globalization;
using System.Threading.Tasks;
using DevHome.SetupFlow.AppManagement.Exceptions;
using DevHome.SetupFlow.AppManagement.Services;
using DevHome.SetupFlow.Common.Models;
using DevHome.SetupFlow.Common.Services;
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

    private InstallPackageException _installPackageException;

    public bool RequiresAdmin => false;

    // As we don't have this information available for each package in the WinGet COM API,
    // simply assume that any package installation may need a reboot.
    public bool RequiresReboot => true;

    public LoadingMessages GetLoadingMessages()
    {
        return new LoadingMessages
        {
            Executing = _stringResource.GetLocalized(StringResourceKey.InstallingPackage, _package.Name),
            Finished = _stringResource.GetLocalized(StringResourceKey.InstallingPackage, _package.Name),
            Error = _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorWithReason, _package.Name, GetErrorReason()),
        };
    }

    public InstallPackageTask(
        ILogger logger,
        IWindowsPackageManager wpm,
        ISetupFlowStringResource stringResource,
        WinGetPackage package)
    {
        _logger = logger;
        _wpm = wpm;
        _stringResource = stringResource;
        _package = package;
    }

    IAsyncOperation<TaskFinishedState> ISetupTask.Execute()
    {
        return Task.Run(async () =>
        {
            try
            {
                await _wpm.InstallPackageAsync(_package);
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
                _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorInstallError, _installPackageException.InstallerErrorCode.ToString("X", CultureInfo.InvariantCulture)),
            InstallResultStatus.NoApplicableInstallers =>
                _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorNoApplicableInstallers),
            _ => _stringResource.GetLocalized(StringResourceKey.InstallPackageErrorUnknownError),
        };
    }
}
