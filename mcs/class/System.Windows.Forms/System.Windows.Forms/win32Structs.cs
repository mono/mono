/*
 * Copyright (C) 5/11/2002 Carlos Harvey Perez 
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject
 * to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.
 * IN NO EVENT SHALL CARLOS HARVEY PEREZ BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
 * THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Except as contained in this notice, the name of Carlos Harvey Perez
 * shall not be used in advertising or otherwise to promote the sale,
 * use or other dealings in this Software without prior written
 * authorization from Carlos Harvey Perez.
 */


using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

//namespace UtilityLibrary.Win32
namespace System.Windows.Forms
{
	
	/// <summary>
	/// Structures to interoperate with the Windows 32 API  
	/// </summary>
	
 	#region SIZE
	[StructLayout(LayoutKind.Sequential)]
	internal struct SIZE
	{
		internal int cx;
		internal int cy;
	}
	#endregion

	#region RECT
	[StructLayout(LayoutKind.Sequential)]
	internal struct RECT
	{
		internal int left;
		internal int top;
		internal int right;
		internal int bottom;
	}
	#endregion

	#region INITCOMMONCONTROLSEX
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal class INITCOMMONCONTROLSEX 
	{
		internal int dwSize = 8;
		internal CommonControlInitFlags dwICC;
	}
	#endregion

	#region TBBUTTON
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct TBBUTTON 
	{
		internal int iBitmap;
		internal int idCommand;
		internal byte fsState;
		internal byte fsStyle;
		internal byte bReserved0;
		internal byte bReserved1;
		internal int dwData;
		internal int iString;
	}
	#endregion

	#region POINT
	[StructLayout(LayoutKind.Sequential)]
	internal struct POINT
	{
		internal int x;
		internal int y;
	}
	#endregion

	#region NMHDR
	[StructLayout(LayoutKind.Sequential)]
	internal struct NMHDR
	{
		internal IntPtr hwndFrom;
		internal int idFrom;
		internal int code;
	}
	#endregion

