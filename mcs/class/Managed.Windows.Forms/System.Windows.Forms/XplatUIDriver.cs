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

		internal abstract Size CursorSize { get; }
		internal abstract bool DragFullWindows { get; }
		internal abstract Size DragSize { get; }
		internal abstract Size IconSize { get; }
		internal abstract Size MaxWindowTrackSize { get; }
		internal abstract Size MinimizedWindowSize { get; }
		internal abstract Size MinimizedWindowSpacingSize { get; }
		internal abstract Size MinimumWindowSize { get; }
		internal abstract Size MinWindowTrackSize { get; }
		internal abstract Size SmallIconSize { get; }
		internal abstract int MouseButtonCount { get; }
		internal abstract bool MouseButtonsSwapped { get; }
		internal abstract bool MouseWheelPresent { get; }
		internal abstract Rectangle VirtualScreen { get; }
		internal abstract Rectangle WorkingArea { get; }
		#endregion	// XplatUI Driver Properties

                internal abstract event EventHandler Idle;

		#region XplatUI Driver Methods
		internal abstract void Exit();

		internal abstract void EnableThemes();

		internal abstract void GetDisplaySize(out Size size);

		internal abstract IntPtr CreateWindow(CreateParams cp);
		internal abstract IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height);
		internal abstract void DestroyWindow(IntPtr handle);

		internal abstract FormWindowState GetWindowState(IntPtr handle);
		internal abstract void SetWindowState(IntPtr handle, FormWindowState state);

		internal abstract void SetWindowStyle(IntPtr handle, CreateParams cp);
		internal abstract void SetBorderStyle(IntPtr handle, BorderStyle border_style);
		internal abstract void SetMenu(IntPtr handle, IntPtr menu_handle);

		internal abstract bool GetText(IntPtr handle, out string text);
		internal abstract bool Text(IntPtr handle, string text);
		internal abstract bool SetVisible(IntPtr handle, bool visible);
		internal abstract bool IsVisible(IntPtr handle);
		internal abstract IntPtr SetParent(IntPtr handle, IntPtr parent);
		internal abstract IntPtr GetParent(IntPtr handle);

		internal abstract void UpdateWindow(IntPtr handle);
		internal abstract void SetWindowBackground(IntPtr handle, Color color);
		internal abstract PaintEventArgs PaintEventStart(IntPtr handle);
		internal abstract void PaintEventEnd(IntPtr handle);

		internal abstract void SetWindowPos(IntPtr handle, int x, int y, int width, int height);
		internal abstract void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height);
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
		internal abstract IntPtr DispatchMessage(ref MSG msg);

		internal abstract bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom);
		internal abstract bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled);

		internal abstract bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, int ExStyle, IntPtr MenuHandle, out Rectangle WindowRect);

		internal abstract void SetCursor(IntPtr hwnd, IntPtr cursor);
		internal abstract void ShowCursor(bool show);
		internal abstract void OverrideCursor(IntPtr cursor);
		internal abstract IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot);
		internal abstract IntPtr DefineStdCursor(StdCursor id);
		internal abstract void DestroyCursor(IntPtr cursor);
		internal abstract void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y);

		internal abstract void GetCursorPos(IntPtr hwnd, out int x, out int y);
		internal abstract void SetCursorPos(IntPtr hwnd, int x, int y);

		internal abstract void ScreenToClient(IntPtr hwnd, ref int x, ref int y);
		internal abstract void ClientToScreen(IntPtr hwnd, ref int x, ref int y);

		internal abstract void GrabWindow(IntPtr hwnd, IntPtr ConfineToHwnd);
		internal abstract void GrabInfo(out IntPtr hwnd, out bool GrabConfined, out Rectangle GrabArea);
		internal abstract void UngrabWindow(IntPtr hwnd);

		internal abstract void SendAsyncMethod (AsyncMethodData method);
		internal abstract void SetTimer (Timer timer);
		internal abstract void KillTimer (Timer timer);

		internal abstract void CreateCaret(IntPtr hwnd, int width, int height);
		internal abstract void DestroyCaret(IntPtr hwnd);
		internal abstract void SetCaretPos(IntPtr hwnd, int x, int y);
		internal abstract void CaretVisible(IntPtr hwnd, bool visible);

		internal abstract void SetFocus(IntPtr hwnd);
		internal abstract IntPtr GetActive();

		internal abstract void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool with_children);
		internal abstract void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool with_children);

		internal abstract bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent);

		internal abstract bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt);
		internal abstract bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt);
		internal abstract void SystrayRemove(IntPtr hwnd, ref ToolTip tt);

		internal abstract Graphics GetMenuDC(IntPtr hwnd, IntPtr ncpaint_region);
		internal abstract Point GetMenuOrigin(IntPtr hwnd);
		internal abstract void ReleaseMenuDC(IntPtr hwnd, Graphics dc);

		internal abstract void SetIcon(IntPtr handle, Icon icon);

		// System information

		internal abstract int KeyboardSpeed { get; } 
		internal abstract int KeyboardDelay { get; } 
		
#endregion	// XplatUI Driver Methods
	}
}
