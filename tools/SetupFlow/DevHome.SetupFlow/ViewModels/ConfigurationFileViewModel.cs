﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHome.Common.Extensions;
using DevHome.Common.TelemetryEvents.SetupFlow;
using DevHome.SetupFlow.Common.Exceptions;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.SetupFlow.Models;
using DevHome.SetupFlow.Services;
using DevHome.Telemetry;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace DevHome.SetupFlow.ViewModels;

public partial class ConfigurationFileViewModel : SetupPageViewModelBase
{
    private readonly IDesiredStateConfiguration _dsc;

    public List<ConfigureTask> TaskList { get; } = new List<ConfigureTask>();

    /// <summary>
    /// Configuration file
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Content))]
    private Configuration _configuration;

    /// <summary>
    /// Store the value for whether the agreements are read.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfigureAsAdminCommand))]
    [NotifyCanExecuteChangedFor(nameof(ConfigureAsNonAdminCommand))]
    private bool _readAndAgree;

    public ConfigurationFileViewModel(
        ISetupFlowStringResource stringResource,
        IDesiredStateConfiguration dsc,
        SetupFlowOrchestrator orchestrator)
        : base(stringResource, orchestrator)
    {
        _dsc = dsc;

        // Configure navigation bar
        NextPageButtonText = StringResource.GetLocalized(StringResourceKey.SetUpButton);
        IsStepPage = false;
    }

    partial void OnReadAndAgreeChanged(bool value)
    {
        Log.Logger?.ReportInfo(Log.Component.Configuration, $"Read and agree changed. Value: {value}");
        CanGoToNextPage = value;
        Orchestrator.NotifyNavigationCanExecuteChanged();
    }

    /// <summary>
    /// Gets the configuration file content
    /// </summary>
    public string Content => Configuration.Content;

    [RelayCommand(CanExecute = nameof(ReadAndAgree))]
    public async Task ConfigureAsAdminAsync()
    {
        foreach (var task in TaskList)
        {
            task.RequiresAdmin = true;
        }

        TelemetryFactory.Get<ITelemetry>().Log("ConfigurationButton_Click", LogLevel.Critical, new ConfigureCommandEvent(true), Orchestrator.ActivityId);
        try
        {
            await Orchestrator.InitializeElevatedServerAsync();
            await Orchestrator.GoToNextPage();
        }
        catch (Exception e)
        {
            Log.Logger?.ReportError(Log.Component.Configuration, $"Failed to initialize elevated process.", e);
        }
    }

    [RelayCommand(CanExecute = nameof(ReadAndAgree))]
    public async Task ConfigureAsNonAdminAsync()
    {
        TelemetryFactory.Get<ITelemetry>().Log("ConfigurationButton_Click", LogLevel.Critical, new ConfigureCommandEvent(false), Orchestrator.ActivityId);
        await Orchestrator.GoToNextPage();
    }

    /// <summary>
    /// Open file picker to select a YAML configuration file.
    /// </summary>
    /// <returns>True if a YAML configuration file was selected, false otherwise</returns>
    public async Task<bool> PickConfigurationFileAsync()
    {
        // Get the application root window.
        var mainWindow = Application.Current.GetService<WindowEx>();

        // Create and configure file picker
        Log.Logger?.ReportInfo(Log.Component.Configuration, "Launching file picker to select configuration file");
        var file = await mainWindow.OpenFilePickerAsync(Log.Logger, ("*.yaml;*.yml", StringResource.GetLocalized(StringResourceKey.FilePickerFileTypeOption, "YAML")));

        // Check if a file was selected
        if (file == null)
        {
            Log.Logger?.ReportInfo(Log.Component.Configuration, "No configuration file selected");
        }
        else
        {
            try
            {
                Log.Logger?.ReportInfo(Log.Component.Configuration, $"Selected file: {file.Path}");
                Configuration = new(file.Path);
                Orchestrator.FlowTitle = StringResource.GetLocalized(StringResourceKey.ConfigurationViewTitle, Configuration.Name);
                await _dsc.ValidateConfigurationAsync(file.Path, Orchestrator.ActivityId);
                TaskList.Add(new(StringResource, _dsc, file, Orchestrator.ActivityId));
                return true;
            }
            catch (OpenConfigurationSetException e)
            {
                Log.Logger?.ReportError(Log.Component.Configuration, $"Opening configuration set failed.", e);
                await mainWindow.ShowErrorMessageDialogAsync(
                    StringResource.GetLocalized(StringResourceKey.ConfigurationViewTitle, file.Name),
                    GetErrorMessage(e),
                    StringResource.GetLocalized(StringResourceKey.Close));
            }
            catch (Exception e)
            {
                Log.Logger?.ReportError(Log.Component.Configuration, $"Unknown error while opening configuration set.", e);

                await mainWindow.ShowErrorMessageDialogAsync(
                    file.Name,
                    StringResource.GetLocalized(StringResourceKey.ConfigurationFileOpenUnknownError),
                    StringResource.GetLocalized(StringResourceKey.Close));
            }
        }

        return false;
    }

    private string GetErrorMessage(OpenConfigurationSetException exception)
    {
        switch (exception.ResultCode.HResult)
        {
            case WinGetConfigurationException.WingetConfigErrorInvalidFieldType:
                return StringResource.GetLocalized(StringResourceKey.ConfigurationFieldInvalidType, exception.Field);
            case WinGetConfigurationException.WingetConfigErrorInvalidFieldValue:
                return StringResource.GetLocalized(StringResourceKey.ConfigurationFieldInvalidValue, exception.Field, exception.Value);
            case WinGetConfigurationException.WingetConfigErrorMissingField:
                return StringResource.GetLocalized(StringResourceKey.ConfigurationFieldMissing, exception.Field);
            case WinGetConfigurationException.WingetConfigErrorUnknownConfigurationFileVersion:
                return StringResource.GetLocalized(StringResourceKey.ConfigurationFileVersionUnknown, exception.Value);
            case WinGetConfigurationException.WingetConfigErrorInvalidConfigurationFile:
            case WinGetConfigurationException.WingetConfigErrorInvalidYaml:
            default:
                return StringResource.GetLocalized(StringResourceKey.ConfigurationFileInvalid);
        }
    }
}
