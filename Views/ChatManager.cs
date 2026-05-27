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
using GateWay;

namespace GateWay.Views;

public class Chat
{
    private int _pagesLoaded = 0;
    public Border? ChatBorder;
    public readonly string ChatId;
    public Chat(Border border)
    {
        ChatBorder = border;
        ChatId = border.Name;
    }

    public int GetPagesLoaded()
    {
        return _pagesLoaded;
    }

    private int AddPage()
    {
        _pagesLoaded++;
        return _pagesLoaded;
    }
}
public partial class MainWindow
{
    public async void AddChatToList(string chatId, string senderAlias, string lastSentence, bool isSelf)
    {
        // string chatId, string senderAlias, string lastSentence, bool isSelf
        Border? border = new Border
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

        border.Child = grid;
        grid.Children.Add(userName);
        grid.Children.Add(messageInfo);
        Grid.SetRow(userName, 0);
        Grid.SetRow(messageInfo, 1);
        messageInfo.Children.Add(sender);
        messageInfo.Children.Add(lastMsg);
        Chat? chat = new Chat(border);

        border.PointerEntered += (_, _) =>
        {
            if (_mainWindowViewModel.CurrentChat != chat)
            {
                border.Background = ColorPaletteNebula.ChatHover;
            }
        };
        border.PointerExited += (_, _) =>
        {
            if (_mainWindowViewModel.CurrentChat != chat)
            {
                border.Background = ColorPaletteNebula.BackgroundColor;
            }
        };
        border.Tapped += (_, _) =>
        {
            border.Background = ColorPaletteNebula.PressColor;
            if (_mainWindowViewModel.CurrentChat.ChatId != chat.ChatId)
            {
                _mainWindowViewModel.CurrentChat.ChatBorder.Background = ColorPaletteNebula.BackgroundColor;
                _mainWindowViewModel.SelectedChat = chat;
            }
        };

        _mainWindowViewModel.ChatList.Add(chat);
        ChatList.Children.Add(border);

        

    }

    public async void DeleteChat(string chatId)
    {
        Chat chat = _mainWindowViewModel.ChatList.Find(chat => chat.ChatBorder.Name == chatId);
        _mainWindowViewModel.ChatList.Remove(chat);
        ChatList.Children.Remove(chat.ChatBorder);
    }
    
}