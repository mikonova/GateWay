using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using GateWay.ViewModels;
using GateWay.Views;

namespace GateWay;

public partial class App : Application
{
    public MainWindow mainWindow = new MainWindow();
    public MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
            desktop.MainWindow.DataContext = mainWindowViewModel;
        }
        base.OnFrameworkInitializationCompleted();
    }
}