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

namespace GateWay.Views;


public partial class MainWindow
{
    
    public async void LoadMessage(string chatId, string senderAlias, string content, string timeStamp, bool isOutgoing)
    {
        if (_mainWindowViewModel.SelectedChat == null)
        {
            //throw new Exception("No selected chat!");
        }
        DateTime dateTime = DateTime.Parse(timeStamp);
        string date = dateTime.ToString("dd.MM.yyyy");
        string time =  dateTime.ToString("HH:mm");
        string UnitedDate = string.Concat(date, " | ", time);
        
        Border message = new Border()
        {
            Name = chatId,
            Margin = new Thickness(20, 15),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10),
            MinWidth = 50
        };

        Grid grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));

        Label senderSignature = new Label()
        {
            FontSize = 16,
            FontFamily = new FontFamily("Nunito"),
            Content =  senderAlias,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        TextBlock contentSignature = new TextBlock()
        {
            FontSize = 16,
            TextWrapping =  TextWrapping.Wrap,
            FontFamily = new FontFamily("Nunito"),
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            Text = content,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        TextBlock timeSignature = new TextBlock()
        {
            FontSize = 16,
            FontFamily = new FontFamily("Nunito"),
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Text = UnitedDate
        };
        Separator upperSeparator = new Separator()
        {
            Height = 2,
            Background = ColorPaletteNebula.GlacierMistColor,
        };
        
        Separator lowerSeparator = new Separator()
        {
            Height = 2,
            Background = ColorPaletteNebula.GlacierMistColor,
        };
        grid.Children.Add(lowerSeparator);
        grid.Children.Add(upperSeparator);
        grid.Children.Add(senderSignature);
        grid.Children.Add(contentSignature);
        grid.Children.Add(timeSignature);
        _mainWindowViewModel.SelectedChat.AddPage();
        
        Grid.SetRow(senderSignature, 0);
        Grid.SetRow(upperSeparator, 1);
        Grid.SetRow(contentSignature, 2);
        Grid.SetRow(lowerSeparator, 3);
        Grid.SetRow(timeSignature, 4);
        
        message.Child = grid;
        DockPanel.SetDock(message, Dock.Bottom);
        StackMessages.Children.Add(message);
        
        
        if (isOutgoing)
        {
            message.Background = ColorPaletteNebula.OnBgColor;
            message.HorizontalAlignment = HorizontalAlignment.Left;
        }
        else
        {
            message.Background = ColorPaletteNebula.ChatCloudColor;
            message.HorizontalAlignment = HorizontalAlignment.Right;
        }
    }
    
    // отслеживание пачек сообщений для загрузки
    private void ScrollViewer_LoadMore(object? sender, ScrollChangedEventArgs e)
    {
        if (MessageScroller.Offset.Y < 400)
        {
            double OldOffsetY = MessageScroller.Offset.Y;
            
            double OldExtHeight = MessageScroller.Extent.Height;
            
            //_template.LoadMessages(_mainWindowViewModel.SelectedChat.ChatId, _mainWindowViewModel.SelectedChat.GetPagesLoaded());
            
            double NewExtHeight = MessageScroller.Extent.Height;
            double delta = OldExtHeight - NewExtHeight;
            MessageScroller.Offset = new Vector(0,  OldOffsetY+ delta);
        }   
    }

    void SendMessage(string ChatId, string Content)
    {
        _template.SendMessage(ChatId, Content);
    }

    
    private void SendButton_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        SendMessage(_mainWindowViewModel.SelectedChat.ChatId, MessageInput.Text);
        MessageInput.Text = String.Empty;
    }
}
/*
 * 
*/