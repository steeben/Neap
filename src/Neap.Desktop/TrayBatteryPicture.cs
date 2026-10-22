using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Neap.Core.Connection;

namespace Neap.Desktop;

/// <summary>Draws the headset's battery as the notification-area icon, in the style the person chose.</summary>
/// <remarks>
/// <para>
/// Drawn at 32 pixels, the size a notification-area icon is shown at on a
/// screen at twice the usual scale, with the batteries' edges on even pixels
/// so they stay sharp when the system halves them. It is drawn in one
/// colour, the taskbar's shade as Neap's glyph is, and turns orange and then
/// red as the charge runs down; see <see cref="TrayTone"/>. The orange and
/// red are a shade for each taskbar, each above 3:1 against it.
/// </para>
/// <para>
/// In the headset styles the battery or number sits in the headset's
/// bottom-right corner, with a clear gap cut around it so the two read
/// apart. A disconnected headset is drawn faint and struck through; the
/// stroke is in full colour, so it does not rely on the faint headset to be
/// seen.
/// </para>
/// <para>
/// The picture is a glance, not the only way to read the battery: the
/// icon's tooltip says the state, the charge and whether it is charging in
/// words.
/// </para>
/// </remarks>
internal static class TrayBatteryPicture
{
    private const int Size = 32;
    private const double Outline = 2;

    /// <summary>The gap cut around the corner battery or number, and around the stroke through a disconnected headset.</summary>
    private const double Gap = 2;

    /// <summary>How faint a disconnected headset is drawn.</summary>
    private const double Faint = 0.45;

    /// <summary>The battery that fills the icon: its body, outside its outline, the terminal on its end, and where the charge is drawn.</summary>
    private static readonly (Rect Body, Rect Nub, Rect Track) Large = (new(0, 6, 26, 20), new(26, 12, 4, 8), new(4, 10, 18, 12));

    /// <summary>The battery in the headset's corner.</summary>
    private static readonly (Rect Body, Rect Nub, Rect Track) Corner = (new(10, 18, 20, 14), new(30, 22, 2, 6), new(13, 21, 14, 8));

    private static readonly Geometry Headband = Geometry.Parse("M5 19 V15 A11 11 0 0 1 27 15 V19");
    private static readonly RoundedRect LeftCup = new(new Rect(3, 17, 7, 11), 2.5);
    private static readonly RoundedRect RightCup = new(new Rect(22, 17, 7, 11), 2.5);
    private static readonly Geometry Strike = Geometry.Parse("M3 3 L29 29");

    private static readonly IBrush OrangeOnDark = new SolidColorBrush(Color.Parse("#FF9C33"));
    private static readonly IBrush OrangeOnLight = new SolidColorBrush(Color.Parse("#B35900"));
    private static readonly IBrush RedOnDark = new SolidColorBrush(Color.Parse("#FF4343"));
    private static readonly IBrush RedOnLight = new SolidColorBrush(Color.Parse("#C42B1C"));

    /// <summary>How many steps a style's battery has: one per pixel of its width.</summary>
    public static int Steps(TrayIconStyle style) => style == TrayIconStyle.HeadsetBattery
        ? (int)Corner.Track.Width
        : (int)Large.Track.Width;

    /// <param name="battery">What to draw.</param>
    /// <param name="shade">The taskbar's shade, as an app theme.</param>
    public static Bitmap Draw(TrayBattery battery, ThemeVariant shade)
    {
        var ink = Ink(shade);
        var colour = Colour(battery.Tone, shade, ink);
        var bitmap = new RenderTargetBitmap(new PixelSize(Size, Size));
        using (var context = bitmap.CreateDrawingContext())
        {
            if (battery.Disconnected)
            {
                DrawDisconnected(context, ink);
            }
            else if (battery.Style == TrayIconStyle.Percentage)
            {
                var number = Number(battery.Percent, 30, Size, corner: false);
                context.DrawGeometry(colour, null, number);
            }
            else if (battery.Style == TrayIconStyle.HeadsetPercentage)
            {
                var number = Number(battery.Percent, 17, 21, corner: true);
                DrawHeadset(context, ink, new RectangleGeometry(number.Bounds.Inflate(Gap)));
                context.DrawGeometry(colour, null, number);
            }
            else if (battery.Style == TrayIconStyle.HeadsetBattery)
            {
                DrawHeadset(context, ink, new RectangleGeometry(Corner.Body.Union(Corner.Nub).Inflate(Gap)));
                DrawBattery(context, Corner, battery, colour, ink, shade, radius: 2.5, boltScale: 0.5, halo: 2.2);
            }
            else
            {
                DrawBattery(context, Large, battery, colour, ink, shade, radius: 3, boltScale: 0.7, halo: 2 * Outline);
            }
        }
        return bitmap;
    }

