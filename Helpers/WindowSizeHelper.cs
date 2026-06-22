using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;

namespace XIVClearSS.Helpers
{
    public unsafe class WindowSizeHelper
    {
        [DllImport("user32.dll")]
        private static extern bool AdjustWindowRect(ref Rect lpRect, uint dwStyle, bool bMenu);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SendMessageA(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int index);

        private const int  GWL_STYLE      = -16;
        private const int  WM_EXITSIZEMOVE = 0x0232;
        private const uint SWP_NOACTIVATE  = 0x0010;
        private const uint SWP_NOMOVE      = 0x0002;
        private const uint SWP_NOZORDER    = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        private readonly WindowSearchHelper _search;
        private IntPtr _hwnd = IntPtr.Zero;

        public WindowSizeHelper(WindowSearchHelper search)
        {
            _search = search;
        }

        public (uint width, uint height) GetCurrentSize()
        {
            Device* dev = Device.Instance();
            return (dev->Width, dev->Height);
        }

        public void SetSize(int width, int height)
        {
            if (_hwnd == IntPtr.Zero)
            {
                _hwnd = _search.FindGameWindow();
                if (_hwnd == IntPtr.Zero)
                    throw new Exception("[XIVClearSS] Could not find FFXIV window handle.");
            }

            // Tell the game engine to resize its render target
            Device* dev = Device.Instance();
            dev->NewWidth  = (uint)width;
            dev->NewHeight = (uint)height;
            dev->RequestResolutionChange = 1;

            // Resize the OS window to match
            Rect clientRect = new Rect(0, 0, width, height);
            uint style = (uint)GetWindowLongPtr(_hwnd, GWL_STYLE);
            AdjustWindowRect(ref clientRect, style, false);

            int cx = clientRect.Right  - clientRect.Left;
            int cy = clientRect.Bottom - clientRect.Top;

            SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, cx, cy,
                SWP_NOACTIVATE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED);
            SendMessageA(_hwnd, WM_EXITSIZEMOVE, 0, 0);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left, Top, Right, Bottom;
        public Rect(int left, int top, int right, int bottom)
        {
            Left = left; Top = top; Right = right; Bottom = bottom;
        }
    }
}
