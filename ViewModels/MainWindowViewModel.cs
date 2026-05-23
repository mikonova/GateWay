using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using GateWay.Views;
using GateWay;

namespace GateWay.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private App app;
    public MainWindowViewModel()
    {
        app = App.Current as App;
    }
    public static List<Border?> _chatList = new List<Border?>();
    
    private bool _isUserSessionActive; // не должно стоять значения, пример
    public bool IsUserSessionActive
    {
        get => _isUserSessionActive;
        set
        {
            _isUserSessionActive = value;
            CheckUserSession().Wait();
        }
    }

    private async Task CheckUserSession()
    {
        if (_isUserSessionActive)
        {
            app.mainWindow.LoginScreen.IsVisible = false;
            app.mainWindow.UserLogged.IsVisible = true;
        }
        else
        {
            app.mainWindow.UserLogged.IsVisible = true;
            app.mainWindow.UserLogged.IsVisible = false;
        }
    }
}
