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
    public static MainWindow mainWindow = new MainWindow();
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = mainWindow;
            {
                DataContext = new MainWindowViewModel();

            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}