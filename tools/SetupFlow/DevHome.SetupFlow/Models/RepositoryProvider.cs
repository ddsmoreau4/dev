﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveCards.Rendering.WinUI3;
using DevHome.Common.Renderers;
using DevHome.Common.Services;
using DevHome.Common.TelemetryEvents.SetupFlow;
using DevHome.Common.Views;
using DevHome.Logging;
using DevHome.Settings.Views;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.Telemetry;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.DevHome.SDK;
using Windows.Storage;

namespace DevHome.SetupFlow.Models;

/// <summary>
/// Object that holds a reference to the providers in a plugin.
/// This needs to be changed to handle multiple accounts per provider.
/// </summary>
internal class RepositoryProvider
{
    /// <summary>
    /// Wrapper for the plugin that is providing a repository and developer id.
    /// </summary>
    /// <remarks>
    /// The plugin is not started in the constructor.  It is started when StartIfNotRunningAsync is called.
    /// This is for lazy loading and starting and prevents all plugins from starting all at once.
    /// </remarks>
    private readonly IPluginWrapper _pluginWrapper;

    /// <summary>
    /// All the repositories for an account.
    /// </summary>
    private Lazy<IEnumerable<IRepository>> _repositories = new ();

    /// <summary>
    /// The DeveloperId provider used to log a user into an account.
    /// </summary>
    private IDeveloperIdProvider _devIdProvider;

    /// <summary>
    /// Provider used to clone a repsitory.
    /// </summary>
    private IRepositoryProvider _repositoryProvider;

    public RepositoryProvider(IPluginWrapper pluginWrapper)
    {
        _pluginWrapper = pluginWrapper;
    }

    public string DisplayName => _repositoryProvider.DisplayName;

