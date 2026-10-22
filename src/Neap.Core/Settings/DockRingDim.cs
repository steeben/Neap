namespace Neap.Core.Settings;

/// <summary>
/// Decides when to turn the Charging Dock's status ring down to nothing as
/// the headset goes, and what to bring it back to when the headset returns.
/// </summary>
/// <remarks>
/// <para>
/// With the headset gone the dock blinks its status ring white for about
/// five minutes, and nothing changes what the lights do; only brightness is
/// reachable. At 0 the ring stays dark through the blinking (confirmed on the
/// owner's headset), so the ring is turned down as the headset goes and put
/// back when it returns.
/// </para>
/// <para>
/// The brightness it had is kept before anything is written, and kept until
/// the headset is back on the dock, so a crash or a quit meanwhile still
/// brings it back. A value already kept is never replaced, or a second
/// disconnect before the first was undone would keep the 0.
/// </para>
/// </remarks>
public static class DockRingDim
{
    /// <summary>The status ring's brightness: light 1 of the transmitter in use.</summary>
    public const int Key = 0x401;

    /// <summary>The brightness the ring is turned down to.</summary>
    public const int Dark = 0;

    /// <summary>The brightness to keep and then turn down from, or null to leave the ring alone.</summary>
    /// <param name="enabled">Whether the person has turned this on.</param>
    /// <param name="onDock">Whether the headset was going from the Charging Dock it was answering through.</param>
    /// <param name="brightness">The status ring's brightness as last read, or null if unread.</param>
    /// <param name="kept">The brightness already kept from an earlier disconnect, or null.</param>
    public static int? ToKeep(bool enabled, bool onDock, int? brightness, int? kept) =>
        enabled && onDock && kept is null && brightness is > Dark ? brightness : null;

    /// <summary>The brightness to put back once the headset is answering on the dock again, or null to leave it.</summary>
    /// <param name="kept">The brightness kept as the headset went, or null.</param>
    /// <param name="brightness">The status ring's brightness as read on return.</param>
    /// <remarks>
    /// Only a ring still at <see cref="Dark"/> is put back. Any other reading
    /// means the dimming never landed or the brightness was set since, and
    /// either way it is the person's.
    /// </remarks>
    public static int? ToRestore(int? kept, int brightness) =>
        kept is int value && brightness == Dark ? value : null;
}
