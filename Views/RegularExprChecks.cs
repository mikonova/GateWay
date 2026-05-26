using System.Text.RegularExpressions;

namespace GateWay.Views;

public partial class MainWindow 
{
    private static bool _isValidUsername(string username)
    {
        // Только буквы, цифры и подчеркивание, 4-20 символов
        return Regex.IsMatch(username, @"^[a-zA-Z0-9_а-яА-ЯёЁ]{4,20}$");
    }
}