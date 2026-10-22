using Neap.Core.Connection;

namespace Neap.Core.Tests.Connection;

public class TrayBatteryTests
{
    private static readonly HeadsetStatus Connected = new(Link.Connected, Route.ChargingDock, "", "");

    private static TrayBattery Drawn(int percent, bool charging = false) =>
        TrayBattery.Of(Connected, new BatteryReading(percent, charging), TrayIconStyle.Battery, 20)!.Value;

    [Theory]
    [InlineData(TrayIconStyle.Battery)]
    [InlineData(TrayIconStyle.Percentage)]
    [InlineData(TrayIconStyle.HeadsetBattery)]
    [InlineData(TrayIconStyle.HeadsetPercentage)]
    public void NoReadingWhileConnectedShowsTheGlyph(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(Connected, null, style, 20));

    [Theory]
    [InlineData(TrayIconStyle.Battery)]
    [InlineData(TrayIconStyle.HeadsetBattery)]
    public void ASettlingReadingShowsTheGlyph(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(Connected, new BatteryReading(40, false, Settling: true), style, 20));

    [Theory]
    [InlineData(TrayIconStyle.Logo)]
    [InlineData((TrayIconStyle)7)]
    public void TheGlyphChosenOrAStyleNotKnownShowsTheGlyph(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(Connected, new BatteryReading(80, false), style, 20));

    [Theory]
    [InlineData(TrayIconStyle.Percentage)]
    [InlineData(TrayIconStyle.HeadsetBattery)]
    [InlineData(TrayIconStyle.HeadsetPercentage)]
    public void KeepsTheChosenStyle(TrayIconStyle style) =>
        Assert.Equal(style, TrayBattery.Of(Connected, new BatteryReading(80, false), style, 20)!.Value.Style);

    [Theory]
    [InlineData(Link.Absent, Route.Unknown, true)]
    [InlineData(Link.Quiet, Route.ChargingDock, true)]
    [InlineData(Link.Quiet, Route.UsbTransmitter, true)]
    [InlineData(Link.Connecting, Route.Unknown, false)]
    [InlineData(Link.Silent, Route.UsbTransmitter, false)]
    public void AHeadsetStyleSaysDisconnectedOnlyWhenItIsKnown(Link link, Route route, bool disconnected)
    {
        var drawn = TrayBattery.Of(new HeadsetStatus(link, route, "", ""), null, TrayIconStyle.HeadsetBattery, 20);
        Assert.Equal(disconnected, drawn is { Disconnected: true });
        if (!disconnected) Assert.Null(drawn);
    }

    [Theory]
    [InlineData(TrayIconStyle.Battery)]
    [InlineData(TrayIconStyle.Percentage)]
    public void TheOtherStylesShowTheGlyphWhenDisconnected(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(new HeadsetStatus(Link.Absent, Route.Unknown, "", ""), null, style, 20));

    [Fact]
    public void SwitchedOffOnItsCableTheHeadsetStyleShowsTheCharge()
    {
        var cable = new HeadsetStatus(Link.Quiet, Route.DirectUsb, "", "");
        var drawn = TrayBattery.Of(cable, new BatteryReading(60, true), TrayIconStyle.HeadsetPercentage, 20)!.Value;
        Assert.False(drawn.Disconnected);
        Assert.Equal(60, drawn.Percent);
    }

    [Theory]
    [InlineData(100, 20)]
    [InlineData(50, 10)]
    [InlineData(76, 15)]
    [InlineData(78, 16)]
    [InlineData(1, 1)]
    [InlineData(0, 0)]
    public void FillsTheBarToTheCharge(int percent, int fill) => Assert.Equal(fill, Drawn(percent).Fill);

    [Theory]
    [InlineData(-5, 0)]
    [InlineData(120, 100)]
    public void AReadingOutOfRangeIsHeldToTheScale(int percent, int held) => Assert.Equal(held, Drawn(percent).Percent);

    [Theory]
    [InlineData(100, TrayTone.Normal)]
    [InlineData(26, TrayTone.Normal)]
    [InlineData(25, TrayTone.Low)]
    [InlineData(16, TrayTone.Low)]
    [InlineData(15, TrayTone.Critical)]
    [InlineData(0, TrayTone.Critical)]
    public void TurnsOrangeAtAQuarterAndRedAtFifteen(int percent, TrayTone tone) =>
        Assert.Equal(tone, Drawn(percent).Tone);

    [Fact]
    public void OnPowerItIsTheUsualShade()
    {
        var battery = Drawn(5, charging: true);
        Assert.Equal(TrayTone.Normal, battery.Tone);
        Assert.True(battery.Charging);
    }
}
