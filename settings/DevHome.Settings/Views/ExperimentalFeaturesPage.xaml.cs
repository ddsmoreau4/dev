// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using DevHome.Common.Extensions;
using DevHome.Settings.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevHome.Settings.Views;

public sealed partial class ExperimentalFeaturesPage : Page
{
    public ExperimentalFeaturesViewModel ViewModel { get; }

    public ExperimentalFeaturesPage()
    {
        ViewModel = Application.Current.GetService<ExperimentalFeaturesViewModel>();
        this.InitializeComponent();
    }
}
