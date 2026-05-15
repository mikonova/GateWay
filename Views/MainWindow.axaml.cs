#pragma warning disable 4014
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace GateWay.Views;

public partial class MainWindow : Window
{
    async void AddChatToList(string senderAlias, string lastSentence, bool isSelf )
    {
        
        Border border = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#3C096C")),
            Name = $"{senderAlias}",
            Width = 250,
            Height = 70,
        };
        Grid grid = new Grid() {RowSpacing = 10, VerticalAlignment =  VerticalAlignment.Top};
        grid.RowDefinitions.Add(new RowDefinition{Height =  new GridLength(20)});
        grid.RowDefinitions.Add(new RowDefinition{Height = new GridLength(1,GridUnitType.Star)});
        TextBlock userName = new TextBlock
        {
            FontSize = 25,
            TextAlignment = TextAlignment.Center, 
            FontWeight = FontWeight.Bold,
            Text = $"{senderAlias}"
        };
        TextBlock sender = new TextBlock
        {
            FontSize = 20, 
            TextAlignment = TextAlignment.Center, 
            FontWeight = FontWeight.Bold, 
            Opacity =  0.5, 
            Margin =  new Thickness(0,0,5,0)
        };
        TextBlock lastMsg = new TextBlock
        {
            FontSize = 20,
            TextAlignment = TextAlignment.Center,
            Opacity =  0.5,
            Text = lastSentence,
        };
        StackPanel messageInfo = new StackPanel(){HorizontalAlignment = HorizontalAlignment.Left};
        
        if(isSelf)
        {
            sender.Text = "You:";
        }
        else
        {
            sender.Text = $"{senderAlias}";
        }
        
        ChatList.Children.Add(border);
        border.Child = grid;
        grid.Children.Add(userName);
        grid.Children.Add(messageInfo);
        Grid.SetRow(userName, 0);
        Grid.SetRow(messageInfo, 1);
        messageInfo.Children.Add(sender);
        messageInfo.Children.Add(lastMsg);
        
        
      
    }
    public MainWindow()
    {
        InitializeComponent();
        AddChatToList("miko", "ты лох", false);
    }

    private void PaneExpand_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SidePane.IsPaneOpen)
        {
            SidePane.IsPaneOpen = false;
            
            //PaneBorder.Width = SidePane.CompactPaneLength;
        }
        else
        {
            SidePane.IsPaneOpen = true;
            
            //PaneBorder.Width = SidePane.OpenPaneLength;
        }
    }

    private void BtnClose_OnClick(object? sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void BtnFullscreen_OnClick(object? sender, RoutedEventArgs e)
    {
        if(this.WindowState == WindowState.FullScreen)
        {
            this.WindowState = WindowState.Normal;
            BtnFullscreen.Text = "□";
        }
        else
        {
            this.WindowState = WindowState.FullScreen;
            BtnFullscreen.Text = "■";
        }
    }

    private void BtnToTray_OnClick(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }
}