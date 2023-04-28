// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Linq;
using DevHome.Common.Extensions;
using DevHome.Common.Models;
using DevHome.Contracts.Services;
using DevHome.SetupFlow.Models;
using DevHome.SetupFlow.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevHome.SetupFlow.Views;

/// <summary>
/// Shows the user the repositories they have sleected.
/// </summary>
public sealed partial class RepoConfigView : UserControl
{
    public RepoConfigView()
    {
        this.InitializeComponent();
    }

    public RepoConfigViewModel ViewModel => (RepoConfigViewModel)this.DataContext;

    /// <summary>
    /// User wants to add a repo.  Bring up the tool.
    /// </summary>
    private async void AddRepoButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Both the hyperlink button and button call this.
        // disable the button to prevent users from double clicking it.
        var senderAsButton = sender as Button;
        if (senderAsButton != null)
        {
            senderAsButton.IsEnabled = false;
        }

        var addRepoDialog = new AddRepoDialog(ViewModel.DevDriveManager, ViewModel.LocalStringResource, ViewModel.RepoReviewItems.ToList());
        var getPluginsTask = addRepoDialog.GetPluginsAsync();
        var setupDevDrivesTask = addRepoDialog.SetupDevDrivesAsync();
        var themeService = Application.Current.GetService<IThemeSelectorService>();
        addRepoDialog.XamlRoot = RepoConfigGrid.XamlRoot;
        addRepoDialog.RequestedTheme = themeService.Theme;

        // Start
        await getPluginsTask;
        await setupDevDrivesTask;
        var result = await addRepoDialog.ShowAsync(ContentDialogPlacement.InPlace);

        if (senderAsButton != null)
        {
            senderAsButton.IsEnabled = true;
        }

        var devDrive = addRepoDialog.EditDevDriveViewModel.DevDrive;

        if (addRepoDialog.EditDevDriveViewModel.IsWindowOpen)
        {
            ViewModel.DevDriveManager.RequestToCloseDevDriveWindow(devDrive);
        }

        var everythingToClone = addRepoDialog.AddRepoViewModel.EverythingToClone;
        if (result == ContentDialogResult.Primary && everythingToClone.Any())
        {
            // We currently only support adding either a local path or a new Dev Drive as the cloning location. Only one can be selected
            // during the add repo dialog flow. So if multiple repositories are selected and the user chose to clone them to a Dev Drive
            // that doesn't exist on the system yet, then we make sure all the locations will clone to that new Dev Drive.
            if (devDrive != null && devDrive.State != DevDriveState.ExistsOnSystem)
            {
                foreach (var cloneInfo in everythingToClone)
                {
                    cloneInfo.CloneToDevDrive = true;
                    cloneInfo.CloneLocationAlias = addRepoDialog.FolderPickerViewModel.CloneLocationAlias;
                }

                // The cloning location may have changed e.g The original Drive clone path for Dev Drives was the F: drive for items
                // on the add repo page, but during the Add repo dialog flow the user chose to change this location to the D: drive.
                // we need to reflect this for all the old items currently in the add repo page.
                ViewModel.UpdateCollectionWithDevDriveInfo(everythingToClone.First());
                ViewModel.DevDriveManager.IncreaseRepositoriesCount(everythingToClone.Count);
                ViewModel.DevDriveManager.ConfirmChangesToDevDrive();
            }

            ViewModel.SaveSetupTaskInformation(everythingToClone);
        }
        else
        {
            // User cancelled the dialog, Report back to the Dev drive Manager to revert any changes.
            ViewModel.ReportDialogCancellation();
        }
    }

    /// <summary>
    /// User wants to edit the clone location of a repo.  Show the dialog.
    /// </summary>
    /// <param name="sender">Used to find the cloning information clicked on.</param>
    private async void EditClonePathButton_Click(object sender, RoutedEventArgs e)
    {
        var cloningInformation = (sender as Button).DataContext as CloningInformation;
        var oldLocation = cloningInformation.CloningLocation;
        var wasCloningToDevDrive = cloningInformation.CloneToDevDrive;
        var editClonePathDialog = new EditClonePathDialog(ViewModel.DevDriveManager, cloningInformation, ViewModel.LocalStringResource);
        var themeService = Application.Current.GetService<IThemeSelectorService>();
        editClonePathDialog.XamlRoot = RepoConfigGrid.XamlRoot;
        editClonePathDialog.RequestedTheme = themeService.Theme;
        var result = await editClonePathDialog.ShowAsync(ContentDialogPlacement.InPlace);

        var devDrive = editClonePathDialog.EditDevDriveViewModel.DevDrive;
        cloningInformation.CloneToDevDrive = devDrive != null;

        if (result == ContentDialogResult.Primary)
        {
            cloningInformation.CloningLocation = new System.IO.DirectoryInfo(editClonePathDialog.FolderPickerViewModel.CloneLocation);
            ViewModel.UpdateCloneLocation(cloningInformation);

            // User intended to clone to Dev Drive before launching dialog but now they are not,
            // so decrease the Dev Managers count.
            if (wasCloningToDevDrive && !cloningInformation.CloneToDevDrive)
            {
                ViewModel.DevDriveManager.DecreaseRepositoriesCount();
                ViewModel.DevDriveManager.CancelChangesToDevDrive();
            }

            if (cloningInformation.CloneToDevDrive)
            {
                ViewModel.DevDriveManager.ConfirmChangesToDevDrive();

                // User switched from local path to Dev Drive
                if (!wasCloningToDevDrive)
                {
                    ViewModel.DevDriveManager.IncreaseRepositoriesCount(1);
                }

                cloningInformation.CloneLocationAlias = editClonePathDialog.FolderPickerViewModel.CloneLocationAlias;
                ViewModel.UpdateCloneLocation(cloningInformation);
            }

            // If the user launches the edit button, and changes or updates the clone path to be a Dev Drive, we need
            // to update the other entries in the list, that are being cloned to the Dev Drive with this new information.
            if (oldLocation != cloningInformation.CloningLocation && cloningInformation.CloneToDevDrive)
            {
                ViewModel.UpdateCollectionWithDevDriveInfo(cloningInformation);
            }
        }
        else
        {
            // User cancelled the dialog, Report back to the Dev drive Manager to revert any changes.
            ViewModel.ReportDialogCancellation();
            cloningInformation.CloneToDevDrive = wasCloningToDevDrive;
        }

        if (editClonePathDialog.EditDevDriveViewModel.IsWindowOpen)
        {
            ViewModel.DevDriveManager.RequestToCloseDevDriveWindow(editClonePathDialog.EditDevDriveViewModel.DevDrive);
        }
    }

    /// <summary>
    /// Removes a repository to clone from the list.
    /// </summary>
    /// <param name="sender">Used to find the cloning information clicked on.</param>
    private void RemoveCloningInformationButton_Click(object sender, RoutedEventArgs e)
    {
        var cloningInformation = (sender as Button).DataContext as CloningInformation;
        ViewModel.RemoveCloningInformation(cloningInformation);
        if (cloningInformation.CloneToDevDrive)
        {
            ViewModel.DevDriveManager.DecreaseRepositoriesCount();
        }
    }
}
