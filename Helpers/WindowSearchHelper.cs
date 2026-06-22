using System;
using System.Runtime.InteropServices;

namespace XIVClearSS.Helpers
{
    public class WindowSearchHelper
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        public IntPtr FindGameWindow()
        {
            IntPtr handle = IntPtr.Zero;
            while ((handle = FindWindowEx(IntPtr.Zero, handle, "FFXIVGAME", "FINAL FANTASY XIV")) != IntPtr.Zero)
            {
                GetWindowThreadProcessId(handle, out int pid);
                if (pid == Environment.ProcessId && IsWindowVisible(handle))
                    break;
            }
            return handle;
        }
    }
}
