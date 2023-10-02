﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace DevHome.Common.Extensions;
public static class IEnumerableExtensions
{
    /// <summary>
    /// Creates a readonly collection for the enumerable input.
    /// </summary>
    /// <returns>Readonly collection.</returns>
    public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> items)
    {
        return new ReadOnlyCollectionBuilder<T>(items).ToReadOnlyCollection();
    }
}
