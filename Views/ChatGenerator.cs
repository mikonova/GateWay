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
namespace GateWay.Views;

public partial class MainWindow
{
    private List<Border> _messageListLoaded = new List<Border>();
    private async void LoadMessage(string chatId, string senderAlias, string content, string timeStamp, bool isOutgoing)
    {
        if (_selectedChat == null)
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
            Margin = new Thickness(50, 0),
            CornerRadius = new CornerRadius(5),
            Padding = new Thickness(10)
        };

        Grid grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));
        grid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Auto));

        Label senderSignature = new Label()
        {
            FontSize = 20,
            FontFamily = new FontFamily("Nunito"),
            Content =  senderAlias,
            FontWeight = FontWeight.SemiBold,
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

        TextBlock contentSignature = new TextBlock()
        {
            FontSize = 20,
            TextWrapping =  TextWrapping.Wrap,
            FontFamily = new FontFamily("Nunito"),
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            Text = content,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };
        TextBlock timeSignature = new TextBlock()
        {
            FontSize = 20,
            FontFamily = new FontFamily("Nunito"),
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Text = UnitedDate
        };
        
        grid.Children.Add(senderSignature);
        grid.Children.Add(contentSignature);
        grid.Children.Add(timeSignature);
        
        Grid.SetRow(senderSignature, 0);
        Grid.SetRow(contentSignature, 1);
        Grid.SetRow(timeSignature, 2);
        
        message.Child = grid;
        StackMessages.Children.Add(message);
        
        if (isOutgoing)
        {
            message.Background = ColorPaletteNebula.OnBgColor;
        }
        else
        {
            message.Background = ColorPaletteNebula.ChatCloudColor;
        }
    }
}