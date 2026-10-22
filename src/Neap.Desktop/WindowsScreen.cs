using System.Runtime.InteropServices;
using Avalonia;

namespace Neap.Desktop;

/// <summary>Reads what Windows is showing on the screen.</summary>
internal static class WindowsScreen
{
    private const uint SourceCopy = 0x00CC0020;

    /// <summary>The pixels shown in an area of the desktop, given in physical pixels, as rows of blue, green, red and an unused byte from the top; null if they cannot be read.</summary>
    public static byte[]? Capture(PixelRect area)
    {
        IntPtr screen = GetDC(IntPtr.Zero);
        if (screen == IntPtr.Zero) return null;
        IntPtr memory = CreateCompatibleDC(screen);
        IntPtr bitmap = CreateCompatibleBitmap(screen, area.Width, area.Height);
        try
        {
            IntPtr previous = SelectObject(memory, bitmap);
            bool copied = BitBlt(memory, 0, 0, area.Width, area.Height, screen, area.X, area.Y, SourceCopy);
            SelectObject(memory, previous);
            if (!copied) return null;

            // A negative height asks for the rows from the top down.
            var header = new BitmapInfoHeader { Size = (uint)Marshal.SizeOf<BitmapInfoHeader>(), Width = area.Width, Height = -area.Height, Planes = 1, BitCount = 32 };
            var pixels = new byte[area.Width * area.Height * 4];
            return GetDIBits(screen, bitmap, 0, (uint)area.Height, pixels, ref header, 0) == area.Height ? pixels : null;
        }
        finally
        {
            DeleteObject(bitmap);
            DeleteDC(memory);
            _ = ReleaseDC(IntPtr.Zero, screen);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BitmapInfoHeader
    {
        public uint Size;
        public int Width;
        public int Height;
        public ushort Planes;
        public ushort BitCount;
        public uint Compression;
        public uint SizeImage;
        public int XPelsPerMeter;
        public int YPelsPerMeter;
        public uint ColoursUsed;
        public uint ColoursImportant;
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr window);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr window, IntPtr context);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr context);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr context, int width, int height);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr context, IntPtr thing);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr target, int x, int y, int width, int height, IntPtr source, int sourceX, int sourceY, uint operation);

    [DllImport("gdi32.dll")]
    private static extern int GetDIBits(IntPtr context, IntPtr bitmap, uint start, uint lines, byte[] bits, ref BitmapInfoHeader header, uint usage);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr thing);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr context);
}
