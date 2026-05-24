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
    
    private async void LoadMessage(string chatId, string senderAlias, string content, string timeStamp, bool isOutgoing)
    {
        if (_mainWindowViewModel.SelectedChat == null)
        {
            throw new Exception("No selected chat!");
        }
        DateTime dateTime = DateTime.Parse(timeStamp);
        string date = dateTime.ToString("dd.MM.yyyy");
        string time =  dateTime.ToString("HH:mm");
        string UnitedDate = string.Concat(date, " | ", time);
        
        Border message = new Border()
        {
            Name = chatId,
            Margin = new Thickness(50, 30),
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
        
        Grid.SetRow(senderSignature, 0);
        Grid.SetRow(upperSeparator, 1);
        Grid.SetRow(contentSignature, 2);
        Grid.SetRow(lowerSeparator, 3);
        Grid.SetRow(timeSignature, 4);
        
        message.Child = grid;
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

    void CreateMessage(string senderAlias, string content, string timeStamp)
    {
        
    }
}
/*
 * что тыкать куда:
 * 
*/