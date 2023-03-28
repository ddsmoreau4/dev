﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevHome.Common.Services;
using Microsoft.Windows.DevHome.SDK;
using Windows.ApplicationModel.DataTransfer;

namespace DevHome.SetupFlow.RepoConfig.Models;

/// <summary>
/// Object that holds a refrence to the providers in a plugin.
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
    /// The DevId provider used to log a user into an account.
    /// </summary>
    private IDevIdProvider _devIdProvider;

    /// <summary>
    /// Provider used to clone a repsitory.
    /// </summary>
    private IRepositoryProvider _repositoryProvider;

    /// <summary>
    /// All the repositories for an account.
    /// </summary>
    private Lazy<IEnumerable<IRepository>> _repositories = new ();

    public RepositoryProvider(IPluginWrapper pluginWrapper)
    {
        _pluginWrapper = pluginWrapper;
    }

    /// <summary>
    /// Starts the plugin if it isn't running.
    /// </summary>
    /// <returns>An awaitable task</returns>
    public async Task StartIfNotRunningAsync()
    {
        _devIdProvider = await _pluginWrapper.GetProviderAsync<IDevIdProvider>();
        _repositoryProvider = await _pluginWrapper.GetProviderAsync<IRepositoryProvider>();
    }

    /// <summary>
    /// Tries to parse the repo name from the URi and makes a Repository from it.
    /// </summary>
    /// <param name="uri">The Uri to parse.</param>
    /// <returns>The repository the user wants to clone.  Null if parsing was unsuccessful.</returns>
    /// <remarks>
    /// Can be null if the provider can't parse the Uri.
    /// </remarks>
    public IRepository ParseRepositoryFromUri(Uri uri)
    {
        return _repositoryProvider.ParseRepositoryFromUrl(uri);
    }

    /// <summary>
    /// Logs the current user into this provider
    /// </summary>
    public async Task LogIntoProvider()
    {
        await _devIdProvider.LoginNewDeveloperIdAsync();
    }

    /// <summary>
    /// Gets all the logged in accounts for this provider.
    /// </summary>
    /// <returns>A list of all accounts.  May be empty.</returns>
    public IEnumerable<IDeveloperId> GetAllLoggedInAccounts()
    {
        return _devIdProvider.GetLoggedInDeveloperIds() ?? new List<IDeveloperId>();
    }

    /// <summary>
    /// Gets all the repositories an account has for this provider.
    /// </summary>
    /// <param name="developerId">The account to search in.</param>
    /// <returns>A collection of repositories.  May be empty</returns>
    public async Task<IEnumerable<IRepository>> GetAllRepositoriesAsync(IDeveloperId developerId)
    {
        if (!_repositories.IsValueCreated)
        {
            _repositories = new Lazy<IEnumerable<IRepository>>(await _repositoryProvider.GetRepositoriesAsync(developerId));
        }

        return _repositories.Value;
    }
}
