using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ColorPalette;

namespace GateWay.Views;

public partial class MainWindow
{
    private ContextMenu createContextMenu(SolidColorBrush borderBrush, SolidColorBrush bgBrush, string chatId)
    {
        MenuItem RemoveItem = new MenuItem()
        {
            Header = "Удалить", 
            Name = "RemoveOption", 
            FontSize = 16,
            CornerRadius = new CornerRadius(5),
            Opacity = 1.0,
            Margin = new Thickness(5)
        };
        RemoveItem.Click += (_, _) =>
        {
            DeleteChat(chatId);
        };
        ContextMenu ChatMenu = new ContextMenu
        {
            BorderThickness = new Thickness(0),
            Background = bgBrush,
            CornerRadius = new CornerRadius(5),
            Opacity = 1.0
        };
        ChatMenu.Items.Add(RemoveItem);
        return ChatMenu;
    }

    
}