﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

namespace DevHome.SetupFlow.ElevatedComponent.Helpers;

/// <summary>
/// The result of an task run in the elevated process.
/// </summary>
/// <remarks>
/// This exists only because it is easier to have this ad-hoc type that
/// we can project with CsWinRT than to make everything else fit on
/// to CsWinRT requirements.
/// </remarks>
public interface IElevatedTaskResult
{
    /// <summary>
    /// Gets a value indicating whether we actually got to attempting to execute the task.
    /// </summary>
    /// <remarks>
    /// This is intended for failure during pre-processing work that should not fail as it was already
    /// validated in the main process, but we have to duplicate here before starting actual work.
    /// For example, for installing an app, this would include finding the app in the catalog.
    /// A false value here would be unexpected.
    /// </remarks>
    public bool TaskAttempted
    {
        get;
    }

    public bool TaskSucceeded
    {
        get;
    }

    public bool RebootRequired
    {
        get;
    }
}
