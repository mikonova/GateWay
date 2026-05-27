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

public partial class MainWindow
{
    // вызов формы РЕГИСТРАЦИИ
    private void CreateUserLabel_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatPress;
        LoginSuggestion.IsVisible = false;
        RegistrationForm.IsVisible = true;
        LoginForm.IsVisible = false;
    }

    private void CreateUserLabel_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatHover;
    }

    private void CreateUserLabel_OnPointerExited(object? sender, PointerEventArgs e)
    {
        CreateUserLabel.Background = ColorPaletteNebula.ChatCloudColor;
    }
    
    // ФОРМА РЕГИСТРАЦИИ: ОБРАБОТКА
    
    
}