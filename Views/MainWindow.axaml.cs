#pragma warning disable 4014
#pragma warning disable AVLN3001
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ColorPalette;
using CoreClasses;
using GateWay.ViewModels;

namespace GateWay.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel _mainWindowViewModel;
    private Templates _template;

    public MainWindow(MainWindowViewModel ViewModel, Templates template)
    {
        _template = template;
        _mainWindowViewModel = ViewModel;
        InitializeComponent();
        // тесты ,удалить 
        AddChatToList("123", "miko", "Привчедел", false);
        //AddChatToList("124", "Вася", "Го в кино", true);
        //AddChatToList("125", "Леша", "Ты тут?", false);
        //LoadMessage("123", "aboba", "safdfgsdWAUKJHGFDSSDHGFDSFSsgdfgfdsfddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddsfd", "2026-05-21T14:30:45+02:00", true);
        //LoadMessage("123", "aboba", "safdfgsdsfd", "2026-05-21T14:30:45Z", false);
        
    }
    private void TopBar_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void PaneExpand_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SidePane.IsPaneOpen)
        {
            SidePane.IsPaneOpen = false;
            PaneContent.IsVisible = false;
            PaneContent.IsEnabled = false;
            //PaneBorder.Width = SidePane.CompactPaneLength;
        }
        else
        {
            SidePane.IsPaneOpen = true;
            PaneContent.IsVisible = true;
            PaneContent.IsEnabled = true;

            //PaneBorder.Width = SidePane.OpenPaneLength;
        }
    }

    private void BtnClose_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void BtnFullscreen_OnClick(object? sender, RoutedEventArgs e)
    {
        if(this.WindowState == WindowState.FullScreen)
        {
            this.WindowState = WindowState.Normal;
            BtnFullscreen.Content = "□";
        }
        else
        {
            this.WindowState = WindowState.FullScreen;
            BtnFullscreen.Content = "■";
        }
    }

    private void BtnToTray_OnClick(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
    // вызов формы регистрации
    private void CreateUserLabel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatPress;
        LoginSuggestion.IsVisible = false;
        RegistrationForm.IsVisible = true;
    }

    private void CreateUserLabel_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatHover;
    }

    private void CreateUserLabel_OnPointerExited(object? sender, PointerEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatCloudColor;
    }

    // обработка логина
    private void AcceptName_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        AcceptName.Background = ColorPaletteNebula.ChatPress;
        string Login = UserLogin.Text;
        string PublicKey = Convert.ToBase64String(_template.GetMyPublicKey());
        if (!string.IsNullOrEmpty(Login) && _isValidUsername(Login) )
        {
            _template.RegistrationUser(Login).Wait(5000); // ждем 5 сек -> таймаут
            if (string.IsNullOrEmpty(PublicKey))
            {
                UserLogin.Clear();
            }
            else
            {
                LoginWindow LoginInfo = new LoginWindow(_template, this, PublicKey); 
                LoginInfo.ShowDialog(this);
            }
        }
        else
        {
            UserWarningLength.IsVisible = true;
            UserLogin.Clear();
        }
    }

    private void AcceptName_OnPointerExited(object? sender, PointerEventArgs e)
    {
        AcceptName.Background = ColorPaletteNebula.ChatCloudColor;
    }

    private void AcceptName_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        AcceptName.Background = ColorPaletteNebula.ChatHover;
    }


    private void LogUserLabel_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        LogUserLabel.Background = ColorPaletteNebula.ChatHover;
    }

    private void LogUserLabel_OnPointerExited(object? sender, PointerEventArgs e)
    {
        LogUserLabel.Background = ColorPaletteNebula.ChatCloudColor;
    }
    // Вызов логин формы
    private void LogUserLabel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        LogUserLabel.Background = ColorPaletteNebula.ChatPress;
        LoginSuggestion.IsVisible = false;
        LoginForm.IsVisible = true;
    }
}