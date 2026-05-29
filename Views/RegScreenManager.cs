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
        private async void AcceptName_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        AcceptName.Background = ColorPaletteNebula.ChatPress;
        string _login = RegUserNameField.Text;
        string _password = RegUserPassField.Text;
        string PublicKey = String.Empty;
        if (!string.IsNullOrEmpty(_login) &&
            !string.IsNullOrEmpty(_password) &&
            _isValidUsername(_login) &&
            _isValidPassword(_password))
        {
            try
            {
                await _template.RegistrationUser(_login, _password);
            }
            catch (Exception ex)
            {
                UserWarning.Text = $"Ошибка: {ex.Message}";
                UserWarning.IsVisible = true;
                return;
            }
            byte[]? key = _template.GetMyPublicKey();
            if (string.IsNullOrEmpty(Convert.ToBase64String(key)))
            {
                UserWarning.Text = "Ошибка регистрации — ключ не сохранён";
                UserWarning.IsVisible = true;
                return;
            }
            PublicKey = Convert.ToBase64String(key);
            CopyKeyWindow LoginInfo = new CopyKeyWindow(_template, this, PublicKey); 
            LoginInfo.ShowDialog(this);
        }
        else if (!_isValidUsername(_login))
        {
            UserWarning.Text = "Минимальная Длина имени - 4 символа";
            UserWarning.IsVisible = true;
            RegUserNameField.Clear();
            return;
        }
        else if (!RegUserPassField.Text.Equals(RegUserPassRepField.Text))
        {
            UserWarning.Text = "Пароли не совпадают!";
            UserWarning.IsVisible = true;
            return;
        }
        else if (RegUserPassField.Text.Length < 8)
        {
            UserWarning.Text = "Минимальный размер - 8 символов";
            UserWarning.IsVisible = true;
            return;
        }
        else if (!_isValidPassword(_password))
        {
            UserWarning.Text = "Пароль должен содержать цифры и буквы";
            UserWarning.IsVisible = true;
            return;
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
    private void BackButtonSimple_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        BackButtonSimple.Background = ColorPaletteNebula.ChatHover;
    }

    private void BackButtonSimple_OnPointerExited(object? sender, PointerEventArgs e)
    {
        BackButtonSimple.Background = new SolidColorBrush(Colors.Transparent);
    }
    private void BackButtonSimple_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        UserWarning.Text = "";
        RegUserNameField.Clear();
        RegUserPassField.Clear();
        RegUserPassRepField.Clear();
        RegistrationForm.IsVisible = false;
        LoginSuggestion.IsVisible = true;
    }
    
}