	#region TOOLTIPTEXTA
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Ansi)]
	internal struct TOOLTIPTEXTA
	{
		internal NMHDR hdr;
		internal IntPtr lpszText;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
		internal string szText;
		internal IntPtr hinst;
		internal ToolTipFlags flags;
	}
	#endregion

	#region TOOLTIPTEXT
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct TOOLTIPTEXT
	{
		internal NMHDR hdr;
		internal IntPtr lpszText;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
		internal string szText;
		internal IntPtr hinst;
		internal int uFlags;
	}
	#endregion

	#region NMCUSTOMDRAW
	[StructLayout(LayoutKind.Sequential)]
	internal struct NMCUSTOMDRAW
	{
		internal NMHDR hdr;
		internal int dwDrawStage;
		internal IntPtr hdc;
		internal RECT rc;
		internal int dwItemSpec;
		internal int uItemState;
		internal IntPtr lItemlParam;
	}
	#endregion

	#region NMTBCUSTOMDRAW
	[StructLayout(LayoutKind.Sequential)]
	internal struct NMTBCUSTOMDRAW
	{
		internal NMCUSTOMDRAW nmcd;
		internal IntPtr hbrMonoDither;
		internal IntPtr hbrLines;
		internal IntPtr hpenLines;
		internal int clrText;
		internal int clrMark;
		internal int clrTextHighlight;
		internal int clrBtnFace;
		internal int clrBtnHighlight;
		internal int clrHighlightHotTrack;
		internal RECT rcText;
		internal int nStringBkMode;
		internal int nHLStringBkMode;
	}
	#endregion
	
	#region NMLVCUSTOMDRAW
	[StructLayout(LayoutKind.Sequential)]
	internal struct NMLVCUSTOMDRAW 
	{
		internal NMCUSTOMDRAW nmcd;
		internal int clrText;
		internal int clrTextBk;
		internal int iSubItem;
	} 
	#endregion

	#region TBBUTTONINFO
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct TBBUTTONINFO
	{
		internal int cbSize;
		internal int dwMask;
		internal int idCommand;
		internal int iImage;
		internal byte fsState;
		internal byte fsStyle;
		internal short cx;
		internal IntPtr lParam;
		internal IntPtr pszText;
		internal int cchText;
	}
	#endregion

	#region REBARBANDINFO
	[StructLayout(LayoutKind.Sequential)]
	internal struct REBARBANDINFO
	{
		internal int cbSize;
		internal RebarInfoMask fMask;
		internal RebarStylesEx fStyle;
		internal int clrFore;
		internal int clrBack;
		internal IntPtr lpText;
		internal int cch;
		internal int iImage;
		internal IntPtr hwndChild;
		internal int cxMinChild;
		internal int cyMinChild;
		internal int cx;
		internal IntPtr hbmBack;
		internal int wID;
		internal int cyChild;
		internal int cyMaxChild;
		internal int cyIntegral;
		internal int cxIdeal;
		internal int lParam;
		internal int cxHeader;
	}
	#endregion

	#region MOUSEHOOKSTRUCT
	[StructLayout(LayoutKind.Sequential)]
	internal struct MOUSEHOOKSTRUCT 
	{ 
		internal POINT     pt; 
		internal IntPtr    hwnd; 
		internal int       wHitTestCode; 
		internal IntPtr    dwExtraInfo; 
	}
	#endregion

	#region NMTOOLBAR
	[StructLayout(LayoutKind.Sequential)]
	internal struct NMTOOLBAR 
	{
		internal NMHDR		hdr;
		internal int		    iItem;
		internal TBBUTTON	    tbButton;
		internal int		    cchText;
		internal IntPtr		pszText;
		internal RECT		    rcButton; 
	}
	#endregion
	
	#region NMREBARCHEVRON
	[StructLayout(LayoutKind.Sequential)]
	internal struct NMREBARCHEVRON
	{
		internal NMHDR hdr;
		internal int uBand;
		internal int wID;
		internal int lParam;
		internal RECT rc;
		internal int lParamNM;
	}
	#endregion

	#region BITMAP
	[StructLayout(LayoutKind.Sequential)]
	internal struct BITMAP
	{
		internal long   bmType; 
		internal long   bmWidth; 
		internal long   bmHeight; 
		internal long   bmWidthBytes; 
		internal short  bmPlanes; 
		internal short  bmBitsPixel; 
		internal IntPtr bmBits; 
	}
	#endregion
 
	#region BITMAPINFO_FLAT
	[StructLayout(LayoutKind.Sequential)]
	internal struct BITMAPINFO_FLAT 
	{
		internal int      bmiHeader_biSize;
		internal int      bmiHeader_biWidth;
		internal int      bmiHeader_biHeight;
		internal short    bmiHeader_biPlanes;
		internal short    bmiHeader_biBitCount;
		internal int      bmiHeader_biCompression;
		internal int      bmiHeader_biSizeImage;
		internal int      bmiHeader_biXPelsPerMeter;
		internal int      bmiHeader_biYPelsPerMeter;
		internal int      bmiHeader_biClrUsed;
		internal int      bmiHeader_biClrImportant;
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=1024)]
		internal byte[] bmiColors; 
	}
	#endregion

    #region RGBQUAD
	internal struct RGBQUAD 
	{
		internal byte		rgbBlue;
		internal byte		rgbGreen;
		internal byte		rgbRed;
		internal byte		rgbReserved;
	}
	#endregion
	
	#region BITMAPINFOHEADER
	[StructLayout(LayoutKind.Sequential)]
	internal class BITMAPINFOHEADER 
	{
		internal int      biSize = Marshal.SizeOf(typeof(BITMAPINFOHEADER));
		internal int      biWidth;
		internal int      biHeight;
		internal short    biPlanes;
		internal short    biBitCount;
		internal int      biCompression;
		internal int      biSizeImage;
		internal int      biXPelsPerMeter;
		internal int      biYPelsPerMeter;
		internal int      biClrUsed;
		internal int      biClrImportant;
	}
	#endregion

	#region BITMAPINFO
	[StructLayout(LayoutKind.Sequential)]
	internal class BITMAPINFO 
	{
		internal BITMAPINFOHEADER bmiHeader = new BITMAPINFOHEADER();
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=1024)]
		internal byte[] bmiColors; 
	}
	#endregion

	#region PALETTEENTRY
	[StructLayout(LayoutKind.Sequential)]
	internal struct PALETTEENTRY 
	{
		internal byte		peRed;
		internal byte		peGreen;
		internal byte		peBlue;
		internal byte		peFlags;
	}
	#endregion

	#region MESSAGE
	[StructLayout(LayoutKind.Sequential)]
	internal struct MESSAGE
	{
		internal IntPtr hwnd;
		internal int message;
		internal IntPtr wParam;
		internal IntPtr lParam;
		internal int time;
		internal int pt_x;
		internal int pt_y;
	}
	#endregion

	#region HD_HITTESTINFO
	[StructLayout(LayoutKind.Sequential)]
	internal struct HD_HITTESTINFO 
	{  
		internal POINT pt;  
		internal HeaderControlHitTestFlags flags; 
		internal int iItem; 
	}
	#endregion
 
	#region DLLVERSIONINFO
	[StructLayout(LayoutKind.Sequential)]
	internal struct DLLVERSIONINFO
	{
		internal int cbSize;
		internal int dwMajorVersion;
		internal int dwMinorVersion;
		internal int dwBuildNumber;
		internal int dwPlatformID;
	}
	#endregion

	#region PAINTSTRUCT
	[StructLayout(LayoutKind.Sequential)]
	internal struct PAINTSTRUCT
	{
		internal IntPtr hdc;
		internal int fErase;
		internal RECT rcPaint;
		internal int fRestore;
		internal int fIncUpdate;
		internal int Reserved1;
		internal int Reserved2;
		internal int Reserved3;
		internal int Reserved4;
		internal int Reserved5;
		internal int Reserved6;
		internal int Reserved7;
		internal int Reserved8;
	}
	#endregion

	#region BLENDFUNCTION
	[StructLayout(LayoutKind.Sequential, Pack=1)]
	internal struct BLENDFUNCTION
	{
		internal byte BlendOp;
		internal byte BlendFlags;
		internal byte SourceConstantAlpha;
		internal byte AlphaFormat;
	}

	#endregion
	
	#region TRACKMOUSEEVENTS
	[StructLayout(LayoutKind.Sequential)]
	internal struct TRACKMOUSEEVENT
	{
		internal int cbSize; // = 16
		internal int dwFlags;	// not TrackerEventFlags 
		internal IntPtr hWnd;
		internal int dwHoverTime;
	}
	#endregion

	#region NMTVCUSTOMDRAW
	[StructLayout(LayoutKind.Sequential)]
	internal struct NMTVCUSTOMDRAW 
	{
		internal NMCUSTOMDRAW nmcd;
		internal int clrText;
		internal int clrTextBk;
		internal int iLevel;
	}
	#endregion

	#region TVITEM
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct TVITEM 
	{
		internal	int      mask;
		internal	IntPtr    hItem;
		internal	int      state;
		internal	int      stateMask;
		internal	IntPtr    pszText;
		internal	int       cchTextMax;
		internal	int       iImage;
		internal	int       iSelectedImage;
		internal	int       cChildren;
		internal	IntPtr    lParam;
	} 
	#endregion

	#region LVITEM
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct LVITEM
	{
		internal	ListViewItemFlags mask;
		internal	int iItem;
		internal	int iSubItem;
		internal	int state;
		internal	int stateMask;
		internal	IntPtr pszText;
		internal	int cchTextMax;
		internal	int iImage;
		internal	int lParam;
		internal	int iIndent;
	}
	#endregion

	#region HDITEM
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct HDITEM
	{
		internal	HeaderItemFlags mask;
		internal	int     cxy;
		internal	IntPtr  pszText;
		internal	IntPtr  hbm;
		internal	int     cchTextMax;
		internal	int     fmt;
		internal	int     lParam;
		internal	int     iImage;      
		internal	int     iOrder;
	}	
	#endregion

    #region WINDOWPLACEMENT
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct WINDOWPLACEMENT
	{	
		internal uint length; //Were int in original code
		internal uint flags; //
		internal uint showCmd; //
		internal POINT ptMinPosition; 
		internal POINT ptMaxPosition; 
		internal RECT  rcNormalPosition; 
	}
	#endregion

	#region SCROLLINFO
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct SCROLLINFO
	{
		internal 	int   cbSize;
		internal 	int   fMask;
		internal 	int    nMin;
		internal 	int    nMax;
		internal 	int   nPage;
		internal 	int    nPos;
		internal 	int    nTrackPos;
	}
	#endregion

	#region SHFILEINFO
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct SHFILEINFO
	{ 
		internal IntPtr hIcon; 
		internal int    iIcon; 
		internal int   dwAttributes; 
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
		internal string szDisplayName; 
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
		internal string szTypeName; 
	}
				
	#endregion

	#region SHITEMID
	[StructLayout(LayoutKind.Sequential)]
	internal struct SHITEMID 
	{ 
		internal short cb; 
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=1)]
		internal byte[]  abID; 
	}
	#endregion

	#region ITEMIDLIST
	[StructLayout(LayoutKind.Sequential)]
	internal struct ITEMIDLIST 
	{
		internal SHITEMID mkid;
	}
	#endregion
 
	#region IID
	[StructLayout(LayoutKind.Sequential)]
	internal struct IID
	{
		int x;
		short s1;
		short s2;
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=8)]
		byte[] chars; 
	}
	#endregion

	#region REFIID
	[StructLayout(LayoutKind.Sequential)]
	internal struct REFIID
	{
		internal int x;
		internal short s1;
		internal short s2;
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=8)]
		internal byte[] chars; 

		internal REFIID(string guid)
		{
			// Needs to be a string of the form:
			// "000214E6-0000-0000-c000-000000000046"
			string[] data = guid.Split('-');
			Debug.Assert(data.Length == 5);
            x = Convert.ToInt32(data[0], 16);
			s1 = Convert.ToInt16(data[1], 16);
			s2 = Convert.ToInt16(data[2], 16);
			string bytesData = data[3] + data[4];
			chars = new byte[] { Convert.ToByte(bytesData.Substring(0,2), 16),  Convert.ToByte(bytesData.Substring(2,2), 16), 
			 Convert.ToByte(bytesData.Substring(4,2), 16),  Convert.ToByte(bytesData.Substring(6,2), 16),
			 Convert.ToByte(bytesData.Substring(8,2), 16),  Convert.ToByte(bytesData.Substring(10,2), 16), 
			 Convert.ToByte(bytesData.Substring(12,2), 16),  Convert.ToByte(bytesData.Substring(14,2), 16) }; 
		}

	}
	#endregion

	#region STRRET
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct STRRET
	{
		internal STRRETFlags     uType;         // One of the STRRET values
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=1024)]
		internal byte[]			cStr;
	}
	#endregion

	#region STRRET_EX
	[StructLayout(LayoutKind.Explicit)]
	internal struct STRRET_EX
	{
		[FieldOffset(0)] internal STRRETFlags     uType;         // One of the STRRET values
		[FieldOffset(4)] internal IntPtr          pOLEString; 
	}
	#endregion

	#region TVINSERTSTRUCT
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct TVINSERTSTRUCT
	{
		internal int hParent;
		internal int hInsertAfter;
		internal TVITEM   item;
    }
	#endregion

	#region NM_TREEVIEW
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct NM_TREEVIEW
	{
		internal NMHDR     hdr;
		internal int      action;
		internal TVITEM    itemOld;
		internal TVITEM    itemNew;
		internal POINT     ptDrag;
	}
	#endregion

	#region TVHITTESTINFO
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct TVHITTESTINFO
	{
		internal POINT  pt;
		internal TreeViewHitTestFlags  flags;
		internal IntPtr hItem;
	}
	#endregion

	#region TVSORTCB
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct TVSORTCB
	{
		internal IntPtr hParent;
		internal Win32.CompareFunc lpfnCompare;
		internal int lParam;
	}
	#endregion

	#region SCROLLBARINFO
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct SCROLLBARINFO
	{
		internal int  cbSize;
		internal RECT  rcScrollBar;
		internal int   dxyLineButton;
		internal int   xyThumbTop;
		internal int   xyThumbBottom;
		internal int   reserved;
		[MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=6)]
		internal int[] rgstate;
	}
	#endregion

	#region CMINVOKECOMMANDINFO
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct CMINVOKECOMMANDINFO
	{
		internal int cbSize;				// sizeof(CMINVOKECOMMANDINFO)
		internal int fMask;				// any combination of CMIC_MASK_*
		internal IntPtr hwnd;				// might be NULL (indicating no owner window)
		internal IntPtr lpVerb;			// either a string or MAKEINTRESOURCE(idOffset)
		internal IntPtr lpParameters;		// might be NULL (indicating no parameter)
		internal IntPtr lpDirectory;		// might be NULL (indicating no specific directory)
		internal int nShow;				// one of SW_ values for ShowWindow() API
		internal int dwHotKey;
		internal IntPtr hIcon;
	}
	#endregion

	#region NMHEADER
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct NMHEADER
	{
		internal NMHDR   hdr;
		internal int     iItem;
		internal int     iButton;
		internal HDITEM  hItem;
	}
	#endregion

	#region SYSTIME
	[ StructLayout( LayoutKind.Sequential )]
	public class SYSTIME
	{
		public ushort wYear; 
		public ushort wMonth; 
		public ushort wDayOfWeek; 
		public ushort wDay; 
		public ushort wHour; 
		public ushort wMinute; 
		public ushort wSecond; 
		public ushort wMilliseconds; 
	}
	#endregion

	[StructLayout(LayoutKind.Sequential)]
	internal struct MSG {
		internal IntPtr   hwnd;
		internal Msg  message; 
		internal IntPtr wParam; 
		internal IntPtr lParam; 
		internal uint  time; 
		internal POINT  pt;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct WNDCLASS {
		internal int style;
		internal Win32.WndProc lpfnWndProc;
		internal int cbClsExtra;
		internal int cbWndExtra;
		internal IntPtr hInstance;
		internal IntPtr hIcon;
		internal IntPtr hCursor;
		internal IntPtr hbrBackground;
		internal string lpszMenuName;
		internal string lpszClassName;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MEASUREITEMSTRUCT {
		public uint      CtlType; 
		public uint      CtlID; 
		public int       itemID; 
		public int       itemWidth; 
		public int       itemHeight; 
		public IntPtr    itemData; 
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct DRAWITEMSTRUCT {
		public uint      CtlType; 
		public uint      CtlID; 
		public int       itemID; 
		public uint      itemAction; 
		public int       itemState; 
		public IntPtr    hwndItem; 
		public IntPtr    hDC; 
		public RECT      rcItem; 
		public IntPtr    itemData; 
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	internal struct TOOLINFO {
		internal uint	cbSize; 
		internal uint 	uFlags; 
		internal IntPtr	hwnd; 
		internal uint	uId; 
		internal RECT	rect; 
		internal IntPtr hinst; 
		internal string lpszText; 
		internal IntPtr lParam;
	}

	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
	internal struct NM_UPDOWN
	{
		internal NMHDR   hdr;
		internal int     iPos;
		internal int     iDelta;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct CLIENTCREATESTRUCT {
		internal IntPtr hWindowMenu; 
		internal uint   idFirstChild; 
	}

	//
	//
	//		[StructLayout(LayoutKind.Sequential)]
	//		internal struct WNDCLASSA {
	//			uint style;
	//			WNDPROC lpfnWndProc;
	//			INT cbClsExtra;
	//			INT cbWndExtra;
	//			HINSTANCE hInstance;
	//			HICON hIcon;
	//			HCURSOR hCursor;
	//			HBRUSH hbrBackground;
	//			LPCSTR lpszMenuName;
	//			LPCSTR lpszClassName;
	//		}
	
}

