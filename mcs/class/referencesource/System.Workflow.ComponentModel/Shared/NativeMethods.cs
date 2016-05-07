// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Activities\Common
 * The two files must be kept in [....].  Any change made here must also
 * be made to WF\Activities\Common\NativeMethods.cs
*********************************************************************/
namespace System.Workflow.Interop
{
    using System;
    using System.Runtime.InteropServices;
    using System.Diagnostics.CodeAnalysis;

    static class NativeMethods
    {
        internal const int HDI_WIDTH = 0x0001;
        internal const int HDI_HEIGHT = HDI_WIDTH;
        internal const int HDI_TEXT = 0x0002;
        internal const int HDI_FORMAT = 0x0004;
        internal const int HDI_LPARAM = 0x0008;
        internal const int HDI_BITMAP = 0x0010;
        internal const int HDI_IMAGE = 0x0020;
        internal const int HDI_DI_SETITEM = 0x0040;
        internal const int HDI_ORDER = 0x0080;
        internal const int HDI_FILTER = 0x0100;
        internal const int HDF_LEFT = 0x0000;
        internal const int HDF_RIGHT = 0x0001;
        internal const int HDF_CENTER = 0x0002;
        internal const int HDF_JUSTIFYMASK = 0x0003;
        internal const int HDF_RTLREADING = 0x0004;
        internal const int HDF_OWNERDRAW = 0x8000;
        internal const int HDF_STRING = 0x4000;
        internal const int HDF_BITMAP = 0x2000;
        internal const int HDF_BITMAP_ON_RIGHT = 0x1000;
        internal const int HDF_IMAGE = 0x0800;
        internal const int HDF_SORTUP = 0x0400;
        internal const int HDF_SORTDOWN = 0x0200;
        internal const int LVM_GETHEADER = (0x1000 + 31);
        internal const int HDM_GETITEM = (0x1200 + 11);
        internal const int HDM_SETITEM = (0x1200 + 12);

        internal const int HORZRES = 8;
        internal const int VERTRES = 10;
        internal const int LOGPIXELSX = 88;
        internal const int LOGPIXELSY = 90;
        internal const int PHYSICALWIDTH = 110;
        internal const int PHYSICALHEIGHT = 111;
        internal const int PHYSICALOFFSETX = 112;
        internal const int PHYSICALOFFSETY = 113;
        internal const int WM_SETREDRAW = 0x000B;
        internal const int HOLLOW_BRUSH = 5;
        internal const int OBJ_PEN = 1;
        internal const int OBJ_BRUSH = 2;
        internal const int OBJ_EXTPEN = 11;
        internal const int GM_ADVANCED = 2;
        internal const int PS_COSMETIC = 0x00000000;
        internal const int PS_USERSTYLE = 7;
        internal const int BS_SOLID = 0;
        internal const int WS_POPUP = unchecked((int)0x80000000);
        internal const int WS_EX_DLGMODALFRAME = 0x00000001;
        internal const int WM_SETICON = 0x0080;
        internal const int SMALL_ICON = 0;
        internal const int LARGE_ICON = 1;
        internal const int PS_SOLID = 0;
        internal const int SWP_NOSIZE = unchecked((int)0x0001);
        internal const int SWP_NOZORDER = unchecked((int)0x0004);
        internal const int SWP_NOACTIVATE = unchecked((int)0x0010);
        internal const int WM_NOTIFY = unchecked((int)0x004E);
        internal const int WM_SETFONT = unchecked((int)0x0030);
        internal const int WS_EX_TOPMOST = unchecked((int)0x00000008L);
        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_KEYUP = 0x101;
        internal const int WM_SYSKEYDOWN = 0x104;
        internal const int WM_SYSKEYUP = 0x105;

