//
// System.Windows.Forms.SystemInformation
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Windows.Forms {

	// <summary>
	//	Provides information about the operating system.
	// </summary>

	public class SystemInformation {

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public static ArrangeDirection ArrangeDirection {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static ArrangeStartingPosition ArrangeStartingPosition {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static BootMode BootMode {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size Border3DSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size BorderSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size CaptionButtonSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int CaptionHeight {

			get{ 
				return Win32.GetSystemMetrics(SystemMetricsCodes.SM_CYCAPTION); 
			}
		}

		[DllImport ("libc")]
		static unsafe extern int gethostname (byte *p, int len);
		
		static string _gethostname ()
		{
			byte [] buf = new byte [256];
			unsafe {
				fixed (byte *p = &buf [0]){
					gethostname (p, 256);
				}
			}
			return new String(Encoding.UTF8.GetChars (buf));
		}
	
		public static string ComputerName {
			get {
				return _gethostname ();
			}
		}
		
		[MonoTODO]
		public static Size CursorSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool DbcsEnabled {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool DebugOS {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size DoubleClickSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int DoubleClickTime {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool DragFullWindows {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size DragSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size FixedFrameBorderSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size FrameBorderSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool HighContrast {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int HorizontalScrollBarArrowWidth {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int HorizontalScrollBarHeight {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int HorizontalScrollBarThumbWidth {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size IconSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size IconSpacingSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int KanjiWindowHeight {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size MaxWindowTrackSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size MenuButtonSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size MenuCheckSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Font MenuFont {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int MenuHeight {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool MidEastEnabled {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size MinimizedWindowSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size MinimizedWindowSpacingSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size MinimumWindowSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size MinWindowTrackSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int MonitorCount {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool MonitorsSameDisplayFormat {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int MouseButtons {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool MouseButtonsSwapped {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool MousePresent {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool MouseWheelPresent {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int MouseWheelScrollLines {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool NativeMouseWheelSupport {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool Network {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool PenWindows {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size PrimaryMonitorMaximizedWindowSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size PrimaryMonitorSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool RightAlignedMenus {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool Secure {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool ShowSounds {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size SmallIconSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Size ToolWindowCaptionButtonSize {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int ToolWindowCaptionHeight {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static string UserDomainName {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool UserInteractive {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static string UserName {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int VerticalScrollBarArrowHeight {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int VerticalScrollBarThumbHeight {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static int VerticalScrollBarWidth {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Rectangle VirtualScreen {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static Rectangle WorkingArea {

			get{ throw new NotImplementedException (); }
		}
	}
}
