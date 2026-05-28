using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ColorPalette;
using CoreClasses;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace GateWay.Views;

public partial class AddNewChatWindow : Window
{
    private Templates _templates;
    public AddNewChatWindow(Templates template)
    {
        _templates = template;
        InitializeComponent();
    }
    private void AddButton_OnPointerEntered(object? s, PointerEventArgs e) => AddButton.Background = ColorPaletteNebula.ChatHover;
    private void AddButton_OnPointerExited(object? s, PointerEventArgs e) => AddButton.Background = ColorPaletteNebula.ChatCloudColor;
    private void AddButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        string name = UserNameField.Text;
        string key = PublicKeyField.Text;
        if (!string.IsNullOrEmpty(name) &&
            !string.IsNullOrWhiteSpace(name) &&
            !string.IsNullOrWhiteSpace(key) &&
            !string.IsNullOrEmpty(key))
        {
            var _convertedKey = Convert.FromBase64String(key);
            try
            { 
               string ChatId = _templates.CreateChat(name, _convertedKey);
                MainWindow window = Owner as MainWindow;
                window.AddChatToList(ChatId, name, "Начните общение!", false);
            }
            catch (Exception ex)
            {
                var msbox = MessageBoxManager.GetMessageBoxStandard("Ой!",
                    $"Произошла ошибка {ex}", ButtonEnum.Ok);
                msbox.ShowAsPopupAsync(this);
            }
        }
    }
    
    
    private void CancelButton_OnPointerEntered(object? s, PointerEventArgs e) => CancelButton.Background = ColorPaletteNebula.OnBgColor;
    private void CancelButton_OnPointerExited(object? s, PointerEventArgs e) => CancelButton.Background = Brushes.Transparent;
    private void CancelButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        this.Close();
    }
}