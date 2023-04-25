﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Provider;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DevHome.Common.Extensions;
using DevHome.Common.Services;
using DevHome.SetupFlow.Common.Helpers;
using DevHome.SetupFlow.Models;
using DevHome.SetupFlow.Services;
using Microsoft.UI.Xaml;
using Microsoft.Windows.DevHome.SDK;
using static DevHome.SetupFlow.Models.Common;

namespace DevHome.SetupFlow.ViewModels;

/// <summary>
/// View model to handle the top layer of the repo tool including
/// 1. Repo Review
/// 2. Switching between account, repositories, and url page
/// </summary>
public partial class AddRepoViewModel : ObservableObject
{
    private readonly ISetupFlowStringResource _stringResource;

    /// <summary>
    /// Gets or sets the list that keeps all repositories the user wants to clone.
    /// </summary>
    public List<CloningInformation> EverythingToClone
    {
        get; set;
    }

    /// <summary>
    /// The url of the repository the user wants to clone.
    /// </summary>
    [ObservableProperty]
    private string _url = string.Empty;

    /// <summary>
    /// All the providers Dev Home found.  Used for logging in the accounts and getting all repositories.
    /// </summary>
    private RepositoryProviders _providers;

    /// <summary>
    /// The list of all repositories shown to the user on the repositories page.
    /// </summary>
    private IEnumerable<IRepository> _repositoriesForAccount;

    /// <summary>
    /// Names of all providers.  This is shown to the user on the accounts page.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _providerNames = new ();

    /// <summary>
    /// Names of all accounts the user has logged into for a particular provider.
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _accounts = new ();

    /// <summary>
    /// All the repositories for a specific account and the symbol to show
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<RepoViewListItem> _repositories = new ();

    /// <summary>
    /// Should the URL page be visible?
    /// </summary>
    [ObservableProperty]
    private Visibility _showUrlPage;

    /// <summary>
    /// Should the account page be visible?
    /// </summary>
    [ObservableProperty]
    private Visibility _showAccountPage;

    /// <summary>
    /// Should the repositories page be visible?
    /// </summary>
    [ObservableProperty]
    private Visibility _showRepoPage;

    /// <summary>
    /// Should the error text be shown?
    /// </summary>
    [ObservableProperty]
    private Visibility _showErrorTextBox;

    /// <summary>
    /// Keeps track of if the account button is checked.  Used to switch UIs
    /// </summary>
    [ObservableProperty]
    private bool? _isAccountToggleButtonChecked;

    /// <summary>
    /// Keeps track if the URL button is checked.  Used to switch UIs
    /// </summary>
    [ObservableProperty]
    private bool? _isUrlAccountButtonChecked;

    /// <summary>
    /// COntrols if the primary button is enabled.  Turns true if everything is correct.
    /// </summary>
    [ObservableProperty]
    private bool _shouldPrimaryButtonBeEnabled;

    [ObservableProperty]
    private string _textToFilterBy;

    [ObservableProperty]
    private string _primaryButtonText;

    [ObservableProperty]
    private string _urlParsingError;

    [ObservableProperty]
    private Visibility _shouldShowUrlError;

