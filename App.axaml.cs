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
    public static MainWindow mainWindow;
    MainWindowViewModel mainWindowViewModel = new MainWindowViewModel();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            mainWindowViewModel.CheckUserSession().Wait();
            MainWindow mainWindow = new MainWindow();
            desktop.MainWindow = mainWindow;
            {
                DataContext = mainWindowViewModel;

            };
            
        }
        base.OnFrameworkInitializationCompleted();
    }
}