using Neap.Core.Audio;
using Neap.Core.Connection;

namespace Neap.Services;

/// <summary>
/// Makes the headset the system's default output and microphone when it
/// connects, and puts back the devices before it, or the ones chosen in
/// settings, when it goes.
/// </summary>
/// <remarks>
/// <para>
/// When to move is decided by <see cref="DeviceSwitch"/>. This watches the
/// headset and the cable and carries it out, whichever screen is showing or
/// none, and only while <see cref="AppSettings.SoundFollowsHeadset"/> is on.
/// </para>
/// <para>
/// Calls move with the rest: a chat app left on the transmitter after the
/// headset goes is as silent as a game.
/// </para>
/// </remarks>
public sealed class SoundFollowsHeadset : IDisposable
{
    private readonly HeadsetService _headset;
    private readonly AudioRoute _route;

    /// <summary>The product the defaults were moved to, or empty while they are not the headset's.</summary>
    private string _on = "";

    public SoundFollowsHeadset(HeadsetService headset, AudioRoute route)
    {
        _headset = headset;
        _route = route;
        _headset.StatusChanged += OnStatus;
        _route.Changed += Look;
        Look();
    }

    /// <summary>Whether the system lets the app choose its defaults.</summary>
    public static bool Supported => AudioRoute.CanSwitch;

    /// <summary>Gets or sets whether the defaults follow the headset.</summary>
    /// <remarks>Turned on with the headset already connected, the defaults move at once.</remarks>
    public bool Enabled
    {
        get => AppSettings.Current.SoundFollowsHeadset;
        set
        {
            AppSettings.Update(settings => settings.SoundFollowsHeadset = value);
            AppLog.Write($"sound devices: {(value ? "follow the headset" : "left where they are")}");
            _on = "";
            Look();
        }
    }

    private void OnStatus(HeadsetStatus status) => Look();

    private void Look()
    {
        if (!Supported || !Enabled) return;
        string? target = DeviceSwitch.Target(_headset.Status, _route.Cable);
        switch (DeviceSwitch.For(_on, target))
        {
            case DeviceMove.ToHeadset:
                _on = target!;
                ToHeadset(_on);
                break;
            case DeviceMove.Back:
                _on = "";
                Back();
                break;
        }
    }

    private void ToHeadset(string product)
    {
        var output = _route.Output;
        var input = _route.Input;
        if (DeviceSwitch.Remember(output) || DeviceSwitch.Remember(input))
        {
            AppSettings.Update(settings =>
            {
                if (DeviceSwitch.Remember(output)) settings.SoundBeforeHeadset = output!.Id;
                if (DeviceSwitch.Remember(input)) settings.MicrophoneBeforeHeadset = input!.Id;
            });
        }
        Set(DeviceSwitch.Pick(AudioRoute.Devices(output: true), product), "output", $"the headset has no output on {product}");
        Set(DeviceSwitch.Pick(AudioRoute.Devices(output: false), product), "microphone", $"the headset has no microphone on {product}");
    }

    private static void Back()
    {
        var settings = AppSettings.Current;
        Set(DeviceSwitch.Restore(AudioRoute.Devices(output: true), settings.SoundBackTo, settings.SoundBeforeHeadset),
            "output", "there is no output to go back to, so the system's choice is left");
        Set(DeviceSwitch.Restore(AudioRoute.Devices(output: false), settings.MicrophoneBackTo, settings.MicrophoneBeforeHeadset),
            "microphone", "there is no microphone to go back to, so the system's choice is left");
    }

    /// <param name="device">The device to make the default, or null when there is none.</param>
    /// <param name="what">Which default, for the log.</param>
    /// <param name="none">What the log says when there is no device.</param>
    private static void Set(AudioDevice? device, string what, string none)
    {
        if (device is null)
        {
            AppLog.Write($"sound devices: {none}");
            return;
        }
        AppLog.Write(AudioRoute.SetDefault(device.Id)
            ? $"sound devices: the default {what} is now {device.Name}"
            : $"sound devices: the system would not make {device.Name} the default {what}");
    }

    public void Dispose()
    {
        _headset.StatusChanged -= OnStatus;
        _route.Changed -= Look;
    }
}
