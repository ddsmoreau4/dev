﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using DevHome.Common.Services;
using DevHome.Common.TelemetryEvents.DeveloperId;
using DevHome.Common.Views;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.Telemetry;
using Microsoft.UI.Xaml;
using Microsoft.Windows.DevHome.SDK;
using Windows.Foundation;

namespace DevHome.SetupFlow.Models;

/// <summary>
/// A collection of all repository providers found by Dev Home.
/// </summary>
/// <remarks>
/// This class only uses providers that implement IDeveloperIdProvider and IRepositoryProvider.
/// </remarks>
internal sealed class RepositoryProviders
{
    /// <summary>
    /// Hold all providers and organize by their names.
    /// </summary>
    private readonly Dictionary<string, RepositoryProvider> _providers = new();

    public string DisplayName(string providerName)
    {
        return _providers.GetValueOrDefault(providerName)?.DisplayName ?? string.Empty;
    }

    public RepositoryProviders(IEnumerable<IExtensionWrapper> extensionWrappers)
    {
        _providers = extensionWrappers.ToDictionary(extensionWrapper => extensionWrapper.Name, extensionWrapper => new RepositoryProvider(extensionWrapper));
    }

    public void StartAllExtensions()
    {
        foreach (var extensionWrapper in _providers.Values)
        {
            extensionWrapper.StartIfNotRunning();
        }
    }

    /// <summary>
    /// Starts a provider if it isn't running.
    /// </summary>
    /// <param name="providerName">The provider to start.</param>
    public void StartIfNotRunning(string providerName)
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Starting RepositoryProvider {providerName}");
        if (_providers.TryGetValue(providerName, out var value))
        {
            value.StartIfNotRunning();
        }
    }

    /// <summary>
    /// Goes through all providers to figure out if they can make a repo from a Uri.
    /// </summary>
    /// <param name="uri">The Uri to parse.</param>
    /// <returns>If a provider was found that can parse the Uri then (providerName, repository) if not
    /// (string.empty, null)</returns>
    public (string, IRepository) GetRepositoryFromUri(Uri uri)
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Parsing repository from URI {uri}");
        foreach (var provider in _providers)
        {
            provider.Value.StartIfNotRunning();
            Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Attempting to parse using provider {provider.Key}");
            var repository = provider.Value.GetRepositoryFromUri(uri);
            if (repository != null)
            {
                Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Repository parsed to {repository.DisplayName} owned by {repository.OwningAccountName}");
                return (provider.Value.DisplayName, repository);
            }
        }

        return (string.Empty, null);
    }

    /// <summary>
    /// Queries each provider to figure out if it can support the URI and can clone from it.
    /// </summary>
    /// <param name="uri">The uri that points to a remote repository</param>
    /// <returns>THe provider that can clone the repo.  Otherwise null.</returns>
    public RepositoryProvider CanAnyProviderSupportThisUri(Uri uri)
    {
        foreach (var provider in _providers)
        {
            provider.Value.StartIfNotRunning();
            var isUriSupported = provider.Value.IsUriSupported(uri);
            if (isUriSupported)
            {
                return provider.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the login UI for the provider with the name providerName
    /// </summary>
    /// <param name="providerName">The provider to search for.</param>
    /// <param name="elementTheme">The theme to use for the ui.</param>
    /// <returns>The ui to show.  Can be null.</returns>
    public ExtensionAdaptiveCardPanel GetLoginUi(string providerName, ElementTheme elementTheme)
    {
        TelemetryFactory.Get<ITelemetry>().Log(
                                                "EntryPoint_DevId_Event",
                                                LogLevel.Critical,
                                                new EntryPointEvent(EntryPointEvent.EntryPoint.SetupFlow));
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Getting login UI {providerName}");
        return _providers.GetValueOrDefault(providerName)?.GetLoginUi(elementTheme);
    }

    /// <summary>
    /// Gets the display names of all providers.
    /// </summary>
    /// <returns>A collection of display names.</returns>
    public IEnumerable<string> GetAllProviderNames()
    {
        return _providers.Keys;
    }

    public IRepositoryProvider GetSDKProvider(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var repoProvider))
        {
            return repoProvider.GetProvider();
        }

        return null;
    }

    public RepositoryProvider GetProvider(string providerName)
    {
        if (_providers.TryGetValue(providerName, out var repoProvider))
        {
            return repoProvider;
        }

        return null;
    }

    /// <summary>
    /// Gets all logged in accounts for a specific provider.
    /// </summary>
    /// <param name="providerName">The provider to use.  Must match the display name of the provider</param>
    /// <returns>A collection of developer Ids of all logged in users.  Can be empty.</returns>
    public IEnumerable<IDeveloperId> GetAllLoggedInAccounts(string providerName)
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Getting all logged in accounts for repository provider {providerName}");
        return _providers.GetValueOrDefault(providerName)?.GetAllLoggedInAccounts() ?? new List<IDeveloperId>();
    }

    public AuthenticationExperienceKind GetAuthenticationExperienceKind(string providerName)
    {
        return _providers.GetValueOrDefault(providerName)?.GetAuthenticationExperienceKind() ?? AuthenticationExperienceKind.CardSession;
    }

    /// <summary>
    /// Gets all the repositories for an account and provider.  The account will be logged in if they aren't already.
    /// </summary>
    /// <param name="providerName">The specific provider.  Must match the display name of a provider</param>
    /// <param name="developerId">The account to look for.  May not be logged in.</param>
    /// <returns>All the repositories for an account and provider.</returns>
    public IEnumerable<IRepository> GetAllRepositories(string providerName, IDeveloperId developerId)
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Getting all repositories for repository provider {providerName}");
        return _providers.GetValueOrDefault(providerName)?.GetAllRepositories(developerId) ?? new List<IRepository>();
    }
}