    /// <summary>
    /// Starts the plugin if it isn't running.
    /// </summary>
    public void StartIfNotRunning()
    {
        // The task.run inside GetProvider makes a deadlock when .Result is called.
        // https://stackoverflow.com/a/17248813.  Solution is to wrap in Task.Run().
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, "Starting DevId and Repository provider plugins");
        _devIdProvider = Task.Run(() => _pluginWrapper.GetProviderAsync<IDeveloperIdProvider>()).Result;
        _repositoryProvider = Task.Run(() => _pluginWrapper.GetProviderAsync<IRepositoryProvider>()).Result;
        var myName = _repositoryProvider.DisplayName;
    }

    public IRepositoryProvider GetProvider()
    {
        return _repositoryProvider;
    }

    /// <summary>
    /// Tries to parse the repo name from the URI and makes a Repository from it.
    /// </summary>
    /// <param name="uri">The Uri to parse.</param>
    /// <returns>The repository the user wants to clone.  Null if parsing was unsuccessful.</returns>
    /// <remarks>
    /// Can be null if the provider can't parse the Uri.
    /// </remarks>
    public IRepository GetRepositoryFromUri(Uri uri, IDeveloperId developerId = null)
    {
        RepositoryResult getResult;
        if (developerId == null)
        {
            getResult = _repositoryProvider.GetRepositoryFromUriAsync(uri).AsTask().Result;
        }
        else
        {
            getResult = _repositoryProvider.GetRepositoryFromUriAsync(uri, developerId).AsTask().Result;
        }

        if (getResult.Result.Status == ProviderOperationStatus.Failure)
        {
            throw getResult.Result.ExtendedError;
        }

        return getResult.Repository;
    }

    /// <summary>
    /// Checks with the provider if it understands and can clone a repo via Uri.
    /// </summary>
    /// <param name="uri">The uri to the repository</param>
    /// <returns>A tuple that containes if the provider can parse the uri and the account it can parse with.</returns>
    /// <remarks>If the provider can't parse the Uri, this will try a second time with any logged in accounts.  If the repo is
    /// public, the developerid can be null.</remarks>
    public (bool, IDeveloperId, IRepositoryProvider) IsUriSupported(Uri uri)
    {
        var developerIdsResult = _devIdProvider.GetLoggedInDeveloperIds();

        // Possible that no accounts are loggd in.  Try in case the repo is public.
        if (developerIdsResult.Result.Status != ProviderOperationStatus.Success)
        {
            Log.Logger?.ReportError(Log.Component.RepoConfig, $"Could not get logged in accounts.  Message: {developerIdsResult.Result.DisplayMessage}", developerIdsResult.Result.ExtendedError);
            var uriSupportResult = Task.Run(() => _repositoryProvider.IsUriSupportedAsync(uri).AsTask()).Result;
            if (uriSupportResult.IsSupported)
            {
                return (true, null, _repositoryProvider);
            }
        }
        else
        {
            if (developerIdsResult.DeveloperIds.Any())
            {
                foreach (var developerId in developerIdsResult.DeveloperIds)
                {
                    var uriSupportResult = Task.Run(() => _repositoryProvider.IsUriSupportedAsync(uri, developerId).AsTask()).Result;
                    if (uriSupportResult.IsSupported)
                    {
                        return (true, developerId, _repositoryProvider);
                    }
                }
            }
            else
            {
                var uriSupportResult = Task.Run(() => _repositoryProvider.IsUriSupportedAsync(uri).AsTask()).Result;
                if (uriSupportResult.IsSupported)
                {
                    return (true, null, _repositoryProvider);
                }
            }
        }

        // no accounts can access this uri or the repo does not exist.
        return (false, null, null);
    }

    public PluginAdaptiveCardPanel GetLoginUi(ElementTheme elementTheme)
    {
        try
        {
            var adaptiveCardSessionResult = _devIdProvider.GetLoginAdaptiveCardSession();
            if (adaptiveCardSessionResult.Result.Status == ProviderOperationStatus.Failure)
            {
                GlobalLog.Logger?.ReportError($"{adaptiveCardSessionResult.Result.DisplayMessage} - {adaptiveCardSessionResult.Result.DiagnosticText}");
                return null;
            }

            var loginUIAdaptiveCardController = adaptiveCardSessionResult.AdaptiveCardSession;
            var renderer = new AdaptiveCardRenderer();
            ConfigureLoginUIRenderer(renderer, elementTheme).Wait();
            renderer.HostConfig.ContainerStyles.Default.BackgroundColor = Microsoft.UI.Colors.Transparent;

            var pluginAdaptiveCardPanel = new PluginAdaptiveCardPanel();
            pluginAdaptiveCardPanel.Bind(loginUIAdaptiveCardController, renderer);
            pluginAdaptiveCardPanel.RequestedTheme = elementTheme;

            return pluginAdaptiveCardPanel;
        }
        catch (Exception ex)
        {
            GlobalLog.Logger?.ReportError($"ShowLoginUIAsync(): loginUIContentDialog failed.", ex);
        }

        return null;
    }

    private async Task ConfigureLoginUIRenderer(AdaptiveCardRenderer renderer, ElementTheme elementTheme)
    {
        var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        // Add custom Adaptive Card renderer for LoginUI as done for Widgets.
        renderer.ElementRenderers.Set(LabelGroup.CustomTypeString, new LabelGroupRenderer());

        var hostConfigContents = string.Empty;
        var hostConfigFileName = (elementTheme == ElementTheme.Light) ? "LightHostConfig.json" : "DarkHostConfig.json";
        try
        {
            var uri = new Uri($"ms-appx:////DevHome.Settings/Assets/{hostConfigFileName}");
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri).AsTask().ConfigureAwait(false);
            hostConfigContents = await FileIO.ReadTextAsync(file);
        }
        catch (Exception ex)
        {
            GlobalLog.Logger?.ReportError($"Failure occurred while retrieving the HostConfig file - HostConfigFileName: {hostConfigFileName}.", ex);
        }

        // Add host config for current theme to renderer
        dispatcher.TryEnqueue(() =>
        {
            if (!string.IsNullOrEmpty(hostConfigContents))
            {
                renderer.HostConfig = AdaptiveHostConfig.FromJsonString(hostConfigContents).HostConfig;
            }
            else
            {
                GlobalLog.Logger?.ReportInfo($"HostConfig file contents are null or empty - HostConfigFileContents: {hostConfigContents}");
            }
        });
        return;
    }

    /// <summary>
    /// Gets all the logged in accounts for this provider.
    /// </summary>
    /// <returns>A list of all accounts.  May be empty.</returns>
    public IEnumerable<IDeveloperId> GetAllLoggedInAccounts()
    {
        var developerIdsResult = _devIdProvider.GetLoggedInDeveloperIds();
        if (developerIdsResult.Result.Status != ProviderOperationStatus.Success)
        {
            Log.Logger?.ReportError(Log.Component.RepoConfig, $"Could not get logged in accounts.  Message: {developerIdsResult.Result.DisplayMessage}", developerIdsResult.Result.ExtendedError);
            return new List<IDeveloperId>();
        }

        return developerIdsResult.DeveloperIds;
    }

    /// <summary>
    /// Gets all the repositories an account has for this provider.
    /// </summary>
    /// <param name="developerId">The account to search in.</param>
    /// <returns>A collection of repositories.  May be empty</returns>
    public IEnumerable<IRepository> GetAllRepositories(IDeveloperId developerId)
    {
        if (!_repositories.IsValueCreated)
        {
            TelemetryFactory.Get<ITelemetry>().Log("RepoTool_GetAllRepos_Event", LogLevel.Critical, new GetReposEvent("CallingExtension", _repositoryProvider.DisplayName, developerId));

            var result = _repositoryProvider.GetRepositoriesAsync(developerId).AsTask().Result;
            if (result.Result.Status != ProviderOperationStatus.Success)
            {
                _repositories = new Lazy<IEnumerable<IRepository>>(new List<IRepository>());
            }
            else
            {
                _repositories = new Lazy<IEnumerable<IRepository>>(result.Repositories);
            }
        }

        TelemetryFactory.Get<ITelemetry>().Log("RepoTool_GetAllRepos_Event", LogLevel.Critical, new GetReposEvent("FoundRepos", _repositoryProvider.DisplayName, developerId));

        return _repositories.Value;
    }
}
