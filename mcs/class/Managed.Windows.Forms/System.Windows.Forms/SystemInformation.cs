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
// Copyright (c) 2004,2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Miguel de Icaza (miguel@novell.com).
//	Peter Bartok	(pbartok@novell.com)
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class SystemInformation
	{
		private SystemInformation ()
		{
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int ActiveWindowTrackingDelay { get { return XplatUI.ActiveWindowTrackingDelay; } }

		public static ArrangeDirection ArrangeDirection {
			get {
				return ThemeEngine.Current.ArrangeDirection;
			}
		}

		public static ArrangeStartingPosition ArrangeStartingPosition {
			get {
				return ThemeEngine.Current.ArrangeStartingPosition;
			}
		}

		public static BootMode BootMode {
			get {
				return BootMode.Normal;
			}
		}

		public static Size Border3DSize {
			get {
				return ThemeEngine.Current.Border3DSize;
			}
		}
		
		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int BorderMultiplierFactor { get { return ThemeEngine.Current.BorderMultiplierFactor; } }

		public static Size BorderSize {
			get {
				return ThemeEngine.Current.BorderSize;
			}
		}

		public static Size CaptionButtonSize {
			get {
				return ThemeEngine.Current.CaptionButtonSize;
			}
		}

		public static int CaptionHeight {
			get {
				return ThemeEngine.Current.CaptionHeight;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int CaretBlinkTime { get { return XplatUI.CaretBlinkTime; } }
		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int CaretWidth { get { return XplatUI.CaretWidth; } }

		public static string ComputerName {
			get {
				return Environment.MachineName;
			}
		}

		public static Size CursorSize {
			get {
				return XplatUI.CursorSize;
			}
		}

		public static bool DbcsEnabled {
			get {
				return false;
			}
		}

		public static bool DebugOS {
			get {
				return false;
			}
		}

		public static Size DoubleClickSize {
			get {
				return ThemeEngine.Current.DoubleClickSize;
			}
		}

		public static int DoubleClickTime {
			get {
				return ThemeEngine.Current.DoubleClickTime;
			}
		}

		public static bool DragFullWindows {
			get {
				return XplatUI.DragFullWindows;
			}
		}

		public static Size DragSize {
			get {
				return XplatUI.DragSize;
			}
		}

		public static Size FixedFrameBorderSize {
			get {
				return ThemeEngine.Current.FixedFrameBorderSize;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int FontSmoothingContrast { get { return XplatUI.FontSmoothingContrast; } }
		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int FontSmoothingType { get { return XplatUI.FontSmoothingType; } }

		public static Size FrameBorderSize {
			get {
				return ThemeEngine.Current.FrameBorderSize;
			}
		}

		public static bool HighContrast {
			get {
				return false;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int HorizontalFocusThickness { get { return ThemeEngine.Current.HorizontalFocusThickness; } }
		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int HorizontalResizeBorderThickness { get { return XplatUI.HorizontalResizeBorderThickness; } }

		public static int HorizontalScrollBarArrowWidth {
			get {
				return ThemeEngine.Current.HorizontalScrollBarArrowWidth;
			}
		}

		public static int HorizontalScrollBarHeight {
			get {
				return ThemeEngine.Current.HorizontalScrollBarHeight;
			}
		}

		public static int HorizontalScrollBarThumbWidth {
			get {
				return ThemeEngine.Current.HorizontalScrollBarThumbWidth;
			}
		}

		public static Size IconSize {
			get {
				return XplatUI.IconSize;
			}
		}

		public static int IconHorizontalSpacing {
			get {
				return IconSpacingSize.Width;
			}
		}

		public static int IconVerticalSpacing {
			get {
				return IconSpacingSize.Height;
			}
		}

		public static Size IconSpacingSize {
			get {
				return ThemeEngine.Current.IconSpacingSize;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsActiveWindowTrackingEnabled {
			get { return XplatUI.IsActiveWindowTrackingEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsComboBoxAnimationEnabled {
			get { return XplatUI.IsComboBoxAnimationEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsDropShadowEnabled {
			get { return XplatUI.IsDropShadowEnabled; }
		}

		public static bool IsFlatMenuEnabled {
			get { return false; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsFontSmoothingEnabled {
			get { return XplatUI.IsFontSmoothingEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsHotTrackingEnabled {
			get { return XplatUI.IsHotTrackingEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsIconTitleWrappingEnabled {
			get { return XplatUI.IsIconTitleWrappingEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsKeyboardPreferred {
			get { return XplatUI.IsKeyboardPreferred; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsListBoxSmoothScrollingEnabled {
			get { return XplatUI.IsListBoxSmoothScrollingEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsMenuAnimationEnabled {
			get { return XplatUI.IsMenuAnimationEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsMenuFadeEnabled {
			get { return XplatUI.IsMenuFadeEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsMinimizeRestoreAnimationEnabled {
			get { return XplatUI.IsMinimizeRestoreAnimationEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsSelectionFadeEnabled {
			get { return XplatUI.IsSelectionFadeEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsSnapToDefaultEnabled {
			get { return XplatUI.IsSnapToDefaultEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsTitleBarGradientEnabled {
			get { return XplatUI.IsTitleBarGradientEnabled; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool IsToolTipAnimationEnabled {
			get { return XplatUI.IsToolTipAnimationEnabled; }
		}

		public static int KanjiWindowHeight {
			get {
				return 0;
			}
		}

		public static int KeyboardDelay {
			get {
				return XplatUI.KeyboardDelay;
			}
		}

		public static int KeyboardSpeed {
			get {
				return XplatUI.KeyboardSpeed;
			}
		}

		public static Size MaxWindowTrackSize {
			get {
				return XplatUI.MaxWindowTrackSize;
			}
		}

		public static bool MenuAccessKeysUnderlined {
			get {
				return ThemeEngine.Current.MenuAccessKeysUnderlined;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static Size MenuBarButtonSize {
			get { return ThemeEngine.Current.MenuBarButtonSize; }
		}

		public static Size MenuButtonSize {
			get {
				return ThemeEngine.Current.MenuButtonSize;
			}
		}

		public static Size MenuCheckSize {
			get {
				return ThemeEngine.Current.MenuCheckSize;
			}
		}

		public static Font MenuFont {
			get {
				// note: we MUST return a clone of the Font instance as anyone
				// can dispose it. However we shouldn't expect the theme to do
				// the cloning for performance reason
				return (Font) ThemeEngine.Current.MenuFont.Clone ();
			}
		}

		public static int MenuHeight {
			get {
				return ThemeEngine.Current.MenuHeight;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int MenuShowDelay { get { return XplatUI.MenuShowDelay; } }

		public static bool MidEastEnabled {
			get {
				return false; // ??? how do we decide?
			}
		}

		public static Size MinimizedWindowSize {
			get {
				return XplatUI.MinimizedWindowSize;
			}
		}

		public static Size MinimizedWindowSpacingSize {
			get {
				return XplatUI.MinimizedWindowSpacingSize;
			}
		}

		public static Size MinimumWindowSize {
			get {
				return XplatUI.MinimumWindowSize;
			}
		}

		public static Size MinWindowTrackSize {
			get {
				return XplatUI.MinWindowTrackSize;
			}
		}

		public static int MonitorCount {
			get {
				return 1;		// Why bother...
			}
		}

		public static bool MonitorsSameDisplayFormat {
			get {
				return true;
			}
		}

		public static int MouseButtons {
			get {
				return XplatUI.MouseButtonCount;
			}
		}

		public static bool MouseButtonsSwapped {
			get {
				return XplatUI.MouseButtonsSwapped;
			}
		}

		public static Size MouseHoverSize {
			get {
				return XplatUI.MouseHoverSize;
			}
		}

		public static int MouseHoverTime {
			get {
				return XplatUI.MouseHoverTime;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int MouseSpeed {
			get { return XplatUI.MouseSpeed; }
		}
		
		public static int MouseWheelScrollDelta {
			get {
				return XplatUI.MouseWheelScrollDelta;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public static bool MousePresent {
			get {
				return true;
			}
		}

		public static bool MouseWheelPresent {
			get {
				return XplatUI.MouseWheelPresent;
			}
		}

		public static int MouseWheelScrollLines {
			get {
				return ThemeEngine.Current.MouseWheelScrollLines;
			}
		}

		public static bool NativeMouseWheelSupport {
			get {
				return MouseWheelPresent;
			}
		}

		public static bool Network {
			get {
				return true;
			}
		}

		public static bool PenWindows {
			get {
				return false;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static LeftRightAlignment PopupMenuAlignment {
			get { return XplatUI.PopupMenuAlignment; }
		}
		
		[MonoTODO ("Only implemented for Win32.")]
		public static PowerStatus PowerStatus {
			get { return XplatUI.PowerStatus; }
		}

		public static Size PrimaryMonitorMaximizedWindowSize {
			get {
				return new Size(WorkingArea.Width, WorkingArea.Height);
			}
		}

		public static Size PrimaryMonitorSize {
			get {
				return new Size(WorkingArea.Width, WorkingArea.Height);
			}
		}

		public static bool RightAlignedMenus {
			get {
				return ThemeEngine.Current.RightAlignedMenus;
			}
		}

		public static ScreenOrientation ScreenOrientation {
			get { return ScreenOrientation.Angle0; }
		}

		public static bool Secure {
			get {
				return true;
			}
		}

		public static bool ShowSounds {
			get {
				return false;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int SizingBorderWidth {
			get { return XplatUI.SizingBorderWidth; }
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static Size SmallCaptionButtonSize {
			get { return XplatUI.SmallCaptionButtonSize; }
		}

		public static Size SmallIconSize {
			get {
				return XplatUI.SmallIconSize;
			}
		}

		public static bool TerminalServerSession {
			get {
				return false;
			}
		}

		public static Size ToolWindowCaptionButtonSize {
			get {
				return ThemeEngine.Current.ToolWindowCaptionButtonSize;
			}
		}

		public static int ToolWindowCaptionHeight {
			get {
				return ThemeEngine.Current.ToolWindowCaptionHeight;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static bool UIEffectsEnabled {
			get { return XplatUI.UIEffectsEnabled; }
		}

		public static string UserDomainName {
			get {
				return Environment.UserDomainName;
			}
		}

		public static bool UserInteractive {
			get {
				return Environment.UserInteractive;
			}
		}

		public static string UserName {
			get {
				return Environment.UserName;
			}
		}

		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int VerticalFocusThickness { get { return ThemeEngine.Current.VerticalFocusThickness; } }
		[MonoInternalNote ("Determine if we need an X11 implementation or if defaults are good.")]
		public static int VerticalResizeBorderThickness { get { return XplatUI.VerticalResizeBorderThickness; } }

		public static int VerticalScrollBarArrowHeight {
			get {
				return ThemeEngine.Current.VerticalScrollBarArrowHeight;
			}
		}

		public static int VerticalScrollBarThumbHeight {
			get {
				return ThemeEngine.Current.VerticalScrollBarThumbHeight;
			}
		}

		public static int VerticalScrollBarWidth {
			get {
				return ThemeEngine.Current.VerticalScrollBarWidth;
			}
		}

		public static Rectangle VirtualScreen {
			get {
				return XplatUI.VirtualScreen;
			}
		}

		public static Rectangle WorkingArea {
			get {
				return XplatUI.WorkingArea;
			}
		}
	}
}
