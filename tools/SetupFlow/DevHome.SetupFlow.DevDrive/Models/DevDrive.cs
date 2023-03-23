﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using System;
using DevHome.Common.Models;
using DevHome.SetupFlow.DevDrive.Utilities;

namespace DevHome.SetupFlow.DevDrive.Models;

/// <summary>
/// Model class representation for Dev Drives.
/// </summary>
public class DevDrive : IDevDrive
{
    public DevDrive(
        char driveLetter,
        ulong driveSizeInBytes,
        ByteUnit driveUnitOfMeasure,
        string driveLocation,
        string driveLabel,
        DevDriveState state)
        : this()
    {
        DriveLetter = driveLetter;
        DriveSizeInBytes = driveSizeInBytes;
        DriveUnitOfMeasure = driveUnitOfMeasure;
        DriveLocation = driveLocation;
        DriveLabel = driveLabel;
        State = state;
    }

    public DevDrive()
    {
        ID = Guid.NewGuid();
    }

    /// <summary>
    /// Gets or sets the state associated with the Dev Drive. Default to the default value of Invalid.
    /// </summary>
    public DevDriveState State
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the drive letter for the Dev Drive.
    /// </summary>
    public char DriveLetter
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the size for the Dev Drive. This size is represented in base2 where one kilobyte is
    /// 1024 bytes.
    /// </summary>
    public ulong DriveSizeInBytes
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the file system location of the Dev Drive. This should be a fully qualified folder path.
    /// </summary>
    public string DriveLocation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the drive label that will be used to identify the Dev Drive in the file system.
    /// </summary>
    public string DriveLabel
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the byte unit of measure for the Dev Drive.
    /// </summary>
    public ByteUnit DriveUnitOfMeasure
    {
        get; set;
    }

    /// <summary>
    /// Gets the app internal ID of the dev drive. This is only used within the app.
    /// </summary>
    public Guid ID
    {
        get;
    }

    /// <summary>
    /// Swaps the DriveLabel , DriveLetter, DrizeSizeInBytes, DriveLocation and DriveUnitOfMearsure
    /// and the Drive state of two Dev Drives.
    /// </summary>
    public static void SwapContent(DevDrive devDriveA, DevDrive devDriveB)
    {
        (devDriveA.DriveLabel, devDriveB.DriveLabel) = (devDriveB.DriveLabel, devDriveA.DriveLabel);

        (devDriveA.DriveLetter, devDriveB.DriveLetter) = (devDriveB.DriveLetter, devDriveA.DriveLetter);

        (devDriveA.DriveSizeInBytes, devDriveB.DriveSizeInBytes) = (devDriveB.DriveSizeInBytes, devDriveA.DriveSizeInBytes);

        (devDriveA.DriveLocation, devDriveB.DriveLocation) = (devDriveB.DriveLocation, devDriveA.DriveLocation);

        (devDriveA.DriveUnitOfMeasure, devDriveB.DriveUnitOfMeasure) = (devDriveB.DriveUnitOfMeasure, devDriveA.DriveUnitOfMeasure);

        (devDriveA.State, devDriveB.State) = (devDriveB.State, devDriveA.State);
    }
}