        internal const int TTF_IDISHWND = (0x0001);
        internal const int TTF_CENTERTIP = (0x0002);
        internal const int TTF_RTLREADING = (0x0004);
        internal const int TTF_SUBCLASS = (0x0010);
        internal const int TTF_TRACK = (0x0020);
        internal const int TTF_ABSOLUTE = (0x0080);
        internal const int TTF_TRANSPARENT = (0x0100);
        internal const int TTF_PARSELINKS = (0x1000);
        internal const int TTF_DI_SETITEM = (0x8000);

        internal const int TTS_ALWAYSTIP = (0x01);
        internal const int TTS_NOPREFIX = (0x02);
        internal const int TTS_NOANIMATE = (0x10);
        internal const int TTS_NOFADE = (0x20);
        internal const int TTS_BALLOON = (0x40);
        internal const int TTS_CLOSE = (0x80);

        internal const int TTDT_AUTOMATIC = 0;
        internal const int TTDT_RESHOW = 1;
        internal const int TTDT_AUTOPOP = 2;
        internal const int TTDT_INITIAL = 3;

        internal const int TTI_NONE = 0;
        internal const int TTI_INFO = 1;
        internal const int TTI_WARNING = 2;
        internal const int TTI_ERROR = 3;

        internal static readonly int TTN_GETDISPINFO;
        internal static readonly int TTN_NEEDTEXT;
        internal static readonly int TTN_SHOW = ((0 - 520) - 1);
        internal static readonly int TTN_POP = ((0 - 520) - 2);

        internal static readonly int TTM_POP = (0x0400 + 28);
        internal static readonly int TTM_ADDTOOL;
        internal static readonly int TTM_SETTITLE;
        internal static readonly int TTM_DELTOOL;
        internal static readonly int TTM_NEWTOOLRECT;
        internal static readonly int TTM_GETTOOLINFO;
        internal static readonly int TTM_SETTOOLINFO;
        internal static readonly int TTM_HITTEST;
        internal static readonly int TTM_GETTEXT;
        internal static readonly int TTM_UPDATETIPTEXT;
        internal static readonly int TTM_ENUMTOOLS;
        internal static readonly int TTM_GETCURRENTTOOL;
        internal static readonly int TTM_TRACKACTIVATE = (0x0400 + 17);
        internal static readonly int TTM_TRACKPOSITION = (0x0400 + 18);
        internal static readonly int TTM_ACTIVATE = (0x0400 + 1);
        internal static readonly int TTM_ADJUSTRECT = (0x400 + 31);
        internal static readonly int TTM_SETDELAYTIME = (0x0400 + 3);
        internal static readonly int TTM_RELAYEVENT = (0x0400 + 7);
        internal static readonly int TTM_UPDATE = (0x0400 + 29);
        internal static readonly int TTM_WINDOWFROMPOINT = (0x0400 + 16);
        internal static readonly int TTM_GETDELAYTIME = (0x0400 + 21);
        internal static readonly int TTM_SETMAXTIPWIDTH = (0x0400 + 24);

        private const int TTN_GETDISPINFOA = ((0 - 520) - 0);
        private const int TTN_GETDISPINFOW = ((0 - 520) - 10);
        private const int TTN_NEEDTEXTA = ((0 - 520) - 0);
        private const int TTN_NEEDTEXTW = ((0 - 520) - 10);

