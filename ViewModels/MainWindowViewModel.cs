using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using GateWay.Views;
using GateWay;
using CoreClasses;

namespace GateWay.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindow? CurrentWindow;
    private Templates _template;

    public Border? SelectedChat
    {
        get => SelectedChat;
        set
        {
            // LoadLastMessages - 
        }
    }
    public List<Border?> ChatList = new List<Border?>();
    private bool _isUserSessionActive;
    
    
    public bool IsUserSessionActive
    {
        get => _isUserSessionActive;
        set
        {
            _isUserSessionActive = value;
            CheckUserSession().Wait();
            _template.LoadAllChats();
        }
    }
    
    public MainWindowViewModel(Templates template)
    {
        _template = template;
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
