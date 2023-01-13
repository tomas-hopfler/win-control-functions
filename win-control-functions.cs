using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
Console.SetWindowSize(20, 7);

[DllImport("user32.dll")]
static extern bool SetCursorPos(int X, int Y);
[System.Runtime.InteropServices.DllImport("user32.dll")]
static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

const int MOUSEEVENTF_LEFTDOWN = 0x02;
const int MOUSEEVENTF_LEFTUP = 0x04;
const int MOUSEEVENTF_RIGHTDOWN = 0x08;
const int MOUSEEVENTF_RIGHTUP = 0x10;


// ---------------- HOW TO USE -----------------
// Bitmap from picture on drive.
Bitmap YourPicture = (Bitmap)Image.FromFile("YourDrive:\\YourDirectory\\YourPicture.png");
// Bitmap from printscreen.
Bitmap screen = (Bitmap)CaptureScreen();
// Point with coords.
Point point = new Point(0, 0);
// Return True if Bitmap is present in other Bitmap False if not. If Bitmap is present, changes the coords of point to center of found YourPicture on screen.
Find(screen, YourPicture, out point);
// Right click on point coords.
RightClick(point);
// Right click on point coords.
LeftClick(point);
// Right click on point coords.
LeftDoubleClick(point);
// Wait time in miliseconds.
Thread.Sleep(500);
// Make sound.
Console.Beep();

static void LeftClick(Point point)
{
    SetCursorPos(point.X, point.Y);
    mouse_event(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, 0);
    mouse_event(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, 0);
}

static void LeftDoubleClick(Point point)
{
    SetCursorPos(point.X, point.Y);
    mouse_event(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, 0);
    mouse_event(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, 0);
    mouse_event(MOUSEEVENTF_LEFTDOWN, point.X, point.Y, 0, 0);
    mouse_event(MOUSEEVENTF_LEFTUP, point.X, point.Y, 0, 0);
}

static void RightClick(Point point)
{
    SetCursorPos(point.X, point.Y);
    mouse_event(MOUSEEVENTF_RIGHTDOWN, point.X, point.Y, 0, 0);
    mouse_event(MOUSEEVENTF_RIGHTUP, point.X, point.Y, 0, 0);
}

Point GetCenter(Point point, Bitmap bitmap)
{
    int xadd = bitmap.Width / 2;
    int yadd = bitmap.Height / 2;
    Point result = point;
    result.X = result.X + xadd;
    result.Y = result.Y + yadd;
    return result;
}

bool Find(Bitmap where, Bitmap what, out Point result)
{
    result = new Point(0, 0);
    if (null == where || null == what)
    {
        return false;
    }
    if (where.Width < what.Width || where.Height < what.Height)
    {
        return false;
    }

    var whereArray = GetArray(where);
    var whatArray = GetArray(what);

    foreach (var firstLineMatchPoint in FindMatch(whereArray.Take(where.Height - what.Height), whatArray[0]))
    {
        if (WhatLocation(whereArray, whatArray, firstLineMatchPoint, 1))
        {
            result = (GetCenter(firstLineMatchPoint, what));
            return true;
        }
    }
    return false;
}

int[][] GetArray(Bitmap bitmap)
{
    var result = new int[bitmap.Height][];
    var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

    for (int y = 0; y < bitmap.Height; ++y)
    {
        result[y] = new int[bitmap.Width];
        Marshal.Copy(bitmapData.Scan0 + y * bitmapData.Stride, result[y], 0, result[y].Length);
    }

    bitmap.UnlockBits(bitmapData);
    return result;
}

IEnumerable<Point> FindMatch(IEnumerable<int[]> whereLines, int[] whatLine)
{
    var y = 0;
    foreach (var whereLine in whereLines)
    {
        for (int x = 0, n = whereLine.Length - whatLine.Length; x < n; ++x)
        {
            if (ContainSameElements(whereLine, x, whatLine, 0, whatLine.Length))
            {
                yield return new Point(x, y);
            }
        }
        y += 1;
    }
}

bool ContainSameElements(int[] first, int firstStart, int[] second, int secondStart, int length)
{
    for (int i = 0; i < length; ++i)
    {
        if (first[i + firstStart] != second[i + secondStart])
        {
            return false;
        }
    }
    return true;
}

bool WhatLocation(int[][] where, int[][] what, Point point, int verified)
{
    for (int y = verified; y < what.Length; ++y)
    {
        if (!ContainSameElements(where[y + point.Y], point.X, what[y], 0, what[y].Length))
        {
            return false;
        }
    }
    return true;
}

Image CaptureScreen()
{
    return CaptureWindow(User32.GetDesktopWindow());
}

Image CaptureWindow(IntPtr handle)
{
    IntPtr hdcSrc = User32.GetWindowDC(handle);
    User32.RECT windowRect = new User32.RECT();
    User32.GetWindowRect(handle, ref windowRect);
    int width = windowRect.right - windowRect.left;
    int height = windowRect.bottom - windowRect.top;
    IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
    IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, width, height);
    IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
    GDI32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32.SRCCOPY);
    GDI32.SelectObject(hdcDest, hOld);
    GDI32.DeleteDC(hdcDest);
    User32.ReleaseDC(handle, hdcSrc);
    Image img = Image.FromHbitmap(hBitmap);
    GDI32.DeleteObject(hBitmap);
    return img;
}
class GDI32
{
    public const int SRCCOPY = 0x00CC0020;
    [DllImport("gdi32.dll")]
    public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
        int nWidth, int nHeight, IntPtr hObjectSource,
        int nXSrc, int nYSrc, int dwRop);
    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
        int nHeight);
    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hDC);
    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);
    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
}

class User32
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
}