        private const int TTM_SETTITLEA = (0x0400 + 32);
        private const int TTM_SETTITLEW = (0x0400 + 33);
        private const int TTM_ADDTOOLA = (0x0400 + 4);
        private const int TTM_ADDTOOLW = (0x0400 + 50);
        private const int TTM_DELTOOLA = (0x0400 + 5);
        private const int TTM_DELTOOLW = (0x0400 + 51);
        private const int TTM_NEWTOOLRECTA = (0x0400 + 6);
        private const int TTM_NEWTOOLRECTW = (0x0400 + 52);
        private const int TTM_GETTOOLINFOA = (0x0400 + 8);
        private const int TTM_GETTOOLINFOW = (0x0400 + 53);
        private const int TTM_SETTOOLINFOA = (0x0400 + 9);
        private const int TTM_SETTOOLINFOW = (0x0400 + 54);
        private const int TTM_HITTESTA = (0x0400 + 10);
        private const int TTM_HITTESTW = (0x0400 + 55);
        private const int TTM_GETTEXTA = (0x0400 + 11);
        private const int TTM_GETTEXTW = (0x0400 + 56);
        private const int TTM_UPDATETIPTEXTA = (0x0400 + 12);
        private const int TTM_UPDATETIPTEXTW = (0x0400 + 57);
        private const int TTM_ENUMTOOLSA = (0x0400 + 14);
        private const int TTM_ENUMTOOLSW = (0x0400 + 58);
        private const int TTM_GETCURRENTTOOLA = (0x0400 + 15);
        private const int TTM_GETCURRENTTOOLW = (0x0400 + 59);

        static NativeMethods()
        {
            if (Marshal.SystemDefaultCharSize == 1)
            {
                TTN_GETDISPINFO = TTN_GETDISPINFOA;
                TTN_NEEDTEXT = TTN_NEEDTEXTA;

                TTM_ADDTOOL = TTM_ADDTOOLA;
                TTM_SETTITLE = TTM_SETTITLEA;
                TTM_DELTOOL = TTM_DELTOOLA;
                TTM_NEWTOOLRECT = TTM_NEWTOOLRECTA;
                TTM_GETTOOLINFO = TTM_GETTOOLINFOA;
                TTM_SETTOOLINFO = TTM_SETTOOLINFOA;
                TTM_HITTEST = TTM_HITTESTA;
                TTM_GETTEXT = TTM_GETTEXTA;
                TTM_UPDATETIPTEXT = TTM_UPDATETIPTEXTA;
                TTM_ENUMTOOLS = TTM_ENUMTOOLSA;
                TTM_GETCURRENTTOOL = TTM_GETCURRENTTOOLA;
            }
            else
            {
                TTN_GETDISPINFO = TTN_GETDISPINFOW;
                TTN_NEEDTEXT = TTN_NEEDTEXTW;

                TTM_ADDTOOL = TTM_ADDTOOLW;
                TTM_SETTITLE = TTM_SETTITLEW;
                TTM_DELTOOL = TTM_DELTOOLW;
                TTM_NEWTOOLRECT = TTM_NEWTOOLRECTW;
                TTM_GETTOOLINFO = TTM_GETTOOLINFOW;
                TTM_SETTOOLINFO = TTM_SETTOOLINFOW;
                TTM_HITTEST = TTM_HITTESTW;
                TTM_GETTEXT = TTM_GETTEXTW;
                TTM_UPDATETIPTEXT = TTM_UPDATETIPTEXTW;
                TTM_ENUMTOOLS = TTM_ENUMTOOLSW;
                TTM_GETCURRENTTOOL = TTM_GETCURRENTTOOLW;
            }
        }

        internal static bool Failed(int hr)
        {
            return (hr < 0);
        }

        internal static int ThrowOnFailure(int hr)
        {
            return ThrowOnFailure(hr, null);
        }

        internal static int ThrowOnFailure(int hr, params int[] expectedHRFailure)
        {
            if (Failed(hr))
            {
                if ((null == expectedHRFailure) || (Array.IndexOf(expectedHRFailure, hr) < 0))
                {
                    Marshal.ThrowExceptionForHR(hr);
                }
            }

            return hr;
        }

