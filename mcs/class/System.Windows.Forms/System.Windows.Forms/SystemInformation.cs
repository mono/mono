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

		public static Size Border3DSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXBORDER ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYBORDER ) );
			}
		}
		[MonoTODO]
		public static Size BorderSize {

			get{ throw new NotImplementedException (); }
		}

		public static Size CaptionButtonSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXSIZE ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYSIZE ) );
			}
		}

		public static int CaptionHeight {
			get{ 	return Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYCAPTION ); }
		}

		[DllImport ("libc")]
		static unsafe extern int gethostname (byte *p, int len);
		
		static string _gethostname ()
		{
			byte [] buf = new byte [256];
			int count;
			unsafe {
				fixed (byte *p = &buf [0]){
					byte *q = p;
					gethostname (p, 256);
					while (*q != 0 && ((int)(p-q) < 256))
						q++;
					count = (int) (q - p);
				}
			}
			return new String(Encoding.UTF8.GetChars (buf, 0, count));
		}
	
		public static string ComputerName {
			get {
				return _gethostname ();
			}
		}
		
		public static Size CursorSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXCURSOR ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYCURSOR ) );
			}
		}

		public static bool DbcsEnabled {
			get{ 	return Win32.GetSystemMetrics( SystemMetricsCodes.SM_DBCSENABLED ) != 0; }
		}

		public static bool DebugOS {
			get{ 	return Win32.GetSystemMetrics( SystemMetricsCodes.SM_DEBUG ) != 0; }
		}

		public static Size DoubleClickSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXDOUBLECLK ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYDOUBLECLK ) );
			}
		}
		[MonoTODO]
		public static int DoubleClickTime {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool DragFullWindows {

			get{ throw new NotImplementedException (); }
		}

		public static Size DragSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXDRAG ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYDRAG ) );
			}
		}

		public static Size FixedFrameBorderSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXFIXEDFRAME ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYFIXEDFRAME ) );
			}
		}

		public static Size FrameBorderSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXFRAME ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYFRAME ) );
			}
		}
		[MonoTODO]
		public static bool HighContrast {

			get{ throw new NotImplementedException (); }
		}

		public static int HorizontalScrollBarArrowWidth {
			get{ 	return Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXHSCROLL ); }
		}

		public static int HorizontalScrollBarHeight {
			get{ 	return Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYHSCROLL ); }
		}

		public static int HorizontalScrollBarThumbWidth {
			get{ 	return Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXHTHUMB ); }
		}

		public static Size IconSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXICON ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYICON ) );
			}
		}

		public static Size IconSpacingSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXICONSPACING ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYICONSPACING ) );
			}
		}

		public static int KanjiWindowHeight {
			get{ return Win32.GetSystemMetrics ( SystemMetricsCodes.SM_CYKANJIWINDOW ); }
		}

		public static Size MaxWindowTrackSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXMAXTRACK ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYMAXTRACK ) );
			}
		}
		[MonoTODO]
		public static Size MenuButtonSize {

			get{ throw new NotImplementedException (); }
		}

		public static Size MenuCheckSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXMENUCHECK ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYMENUCHECK ) );
			}
		}
		public static Font MenuFont {
			get { return Font.FromHfont( Win32.GetStockObject( GSO_.SYSTEM_FONT ) ); }
		}

		public static int MenuHeight {
			get{ return Win32.GetSystemMetrics ( SystemMetricsCodes.SM_CYMENU ); }
		}

		public static bool MidEastEnabled {
			get{ return Win32.GetSystemMetrics ( SystemMetricsCodes.SM_MIDEASTENABLED ) != 0; }
		}

		public static Size MinimizedWindowSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXMINIMIZED ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYMINIMIZED ) );
			}
		}

		public static Size MinimizedWindowSpacingSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXMINSPACING ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYMINSPACING ) );
			}
		}

		public static Size MinimumWindowSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXMIN ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYMIN ) );
			}
		}

		public static Size MinWindowTrackSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXMINTRACK ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYMINTRACK ) );
			}
		}

		public static int MonitorCount {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_CMONITORS ); }
		}

		public static bool MonitorsSameDisplayFormat {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_SAMEDISPLAYFORMAT ) != 0; }
		}

		public static int MouseButtons {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_CMOUSEBUTTONS ); }
		}

		public static bool MouseButtonsSwapped {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_SWAPBUTTON ) != 0; }
		}

		public static bool MousePresent {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_MOUSEPRESENT ) != 0; }
		}

		public static bool MouseWheelPresent {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_MOUSEWHEELPRESENT ) != 0; }
		}
		[MonoTODO]
		public static int MouseWheelScrollLines {

			get{ throw new NotImplementedException (); }
		}
		[MonoTODO]
		public static bool NativeMouseWheelSupport {

			get{ throw new NotImplementedException (); }
		}

		public static bool Network {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_NETWORK ) != 0; }
		}

		public static bool PenWindows {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_PENWINDOWS ) != 0; }
		}

		public static Size PrimaryMonitorMaximizedWindowSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXFULLSCREEN ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYFULLSCREEN ) );
			}
		}

		public static Size PrimaryMonitorSize {
			get{ 
				return new Size ( Win32.GetSystemMetrics( SystemMetricsCodes.SM_CXSCREEN ),
						  Win32.GetSystemMetrics( SystemMetricsCodes.SM_CYSCREEN ) );
			}
		}
		[MonoTODO]
		public static bool RightAlignedMenus {

			get{ throw new NotImplementedException (); }
		}

		public static bool Secure {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_SECURE ) != 0; }
		}

		public static bool ShowSounds {
			get{ return Win32.GetSystemMetrics( SystemMetricsCodes.SM_SHOWSOUNDS ) != 0; }
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
