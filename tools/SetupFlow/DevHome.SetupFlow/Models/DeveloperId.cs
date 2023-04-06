﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using Microsoft.Windows.DevHome.SDK;

namespace DevHome.SetupFlow.Models;

/// <summary>
/// Concrete DeveloperId.  Used when a user selects a repo from the URL page
/// because URL page does not require a login.
/// </summary>
public class DeveloperId : IDeveloperId
{
    private readonly string _displayName;

    private readonly string _email;

    private readonly string _loginId;

    private readonly string _url;

    public string DisplayName() => _displayName;

    public string Email() => _email;

    public string LoginId() => _loginId;

    public string Url() => _url;

    public DeveloperId(string displayName, string email, string loginId, string url)
    {
        _displayName = displayName;
        _email = email;
        _loginId = loginId;
        _url = url;
    }
}
