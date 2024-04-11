﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DevHome.Common.Environments.Models;
using DevHome.Common.Environments.Services;
using DevHome.Common.Services;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Windows.DevHome.SDK;
using Serilog;
using WinUIEx;

namespace DevHome.Environments.ViewModels;

/// <summary>
/// Represents a view model for the create compute system operation that will appear in the UI
/// </summary>
public partial class CreateComputeSystemOperationViewModel : ComputeSystemCardBase
{
    private readonly ILogger _log = Log.ForContext("SourceContext", nameof(ComputeSystemViewModel));

    private readonly IComputeSystemManager _computeSystemManager;

    private readonly WindowEx _windowEx;

    private readonly StringResource _stringResource;

    /// <summary>
    /// This glyph can be found in the Fluent UI MDL2 Assets font that ships with Windows.
    /// Check the Win UI 3 gallery for the visual representation of the glyph. It is
    /// the trash can icon.
    /// </summary>
    private readonly string _cancelationUniCodeForGlyph = "\uE74D";

    public string EnvironmentName => Operation.EnvironmentName;

    /// <summary>
    /// Callback action to remove the view model from the view.
    /// </summary>
    private readonly Func<ComputeSystemCardBase, bool> _removalAction;

    public CreateComputeSystemOperation Operation { get; }

    public CreateComputeSystemOperationViewModel(
        IComputeSystemManager computeSystemManager,
        StringResource stringResource,
        WindowEx windowsEx,
        Func<ComputeSystemCardBase, bool> removalAction,
        CreateComputeSystemOperation operation)
    {
        IsCreationInProgress = true;
        _windowEx = windowsEx;
        _removalAction = removalAction;
        _stringResource = stringResource;
        _computeSystemManager = computeSystemManager;
        Operation = operation;

        var providerDetails = Operation.ProviderDetails;
        ProviderDisplayName = providerDetails.ComputeSystemProvider.DisplayName;
        IsCreateComputeSystemOperation = true;

        // Hook up event handlers to the operation
        Operation.Completed += OnOperationCompleted;
        Operation.Progress += OnOperationProgressChanged;

        // make sure the last update appears in the UI if the operation is already completed at this point
        UpdateUiMessage(Operation.LastProgressMessage, Operation.LastProgressPercentage);

        // Update the state of the card
        State = ComputeSystemState.Creating;
        StateColor = CardStateColor.Caution;

        // Setup the button to remove the the view model from the UI and the header Image
        DotOperations = new ObservableCollection<OperationsViewModel>() { new(_stringResource.GetLocalized("RemoveButtonTextForCreateComputeSystem"), _cancelationUniCodeForGlyph, RemoveViewModelFromUI) };
        HeaderImage = CardProperty.ConvertMsResourceToIcon(providerDetails.ComputeSystemProvider.Icon, providerDetails.ExtensionWrapper.PackageFullName);

        // If the operation is already completed update the status
        if (operation.CreateComputeSystemResult != null)
        {
            UpdateStatusIfCompleted(operation.CreateComputeSystemResult);
        }
    }

    private void OnOperationCompleted(object sender, CreateComputeSystemResult createComputeSystemResult)
    {
        UpdateStatusIfCompleted(createComputeSystemResult);
    }

    private void UpdateStatusIfCompleted(CreateComputeSystemResult createComputeSystemResult)
    {
        _windowEx.DispatcherQueue.TryEnqueue(() =>
        {
            // Update the creation status
            IsCreationInProgress = false;
            if (createComputeSystemResult.Result.Status == ProviderOperationStatus.Success)
            {
                UpdateUiMessage(_stringResource.GetLocalized("SuccessMessageForCreateComputeSystem", ProviderDisplayName));
                ComputeSystem = new(createComputeSystemResult.ComputeSystem);
                State = ComputeSystemState.Created;
                StateColor = CardStateColor.Success;
            }
            else
            {
                UpdateUiMessage(_stringResource.GetLocalized("FailureMessageForCreateComputeSystem", createComputeSystemResult.Result.DisplayMessage));
                State = ComputeSystemState.Unknown;
                StateColor = CardStateColor.Failure;
            }

            RemoveEventHandlers();
        });
    }

    private void OnOperationProgressChanged(object sender, CreateComputeSystemProgressEventArgs args)
    {
        UpdateUiMessage(args.Status, args.PercentageCompleted);
    }

    public void RemoveEventHandlers()
    {
        Operation.Completed -= OnOperationCompleted;
        Operation.Progress -= OnOperationProgressChanged;
    }

    private void UpdateUiMessage(string operationStatus, uint percentage = 0)
    {
        _windowEx.DispatcherQueue.TryEnqueue(() =>
        {
            if (operationStatus == null)
            {
                return;
            }

            var percentageString = percentage == 0 ? string.Empty : $"({percentage}%)";
            UiMessageToDisplay = _stringResource.GetLocalized("CreationStatusTextForCreateEnvironmentFlow", $"{operationStatus} {percentageString}");
        });
    }

    private void RemoveViewModelFromUI()
    {
        _windowEx.DispatcherQueue.TryEnqueue(() =>
        {
            _removalAction(this);
            RemoveEventHandlers();
            Operation.CancelOperation();
            _computeSystemManager.RemoveOperation(Operation);
        });
    }
}
