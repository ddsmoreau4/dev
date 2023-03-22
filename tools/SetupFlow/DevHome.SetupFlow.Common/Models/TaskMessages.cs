﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevHome.SetupFlow.Common.Models;

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
    /// Gets or sets the message to display when the task finished, but needs attention.
    /// </summary>
    public string NeedsAttention
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the content of the primary button for action center items.
    /// </summary>
    public string PrimaryBuittonContent
    {
        get; set;
    }

    public TaskMessages()
    {
    }

    public TaskMessages(string executingMessage, string finishedMessage, string errorMessage, string needsAttention)
    {
        Executing = executingMessage;
        Finished = finishedMessage;
        Error = errorMessage;
        NeedsAttention = needsAttention;
    }
}
