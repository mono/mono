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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com

// NOT COMPLETE

// define to log API calls to stdout
#undef DriverDebug
#undef DriverDebugPaint
#undef DriverDebugCreate
#undef DriverDebugDestroy
#undef DriverDebugState

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Windows.Forms {
	internal class XplatUI {
		#region Local Variables
		static XplatUIDriver		driver;
		static String			default_class_name;
		#endregion	// Local Variables

		#region Private Methods
		internal static string Window(IntPtr handle) {
			return String.Format("'{0}' ({1:X})", Control.FromHandle(handle), handle.ToInt32());
		}
		#endregion	// Private Methods

		#region Subclasses
		public class State {
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
			Console.WriteLine("Mono System.Windows.Forms Assembly [Revision: 54007; built: 2005/12/6 14:35:24]");

			// Don't forget to throw the mac in here somewhere, too
			default_class_name="SWFClass";

			// check for Unix platforms - see FAQ for more details
			// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
			int platform = (int) Environment.OSVersion.Platform;
			if ((platform == 4) || (platform == 128)) {
				if (Environment.GetEnvironmentVariable ("MONO_MWF_USE_QUARTZ_BACKEND") != null)
					driver=XplatUIOSX.GetInstance();
				else
					driver=XplatUIX11.GetInstance();
			} else {
				driver=XplatUIWin32.GetInstance();
			}

			driver.InitializeDriver();

			// Initialize things that need to be done after the driver is ready
			DataFormats.GetFormat(0);
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

		static public int CaptionHeight {
			get {
				return driver.Caption;
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

		public static Size FrameBorderSize {
			get {
				return driver.FrameBorderSize;
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
				Console.WriteLine("Activate({0}): Called", Window(handle));
			#endif
			driver.Activate(handle);
		}

		internal static void AudibleAlert() {
			#if DriverDebug
				Console.WriteLine("AudibleAlert(): Called");
			#endif
			driver.AudibleAlert();
		}

		internal static bool CalculateWindowRect(IntPtr handle, ref Rectangle ClientRect, int Style, int ExStyle, Menu menu, out Rectangle WindowRect) {
			#if DriverDebug
				Console.WriteLine("CalculateWindowRect({0}): Called", Window(handle));
			#endif
			return driver.CalculateWindowRect(handle, ref ClientRect, Style, ExStyle, menu, out WindowRect);
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
			#if DriverDebug || DriverDebugCreate
				IntPtr handle;

				handle = driver.CreateWindow(cp);

				Console.WriteLine("CreateWindow(): Called, returning {0:X}", handle.ToInt32());
				return handle;
			#else
				return driver.CreateWindow(cp);
			#endif
		}

		internal static IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			#if DriverDebug || DriverDebugCreate
				Console.WriteLine("CreateWindow(): Called");
			#endif
			return driver.CreateWindow(Parent, X, Y, Width, Height);
		}

		internal static void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			#if DriverDebug
				Console.WriteLine("ClientToScreen({0}, {1}, {2}): Called", Window(handle), x, y);
			#endif
			driver.ClientToScreen(handle, ref x, ref y);
		}

		internal static int[] ClipboardAvailableFormats(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("ClipboardAvailableTypes({0:X}): Called", handle.ToInt32());
			#endif
			return driver.ClipboardAvailableFormats(handle);
		}

		internal static void ClipboardClose(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("ClipboardClose({0:X}): Called", handle.ToInt32());
			#endif
			driver.ClipboardClose(handle);
		}

		internal static int ClipboardGetID(IntPtr handle, string format) {
			#if DriverDebug
				Console.WriteLine("ClipboardGetID({0:X}, {1}): Called", handle.ToInt32(), format);
			#endif
			return driver.ClipboardGetID(handle, format);
		}

		internal static IntPtr ClipboardOpen() {
			#if DriverDebug
				Console.WriteLine("ClipboardOpen(): Called");
			#endif
			return driver.ClipboardOpen();
		}

		internal static void ClipboardStore(IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter) {
			#if DriverDebug
				Console.WriteLine("ClipboardStore({0:X}, {1}, {2}): Called", handle.ToInt32(), obj, type, converter);
			#endif
			driver.ClipboardStore(handle, obj, type, converter);
		}

		internal static object ClipboardRetrieve(IntPtr handle, int type, XplatUI.ClipboardToObject converter) {
			#if DriverDebug
				Console.WriteLine("ClipboardRetrieve({0:X}, type, {1}): Called", handle.ToInt32(), converter);
			#endif
			return driver.ClipboardRetrieve(handle, type, converter);
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
			#if DriverDebug || DriverDebugDestroy
				Console.WriteLine("DestroyWindow({0}): Called", Window(handle));
			#endif
			driver.DestroyWindow(handle);
		}

		internal static IntPtr DispatchMessage(ref MSG msg) {
			return driver.DispatchMessage(ref msg);
		}

		internal static void DoEvents() {
			driver.DoEvents();
		}

		internal static void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width) {
			#if DriverDebug
				Console.WriteLine("DrawReversibleRectangle({0}, {1}, {2}): Called", Window(handle), rect, line_width);
			#endif
			driver.DrawReversibleRectangle(handle, rect, line_width);
		}

		internal static void EnableThemes() {
			driver.EnableThemes();
		}

		internal static void EnableWindow(IntPtr handle, bool Enable) {
			#if DriverDebug || DriverDebugState
				Console.WriteLine("EnableWindow({0}, {1}): Called", Window(handle), Enable);
			#endif
			driver.EnableWindow(handle, Enable);
		}

		internal static void EndLoop(Thread thread) {
			#if DriverDebug
				Console.WriteLine("EndLoop({0:X}): Called", thread.GetHashCode());
			#endif
			driver.EndLoop(thread);
		}

		internal static IntPtr GetActive() {
			#if DriverDebug
				Console.WriteLine("GetActive(): Called");
			#endif
			return driver.GetActive();
		}

		internal static SizeF GetAutoScaleSize(Font font) {
			#if DriverDebug
				Console.WriteLine("GetAutoScaleSize({0}): Called", font);
			#endif
			return driver.GetAutoScaleSize(font);
		}

		internal static void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			#if DriverDebug
				Console.WriteLine("GetCursorInfo({0}): Called", cursor.ToInt32());
			#endif
			driver.GetCursorInfo(cursor, out width, out height, out hotspot_x, out hotspot_y);
		}

		internal static void GetCursorPos(IntPtr handle, out int x, out int y) {
			#if DriverDebug
				Console.WriteLine("GetCursorPos({0}): Called", Window(handle));
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
			
		internal static Point GetMenuOrigin(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("GetMenuOrigin({0}): Called", Window(handle));
			#endif
			return driver.GetMenuOrigin(handle);
		}

		internal static bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			return driver.GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
		}

		internal static IntPtr GetParent(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("GetParent({0}): Called", Window(handle));
			#endif
			return driver.GetParent(handle);
		}

		internal static bool GetText(IntPtr handle, out string text) {
			#if DriverDebug
				Console.WriteLine("GetText({0}): Called", Window(handle));
			#endif
			return driver.GetText(handle, out text);
		}

		internal static void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			#if DriverDebug
				Console.WriteLine("GetWindowPos({0}): Called", Window(handle));
			#endif
			driver.GetWindowPos(handle, is_toplevel, out x, out y, out width, out height, out client_width, out client_height);
		}

		internal static FormWindowState GetWindowState(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("GetWindowState({0}): Called", Window(handle));
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
				Console.WriteLine("GrabWindow({0}, {1}): Called", Window(handle), Window(ConfineToHwnd));
			#endif
			driver.GrabWindow(handle, ConfineToHwnd);
		}

		internal static void HandleException(Exception e) {
			driver.HandleException(e);
		}

		internal static void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			#if DriverDebug
				Console.WriteLine("Invalidate({0}, {1}, {2}): Called", Window(handle), rc, clear);
			#endif
			driver.Invalidate(handle, rc, clear);
		}

		internal static bool IsEnabled(IntPtr handle) {
			#if DriverDebug || DriverDebugState
				Console.WriteLine("IsEnabled({0}): Called, Result={1}", Window(handle), driver.IsEnabled(handle));
			#endif
			return driver.IsEnabled(handle);
		}

		internal static bool IsVisible(IntPtr handle) {
			#if DriverDebug || DriverDebugState
				Console.WriteLine("IsVisible({0}): Called, Result={1}", Window(handle), driver.IsVisible(handle));
			#endif
			return driver.IsVisible(handle);
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
				Console.WriteLine("MenuToScreen({0}, {1}, {2}): Called", Window(handle), x, y);
			#endif
			driver.MenuToScreen(handle, ref x, ref y);
		}

		internal static void OverrideCursor(IntPtr cursor) {
			#if DriverDebug
				Console.WriteLine("OverrideCursor({0:X}): Called", cursor.ToInt32());
			#endif
			driver.OverrideCursor(cursor);
		}

		internal static void PaintEventEnd(IntPtr handle, bool client) {
			#if DriverDebug || DriverDebugPaint
				Console.WriteLine("PaintEventEnd({0}, {1}): Called from thread {2}", Window(handle), client, Thread.CurrentThread.GetHashCode());
			#endif
			driver.PaintEventEnd(handle, client);
		}

		internal static PaintEventArgs PaintEventStart(IntPtr handle, bool client) {
			#if DriverDebug || DriverDebugPaint
				Console.WriteLine("PaintEventStart({0}, {1}): Called from thread {2}", Window(handle), client, Thread.CurrentThread.GetHashCode());
			#endif
			return driver.PaintEventStart(handle, client);
		}

		internal static bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return driver.PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal static bool PostMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			#if DriverDebug
				Console.WriteLine("PostMessage({0}, {1}, {2:X}, {3:X}): Called", Window(hwnd), message, wParam.ToInt32(), lParam.ToInt32());
			#endif
			return driver.PostMessage(hwnd, message, wParam, lParam);
		}

		internal static bool PostMessage(ref MSG msg) {
			#if DriverDebug
				Console.WriteLine("PostMessage({0}): Called", msg);
			#endif
			return driver.PostMessage(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		internal static void PostQuitMessage(int exitCode) {
			#if DriverDebug
				Console.WriteLine("PostQuitMessage({0}): Called", exitCode);
			#endif
			driver.PostQuitMessage(exitCode);
		}

		internal static void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			#if DriverDebug
				Console.WriteLine("ScreenToClient({0}, {1}, {2}): Called", Window(handle), x, y);
			#endif
			driver.ScreenToClient (handle, ref x, ref y);
		}

		internal static void ScreenToMenu(IntPtr handle, ref int x, ref int y) {
			#if DriverDebug
				Console.WriteLine("ScreenToMenu({0}, {1}, {2}): Called", Window(handle), x, y);
			#endif
			driver.ScreenToMenu(handle, ref x, ref y);
		}

		internal static void ScrollWindow(IntPtr handle, Rectangle rectangle, int XAmount, int YAmount, bool with_children) {
			#if DriverDebug
				Console.WriteLine("ScrollWindow({0}, {1}, {2}, {3}, {4}): Called", Window(handle), rectangle, XAmount, YAmount, with_children);
			#endif
			driver.ScrollWindow(handle, rectangle, XAmount, YAmount, with_children);
		}

		internal static void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children) {
			#if DriverDebug
				Console.WriteLine("ScrollWindow({0}, {2}, {3}, {4}): Called", Window(handle), XAmount, YAmount, with_children);
			#endif
			driver.ScrollWindow(handle, XAmount, YAmount, with_children);
		}

		internal static void SendAsyncMethod (AsyncMethodData data) {
			#if DriverDebug
				Console.WriteLine("SendAsyncMethod({0}): Called", data);
			#endif
			driver.SendAsyncMethod (data);
		}

		internal static IntPtr SendMessage (IntPtr handle, Msg message, IntPtr wParam, IntPtr lParam) {
			#if DriverDebug
				Console.WriteLine("SendMessage ({0}, {1}, {2:X}, {3:X}): Called", Window(handle), message, wParam.ToInt32(), lParam.ToInt32());
			#endif
			return driver.SendMessage (handle, message, wParam, lParam);
		}

		internal static void SendMessage (ref Message m) {
			#if DriverDebug
				Console.WriteLine("SendMessage ({0}): Called", m);
			#endif
			m.Result = driver.SendMessage(m.HWnd, (Msg)m.Msg, m.WParam, m.LParam);
		}

		internal static void SetAllowDrop (IntPtr handle, bool value)
		{
			#if DriverDebug
			Console.WriteLine ("SetAllowDrop({0}, {1}): Called", handle, value);
			#endif
			driver.SetAllowDrop (handle, value);
		}

		internal static void SetBorderStyle(IntPtr handle, FormBorderStyle border_style) {
			#if DriverDebug
				Console.WriteLine("SetBorderStyle({0}, {1}): Called", Window(handle), border_style);
			#endif
			driver.SetBorderStyle(handle, border_style);
		}

		internal static void SetCaretPos(IntPtr handle, int x, int y) {
			#if DriverDebug
				Console.WriteLine("SetCaretPos({0}, {1}, {2}): Called", Window(handle), x, y);
			#endif
			driver.SetCaretPos(handle, x, y);
		}

		internal static void SetCursor(IntPtr handle, IntPtr cursor) {
			#if DriverDebug
				Console.WriteLine("SetCursor({0}, {1:X}): Called", Window(handle), cursor.ToInt32());
			#endif
			driver.SetCursor(handle, cursor);
		}

		internal static void SetCursorPos(IntPtr handle, int x, int y) {
			#if DriverDebug
				Console.WriteLine("SetCursorPos({0}, {1}, {2}): Called", Window(handle), x, y);
			#endif
			driver.SetCursorPos(handle, x, y);
		}

		internal static void SetFocus(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("SetFocus({0}): Called", Window(handle));
			#endif
			driver.SetFocus(handle);
		}

		internal static void SetIcon(IntPtr handle, Icon icon) {
			#if DriverDebug
				Console.WriteLine("SetIcon({0}, {1}): Called", Window(handle), icon);
			#endif
			driver.SetIcon(handle, icon);
		}

		internal static void SetMenu(IntPtr handle, Menu menu) {
			#if DriverDebug
				Console.WriteLine("SetMenu({0}, {1}): Called", Window(handle), menu);
			#endif
			driver.SetMenu(handle, menu);
		}

		internal static void SetModal(IntPtr handle, bool Modal) {
			#if DriverDebug || DriverDebugState
				Console.WriteLine("SetModal({0}, {1}): Called", Window(handle), Modal);
			#endif
			driver.SetModal(handle, Modal);
		}

		internal static IntPtr SetParent(IntPtr handle, IntPtr hParent) {
			#if DriverDebug
				Console.WriteLine("SetParent({0}, {1:X}): Called", Window(handle), Window(hParent));
			#endif
			return driver.SetParent(handle, hParent);
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
				Console.WriteLine("SetTopMost({0}, {1}, {2}): Called", Window(handle), Window(hWndOwner), Enabled);
			#endif
			return driver.SetTopmost(handle, hWndOwner, Enabled);
		}

		internal static bool SetVisible(IntPtr handle, bool visible) {
			#if DriverDebug || DriverDebugState
				Console.WriteLine("SetVisible({0}, {1}): Called", Window(handle), visible);
			#endif
			return driver.SetVisible(handle, visible);
		}

		internal static void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max) {
			#if DriverDebug || DriverDebugState
				Console.WriteLine("SetWindowMinMax({0}, {1}, {2}, {3}): Called", Window(handle), maximized, min, max);
			#endif
			driver.SetWindowMinMax(handle, maximized, min, max);
		}

		internal static void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			#if DriverDebug
				Console.WriteLine("SetWindowPos({0}, {1}, {2}, {3}, {4}): Called", Window(handle), x, y, width, height);
			#endif
			driver.SetWindowPos(handle, x, y, width, height);
		}

		internal static void SetWindowState(IntPtr handle, FormWindowState state) {
			#if DriverDebug || DriverDebugState
				Console.WriteLine("SetWindowState({0} {1}): Called", Window(handle), state);
			#endif
			driver.SetWindowState(handle, state);
		}

		internal static void SetWindowStyle(IntPtr handle, CreateParams cp) {
			#if DriverDebug
				Console.WriteLine("SetWindowStyle({0}): Called", Window(handle));
			#endif
			driver.SetWindowStyle(handle, cp);
		}

		internal static void SetWindowTransparency(IntPtr handle, double transparency, Color key) 
		{
			#if DriverDebug
				Console.WriteLine("SetWindowTransparency({0}): Called", Window(handle));
			#endif
			driver.SetWindowTransparency(handle, transparency, key);
		}

		internal static bool SetZOrder(IntPtr handle, IntPtr AfterhWnd, bool Top, bool Bottom) {
			#if DriverDebug
				Console.WriteLine("SetZOrder({0}, {1:X}, {2}, {3}): Called", Window(handle), Window(AfterhWnd), Top, Bottom);
			#endif
			return driver.SetZOrder(handle, AfterhWnd, Top, Bottom);
		}

		internal static void ShowCursor(bool show) {
			#if DriverDebug
				Console.WriteLine("ShowCursor({0}): Called", show);
			#endif
			driver.ShowCursor(show);
		}

		internal static DragDropEffects StartDrag(IntPtr handle, object data, DragDropEffects allowedEffects) {
			#if DriverDebug
			Console.WriteLine ("StartDrag({0}, {1}, {2}): Called", Window(handle), data, allowedEffects);
			#endif
			return driver.StartDrag (handle, data, allowedEffects);
		}

		internal static void StartLoop(Thread thread) {
			#if DriverDebug
				Console.WriteLine("StartLoop({0:X}): Called", thread.GetHashCode());
			#endif
			driver.StartLoop(thread);
		}

		internal static bool SupportsTransparency() {
			#if DriverDebug
				Console.WriteLine("SupportsTransparency(): Called, result={1}", driver.SupportsTransparency());
			#endif
			return driver.SupportsTransparency();
		}

		internal static bool SystrayAdd(IntPtr handle, string tip, Icon icon, out ToolTip tt) {
			#if DriverDebug
				Console.WriteLine("SystrayAdd({0}, {1}): Called", Window(handle), tip);
			#endif
			return driver.SystrayAdd(handle, tip, icon, out tt);
		}

		internal static void SystrayChange(IntPtr handle, string tip, Icon icon, ref ToolTip tt) {
			#if DriverDebug
				Console.WriteLine("SystrayChange({0}, {1}): Called", Window(handle), tip);
			#endif
			driver.SystrayChange(handle, tip, icon, ref tt);
		}

		internal static void SystrayRemove(IntPtr handle, ref ToolTip tt) {
			#if DriverDebug
				Console.WriteLine("SystrayRemove({0}): Called", Window(handle));
			#endif
			driver.SystrayRemove(handle, ref tt);
		}

		internal static bool Text(IntPtr handle, string text) {
			#if DriverDebug
				Console.WriteLine("Text({0}, {1}): Called", Window(handle), text);
			#endif
			return driver.Text(handle, text);
		}

		internal static bool TranslateMessage(ref MSG msg) {
			return driver.TranslateMessage(ref msg);
		}

		internal static void UngrabWindow(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("UngrabWindow({0}): Called", Window(handle));
			#endif
			driver.UngrabWindow(handle);
		}

		internal static void UpdateWindow(IntPtr handle) {
			#if DriverDebug
				Console.WriteLine("UpdateWindow({0}): Called", Window(handle));
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

		#region	Delegates
		public delegate bool ClipboardToObject(int type, IntPtr data, out object obj);
		public delegate bool ObjectToClipboard(ref int type, object obj, out byte[] data);
		#endregion	// Delegates

	}
}
