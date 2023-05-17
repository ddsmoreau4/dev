// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DevHome.SetupFlow.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevHome.SetupFlow.Views;

[INotifyPropertyChanged]
public sealed partial class PackageCatalogView : UserControl
{
    /// <summary>
    /// List of package groups of size <see cref="GroupSize"/>
    /// </summary>
    private List<PackageViewModel[]> _packageGroupsCache = new ();

    /// <summary>
    /// Gets the list of package groups of size <see cref="GroupSize"/>
    /// </summary>
    public List<PackageViewModel[]> PackageGroups => _packageGroupsCache;

    /// <summary>
    /// Gets a value indicating whether the pager should be visible.
    /// Show the pager if there's more than one group
    /// </summary>
    public Visibility PagerVisibility => PackageGroups.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Gets or sets the package catalog to display
    /// </summary>
    public PackageCatalogViewModel Catalog
    {
        get => (PackageCatalogViewModel)GetValue(CatalogProperty);
        set => SetValue(CatalogProperty, value);
    }

    /// <summary>
    /// Gets or sets the max size of each package group. If the total number of
    /// packages is not divisible by the group size, then the lsat group will
    /// have less packages.
    /// </summary>
    public int GroupSize
    {
        get => (int)GetValue(GroupSizeProperty);
        set => SetValue(GroupSizeProperty, value);
    }

    public PackageCatalogView()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Re-compute the FlipView height.
    /// </summary>
    private void UpdateFlipViewSize()
    {
        var selectedIndex = PackagesFlipView.SelectedIndex;
        if (selectedIndex >= 0)
        {
            var flipViewItem = PackagesFlipView.ContainerFromIndex(selectedIndex) as FlipViewItem;
            if (flipViewItem != null)
            {
                var scrollViewer = flipViewItem.ContentTemplateRoot as ScrollViewer;
                if (scrollViewer != null)
                {
                    // Set the FlipView height to the ScrollViewer content height
                    PackagesFlipView.Height = scrollViewer.ExtentHeight;
                }
            }
        }
    }

    /// <summary>
    /// Update the list of package group cache
    /// </summary>
    private void UpdatePackageGroups()
    {
        if (Catalog != null)
        {
            _packageGroupsCache = Catalog.Packages.Chunk(GroupSize).ToList();
            OnPropertyChanged(nameof(PackageGroups));
            OnPropertyChanged(nameof(PagerVisibility));
        }
    }

    /// <summary>
    /// Perform all UI updates
    /// </summary>
    private void UpdateAll()
    {
        UpdatePackageGroups();
        UpdateFlipViewSize();
    }

    public static readonly DependencyProperty CatalogProperty = DependencyProperty.Register(nameof(Catalog), typeof(PackageCatalogViewModel), typeof(PackageCatalogView), new PropertyMetadata(null, (c, _) => ((PackageCatalogView)c).UpdateAll()));
    public static readonly DependencyProperty GroupSizeProperty = DependencyProperty.Register(nameof(GroupSize), typeof(int), typeof(PackageCatalogView), new PropertyMetadata(4, (c, _) => ((PackageCatalogView)c).UpdateAll()));

    private void OnLayoutUpdated(object sender, object e)
    {
        UpdateFlipViewSize();
    }
}
