// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//
// $Revision: 1.14 $
// $Modtime: $
// $Log: XplatUIDriver.cs,v $
// Revision 1.14  2004/08/13 21:42:15  pbartok
// - Changed signature for GetCursorPos
//
// Revision 1.13  2004/08/13 19:00:15  jordi
// implements PointToClient (ScreenToClient)
//
// Revision 1.12  2004/08/13 18:53:14  pbartok
// - Changed GetWindowPos to also provide client area size
//
// Revision 1.11  2004/08/12 22:59:03  pbartok
// - Implemented method to get current mouse position
//
// Revision 1.10  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.9  2004/08/11 19:19:44  pbartok
// - We had SetWindowPos and MoveWindow to set window positions and size,
//   removed MoveWindow. We have GetWindowPos, so it made sense to keep
//   SetWindowPos as matching counterpart
// - Added some X11 sanity checking
//
// Revision 1.8  2004/08/11 18:55:46  pbartok
// - Added method to calculate difference between decorated window and raw
//   client area
//
// Revision 1.7  2004/08/10 17:39:22  pbartok
// - Added GetWindowPos method
//
// Revision 1.6  2004/08/10 14:34:15  jackson
// Use default colors from the theme
//
// Revision 1.5  2004/08/09 20:55:59  pbartok
// - Removed Run method, was only required for initial development
//
// Revision 1.4  2004/08/09 20:51:25  pbartok
// - Implemented GrabWindow/ReleaseWindow methods to allow pointer capture
//
// Revision 1.3  2004/08/04 20:11:24  pbartok
// - Added Invalidate handling
//
// Revision 1.2  2004/08/03 23:09:02  jordi
// fixes spelling mistake
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// COMPLETE

using System.Drawing;

namespace System.Windows.Forms {
	internal abstract class XplatUIDriver {
		internal abstract IntPtr	InitializeDriver();
		internal abstract void		ShutdownDriver(IntPtr token);
		internal delegate IntPtr	WndProc(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);


		#region XplatUI Driver Properties
		internal virtual Color ForeColor {
			get {
				return ThemeEngine.Current.DefaultWindowForeColor;
			}
		}

		internal virtual  Color BackColor { 
			get {
				return ThemeEngine.Current.DefaultWindowBackColor;
			}
		}

		internal virtual Font Font {
			get {
				return ThemeEngine.Current.DefaultFont;
			}
		}

		internal virtual Keys ModifierKeys {
			get {
				return Keys.None;
			}
		}

		internal virtual MouseButtons MouseButtons {
			get {
				return MouseButtons.None;
			}
		}

		internal virtual Point MousePosition {
			get {
				return Point.Empty;
			}
		}

		internal virtual bool DropTarget {
			get {
				return false;
			}

			set {
			}
		}
		#endregion	// XplatUI Driver Properties

		#region XplatUI Driver Methods
		internal abstract void Exit();

		internal abstract void EnableThemes();

		internal abstract IntPtr CreateWindow(CreateParams cp);
		internal abstract IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height);
		internal abstract void DestroyWindow(IntPtr handle);

		internal abstract bool Text(IntPtr handle, string text);
		internal abstract bool SetVisible(IntPtr handle, bool visible);
		internal abstract bool IsVisible(IntPtr handle);
		internal abstract IntPtr SetParent(IntPtr handle, IntPtr parent);
		internal abstract IntPtr GetParent(IntPtr handle);

		internal abstract void RefreshWindow(IntPtr handle);
		internal abstract PaintEventArgs PaintEventStart(IntPtr handle);
		internal abstract void PaintEventEnd(IntPtr handle);

		internal abstract void SetWindowPos(IntPtr handle, int x, int y, int width, int height);
		internal abstract void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height);
		internal abstract void Activate(IntPtr handle);
		internal abstract void Invalidate(IntPtr handle, Rectangle rc, bool clear);
		internal abstract IntPtr DefWndProc(ref Message msg);
		internal abstract void HandleException(Exception e);
		internal abstract void DoEvents();
		internal abstract bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);
		internal abstract bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);
		internal abstract bool TranslateMessage(ref MSG msg);
		internal abstract bool DispatchMessage(ref MSG msg);

		internal abstract bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect);

		internal abstract void GetCursorPos(IntPtr handle, out int x, out int y);
		internal abstract void ScreenToClient(IntPtr handle, ref int x, ref int y);

		internal abstract void GrabWindow(IntPtr hWnd);
		internal abstract void ReleaseWindow(IntPtr hWnd);
		#endregion	// XplatUI Driver Methods
	}
}
