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

    public Chat? CurrentChat = new Chat(new Border());
    public Chat? SelectedChat
    {
        get => CurrentChat;
        set
        {
            CurrentChat = value;
            _template.LoadMessages(value.ChatId, 0);
            CurrentWindow.MessageScroller.ScrollToEnd();
        }
    }
    public List<Chat> ChatList = new List<Chat>();
    private bool _isUserSessionActive;
    private int _sessionCheckupRes;
    
    public bool IsUserSessionActive
    {
        get => _isUserSessionActive;
        set
        {
            _isUserSessionActive = value;
            AsyncUserCheckUp();
        }
    }
    
    public MainWindowViewModel(Templates template)
    {
        _template = template;
    }

    private async void AsyncUserCheckUp()
    {
        await CheckUserSession();
    }
    
    private Task CheckUserSession()
    {
        if (_isUserSessionActive)
        {
            CurrentWindow.LoginScreen.IsVisible = false;
            CurrentWindow.UserLogged.IsVisible = true;
            _template.LoadAllChats();
        }
        else
        {
            CurrentWindow.LoginScreen.IsVisible = true;
            CurrentWindow.UserLogged.IsVisible = false;
        }
        return Task.CompletedTask;
    }
}
