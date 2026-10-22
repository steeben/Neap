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

    /// <summary>A headset with a battery in its corner, and the headset struck through while it is disconnected.</summary>
    HeadsetBattery,

    /// <summary>A headset with the charge as a number in its corner, and the headset struck through while it is disconnected.</summary>
    HeadsetPercentage,
}

/// <summary>The battery as the notification-area icon draws it.</summary>
/// <param name="Style">How it is drawn.</param>
/// <param name="Percent">The charge, held between 0 and 100.</param>
/// <param name="Fill">How many of the battery's steps to fill.</param>
/// <param name="Low">Whether the charge is down to the person's warning level, and it is drawn in red.</param>
/// <param name="Charging">Whether the headset is on power.</param>
/// <param name="Disconnected">Whether the headset is away, and only a struck-through headset is drawn.</param>
public readonly record struct TrayBattery(TrayIconStyle Style, int Percent, int Fill, bool Low, bool Charging, bool Disconnected = false)
{
    /// <summary>The warning level a new install starts with.</summary>
    public const int DefaultWarnAt = 20;

    /// <summary>The lowest warning level that can be chosen.</summary>
    public const int MinWarnAt = 5;

    /// <summary>The highest warning level that can be chosen.</summary>
    public const int MaxWarnAt = 50;

    /// <summary>Whether a style draws a headset.</summary>
    public static bool ShowsHeadset(TrayIconStyle style) =>
        style is TrayIconStyle.HeadsetBattery or TrayIconStyle.HeadsetPercentage;

    /// <summary>Decides whether the icon shows the battery, and how.</summary>
    /// <param name="status">The headset's state.</param>
    /// <param name="reading">The battery reading worth showing, or null when there is none.</param>
    /// <param name="style">What the person has chosen the icon to show.</param>
    /// <param name="warnAt">The charge at and below which the battery is drawn in red.</param>
    /// <param name="steps">How many steps the style's battery has, one per pixel of its width.</param>
    /// <returns>The battery to draw, or null for Neap's own glyph.</returns>
    /// <remarks>
    /// <para>
    /// With no reading, or one still settling, the icon goes back to Neap's
    /// glyph rather than drawing a battery: an empty bar or a stale number
    /// would say something untrue. The headset styles say "disconnected"
    /// instead, but only when it is known: nothing plugged in, or a
    /// transmitter with the headset off or out of range. While Neap is still
    /// asking, or a transmitter answers without the settings, which leaves
    /// sound playing, the glyph stands in.
    /// </para>
    /// <para>
    /// Any charge at all fills at least one step, so a battery nearly gone
    /// still shows a red sliver rather than an empty outline. A battery on
    /// power is not low, as it is already being seen to.
    /// </para>
    /// </remarks>
    public static TrayBattery? Of(HeadsetStatus status, BatteryReading? reading, TrayIconStyle style, int warnAt, int steps)
    {
        if (style is not (TrayIconStyle.Battery or TrayIconStyle.Percentage) && !ShowsHeadset(style)) return null;
        if (reading is not { Settling: false } battery)
        {
            bool away = status.Link == Link.Absent || status is { NotConnected: true, SwitchedOff: false };
            return ShowsHeadset(style) && reading is null && away
                ? new TrayBattery(style, 0, 0, false, false, Disconnected: true)
                : null;
        }
        int percent = Math.Clamp(battery.Percent, 0, 100);
        int fill = (int)Math.Round(steps * percent / 100.0, MidpointRounding.AwayFromZero);
        if (percent > 0) fill = Math.Max(fill, 1);
        bool low = !battery.Charging && percent <= Math.Clamp(warnAt, MinWarnAt, MaxWarnAt);
        return new TrayBattery(style, percent, fill, low, battery.Charging);
    }
}