    [RelayCommand]
    private void FilterRepositories(string text)
    {
        IEnumerable<RepoViewListItem> filteredRepositories;
        if (text.Equals(string.Empty, StringComparison.OrdinalIgnoreCase))
        {
            filteredRepositories = _repositoriesForAccount.OrderBy(x => x.IsPrivate).Select(x => new RepoViewListItem(x));
        }
        else
        {
            filteredRepositories = _repositoriesForAccount.OrderBy(x => x.IsPrivate)
                .Where(x => x.DisplayName.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                .Select(x => new RepoViewListItem(x));
        }

        Repositories = new ObservableCollection<RepoViewListItem>(filteredRepositories);
    }

    /// <summary>
    /// Gets or sets what page the user is currently on.  Used to branch logic depending on the page.
    /// </summary>
    internal PageKind CurrentPage
    {
        get; set;
    }

    public AddRepoViewModel(ISetupFlowStringResource stringResource)
    {
        _stringResource = stringResource;
        ChangeToUrlPage();

        // override changes ChangeToUrlPage to correctly set the state.
        UrlParsingError = string.Empty;
        ShouldShowUrlError = Visibility.Collapsed;
        ShouldPrimaryButtonBeEnabled = false;
        ShowErrorTextBox = Visibility.Collapsed;
        EverythingToClone = new ();
    }

    /// <summary>
    /// Gets all the plugins the DevHome can see.
    /// </summary>
    /// <remarks>
    /// A valid plugin is one that has a repository provider and devid provider.
    /// </remarks>
    public void GetPlugins()
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, "Getting installed plugins with Repository and DevId providers");
        var pluginService = Application.Current.GetService<IPluginService>();
        var pluginWrappers = pluginService.GetInstalledPluginsAsync().Result;
        var plugins = pluginWrappers.Where(
            plugin => plugin.HasProviderType(ProviderType.Repository) &&
            plugin.HasProviderType(ProviderType.DeveloperId));

        _providers = new RepositoryProviders(plugins);

