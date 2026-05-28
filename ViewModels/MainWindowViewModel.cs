using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using GateWay.Views;
using GateWay;
using CoreClasses;
using System.IO;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;

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
            CurrentWindow?.ChatSendBar.IsVisible = true;
            CurrentWindow?.StackMessages.Children.Clear();
            CurrentChat = value;
            try
            {
                _template.LoadMessages(value.ChatId, 0);
            }
            catch (DirectoryNotFoundException ex)
            {
                var box = MessageBoxManager.GetMessageBoxStandard("Ой!",
                    $"Ошибка загрузки сообщений:\n {ex.Message}", ButtonEnum.Ok);
                box.ShowAsPopupAsync(CurrentWindow);
            }

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
            CurrentWindow.LoginScreen.IsVisible = true;
            CurrentWindow.LoginForm.IsVisible = true;
        }
        else
        {
            CurrentWindow.LoginScreen.IsVisible = true;
            CurrentWindow.LoginSuggestion.IsVisible = true;
        }
        return Task.CompletedTask;
    }
}
