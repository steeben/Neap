using Neap.Core.Audio;
using Neap.Core.Connection;

namespace Neap.Core.Tests.Connection;

public class DeviceSwitchTests
{
    private const string Dock = "229B";
    private const string Transmitter = "229D";
    private const string Headset = "229E";

    private static readonly AudioDevice Speakers = new("{speakers}", "Speakers", "");
    private static readonly AudioDevice Monitor = new("{monitor}", "Monitor", "");
    private static readonly AudioDevice OnDock = new("{dock}", "Stealth Pro II", Dock);
    private static readonly AudioDevice OnTransmitter = new("{transmitter}", "Stealth Pro II", Transmitter);

    private static HeadsetStatus On(Link link, string product = Dock) =>
        new(link, Route.ChargingDock, "Charging Dock", "", product);

    [Fact]
    public void ConnectedTargetsTheTransmitterItIsOn()
    {
        Assert.Equal(Dock, DeviceSwitch.Target(On(Link.Connected), cable: ""));
    }

    [Fact]
    public void TheCableWinsWhileItIsIn()
    {
        Assert.Equal(Headset, DeviceSwitch.Target(On(Link.Connected), cable: Headset));
    }

    [Fact]
    public void SwitchedOffOrUnpluggedTargetsNothing()
    {
        Assert.Equal("", DeviceSwitch.Target(On(Link.Quiet), cable: ""));
        Assert.Equal("", DeviceSwitch.Target(On(Link.Absent, product: ""), cable: ""));
    }

    [Fact]
    public void StatesOnTheWaySayNeither()
    {
        Assert.Null(DeviceSwitch.Target(On(Link.Connecting), cable: ""));
        Assert.Null(DeviceSwitch.Target(On(Link.Silent), cable: ""));
        Assert.Null(DeviceSwitch.Target(On(Link.Connected, product: ""), cable: ""));
    }

    [Fact]
    public void ConnectingMovesToTheHeadsetAndGoingMovesBack()
    {
        Assert.Equal(DeviceMove.ToHeadset, DeviceSwitch.For("", Dock));
        Assert.Equal(DeviceMove.Back, DeviceSwitch.For(Dock, ""));
    }

    [Fact]
    public void NothingNewMovesNothing()
    {
        Assert.Equal(DeviceMove.Stay, DeviceSwitch.For(Dock, Dock));
        Assert.Equal(DeviceMove.Stay, DeviceSwitch.For("", ""));
        Assert.Equal(DeviceMove.Stay, DeviceSwitch.For(Dock, null));
        Assert.Equal(DeviceMove.Stay, DeviceSwitch.For("", null));
    }

    [Fact]
    public void AnotherTransmitterWhileConnectedMovesAgain()
    {
        Assert.Equal(DeviceMove.ToHeadset, DeviceSwitch.For(Dock, Transmitter));
        Assert.Equal(DeviceMove.ToHeadset, DeviceSwitch.For(Dock, Headset));
    }

    [Fact]
    public void PicksTheProductsOwnDevice()
    {
        Assert.Equal(OnTransmitter, DeviceSwitch.Pick([Speakers, OnDock, OnTransmitter], Transmitter));
        Assert.Equal(OnDock, DeviceSwitch.Pick([Speakers, OnDock], "229b"));
        Assert.Null(DeviceSwitch.Pick([Speakers, OnDock], Transmitter));
    }

    [Fact]
    public void OnlyADeviceThatIsNotTheHeadsetsIsRemembered()
    {
        Assert.True(DeviceSwitch.Remember(new Routed("Speakers", "", "{speakers}")));
        Assert.False(DeviceSwitch.Remember(new Routed("Stealth Pro II", Transmitter, "{transmitter}")));
        Assert.False(DeviceSwitch.Remember(new Routed("Speakers", "")));
        Assert.False(DeviceSwitch.Remember(null));
    }

    [Fact]
    public void TheChosenDeviceGoesBackFirst()
    {
        Assert.Equal(Monitor, DeviceSwitch.Restore([Speakers, Monitor], chosen: Monitor.Id, remembered: Speakers.Id));
    }

    [Fact]
    public void WithNothingChosenTheOneBeforeGoesBack()
    {
        Assert.Equal(Speakers, DeviceSwitch.Restore([Speakers, Monitor], chosen: "", remembered: Speakers.Id));
    }

    [Fact]
    public void AChosenDeviceNotPluggedInFallsBackToTheOneBefore()
    {
        Assert.Equal(Speakers, DeviceSwitch.Restore([Speakers], chosen: Monitor.Id, remembered: Speakers.Id));
    }

    [Fact]
    public void NothingToGoBackToLeavesTheSystemsChoice()
    {
        Assert.Null(DeviceSwitch.Restore([Speakers], chosen: "", remembered: ""));
        Assert.Null(DeviceSwitch.Restore([Speakers], chosen: Monitor.Id, remembered: "{gone}"));
    }
}
