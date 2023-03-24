﻿// Copyright (c) Microsoft Corporation and Contributors
// Licensed under the MIT license.

using DevHome.Common.Extensions;
using DevHome.SetupFlow.AppManagement.Models;
using DevHome.SetupFlow.AppManagement.ViewModels;
using DevHome.SetupFlow.UnitTest.Helpers;
using Microsoft.Management.Deployment;
using Moq;

namespace DevHome.SetupFlow.UnitTest.ViewModels;

[TestClass]
public class PackageViewModelTest : BaseSetupFlowTest
{
    private const string MockPackageUrl = "https://mock/packageUrl";
    private const string MockPublisherUrl = "https://mock/publisherUrl";
    private const string WinGetPkgsUrl = "https://github.com/microsoft/winget-pkgs";
    private const string MsStoreAppUrl = "ms-windows-store://pdp/?productid=mockId";

    [TestMethod]
    public void CreatePackageViewModel_Success()
    {
        var packageCatalog = PackageHelper.CreatePackageCatalog(10);
        var packageCatalogViewModel = TestHost!.CreateInstance<PackageCatalogViewModel>(packageCatalog);
        var expectedPackages = packageCatalog.Packages.ToList();
        var packages = packageCatalogViewModel.Packages.ToList();
        Assert.AreEqual(expectedPackages.Count, packages.Count);
        for (var i = 0; i < expectedPackages.Count; ++i)
        {
            Assert.AreEqual(expectedPackages[i].Name, packages[i].Name);
            Assert.AreEqual(expectedPackages[i].Version, packages[i].Version);
        }
    }

    [TestMethod]
    [DataRow(MockPackageUrl, MockPublisherUrl, MockPackageUrl)]
    [DataRow("", MockPublisherUrl, MockPublisherUrl)]
    [DataRow("", "", WinGetPkgsUrl)]
    public void LearnMore_PackageFromWinGetOrCustomCatalog_ReturnsExpectedUri(string packageUrl, string publisherUrl, string expectedUrl)
    {
        // Arrange
        WindowsPackageManager!.Setup(wpm => wpm.MsStoreId).Returns("mockMsStoreId");
        var package = PackageHelper.CreatePackage("mockId");
        package.Setup(p => p.CatalogId).Returns("mockWinGetCatalogId");
        package.Setup<Uri?>(p => p.PackageUrl).Returns(string.IsNullOrEmpty(packageUrl) ? null : new Uri(packageUrl));
        package.Setup<Uri?>(p => p.PublisherUrl).Returns(string.IsNullOrEmpty(publisherUrl) ? null : new Uri(publisherUrl));

        // Act
        var packageViewModel = TestHost!.CreateInstance<PackageViewModel>(package.Object);

        // Assert
        Assert.AreEqual(expectedUrl, packageViewModel.GetLearnMoreUri().ToString());
    }

    [TestMethod]
    [DataRow(MockPackageUrl, MockPublisherUrl, MockPackageUrl)]
    [DataRow("", MockPublisherUrl, MsStoreAppUrl)]
    [DataRow("", "", MsStoreAppUrl)]
    public void LearnMore_PackageFromMsStoreCatalog_ReturnsExpectedUri(string packageUrl, string publisherUrl, string expectedUrl)
    {
        // Arrange
        WindowsPackageManager!.Setup(wpm => wpm.MsStoreId).Returns("mockMsStoreId");
        var package = PackageHelper.CreatePackage("mockId");
        package.Setup(p => p.CatalogId).Returns(WindowsPackageManager!.Object.MsStoreId);
        package.Setup<Uri?>(p => p.PackageUrl).Returns(string.IsNullOrEmpty(packageUrl) ? null : new Uri(packageUrl));
        package.Setup<Uri?>(p => p.PublisherUrl).Returns(string.IsNullOrEmpty(publisherUrl) ? null : new Uri(publisherUrl));

        // Act
        var packageViewModel = TestHost!.CreateInstance<PackageViewModel>(package.Object);

        // Assert
        Assert.AreEqual(expectedUrl, packageViewModel.GetLearnMoreUri().ToString());
    }
}
