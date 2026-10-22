using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Neap.Core.Connection;

namespace Neap.Desktop;

/// <summary>Draws the headset's battery as the notification-area icon: a battery filled to the charge, or the charge as a number.</summary>
/// <remarks>
/// <para>
/// Drawn at 32 pixels, the size a notification-area icon is shown at on a
/// screen at twice the usual scale, with the battery's edges on even pixels
/// so it stays sharp when the system halves it. It is drawn in one colour,
/// the taskbar's shade as Neap's glyph is, and turns red at the person's
/// warning level. The red is a shade for each taskbar, each above 3:1
/// against it.
/// </para>
/// <para>
/// The picture is a glance, not the only way to read the battery: the
/// icon's tooltip says the charge and whether it is charging in words.
/// </para>
/// </remarks>
internal static class TrayBatteryPicture
{
    private const int Size = 32;
    private const double Outline = 2;

    /// <summary>The battery's body, outside its outline.</summary>
    private static readonly Rect Body = new(0, 6, 26, 20);

    /// <summary>The terminal on the end that makes it read as a battery.</summary>
    private static readonly Rect Nub = new(26, 12, 4, 8);

    /// <summary>Where the charge is drawn: inside the outline, with a gap so the two do not run together.</summary>
    private static readonly Rect Track = new(4, 10, 18, 12);

    private static readonly IBrush RedOnDark = new SolidColorBrush(Color.Parse("#FF4343"));
    private static readonly IBrush RedOnLight = new SolidColorBrush(Color.Parse("#C42B1C"));

    /// <summary>How many steps the battery's bar has: one per pixel of its width.</summary>
    public const int Steps = 18;

    /// <param name="battery">What to draw.</param>
    /// <param name="shade">The taskbar's shade, as an app theme.</param>
    public static Bitmap Draw(TrayBattery battery, ThemeVariant shade)
    {
        var ink = Ink(shade);
        var bitmap = new RenderTargetBitmap(new PixelSize(Size, Size));
        using (var context = bitmap.CreateDrawingContext())
        {
            if (battery.Style == TrayIconStyle.Percentage) DrawNumber(context, battery, battery.Low ? Red(shade) : ink);
            else DrawBattery(context, battery, ink, shade);
        }
        return bitmap;
    }

    private static void DrawBattery(DrawingContext context, TrayBattery battery, IBrush ink, ThemeVariant shade)
    {
        context.DrawRectangle(null, new Pen(ink, Outline), Body.Deflate(Outline / 2), 3, 3);
        context.DrawRectangle(ink, null, Nub, 1, 1);
        if (battery.Fill > 0) context.DrawRectangle(battery.Low ? Red(shade) : ink, null, Track.WithWidth(battery.Fill), 1, 1);
        if (battery.Charging) DrawBolt(context, ink, shade);
    }

    /// <summary>Draws the charging bolt over the middle of the battery, edged in the opposite shade so it stands out from the fill.</summary>
    private static void DrawBolt(DrawingContext context, IBrush ink, ThemeVariant shade)
    {
        var bolt = (Geometry)Application.Current!.FindResource("IconCharging")!;
        var edge = Ink(shade == ThemeVariant.Light ? ThemeVariant.Dark : ThemeVariant.Light);
        const double scale = 0.7;
        var centre = Body.Center;
        var place = Matrix.CreateTranslation(-12, -12) * Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(centre.X, centre.Y);
        using (context.PushTransform(place))
        {
            context.DrawGeometry(null, new Pen(edge, 2 * Outline / scale, lineJoin: PenLineJoin.Round), bolt);
            context.DrawGeometry(ink, null, bolt);
        }
    }

    /// <summary>Draws the charge as a number, as large as fits, centred on its own outline rather than its line height.</summary>
    private static void DrawNumber(DrawingContext context, TrayBattery battery, IBrush brush)
    {
        const double largest = 30;
        var typeface = new Typeface(FontFamily.Default, FontStyle.Normal, FontWeight.Bold);
        string digits = battery.Percent.ToString(CultureInfo.CurrentCulture);
        var measured = new FormattedText(digits, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, largest, brush);
        double size = Math.Min(largest, largest * Size / measured.Width);
        var text = new FormattedText(digits, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, size, brush);
        if (text.BuildGeometry(default) is not { } outline) return;
        var bounds = outline.Bounds;
        using (context.PushTransform(Matrix.CreateTranslation((Size - bounds.Width) / 2 - bounds.X, (Size - bounds.Height) / 2 - bounds.Y)))
        {
            context.DrawGeometry(brush, null, outline);
        }
    }

    private static IBrush Red(ThemeVariant shade) => shade == ThemeVariant.Light ? RedOnLight : RedOnDark;

    private static IBrush Ink(ThemeVariant shade) =>
        Application.Current!.TryGetResource("NeapTextBrush", shade, out object? value) && value is IBrush brush
            ? brush
            : throw new InvalidOperationException($"The theme has no NeapTextBrush for {shade}.");
}
