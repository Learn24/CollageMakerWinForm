using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Adsophic.PhotoEditor.GUILibrary.WinAPI
{
    public static class WindowUtils
    {
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        private const int GWL_STYLE = -16;

        public static void RemoveMinimizeButton(Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            long windowLong = GetWindowLong(hwnd, GWL_STYLE);
            windowLong = windowLong & -131073;

            SetWindowLong(hwnd, GWL_STYLE, (uint)windowLong);
        }

        public static void RemoveMaximizeButton(Window window) 
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            long windowLong = GetWindowLong(hwnd, GWL_STYLE);

            windowLong = windowLong & -65537;

            SetWindowLong(hwnd, GWL_STYLE, (uint)windowLong);
        }
    }
}
