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
//	Peter Bartok		pbartok@novell.com
//	Sebastien Pouliot	sebastien@ximian.com
//

// COMPLETE

using System.Drawing;
using System.Threading;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	internal abstract class XplatUIDriver {
		internal abstract IntPtr	InitializeDriver();
		internal abstract void		ShutdownDriver(IntPtr token);
		internal delegate IntPtr	WndProc(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);


		#region XplatUI Driver Properties
		internal virtual int ActiveWindowTrackingDelay { get { return 0; } }

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

		internal virtual Size Border3DSize {
			get {
				return new Size (2, 2);
			}
		}

		internal virtual Size BorderSize {
			get {
				return new Size (1, 1);
			}
		}

		internal virtual Size CaptionButtonSize {
			get {
				return new Size (18, 18);
			}
		}

		internal virtual int CaretBlinkTime { get { return 530; } }
		internal virtual int CaretWidth { get { return 10; } }
		
		internal virtual Size DoubleClickSize {
			get {
				return new Size (4, 4);
			}
		}

		internal virtual int DoubleClickTime {
			get {
				return 500;
			}
		}

		internal virtual Size FixedFrameBorderSize {
			get {
				return new Size (3, 3);
			}
		}

		internal virtual Font Font {
			get {
				return ThemeEngine.Current.DefaultFont;
			}
		}

		internal virtual int FontSmoothingContrast { get { return 1400; } }
		internal virtual int FontSmoothingType { get { return 1; } }
		internal virtual int HorizontalResizeBorderThickness { get { return 8; } }
		internal virtual bool IsActiveWindowTrackingEnabled { get { return false; } }
		internal virtual bool IsComboBoxAnimationEnabled { get { return false; } }
		internal virtual bool IsDropShadowEnabled { get { return false; } }
		internal virtual bool IsFontSmoothingEnabled { get { return true; } }
		internal virtual bool IsHotTrackingEnabled { get { return false; } }
		internal virtual bool IsIconTitleWrappingEnabled { get { return true; } }
		internal virtual bool IsKeyboardPreferred { get { return false; } }
		internal virtual bool IsListBoxSmoothScrollingEnabled { get { return true; } }
		internal virtual bool IsMenuAnimationEnabled { get { return false; } }
		internal virtual bool IsMenuFadeEnabled { get { return true; } }
		internal virtual bool IsMinimizeRestoreAnimationEnabled { get { return false; } }
		internal virtual bool IsSelectionFadeEnabled { get { return false; } }
		internal virtual bool IsSnapToDefaultEnabled { get { return false; } }
		internal virtual bool IsTitleBarGradientEnabled { get { return false; } }
		internal virtual bool IsToolTipAnimationEnabled { get { return false; } }
		internal virtual Size MenuBarButtonSize { get { return new Size (19, 19); } }
		public virtual Size MenuButtonSize {
			get {
				return new Size(18, 18);
			}
		}
		internal virtual int MenuShowDelay { get { return 0; } }
		
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

		internal virtual Size MouseHoverSize {
			get {
				return new Size (1, 1);
			}
		}

		internal virtual int MouseHoverTime {
			get {
				return 500;
			}
		}

		internal virtual int MouseSpeed {
			get { return 10; }
		}
		
		internal virtual int MouseWheelScrollDelta {
			get {
				return 120;
			}
		}
		
		internal virtual Point MousePosition {
			get {
				return Point.Empty;
			}
		}

		internal virtual int MenuHeight {
			get {
				return 19;
			}
		}

		internal virtual LeftRightAlignment PopupMenuAlignment {
			get { return LeftRightAlignment.Left; }
		}
		
		internal virtual PowerStatus PowerStatus {
			get { throw new NotImplementedException ("Has not been implemented yet for this platform."); }
		}

		internal virtual int SizingBorderWidth {
			get { return 4; }
		}
		
		internal virtual Size SmallCaptionButtonSize {
			get { return new Size (15, 15); }
		}
		
		internal virtual bool UIEffectsEnabled {
			get { return false; }
		}
		
		internal virtual bool DropTarget {
			get {
				return false;
			}

			set {
			}
		}

		internal virtual int HorizontalScrollBarHeight {
			get {
				return 16;
			}
		}

		internal virtual bool UserClipWontExposeParent {
			get {
				return true;
			}
		}

		internal virtual int VerticalResizeBorderThickness { get { return 8; } }

		internal virtual int VerticalScrollBarWidth {
			get {
				return 16;
			}
		}

		internal abstract int CaptionHeight { get; }
		internal abstract Size CursorSize { get; }
		internal abstract bool DragFullWindows { get; }
		internal abstract Size DragSize { get; }
		internal abstract Size FrameBorderSize { get; }
		internal abstract Size IconSize { get; }
		internal abstract Size MaxWindowTrackSize { get; }
		internal abstract bool MenuAccessKeysUnderlined { get; }
		internal virtual Size MinimizedWindowSize {
			get {
				const int BorderWidth = 3;
				return new Size (154 + 2 * BorderWidth, SystemInformation.CaptionHeight + 2 * BorderWidth - 1);
			}
		}
		internal abstract Size MinimizedWindowSpacingSize { get; }
		internal abstract Size MinimumWindowSize { get; }
		internal virtual Size MinimumFixedToolWindowSize { get { return Size.Empty; } }
		internal virtual Size MinimumSizeableToolWindowSize { get { return Size.Empty; } }
		internal virtual Size MinimumNoBorderWindowSize { get { return Size.Empty; } }
		internal virtual Size MinWindowTrackSize {
			get {
				return new Size (112, 27);
			}
		}
		internal abstract Size SmallIconSize { get; }
		internal abstract int MouseButtonCount { get; }
		internal abstract bool MouseButtonsSwapped { get; }
		internal abstract bool MouseWheelPresent { get; }
		internal abstract Rectangle VirtualScreen { get; }
		internal abstract Rectangle WorkingArea { get; }
		internal abstract bool ThemesEnabled { get; }

		internal virtual bool RequiresPositiveClientAreaSize {
			get {
				return true;
			}
		}

		public virtual int ToolWindowCaptionHeight {
			get {
				return 16;
			}
		}

		public virtual Size ToolWindowCaptionButtonSize {
			get {
				return new Size (15, 15);
			}
		}
		#endregion	// XplatUI Driver Properties

		internal abstract event EventHandler Idle;

		#region XplatUI Driver Methods
		internal abstract void AudibleAlert(AlertType alert);

		internal abstract void BeginMoveResize (IntPtr handle); // init a window manager driven resize event

		internal abstract void EnableThemes();

		internal abstract void GetDisplaySize(out Size size);

		internal abstract IntPtr CreateWindow(CreateParams cp);
		internal abstract IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height);
		internal abstract void DestroyWindow(IntPtr handle);

		internal abstract FormWindowState GetWindowState(IntPtr handle);
		internal abstract void SetWindowState(IntPtr handle, FormWindowState state);
		internal abstract void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max);

		internal abstract void SetWindowStyle(IntPtr handle, CreateParams cp);

		internal abstract double GetWindowTransparency(IntPtr handle);
		internal abstract void SetWindowTransparency(IntPtr handle, double transparency, Color key);
		internal abstract TransparencySupport SupportsTransparency();

		internal virtual void SetAllowDrop (IntPtr handle, bool value)
		{
			Console.Error.WriteLine ("Drag and Drop is currently " +
					"not supported on this platform");
		}

		internal virtual DragDropEffects StartDrag(IntPtr handle, object data, DragDropEffects allowedEffects) {
			Console.Error.WriteLine ("Drag and Drop is currently " +
					"not supported on this platform");
			return DragDropEffects.None;
		}

		internal abstract void SetBorderStyle(IntPtr handle, FormBorderStyle border_style);
		internal abstract void SetMenu(IntPtr handle, Menu menu);

		internal abstract bool GetText(IntPtr handle, out string text);
		internal abstract bool Text(IntPtr handle, string text);
		internal abstract bool SetVisible(IntPtr handle, bool visible, bool activate);
		internal abstract bool IsVisible(IntPtr handle);
		internal abstract bool IsEnabled(IntPtr handle);
		internal virtual bool IsKeyLocked (VirtualKeys key) { return false; }
		internal abstract IntPtr SetParent(IntPtr handle, IntPtr parent);
		internal abstract IntPtr GetParent(IntPtr handle);

		internal abstract void UpdateWindow(IntPtr handle);
		internal abstract PaintEventArgs PaintEventStart (ref Message msg, IntPtr handle, bool client);
		internal abstract void PaintEventEnd (ref Message msg, IntPtr handle, bool client);

		internal abstract void SetWindowPos(IntPtr handle, int x, int y, int width, int height);
		internal abstract void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height);
		internal abstract void Activate(IntPtr handle);
		internal abstract void EnableWindow(IntPtr handle, bool Enable);
		internal abstract void SetModal(IntPtr handle, bool Modal);
		internal abstract void Invalidate(IntPtr handle, Rectangle rc, bool clear);
		internal abstract void InvalidateNC(IntPtr handle);
		internal abstract IntPtr DefWndProc(ref Message msg);
		internal abstract void HandleException(Exception e);
		internal abstract void DoEvents();
		internal abstract bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);
		internal abstract void PostQuitMessage(int exitCode);
		internal abstract bool GetMessage(object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);
		internal abstract bool TranslateMessage(ref MSG msg);
		internal abstract IntPtr DispatchMessage(ref MSG msg);

		internal abstract bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom);
		internal abstract bool SetTopmost(IntPtr hWnd, bool Enabled);
		internal abstract bool SetOwner(IntPtr hWnd, IntPtr hWndOwner);

		internal abstract bool CalculateWindowRect(ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect);

		internal abstract Region GetClipRegion(IntPtr hwnd);
		internal abstract void SetClipRegion(IntPtr hwnd, Region region);

		internal abstract void SetCursor(IntPtr hwnd, IntPtr cursor);
		internal abstract void ShowCursor(bool show);
		internal abstract void OverrideCursor(IntPtr cursor);
		internal abstract IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot);
		internal abstract IntPtr DefineStdCursor(StdCursor id);
		internal abstract Bitmap DefineStdCursorBitmap(StdCursor id);
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

		internal abstract IntPtr GetFocus();
		internal abstract void SetFocus(IntPtr hwnd);
		internal abstract IntPtr GetActive();
		internal abstract IntPtr GetPreviousWindow(IntPtr hwnd);

		internal abstract void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool with_children);
		internal abstract void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool with_children);

		internal abstract bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent);

		internal abstract bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt);
		internal abstract bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt);
		internal abstract void SystrayRemove(IntPtr hwnd, ref ToolTip tt);
		internal abstract void SystrayBalloon(IntPtr hwnd, int timeout, string title, string text, ToolTipIcon icon);

		internal abstract Point GetMenuOrigin(IntPtr hwnd);
		internal abstract void MenuToScreen(IntPtr hwnd, ref int x, ref int y);
		internal abstract void ScreenToMenu(IntPtr hwnd, ref int x, ref int y);

		internal abstract void SetIcon(IntPtr handle, Icon icon);

		internal abstract void ClipboardClose(IntPtr handle);
		internal abstract IntPtr ClipboardOpen (bool primary_selection);
		internal abstract int ClipboardGetID(IntPtr handle, string format);
		internal abstract void ClipboardStore(IntPtr handle, object obj, int id, XplatUI.ObjectToClipboard converter, bool copy);
		internal abstract int[] ClipboardAvailableFormats(IntPtr handle);
		internal abstract object ClipboardRetrieve(IntPtr handle, int id, XplatUI.ClipboardToObject converter);

		internal abstract void DrawReversibleLine(Point start, Point end, Color backColor);
		internal abstract void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width);
		internal abstract void FillReversibleRectangle (Rectangle rectangle, Color backColor);
		internal abstract void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style);

		internal abstract SizeF GetAutoScaleSize(Font font);

		internal abstract IntPtr SendMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);
		internal abstract bool PostMessage(IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);
		internal abstract int SendInput(IntPtr hwnd, System.Collections.Queue keys);

		internal abstract object StartLoop(Thread thread);
		internal abstract void EndLoop(Thread thread);

		internal abstract void RequestNCRecalc(IntPtr hwnd);
		internal abstract void ResetMouseHover(IntPtr hwnd);
		internal abstract void RequestAdditionalWM_NCMessages(IntPtr hwnd, bool hover, bool leave);

		internal abstract void RaiseIdle (EventArgs e);
		
		// System information
		internal abstract int KeyboardSpeed { get; } 
		internal abstract int KeyboardDelay { get; } 


		// Double buffering
		internal virtual void CreateOffscreenDrawable (IntPtr handle,
							       int width, int height,
							       out object offscreen_drawable)
		{
			Bitmap bmp = new Bitmap (width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			offscreen_drawable = bmp;
		}

		internal virtual void DestroyOffscreenDrawable (object offscreen_drawable)
		{
			Bitmap bmp = (Bitmap)offscreen_drawable;

			bmp.Dispose ();
		}
		
		internal virtual Graphics GetOffscreenGraphics (object offscreen_drawable)
		{
			Bitmap bmp = (Bitmap)offscreen_drawable;
			return Graphics.FromImage (bmp);
		}

		internal virtual void BlitFromOffscreen (IntPtr dest_handle,
							 Graphics dest_dc,
							 object offscreen_drawable,
							 Graphics offscreen_dc,
							 Rectangle r)
		{
			dest_dc.DrawImage ((Bitmap)offscreen_drawable, r, r, GraphicsUnit.Pixel);
		}
		
		internal virtual void SetForegroundWindow (IntPtr handle)
		{
		}

#endregion	// XplatUI Driver Methods
	}

	internal class XplatUIDriverSupport {
		#region XplatUI Driver Support Methods
		internal static void ExecutionCallback (object state)
		{
			AsyncMethodData data = (AsyncMethodData) state;
			AsyncMethodResult result = data.Result;
			
			object ret;
			try {
				ret = data.Method.DynamicInvoke (data.Args);
			} catch (Exception ex) {
				if (result != null) {
					result.CompleteWithException (ex);
					return;
				}
				
				throw;
			}
		
			if (result != null) {
				result.Complete (ret);
			}
		}

		internal static void ExecuteClientMessage (GCHandle gchandle)
		{
			AsyncMethodData data = (AsyncMethodData) gchandle.Target;
			try {
				if (data.Context == null) {
					ExecutionCallback (data);
				} else {
					ExecutionContext.Run (data.Context, new ContextCallback (ExecutionCallback), data);
				}
			}
			finally {
				gchandle.Free ();
			}
		}
		
		#endregion	// XplatUI Driver Support Methods
	}
}
