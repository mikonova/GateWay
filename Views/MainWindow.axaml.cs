#pragma warning disable 4014
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace GateWay.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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