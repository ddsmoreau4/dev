﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Collections.Generic;

namespace DevHome.SetupFlow.Common.Contracts;
public interface ITaskArguments
{
    /// <summary>
    /// Create a list of arguments from this object.
    /// </summary>
    /// <returns>List of argument strings from this object</returns>
    public IList<string> ToArgumentList();
}
