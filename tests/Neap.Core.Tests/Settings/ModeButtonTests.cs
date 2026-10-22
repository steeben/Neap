using Neap.Core.Settings;

namespace Neap.Core.Tests.Settings;

public class ModeButtonTests
{
    [Fact]
    public void TheCyclesAreOfferedStraightAfterNoiseCancellationOnOff() =>
        Assert.Equal(
            [
                new ModeChoice(0, NoiseCycle.None), new ModeChoice(0, NoiseCycle.AllThree),
                new ModeChoice(0, NoiseCycle.SkippingOff), new ModeChoice(1, NoiseCycle.None), new ModeChoice(2, NoiseCycle.None),
            ],
            ModeButton.Choices);

    [Theory]
    [InlineData(0, NoiseCycle.AllThree, NoiseCycle.AllThree)]
    [InlineData(0, NoiseCycle.SkippingOff, NoiseCycle.SkippingOff)]
    [InlineData(0, NoiseCycle.None, NoiseCycle.None)]
    [InlineData(1, NoiseCycle.AllThree, NoiseCycle.None)]
    [InlineData(2, NoiseCycle.SkippingOff, NoiseCycle.None)]
    public void ACycleShowsOnlyWhileTheHeadsetTogglesNoiseCancellation(int function, NoiseCycle cycle, NoiseCycle shown) =>
        Assert.Equal(new ModeChoice(function, shown), ModeButton.Shown(function, cycle));

    [Fact]
    public void NothingIsShownUntilTheHeadsetReports() => Assert.Null(ModeButton.Shown(null, NoiseCycle.AllThree));

    [Theory]
    [InlineData(false, false, NoiseCycle.None)]
    [InlineData(false, true, NoiseCycle.None)]
    [InlineData(true, false, NoiseCycle.AllThree)]
    [InlineData(true, true, NoiseCycle.SkippingOff)]
    public void TheSavedSwitchesReadAsACycle(bool cycles, bool skipsOff, NoiseCycle cycle) =>
        Assert.Equal(cycle, ModeButton.CycleOf(cycles, skipsOff));
}
