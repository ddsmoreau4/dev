// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using DevHome.Common.Extensions;
using DevHome.Common.Helpers;

using DevHome.SetupFlow.DevDrive.ViewModels;
using DevHome.SetupFlow.DevDrive.Views;
using Microsoft.UI.Xaml;
using WinUIEx;

namespace DevHome.SetupFlow.DevDrive.Windows;

public sealed partial class DevDriveWindow : SecondaryWindowExtension
{
    private readonly DevDriveViewModel _devDriveViewModel;
    private readonly DevDriveView _devDriveView;
    private readonly double _initialHeight = 600;
    private readonly double _initialWidth = 600;

    public DevDriveWindow(DevDriveViewModel viewModel)
        : base()
    {
        _devDriveViewModel = viewModel;
        _devDriveView = new DevDriveView(viewModel);
        this.SetIcon(viewModel.AppImage);
        Title = viewModel.AppTitle;
        Content = _devDriveView;
        ExtendsContentIntoTitleBar = true;
        SetTitleBar(_devDriveView.TitleBar);
        UseAppTheme = true;
        Activated += UpdateTitleBarTextColors;
        MinHeight = _initialHeight;
        Height = _initialHeight;
        MinWidth = _initialWidth;
        Width = _initialWidth;
    }

    public DevDriveViewModel ViewModel => _devDriveViewModel;

    public void UpdateTitleBarTextColors(object sender, WindowActivatedEventArgs args)
    {
        var colorBrush = TitleBarHelper.GetTitleBarTextColorBrush(args.WindowActivationState);
        _devDriveView.UpdateTitleBarTextForeground(colorBrush);
    }
}
