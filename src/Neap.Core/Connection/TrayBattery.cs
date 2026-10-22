namespace Neap.Core.Connection;

/// <summary>What the notification-area icon shows.</summary>
public enum TrayIconStyle
{
    /// <summary>Neap's own glyph.</summary>
    Logo,

    /// <summary>A battery filled to the charge.</summary>
    Battery,

    /// <summary>The charge as a number.</summary>
    Percentage,
}

/// <summary>The battery as the notification-area icon draws it.</summary>
/// <param name="Style">Whether it is drawn as a battery or a number.</param>
/// <param name="Percent">The charge, held between 0 and 100.</param>
/// <param name="Fill">How many of the battery's steps to fill.</param>
/// <param name="Low">Whether the charge is down to the person's warning level, and it is drawn in red.</param>
/// <param name="Charging">Whether the headset is on power.</param>
public readonly record struct TrayBattery(TrayIconStyle Style, int Percent, int Fill, bool Low, bool Charging)
{
    /// <summary>The warning level a new install starts with.</summary>
    public const int DefaultWarnAt = 20;

    /// <summary>The lowest warning level that can be chosen.</summary>
    public const int MinWarnAt = 5;

    /// <summary>The highest warning level that can be chosen.</summary>
    public const int MaxWarnAt = 50;

    /// <summary>Decides whether the icon shows the battery, and how.</summary>
    /// <param name="reading">The battery reading worth showing, or null when there is none.</param>
    /// <param name="style">What the person has chosen the icon to show.</param>
    /// <param name="warnAt">The charge at and below which the battery is drawn in red.</param>
    /// <param name="steps">How many steps the battery's bar has, one per pixel of its width.</param>
    /// <returns>The battery to draw, or null for Neap's own glyph.</returns>
    /// <remarks>
    /// <para>
    /// With no reading, or one still settling, the icon goes back to Neap's
    /// glyph rather than drawing a battery: an empty bar or a stale number
    /// would say something untrue.
    /// </para>
    /// <para>
    /// Any charge at all fills at least one step, so a battery nearly gone
    /// still shows a red sliver rather than an empty outline. A battery on
    /// power is not low, as it is already being seen to.
    /// </para>
    /// </remarks>
    public static TrayBattery? Of(BatteryReading? reading, TrayIconStyle style, int warnAt, int steps)
    {
        if (style is not (TrayIconStyle.Battery or TrayIconStyle.Percentage)) return null;
        if (reading is not { Settling: false } battery) return null;
        int percent = Math.Clamp(battery.Percent, 0, 100);
        int fill = (int)Math.Round(steps * percent / 100.0, MidpointRounding.AwayFromZero);
        if (percent > 0) fill = Math.Max(fill, 1);
        bool low = !battery.Charging && percent <= Math.Clamp(warnAt, MinWarnAt, MaxWarnAt);
        return new TrayBattery(style, percent, fill, low, battery.Charging);
    }
}
