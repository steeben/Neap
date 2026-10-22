using Neap.Core.Audio;

namespace Neap.Core.Connection;

/// <summary>What to do with the system's default devices after a change to the headset's connection.</summary>
public enum DeviceMove { Stay, ToHeadset, Back }

/// <summary>
/// When to make the headset the system's default output and microphone, and
/// what to put back when it goes.
/// </summary>
/// <remarks>
/// <para>
/// A transmitter keeps its devices while the headset is switched off, so the
/// system goes on sending sound to a transmitter nobody is listening to. The
/// headset answering or going quiet is what says the defaults should move,
/// not a device arriving or leaving.
/// </para>
/// <para>
/// States on the way between, connecting and settings out of reach, move
/// nothing: sound often still plays in the second, and either one is usually
/// over in a moment.
/// </para>
/// </remarks>
public static class DeviceSwitch
{
    /// <summary>The product whose devices should be the defaults, empty when the headset is away, or null when this state says neither.</summary>
    /// <param name="status">The headset's current connection.</param>
    /// <param name="cable">The headset's own product id when it is plugged in with its USB-C cable, or empty.</param>
    /// <remarks>
    /// With the cable in, the cable is the only right place for sound and the
    /// microphone, as <see cref="RoutingCheck"/> explains.
    /// </remarks>
    public static string? Target(HeadsetStatus status, string cable) => status.Link switch
    {
        Link.Connected when cable.Length > 0 => cable,
        Link.Connected when status.Product.Length > 0 => status.Product,
        Link.Quiet or Link.Absent => "",
        _ => null,
    };

    /// <param name="on">The product the defaults were last moved to, or empty when they have not been moved.</param>
    /// <param name="target">What <see cref="Target"/> says now.</param>
    /// <remarks>
    /// A new product while the headset stays connected, such as CrossPlay
    /// moving it to the other transmitter or its cable going in, moves the
    /// defaults again.
    /// </remarks>
    public static DeviceMove For(string on, string? target) =>
        target is null || target == on ? DeviceMove.Stay
        : target.Length > 0 ? DeviceMove.ToHeadset
        : DeviceMove.Back;

    /// <summary>The device to make the default for a product: the first of its own.</summary>
    /// <returns>Null when the product has no device of that kind.</returns>
    public static AudioDevice? Pick(IReadOnlyList<AudioDevice> devices, string product) =>
        devices.FirstOrDefault(d => string.Equals(d.Product, product, StringComparison.OrdinalIgnoreCase));

    /// <summary>Whether a default is worth putting back later: a device that is not one of the headset's.</summary>
    /// <remarks>
    /// Moving from one of the headset's devices to another is a move between
    /// transmitters. What came before them both is what goes back.
    /// </remarks>
    public static bool Remember(Routed? replaced) => replaced is { Product.Length: 0, Id.Length: > 0 };

    /// <summary>The device to put back when the headset goes.</summary>
    /// <param name="devices">The devices there are to choose from now.</param>
    /// <param name="chosen">The device chosen in settings, or empty to put back what was there before.</param>
    /// <param name="remembered">The default before the headset's, or empty when there was none to remember.</param>
    /// <returns>
    /// The chosen device, or what was there before when the chosen one is
    /// not plugged in. Null when neither is there: the system's own choice
    /// is left as it is.
    /// </returns>
    public static AudioDevice? Restore(IReadOnlyList<AudioDevice> devices, string chosen, string remembered)
    {
        AudioDevice? Find(string id) => id.Length == 0 ? null : devices.FirstOrDefault(d => d.Id == id);
        return Find(chosen) ?? Find(remembered);
    }
}
