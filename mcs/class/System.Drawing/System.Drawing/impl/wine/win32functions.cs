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
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing.Imaging;

namespace System.Drawing.Win32Impl {
	/// <summary>
	/// Windows API Functions
	/// </summary>
	public class Win32 {
		#region Constructors
		// No need to construct this object
		#endregion
		
		#region Gdi32.dll functions
		[DllImport("gdi32.dll")]
		static internal extern bool StretchBlt(IntPtr hDCDest, int XOriginDest, int YOriginDest, int WidthDest, int HeightDest,
			IntPtr hDCSrc,  int XOriginScr, int YOriginSrc, int WidthScr, int HeightScr, PatBltTypes Rop);
		[DllImport("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi,EntryPoint="CreateDCA")]
		static internal extern IntPtr CreateDC( string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData );
		[DllImport("gdi32.dll")]
		static internal extern IntPtr CreateCompatibleDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int Width, int Heigth);
		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO_FLAT bmi, 
			DibUsage iUsage, out IntPtr ppvBits, IntPtr hSection, int dwOffset);
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
		//[DllImport("gdi32.dll")]
		//internal static extern int GetDIBits(IntPtr hDC, IntPtr hbm, int StartScan, int ScanLines, int lpBits, BITMAPINFOHEADER bmi, int usage);
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
		[DllImport("gdi32.dll",CharSet = CharSet.Ansi,EntryPoint="ExtTextOutA")]
		internal static extern int ExtTextOut(IntPtr hdc, int x, int y,
			ExtTextOutFlags options, IntPtr rc, string str, int strLen, IntPtr distances);

		[DllImport ("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern uint GetBkColor (IntPtr hdc);

		[DllImport ("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern uint SetBkColor (IntPtr hdc, uint crColor);

		internal static int RGB(Color color) {
			return color.R | (color.G << 8) | (color.B << 16);
		}
		
		[DllImport ("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi,EntryPoint="GetTextExtentPoint32A")]
		internal static extern int GetTextExtentPoint32(IntPtr hdc, string lpString, int cbString, ref SIZE lpSize);

		[DllImport ("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern int Polyline(IntPtr hdc, POINT[] lppt, int cPoints);

		[DllImport ("gdi32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern int GdiFlush();
		
		#endregion
		
		#region User32.dll functions
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int FillRect(IntPtr hDC, ref RECT rect, IntPtr hBrush);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetDC(IntPtr hwnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
		
		#endregion
		
		#region Kernel32.dll functions
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
	}

}
