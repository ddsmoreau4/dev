// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using DevHome.Common.Extensions;
using DevHome.Common.Services;
using DevHome.Settings.Models;
using DevHome.Settings.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevHome.Settings.Views;

public sealed partial class PreferencesPage : Page
{
    public PreferencesViewModel ViewModel
    {
        get;
    }

    public ObservableCollection<Breadcrumb> Breadcrumbs
    {
        get;
    }

    public PreferencesPage()
    {
        ViewModel = Application.Current.GetService<PreferencesViewModel>();
        this.InitializeComponent();

        var stringResource = new StringResource("DevHome.Settings/Resources");
        Breadcrumbs = new ObservableCollection<Breadcrumb>
        {
            new (stringResource.GetLocalized("Settings_Header"), typeof(SettingsViewModel).FullName!),
            new (stringResource.GetLocalized("Settings_Preferences_Header"), typeof(PreferencesViewModel).FullName!),
        };
    }

    private void BreadcrumbBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
    {
        if (args.Index < Breadcrumbs.Count - 1)
        {
            var crumb = (Breadcrumb)args.Item;
            crumb.NavigateTo();
        }
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        var selectedTheme = ViewModel.ElementTheme;
        foreach (var item in ThemeSelectionComboBox.Items)
        {
            var comboItem = item as ComboBoxItem;
            if (comboItem?.Tag is ElementTheme tag && tag == selectedTheme)
            {
                ThemeSelectionComboBox.SelectedValue = item;
                break;
            }
        }
    }
}
