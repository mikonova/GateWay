using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using ColorPalette;
using GateWay.ViewModels;

namespace GateWay.Views;
public partial class MainWindow
{
    
    private Border? _selectedChat;
    public async void AddChatToList(string chatId, string senderAlias, string lastSentence, bool isSelf)
    {

        Border border = new Border
        {
            Background = ColorPaletteNebula.BackgroundColor,
            Name = chatId,
            Width = 250,
            Height = 60,
            Padding = new Thickness(4, 2, 4, 2)
        };
        border.ContextMenu = createContextMenu(ColorPaletteNebula.OnBgColor, ColorPaletteNebula.OnBgColor, border.Name);

        Grid grid = new Grid() { RowSpacing = 4, VerticalAlignment = VerticalAlignment.Top };
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        TextBlock userName = new TextBlock
        {
            FontSize = 20,
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeight.Bold,
            Text = $"{senderAlias}"
        };
        TextBlock sender = new TextBlock
        {
            FontSize = 16,
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeight.Bold,
            Opacity = 0.8,
            Margin = new Thickness(0, 0, 5, 0)
        };
        TextBlock lastMsg = new TextBlock
        {
            FontSize = 16,
            TextAlignment = TextAlignment.Center,
            Opacity = 0.8,
            Text = lastSentence,
        };
        StackPanel messageInfo = new StackPanel()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Orientation = Orientation.Horizontal
        };

        if (isSelf)
        {
            sender.Text = "You:";
        }
        else
        {
            sender.Text = $"{senderAlias}:";
        }

        border.PointerEntered += (_, _) =>
        {
            if (_selectedChat != border)
            {
                border.Background = ColorPaletteNebula.ChatHover;
            }
        };
        border.PointerExited += (_, _) =>
        {
            if (_selectedChat != border)
            {
                border.Background = ColorPaletteNebula.BackgroundColor;
            }
        };
        border.Tapped += (_, _) =>
        {
            border.Background = ColorPaletteNebula.PressColor;
            if (_selectedChat != null && _selectedChat != border)
            {
                _selectedChat.Background = ColorPaletteNebula.BackgroundColor;
            }

            _selectedChat = border;
        };

        MainWindowViewModel._chatList.Add(border);
        border.Child = grid;
        grid.Children.Add(userName);
        grid.Children.Add(messageInfo);
        Grid.SetRow(userName, 0);
        Grid.SetRow(messageInfo, 1);
        messageInfo.Children.Add(sender);
        messageInfo.Children.Add(lastMsg);
        ChatList.Children.Add(border);

    }

    public async void DeleteChat(string chatId)
    {
        Border chat = MainWindowViewModel._chatList.Find(border => border.Name == chatId);
        MainWindowViewModel._chatList.Remove(chat);
        ChatList.Children.Remove(chat);
    }
}