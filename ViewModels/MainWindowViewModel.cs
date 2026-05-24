using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using GateWay.Views;
using GateWay;

namespace GateWay.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindow? CurrentWindow;
    public MainWindowViewModel()
    {
        
    }
    public List<Border?> ChatList = new List<Border?>();

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
            CurrentWindow.LoginScreen.IsVisible = false;
            CurrentWindow.UserLogged.IsVisible = true;
        }
        else
        {
            CurrentWindow.LoginScreen.IsVisible = true;
            CurrentWindow.UserLogged.IsVisible = false;
        }
    }
}
