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

namespace System.Windows.Forms
{
	public class SystemInformation
	{
		private SystemInformation ()
		{
		}

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

		public static Size BorderSize {
			get {
				return ThemeEngine.Current.Border3DSize;
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

		public static Size IconSpacingSize {
			get {
				return ThemeEngine.Current.IconSpacingSize;
			}
		}

		public static int KanjiWindowHeight {
			get {
				return 0;
			}
		}

#if NET_2_0
		public
#else
		internal
#endif
		static int KeyboardDelay {
			get {
				return XplatUI.KeyboardDelay;
			}
		}

#if NET_2_0
		public
#else
		internal
#endif
		static int KeyboardSpeed {
			get {
				return XplatUI.KeyboardSpeed;
			}
		}

		public static Size MaxWindowTrackSize {
			get {
				return XplatUI.MaxWindowTrackSize;
			}
		}

#if NET_2_0
		public
#else
		internal
#endif
		static bool MenuAccessKeysUnderlined {
			get {
				return ThemeEngine.Current.MenuAccessKeysUnderlined;
			}
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

#if NET_2_0
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
#endif

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

		public static Size SmallIconSize {
			get {
				return XplatUI.SmallIconSize;
			}
		}

#if NET_2_0
		public static bool TerminalServerSession {
			get {
				return false;
			}
		}
#endif

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
