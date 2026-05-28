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
    
    // ЛОГИН ФОРМА
    
       private async void AcceptLogin_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        AcceptName.Background = ColorPaletteNebula.ChatPress;
        string _login = RegUserNameField.Text;
        string _password = RegUserPassField.Text;
        if (!string.IsNullOrEmpty(_login) &&
            !string.IsNullOrEmpty(_password))
        {
            try
            {
                await _template.LoginUser(_login, _password);
            }
            catch (Exception ex)
            {
                LoginWarning.Text = $"Ошибка: {ex.Message}";
                LoginWarning.IsVisible = true;
                return;
            }
        }
        else if (string.IsNullOrEmpty(_login))
        {
            LoginWarning.Text = "Заполните поле логина";
            LoginWarning.IsVisible = true;
        }
        else if (string.IsNullOrEmpty(_password))
        {
            LoginWarning.Text = "Заполните поле пароля";
            LoginWarning.IsVisible = true;
        }
    }

    private void AcceptLogin_OnPointerExited(object? sender, PointerEventArgs e)
    {
        AcceptName.Background = ColorPaletteNebula.ChatCloudColor;
    }

    private void AcceptLogin_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        AcceptName.Background = ColorPaletteNebula.ChatHover;
    }
    private void BackButtonLogin_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        BackButtonSimple.Background = ColorPaletteNebula.ChatHover;
    }

    private void BackButtonLogin_OnPointerExited(object? sender, PointerEventArgs e)
    {
        BackButtonSimple.Background = ColorPaletteNebula.OnBgColor;
    }
    private void BackButtonLogin_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        LoginWarning.Text = "";
        LoginUserNameField.Clear();
        LoginPassField.Clear();
        LoginSuggestion.IsVisible = true;
        RegistrationForm.IsVisible = false;
    }
}