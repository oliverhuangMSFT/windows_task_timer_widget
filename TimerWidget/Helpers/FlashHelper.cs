using System;
using System.Runtime.InteropServices;

namespace TimerWidget.Helpers
{
    public static class FlashHelper
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 12;
        private const uint FLASHW_STOP = 0;

        public static void FlashWindow(IntPtr hwnd)
        {
            var fi = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = hwnd,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = uint.MaxValue,
                dwTimeout = 0
            };
            FlashWindowEx(ref fi);
        }

        public static void StopFlash(IntPtr hwnd)
        {
            var fi = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = hwnd,
                dwFlags = FLASHW_STOP,
                uCount = 0,
                dwTimeout = 0
            };
            FlashWindowEx(ref fi);
        }
    }
}
