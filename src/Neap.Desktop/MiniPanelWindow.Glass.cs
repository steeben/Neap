using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Neap.Core;
using SkiaSharp;

namespace Neap.Desktop;

/// <summary>The mini panel drawn as frosted glass.</summary>
/// <remarks>
/// <para>
/// The blur is drawn by the panel itself, from a picture of the screen
/// behind it taken as it opens, rather than asked of the system. The system's
/// blur fills the window and stays where it is, so the card could not fade or
/// rise over it; this one is the card's own ground and moves with it, and
/// needs no rounding of the window's corners, so it works on Windows 10 too.
/// What is behind stays as it was when the panel opened, which for a panel
/// open a few seconds is not seen.
/// </para>
/// <para>
/// Where the screen cannot be read, as on Linux, the glass is solid.
/// </para>
/// </remarks>
public partial class MiniPanelWindow
{
    /// <summary>How many times smaller the picture is blurred than it is shown; a blur this soft loses nothing to it.</summary>
    private const int Shrink = 4;

    /// <summary>How soft the blur is, in device-independent pixels.</summary>
    private const double Softness = 15;

    private bool _frosted;
    private Bitmap? _behind;
    private ImageBrush? _glass;
    private int _behindTop;

    /// <summary>Draws the panel as frosted glass: the look's colours, over what is behind it.</summary>
    /// <remarks>The card starts unseen, so that its first frame is not in the picture of what is behind it.</remarks>
    private void Frost()
    {
        _frosted = true;
        Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://Neap.Desktop/"))
        {
            Source = new Uri("avares://Neap.Desktop/Themes/Frosty.axaml"),
        });
        Classes.Add("frosty");
        Card.Opacity = 0;
    }

    /// <summary>Lays the blurred screen behind the card, taking it the first time, and lines it up with where the card now is.</summary>
    /// <param name="work">The work area of the screen the panel is on, in physical pixels.</param>
    /// <param name="box">Where the panel is, in physical pixels.</param>
    /// <remarks>The whole height of the work area is taken, so the panel can grow or shrink and still have glass behind it.</remarks>
    private void Ground(PixelRect work, Box box)
    {
        if (_behind is null)
        {
            _behind = Blurred(new PixelRect(box.X, work.Y, box.Width, work.Height));
            _behindTop = work.Y;
            Card.Opacity = 1;
            if (_behind is null) return;
            _glass = new ImageBrush(_behind) { Stretch = Stretch.Fill };
            Card.Background = _glass;
        }
        if (_glass is null) return;
        double scale = RenderScaling;
        _glass.DestinationRect = new RelativeRect(0, (_behindTop - box.Y) / scale,
            _behind.PixelSize.Width * Shrink / scale, _behind.PixelSize.Height * Shrink / scale, RelativeUnit.Absolute);
    }

    /// <summary>What is on the screen in an area, blurred and under the glass's tint, at a quarter of its size; null where the screen cannot be read.</summary>
    private Bitmap? Blurred(PixelRect area)
    {
        if (!OperatingSystem.IsWindows()) return null;
        if (WindowsScreen.Capture(area) is not { } pixels)
        {
            AppLog.Write("could not read the screen behind the mini panel, so its glass is solid");
            return null;
        }

        var tint = this.TryFindResource("NeapGlassBrush", ActualThemeVariant, out var found) && found is ISolidColorBrush brush
            ? brush.Color : Colors.Transparent;
        var small = new SKImageInfo(Math.Max(1, area.Width / Shrink), Math.Max(1, area.Height / Shrink), SKColorType.Bgra8888, SKAlphaType.Premul);
        float sigma = (float)(Softness * RenderScaling / Shrink);

        using var whole = new SKBitmap(new SKImageInfo(area.Width, area.Height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        System.Runtime.InteropServices.Marshal.Copy(pixels, 0, whole.GetPixels(), pixels.Length);
        using var shrunk = whole.Resize(small, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None));
        using var surface = SKSurface.Create(small);
        using (var blur = new SKPaint { ImageFilter = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp) })
            surface.Canvas.DrawBitmap(shrunk, 0, 0, blur);
        surface.Canvas.DrawColor(new SKColor(tint.R, tint.G, tint.B, tint.A), SKBlendMode.SrcOver);

        using var picture = surface.Snapshot();
        using var png = picture.Encode(SKEncodedImageFormat.Png, 100);
        return new Bitmap(png.AsStream());
    }
}
