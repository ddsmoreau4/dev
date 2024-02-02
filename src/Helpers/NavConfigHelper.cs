﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Text.Json.Serialization;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace DevHome.Helpers;

internal sealed class NavConfig
{
    [JsonPropertyName("navMenu")]
    public NavMenu NavMenu { get; set; }

    [JsonPropertyName("experiments")]
    public string[] ExperimentIds { get; set; }
}

internal sealed class NavMenu
{
    [JsonPropertyName("groups")]
    public Group[] Groups { get; set; }
}

internal sealed class Group
{
    [JsonPropertyName("identity")]
    public string Identity { get; set; }

    [JsonPropertyName("tools")]
    public Tool[] Tools { get; set; }
}

internal sealed class Tool
{
    [JsonPropertyName("identity")]
    public string Identity { get; set; }

    [JsonPropertyName("assembly")]
    public string Assembly { get; set; }

    [JsonPropertyName("viewFullName")]
    public string ViewFullName { get; set; }

    [JsonPropertyName("viewModelFullName")]
    public string ViewModelFullName { get; set; }

    [JsonPropertyName("icon")]
    public string Icon { get; set; }

    [JsonPropertyName("experimentId")]
    public string ExperimentId { get; set; }
}

// Uses .NET's JSON source generator support for serializing / deserializing NavConfig to get some perf gains at startup.
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(NavConfig))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext
{
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
