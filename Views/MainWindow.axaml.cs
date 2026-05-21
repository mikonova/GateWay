#pragma warning disable 4014
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

namespace GateWay.Views;

public partial class MainWindow : Window
{
    private bool _isUserSessionActive = false; // не должно стоять значения, пример

    public bool IsUserSessionActive
    {
        get => _isUserSessionActive;
        set => _isUserSessionActive = value;
    }

    public static MainWindow? Context;
    public MainWindow()
    {
        Context = this;
        InitializeComponent();
        if (_isUserSessionActive)
        {
            LoginScreen.IsVisible = false;
            UserLogged.IsVisible = true;
        }
        else
        {
            UserLogged.IsVisible = true;
            UserLogged.IsVisible = false;
        }
        // тесты ,удалить 
        AddChatToList("123", "miko", "Привчедел", false);
        AddChatToList("124", "Вася", "Го в кино", true);
        AddChatToList("125", "Леша", "Ты тут?", false);
        LoadMessage("123", "aboba", "safdfgsdWAUKJHGFDSSDHGFDSFSsgdfgfdsfddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddddsfd", "2026-05-21T14:30:45+02:00", true);
        LoadMessage("123", "aboba", "safdfgsdsfd", "2026-05-21T14:30:45Z", false);
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
            BtnFullscreen.Text = "□";
        }
        else
        {
            this.WindowState = WindowState.FullScreen;
            BtnFullscreen.Text = "■";
        }
    }

    private void BtnToTray_OnClick(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void CreateUserLabel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatPress;
        LoginWindow LoginInfo = new LoginWindow();
        LoginInfo.ShowDialog(this);
    }

    private void CreateUserLabel_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatHover;
    }

    private void CreateUserLabel_OnPointerExited(object? sender, PointerEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatCloudColor;
    }
}