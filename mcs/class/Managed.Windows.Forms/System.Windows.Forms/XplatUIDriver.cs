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
// $Revision: 1.25 $
// $Modtime: $
// $Log: XplatUIDriver.cs,v $
// Revision 1.25  2004/10/18 04:50:54  pbartok
// - Added method for enabling/disabling windows
// - Added method for setting window modality
// - Added method for setting topmost window
//
// Revision 1.24  2004/10/02 19:07:36  pbartok
// - Added ClientToScreen coordinate translation method
//
// Revision 1.23  2004/09/21 00:54:15  jackson
// New message loop that uses poll so we don't get a busy loop
//
// Revision 1.22  2004/09/13 21:18:32  pbartok
// - Added Z-Ordering methods
//
// Revision 1.21  2004/09/11 00:57:35  pbartok
// - Added method to retrieve text from window
//
// Revision 1.20  2004/08/24 11:29:44  jackson
// Move timers to the driver level. On X they are queued by the driver and checked on idle.
//
// Revision 1.19  2004/08/23 19:39:30  pbartok
// - Added method to move mouse cursor
//
// Revision 1.18  2004/08/21 20:51:27  pbartok
// - Added method to get default display size
//
// Revision 1.17  2004/08/21 20:23:56  pbartok
// - Added method to query current grab state
// - Added argument to allow confining a grab to a window
//
// Revision 1.16  2004/08/20 20:03:20  pbartok
// - Added method for setting the window background
//
// Revision 1.15  2004/08/20 19:19:28  jackson
// Expose functionality to send async messages through the driver
//
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

                internal abstract event EventHandler Idle;

		#region XplatUI Driver Methods
		internal abstract void Exit();

		internal abstract void EnableThemes();

		internal abstract void GetDisplaySize(out Size size);

		internal abstract IntPtr CreateWindow(CreateParams cp);
		internal abstract IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height);
		internal abstract void DestroyWindow(IntPtr handle);

		internal abstract bool GetText(IntPtr handle, out string text);
		internal abstract bool Text(IntPtr handle, string text);
		internal abstract bool SetVisible(IntPtr handle, bool visible);
		internal abstract bool IsVisible(IntPtr handle);
		internal abstract IntPtr SetParent(IntPtr handle, IntPtr parent);
		internal abstract IntPtr GetParent(IntPtr handle);

		internal abstract void RefreshWindow(IntPtr handle);
		internal abstract void SetWindowBackground(IntPtr handle, Color color);
		internal abstract PaintEventArgs PaintEventStart(IntPtr handle);
		internal abstract void PaintEventEnd(IntPtr handle);

		internal abstract void SetWindowPos(IntPtr handle, int x, int y, int width, int height);
		internal abstract void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height);
		internal abstract void Activate(IntPtr handle);
		internal abstract void EnableWindow(IntPtr handle, bool Enable);
		internal abstract void SetModal(IntPtr handle, bool Modal);
		internal abstract void Invalidate(IntPtr handle, Rectangle rc, bool clear);
		internal abstract IntPtr DefWndProc(ref Message msg);
		internal abstract void HandleException(Exception e);
		internal abstract void DoEvents();
		internal abstract bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);
		internal abstract bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);
		internal abstract bool TranslateMessage(ref MSG msg);
		internal abstract bool DispatchMessage(ref MSG msg);

		internal abstract bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom);
		internal abstract bool SetTopmost(IntPtr hWnd, bool Enabled);

		internal abstract bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect);

		internal abstract void GetCursorPos(IntPtr handle, out int x, out int y);
		internal abstract void SetCursorPos(IntPtr handle, int x, int y);
		internal abstract void ScreenToClient(IntPtr handle, ref int x, ref int y);
		internal abstract void ClientToScreen(IntPtr handle, ref int x, ref int y);

		internal abstract void GrabWindow(IntPtr hWnd, IntPtr ConfineToHwnd);
		internal abstract void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea);
		internal abstract void ReleaseWindow(IntPtr hWnd);

		internal abstract void SendAsyncMethod (AsyncMethodData method);
		internal abstract void SetTimer (Timer timer);
		internal abstract void KillTimer (Timer timer);


#endregion	// XplatUI Driver Methods
	}
}
