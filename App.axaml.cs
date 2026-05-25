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

    private MainWindowViewModel _mainWindowViewModel;
    private MainWindow _mainWindow;
    private Templates _templates;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _templates = new Templates(AppDomain.CurrentDomain.BaseDirectory.ToString());
            _mainWindowViewModel = new MainWindowViewModel(_templates);
            _mainWindow = new MainWindow(_mainWindowViewModel, _templates);
            _mainWindowViewModel.CurrentWindow = _mainWindow;
            _templates.MainWindow = _mainWindow;
            _templates.MainWindowViewModel = _mainWindowViewModel;
            
            desktop.MainWindow = _mainWindow;
            desktop.MainWindow.DataContext = _mainWindowViewModel;
        }
        base.OnFrameworkInitializationCompleted();
        _mainWindow.RegistrationForm.IsVisible = false;
        _mainWindow.LoginForm.IsVisible = false;
        _mainWindowViewModel.IsUserSessionActive = _templates.IsUserRegistered();
    }
    


}