        internal static IntPtr ListView_GetHeader(IntPtr hWndLV)
        {
            return SendMessage(hWndLV, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
        }

        internal static bool Header_GetItem(IntPtr hWndHeader, int index, [In, Out] NativeMethods.HDITEM hdi)
        {
            IntPtr success = SendMessage(hWndHeader, HDM_GETITEM, new IntPtr(index), hdi);
            return (success != IntPtr.Zero) ? true : false;
        }

        internal static bool Header_SetItem(IntPtr hWndHeader, int index, [In, Out] NativeMethods.HDITEM hdi)
        {
            IntPtr success = SendMessage(hWndHeader, HDM_SETITEM, new IntPtr(index), hdi);
            return (success != IntPtr.Zero) ? true : false;
        }

        //[DllImport("gdi32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        //public static extern IntPtr CreateSolidBrush(int crColor);

        //[DllImport("gdi32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        //internal static extern bool RoundRect(HandleRef hDC, int left, int top, int right, int bottom, int width, int height);

        //[DllImport("gdi32.dll", ExactSpelling = true, EntryPoint = "CreatePen", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        //internal static extern IntPtr CreatePen(int nStyle, int nWidth, int crColor);

        [DllImport("gdi32", EntryPoint = "DeleteObject", CharSet = CharSet.Auto)]
        internal static extern bool DeleteObject(IntPtr hObject);

        [System.Runtime.InteropServices.DllImport("gdi32.dll", ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

        [DllImport("user32", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, [In, Out] NativeMethods.HDITEM lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public extern static IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool LineTo(HandleRef hdc, int x, int y);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern bool MoveToEx(HandleRef hdc, int x, int y, POINT pt);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SelectObject(HandleRef hdc, HandleRef obj);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr GetCurrentObject(HandleRef hDC, uint uObjectType);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int DeleteObject(HandleRef hObject);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern IntPtr ExtCreatePen(int style, int nWidth, LOGBRUSH logbrush, int styleArrayLength, int[] styleArray);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int SetWorldTransform(HandleRef hdc, XFORM xform);

        [DllImport("gdi32.dll", SetLastError = true, ExactSpelling = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        public static extern int SetGraphicsMode(HandleRef hdc, int iMode);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TOOLINFO ti);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref RECT rc);

        [DllImport("user32.dll")]
        internal static extern int SetWindowPos(IntPtr hWnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, int flags);

        [System.Runtime.InteropServices.ComVisible(false), StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class HDITEM
        {
            public int mask = 0;
            public int cxy = 0;
            public IntPtr pszText = IntPtr.Zero;
            public IntPtr hbm = IntPtr.Zero;
            public int cchTextMax = 0;
            public int fmt = 0;
            public int lParam = 0;
            public int image = 0;
            public int order = 0;
            public int type = 0;
            public IntPtr filter = IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class XFORM
        {
            //Default is identity matrix
            public float eM11 = 1.0f;
            public float eM12 = 0.0f;
            public float eM21 = 0.0f;
            public float eM22 = 1.0f;
            public float eDx = 0.0f;
            public float eDy = 0.0f;

            public XFORM()
            {

            }

            public XFORM(System.Drawing.Drawing2D.Matrix transform)
            {
                this.eM11 = transform.Elements[0];
                this.eM12 = transform.Elements[1];
                this.eM21 = transform.Elements[2];
                this.eM22 = transform.Elements[3];
                this.eDx = transform.Elements[4];
                this.eDy = transform.Elements[5];
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class LOGBRUSH
        {
            public int lbStyle;
            public int lbColor;
            public long lbHatch;

            public LOGBRUSH(int style, int color, int hatch)
            {
                this.lbStyle = style;
                this.lbColor = color;
                this.lbHatch = hatch;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        internal class NMHDR
        {
            public IntPtr hwndFrom;
            public int idFrom;
            public int code;

            public NMHDR()
            {
                this.hwndFrom = IntPtr.Zero;
                this.idFrom = 0;
                this.code = 0;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        internal struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Auto)]
        internal struct TOOLINFO
        {
            public int size;
            public int flags;
            public IntPtr hwnd;
            public IntPtr id;
            public RECT rect;
            public IntPtr hinst;
            [SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources", Justification = "Not a security threat since its used by designer scenarios only")]
            public IntPtr text;
            public IntPtr lParam;
        }
    }
}
