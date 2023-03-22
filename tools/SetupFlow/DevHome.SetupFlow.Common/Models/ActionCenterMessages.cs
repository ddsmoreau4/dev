﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

namespace DevHome.SetupFlow.Common.Models;

/// <summary>
/// Messages to show in the action center part of the loading screen when an item encountered an error
/// </summary>
public class ActionCenterMessages
{
    /// <summary>
    /// Gets or sets tain message to show to the user
    /// </summary>
    public string PrimaryMessage
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the seconday message to show to the user.
    /// </summary>
    public string SecondaryMessage
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the content for the primary button in the action center.
    /// </summary>
    public string PrimaryButtonContent
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the content for the secondary button in the action cneter.
    /// </summary>
    public string SecondaryButtonContent
    {
        get; set;
    }
}
