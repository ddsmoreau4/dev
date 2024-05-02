﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevHome.Common.Environments.Scripts;

public static class HyperVSetupScript
{
    public const string SetupFunction = @"
        <#
            This script does the following:
            1. Checks if the user is in the Hyper-V Administrators group and adds them if they are not in it
            2. Checks if the Hyper-V Feature is enabled and enables the feature if it is not already enabled.
            3. If the user is in the Hyper-V Administrators group and the Hyper-V feature is enabled. The script does nothing.

            The script uses the following exit codes to signal to Dev Home what message it should show to the user.

            Exit Codes (Note: we don't use exit code of 1 since that's the generic exit code if there was an exception):
            0. The script successfully added the user to the Hyper-V Admin Group and enabled the Hyper-V Feature.
            2. The user is not in the Hyper-V Admin Group and the Hyper-V Feature is already enabled. The script successfully added the user to the Hyper-V Admin group.
            3. The user is not in the Hyper-V Admin Group and the Hyper-V Feature is already enabled. The script failed to add the user to the Hyper-V Admin group.
            4. The user is in the Hyper-V Admin group and the Hyper-V Feature is not already enabled. The script successfully enabled the Hyper-Feature.
            5. The user is in the Hyper-V Admin group and the Hyper-V Feature is not already enabled. The script failed to enable the Hyper-Feature.
            6. The user is already in the Hyper-V Admin group and the Hyper-V Feature is already enabled.
        #>
        function Setup-HyperVForDevHome()
        {
            # For cmdlet operation results so we can compare results and choose the right exit code
            $operationSucceeded = 0
            $operationFailed = 1
            $operationNotRun = 2
            $featureEnablementResult = $operationNotRun
            $adminGroupResult = $operationNotRun
            

            # Check if the user is in the Hyper-V Administrators group
            $isUserInGroup = Get-LocalGroupMember -Group 'Hyper-V Administrators' | Where-Object { $_.Name -eq ( [System.Security.Principal.WindowsIdentity]::GetCurrent().Name) }

            # Check if the Hyper-V feature is enabled
            $featureState = Get-WindowsOptionalFeature -FeatureName 'Microsoft-Hyper-V' -Online | Select-Object -ExpandProperty State
            $featureEnabled = $featureState -eq 'Enabled'

            if ($isUserInGroup -and $featureEnabled)
            {
                # User already in Admin group and feature already enabled
                exit 6
            }
            
            # Add user to Hyper-v Administrators group if they aren't already in the group
            if (-not $isUserInGroup) 
            {
                Add-LocalGroupMember -Group 'Hyper-V Administrators' -Member ( [System.Security.Principal.WindowsIdentity]::GetCurrent().Name)

                # Check if the last command succeeded
                if ($?)
                {
                    # User added to the Hyper-V Administrators group.
                    $adminGroupResult = $operationSucceeded
                }
                else
                {
                    # Failed to add user to the Hyper-V Administrators group.
                    $adminGroupResult = $operationFailed
                }
            }

            # Enable the Hyper-V feature if it is not already enabled
            if (-not $featureEnabled)
            {
                $dsimHyperVFeature = Enable-WindowsOptionalFeature -Online -FeatureName 'Microsoft-Hyper-V' -All -NoRestart
                
                # when $dsimHyperVFeature is not null we've enabled the feature successfully
                if ($null -ne $dsimHyperVFeature)
                {
                    # Hyper-V feature enabled successfully.
                    $featureEnablementResult = $operationSucceeded
                }
                else
                {    
                    # Failed to enable the Hyper-V feature.
                    $featureEnablementResult = $operationFailed
                }
            }

            # We added the user to the admin group and enabled the Hyper-V feature during this script
            if ($adminGroupResult -eq $operationSucceeded -and $FeatureEnablementResult -eq $operationSucceeded)
            {
                exit 0
            }
            # We added the user to the admin group but the Hyper-V feature was already enabled before this script ran
            elif ($adminGroupResult -eq $operationSucceeded -and $FeatureEnablementResult -eq $operationNotRun)
            {
                exit 2
            }
            # We failed to add the user to the admin group and the Hyper-V feature was already enabled before this script ran
            elif ($adminGroupResult -eq $operationFailed -and $FeatureEnablementResult -eq $operationNotRun)
            {
                exit 3
            }
            # We enabled the Hyper-V feature but the user was already in the Hyper-V admin group before this script ran
            elif ($FeatureEnablementResult -eq $operationSucceeded  -and $adminGroupResult -eq $operationNotRun)
            {
                exit 4
            }
            # We failed to enable the Hyper-V feature and the user was already in the Hyper-V admin group before this script ran
            elif ($FeatureEnablementResult -eq $operationFailed -and $adminGroupResult -eq $operationNotRun)
            {
                exit 5
            }

            # We shouldn't ever get here
            exit 99
        }

        # Run script
        Setup-HyperVForDevHome
        ";
}
