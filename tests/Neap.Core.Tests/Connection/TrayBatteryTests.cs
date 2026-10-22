using Neap.Core.Connection;

namespace Neap.Core.Tests.Connection;

public class TrayBatteryTests
{
    private static TrayBattery Drawn(int percent, bool charging = false, int warnAt = 20) =>
        TrayBattery.Of(new BatteryReading(percent, charging), TrayIconStyle.Battery, warnAt, 20)!.Value;

    [Theory]
    [InlineData(TrayIconStyle.Battery)]
    [InlineData(TrayIconStyle.Percentage)]
    public void NoReadingShowsTheGlyph(TrayIconStyle style) => Assert.Null(TrayBattery.Of(null, style, 20, 20));

    [Fact]
    public void ASettlingReadingShowsTheGlyph() =>
        Assert.Null(TrayBattery.Of(new BatteryReading(40, false, Settling: true), TrayIconStyle.Battery, 20, 20));

    [Theory]
    [InlineData(TrayIconStyle.Logo)]
    [InlineData((TrayIconStyle)7)]
    public void TheGlyphChosenOrAStyleNotKnownShowsTheGlyph(TrayIconStyle style) =>
        Assert.Null(TrayBattery.Of(new BatteryReading(80, false), style, 20, 20));

    [Fact]
    public void KeepsTheChosenStyle() =>
        Assert.Equal(TrayIconStyle.Percentage,
            TrayBattery.Of(new BatteryReading(80, false), TrayIconStyle.Percentage, 20, 20)!.Value.Style);

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
