using Neap.Core.Connection;

namespace Neap.Core.Tests.Connection;

public class TrayBatteryTests
{
    private static readonly HeadsetStatus Connected = new(Link.Connected, Route.ChargingDock, "", "");

    private static TrayBattery Drawn(int percent, bool charging = false, int warnAt = 20) =>
        TrayBattery.Of(Connected, new BatteryReading(percent, charging), TrayIconStyle.Battery, warnAt, 20)!.Value;

    [Theory]
    [InlineData(TrayIconStyle.Battery)]
    [InlineData(TrayIconStyle.Percentage)]
    [InlineData(TrayIconStyle.HeadsetBattery)]
    [InlineData(TrayIconStyle.HeadsetPercentage)]
    public void NoReadingWhileConnectedShowsTheGlyph(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(Connected, null, style, 20, 20));

    [Theory]
    [InlineData(TrayIconStyle.Battery)]
    [InlineData(TrayIconStyle.HeadsetBattery)]
    public void ASettlingReadingShowsTheGlyph(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(Connected, new BatteryReading(40, false, Settling: true), style, 20, 20));

    [Theory]
    [InlineData(TrayIconStyle.Logo)]
    [InlineData((TrayIconStyle)7)]
    public void TheGlyphChosenOrAStyleNotKnownShowsTheGlyph(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(Connected, new BatteryReading(80, false), style, 20, 20));

    [Theory]
    [InlineData(TrayIconStyle.Percentage)]
    [InlineData(TrayIconStyle.HeadsetBattery)]
    [InlineData(TrayIconStyle.HeadsetPercentage)]
    public void KeepsTheChosenStyle(TrayIconStyle style) =>
        Assert.Equal(style, TrayBattery.Of(Connected, new BatteryReading(80, false), style, 20, 20)!.Value.Style);

    [Theory]
    [InlineData(Link.Absent, Route.Unknown, true)]
    [InlineData(Link.Quiet, Route.ChargingDock, true)]
    [InlineData(Link.Quiet, Route.UsbTransmitter, true)]
    [InlineData(Link.Connecting, Route.Unknown, false)]
    [InlineData(Link.Silent, Route.UsbTransmitter, false)]
    public void AHeadsetStyleSaysDisconnectedOnlyWhenItIsKnown(Link link, Route route, bool disconnected)
    {
        var drawn = TrayBattery.Of(new HeadsetStatus(link, route, "", ""), null, TrayIconStyle.HeadsetBattery, 20, 20);
        Assert.Equal(disconnected, drawn is { Disconnected: true });
        if (!disconnected) Assert.Null(drawn);
    }

    [Theory]
    [InlineData(TrayIconStyle.Battery)]
    [InlineData(TrayIconStyle.Percentage)]
    public void TheOtherStylesShowTheGlyphWhenDisconnected(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(new HeadsetStatus(Link.Absent, Route.Unknown, "", ""), null, style, 20, 20));

    [Fact]
    public void SwitchedOffOnItsCableTheHeadsetStyleShowsTheCharge()
    {
        var cable = new HeadsetStatus(Link.Quiet, Route.DirectUsb, "", "");
        var drawn = TrayBattery.Of(cable, new BatteryReading(60, true), TrayIconStyle.HeadsetPercentage, 20, 20)!.Value;
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
    [InlineData(21, 20, false)]
    [InlineData(20, 20, true)]
    [InlineData(35, 40, true)]
    [InlineData(5, 20, true)]
    public void IsLowAtAndBelowTheWarningLevel(int percent, int warnAt, bool low) =>
        Assert.Equal(low, Drawn(percent, warnAt: warnAt).Low);

    [Theory]
    [InlineData(4, 0, true)]
    [InlineData(6, 0, false)]
    [InlineData(50, 90, true)]
    [InlineData(51, 90, false)]
    public void AWarningLevelOutOfRangeIsHeldToTheChoices(int percent, int warnAt, bool low) =>
        Assert.Equal(low, Drawn(percent, warnAt: warnAt).Low);

    [Fact]
    public void OnPowerItIsNotLow()
    {
        var battery = Drawn(5, charging: true);
        Assert.False(battery.Low);
        Assert.True(battery.Charging);
    }
}
