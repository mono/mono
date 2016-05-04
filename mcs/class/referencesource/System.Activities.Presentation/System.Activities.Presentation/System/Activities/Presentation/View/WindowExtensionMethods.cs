//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;
    using Microsoft.Tools.Common;

    static class WindowExtensionMethods
    {
        public static void ShowContextHelpButton(this Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            IntPtr exStyle = Win32Interop.GetWindowLongPtr(hwnd, Win32Interop.GWL_EXSTYLE);
            if (IntPtr.Size == 4)
            {
                exStyle = new IntPtr(exStyle.ToInt32() | Win32Interop.WS_EX_CONTEXTHELP);
            }
            else
            {
                exStyle = new IntPtr(exStyle.ToInt64() | ((long)Win32Interop.WS_EX_CONTEXTHELP));
            }
            Win32Interop.SetWindowLongPtr(new HandleRef(window, hwnd), Win32Interop.GWL_EXSTYLE, exStyle);
        }

        public static void HideMinMaxButton(this Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            IntPtr style = Win32Interop.GetWindowLongPtr(hwnd, Win32Interop.GWL_STYLE);
            if (IntPtr.Size == 4)
            {
                int intValue = style.ToInt32();
                intValue = SetBit(Win32Interop.WS_MAXIMIZEBOX, intValue, false);
                intValue = SetBit(Win32Interop.WS_MINIMIZEBOX, intValue, false);
                style = new IntPtr(intValue);
            }
            else
            {
                long longValue = style.ToInt64();
                longValue = SetBit((long)Win32Interop.WS_MAXIMIZEBOX, longValue, false);
                longValue = SetBit((long)Win32Interop.WS_MINIMIZEBOX, longValue, false);
                style = new IntPtr(longValue);
            }
            Win32Interop.SetWindowLongPtr(new HandleRef(window, hwnd), Win32Interop.GWL_STYLE, style);
        }

        public static void AddWindowsHook(this Window window, HwndSourceHook wmHandler)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(wmHandler);
        }

        public static void RemoveWindowsHook(this Window window, HwndSourceHook wmHandler)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.RemoveHook(wmHandler);
        }

        public static void HideIcon(this Window window)
        {
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            IntPtr exStyle = Win32Interop.GetWindowLongPtr(hwnd, Win32Interop.GWL_EXSTYLE);
            if (IntPtr.Size == 4)
            {
                exStyle = new IntPtr(exStyle.ToInt32() | Win32Interop.WS_EX_DLGMODALFRAME);
            }
            else
            {
                exStyle = new IntPtr(exStyle.ToInt64() | ((long)Win32Interop.WS_EX_DLGMODALFRAME));
            }
            Win32Interop.SetWindowLongPtr(new HandleRef(window, hwnd), Win32Interop.GWL_EXSTYLE, exStyle);

            Win32Interop.SendMessage(hwnd, Win32Interop.WM_SETICON, new IntPtr(Win32Interop.ICON_SMALL), IntPtr.Zero);
            Win32Interop.SendMessage(hwnd, Win32Interop.WM_SETICON, new IntPtr(Win32Interop.ICON_BIG), IntPtr.Zero);
        }

        private static long SetBit(long mask, long value, bool flag)
        {
            if (flag)
            {
                return value | mask;
            }
            else
            {
                return value & ~mask;
            }
        }

        private static int SetBit(int mask, int value, bool flag)
        {
            if (flag)
            {
                return value | mask;
            }
            else
            {
                return value & ~mask;
            }
        }
    }
}
