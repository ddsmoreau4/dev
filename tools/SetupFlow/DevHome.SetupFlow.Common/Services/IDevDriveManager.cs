﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevHome.Common.Models;

namespace DevHome.Common.Services;

/// <summary>
/// Enum Operation results when the Dev Drive manager performs an operation
/// related to a Dev Drive such as validating its contents. This is only to
/// allow us to know which error to show in the UI. These do not replace any
/// established error coding system.
/// </summary>
public enum DevDriveOperationResult
{
    Successful,
    ObjectWasNull,
    InvalidDriveSize,
    InvalidDriveLabel,
    InvalidFolderLocation,
    FolderLocationNotFound,
    FileNameAlreadyExists,
    DefaultFolderNotAvailable,
    DriveLetterNotAvailable,
    NoDriveLettersAvailable,
    NotEnoughFreeSpace,
    CreateDevDriveFailed,
    DevDriveNotFound,
}

/// <summary>
/// Interface for Dev Drive manager. Managers should be able to associate the Dev Drive that it creates to a
/// Dev drive window that is launched.
/// </summary>
public interface IDevDriveManager
{
    /// <summary>
    /// Starts off the Dev Drive creation operations for the requested IDevDrive object.
    /// </summary>
    /// <param name="devDrive">IDevDrive to create</param>
    /// <returns>Returns true if the Dev Drive was created successfully</returns>
    public Task<DevDriveOperationResult> CreateDevDrive(IDevDrive devDrive);

    /// <summary>
    /// Allows objects to request a Dev Drive window be created.
    /// </summary>
    /// <param name="devDrive">Dev Drive the window will be created for</param>
    /// <returns>Returns true if the Dev Drive window was launched successfully</returns>
    public Task<bool> LaunchDevDriveWindow(IDevDrive devDrive);

    /// <summary>
    /// Allows objects to notify the Dev Drive Manager that a Dev Drive window was closed.
    /// </summary>
    /// <param name="newDevDrive">Dev Drive object</param>
    public void NotifyDevDriveWindowClosed(IDevDrive newDevDrive);

    /// <summary>
    /// Gets a new Dev Drive object.
    /// </summary>
    /// <returns>
    /// An Dev Drive a new Dev Drive and a result that indicates whether the operation
    /// was successful.
    /// </returns>
    public (DevDriveOperationResult, IDevDrive) GetNewDevDrive();

    /// <summary>
    /// Gets a list of all Dev Drives currently on the local system. This will cause storage calls
    /// that may be slow so it is done through a task. These Dev Drives have their DevDriveState set to Exists.
    /// </summary>
    public Task<IEnumerable<IDevDrive>> GetAllDevDrivesThatExistOnSystem();

    /// <summary>
    /// Event that requesters can subscribe to, to know when a Dev Drive window has closed.
    /// </summary>
    public event EventHandler<IDevDrive> ViewModelWindowClosed;

    /// <summary>
    /// Validates the values inside the Dev Drive against Dev Drive requirements. A Dev drive is only validated
    /// if the only result returned is DevDriveOperationResult.Successful
    /// </summary>
    /// <param name="devDrive">Dev Drive object</param>
    /// <returns>
    /// A set of operation results from the Dev Drive manager attempting to validate the contents
    /// of the Dev Drive.
    /// </returns>
    public ISet<DevDriveOperationResult> GetDevDriveValidationResults(IDevDrive devDrive);

    /// <summary>
    /// Gets a list of drive letters that have been marked for creation by the Dev Drive Manager.
    /// </summary>
    /// <returns>A list of IDevDrive objects that will be created</returns>
    public IList<IDevDrive> DevDrivesMarkedForCreation
    {
        get;
    }

    /// <summary>
    /// Gets all available drive letters on the system. From these letters, those that are currently
    /// being used by a Dev Drive created in memory by the Dev Drive manager are removed.
    /// </summary>
    /// <param name="usedLetterToKeepInList">
    /// when not null the Dev Drive manager should add the letter in usedLetterToKeepInList even if it
    /// is in used by a Dev Drive in memory.
    /// </param>
    /// <returns>
    /// A list of sorted drive letters currently not in use by the Dev Drive manager and the system
    /// </returns>
    public IList<char> GetAvailableDriveLetters(char? usedLetterToKeepInList = null);

    /// <summary>
    /// Removes Dev Drives that were created in memory by the Dev Drive Manager. This does not detach
    /// or remove a Dev Drive from the users machine.
    /// <param name="devDrive">Dev Drive object</param>
    /// <returns>
    /// A result indicating whether the operation was successful.
    /// </returns>
    public DevDriveOperationResult RemoveDevDrive(IDevDrive devDrive);

    /// <summary>
    /// Allows objects who hold a IDevDrive object to request that the Manager tell the view model to close the
    /// Dev Drive window. In this case the requester wants to close the window, whereas in the NotifyDevDriveWindowClosed case
    /// the view model is telling the requester the window closed.
    /// </summary>
    /// <param name="devDrive">Dev Drive object</param>
    public void RequestToCloseDevDriveWindow(IDevDrive devDrive);

    /// <summary>
    /// Event that the Dev Drive view model can subscribe to, to know if a requester wants them to close the window, without the user explicity
    /// closing the window themselves, through actions like clicking the close button.
    /// </summary>
    public event EventHandler<IDevDrive> RequestToCloseViewModelWindow;
}
