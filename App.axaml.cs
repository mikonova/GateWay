using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using GateWay.ViewModels;
using GateWay.Views;
using CoreClasses;

namespace GateWay;

public partial class App : Application
{

    public MainWindowViewModel mainWindowViewModel;
    public MainWindow mainWindow;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            mainWindowViewModel = new MainWindowViewModel();
            mainWindow = new MainWindow(mainWindowViewModel);
            mainWindowViewModel.CurrentWindow = mainWindow;
            
            desktop.MainWindow = mainWindow;
            desktop.MainWindow.DataContext = mainWindowViewModel;
        }
        base.OnFrameworkInitializationCompleted();
    }

    public void PostInitActions()
    {
        Templates appCurrentTemplate = new Templates(AppDomain.CurrentDomain.BaseDirectory.ToString(), mainWindow, mainWindowViewModel);
        mainWindowViewModel.IsUserSessionActive = appCurrentTemplate.IsUserRegistered();
    }
}