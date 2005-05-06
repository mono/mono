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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com

// NOT COMPLETE

// define to log API calls to stdout
#undef DriverDebug

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUI {
		#region Local Variables
		static XplatUIDriver		driver;
		static String			default_class_name;
		#endregion	// Local Variables

		#region Subclasses
		public class State {
			static public bool DropTarget {
				get {
					return driver.DropTarget;
				}

				set {
					driver.DropTarget=value;
				}
			}

			static public Keys ModifierKeys {
				get {
					return driver.ModifierKeys;
				}
			}

			static public MouseButtons MouseButtons {
				get {
					return driver.MouseButtons;
				}
			}

			static public Point MousePosition {
				get {
					return driver.MousePosition;
				}
			}

		}
		#endregion	// Subclasses

		#region Constructor & Destructor
		static XplatUI() {
			Console.WriteLine("Mono System.Windows.Forms Assembly [Revision: 44101; built: 2005/5/5 21:38:0]");

			// Don't forget to throw the mac in here somewhere, too
			default_class_name="SWFClass";

			if (Environment.OSVersion.Platform == (PlatformID)128) {
				if (Environment.GetEnvironmentVariable ("MONO_MWF_USE_QUARTZ_BACKEND") != null)
					driver=XplatUIOSX.GetInstance();
				else
					driver=XplatUIX11.GetInstance();
			} else {
				driver=XplatUIWin32.GetInstance();
			}

		}

		~XplatUI() {
			Console.WriteLine("XplatUI Destructor called");
		}
		#endregion	// Constructor & Destructor

		#region Public Static Properties
		internal static string DefaultClassName {
			get {
				return default_class_name;
			}

			set {
				default_class_name=value;
			}
		}

		static public Size CursorSize {
			get {
				return driver.CursorSize;
			}
		}

		static public bool DragFullWindows {
			get {
				return driver.DragFullWindows;
			}
		}

		static public Size DragSize {
			get {
				return driver.DragSize;
			}
		}

		static public Size IconSize {
			get {
				return driver.IconSize;
			}
		}

		static public int KeyboardSpeed {
			get {
				return driver.KeyboardSpeed;
			}
		}

		static public int KeyboardDelay {
			get {
				return driver.KeyboardSpeed;
			}
		}

		static public Size MaxWindowTrackSize {
			get {
				return driver.MaxWindowTrackSize;
			}
		}

		static public Size MinimizedWindowSize {
			get {
				return driver.MinimizedWindowSize;
			}
		}

		static public Size MinimizedWindowSpacingSize {
			get {
				return driver.MinimizedWindowSpacingSize;
			}
		}

		static public Size MinimumWindowSize {
			get {
				return driver.MinimumWindowSize;
			}
		}

		static public Size MinWindowTrackSize {
			get {
				return driver.MinWindowTrackSize;
			}
		}

		static public Size SmallIconSize {
			get {
				return driver.SmallIconSize;
			}
		}

		static public int MouseButtonCount {
			get {
				return driver.MouseButtonCount;
			}
		}

		static public bool MouseButtonsSwapped {
			get {
				return driver.MouseButtonsSwapped;
			}
		}

		static public bool MouseWheelPresent {
			get {
				return driver.MouseWheelPresent;
			}
		}

		static public Rectangle VirtualScreen {
			get {
				return driver.VirtualScreen;
			}
		}

		static public Rectangle WorkingArea {
			get {
				return driver.WorkingArea;
			}
		}
		#endregion	// Public Static Properties

		#region Events
                internal static event EventHandler Idle {
                        add {
                                driver.Idle += value;
                        }
                        remove {
                                driver.Idle -= value;
                        }
                }
                
		#endregion	// Events

		#region Public Static Methods
		internal static void Activate(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("Activate({0:X}): Called", handle.ToInt32());
			#endif
			driver.Activate(handle);
		}

		internal static bool CalculateWindowRect(IntPtr handle, ref Rectangle ClientRect, int Style, int ExStyle, IntPtr MenuHandle, out Rectangle WindowRect) {
			#if DriverDebug
				Console.WriteLine("CalculateWindowRect({0:X}): Called", handle.ToInt32());
			#endif
			return driver.CalculateWindowRect(handle, ref ClientRect, Style, ExStyle, MenuHandle, out WindowRect);
		}

		internal static void CaretVisible(IntPtr handle, bool visible) {
			#if DriverDebug
				Console.WriteLine("CaretVisible({0:X}, {1}): Called", handle.ToInt32(), visible);
			#endif
			driver.CaretVisible(handle, visible);
		}

		internal static void CreateCaret(IntPtr handle, int width, int height) {
			#if DriverDebug
				Console.WriteLine("CreateCaret({0:X}), {1}, {2}: Called", handle.ToInt32(), width, height);
			#endif
			driver.CreateCaret(handle, width, height);
		}

		internal static IntPtr CreateWindow(CreateParams cp) {
			#if DriverDebug
				Console.WriteLine("CreateWindow(): Called");
			#endif
			return driver.CreateWindow(cp);
		}

		internal static IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			#if DriverDebug
				Console.WriteLine("CreateWindow(): Called");
			#endif
			return driver.CreateWindow(Parent, X, Y, Width, Height);
		}

		internal static void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			#if DriverDebug
				Console.WriteLine("ClientToScreen({0:X}, {1}, {2}): Called", handle.ToInt32(), x, y);
			#endif
			driver.ClientToScreen(handle, ref x, ref y);
		}

		internal static IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			#if DriverDebug
				Console.WriteLine("DefineCursor(...): Called");
			#endif
			return driver.DefineCursor(bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);
		}

		internal static IntPtr DefineStdCursor(StdCursor id) {
			return driver.DefineStdCursor(id);
		}

		internal static IntPtr DefWndProc(ref Message msg) {
			return driver.DefWndProc(ref msg);
		}

		internal static void DestroyCaret(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("DestroyCaret({0:X}): Called", handle.ToInt32());
			#endif
			driver.DestroyCaret(handle);
		}

		internal static void DestroyCursor(IntPtr cursor) {
			#if DriverDebug
				Console.WriteLine("DestroyCursor({0:X}): Called", cursor.ToInt32());
			#endif
			driver.DestroyCursor(cursor);
		}

		internal static void DestroyWindow(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("DestroyWindow({0:X}): Called", handle.ToInt32());
			#endif
			driver.DestroyWindow(handle);
		}

		internal static IntPtr DispatchMessage(ref MSG msg) {
			return driver.DispatchMessage(ref msg);
		}

		internal static void DoEvents() {
			driver.DoEvents();
		}

		internal static void EnableThemes() {
			driver.EnableThemes();
		}

		internal static void EnableWindow(IntPtr handle, bool Enable) {
			#if DriverDebug
				Console.WriteLine("EnableWindow({0:X}, {1}): Called", handle.ToInt32(), Enable);
			#endif
			driver.EnableWindow(handle, Enable);
		}

		internal static void Exit() {
			driver.Exit();
		}

		internal static void EraseWindowBackground(IntPtr handle, IntPtr wParam) {
			driver.EraseWindowBackground(handle, wParam);
		}

		internal static IntPtr GetActive() {
			#if DriverDebug
				Console.WriteLine("GetActive(): Called");
			#endif
			return driver.GetActive();
		}

		internal static void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			#if DriverDebug
				Console.WriteLine("GetCursorInfo({0:X}): Called", cursor.ToInt32());
			#endif
			driver.GetCursorInfo(cursor, out width, out height, out hotspot_x, out hotspot_y);
		}

		internal static void GetCursorPos(IntPtr handle, out int x, out int y) {
			#if DriverDebug
				Console.WriteLine("GetCursorPos({0:X}): Called", handle.ToInt32());
			#endif
			driver.GetCursorPos(handle, out x, out y);
		}

		internal static void GetDisplaySize(out Size size) {
			#if DriverDebug
				Console.WriteLine("GetDisplaySize(): Called");
			#endif
			driver.GetDisplaySize(out size);
		}

		internal static bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			#if DriverDebug
				Console.WriteLine("GetFontMetrics(): Called");
			#endif
			return driver.GetFontMetrics(g, font, out ascent, out descent);
		}
			
		internal static Graphics GetMenuDC(IntPtr handle, IntPtr ncpaint_region) {
			#if DriverDebug
				Console.WriteLine("GetMenuDC({0:X}): Called", handle.ToInt32());
			#endif
			return driver.GetMenuDC(handle, ncpaint_region);
		}

		internal static Point GetMenuOrigin(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("GetMenuOrigin({0:X}): Called", handle.ToInt32());
			#endif
			return driver.GetMenuOrigin(handle);
		}

		internal static bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			return driver.GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
		}

		internal static IntPtr GetParent(IntPtr hWnd) {
			#if DriverDebug
				Console.WriteLine("GetParent({0:X}): Called", hWnd.ToInt32());
			#endif
			return driver.GetParent(hWnd);
		}

		internal static bool GetText(IntPtr hWnd, out string text) {
			#if DriverDebug
				Console.WriteLine("GetText(): Called");
			#endif
			return driver.GetText(hWnd, out text);
		}

		internal static void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			#if DriverDebug
				Console.WriteLine("GetWindowPos({0:X}): Called", handle.ToInt32());
			#endif
			driver.GetWindowPos(handle, is_toplevel, out x, out y, out width, out height, out client_width, out client_height);
		}

		internal static FormWindowState GetWindowState(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("GetWindowState({0:X}): Called", handle.ToInt32());
			#endif
			return driver.GetWindowState(handle);
		}

		internal static void GrabInfo(out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea) {
			#if DriverDebug
				Console.WriteLine("GrabInfo(): Called");
			#endif
			driver.GrabInfo(out handle, out GrabConfined, out GrabArea);
		}

		internal static void GrabWindow(IntPtr handle, IntPtr ConfineToHwnd) {
			#if DriverDebug
				Console.WriteLine("GrabWindow({0:X}): Called", handle.ToInt32());
			#endif
			driver.GrabWindow(handle, ConfineToHwnd);
		}

		internal static void HandleException(Exception e) {
			driver.HandleException(e);
		}

		internal static void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			#if DriverDebug
				Console.WriteLine("Invalidate({0:X}, {1}, {2}): Called", handle.ToInt32(), rc, clear);
			#endif
			driver.Invalidate(handle, rc, clear);
		}

		internal static bool IsVisible(IntPtr hWnd) {
			#if DriverDebug
				Console.WriteLine("IsVisible({0:X}): Called", hWnd.ToInt32());
			#endif
			return driver.IsVisible(hWnd);
		}

		internal static void KillTimer (Timer timer)
		{
			#if DriverDebug
				Console.WriteLine("KillTimer({0}): Called", timer);
			#endif
			driver.KillTimer (timer);
		}

		internal static void MenuToScreen(IntPtr handle, ref int x, ref int y) {
			#if DriverDebug
				Console.WriteLine("MenuToScreen({0:X}, {1}, {2}): Called", handle.ToInt32(), x, y);
			#endif
			driver.MenuToScreen(handle, ref x, ref y);
		}

		internal static void OverrideCursor(IntPtr cursor) {
			#if DriverDebug
				Console.WriteLine("OverrideCursor({0:X}): Called", cursor.ToInt32());
			#endif
			driver.OverrideCursor(cursor);
		}

		internal static void PaintEventEnd(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("PaintEventEnd({0:X}): Called", handle.ToInt32());
			#endif
			driver.PaintEventEnd(handle);
		}

		internal static PaintEventArgs PaintEventStart(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("PaintEventStart({0:X}): Called", handle.ToInt32());
			#endif
			return driver.PaintEventStart(handle);
		}

		internal static bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return driver.PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal static void ReleaseMenuDC(IntPtr handle, Graphics dc) {
			#if DriverDebug
				Console.WriteLine("ReleaseMenuDC({0:X}): Called", handle.ToInt32());
			#endif
			driver.ReleaseMenuDC(handle, dc);
		}

		internal static void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			#if DriverDebug
				Console.WriteLine("ScreenToClient({0:X}, {1}, {2}): Called", handle.ToInt32(), x, y);
			#endif
			driver.ScreenToClient (handle, ref x, ref y);
		}

		internal static void ScreenToMenu(IntPtr handle, ref int x, ref int y) {
			#if DriverDebug
				Console.WriteLine("ScreenToMenu({0:X}, {1}, {2}): Called", handle.ToInt32(), x, y);
			#endif
			driver.ScreenToMenu(handle, ref x, ref y);
		}

		internal static void ScrollWindow(IntPtr handle, Rectangle rectangle, int XAmount, int YAmount, bool with_children) {
			#if DriverDebug
				Console.WriteLine("ScrollWindow({0:X}, {1}, {2}, {3}, {4}): Called", handle.ToInt32(), rectangle, XAmount, YAmount, with_children);
			#endif
			driver.ScrollWindow(handle, rectangle, XAmount, YAmount, with_children);
		}

		internal static void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children) {
			#if DriverDebug
				Console.WriteLine("ScrollWindow({0:X}, {2}, {3}, {4}): Called", handle.ToInt32(), XAmount, YAmount, with_children);
			#endif
			driver.ScrollWindow(handle, XAmount, YAmount, with_children);
		}

		internal static void SendAsyncMethod (AsyncMethodData data) {
			#if DriverDebug
				Console.WriteLine("SendAsyncMethod({0}): Called", data);
			#endif
			driver.SendAsyncMethod (data);
		}

		internal static void SetBorderStyle(IntPtr handle, BorderStyle border_style) {
			#if DriverDebug
				Console.WriteLine("SetBorderStyle({0:X}, {1}): Called", handle.ToInt32(), border_style);
			#endif
			driver.SetBorderStyle(handle, border_style);
		}

		internal static void SetCaretPos(IntPtr handle, int x, int y) {
			#if DriverDebug
				Console.WriteLine("SetCaretPos({0:X}, {1}, {2}): Called", handle.ToInt32(), x, y);
			#endif
			driver.SetCaretPos(handle, x, y);
		}

		internal static void SetCursor(IntPtr handle, IntPtr cursor) {
			#if DriverDebug
				Console.WriteLine("SetCursor({0:X}, {1:X}): Called", handle.ToInt32(), cursor.ToInt32());
			#endif
			driver.SetCursor(handle, cursor);
		}

		internal static void SetCursorPos(IntPtr handle, int x, int y) {
			#if DriverDebug
				Console.WriteLine("SetCursorPos({0:X}, {1}, {2}): Called", handle.ToInt32(), x, y);
			#endif
			driver.SetCursorPos(handle, x, y);
		}

		internal static void SetFocus(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("SetFocus({0:X}): Called", handle.ToInt32());
			#endif
			driver.SetFocus(handle);
		}

		internal static void SetIcon(IntPtr handle, Icon icon) {
			#if DriverDebug
				Console.WriteLine("SetIcon({0:X}, {1}): Called", handle.ToInt32(), icon);
			#endif
			driver.SetIcon(handle, icon);
		}

		internal static void SetMenu(IntPtr handle, IntPtr menu_handle) {
			#if DriverDebug
				Console.WriteLine("SetMenu({0:X}, {1:X}): Called", handle.ToInt32(), menu_handle.ToInt32());
			#endif
			driver.SetMenu(handle, menu_handle);
		}

		internal static void SetModal(IntPtr handle, bool Modal) {
			#if DriverDebug
				Console.WriteLine("SetModal({0:X}, {1}): Called", handle.ToInt32(), Modal);
			#endif
			driver.SetModal(handle, Modal);
		}

		internal static IntPtr SetParent(IntPtr hWnd, IntPtr hParent) {
			#if DriverDebug
				Console.WriteLine("SetParent({0:X}, {1:X}): Called", hWnd.ToInt32(), hParent.ToInt32());
			#endif
			return driver.SetParent(hWnd, hParent);
		}

		internal static void SetTimer (Timer timer)
		{
			#if DriverDebug
				Console.WriteLine("SetTimer({0}): Called", timer);
			#endif
			driver.SetTimer (timer);
		}

		internal static bool SetTopmost(IntPtr handle, IntPtr hWndOwner, bool Enabled) {
			#if DriverDebug
				Console.WriteLine("SetTopMost({0:X}, {1:X}, {2}): Called", handle.ToInt32(), hWndOwner.ToInt32(), Enabled);
			#endif
			return driver.SetTopmost(handle, hWndOwner, Enabled);
		}

		internal static bool SetVisible(IntPtr hWnd, bool visible) {
			#if DriverDebug
				Console.WriteLine("SetVisible({0:X}, {1}): Called", hWnd.ToInt32(), visible);
			#endif
			return driver.SetVisible(hWnd, visible);
		}

		internal static void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			#if DriverDebug
				Console.WriteLine("SetWindowPos({0:X}, {1}, {2}, {3}, {4}): Called", handle.ToInt32(), x, y, width, height);
			#endif
			driver.SetWindowPos(handle, x, y, width, height);
		}

		internal static void SetWindowState(IntPtr handle, FormWindowState state) {
			#if DriverDebug
				Console.WriteLine("SetWindowState({0:X} {1}): Called", handle.ToInt32(), state);
			#endif
			driver.SetWindowState(handle, state);
		}

		internal static void SetWindowStyle(IntPtr handle, CreateParams cp) {
			#if DriverDebug
				Console.WriteLine("SetWindowStyle({0:X}): Called", handle.ToInt32());
			#endif
			driver.SetWindowStyle(handle, cp);
		}

		internal static void SetWindowBackground(IntPtr handle, Color color) {
			#if DriverDebug
				Console.WriteLine("SetWindowBackground({0:X}, {1}): Called", handle.ToInt32(), color);
			#endif
			driver.SetWindowBackground(handle, color);
		}
			
		internal static bool SetZOrder(IntPtr handle, IntPtr AfterhWnd, bool Top, bool Bottom) {
			#if DriverDebug
				Console.WriteLine("SetZOrder({0:X}, {1:X}, {2}, {3}): Called", handle.ToInt32(), AfterhWnd.ToInt32(), Top, Bottom);
			#endif
			return driver.SetZOrder(handle, AfterhWnd, Top, Bottom);
		}

		internal static void ShowCursor(bool show) {
			#if DriverDebug
				Console.WriteLine("ShowCursor({0}): Called", show);
			#endif
			driver.ShowCursor(show);
		}

		internal static bool SystrayAdd(IntPtr handle, string tip, Icon icon, out ToolTip tt) {
			#if DriverDebug
				Console.WriteLine("SystrayAdd({0:X}, {1}): Called", handle.ToInt32(), tip);
			#endif
			return driver.SystrayAdd(handle, tip, icon, out tt);
		}

		internal static void SystrayChange(IntPtr handle, string tip, Icon icon, ref ToolTip tt) {
			#if DriverDebug
				Console.WriteLine("SystrayChange({0:X}, {1}): Called", handle.ToInt32(), tip);
			#endif
			driver.SystrayChange(handle, tip, icon, ref tt);
		}

		internal static void SystrayRemove(IntPtr handle, ref ToolTip tt) {
			#if DriverDebug
				Console.WriteLine("SystrayRemove({0:X}): Called", handle.ToInt32());
			#endif
			driver.SystrayRemove(handle, ref tt);
		}

		internal static bool Text(IntPtr hWnd, string text) {
			#if DriverDebug
				Console.WriteLine("Text({0:X}, {1}): Called", hWnd.ToInt32(), text);
			#endif
			return driver.Text(hWnd, text);
		}

		internal static bool TranslateMessage(ref MSG msg) {
			return driver.TranslateMessage(ref msg);
		}

		internal static void UngrabWindow(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("UngrabWindow({0:X}): Called", handle.ToInt32());
			#endif
			driver.UngrabWindow(handle);
		}

		internal static void UpdateWindow(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("UpdateWindow({0:X}): Called", handle.ToInt32());
			#endif
			driver.UpdateWindow(handle);
		}

		// Santa's little helper
		internal static void Version() {
			Console.WriteLine("Xplat version $Revision: $");
		}

		internal static void Where() {
			XplatUIX11.Where();
		}
		#endregion	// Public Static Methods

	}
}
