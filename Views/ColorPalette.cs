using Avalonia;
using Avalonia.Media;
using Avalonia.Themes.Fluent;

namespace ColorPalette;
// aqua nebula palette
public static class ColorPaletteNebula
{
    // midnight abyss
    public static readonly SolidColorBrush BackgroundColor = new SolidColorBrush(Color.Parse("#061826"));
    
    // atlantic steel
    public static readonly SolidColorBrush OnBgColor = new SolidColorBrush(Color.Parse("#901C4E75"));
    
    // smoky white
    public static readonly SolidColorBrush HighlightColor = new SolidColorBrush(Color.Parse("#40f5f5f5"));
    
    // less transparent smoky white
    public static readonly SolidColorBrush PressColor = new SolidColorBrush(Color.Parse("#80f5f5f5"));
    
    //tidepool teal
    public static readonly SolidColorBrush ChatCloudColor = new SolidColorBrush(Color.Parse("#2FA0C6"));
    
    // summer surf
    public static readonly SolidColorBrush SummerSurfColor = new SolidColorBrush(Color.Parse("#58C9F3"));
    // glacier mist
    public static readonly SolidColorBrush GlacierMistColor = new SolidColorBrush(Color.Parse("#BDE5FF"));
    
    // blended brushes
    public static readonly SolidColorBrush ChatHover = BrushManipulator.BlendBrushes(BackgroundColor, HighlightColor, 0.3);
    public static readonly SolidColorBrush ChatPress = BrushManipulator.BlendBrushes(BackgroundColor, PressColor, 0.3);
    
    // linear gradient brush atlantic steel to midnight abyss
    public static readonly LinearGradientBrush HorBarGradient = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
        GradientStops = new GradientStops
        {
            new GradientStop(Color.Parse("#061826"),0),
            new GradientStop(Color.Parse("#1C4E75"), 1)
        }
    };
    
    public static readonly LinearGradientBrush VertBarGradient = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
        GradientStops = new GradientStops
        {
            new GradientStop(Color.Parse("#061826"),0),
            new GradientStop(Color.Parse("#1C4E75"), 0.85),
            new GradientStop(Color.Parse("#061826"), 1)
        }
    };
}

public static class BrushManipulator
{
    public static SolidColorBrush BlendBrushes(SolidColorBrush brush1, SolidColorBrush brush2, double ratio = 0.5)
    {
        Color c1 = brush1.Color;
        Color c2 = brush2.Color;

        byte a = (byte)(c1.A * (1 - ratio) + c2.A * ratio);
        byte r = (byte)(c1.R * (1 - ratio) + c2.R * ratio);
        byte g = (byte)(c1.G * (1 - ratio) + c2.G * ratio);
        byte b = (byte)(c1.B * (1 - ratio) + c2.B * ratio);

        return new SolidColorBrush(Color.FromArgb(a, r, g, b));
    }
}