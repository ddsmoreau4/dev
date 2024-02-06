﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DevHome.SetupFlow.Models;

/// <summary>
/// Messages to display in the loading screen.
/// </summary>
public class TaskMessages
{
    /// <summary>
    /// Gets or sets the message to display when the task is executing.
    /// </summary>
    public string Executing
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the message to display when the task finished successfully.
    /// </summary>
    public string Finished
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the message to display when the task has a non-recoverable error.
    /// </summary>
    public string Error
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the message that is displayed when the task requires a reboot of the machine.
    /// </summary>
    public string NeedsReboot
    {
        get; set;
    }

    public TaskMessages()
    {
    }

    public TaskMessages(string executingMessage, string finishedMessage, string errorMessage, string needsReboot)
    {
        Executing = executingMessage;
        Finished = finishedMessage;
        Error = errorMessage;
        NeedsReboot = needsReboot;
    }
}
