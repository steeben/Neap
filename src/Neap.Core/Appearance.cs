namespace Neap.Core;

/// <summary>Whether the app is drawn light or dark.</summary>
public enum ThemeChoice
{
    /// <summary>As the system is set, and changing when it does.</summary>
    System,

    /// <summary>Light, whatever the system is set to.</summary>
    Light,

    /// <summary>Dark, whatever the system is set to.</summary>
    Dark,
}

/// <summary>How the mini panel is drawn.</summary>
public enum PanelLook
{
    /// <summary>A card like Home's, on the app's own panel colour.</summary>
    Standard,

    /// <summary>Frosted glass: what is behind the panel shows through it, blurred, under a tint and a soft edge.</summary>
    FrostyGlass,
}
