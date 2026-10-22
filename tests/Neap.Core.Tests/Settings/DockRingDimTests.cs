using Neap.Core.Settings;

namespace Neap.Core.Tests.Settings;

public class DockRingDimTests
{
    [Fact]
    public void TheBrightnessIsKeptAsTheHeadsetLeavesTheDock() =>
        Assert.Equal(60, DockRingDim.ToKeep(enabled: true, onDock: true, brightness: 60, kept: null));

    [Fact]
    public void NothingIsKeptWhileTurnedOff() =>
        Assert.Null(DockRingDim.ToKeep(enabled: false, onDock: true, brightness: 60, kept: null));

    [Fact]
    public void NothingIsKeptOffTheDock() =>
        Assert.Null(DockRingDim.ToKeep(enabled: true, onDock: false, brightness: 60, kept: null));

    [Fact]
    public void AValueAlreadyKeptIsNotReplaced() =>
        Assert.Null(DockRingDim.ToKeep(enabled: true, onDock: true, brightness: 60, kept: 40));

    [Theory]
    [InlineData(0)]
    [InlineData(null)]
    public void ARingAlreadyDarkOrUnreadIsLeftAlone(int? brightness) =>
        Assert.Null(DockRingDim.ToKeep(enabled: true, onDock: true, brightness: brightness, kept: null));

    [Fact]
    public void TheKeptBrightnessComesBackToADarkRing() =>
        Assert.Equal(60, DockRingDim.ToRestore(kept: 60, brightness: DockRingDim.Dark));

    [Fact]
    public void ABrightnessSetSinceIsLeftAlone() =>
        Assert.Null(DockRingDim.ToRestore(kept: 60, brightness: 30));

    [Fact]
    public void ADarkRingWithNothingKeptIsThePersonsChoice() =>
        Assert.Null(DockRingDim.ToRestore(kept: null, brightness: DockRingDim.Dark));
}