        ProviderNames = new ObservableCollection<string>(_providers.GetAllProviderNames());
    }

    public void ChangeToUrlPage()
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, "Changing to Url page");
        ShowUrlPage = Visibility.Visible;
        ShowAccountPage = Visibility.Collapsed;
        ShowRepoPage = Visibility.Collapsed;
        IsUrlAccountButtonChecked = true;
        IsAccountToggleButtonChecked = false;
        CurrentPage = PageKind.AddViaUrl;
        PrimaryButtonText = _stringResource.GetLocalized(StringResourceKey.RepoEverythingElsePrimaryButtonText);
    }

    public void ChangeToAccountPage()
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, "Changing to Account page");
        ShouldShowUrlError = Visibility.Collapsed;
        ShowUrlPage = Visibility.Collapsed;
        ShowAccountPage = Visibility.Visible;
        ShowRepoPage = Visibility.Collapsed;
        IsUrlAccountButtonChecked = false;
        IsAccountToggleButtonChecked = true;
        CurrentPage = PageKind.AddViaAccount;
        PrimaryButtonText = _stringResource.GetLocalized(StringResourceKey.RepoAccountPagePrimaryButtonText);
    }

    public void ChangeToRepoPage()
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, "Changing to Repo page");
        ShowUrlPage = Visibility.Collapsed;
        ShowAccountPage = Visibility.Collapsed;
        ShowRepoPage = Visibility.Visible;
        CurrentPage = PageKind.Repositories;
        PrimaryButtonText = _stringResource.GetLocalized(StringResourceKey.RepoEverythingElsePrimaryButtonText);

        // The only way to get the repo page is through the account page.
        // No need to change toggle buttons.
    }

    /// <summary>
    /// Makes sure all needed information is present.
    /// </summary>
    /// <returns>True if all information is in order, otherwise false</returns>
    public bool ValidateRepoInformation()
    {
        if (CurrentPage == PageKind.AddViaUrl)
        {
            // Check if Url field is empty
            if (string.IsNullOrEmpty(Url))
            {
                UrlParsingError = _stringResource.GetLocalized(StringResourceKey.UrlValidationEmpty);
                ShouldShowUrlError = Visibility.Visible;
                return false;
            }

            if (!Uri.TryCreate(Url, UriKind.Relative, out _))
            {
                UrlParsingError = _stringResource.GetLocalized(StringResourceKey.UrlValidationBadUrl);
                ShouldShowUrlError = Visibility.Visible;
                return false;
            }

            ShouldShowUrlError = Visibility.Collapsed;
            return true;
        }
        else if (CurrentPage == PageKind.AddViaAccount || CurrentPage == PageKind.Repositories)
        {
            // make sure the user has selected some repositories.
            return EverythingToClone.Count > 0;
        }
        else
        {
            return false;
        }
    }

    public void EnablePrimaryButton()
    {
        ShouldPrimaryButtonBeEnabled = true;
    }

    public void DisablePrimaryButton()
    {
        ShouldPrimaryButtonBeEnabled = false;
    }

    /// <summary>
    /// Gets all the accounts for a provider and updates the UI.
    /// </summary>
    /// <param name="repositoryProviderName">The provider the user wants to use.</param>
    public async Task GetAccountsAsync(string repositoryProviderName)
    {
        await Task.Run(() => _providers.StartIfNotRunning(repositoryProviderName));
        var loggedInAccounts = await Task.Run(() => _providers.GetAllLoggedInAccounts(repositoryProviderName));
        if (!loggedInAccounts.Any())
        {
            // Throw away developer id becase we're calling GetAllLoggedInAccounts in anticipation
            // of 1 Provider : N DeveloperIds
            await Task.Run(() => _providers.LogInToProvider(repositoryProviderName));
            loggedInAccounts = await Task.Run(() => _providers.GetAllLoggedInAccounts(repositoryProviderName));
        }

        Accounts = new ObservableCollection<string>(loggedInAccounts.Select(x => x.LoginId()));
    }

    /// <summary>
    /// Adds repositories to the list of repos to clone.
    /// Removes repositories from the list of repos to clone.
    /// </summary>
    /// <param name="providerName">The provider that is used to do the cloning.</param>
    /// <param name="accountName">The account used to authenticate into the provider.</param>
    /// <param name="repositoriesToAdd">Repositories to add</param>
    /// <param name="repositoriesToRemove">Repositories to remove.</param>
    /// <remarks>
    /// User has to go through the account screen to get here.  The login id to use is known.
    /// </remarks>
    public void AddOrRemoveRepository(string providerName, string accountName, IList<object> repositoriesToAdd, IList<object> repositoriesToRemove)
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Adding and removing repositories");
        var developerId = _providers.GetAllLoggedInAccounts(providerName).FirstOrDefault(x => x.LoginId() == accountName);
        foreach (RepoViewListItem repositoryToRemove in repositoriesToRemove)
        {
            Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Removing repository {repositoryToRemove}");
            var cloningInformation = new CloningInformation();
            cloningInformation.ProviderName = providerName;
            cloningInformation.OwningAccount = developerId;
            cloningInformation.RepositoryToClone = _repositoriesForAccount.FirstOrDefault(x => x.DisplayName.Equals(repositoryToRemove.RepoName, StringComparison.OrdinalIgnoreCase));

            EverythingToClone.Remove(cloningInformation);
        }

        foreach (RepoViewListItem repositoryToAdd in repositoriesToAdd)
        {
            Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Adding repository {repositoryToAdd}");
            var cloningInformation = new CloningInformation();
            cloningInformation.ProviderName = providerName;
            cloningInformation.OwningAccount = developerId;
            cloningInformation.RepositoryToClone = _repositoriesForAccount.FirstOrDefault(x => x.DisplayName.Equals(repositoryToAdd.RepoName, StringComparison.OrdinalIgnoreCase));
            cloningInformation.EditClonePathAutomationName = _stringResource.GetLocalized(StringResourceKey.RepoPageEditClonePathAutomationProperties, $"{providerName}/{repositoryToAdd}");
            cloningInformation.RemoveFromCloningAutomationName = _stringResource.GetLocalized(StringResourceKey.RepoPageRemoveRepoAutomationProperties, $"{providerName}/{repositoryToAdd}");

            EverythingToClone.Add(cloningInformation);
        }
    }

    /// <summary>
    /// Adds a repository from the URL page. Steps to determine what repoProvider to use.
    /// 1. All providers are asked "Can you parse this into a URL you understand."  If yes, that provider to clone the repo.
    /// 2. If no providers can parse the URL a fall back "GitProvider" is used that uses libgit2sharp to clone the repo.
    /// </summary>
    /// <param name="cloneLocation">The location to clone the repo to</param>
    public void AddRepositoryViaUri(string url, string cloneLocation)
    {
        // If the url isn't valid don't bother finding a provider.
        Uri uriToParse;
        if (!Uri.TryCreate(url, UriKind.Relative, out uriToParse))
        {
            UrlParsingError = _stringResource.GetLocalized(StringResourceKey.UrlValidationBadUrl);
            ShouldShowUrlError = Visibility.Visible;
            return;
        }

        var cloningInformation = new CloningInformation();

        // If the URL points to a private repo the URL tab has no way of knowing what account has access.
        // Keep owning account null to make github extension try all logged in accounts.
        cloningInformation.OwningAccount = null;
        (string, IRepository) providerNameAndRepo;

        try
        {
            providerNameAndRepo = _providers.ParseRepositoryFromUri(uriToParse);
        }
        catch (Exception e)
        {
            // Catching should not be used for branching logic.
            // However, I forgot to consider the scenario where the URL can be parsed
            // but the repo can't be found.  This can happen if
            // 1. Any logged in account does not have access
            // 2. The repo does not exist.
            UrlParsingError = _stringResource.GetLocalized(StringResourceKey.UrlValidationNotFound);
            ShouldShowUrlError = Visibility.Visible;
            Log.Logger?.ReportInfo(Log.Component.RepoConfig, e.ToString());
            return;
        }

        if (providerNameAndRepo.Item2 != null)
        {
            // A provider parsed the URL and at least 1 logged in account has access to the repo.
            var repository = providerNameAndRepo.Item2;
            cloningInformation.ProviderName = providerNameAndRepo.Item1;
            cloningInformation.RepositoryToClone = repository;
            cloningInformation.CloningLocation = new DirectoryInfo(cloneLocation);
        }
        else
        {
            Log.Logger?.ReportInfo(Log.Component.RepoConfig, "No providers could parse the Url.  Falling back to internal git provider");

            // No providers can parse the Url.
            // Fall back to a git Url.
            cloningInformation.ProviderName = "git";
            cloningInformation.RepositoryToClone = new GenericRepository(uriToParse);
            cloningInformation.CloningLocation = new DirectoryInfo(cloneLocation);
        }

        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Adding repository to clone {cloningInformation.RepositoryId} to location '{cloneLocation}'");
        EverythingToClone.Add(cloningInformation);
    }

    /// <summary>
    /// Gets all the repositories for the the specified provider and account.
    /// </summary>
    /// <param name="repositoryProvider">The provider.  This should match IRepositoryProvider.LoginId</param>
    /// <param name="loginId">The login Id to get the repositories for</param>
    public void GetRepositories(string repositoryProvider, string loginId)
    {
        var loggedInDeveloper = _providers.GetAllLoggedInAccounts(repositoryProvider).FirstOrDefault(x => x.LoginId() == loginId);
        _repositoriesForAccount = _providers.GetAllRepositories(repositoryProvider, loggedInDeveloper);

        // TODO: What if the user comes back here with repos selected?
        Repositories = new ObservableCollection<RepoViewListItem>(_repositoriesForAccount.OrderBy(x => x.IsPrivate).Select(x => new RepoViewListItem(x)));
    }

    /// <summary>
    /// Sets the clone location for all repositories to _cloneLocation
    /// </summary>
    /// <param name="cloneLocation">The location to clone all repositories to.</param>
    public void SetCloneLocation(string cloneLocation)
    {
        Log.Logger?.ReportInfo(Log.Component.RepoConfig, $"Setting the clone location for all repositories to {cloneLocation}");
        foreach (var cloningInformation in EverythingToClone)
        {
            cloningInformation.CloningLocation = new DirectoryInfo(cloneLocation);
        }
    }
}
