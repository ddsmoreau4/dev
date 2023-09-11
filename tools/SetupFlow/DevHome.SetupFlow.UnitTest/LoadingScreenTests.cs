﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using DevHome.Common.Extensions;
using DevHome.SetupFlow.Services;
using DevHome.SetupFlow.ViewModels;

namespace DevHome.SetupFlow.UnitTest;

[TestClass]
public class LoadingScreenTests : BaseSetupFlowTest
{
    [TestMethod]
    public void HideRetryBannerTest()
    {
        var loadingViewModel = TestHost!.GetService<LoadingViewModel>();

        loadingViewModel.HideMaxRetryBanner();

        Assert.IsFalse(loadingViewModel.ShowOutOfRetriesBanner);
    }
}
