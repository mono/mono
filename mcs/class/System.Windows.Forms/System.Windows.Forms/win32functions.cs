/*
 * Copyright (C) 5/11/2002 Carlos Harvey Perez 
 * Modifcations and additons, Copyright Ximian 2002/3
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

//using UtilityLibrary.WinControls;

//namespace UtilityLibrary.Win32
namespace System.Windows.Forms{
	/// <summary>
	/// Windows API Functions
	/// </summary>
	public class Win32 
	{
		#region Constructors
		// No need to construct this object
		#endregion
		
		#region Constans values
		internal const string TOOLBARCLASSNAME = "ToolbarWindow32";
		internal const string REBARCLASSNAME = "ReBarWindow32";
		internal const string PROGRESSBARCLASSNAME = "msctls_progress32";
		internal const string SCROLLBAR = "SCROLLBAR";
		internal const string TOOLTIPS_CLASS = "tooltips_class32";
		internal const string MDICLIENTCLASSNAME = "MDICLIENT";
		internal const string TABCONTROL = "SysTabControl32";
		internal const string DEFAULT_WINDOW_CLASS = "mono_scrollable_control";
		
		#endregion

		#region CallBacks
		internal delegate IntPtr FnHookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam);
		internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
		internal delegate int CompareFunc(IntPtr param1, IntPtr param2, IntPtr sortParam);
		internal delegate int WinProc(IntPtr hWnd, int message, int wParam, int lParam);
		internal delegate int WinProcMsg(IntPtr hWnd, Msg message, int wParam, int lParam);
		#endregion

		#region Kernel32.dll functions
		[DllImport("kernel32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
		internal static extern int GetCurrentThreadId();
		[DllImport("kernel32.dll")]
		internal static extern int GetDriveType(string rootPathName);
		[DllImport("kernel32.dll")]
		internal static extern int GetVolumeInformation(string drivePath,
			StringBuilder volumeNameBuffer,
			int driveNameBufferSize,
			out int serialNumber,
			out int maxFileNameLength,
			out int fileSystemFlags,
			StringBuilder systemNameBuffer,
			int systemNameBufferSize);

		[DllImport("kernel32.dll")]
		internal static extern IntPtr LoadLibraryA(string filename);
		
		[DllImport("kernel32.dll")]
		internal static extern bool FreeLibrary(IntPtr handle);
		
		[DllImport("kernel32.dll", EntryPoint="OutputDebugStringW")]
		internal static extern void OutputDebugString(string message);

		[DllImport ("kernel32.dll", CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static uint GetLastError ();
		
		[DllImport ("kernel32.dll", CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Auto, EntryPoint = "FormatMessageW")]
		internal extern static uint  FormatMessage (
			uint flags, IntPtr lpSource,uint messageId, uint languageId,
			StringBuilder lpBuffer, int nSize, IntPtr Arguments);
				
		internal static string FormatMessage(uint error) {
			StringBuilder sb = new StringBuilder(2048);
			Win32.FormatMessage( (uint)(FM_.FORMAT_MESSAGE_FROM_SYSTEM | FM_.FORMAT_MESSAGE_IGNORE_INSERTS),
				IntPtr.Zero, error, 0, sb, sb.Capacity, IntPtr.Zero);
			return sb.ToString();
		}
	#endregion
	
		#region Gdi32.dll functions
		[DllImport("gdi32.dll")]
		static internal extern bool StretchBlt(IntPtr hDCDest, int XOriginDest, int YOriginDest, int WidthDest, int HeightDest,
			IntPtr hDCSrc,  int XOriginScr, int YOriginSrc, int WidthScr, int HeightScr, PatBltTypes Rop);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr CreateCompatibleDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int Width, int Heigth);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
		[DllImport("gdi32.dll")]
		static internal extern bool BitBlt(IntPtr hDCDest, int XOriginDest, int YOriginDest, int WidthDest, int HeightDest,
			IntPtr hDCSrc,  int XOriginScr, int YOriginSrc, PatBltTypes flags);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr DeleteDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		static internal extern bool PatBlt(IntPtr hDC, int XLeft, int YLeft, int Width, int Height, int Rop);
		[DllImport("gdi32.dll")]
		static internal extern bool DeleteObject(IntPtr hObject);
		[DllImport("gdi32.dll")]
		static internal extern int GetPixel(IntPtr hDC, int XPos, int YPos);
		[DllImport("gdi32.dll")]
		static internal extern int SetMapMode(IntPtr hDC, int fnMapMode);
		[DllImport("gdi32.dll")]
		static internal extern int GetObjectType(IntPtr handle);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO_FLAT bmi, 
			int iUsage, ref int ppvBits, IntPtr hSection, int dwOffset);
		[DllImport("gdi32.dll")]
		internal static extern int GetDIBits(IntPtr hDC, IntPtr hbm, int StartScan, int ScanLines, int lpBits, BITMAPINFOHEADER bmi, int usage);
		[DllImport("gdi32.dll")]
		internal static extern int GetDIBits(IntPtr hdc, IntPtr hbm, int StartScan, int ScanLines, int lpBits, ref BITMAPINFO_FLAT bmi, int usage);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr GetPaletteEntries(IntPtr hpal, int iStartIndex, int nEntries, byte[] lppe);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr GetSystemPaletteEntries(IntPtr hdc, int iStartIndex, int nEntries, byte[] lppe);
		[DllImport("gdi32.dll")]
		internal static extern int SetDCBrushColor(IntPtr hdc,  int crColor);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateSolidBrush(int crColor);
		[DllImport("gdi32.dll")]
		internal static extern BackgroundMode SetBkMode(IntPtr hDC, BackgroundMode mode);
		[DllImport("gdi32.dll")]
		internal static extern int SetViewportOrgEx(IntPtr hdc,  int x, int y,  int param);
		[DllImport("gdi32.dll")]
		internal static extern int SetTextColor(IntPtr hDC, int colorRef);
		[DllImport("gdi32.dll")]
		internal static extern int SetStretchBltMode(IntPtr hDC, StrechModeFlags StrechMode);
		[DllImport("gdi32.dll")]
		internal static extern int SetPixel(IntPtr hDC, int x, int y, int color);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreatePen(PenStyle penStyle, int width, int color);
		[DllImport("gdi32.dll")]
		internal static extern int GetClipRgn(IntPtr hDC, ref IntPtr region);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateRectRgn(int nLeftRect,  int TopRect, int nRightRect, int nBottomRect);
		[DllImport("gdi32.dll")]
		internal static extern int GetRgnBox(IntPtr hRegion, ref RECT rc);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr GetStockObject(GSO_ objectType);
		[DllImport("gdi32.dll",CharSet = CharSet.Ansi,EntryPoint="ExtTextOutA")]
		internal static extern int ExtTextOut(IntPtr hdc, int x, int y,
			ExtTextOutFlags options, ref RECT rc, int str, int strLen, IntPtr distances);
		[DllImport("gdi32.dll",CharSet = CharSet.Ansi,EntryPoint="ExtTextOutA")]
		internal static extern int ExtTextOut(IntPtr hdc, int x, int y,
			ExtTextOutFlags options, ref RECT rc, string str, int strLen, IntPtr distances);
		[DllImport("gdi32.dll",CharSet = CharSet.Ansi,EntryPoint="GetTextExtentPoint32A")]
		internal static extern bool GetTextExtentPoint32(IntPtr hDC, string lpString, int cbString, ref SIZE lpSize);


		[DllImport ("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern uint GetBkColor (IntPtr hdc);

		[DllImport ("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern uint SetBkColor (IntPtr hdc, uint crColor);

		internal static int RGB(Color color) 
		{
			return color.R | (color.G << 8) | (color.B << 16);
		}
		
		#endregion

		#region Uxtheme.dll functions
		[DllImport("uxtheme.dll")]
		static public extern int SetWindowTheme(IntPtr hWnd, StringBuilder AppID, StringBuilder ClassID);
		static public void DisableWindowsXPTheme(IntPtr hWnd) 
		{
			// Disable using the Window XP Theme for the Window handle
			// passed as a parameter
			StringBuilder applicationName = new StringBuilder(" ", 1); 
			StringBuilder classIDs = new StringBuilder(" " , 1); 
			Win32.SetWindowTheme(hWnd, applicationName, classIDs);
		}
		#endregion
	
		#region user32.dll functions
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern IntPtr GetDesktopWindow();
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool ShowWindow(IntPtr hWnd, ShowWindowStyles State);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool OpenClipboard(IntPtr hWndNewOwner);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool CloseClipboard();
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool EmptyClipboard();
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern IntPtr SetClipboardData( int Format, IntPtr hData);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool GetMenuItemRect(IntPtr hWnd, IntPtr hMenu, int Item, ref RECT rc);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, Msg msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, int msg, int wParam, ref RECT lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref POINT lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, string lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, Msg msg, int wParam, string lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, ToolBarMessages msg, int wParam, ref TBBUTTON lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, ToolBarMessages msg, int wParam, ref TBBUTTONINFO lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, RebarMessages msg, int wParam, ref REBARBANDINFO lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVITEM lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVINSERTSTRUCT lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVSORTCB lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVHITTESTINFO hti);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, ListViewMessages msg, int wParam, ref LVITEM lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, HeaderControlMessages msg, int wParam, ref HDITEM lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern void SendMessage(IntPtr hWnd, HeaderControlMessages msg, int wParam, ref HD_HITTESTINFO hti);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage(IntPtr hWnd, HeaderControlMessages msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="PostMessageA")]
		internal static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="PostMessageA")]
		internal static extern IntPtr PostMessage(IntPtr hWnd, Msg msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto, EntryPoint="SetWindowsHookExW")]
		internal static extern IntPtr SetWindowsHookEx(WindowsHookCodes hookid, HookProc pfnhook, IntPtr hinst, int threadid);
		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		internal static extern bool UnhookWindowsHookEx(IntPtr hhook);
		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		internal static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wparam, IntPtr lparam);
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="DrawTextA")]
		internal extern static int DrawText(IntPtr hdc, string lpString, int nCount, ref RECT lpRect, DrawTextFormatFlags flags);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal extern static IntPtr GetDlgItem(IntPtr hDlg, int nControlID);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal extern static int InvalidateRect(IntPtr hWnd,  ref RECT rc, int bErase);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal extern static int InvalidateRect(IntPtr hWnd,  IntPtr rc, int bErase);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool WaitMessage();
		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		internal static extern int SendMessage2ref(IntPtr hWnd, int msg, ref int wParam, ref int lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="PeekMessageA")]
		internal static extern bool PeekMessage(ref MESSAGE msg, int hWnd, int wFilterMin, int wFilterMax, PeekMessageFlags flags);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="GetMessageA")]
		internal static extern bool GetMessage(ref MESSAGE msg, int hWnd, int wFilterMin, int wFilterMax);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool TranslateMessage(ref MESSAGE msg);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="DispatchMessageA")]
		internal static extern bool DispatchMessage(ref MESSAGE msg);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="LoadCursorA")]
		internal static extern IntPtr LoadCursor(IntPtr hInstance, CursorType cursor);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SetCursor(IntPtr hCursor);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, 
			IntPtr hdcSrc, ref POINT pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, UpdateLayeredWindowFlags dwFlags);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);
		
		internal static bool ClientToScreen(IntPtr hWnd, ref RECT rect) {
			POINT pt1 = new POINT();
			pt1.x = rect.left;
			pt1.y = rect.top;
			POINT pt2 = new POINT();
			pt2.x = rect.right;
			pt2.y = rect.bottom;
			bool result = Win32.ClientToScreen(hWnd, ref pt1);
			result &= Win32.ClientToScreen(hWnd, ref pt2);
			rect.left = pt1.x;
			rect.top = pt1.y;
			rect.right = pt2.x;
			rect.bottom = pt2.y;
			return result;
		}


		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool ScreenToClient(IntPtr hWnd, ref POINT pt);

		internal static bool ScreenToClient(IntPtr hWnd, ref RECT rect) {
			POINT pt1 = new POINT();
			pt1.x = rect.left;
			pt1.y = rect.top;
			POINT pt2 = new POINT();
			pt2.x = rect.right;
			pt2.y = rect.bottom;
			bool result = Win32.ScreenToClient(hWnd, ref pt1);
			result &= Win32.ScreenToClient(hWnd, ref pt2);
			rect.left = pt1.x;
			rect.top = pt1.y;
			rect.right = pt2.x;
			rect.bottom = pt2.y;
			return result;
		}

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT tme);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern short GetKeyState(int virtKey);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

		[DllImport("user32.dll", CharSet=CharSet.Auto, EntryPoint="GetClassNameW")]
		internal static extern int GetClassName(IntPtr hWnd,  StringBuilder ClassName, int nMaxCount);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="SetWindowLongA")]
		internal static extern int SetWindowLong(IntPtr hWnd, GetWindowLongFlag flag, int dwNewLong);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="SetWindowLongA")]
		internal static extern IntPtr SetWindowLong(IntPtr hWnd, GetWindowLongFlag flag, WinProc winProc);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="SetWindowLongA")]
		internal static extern IntPtr SetWindowLong(IntPtr hWnd, GetWindowLongFlag flag, WndProc winProc);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hRegion, int flags);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int FillRect(IntPtr hDC, ref RECT rect, IntPtr hBrush);

		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SetWindowTextA")]
		internal static extern int SetWindowText(IntPtr hWnd, string text);

		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="GetWindowTextA")]
		internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

		[DllImport("user32.dll", CharSet=CharSet.Ansi,EntryPoint="SendMessageA")]
		static internal extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)] 
		static internal extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int GetSystemMetrics(SystemMetricsCodes code);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int SetScrollInfo(IntPtr hwnd,  int bar, ref SCROLLINFO si, int fRedraw);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int ShowScrollBar(IntPtr hWnd, int bar,  int show);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int EnableScrollBar(IntPtr hWnd, int flags, int arrows);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int GetScrollInfo(IntPtr hwnd, int bar, ref SCROLLINFO si);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int ScrollWindowEx(IntPtr hWnd, int dx, int dy, 
			ref RECT rcScroll, ref RECT rcClip, IntPtr UpdateRegion, ref RECT rcInvalidated, int flags);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int LockWindowUpdate(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool ValidateRect(IntPtr hWnd, ref RECT rcInvalidated);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool ValidateRect(IntPtr hWnd, IntPtr rc);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int GetScrollBarInfo(IntPtr hWnd, SystemObject id, ref SCROLLBARINFO sbi);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="GetWindowLongA")]
		static internal extern IntPtr GetWindowLong(IntPtr hWnd, GetWindowLongFlag flag);

		[DllImport("user32.dll", CharSet=CharSet.Auto, EntryPoint="SetPropW")]
		static internal extern int SetProp(IntPtr hWnd, IntPtr atom, IntPtr hData);

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="CallWindowProcA")]
		static internal extern int CallWindowProc(IntPtr hOldProc, IntPtr hWnd, int message, int wParam, int lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int EndMenu();

		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="DefWindowProcA")]
		static internal extern int DefWindowProc(IntPtr hWnd, int message, int wParam, int lParam);

		[DllImport("user32.dll", CharSet=CharSet.Unicode,EntryPoint="DefMDIChildProcA")]
		static internal extern IntPtr DefMDIChildProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Unicode,EntryPoint="DefFrameProcA")]
		static internal extern IntPtr DefFrameProc(IntPtr hWnd, IntPtr hWndMDIClient, Msg Msg, IntPtr wParam, IntPtr lParam);
		
		[DllImport("user32.dll", CharSet=CharSet.Auto,EntryPoint="LoadCursorA")]
		static internal extern IntPtr LoadCursor(IntPtr hInstance, LC_ standardCursor);

		[DllImport("user32.dll", CharSet=CharSet.Auto, EntryPoint="RegisterWindowMessageA")]
		static internal extern int RegisterWindowMessage( string message_name);

		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetMenu (IntPtr hWnd);
		
		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int SetMenu (IntPtr hWnd, IntPtr hMenu);

		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern int InsertMenuA(IntPtr hMenu, uint pos, uint uflags, IntPtr NewItem, string item);
		
		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern int RemoveMenu(IntPtr hMenu, uint pos, uint uflags);
		
		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall)]
		internal static extern int DrawMenuBar (IntPtr hWnd);
		
		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall)]
		internal static extern int SetMenuDefaultItem(IntPtr hMenu, int uItem, int fByPos );

		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal extern static int AdjustWindowRect( ref RECT rc, int dwStyle, int bMenu);

		[DllImport ("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal extern static int AdjustWindowRectEx( ref RECT rc, int dwStyle, int bMenu, int dwStyleEx);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int DrawEdge(IntPtr hdc, ref RECT rc, Border3DStyle edge, Border3DSide flags);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int DrawFrameControl(IntPtr hdc, ref RECT rc, uint uType, uint uState);
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int DrawFocusRect( IntPtr hdc, ref RECT rc);
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern IntPtr WindowFromPoint( POINT pt);
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int GetSysColor( GetSysColorIndex color);

		internal delegate void TimerProc(IntPtr hWnd, uint uMsg, uint idEvent, int dwTime);
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern uint SetTimer (IntPtr hWnd, uint nIDEvent, uint uElapse, TimerProc lpTimerFunc);
		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern bool KillTimer (IntPtr hWnd, uint nIDEvent);
		#endregion

		#region Shell32.dll functions

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SHGetFileInfo(string drivePath, int fileAttributes,
			out SHFILEINFO fileInfo, int countBytesFileInfo, ShellFileInfoFlags flags);

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SHGetFileInfo(IntPtr idl, int fileAttributes,
			out SHFILEINFO fileInfo, int countBytesFileInfo, ShellFileInfoFlags flags);

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, ShellSpecialFolder folder, out IntPtr idl);

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetMalloc(out IMalloc alloc);

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetDesktopFolder(out IShellFolder folder);

		[DllImport("shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetPathFromIDList(IntPtr idl, StringBuilder path);

		internal static void SHFreeMalloc(IntPtr handle) 
		{
			IMalloc alloc = null;
			try 
			{
				Win32.SHGetMalloc(out alloc);
				Debug.Assert(alloc != null);
				alloc.Free(handle);
				// Free allocator itself
				IUnknown iUnknown = (IUnknown)alloc;
				iUnknown.Release();
			}
			catch (Exception e) 
			{
				// In case the Garbage collector is trying to free
				// this memory from its own thread
				Debug.WriteLine(e.Message);
			}
		}

		#endregion

		#region Common Controls functions

		[DllImport("comctl32.dll")]
		internal static extern bool InitCommonControlsEx(INITCOMMONCONTROLSEX icc);

		[DllImport("comctl32.dll")]
		internal static extern bool InitCommonControls();

		[DllImport("comctl32.dll", EntryPoint="DllGetVersion")]
		internal extern static int GetCommonControlDLLVersion(ref DLLVERSIONINFO dvi);

		[DllImport("comctl32.dll")]
		internal static extern IntPtr ImageList_Create(int width, int height, int flags, int count, int grow);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_Destroy(IntPtr handle);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_Add(IntPtr imageHandle, IntPtr hBitmap, IntPtr hMask);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_Remove(IntPtr imageHandle, int index);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_BeginDrag(IntPtr imageHandle, int imageIndex, int xHotSpot, int yHotSpot);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_DragEnter(IntPtr hWndLock, int x, int y);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_DragMove(int x, int y);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_DragLeave(IntPtr hWndLock);

		[DllImport("comctl32.dll")]
		internal static extern void ImageList_EndDrag();

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_Draw(IntPtr hImageList, int imageIndex, 
			IntPtr hDCDest, int x, int y, ImageListDrawFlags flags);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_DrawEx(
			IntPtr hImageList, int imageIndex, IntPtr hDCDest, int x, int y, int dx, int dy, 
			uint backColor, uint foregColor, ImageListDrawFlags flags);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_DragShowNolock(int show);
		
		[DllImport("comctl32.dll")]
		internal static extern int ImageList_AddMasked(IntPtr hImageList, IntPtr hBitmap, int crMask);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_SetDragCursorImage(IntPtr himlDrag, int iDrag, int dxHotspot, int dyHotspot);

		internal static int ImageList_DrawEx(IntPtr hImageList, int imageIndex, IntPtr hDCDest, int x, int y, int dx, int dy,   
			ImageListDrawColor backColor, ImageListDrawColor foreColor, ImageListDrawFlags flags) 
		{
			uint bColor = (uint)ImageListDrawColors.CLR_NONE;
			if ( backColor == ImageListDrawColor.Default )
				bColor =  (uint)ImageListDrawColors.CLR_DEFAULT;

			uint fColor = (uint)ImageListDrawColors.CLR_NONE;
			if ( foreColor == ImageListDrawColor.Default )
				fColor =  (uint)ImageListDrawColors.CLR_DEFAULT;
			 
			// Call actual function
			return ImageList_DrawEx(hImageList, imageIndex, hDCDest, x, y, dx, dy, bColor, fColor, flags);
		}

		
		static internal bool IsCommonCtrl6() 
		{
			DLLVERSIONINFO dllVersion = new DLLVERSIONINFO();
			// We are assummng here that anything greater or equal than 6
			// will have the new XP theme drawing enable
			dllVersion.cbSize = Marshal.SizeOf(typeof(DLLVERSIONINFO));
			Win32.GetCommonControlDLLVersion(ref dllVersion);
			return (dllVersion.dwMajorVersion >= 6);
		}

		#endregion

		#region Win32 Macro-Like helpers
		internal static int X_LPARAM(int lParam) 
		{
			return (lParam & 0xffff);
		}
	 
		internal static int Y_LPARAM(int lParam) 
		{
			return (lParam >> 16);
		}

		internal static Point GetPointFromLPARAM(int lParam) 
		{
			return new Point(X_LPARAM(lParam), Y_LPARAM(lParam));
		}

		internal static int LOW_ORDER(int param) 
		{
			return (ushort)param /*(param & 0xffff)*/;
		}

		internal static int HIGH_ORDER(int param) 
		{
			return (param >> 16);
		}

		internal static int INDEXTOOVERLAYMASK(int index) 
		{
			return (int)((uint)index << 8); 
		}

		internal static int OVERLAYMASKTOINDEX(int index) 
		{
			return (int)((uint)index >> 8);
		}

		internal static int INDEXTOSTATEIMAGEMASK(int i) 
		{
			return (int)((uint)i << 12);
		}

		internal static int STATEIMAGEMASKTOINDEX(int i) 
		{
			return (int)((uint)i >> 12);
		}

		internal static short HRESULT_CODE(int hr) 
		{
			return (short)(hr & 0xFFFF);
		}

		internal static bool SUCCEEDED(int status) 
		{
			return (status >= 0);
		}

		internal static bool FAILED(int status) 
		{
			return (status < 0);
		}

		internal static int  MAKEINTRESOURCE(int res) 
		{
			return 0x0000FFFF & res;
		}
		
		internal static int MAKELONG(int lo, int hi)
		{
			return (hi << 16) | (lo & 0x0000ffff);
		}
		#endregion

		#region Mono win32 Fuinctions

		internal delegate IntPtr WndProc (IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);

		
		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Ansi, EntryPoint = "RegisterClassA")]
		internal static extern uint RegisterClass(ref WNDCLASS wndClass);
		
		#region Added by Dennis hayes 10-20-2002
		//correct?
		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Auto,EntryPoint="SendMessageA")]
		internal static extern uint SendMessage(
			IntPtr hWnd, uint Msg,
			IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal static extern bool GetWindowPlacement(
			IntPtr hWnd,
			ref  WINDOWPLACEMENT  lpwndpl  // position data
			);
		#endregion

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Ansi, EntryPoint = "CreateWindowExA")]
		internal static extern IntPtr CreateWindowEx (
			uint dwExStyle, string lpClassName, 
			string lpWindowName, uint dwStyle, 
			int x, int y, int nWidth, int nHeight,
			IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance,
			[ MarshalAs( UnmanagedType.AsAny )]
			object lpParam);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr CreateMenu ();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern bool AppendMenuA(IntPtr hMenu, uint uflags, IntPtr NewItem, string item);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall)]
		internal static extern bool DestroyMenu (IntPtr hMenu);
		
		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal extern static IntPtr DefWindowProcA (
			IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal extern static int DestroyWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int ShowWindow (IntPtr hWnd, 
			uint nCmdShow);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,CharSet = CharSet.Auto)]
		internal static extern int GetMessageA (ref MSG msg, int hwnd, 
			int msgFrom,  int msgTo);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int  TranslateMessage (ref MSG msg);

		[DllImport ("user32.dll", CallingConvention =
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int DispatchMessageA (ref MSG msg);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int PeekMessageA (
			ref MSG msg, IntPtr hWnd, uint wMsgFilterMin, 
			uint wMsgFilterMax, uint wRemoveMsg);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static void PostQuitMessage (int nExitCode);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static IntPtr SetActiveWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention =
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static int CloseWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention =
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static int SetWindowPos (
			IntPtr hWnd, SetWindowPosZOrder pos,
			int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern int MessageBoxA (
			IntPtr hWnd, string pText, string pCaption, uint uType);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr SetParent (
			IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetParent (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetWindow (IntPtr hWnd, uint uCmd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Ansi)]
		internal static extern bool SetWindowTextA (
			IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool UpdateWindow (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetDC (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int ReleaseDC (IntPtr hWnd, IntPtr hDC);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetFocus();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr SetFocus (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool IsWindowEnabled (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool IsMenu (IntPtr hWnd);


		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool EnableWindow (
			IntPtr hWnd, bool bEnable);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool IsDialogMessage (
			IntPtr hWnd, ref MSG mes);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool GetWindowRect (
			IntPtr hWnd, ref RECT lpRect);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool GetClientRect (
			IntPtr hWnd, ref RECT lpRect);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool InvalidateRect (
			IntPtr hWnd, ref RECT lpRect, bool bErase); 

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetCapture ();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr SetCapture (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool ReleaseCapture ();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int GetWindowTextA (
			IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int GetWindowTextLengthA (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool GetCursorPos (ref POINT lpPoint);

		[DllImport ("Comdlg32.dll",
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern bool GetOpenFileName ( ref OPENFILENAME lpofn );

		#endregion

		internal static void UpdateWindowStyle( IntPtr hwnd, int RemoveStyle, int AddStyle) {
			if( Win32.IsWindow(hwnd)) {
				int style = Win32.GetWindowLong(hwnd, GetWindowLongFlag.GWL_STYLE).ToInt32();
				style &= ~RemoveStyle;
				style |= AddStyle;
				Win32.SetWindowLong(hwnd, GetWindowLongFlag.GWL_STYLE, style);
				Win32.SetWindowPos(hwnd, 0, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE |
					SetWindowPosFlags.SWP_NOZORDER | SetWindowPosFlags.SWP_NOSIZE |
					SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_FRAMECHANGED);
			}
		}
		
		internal static ButtonStyles ContentAlignment2SystemButtonStyle( ContentAlignment contentAlign) {
			ButtonStyles sysButtonStyle = 0;
			
			if( contentAlign == ContentAlignment.BottomCenter ||
				contentAlign == ContentAlignment.BottomLeft ||
				contentAlign == ContentAlignment.BottomRight) {
				sysButtonStyle |= ButtonStyles.BS_BOTTOM;
			}
			else if(contentAlign == ContentAlignment.TopCenter ||
				contentAlign == ContentAlignment.TopLeft ||
				contentAlign == ContentAlignment.TopRight) {
				sysButtonStyle |= ButtonStyles.BS_TOP;
			}
			else {
				sysButtonStyle |= ButtonStyles.BS_VCENTER;
			}

			if( contentAlign == ContentAlignment.BottomLeft ||
				contentAlign == ContentAlignment.MiddleLeft ||
				contentAlign == ContentAlignment.TopLeft) {
				sysButtonStyle |= ButtonStyles.BS_LEFT;
			}
			else if(contentAlign == ContentAlignment.BottomRight ||
				contentAlign == ContentAlignment.MiddleRight ||
				contentAlign == ContentAlignment.TopRight) {
				sysButtonStyle |= ButtonStyles.BS_RIGHT;
			}
			else {
				sysButtonStyle |= ButtonStyles.BS_CENTER;
			}
			return sysButtonStyle;
		}

		internal static StringFormat ContentAlignment2StringFormat( ContentAlignment contentAlign) {
			StringAlignment	alignment = 0;
			StringAlignment	lineAlignment = 0;
			
			if( contentAlign == ContentAlignment.BottomCenter ||
				contentAlign == ContentAlignment.BottomLeft ||
				contentAlign == ContentAlignment.BottomRight) {
				lineAlignment = StringAlignment.Far;
			}
			else if(contentAlign == ContentAlignment.TopCenter ||
				contentAlign == ContentAlignment.TopLeft ||
				contentAlign == ContentAlignment.TopRight) {
				lineAlignment = StringAlignment.Near;
			}
			else {
				lineAlignment = StringAlignment.Center;
			}

			if( contentAlign == ContentAlignment.BottomLeft ||
				contentAlign == ContentAlignment.MiddleLeft ||
				contentAlign == ContentAlignment.TopLeft) {
				alignment = StringAlignment.Near;
			}
			else if(contentAlign == ContentAlignment.BottomRight ||
				contentAlign == ContentAlignment.MiddleRight ||
				contentAlign == ContentAlignment.TopRight) {
				alignment = StringAlignment.Far;
			}
			else {
				alignment = StringAlignment.Center;
			}
			StringFormat result = new StringFormat();
			result.Alignment = alignment;
			result.LineAlignment = lineAlignment;
			return result;
		}
		
		internal static DrawTextFormatFlags ContentAlignment2DrawTextFormat( ContentAlignment contentAlign) {
			DrawTextFormatFlags format = 0;
			
			if( contentAlign == ContentAlignment.BottomCenter ||
				contentAlign == ContentAlignment.BottomLeft ||
				contentAlign == ContentAlignment.BottomRight) {
				format |= DrawTextFormatFlags.DT_BOTTOM;
			}
			else if(contentAlign == ContentAlignment.TopCenter ||
				contentAlign == ContentAlignment.TopLeft ||
				contentAlign == ContentAlignment.TopRight) {
				format |= DrawTextFormatFlags.DT_TOP;
			}
			else {
				format |= DrawTextFormatFlags.DT_VCENTER;
			}

			if( contentAlign == ContentAlignment.BottomLeft ||
				contentAlign == ContentAlignment.MiddleLeft ||
				contentAlign == ContentAlignment.TopLeft) {
				format |= DrawTextFormatFlags.DT_LEFT;
			}
			else if(contentAlign == ContentAlignment.BottomRight ||
				contentAlign == ContentAlignment.MiddleRight ||
				contentAlign == ContentAlignment.TopRight) {
				format |= DrawTextFormatFlags.DT_RIGHT;
			}
			else {
				format |= DrawTextFormatFlags.DT_CENTER;
			}
			return format;
		}
		
		internal static void DrawText(Graphics paintOn, string text, Font font, Color color, Rectangle rect, ContentAlignment alignment) {

			RECT rc = new RECT();
			rc.left = rect.Left;
			rc.top = rect.Top;
			rc.right = rect.Right;
			rc.bottom = rect.Bottom;
			
			IntPtr hdc = paintOn.GetHdc();
			int prevColor = Win32.SetTextColor(hdc, RGB(color));
			IntPtr prevFont = Win32.SelectObject(hdc, font.ToHfont());
			BackgroundMode prevBkMode = Win32.SetBkMode(hdc, BackgroundMode.TRANSPARENT);
			Win32.DrawText(hdc, text, text.Length, ref rc, 
			       DrawTextFormatFlags.DT_SINGLELINE | Win32.ContentAlignment2DrawTextFormat(alignment));
			Win32.SetBkMode(hdc, prevBkMode);
			Win32.SelectObject(hdc, prevFont);
			Win32.SetTextColor(hdc, prevColor);
			paintOn.ReleaseHdc(hdc);
		}

		internal static SIZE GetTextExtent ( IntPtr hWnd, string text ) {
			IntPtr hOldFont = new IntPtr ( 0 );
			IntPtr hFont = new IntPtr ( Win32.SendMessage ( hWnd, (int)Msg.WM_GETFONT, 0, 0 ) );
			IntPtr hDC   = Win32.GetWindowDC ( hWnd );
			if ( hFont != IntPtr.Zero )
				hOldFont = Win32.SelectObject ( hDC, hFont );
			SIZE size = new SIZE();
			Win32.GetTextExtentPoint32 ( hDC, text, text.Length, ref size);
			if ( hOldFont != IntPtr.Zero )
				Win32.SelectObject ( hDC, hOldFont );
			Win32.ReleaseDC ( hWnd, hDC );
			return size;
		}
	}
}
