//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Tools.Common
{
    using System;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;

    
    internal static class Win32Interop
    {
        public const int WM_SETICON = 0x80;
        public const int WM_NCHITTEST = 0x84;
        public const int WM_SYSCOMMAND = 0x0112;

        public const int GWL_STYLE = -16;
        public const int WS_MAXIMIZEBOX = 0x00010000;
        public const int WS_MINIMIZEBOX = 0x00020000;
        public const int WS_CLIPCHILDREN = 0x02000000;
        public const int WS_CLIPSIBLINGS = 0x04000000;

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_DLGMODALFRAME = 0x00000001;
        public const int WS_EX_CONTEXTHELP = 0x00000400;

        public const int SC_CONTEXTHELP = 0xf180;

        public const int ICON_SMALL = 0;
        public const int ICON_BIG = 1;
        

        [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "This class is shared by various projects and may not be used by a specific project")]
        [StructLayout(LayoutKind.Sequential)]
        public sealed class POINT
        {
            public int x;
            public int y;

            public POINT()
            {
                this.x = 0;
                this.y = 0;
            }

            public POINT(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }

        internal static void MakeWindowClipSiblingsAndChildren(HandleRef hwnd)
        {
            IntPtr windowStyle = Win32Interop.GetWindowLongPtr(hwnd.Handle, Win32Interop.GWL_STYLE);
            if (IntPtr.Size == 4)
            {
                windowStyle = new IntPtr(windowStyle.ToInt32() | Win32Interop.WS_CLIPSIBLINGS | Win32Interop.WS_CLIPCHILDREN);
            }
            else
            {
                windowStyle = new IntPtr(windowStyle.ToInt64() | ((long)Win32Interop.WS_CLIPSIBLINGS) | ((long)Win32Interop.WS_CLIPCHILDREN));
            }
            Win32Interop.SetWindowLongPtr(hwnd, Win32Interop.GWL_STYLE, (IntPtr)windowStyle);
        }

        // This static method is required because legacy OSes do not support
        // SetWindowLongPtr
        internal static IntPtr SetWindowLongPtr(HandleRef hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }


        // This static method is required because Win32 does not support
        // GetWindowLongPtr directly
        public static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            else
                return GetWindowLongPtr32(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, IntPtr dwNewLong);

        [SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Justification = "Calling code is expected to handle the different size of IntPtr")]
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This function is shared by various projects and may not be used by a specific project")]
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This function is shared by various projects and may not be used by a specific project")]
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This function is shared by various projects and may not be used by a specific project")]
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "This function is shared by various projects and may not be used by a specific project")]
        [DllImport("User32", EntryPoint = "ScreenToClient", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern int ScreenToClient(IntPtr hWnd, [In, Out] POINT pt);
    }
}
