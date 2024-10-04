﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace DevHome.Database.Services;

public interface ISchemaAccessFactory
{
    ISchemaAccessor GenerateSchemaAccessor();
}
