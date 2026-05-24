using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using GateWay.Views;
using GateWay;

namespace GateWay.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private App CurrentApp;
    public MainWindowViewModel()
    {
        CurrentApp = App.Current as App;
    }
    public List<Border?> _chatList = new List<Border?>();
    
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
            CurrentApp.mainWindow.LoginScreen.IsVisible = false;
            CurrentApp.mainWindow.UserLogged.IsVisible = true;
        }
        else
        {
            CurrentApp.mainWindow.UserLogged.IsVisible = true;
            CurrentApp.mainWindow.UserLogged.IsVisible = false;
        }
    }
}
