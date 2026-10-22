using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;

namespace Neap.Desktop;

/// <summary>The mini panel drawn as frosted glass.</summary>
public partial class MiniPanelWindow
{
    private const int CornerPreference = 33, BorderColour = 34, Rounded = 2;

    /// <summary>The colour that tells Windows to draw no edge of its own round the window.</summary>
    private const int NoColour = unchecked((int)0xFFFFFFFE);

    private bool _frosted;
    private bool _blurred;

    /// <summary>Draws the panel as frosted glass: the look's colours, over the system's blur where there is one.</summary>
    /// <remarks>
    /// The blur fills the window to its square edges, so it is asked for only
    /// where the window's corners can be rounded to the card's, which is
    /// Windows 11. Elsewhere, and while the system gives no blur, as with its
    /// transparency effects off, the glass is solid; see <see cref="OnPropertyChanged"/>.
    /// </remarks>
    private void Frost()
    {
        _frosted = true;
        Resources.MergedDictionaries.Add(new ResourceInclude(new Uri("avares://Neap.Desktop/"))
        {
            Source = new Uri("avares://Neap.Desktop/Themes/Frosty.axaml"),
        });
        Classes.Add("frosty");
        Classes.Add("solid");
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            TransparencyLevelHint = [WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Transparent];
    }

    /// <summary>Has Windows round the window's corners to the card's, and leave its own edge off so the glass's shows.</summary>
    private void RoundGlass()
    {
        if (!_frosted || !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) || TryGetPlatformHandle()?.Handle is not { } window) return;
        int rounded = Rounded, none = NoColour;
        int failed = DwmSetWindowAttribute(window, CornerPreference, ref rounded, sizeof(int));
        if (failed == 0) failed = DwmSetWindowAttribute(window, BorderColour, ref none, sizeof(int));
        if (failed != 0) AppLog.Write($"could not round the mini panel's corners: 0x{failed:X8}");
    }

    /// <summary>Makes the glass solid while the system gives no blur behind it, and clear again once it does.</summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (!_frosted || change.Property != ActualTransparencyLevelProperty) return;
        _blurred = ActualTransparencyLevel == WindowTransparencyLevel.AcrylicBlur;
        Classes.Set("solid", !_blurred);
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr window, int attribute, ref int value, int size);
}
