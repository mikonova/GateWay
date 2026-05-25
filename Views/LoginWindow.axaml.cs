using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using CoreClasses;
using System.Threading.Tasks;
namespace GateWay.Views;

public partial class LoginWindow : Window
{
    private Templates _template;
    private MainWindow _window;
    public LoginWindow(Templates template, MainWindow window)
    {
        InitializeComponent();
        _template = template;
        _window = window;
    }

    private void CopyButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (!string.IsNullOrEmpty(PublicKeyTextBlock.Text))
        {
            clipboard.SetTextAsync(PublicKeyTextBlock.Text);
        }
        else
        {
            var box = MessageBoxManager.GetMessageBoxStandard("Ой!",
                "Кажется произошла ошибка и ключ не сгенерировался", ButtonEnum.Ok);
            box.ShowAsync();
        }
    }

    private void OkLabel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _summonMsBox();
    }
    private async Task _summonMsBox()
    {
        var msBox = MessageBoxManager.GetMessageBoxStandard("Вы уверены?",
            "У вас больше не будет возможности увидеть ключ внутри программы", ButtonEnum.YesNo);
        var Choice = await msBox.ShowWindowDialogAsync(this);
        if (Choice == ButtonResult.Yes)
        {
            _window.LoginScreen.IsVisible = false;
            _window.UserLogged.IsVisible = true;
            this.Close();
        }
        else if (Choice == ButtonResult.No)
        {

        }
    }
}