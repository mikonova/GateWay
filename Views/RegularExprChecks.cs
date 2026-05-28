using System.Text.RegularExpressions;
using Avalonia.Input;

namespace GateWay.Views;

public partial class MainWindow 
{
    private static bool _isValidUsername(string username)
    {
        // Только буквы, цифры и подчеркивание, 4-20 символов
        return Regex.IsMatch(username, @"^[a-zA-Z0-9_а-яА-ЯёЁ]{4,20}$");
    }
    
    private static bool _isValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;
        string pattern = @"^(?=.*[a-zA-Zа-яА-ЯёЁ])(?=.*\d).{8,}$";

        return Regex.IsMatch(password, pattern);
    }
    
}