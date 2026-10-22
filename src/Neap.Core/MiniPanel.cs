namespace Neap.Core;

/// <summary>What a click on the tray icon does.</summary>
public enum TrayClick
{
    /// <summary>Brings the window forward.</summary>
    OpenWindow,

    /// <summary>Opens the mini panel, or closes it when it is open.</summary>
    ToggleMiniPanel,
}

/// <summary>Decides where the mini panel opens, what a click on the tray icon does, and whether it opens the panel.</summary>
/// <remarks>
/// <para>
/// The panel opens by the icon that was clicked. A desktop does not say where
/// its icons are, but a click on one lands on a taskbar, outside the work
/// area, which is the screen less its bars. So a click on a bar puts the panel
/// against that bar, centred on the click as far as the work area allows,
/// which keeps it by the icon on whichever screen the notification area is.
/// </para>
/// <para>
/// Without such a click, as from the icon's menu or on a desktop that does not
/// say where the pointer is, the panel goes in a corner, as the system's own
/// volume flyout does. The side a bar takes shows as a gap between the screen
/// and its work area, and the panel goes in the corner next to that bar, on
/// the side the icons are on: the right, except beside a bar on the left edge.
/// With no bar to find, as with one that hides itself or a desktop that
/// reports none, it goes bottom right.
/// </para>
/// <para>
/// Clicking the icon while the panel is open first takes focus from the
/// panel, which closes it, and then reports the click. Opening again on that
/// click would make the icon unable to close the panel, so a click just after
/// a close is that close's own.
/// </para>
/// </remarks>
public static class MiniPanel
{
    /// <summary>How soon after the panel closed for losing focus a click on the icon is taken as the one that did it.</summary>
    public static readonly TimeSpan ClickSettle = TimeSpan.FromMilliseconds(400);

    /// <param name="screen">The whole screen the notification area is on.</param>
    /// <param name="work">The screen less its taskbars.</param>
    /// <param name="width">The panel's width, in pixels.</param>
    /// <param name="height">The panel's height, in pixels.</param>
    /// <param name="margin">The gap kept to the work area's edges, in pixels.</param>
    /// <param name="click">Where the pointer was when the icon was clicked, or null when that is not known.</param>
    public static Box Place(Box screen, Box work, int width, int height, int margin, (int X, int Y)? click)
    {
        int w = Math.Min(width, work.Width - 2 * margin), h = Math.Min(height, work.Height - 2 * margin);
        int right = work.X + work.Width - w - margin, bottom = work.Y + work.Height - h - margin;
        if (click is { } c)
        {
            if (c.Y < work.Y || c.Y >= work.Y + work.Height)
                return new(Math.Clamp(c.X - w / 2, work.X + margin, right), c.Y < work.Y ? work.Y + margin : bottom, w, h);
            if (c.X < work.X || c.X >= work.X + work.Width)
                return new(c.X < work.X ? work.X + margin : right, Math.Clamp(c.Y - h / 2, work.Y + margin, bottom), w, h);
        }

        bool top = work.Y > screen.Y && work.Y + work.Height == screen.Y + screen.Height;
        bool left = work.X > screen.X && work.X + work.Width == screen.X + screen.Width;
        int x = left ? work.X + margin : right;
        int y = top ? work.Y + margin : bottom;
        return new(x, y, w, h);
    }

    /// <summary>What a click on the icon does.</summary>
    /// <param name="panelOn">Whether the person has the mini panel switched on.</param>
    public static TrayClick Click(bool panelOn) => panelOn ? TrayClick.ToggleMiniPanel : TrayClick.OpenWindow;

    /// <summary>Whether a click on the icon opens the panel, rather than closing it or finishing the close that began it.</summary>
    /// <param name="isOpen">Whether the panel is open now.</param>
    /// <param name="sinceClosed">How long ago it last closed.</param>
    public static bool Opens(bool isOpen, TimeSpan sinceClosed) => !isOpen && sinceClosed >= ClickSettle;

    /// <summary>The look the panel is drawn in.</summary>
    /// <param name="chosen">The look the person chose.</param>
    /// <param name="highContrast">Whether the person has asked their system for high contrast.</param>
    /// <remarks>High contrast draws every window in the system's own solid colours, so frosted glass gives way to it.</remarks>
    public static PanelLook Look(PanelLook chosen, bool highContrast) => highContrast ? PanelLook.Standard : chosen;
}