    private static void DrawBattery(
        DrawingContext context, (Rect Body, Rect Nub, Rect Track) shape, TrayBattery battery, IBrush colour, IBrush ink,
        ThemeVariant shade, double radius, double boltScale, double halo)
    {
        context.DrawRectangle(null, new Pen(ink, Outline), shape.Body.Deflate(Outline / 2), radius, radius);
        context.DrawRectangle(ink, null, shape.Nub, 1, 1);
        if (battery.Fill > 0) context.DrawRectangle(colour, null, shape.Track.WithWidth(battery.Fill), 1, 1);
        if (battery.Charging) DrawBolt(context, shape.Body.Center, boltScale, halo, ink, shade);
    }

    /// <summary>Draws the charging bolt over the middle of a battery, edged <paramref name="halo"/> pixels wide in the opposite shade so it stands out from the fill.</summary>
    private static void DrawBolt(DrawingContext context, Point centre, double scale, double halo, IBrush ink, ThemeVariant shade)
    {
        var bolt = (Geometry)Application.Current!.FindResource("IconCharging")!;
        var edge = Ink(shade == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light);
        var place = Matrix.CreateTranslation(-12, -12) * Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(centre.X, centre.Y);
        using (context.PushTransform(place))
        {
            context.DrawGeometry(null, new Pen(edge, halo / scale, lineJoin: PenLineJoin.Round), bolt);
            context.DrawGeometry(ink, null, bolt);
        }
    }

    /// <summary>Draws the headset, less a cut-out where something else is drawn over it.</summary>
    private static void DrawHeadset(DrawingContext context, IBrush ink, Geometry cutOut)
    {
        var whole = new RectangleGeometry(new Rect(0, 0, Size, Size));
        using (context.PushGeometryClip(new CombinedGeometry(GeometryCombineMode.Exclude, whole, cutOut)))
        {
            context.DrawGeometry(null, new Pen(ink, 2.5, lineCap: PenLineCap.Round), Headband);
            context.DrawRectangle(ink, null, LeftCup);
            context.DrawRectangle(ink, null, RightCup);
        }
    }

    private static void DrawDisconnected(DrawingContext context, IBrush ink)
    {
        var stroke = new Pen(ink, 2.5, lineCap: PenLineCap.Round);
        var cutOut = Strike.GetWidenedGeometry(new Pen(ink, stroke.Thickness + 2 * Gap, lineCap: PenLineCap.Round));
        using (context.PushOpacity(Faint))
        {
            DrawHeadset(context, ink, cutOut);
        }
        context.DrawGeometry(null, stroke, Strike);
    }

    /// <summary>The charge as a number's outline, as large as fits, placed by its own outline rather than its line height.</summary>
    /// <param name="percent">The charge.</param>
    /// <param name="largest">The largest size to draw it at.</param>
    /// <param name="width">The widest it may be.</param>
    /// <param name="corner">Whether it sits in the bottom-right corner, rather than the middle.</param>
    private static Geometry Number(int percent, double largest, double width, bool corner)
    {
        var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
        string digits = percent.ToString(CultureInfo.CurrentCulture);
        var measured = new FormattedText(digits, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, largest, Brushes.Black);
        double size = Math.Min(largest, largest * width / measured.Width);
        var text = new FormattedText(digits, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, size, Brushes.Black);
        if (text.BuildGeometry(default) is not { } outline)
        {
            AppLog.Write("notification area: the font gave no outline for the battery's number");
            return new StreamGeometry();
        }
        var bounds = outline.Bounds;
        outline.Transform = corner
            ? new TranslateTransform(Size - bounds.Right, Size - bounds.Bottom)
            : new TranslateTransform((Size - bounds.Width) / 2 - bounds.X, (Size - bounds.Height) / 2 - bounds.Y);
        return outline;
    }

    private static IBrush Colour(TrayTone tone, ThemeVariant shade, IBrush ink) => tone switch
    {
        TrayTone.Low => shade == ThemeVariant.Light ? OrangeOnLight : OrangeOnDark,
        TrayTone.Critical => shade == ThemeVariant.Light ? RedOnLight : RedOnDark,
        _ => ink,
    };

    private static IBrush Ink(ThemeVariant shade) =>
        Application.Current!.TryGetResource("NeapTextBrush", shade, out object? value) && value is IBrush brush
            ? brush
            : throw new InvalidOperationException($"The theme has no NeapTextBrush for {shade}.");
}
