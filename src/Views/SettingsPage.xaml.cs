﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System.Diagnostics;
using System.Runtime.InteropServices;
using DevHome.Common.Extensions;
using DevHome.Contracts.Services;
using DevHome.Services;
using DevHome.Telemetry;
using DevHome.ViewModels;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DevHome.Views;

// TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel
    {
        get;
    }

    public AccountsPageViewModel AccountsPageViewModel
    {
        get;
    }

    public SettingsPage()
    {
        ViewModel = Application.Current.GetService<SettingsViewModel>();
        AccountsPageViewModel = Application.Current.GetService<AccountsPageViewModel>();
        InitializeComponent();
    }

    private async void AddDeveloperId_Click(object sender, RoutedEventArgs e)
    {
        if (AccountsPageViewModel.AccountsProviders.Count == 0)
        {
            var confirmLogoutContentDialog = new ContentDialog
            {
                Title = "No Dev Home Plugins found!",
                Content = "Please install a Dev Home Plugin and restart Dev Home to add an account.",
                PrimaryButtonText = "Ok",
                XamlRoot = XamlRoot,
            };

            await confirmLogoutContentDialog.ShowAsync();
            return;
        }

        // TODO: expand this for multiple providers after their buttons are added
        if (sender as Button is Button addAccountButton)
        {
            if (addAccountButton.Tag is AccountsProviderViewModel accountProvider)
            {
                try
                {
                    await accountProvider.ShowLoginUI("Settings", this);
                }
                catch (Exception ex)
                {
                    LoggerFactory.Get<ILogger>().Log($"AddAccount_Click(): loginUIContentDialog failed", LogLevel.Local, $"Error: {ex} Sender: {sender} RoutedEventArgs: {e}");
                }

                accountProvider.RefreshLoggedInAccounts();
            }
            else
            {
                LoggerFactory.Get<ILogger>().Log($"AddAccount_Click(): addAccountButton.Tag is not AccountsProviderViewModel", LogLevel.Local, $"Sender: {sender} RoutedEventArgs: {e}");
                return;
            }
        }
    }

    private async void Logout_Click(object sender, RoutedEventArgs e)
    {
        var confirmLogoutContentDialog = new ContentDialog
        {
            Title = "Are you sure?",
            Content = "Are you sure you want to remove this user account?"
                    + Environment.NewLine
                    + Environment.NewLine
                    + "Dev Home will no longer be able to access online resources that use this account.",
            PrimaryButtonText = "Yes",
            SecondaryButtonText = "No",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };
        var contentDialogResult = await confirmLogoutContentDialog.ShowAsync();

        if (contentDialogResult.Equals(ContentDialogResult.Secondary))
        {
            return;
        }

        var loginIdToRemove = (sender as Button)?.Tag.ToString();
        if (string.IsNullOrEmpty(loginIdToRemove))
        {
            return;
        }

        AccountsPageViewModel.AccountsProviders.First().RemoveAccount(loginIdToRemove);
        AccountsPageViewModel.AccountsProviders.First().RefreshLoggedInAccounts();

        var afterLogoutContentDialog = new ContentDialog
        {
            Title = "Logout Successful",
            Content = loginIdToRemove + " has successfully logged out",
            PrimaryButtonText = "OK",
            XamlRoot = XamlRoot,
        };
        _ = await afterLogoutContentDialog.ShowAsync();
    }
}
