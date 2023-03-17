﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using DevHome.SetupFlow.Common.Models;
using Windows.Foundation;

namespace DevHome.SetupFlow.DevDrive.Models;

internal class CreateDevDriveTask : ISetupTask
{
    public bool RequiresAdmin => throw new NotImplementedException();

    public bool RequiresReboot => false;

    public LoadingMessages GetLoadingMessages() => throw new NotImplementedException();

    IAsyncOperation<TaskFinishedState> ISetupTask.Execute() => throw new NotImplementedException();
}
