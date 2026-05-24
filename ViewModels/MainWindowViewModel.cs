using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using GateWay.Views;
using GateWay;

namespace GateWay.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
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

    public async Task CheckUserSession()
    {
        if (_isUserSessionActive)
        {
            App.mainWindow.LoginScreen.IsVisible = false;
            App.mainWindow.UserLogged.IsVisible = true;
        }
        else
        {
            App.mainWindow.UserLogged.IsVisible = true;
            App.mainWindow.UserLogged.IsVisible = false;
        }
    }
}
