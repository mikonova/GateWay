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

namespace GateWay.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
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
        this.Close();
    }
}