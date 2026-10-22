namespace Neap.Core.Tests;

public class MiniPanelTests
{
    private static readonly Box Screen = new(0, 0, 1920, 1080);

    [Fact]
    public void BesideATaskbarAtTheFootItIsBottomRight() =>
        Assert.Equal(new Box(1568, 570, 340, 450),
            MiniPanel.Place(Screen, new Box(0, 0, 1920, 1032), 340, 450, 12, null));

    [Fact]
    public void BesideATaskbarAtTheTopItIsTopRight() =>
        Assert.Equal(new Box(1568, 60, 340, 450),
            MiniPanel.Place(Screen, new Box(0, 48, 1920, 1032), 340, 450, 12, null));

    [Fact]
    public void BesideATaskbarOnTheRightItIsBottomRight() =>
        Assert.Equal(new Box(1520, 618, 340, 450),
            MiniPanel.Place(Screen, new Box(0, 0, 1872, 1080), 340, 450, 12, null));

    [Fact]
    public void BesideATaskbarOnTheLeftItIsBottomLeft() =>
        Assert.Equal(new Box(60, 618, 340, 450),
            MiniPanel.Place(Screen, new Box(48, 0, 1872, 1080), 340, 450, 12, null));

    [Fact]
    public void WithNoTaskbarToFindItIsBottomRight() =>
        Assert.Equal(new Box(1568, 618, 340, 450), MiniPanel.Place(Screen, Screen, 340, 450, 12, null));

    [Fact]
    public void OnASecondScreenItStaysOnThatScreen()
    {
        var second = new Box(1920, 0, 2560, 1440);
        Assert.Equal(new Box(4128, 930, 340, 450),
            MiniPanel.Place(second, new Box(1920, 0, 2560, 1392), 340, 450, 12, null));
    }

    [Fact]
    public void ItNeverGrowsPastTheWorkArea() =>
        Assert.Equal(new Box(12, 12, 676, 376),
            MiniPanel.Place(new Box(0, 0, 700, 400), new Box(0, 0, 700, 400), 800, 800, 12, null));

    [Fact]
    public void AClickOnATaskbarAtTheFootPutsItAboveTheClick() =>
        Assert.Equal(new Box(1430, 570, 340, 450),
            MiniPanel.Place(Screen, new Box(0, 0, 1920, 1032), 340, 450, 12, (1600, 1056)));

    [Fact]
    public void AClickOnATaskbarAtTheTopPutsItBelowTheClick() =>
        Assert.Equal(new Box(830, 60, 340, 450),
            MiniPanel.Place(Screen, new Box(0, 48, 1920, 1032), 340, 450, 12, (1000, 20)));

    [Fact]
    public void AClickNearTheEndOfTheTaskbarKeepsItInsideTheWorkArea() =>
        Assert.Equal(new Box(1568, 570, 340, 450),
            MiniPanel.Place(Screen, new Box(0, 0, 1920, 1032), 340, 450, 12, (1900, 1056)));

    [Fact]
    public void AClickOnATaskbarOnTheLeftPutsItBesideTheClick() =>
        Assert.Equal(new Box(60, 400, 340, 450),
            MiniPanel.Place(Screen, new Box(48, 0, 1872, 1080), 340, 450, 12, (24, 625)));

    [Fact]
    public void AClickOnATaskbarOnASecondScreenPutsItThereAboveTheClick()
    {
        var second = new Box(1920, 0, 2560, 1440);
        Assert.Equal(new Box(4030, 930, 340, 450),
            MiniPanel.Place(second, new Box(1920, 0, 2560, 1392), 340, 450, 12, (4200, 1416)));
    }

    [Fact]
    public void AClickInsideTheWorkAreaLeavesItInTheCorner() =>
        Assert.Equal(new Box(1568, 570, 340, 450),
            MiniPanel.Place(Screen, new Box(0, 0, 1920, 1032), 340, 450, 12, (900, 500)));

    [Fact]
    public void AClickOpensAClosedPanel() =>
        Assert.True(MiniPanel.Opens(false, TimeSpan.FromSeconds(5)));

    [Fact]
    public void AClickClosesAnOpenPanel() =>
        Assert.False(MiniPanel.Opens(true, TimeSpan.FromSeconds(5)));

    [Fact]
    public void AClickOnTheIconTogglesThePanelWhileItIsOn() =>
        Assert.Equal(TrayClick.ToggleMiniPanel, MiniPanel.Click(true));

    [Fact]
    public void AClickOnTheIconOpensTheWindowWhileThePanelIsOff() =>
        Assert.Equal(TrayClick.OpenWindow, MiniPanel.Click(false));

    [Fact]
    public void AClickRightAfterTheCloseIsThatCloseAndOpensNothing() =>
        Assert.False(MiniPanel.Opens(false, TimeSpan.FromMilliseconds(80)));

    [Fact]
    public void ThePanelIsDrawnInTheLookChosen() =>
        Assert.Equal(PanelLook.FrostyGlass, MiniPanel.Look(PanelLook.FrostyGlass, false));

    [Fact]
    public void HighContrastDrawsThePanelPlain() =>
        Assert.Equal(PanelLook.Standard, MiniPanel.Look(PanelLook.FrostyGlass, true));
}
