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
//	Alexander Olk   alex.olk@googlemail.com
//
//

// NOTE:
//	This driver understands the following environment variables: (Set the var to enable feature)
//
//	MONO_XEXCEPTIONS	= throw an exception when a X11 error is encountered;
//				  by default a message is displayed but execution continues
//
//	MONO_XSYNC		= perform all X11 commands synchronous; this is slower but
//				  helps in debugging errors
//

// NOT COMPLETE - WORK IN PROGRESS

// One feature of the driver is, that PaintEventstart returns a graphics context created from a offscreen drawable (pixmap)

// define to log Window handles and relationships to stdout
#undef DriverDebug

// Extra detailed debug
#undef	DriverDebugExtra

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mono.Unix.Native;

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11GTK : XplatUIDriver {
		
		internal enum GdkWindowClass {
			GDK_INPUT_OUTPUT,
			GDK_INPUT_ONLY
		}
		
		internal enum GdkWindowType {
			GDK_WINDOW_ROOT,
			GDK_WINDOW_TOPLEVEL,
			GDK_WINDOW_CHILD,
			GDK_WINDOW_DIALOG,
			GDK_WINDOW_TEMP,
			GDK_WINDOW_FOREIGN
		}
		
		internal enum GdkWindowHints {
			GDK_HINT_POS	     = 1 << 0,
			GDK_HINT_MIN_SIZE    = 1 << 1,
			GDK_HINT_MAX_SIZE    = 1 << 2,
			GDK_HINT_BASE_SIZE   = 1 << 3,
			GDK_HINT_ASPECT      = 1 << 4,
			GDK_HINT_RESIZE_INC  = 1 << 5,
			GDK_HINT_WIN_GRAVITY = 1 << 6,
			GDK_HINT_USER_POS    = 1 << 7,
			GDK_HINT_USER_SIZE   = 1 << 8
		}
		
		internal enum GdkGravity {
			GDK_GRAVITY_NORTH_WEST = 1,
			GDK_GRAVITY_NORTH,
			GDK_GRAVITY_NORTH_EAST,
			GDK_GRAVITY_WEST,
			GDK_GRAVITY_CENTER,
			GDK_GRAVITY_EAST,
			GDK_GRAVITY_SOUTH_WEST,
			GDK_GRAVITY_SOUTH,
			GDK_GRAVITY_SOUTH_EAST,
			GDK_GRAVITY_STATIC
		}
		
		internal enum GdkWindowEdge {
			GDK_WINDOW_EDGE_NORTH_WEST,
			GDK_WINDOW_EDGE_NORTH,
			GDK_WINDOW_EDGE_NORTH_EAST,
			GDK_WINDOW_EDGE_WEST,
			GDK_WINDOW_EDGE_EAST,
			GDK_WINDOW_EDGE_SOUTH_WEST,
			GDK_WINDOW_EDGE_SOUTH,
			GDK_WINDOW_EDGE_SOUTH_EAST  
		}
		
		internal enum GdkWindowTypeHint {
			GDK_WINDOW_TYPE_HINT_NORMAL,
			GDK_WINDOW_TYPE_HINT_DIALOG,
			GDK_WINDOW_TYPE_HINT_MENU,
			GDK_WINDOW_TYPE_HINT_TOOLBAR,
			GDK_WINDOW_TYPE_HINT_SPLASHSCREEN,
			GDK_WINDOW_TYPE_HINT_UTILITY,
			GDK_WINDOW_TYPE_HINT_DOCK,
			GDK_WINDOW_TYPE_HINT_DESKTOP
		}
		
		internal enum GdkWindowAttributesType {
			GDK_WA_TITLE 	= 1 << 1,
			GDK_WA_X 	= 1 << 2,
			GDK_WA_Y 	= 1 << 3,
			GDK_WA_CURSOR 	= 1 << 4,
			GDK_WA_COLORMAP = 1 << 5,
			GDK_WA_VISUAL	= 1 << 6,
			GDK_WA_WMCLASS 	= 1 << 7,
			GDK_WA_NOREDIR  = 1 << 8
		}
		
		internal enum GdkEventMask {
			GDK_EXPOSURE_MASK		= 1 << 1,
			GDK_POINTER_MOTION_MASK		= 1 << 2,
			GDK_POINTER_MOTION_HINT_MASK	= 1 << 3,
			GDK_BUTTON_MOTION_MASK		= 1 << 4,
			GDK_BUTTON1_MOTION_MASK		= 1 << 5,
			GDK_BUTTON2_MOTION_MASK		= 1 << 6,
			GDK_BUTTON3_MOTION_MASK		= 1 << 7,
			GDK_BUTTON_PRESS_MASK		= 1 << 8,
			GDK_BUTTON_RELEASE_MASK		= 1 << 9,
			GDK_KEY_PRESS_MASK		= 1 << 10,
			GDK_KEY_RELEASE_MASK		= 1 << 11,
			GDK_ENTER_NOTIFY_MASK		= 1 << 12,
			GDK_LEAVE_NOTIFY_MASK		= 1 << 13,
			GDK_FOCUS_CHANGE_MASK		= 1 << 14,
			GDK_STRUCTURE_MASK		= 1 << 15,
			GDK_PROPERTY_CHANGE_MASK	= 1 << 16,
			GDK_VISIBILITY_NOTIFY_MASK	= 1 << 17,
			GDK_PROXIMITY_IN_MASK		= 1 << 18,
			GDK_PROXIMITY_OUT_MASK		= 1 << 19,
			GDK_SUBSTRUCTURE_MASK		= 1 << 20,
			GDK_SCROLL_MASK 		= 1 << 21,
			GDK_ALL_EVENTS_MASK		= 0x3FFFFE
		}
		
		internal enum GdkEventType {
			GDK_NOTHING		= -1,
			GDK_DELETE		= 0,
			GDK_DESTROY		= 1,
			GDK_EXPOSE		= 2,
			GDK_MOTION_NOTIFY	= 3,
			GDK_BUTTON_PRESS	= 4,
			GDK_2BUTTON_PRESS	= 5,
			GDK_3BUTTON_PRESS	= 6,
			GDK_BUTTON_RELEASE	= 7,
			GDK_KEY_PRESS		= 8,
			GDK_KEY_RELEASE		= 9,
			GDK_ENTER_NOTIFY	= 10,
			GDK_LEAVE_NOTIFY	= 11,
			GDK_FOCUS_CHANGE	= 12,
			GDK_CONFIGURE		= 13,
			GDK_MAP			= 14,
			GDK_UNMAP		= 15,
			GDK_PROPERTY_NOTIFY	= 16,
			GDK_SELECTION_CLEAR	= 17,
			GDK_SELECTION_REQUEST 	= 18,
			GDK_SELECTION_NOTIFY	= 19,
			GDK_PROXIMITY_IN	= 20,
			GDK_PROXIMITY_OUT	= 21,
			GDK_DRAG_ENTER        	= 22,
			GDK_DRAG_LEAVE        	= 23,
			GDK_DRAG_MOTION       	= 24,
			GDK_DRAG_STATUS       	= 25,
			GDK_DROP_START        	= 26,
			GDK_DROP_FINISHED     	= 27,
			GDK_CLIENT_EVENT	= 28,
			GDK_VISIBILITY_NOTIFY 	= 29,
			GDK_NO_EXPOSE		= 30,
			GDK_SCROLL            	= 31,
			GDK_WINDOW_STATE      	= 32,
			GDK_SETTING           	= 33,
			GDK_OWNER_CHANGE      	= 34,
			GDK_GRAB_BROKEN       	= 35
		}
		
		internal enum GdkWMDecoration {
			GDK_DECOR_ALL		= 1 << 0,
			GDK_DECOR_BORDER	= 1 << 1,
			GDK_DECOR_RESIZEH	= 1 << 2,
			GDK_DECOR_TITLE		= 1 << 3,
			GDK_DECOR_MENU		= 1 << 4,
			GDK_DECOR_MINIMIZE	= 1 << 5,
			GDK_DECOR_MAXIMIZE	= 1 << 6
		}
		
		internal enum GdkWMFunction {
			GDK_FUNC_ALL		= 1 << 0,
			GDK_FUNC_RESIZE		= 1 << 1,
			GDK_FUNC_MOVE		= 1 << 2,
			GDK_FUNC_MINIMIZE	= 1 << 3,
			GDK_FUNC_MAXIMIZE	= 1 << 4,
			GDK_FUNC_CLOSE		= 1 << 5
		}
		
		internal enum GdkCursorType {
			GDK_X_CURSOR 		  = 0,
			GDK_ARROW 		  = 2,
			GDK_BASED_ARROW_DOWN      = 4,
			GDK_BASED_ARROW_UP 	  = 6,
			GDK_BOAT 		  = 8,
			GDK_BOGOSITY 		  = 10,
			GDK_BOTTOM_LEFT_CORNER    = 12,
			GDK_BOTTOM_RIGHT_CORNER   = 14,
			GDK_BOTTOM_SIDE 	  = 16,
			GDK_BOTTOM_TEE 	  	  = 18,
			GDK_BOX_SPIRAL 	  	  = 20,
			GDK_CENTER_PTR 	  	  = 22,
			GDK_CIRCLE 		  = 24,
			GDK_CLOCK	 	  = 26,
			GDK_COFFEE_MUG 	  	  = 28,
			GDK_CROSS 		  = 30,
			GDK_CROSS_REVERSE 	  = 32,
			GDK_CROSSHAIR 	  	  = 34,
			GDK_DIAMOND_CROSS 	  = 36,
			GDK_DOT 		  = 38,
			GDK_DOTBOX 		  = 40,
			GDK_DOUBLE_ARROW 	  = 42,
			GDK_DRAFT_LARGE 	  = 44,
			GDK_DRAFT_SMALL 	  = 46,
			GDK_DRAPED_BOX 	  	  = 48,
			GDK_EXCHANGE 		  = 50,
			GDK_FLEUR 		  = 52,
			GDK_GOBBLER 		  = 54,
			GDK_GUMBY 		  = 56,
			GDK_HAND1 		  = 58,
			GDK_HAND2 		  = 60,
			GDK_HEART 		  = 62,
			GDK_ICON 		  = 64,
			GDK_IRON_CROSS 	  	  = 66,
			GDK_LEFT_PTR 		  = 68,
			GDK_LEFT_SIDE 	  	  = 70,
			GDK_LEFT_TEE 		  = 72,
			GDK_LEFTBUTTON 	  	  = 74,
			GDK_LL_ANGLE 		  = 76,
			GDK_LR_ANGLE 	 	  = 78,
			GDK_MAN 		  = 80,
			GDK_MIDDLEBUTTON 	  = 82,
			GDK_MOUSE 		  = 84,
			GDK_PENCIL 		  = 86,
			GDK_PIRATE 		  = 88,
			GDK_PLUS 		  = 90,
			GDK_QUESTION_ARROW 	  = 92,
			GDK_RIGHT_PTR 	  	  = 94,
			GDK_RIGHT_SIDE 	  	  = 96,
			GDK_RIGHT_TEE 	  	  = 98,
			GDK_RIGHTBUTTON 	  = 100,
			GDK_RTL_LOGO 		  = 102,
			GDK_SAILBOAT 		  = 104,
			GDK_SB_DOWN_ARROW 	  = 106,
			GDK_SB_H_DOUBLE_ARROW     = 108,
			GDK_SB_LEFT_ARROW 	  = 110,
			GDK_SB_RIGHT_ARROW 	  = 112,
			GDK_SB_UP_ARROW 	  = 114,
			GDK_SB_V_DOUBLE_ARROW 	  = 116,
			GDK_SHUTTLE 		  = 118,
			GDK_SIZING 		  = 120,
			GDK_SPIDER		  = 122,
			GDK_SPRAYCAN 		  = 124,
			GDK_STAR 		  = 126,
			GDK_TARGET 		  = 128,
			GDK_TCROSS 		  = 130,
			GDK_TOP_LEFT_ARROW 	  = 132,
			GDK_TOP_LEFT_CORNER 	  = 134,
			GDK_TOP_RIGHT_CORNER 	  = 136,
			GDK_TOP_SIDE 		  = 138,
			GDK_TOP_TEE 		  = 140,
			GDK_TREK 		  = 142,
			GDK_UL_ANGLE 		  = 144,
			GDK_UMBRELLA 		  = 146,
			GDK_UR_ANGLE 		  = 148,
			GDK_WATCH 		  = 150,
			GDK_XTERM 		  = 152,
			GDK_LAST_CURSOR,
			GDK_CURSOR_IS_PIXMAP 	  = -1
		}
		
		internal enum GdkPropMode {
			GDK_PROP_MODE_REPLACE,
			GDK_PROP_MODE_PREPEND,
			GDK_PROP_MODE_APPEND
		}
		
		[StructLayout (LayoutKind.Sequential)] 
		internal struct GdkGeometry {
			internal int min_width;
			internal int min_height;
			internal int max_width;
			internal int max_height;
			internal int base_width;
			internal int base_height;
			internal int width_inc;
			internal int height_inc;
			internal double min_aspect;
			internal double max_aspect;
			internal GdkGravity win_gravity;
		}
		
		[StructLayout (LayoutKind.Sequential)] 
		internal struct GdkWindowAttr {
			internal string title;
			internal int event_mask;
			internal int x, y;
			internal int width;
			internal int height;
			internal GdkWindowClass wclass;
			internal IntPtr visual;
			internal IntPtr colormap;
			internal GdkWindowType window_type;
			internal IntPtr cursor;
			internal string wmclass_name;
			internal string wmclass_class;
			internal bool override_redirect;
		}
		
		#region Local Variables
		// General
		static volatile XplatUIX11GTK	Instance;
		private static int		RefCount;
		private static object		XlibLock;		// Our locking object
		private static bool		ThemesEnabled;
		
		// General X11
		private static IntPtr		DisplayHandle;		// X11 handle to display
		private static IntPtr		GdkDisplayHandle;	// gdk handle to display
		private static int		ScreenNo;		// Screen number used
		private static IntPtr 		GdkScreen;
		private static IntPtr		DefaultColormap;	// Colormap for screen
		private static IntPtr		GdkDefaultColormap;	// Gdk Colormap for screen
		private static IntPtr		CustomVisual;		// Visual for window creation
		private static IntPtr 		GdkCustomVisual;
		private static IntPtr		CustomColormap;		// Colormap for window creation
		private static IntPtr 		GdkCustomColormap;
		private static int 		VisualBestDepth;
		private static IntPtr		RootWindow;		// Handle of the root window for the screen/display
		private static IntPtr		GdkRootWindow;		// Gdk handle of the root window for the screen/display
		private static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		private static IntPtr		GdkFosterParent;	// Container to hold child windows until their parent exists
		private static XErrorHandler	ErrorHandler;		// Error handler delegate
		private static bool		ErrorExceptions;	// Throw exceptions on X errors
		private static bool		PostQuitState;		// True if we've got an pending exit
		
		// Clipboard
		private static IntPtr 		ClipMagic = new IntPtr(27051977);
		private static ClipboardStruct	Clipboard;		// Our clipboard
		
		// Communication
		private static int		PostAtom;		// PostMessage atom
		private static int		AsyncAtom;		// Support for async messages
		
		// Message Loop
		private static XEventQueue	MessageQueue;		// Holds our queued up events
		private static Pollfd[]		pollfds;		// For watching the X11 socket
		private static X11Keyboard	Keyboard;		//
		private static X11Dnd		Dnd;
		private static Socket		listen;			//
		private static Socket		wake;			//
		private static Socket		wake_receive;		//
		private static byte[]		network_buffer;		//
		
		
		// Focus tracking
		private static IntPtr		ActiveWindow;		// Handle of the active window
		private static IntPtr		FocusWindow;		// Handle of the window with keyboard focus (if any)
		
		// Modality support
		private static Stack		ModalWindows;		// Stack of our modal windows
		
		// Systray
		private static IntPtr		SystrayMgrWindow;	// Handle of the Systray Manager window
		
		// Cursors
		private static IntPtr		LastCursorWindow;	// The last window we set the cursor on
		private static IntPtr		LastCursorHandle;	// The handle that was last set on LastCursorWindow
		private static IntPtr		OverrideCursorHandle;	// The cursor that is set to override any other cursors
		
		// Caret
		private static CaretStruct	Caret;			//
		
		// Support for Window Styles
		private static int[]		NetAtoms;		// All atoms we know
		
		// mouse hover message generation
		private static HoverStruct	HoverState;		//
		
		// double click message generation
		private static ClickStruct	ClickPending;		//
		
		// Support for mouse grab
		private static GrabStruct	Grab;			//
		
		// State
		private static Point		MousePosition;		// Last position of mouse, in screen coords
		internal static MouseButtons	MouseState;		// Last state of mouse buttons
		
		// Timers
		private static ArrayList	TimerList;		// Holds SWF.Timers
		
		// 'Constants'
		private static int		DoubleClickInterval;	// msec; max interval between clicks to count as double click
		
		const GdkEventMask GdkSelectInputMask = GdkEventMask.GDK_BUTTON_PRESS_MASK | 
		GdkEventMask.GDK_BUTTON_RELEASE_MASK | 
		GdkEventMask.GDK_KEY_PRESS_MASK | 
		GdkEventMask.GDK_KEY_RELEASE_MASK | 
		GdkEventMask.GDK_ENTER_NOTIFY_MASK | 
		GdkEventMask.GDK_LEAVE_NOTIFY_MASK |
		GdkEventMask.GDK_EXPOSURE_MASK |
		GdkEventMask.GDK_FOCUS_CHANGE_MASK |
		GdkEventMask.GDK_POINTER_MOTION_MASK | 
		GdkEventMask.GDK_VISIBILITY_NOTIFY_MASK |
		GdkEventMask.GDK_SUBSTRUCTURE_MASK |
		GdkEventMask.GDK_STRUCTURE_MASK;
		
		static readonly object lockobj = new object ();
		
		static Hashtable backing_store = new Hashtable (5);
		
		#endregion	// Local Variables
		#region Constructors
		private XplatUIX11GTK ()
		{
			Console.WriteLine ("XplatUIX11GTK ctor...");
			// Handle singleton stuff first
			RefCount = 0;
			
			// init gdk
			gdk_init_check (IntPtr.Zero, IntPtr.Zero);
			
			// Now regular initialization
			XlibLock = new object ();
			MessageQueue = new XEventQueue ();
			TimerList = new ArrayList ();
			XInitThreads ();
			
			ErrorExceptions = false;
			
			// X11 Initialization
			SetDisplay (gdk_x11_display_get_xdisplay (gdk_display_get_default ()));
			X11DesktopColors.Initialize ();
			
			// Handle any upcoming errors; we re-set it here, X11DesktopColor stuff might have stolen it (gtk does)
			ErrorHandler = new XErrorHandler (HandleError);
			XSetErrorHandler (ErrorHandler);
		}
		#endregion	// Constructors
		
		#region Singleton Specific Code
		public static XplatUIX11GTK GetInstance ()
		{
			lock (lockobj) {
				if (Instance == null) {
					Instance = new XplatUIX11GTK ();
				}
				RefCount++;
			}
			return Instance;
		}
		
		public int Reference {
			get {
				return RefCount;
			}
		}
		#endregion
		
		#region Internal Properties
		internal static IntPtr Display {
			get {
				return DisplayHandle;
			}
			
			set {
				XplatUIX11GTK.GetInstance ().SetDisplay (value);
			}
		}
		
		internal static int Screen {
			get {
				return ScreenNo;
			}
			
			set {
				ScreenNo = value;
			}
		}
		
		internal static IntPtr RootWindowHandle {
			get {
				return RootWindow;
			}
			
			set {
				RootWindow = value;
			}
		}
		
		internal static IntPtr Visual {
			get {
				return CustomVisual;
			}
			
			set {
				CustomVisual = value;
			}
		}
		
		internal static IntPtr ColorMap {
			get {
				return CustomColormap;
			}
			
			set {
				CustomColormap = value;
			}
		}
		#endregion
		
		#region XExceptionClass
		internal class XException : ApplicationException {
			IntPtr		Display;
			IntPtr		ResourceID;
			IntPtr		Serial;
			XRequest	RequestCode;
			byte		ErrorCode;
			byte		MinorCode;
			
			public XException (IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode)
			{
				this.Display = Display;
				this.ResourceID = ResourceID;
				this.Serial = Serial;
				this.RequestCode = RequestCode;
				this.ErrorCode = ErrorCode;
				this.MinorCode = MinorCode;
			}
			
			public override string Message {
				get {
					return GetMessage (Display, ResourceID, Serial, ErrorCode, RequestCode, MinorCode);
				}
			}
			
			public static string GetMessage (IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode)
			{
				StringBuilder	sb;
				string		x_error_text;
				string		error;
				
				sb = new StringBuilder (160);
				XGetErrorText (Display, ErrorCode, sb, sb.Capacity);
				x_error_text = sb.ToString ();
				
				error = String.Format ("\n  Error: {0}\n  Request:     {1:D} ({2})\n  Resource ID: 0x{3:x}\n  Serial:      {4}", x_error_text, RequestCode, RequestCode, ResourceID.ToInt32 (), Serial);
				return error;
			}
		}
		#endregion	// XExceptionClass
		
		#region Internal Methods
		// native X display handle
		internal void SetDisplay (IntPtr display_handle)
		{
			if (display_handle != IntPtr.Zero) {
				Hwnd	hwnd;
				
				if ((GdkDisplayHandle != IntPtr.Zero) && (GdkFosterParent != IntPtr.Zero)) {
					hwnd = Hwnd.ObjectFromHandle (gdk_x11_drawable_get_xid (GdkFosterParent));
					gdk_window_destroy (GdkFosterParent);
					hwnd.Dispose ();
				}
				
				if (GdkDisplayHandle != IntPtr.Zero) {
					gdk_display_close (GdkDisplayHandle);
				}
				
				DisplayHandle = display_handle;
				GdkDisplayHandle = gdk_x11_lookup_xdisplay (display_handle);
				
				// We need to tell System.Drawing our DisplayHandle. FromHdcInternal has
				// been hacked to do this for us.
				Graphics.FromHdcInternal (DisplayHandle);
				
				// Debugging support
				if (Environment.GetEnvironmentVariable ("MONO_XSYNC") != null) {
					XSynchronize (DisplayHandle, true);					
				}
				
				if (Environment.GetEnvironmentVariable ("MONO_XEXCEPTIONS") != null) {
					ErrorExceptions = true;
				}
				
				// Generic X11 setup
				GdkScreen = gdk_screen_get_default ();
				// or gdk_x11_get_default_screen
				ScreenNo = gdk_screen_get_number (GdkScreen);
				GdkRootWindow = gdk_get_default_root_window ();
				RootWindow = gdk_x11_drawable_get_xid (GdkRootWindow);
				GdkDefaultColormap = gdk_colormap_get_system ();
				DefaultColormap = gdk_x11_colormap_get_xcolormap (GdkDefaultColormap);
				
				VisualBestDepth = gdk_visual_get_best_depth ();
				//Console.WriteLine (VisualBestDepth);
				
				// Create the foster parent
				FosterParent = XCreateSimpleWindow (DisplayHandle, RootWindow, 0, 0, 1, 1, 4, 0, 0);
				GdkFosterParent = gdk_window_foreign_new (FosterParent);
				
				if (GdkFosterParent == IntPtr.Zero) {
					Console.WriteLine ("XplatUIX11GTK Constructor failed to create FosterParent");
				}
				
				hwnd = new Hwnd ();
				
				hwnd.WholeWindow = FosterParent;
				hwnd.ClientWindow = FosterParent;
				
				// For sleeping on the X11 socket
				listen = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
				IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 0);
				listen.Bind (ep);
				listen.Listen (1);
				
				// To wake up when a timer is ready
				network_buffer = new byte [10];
				
				wake = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
				wake.Connect (listen.LocalEndPoint);
				wake_receive = listen.Accept ();
				
				pollfds = new Pollfd [2];
				pollfds [0] = new Pollfd ();
				pollfds [0].fd = XConnectionNumber (DisplayHandle);
				pollfds [0].events = PollEvents.POLLIN;

				pollfds [1] = new Pollfd ();
				pollfds [1].fd = wake_receive.Handle.ToInt32 ();
				pollfds [1].events = PollEvents.POLLIN;
				
				Keyboard = new X11Keyboard (DisplayHandle);
				Dnd = new X11Dnd (DisplayHandle);
				
				PostQuitState = false;
				
				DoubleClickInterval = 500;
				
				HoverState.Interval = 500;
				HoverState.Timer = new Timer ();
				HoverState.Timer.Enabled = false;
				HoverState.Timer.Interval = HoverState.Interval;
				HoverState.Timer.Tick += new EventHandler (MouseHover);
				HoverState.X = -1;
				HoverState.Y = -1;
				
				ActiveWindow = IntPtr.Zero;
				FocusWindow = IntPtr.Zero;
				ModalWindows = new Stack (3);
				
				MouseState = MouseButtons.None;
				MousePosition = new Point (0, 0);
				
				Caret.Timer = new Timer ();
				Caret.Timer.Interval = 500;		// FIXME - where should this number come from?
				Caret.Timer.Tick += new EventHandler (CaretCallback);
				
				SetupAtoms ();
				
				// Grab atom changes off the root window to catch certain WM events
				gdk_window_set_events (GdkRootWindow, (int)GdkEventMask.GDK_PROPERTY_CHANGE_MASK);
				
				// Handle any upcoming errors
				ErrorHandler = new XErrorHandler (HandleError);
				XSetErrorHandler (ErrorHandler);
			} else {
				throw new ArgumentNullException ("Display", "Could not open display (X-Server required. Check your DISPLAY environment variable)");
			}
		}
		#endregion	// Internal Methods
		
		#region Private Methods
		private static void SetupAtoms ()
		{
			NetAtoms = new int [(int)NA.LAST_NET_ATOM];
			
			NetAtoms [(int)NA.WM_PROTOCOLS] = XInternAtom (DisplayHandle, "WM_PROTOCOLS", false);
			NetAtoms [(int)NA.WM_DELETE_WINDOW] = XInternAtom (DisplayHandle, "WM_DELETE_WINDOW", false);
			NetAtoms [(int)NA.WM_TAKE_FOCUS] = XInternAtom (DisplayHandle, "WM_TAKE_FOCUS", false);
			
			NetAtoms [(int)NA._NET_SUPPORTED] = XInternAtom (DisplayHandle, "_NET_SUPPORTED", false);
			NetAtoms [(int)NA._NET_CLIENT_LIST] = XInternAtom (DisplayHandle, "_NET_CLIENT_LIST", false);
			NetAtoms [(int)NA._NET_NUMBER_OF_DESKTOPS] = XInternAtom (DisplayHandle, "_NET_NUMBER_OF_DESKTOPS", false);
			NetAtoms [(int)NA._NET_DESKTOP_GEOMETRY] = XInternAtom (DisplayHandle, "_NET_DESKTOP_GEOMETRY", false);
			NetAtoms [(int)NA._NET_DESKTOP_VIEWPORT] = XInternAtom (DisplayHandle, "_NET_DESKTOP_VIEWPORT", false);
			NetAtoms [(int)NA._NET_CURRENT_DESKTOP] = XInternAtom (DisplayHandle, "_NET_CURRENT_DESKTOP", false);
			NetAtoms [(int)NA._NET_DESKTOP_NAMES] = XInternAtom (DisplayHandle, "_NET_DESKTOP_NAMES", false);
			NetAtoms [(int)NA._NET_ACTIVE_WINDOW] = XInternAtom (DisplayHandle, "_NET_ACTIVE_WINDOW", false);
			NetAtoms [(int)NA._NET_WORKAREA] = XInternAtom (DisplayHandle, "_NET_WORKAREA", false);
			NetAtoms [(int)NA._NET_SUPPORTING_WM_CHECK] = XInternAtom (DisplayHandle, "_NET_SUPPORTING_WM_CHECK", false);
			NetAtoms [(int)NA._NET_VIRTUAL_ROOTS] = XInternAtom (DisplayHandle, "_NET_VIRTUAL_ROOTS", false);
			NetAtoms [(int)NA._NET_DESKTOP_LAYOUT] = XInternAtom (DisplayHandle, "_NET_DESKTOP_LAYOUT", false);
			NetAtoms [(int)NA._NET_SHOWING_DESKTOP] = XInternAtom (DisplayHandle, "_NET_SHOWING_DESKTOP", false);
			
			NetAtoms [(int)NA._NET_CLOSE_WINDOW] = XInternAtom (DisplayHandle, "_NET_CLOSE_WINDOW", false);
			NetAtoms [(int)NA._NET_MOVERESIZE_WINDOW] = XInternAtom (DisplayHandle, "_NET_MOVERESIZE_WINDOW", false);
			NetAtoms [(int)NA._NET_WM_MOVERESIZE] = XInternAtom (DisplayHandle, "_NET_WM_MOVERESIZE", false);
			NetAtoms [(int)NA._NET_RESTACK_WINDOW] = XInternAtom (DisplayHandle, "_NET_RESTACK_WINDOW", false);
			NetAtoms [(int)NA._NET_REQUEST_FRAME_EXTENTS] = XInternAtom (DisplayHandle, "_NET_REQUEST_FRAME_EXTENTS", false);
			
			NetAtoms [(int)NA._NET_WM_NAME] = XInternAtom (DisplayHandle, "_NET_WM_NAME", false);
			NetAtoms [(int)NA._NET_WM_VISIBLE_NAME] = XInternAtom (DisplayHandle, "_NET_WM_VISIBLE_NAME", false);
			NetAtoms [(int)NA._NET_WM_ICON_NAME] = XInternAtom (DisplayHandle, "_NET_WM_ICON_NAME", false);
			NetAtoms [(int)NA._NET_WM_VISIBLE_ICON_NAME] = XInternAtom (DisplayHandle, "_NET_WM_VISIBLE_ICON_NAME", false);
			NetAtoms [(int)NA._NET_WM_DESKTOP] = XInternAtom (DisplayHandle, "_NET_WM_DESKTOP", false);
			NetAtoms [(int)NA._NET_WM_WINDOW_TYPE] = XInternAtom (DisplayHandle, "_NET_WM_WINDOW_TYPE", false);
			NetAtoms [(int)NA._NET_WM_STATE] = XInternAtom (DisplayHandle, "_NET_WM_STATE", false);
			NetAtoms [(int)NA._NET_WM_ALLOWED_ACTIONS] = XInternAtom (DisplayHandle, "_NET_WM_ALLOWED_ACTIONS", false);
			NetAtoms [(int)NA._NET_WM_STRUT] = XInternAtom (DisplayHandle, "_NET_WM_STRUT", false);
			NetAtoms [(int)NA._NET_WM_STRUT_PARTIAL] = XInternAtom (DisplayHandle, "_NET_WM_STRUT_PARTIAL", false);
			NetAtoms [(int)NA._NET_WM_ICON_GEOMETRY] = XInternAtom (DisplayHandle, "_NET_WM_ICON_GEOMETRY", false);
			NetAtoms [(int)NA._NET_WM_ICON] = XInternAtom (DisplayHandle, "_NET_WM_ICON", false);
			NetAtoms [(int)NA._NET_WM_PID] = XInternAtom (DisplayHandle, "_NET_WM_PID", false);
			NetAtoms [(int)NA._NET_WM_HANDLED_ICONS] = XInternAtom (DisplayHandle, "_NET_WM_HANDLED_ICONS", false);
			NetAtoms [(int)NA._NET_WM_USER_TIME] = XInternAtom (DisplayHandle, "_NET_WM_USER_TIME", false);
			NetAtoms [(int)NA._NET_FRAME_EXTENTS] = XInternAtom (DisplayHandle, "_NET_FRAME_EXTENTS", false);
			
			NetAtoms [(int)NA._NET_WM_PING] = XInternAtom (DisplayHandle, "_NET_WM_PING", false);
			NetAtoms [(int)NA._NET_WM_SYNC_REQUEST] = XInternAtom (DisplayHandle, "_NET_WM_SYNC_REQUEST", false);
			
			NetAtoms [(int)NA._NET_SYSTEM_TRAY_S] = XInternAtom (DisplayHandle, "_NET_SYSTEM_TRAY_S" + ScreenNo.ToString (), false);
			NetAtoms [(int)NA._NET_SYSTEM_TRAY_OPCODE] = XInternAtom (DisplayHandle, "_NET_SYSTEM_TRAY_OPCODE", false);
			NetAtoms [(int)NA._NET_SYSTEM_TRAY_ORIENTATION] = XInternAtom (DisplayHandle, "_NET_SYSTEM_TRAY_ORIENTATION", false);
			
			NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_HORZ] = XInternAtom (DisplayHandle, "_NET_WM_STATE_MAXIMIZED_HORZ", false);
			NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_VERT] = XInternAtom (DisplayHandle, "_NET_WM_STATE_MAXIMIZED_VERT", false);
			NetAtoms [(int)NA._NET_WM_STATE_HIDDEN] = XInternAtom (DisplayHandle, "_NET_WM_STATE_HIDDEN", false);
			
			NetAtoms [(int)NA._XEMBED] = XInternAtom (DisplayHandle, "_XEMBED", false);
			NetAtoms [(int)NA._XEMBED_INFO] = XInternAtom (DisplayHandle, "_XEMBED_INFO", false);
			
			NetAtoms [(int)NA._MOTIF_WM_HINTS] = XInternAtom (DisplayHandle, "_MOTIF_WM_HINTS", false);
			
			NetAtoms [(int)NA._NET_WM_STATE_NO_TASKBAR] = XInternAtom (DisplayHandle, "_NET_WM_STATE_NO_TASKBAR", false);
			NetAtoms [(int)NA._NET_WM_STATE_ABOVE] = XInternAtom (DisplayHandle, "_NET_WM_STATE_ABOVE", false);
			NetAtoms [(int)NA._NET_WM_STATE_MODAL] = XInternAtom (DisplayHandle, "_NET_WM_STATE_MODAL", false);
			NetAtoms [(int)NA._NET_WM_CONTEXT_HELP] = XInternAtom (DisplayHandle, "_NET_WM_CONTEXT_HELP", false);
			NetAtoms [(int)NA._NET_WM_WINDOW_OPACITY] = XInternAtom (DisplayHandle, "_NET_WM_WINDOW_OPACITY", false);
			
			// Clipboard support
			NetAtoms [(int)NA.CLIPBOARD] = XInternAtom (DisplayHandle, "CLIPBOARD", false);
			NetAtoms [(int)NA.DIB] = (int)Atom.XA_PIXMAP;
			NetAtoms [(int)NA.OEMTEXT] = XInternAtom (DisplayHandle, "COMPOUND_TEXT", false);
			NetAtoms [(int)NA.UNICODETEXT] = XInternAtom (DisplayHandle, "UTF8_STRING", false);
			NetAtoms [(int)NA.TARGETS] = XInternAtom (DisplayHandle, "TARGETS", false);
			
			// Special Atoms
			AsyncAtom = XInternAtom (DisplayHandle, "_SWF_AsyncAtom", false);
			PostAtom = XInternAtom (DisplayHandle, "_SWF_PostMessageAtom", false);
			HoverState.Atom = XInternAtom (DisplayHandle, "_SWF_HoverAtom", false);
		}
		
		private void GetSystrayManagerWindow ()
		{
			gdk_x11_grab_server ();
			SystrayMgrWindow = XGetSelectionOwner (DisplayHandle, NetAtoms [(int)NA._NET_SYSTEM_TRAY_S]);
			gdk_x11_ungrab_server ();
			gdk_display_flush (GdkDisplayHandle);
		}
		
		private void SendNetWMMessage (IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2)
		{
			XEvent	xev;
			
			xev = new XEvent ();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;
			XSendEvent (DisplayHandle, RootWindow, false, EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask, ref xev);
		}
		
		private void SendNetClientMessage (IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2)
		{
			XEvent	xev;
			
			xev = new XEvent ();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;
			XSendEvent (DisplayHandle, window, false, EventMask.NoEventMask, ref xev);
		}
		
		private void DeriveStyles (IntPtr handle, int Style, int ExStyle, out FormBorderStyle border_style, out TitleStyle title_style, out int caption_height, out int tool_caption_height)
		{
			
			// Only MDI windows get caption_heights
			caption_height = 0;
			tool_caption_height = 19;
			
			if ((Style & (int) WindowStyles.WS_CHILD) != 0) {
				if ((Style & (int) WindowStyles.WS_BORDER) == 0) {
					border_style = FormBorderStyle.None;
				} else if ((ExStyle & (int) WindowStyles.WS_EX_CLIENTEDGE) != 0) {
					border_style = FormBorderStyle.Fixed3D;
				} else {
					border_style = FormBorderStyle.FixedSingle;
				}
				title_style = TitleStyle.None;
			} else {
				bool is_mdi = false;
				
				if ((ExStyle & (int) WindowStyles.WS_EX_MDICHILD) != 0) {
					caption_height = 26;
					is_mdi = true;
				}
				
				title_style = TitleStyle.None;
				if ((Style & (int)WindowStyles.WS_CAPTION) != 0) {
					if ((ExStyle & (int)WindowStyles.WS_EX_TOOLWINDOW) != 0) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}
				
				if (!is_mdi) {
					border_style = FormBorderStyle.None;
					
					if ((Style & (int)WindowStyles.WS_THICKFRAME) != 0) {
						if ((ExStyle & (int)WindowStyles.WS_EX_TOOLWINDOW) != 0) {
							border_style = FormBorderStyle.SizableToolWindow;
						} else {
							border_style = FormBorderStyle.Sizable;
						}
					} else {
						if ((ExStyle & (int)WindowStyles.WS_EX_CLIENTEDGE) != 0) {
							border_style = FormBorderStyle.Fixed3D;
						} else if ((ExStyle & (int)WindowStyles.WS_EX_DLGMODALFRAME) != 0) {
							border_style = FormBorderStyle.FixedDialog;
						} else if ((ExStyle & (int)WindowStyles.WS_EX_TOOLWINDOW) != 0) {
							border_style = FormBorderStyle.FixedToolWindow;
						} else if ((Style & (int)WindowStyles.WS_BORDER) != 0) {
							border_style = FormBorderStyle.Sizable;
						} else {
							border_style = FormBorderStyle.None;
						}
					}
				} else {
					if ((Style & (int) WindowStyles.WS_OVERLAPPEDWINDOW) != 0 ||
					    (ExStyle & (int) WindowStyles.WS_EX_TOOLWINDOW) != 0) {
						border_style = (FormBorderStyle) 0xFFFF;
					} else {
						border_style = FormBorderStyle.None;
					}
				}
			}
		}
		
		private void SetHwndStyles (Hwnd hwnd, CreateParams cp)
		{
			DeriveStyles (hwnd.Handle, cp.Style, cp.ExStyle, out hwnd.border_style, out hwnd.title_style, out hwnd.caption_height, out hwnd.tool_caption_height);
		}
		
		private void SetWMStyles (Hwnd hwnd, CreateParams cp)
		{
			GdkWMDecoration decorations = GdkWMDecoration.GDK_DECOR_ALL;
			
			if ((cp.Style & (int)WindowStyles.WS_CAPTION) != 0) {
				decorations |= GdkWMDecoration.GDK_DECOR_TITLE | GdkWMDecoration.GDK_DECOR_MENU;
			}
			
			if ((cp.Style & ((int)WindowStyles.WS_THICKFRAME)) != 0) {
				decorations |= GdkWMDecoration.GDK_DECOR_BORDER | GdkWMDecoration.GDK_DECOR_RESIZEH;
			}
			if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) {
				decorations |= GdkWMDecoration.GDK_DECOR_MINIMIZE;
			}
			
			if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
				decorations |= GdkWMDecoration.GDK_DECOR_MAXIMIZE;
			}
			
			// is this needed ? most window managers do not even honour any MotifFunctions...
//			if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
//				functions |= MotifFunctions.Close;
//			}
			
			if ((cp.ExStyle & ((int)WindowStyles.WS_EX_DLGMODALFRAME)) != 0) {
				decorations |= GdkWMDecoration.GDK_DECOR_BORDER;
			}
			
			if ((cp.Style & ((int)WindowStyles.WS_DLGFRAME)) != 0) {
				decorations |= GdkWMDecoration.GDK_DECOR_BORDER;
			}
			
			if ((cp.Style & ((int)WindowStyles.WS_BORDER)) != 0) {
				decorations |= GdkWMDecoration.GDK_DECOR_BORDER;
			}
			
			if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
				decorations = 0;
			}
			
			gdk_window_set_decorations (gdk_window_foreign_new (hwnd.whole_window), (int)decorations);
		}
		
		private void SetIcon (Hwnd hwnd, Icon icon)
		{
			Bitmap		bitmap;
			int		size;
			uint[]		data;
			int		index;
			
			bitmap = icon.ToBitmap ();
			index = 0;
			size = bitmap.Width * bitmap.Height + 2;
			data = new uint [size];
			
			data [index++] = (uint)bitmap.Width;
			data [index++] = (uint)bitmap.Height;
			
			for (int y = 0; y < bitmap.Height; y++) {
				for (int x = 0; x < bitmap.Width; x++) {
					data [index++] = (uint)bitmap.GetPixel (x, y).ToArgb ();
				}
			}
			XChangeProperty (DisplayHandle, hwnd.whole_window, NetAtoms [(int)NA._NET_WM_ICON], Atom.XA_CARDINAL, 32, PropertyMode.Replace, data, size);
		}
		
		private IntPtr ImageToPixmap (Image image)
		{
			return IntPtr.Zero;
		}
		
		private void WakeupMain ()
		{
			wake.Send (new byte    [] { 0xFF });
		}
		
		private void TranslatePropertyToClipboard (int property)
		{
			Atom			actual_atom;
			int			actual_format;
			int			nitems;
			int			bytes_after;
			IntPtr			prop = IntPtr.Zero;
			
			Clipboard.Item = null;
			
			XGetWindowProperty (DisplayHandle, FosterParent, property, 0, 0x7fffffff, true, Atom.AnyPropertyType, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			
			if (nitems > 0) {
				if (property == (int)Atom.XA_STRING) {
					Clipboard.Item = Marshal.PtrToStringAnsi (prop);
				} else if (property == (int)Atom.XA_BITMAP) {
					// FIXME - convert bitmap to image
				} else if (property == (int)Atom.XA_PIXMAP) {
					// FIXME - convert pixmap to image
				} else if (property == NetAtoms [(int)NA.OEMTEXT]) {
					Clipboard.Item = Marshal.PtrToStringAnsi (prop);
				} else if (property == NetAtoms [(int)NA.UNICODETEXT]) {
					Clipboard.Item = Marshal.PtrToStringAnsi (prop);
				}
				
				XFree (prop);
			}
		}
		
		private void AddExpose (XEvent xevent)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.GetObjectFromWindow (xevent.AnyEvent.window);
			
			// Don't waste time
			if (hwnd == null) {	
				return;
			}
			
			if (xevent.AnyEvent.window == hwnd.client_window) {
				hwnd.AddInvalidArea (xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
				if (!hwnd.expose_pending) {
					MessageQueue.Enqueue (xevent);
					hwnd.expose_pending = true;
				}
			} else {
				if (!hwnd.nc_expose_pending) {
					MessageQueue.Enqueue (xevent);
					hwnd.nc_expose_pending = true;
				}
			}
		}
		
		private void InvalidateWholeWindow (IntPtr handle)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			InvalidateWholeWindow (handle, new Rectangle (0, 0, hwnd.Width, hwnd.Height));
		}
		
		private void InvalidateWholeWindow (IntPtr handle, Rectangle rectangle)
		{
			Hwnd	hwnd;
			XEvent	xevent;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			
			xevent = new XEvent ();
			xevent.type = XEventName.Expose;
			xevent.ExposeEvent.display = DisplayHandle;
			xevent.ExposeEvent.window = hwnd.whole_window;
			
			xevent.ExposeEvent.x = rectangle.X;
			xevent.ExposeEvent.y = rectangle.Y;
			xevent.ExposeEvent.width = rectangle.Width;
			xevent.ExposeEvent.height = rectangle.Height;
			
			AddExpose (xevent);
		}
		
		private void WholeToScreen (IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			lock (XlibLock) {
				gdk_window_get_origin (gdk_window_lookup (hwnd.whole_window), out dest_x_return, out dest_y_return);
			}
			
			x = dest_x_return;
			y = dest_y_return;
		}
		
		private void AddConfigureNotify (XEvent xevent)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.GetObjectFromWindow (xevent.ConfigureEvent.window);
			
			// Don't waste time
			if (hwnd == null) {
				return;
			}
			
			if (xevent.ConfigureEvent.window == hwnd.whole_window) {
				if (!hwnd.reparented) {
					hwnd.x = xevent.ConfigureEvent.x;
					hwnd.y = xevent.ConfigureEvent.y;
				} else {
					int	dummy_int;
					
					gdk_window_get_geometry (gdk_window_lookup (hwnd.whole_window), out hwnd.x, out hwnd.y, out dummy_int, out dummy_int, out dummy_int);
				}
				
				hwnd.width = xevent.ConfigureEvent.width;
				hwnd.height = xevent.ConfigureEvent.height;
				
				if (!hwnd.configure_pending) {
					MessageQueue.Enqueue (xevent);
					hwnd.configure_pending = true;
				}
			}
			// We drop configure events for Client windows
		}
		
		private void ShowCaret ()
		{
			if ((Caret.gc == IntPtr.Zero) || Caret.On) {
				return;
			}
			Caret.On = true;
			
			// gdk_gc_set_foreground
			// gdk_draw_line
			lock (XlibLock) {
				XDrawLine (DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}
		
		private void HideCaret ()
		{
			if ((Caret.gc == IntPtr.Zero) || !Caret.On) {
				return;
			}
			Caret.On = false;
			
			// gdk_gc_set_foreground
			// gdk_draw_text_wc
			lock (XlibLock) {
				XDrawLine (DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}
		
		private int NextTimeout (DateTime now)
		{
			int timeout = Int32.MaxValue; 
			lock (TimerList) {
				foreach (Timer timer in TimerList) {
					int next = (int) (timer.Expires - now).TotalMilliseconds;
					if (next < 0) {
						return 0; // Have a timer that has already expired
					}
					
					if (next < timeout) {
						timeout = next;
					}
				}
			}
			if (timeout < Timer.Minimum) {
				timeout = Timer.Minimum;
			}
			
			return timeout;
		}
		
		private void CheckTimers (DateTime now)
		{
			lock (TimerList) {
				int count;
				
				count = TimerList.Count;
				
				if (count == 0) {
					return;
				}
				
				for (int i = 0; i < TimerList.Count; i++) {
					Timer timer;
					
					timer = (Timer) TimerList [i];
					
					if (timer.Enabled && timer.Expires <= now) {
						timer.Update (now);
						timer.FireTick ();
					}
				}
			}
		}
		
		private void UpdateMessageQueue ()
		{
			DateTime	now;
			int		pending;
			
			now = DateTime.Now;
			
			lock (XlibLock) {
				pending = XPending (DisplayHandle);
			}
			
			if (pending == 0) {
				if (Idle != null) {
					Idle (this, EventArgs.Empty);
				}
				
				lock (XlibLock) {
					pending = XPending (DisplayHandle);
				}
			}
			
			if (pending == 0) {
				int	timeout;
				
				timeout = NextTimeout (now);
				if (timeout > 0) {
					Syscall.poll (pollfds, (uint) pollfds.Length, timeout);
					// Clean out buffer, so we're not busy-looping on the same data
					if (pollfds[1].revents != 0) {
						wake_receive.Receive(network_buffer, 0, 1, SocketFlags.None);
					}
					lock (XlibLock) {
						pending = XPending (DisplayHandle);
					}
				}
			}
			
			CheckTimers (now);
			
			if (pending == 0) {
				lock (XlibLock) {
					pending = XPending (DisplayHandle);
				}
			}
			
			while (pending > 0) {
				XEvent xevent = new XEvent ();
				
				lock (XlibLock) {
					XNextEvent (DisplayHandle, ref xevent);
				}
//Console.WriteLine("Got x event {0}", xevent);
				switch (xevent.type) {
				case XEventName.Expose:
					AddExpose (xevent);
					break;
					
				case XEventName.SelectionClear: {
						// Should we do something?
						break;
					}
					
				case XEventName.SelectionRequest: {
						if (Dnd.HandleSelectionRequestEvent (ref xevent))
							break;
						XEvent sel_event;
						
						sel_event = new XEvent ();
						sel_event.SelectionEvent.type = XEventName.SelectionNotify;
						sel_event.SelectionEvent.send_event = true;
						sel_event.SelectionEvent.display = DisplayHandle;
						sel_event.SelectionEvent.selection = xevent.SelectionRequestEvent.selection;
						sel_event.SelectionEvent.target = xevent.SelectionRequestEvent.target;
						sel_event.SelectionEvent.requestor = xevent.SelectionRequestEvent.requestor;
						sel_event.SelectionEvent.time = xevent.SelectionRequestEvent.time;
						sel_event.SelectionEvent.property = 0;
						
						// Seems that some apps support asking for supported types
						if (xevent.SelectionEvent.target == NetAtoms [(int)NA.TARGETS]) {
							uint[]	atoms;
							int	atom_count;
							
							atoms = new uint [5];
							atom_count = 0;
							
							if (Clipboard.Item is String) {
								atoms [atom_count++] = (uint)Atom.XA_STRING;
								atoms [atom_count++] = (uint)NetAtoms [(int)NA.OEMTEXT];
								atoms [atom_count++] = (uint)NetAtoms [(int)NA.UNICODETEXT];
							} else if (Clipboard.Item is Image) {
								atoms [atom_count++] = (uint)Atom.XA_PIXMAP;
								atoms [atom_count++] = (uint)Atom.XA_BITMAP;
							} else {
								// FIXME - handle other types
							}
							
							XChangeProperty (DisplayHandle, xevent.SelectionEvent.requestor, xevent.SelectionRequestEvent.property, xevent.SelectionRequestEvent.target, 32, PropertyMode.Replace, atoms, atom_count);
						} else if (Clipboard.Item is string) {
							IntPtr	buffer;
							int	buflen;
							
							buflen = 0;
							
							if (xevent.SelectionRequestEvent.target == (int)Atom.XA_STRING) {
								Byte[] bytes;
								
								bytes = new ASCIIEncoding ().GetBytes ((string)Clipboard.Item);
								buffer = Marshal.AllocHGlobal (bytes.Length);
								buflen = bytes.Length;
								
								for (int i = 0; i < buflen; i++) {
									Marshal.WriteByte (buffer, i, bytes [i]);
								}
							} else if (xevent.SelectionRequestEvent.target == NetAtoms [(int)NA.OEMTEXT]) {
								// FIXME - this should encode into ISO2022
								buffer = Marshal.StringToHGlobalAnsi ((string)Clipboard.Item);
								while (Marshal.ReadByte (buffer, buflen) != 0) {
									buflen++;
								}
							} else if (xevent.SelectionRequestEvent.target == NetAtoms [(int)NA.UNICODETEXT]) {
								buffer = Marshal.StringToHGlobalAnsi ((string)Clipboard.Item);
								while (Marshal.ReadByte (buffer, buflen) != 0) {
									buflen++;
								}
							} else {
								buffer = IntPtr.Zero;
							}
							
							if (buffer != IntPtr.Zero) {
								XChangeProperty (DisplayHandle, xevent.SelectionRequestEvent.requestor, xevent.SelectionRequestEvent.property, xevent.SelectionRequestEvent.target, 8, PropertyMode.Replace, buffer, buflen);
								sel_event.SelectionEvent.property = xevent.SelectionRequestEvent.property;
								Marshal.FreeHGlobal (buffer);
							}
						} else if (Clipboard.Item is Image) {
							if (xevent.SelectionEvent.target == (int)Atom.XA_PIXMAP) {
								// FIXME - convert image and store as property
							} else if (xevent.SelectionEvent.target == (int)Atom.XA_PIXMAP) {
								// FIXME - convert image and store as property
							}
						}
						
						XSendEvent (DisplayHandle, xevent.SelectionRequestEvent.requestor, false, EventMask.NoEventMask, ref sel_event);
						break;
					}
					
				case XEventName.SelectionNotify: {
						if (Clipboard.Enumerating) {
							Clipboard.Enumerating = false;
							if (xevent.SelectionEvent.property != 0) {
								XDeleteProperty (DisplayHandle, FosterParent, xevent.SelectionEvent.property);
								if (!Clipboard.Formats.Contains (xevent.SelectionEvent.property)) {
									Clipboard.Formats.Add (xevent.SelectionEvent.property);
									#if DriverDebugExtra
										Console.WriteLine("Got supported clipboard atom format: {0}", xevent.SelectionEvent.property);
									#endif
								}
							}
						} else if (Clipboard.Retrieving) {
							Clipboard.Retrieving = false;
							if (xevent.SelectionEvent.property != 0) {
								TranslatePropertyToClipboard (xevent.SelectionEvent.property);
							} else {
								Clipboard.Item = null;
							}
						} else {
							Dnd.HandleSelectionNotifyEvent (ref xevent);
						}
						break;
					}
					
				case XEventName.KeyPress:
				case XEventName.KeyRelease:
				case XEventName.ButtonPress:
				case XEventName.ButtonRelease:
				case XEventName.MotionNotify:
				case XEventName.EnterNotify:
				case XEventName.LeaveNotify:
				case XEventName.CreateNotify:
				case XEventName.DestroyNotify:
				case XEventName.FocusIn:
				case XEventName.FocusOut:
				case XEventName.ClientMessage:
				case XEventName.ReparentNotify:
					MessageQueue.Enqueue (xevent);
					break;
					
				case XEventName.ConfigureNotify:
					AddConfigureNotify (xevent);
					break;
					
				case XEventName.PropertyNotify:
					if (xevent.PropertyEvent.atom == NetAtoms [(int)NA._NET_ACTIVE_WINDOW]) {
						Atom	actual_atom;
						int	actual_format;
						int	nitems;
						int	bytes_after;
						IntPtr	prop = IntPtr.Zero;
						IntPtr	prev_active;;
						
						prev_active = ActiveWindow;
						XGetWindowProperty (DisplayHandle, RootWindow, NetAtoms [(int)NA._NET_ACTIVE_WINDOW], 0, 1, false, Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
						if ((nitems > 0) && (prop != IntPtr.Zero)) {
							ActiveWindow = Hwnd.GetHandleFromWindow ((IntPtr)Marshal.ReadInt32 (prop));
							XFree (prop);
							
							if (prev_active != ActiveWindow) {
								if (prev_active != IntPtr.Zero) {
									PostMessage (prev_active, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
								}
								if (ActiveWindow != IntPtr.Zero) {
									PostMessage (ActiveWindow, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
								}
							}
							if (ModalWindows.Count == 0) {
								break;
							} else {
								// Modality handling, if we are modal and the new active window is one
								// of ours but not the modal one, switch back to the modal window
								
								if (NativeWindow.FromHandle (ActiveWindow) != null) {
									if (ActiveWindow != (IntPtr)ModalWindows.Peek ()) {
										Activate ((IntPtr)ModalWindows.Peek ());
									}
								}
								break;
							}
						}
					}
					break;
					
				}
				
				lock (XlibLock) {
					pending = XPending (DisplayHandle);
				}
			}
		}
		
		private IntPtr GetMousewParam (int Delta)
		{
			int	result = 0;
			
			if ((MouseState & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}
			
			if ((MouseState & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}
			
			if ((MouseState & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}
			
			Keys mods = ModifierKeys;
			if ((mods & Keys.Control) != 0) {
				result |= (int)MsgButtons.MK_CONTROL;
			}
			
			if ((mods & Keys.Shift) != 0) {
				result |= (int)MsgButtons.MK_SHIFT;
			}
			
			result |= Delta << 16;
			
			return (IntPtr)result;
		}
		private IntPtr XGetParent (IntPtr handle)
		{
			return gdk_x11_drawable_get_xid (gdk_window_get_parent (gdk_window_lookup (handle)));
		}
		
		private int HandleError (IntPtr display, ref XErrorEvent error_event)
		{
			if (ErrorExceptions) {
				throw new XException (error_event.display, error_event.resourceid, error_event.serial, error_event.error_code, error_event.request_code, error_event.minor_code);
			} else {
				Console.WriteLine ("X11 Error encountered: {0}{1}\n", XException.GetMessage (error_event.display, error_event.resourceid, error_event.serial, error_event.error_code, error_event.request_code, error_event.minor_code), Environment.StackTrace);
			}
			return 0;
		}
		
		private void DestroyChildWindow (Control c)
		{
			Hwnd		hwnd;
			int		i;
			Control[]	controls;
			
			if (c != null) {
				controls = c.Controls.GetAllControls ();
				
				for (i = 0; i < controls.Length; i++) {
					if (controls [i].IsHandleCreated) {
						hwnd = Hwnd.ObjectFromHandle (controls [i].Handle);
						SendMessage (controls [i].Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
						hwnd.Dispose ();
					}
					DestroyChildWindow (controls [i]);
				}
			}
		}
		
		#endregion	// Private Methods
		
		#region	Callbacks
		private void MouseHover (object sender, EventArgs e)
		{
			if ((HoverState.X == MousePosition.X) && (HoverState.Y == MousePosition.Y)) {
				XEvent xevent;
				
				HoverState.Timer.Enabled = false;
				
				if (HoverState.Window != IntPtr.Zero) {
					xevent = new XEvent ();
					
					xevent.type = XEventName.ClientMessage;
					xevent.ClientMessageEvent.display = DisplayHandle;
					xevent.ClientMessageEvent.window = (IntPtr)HoverState.Window;
					xevent.ClientMessageEvent.message_type = (IntPtr)HoverState.Atom;
					xevent.ClientMessageEvent.format = 32;
					xevent.ClientMessageEvent.ptr1 = (IntPtr) (HoverState.Y << 16 | HoverState.X);
					
					MessageQueue.EnqueueLocked (xevent);
					
					WakeupMain ();
				}
			}
		}
		
		private void CaretCallback (object sender, EventArgs e)
		{
			if (Caret.Paused) {
				return;
			}
			Caret.On = !Caret.On;
			
			XDrawLine (DisplayHandle, Caret.Hwnd, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
		}
		#endregion	// Callbacks
		
		#region Public Properties
		
		internal override int Caption {
			get {
				return 25; 
			}
		}
		
		internal override Size CursorSize {
			get {
				uint	x;
				uint	y;
				gdk_display_get_maximal_cursor_size (GdkDisplayHandle, out x, out y);
				
				return new Size ((int)x, (int)y);
			}
		} 
		
		internal override  bool DragFullWindows {
			get {
				return true;
			}
		} 
		
		internal override  Size DragSize {
			get {
				return new Size (4, 4);
			}
		} 
		
		internal override  Size FrameBorderSize { 
			get {
				throw new NotImplementedException (); 
			}
		}
		
		internal override  Size IconSize {
			get {
				IntPtr		list;
				XIconSize	size;
				int		count;
				
				if (XGetIconSizes (DisplayHandle, RootWindow, out list, out count) != 0) {
					long		current;
					int		largest;
					
					current = (long)list;
					largest = 0;
					
					size = new XIconSize ();
					
					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure ((IntPtr)current, size.GetType ());
						current += Marshal.SizeOf (size);
						
						// Look for our preferred size
						if (size.min_width == 32) {
							XFree (list);
							return new Size (32, 32);
						}
						
						if (size.max_width == 32) {
							XFree (list);
							return new Size (32, 32);
						}
						
						if (size.min_width < 32 && size.max_width > 32) {
							int	x;
							
							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 32) {
									XFree (list);
									return new Size (32, 32);
								}
							}
						}
						
						if (largest < size.max_width) {
							largest = size.max_width;
						}
					}
					
					// We didn't find a match or we wouldn't be here
					return new Size (largest, largest);
					
				} else {
					return new Size (32, 32);
				}
			}
		} 
		
		internal override int KeyboardSpeed {
			get {
				//
				// A lot harder: need to do:
				// XkbQueryExtension(0x08051008, 0xbfffdf4c, 0xbfffdf50, 0xbfffdf54, 0xbfffdf58)       = 1
				// XkbAllocKeyboard(0x08051008, 0xbfffdf4c, 0xbfffdf50, 0xbfffdf54, 0xbfffdf58)        = 0x080517a8
				// XkbGetControls(0x08051008, 1, 0x080517a8, 0xbfffdf54, 0xbfffdf58)                   = 0
				//
				// And from that we can tell the repetition rate
				//
				// Notice, the values must map to:
				//   [0, 31] which maps to 2.5 to 30 repetitions per second.
				//
				return 0;
			}
		}
		
		internal override int KeyboardDelay {
			get {
				//
				// Return values must range from 0 to 4, 0 meaning 250ms,
				// and 4 meaning 1000 ms.
				//
				return 1; // ie, 500 ms
			}
		} 
		
		internal override  Size MaxWindowTrackSize {
			get {
				return new Size (WorkingArea.Width, WorkingArea.Height);
			}
		} 
		
		internal override  Size MinimizedWindowSize {
			get {
				return new Size (1, 1);
			}
		} 
		
		internal override  Size MinimizedWindowSpacingSize {
			get {
				return new Size (1, 1);
			}
		} 
		
		internal override  Size MinimumWindowSize {
			get {
				return new Size (1, 1);
			}
		} 
		
		internal override  Size MinWindowTrackSize {
			get {
				return new Size (1, 1);
			}
		}
		
		internal override Keys ModifierKeys {
			get {
				return Keyboard.ModifierKeys;
			}
		}
		
		internal override  Size SmallIconSize {
			get {
				IntPtr		list;
				XIconSize	size;
				int		count;
				
				if (XGetIconSizes (DisplayHandle, RootWindow, out list, out count) != 0) {
					long		current;
					int		smallest;
					
					current = (long)list;
					smallest = 0;
					
					size = new XIconSize ();
					
					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure ((IntPtr)current, size.GetType ());
						current += Marshal.SizeOf (size);
						
						// Look for our preferred size
						if (size.min_width == 16) {
							XFree (list);
							return new Size (16, 16);
						}
						
						if (size.max_width == 16) {
							XFree (list);
							return new Size (16, 16);
						}
						
						if (size.min_width < 16 && size.max_width > 16) {
							int	x;
							
							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 16) {
									XFree (list);
									return new Size (16, 16);
								}
							}
						}
						
						if (smallest == 0 || smallest > size.min_width) {
							smallest = size.min_width;
						}
					}
					
					// We didn't find a match or we wouldn't be here
					return new Size (smallest, smallest);
					
				} else {
					return new Size (16, 16);
				}
			}
		} 
		
		internal override  int MouseButtonCount {
			get {
				return 3;
			}
		} 
		
		internal override  bool MouseButtonsSwapped {
			get {
				return false;	// FIXME - how to detect?
			}
		} 
		
		internal override  bool MouseWheelPresent {
			get {
				return true;	// FIXME - how to detect?
			}
		} 
		
		internal override  Rectangle VirtualScreen {
			get {
				return WorkingArea;
			}
		} 
		
		internal override  Rectangle WorkingArea {
			get {
				Atom			actual_atom;
				int			actual_format;
				int			nitems;
				int			bytes_after;
				IntPtr			prop = IntPtr.Zero;
				int			width;
				int			height;
				
				XGetWindowProperty (DisplayHandle, RootWindow, NetAtoms [(int)NA._NET_DESKTOP_GEOMETRY], 0, 256, false, Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				if ((nitems == 2) && (prop != IntPtr.Zero)) {
					width = Marshal.ReadInt32 (prop, 0);
					height = Marshal.ReadInt32 (prop, 4);
					
					XFree (prop);
					return new Rectangle (0, 0, width, height);
				} else {
					XWindowAttributes	attributes=new XWindowAttributes ();
					
					lock (XlibLock) {
						XGetWindowAttributes (DisplayHandle, XRootWindow (DisplayHandle, ScreenNo), ref attributes);
					}
					
					return new Rectangle (0, 0, attributes.width, attributes.height);
				}
			}
		} 
		#endregion	// Public properties
		
		#region Public Static Methods
		internal override IntPtr InitializeDriver ()
		{
			lock (this) {
				if (GdkDisplayHandle == IntPtr.Zero) {
					SetDisplay (gdk_x11_display_get_xdisplay (gdk_display_get_default ()));
				}
			}
			return IntPtr.Zero;
		}
		
		internal override void ShutdownDriver (IntPtr token)
		{
			lock (this) {
				if (GdkDisplayHandle != IntPtr.Zero) {
					gdk_display_close (GdkDisplayHandle);
					DisplayHandle = IntPtr.Zero;
					GdkDisplayHandle = IntPtr.Zero;
				}
			}
		}
		
		internal override void EnableThemes ()
		{
			ThemesEnabled = true;
		}
		
		
		internal override void Activate (IntPtr handle)
		{
			Hwnd hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd != null) lock (XlibLock) {
					SendNetWMMessage (hwnd.whole_window, (IntPtr)NetAtoms [(int)NA._NET_ACTIVE_WINDOW], IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
					//XRaiseWindow(DisplayHandle, handle);
				}
			return;
		}
		
		internal override void AudibleAlert ()
		{
			gdk_display_beep (gdk_x11_lookup_xdisplay (DisplayHandle));
			return;
		}
		
		
		internal override void CaretVisible (IntPtr handle, bool visible)
		{
			// Visible is cumulative; two hides require two shows before the caret is visible again
			if (Caret.Hwnd == handle) {
				if (visible) {
					if (Caret.Visible < 1) {
						Caret.Visible++;
						Caret.On = false;
						if (Caret.Visible == 1) {
							ShowCaret ();
							Caret.Timer.Start ();
						}
					}
				} else {
					Caret.Visible--;
					if (Caret.Visible == 0) {
						Caret.Timer.Stop ();
						HideCaret ();
					}
				}
			}
		}
		
		internal override bool CalculateWindowRect (IntPtr handle, ref Rectangle ClientRect, int Style, int ExStyle, Menu menu, out Rectangle WindowRect)
		{
			FormBorderStyle	border_style;
			TitleStyle	title_style;
			int caption_height;
			int tool_caption_height;
			
			DeriveStyles (handle, Style, ExStyle, out border_style, out title_style,
				      out caption_height, out tool_caption_height);
			
			WindowRect = Hwnd.GetWindowRectangle (border_style, menu, title_style,
							      caption_height, tool_caption_height,
							      ClientRect);
			
			return true;
		}
		
		internal override void ClientToScreen (IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, hwnd.client_window, RootWindow, x, y, out dest_x_return, out dest_y_return, out child);
			}
			
			x = dest_x_return;
			y = dest_y_return;
		}
		
		internal override int[] ClipboardAvailableFormats (IntPtr handle)
		{
			DataFormats.Format	f;
			int[]			result;
			
			f = DataFormats.Format.List;
			
			if (XGetSelectionOwner (DisplayHandle, NetAtoms [(int)NA.CLIPBOARD]) == IntPtr.Zero) {
				return null;
			}
			
			Clipboard.Formats = new ArrayList ();
			
			while (f != null) {
				XConvertSelection (DisplayHandle, NetAtoms [(int)NA.CLIPBOARD], f.Id, f.Id, FosterParent, IntPtr.Zero);
				
				Clipboard.Enumerating = true;
				while (Clipboard.Enumerating) {
					UpdateMessageQueue ();
				}
				f = f.Next;
			}
			
			result = new int [Clipboard.Formats.Count];
			
			for (int i = 0; i < Clipboard.Formats.Count; i++) {
				result [i] = (int)Clipboard.Formats [i];
			}
			
			Clipboard.Formats = null;
			return result;
		}
		
		internal override void ClipboardClose (IntPtr handle)
		{
			if (handle != ClipMagic) {
				throw new ArgumentException ("handle is not a valid clipboard handle");
			}
			return;
		}
		
		internal override int ClipboardGetID (IntPtr handle, string format)
		{
			if (handle != ClipMagic) {
				throw new ArgumentException ("handle is not a valid clipboard handle");
			}
			
			if (format == "Text") return (int)Atom.XA_STRING;
			else if (format == "Bitmap") return (int)Atom.XA_BITMAP;
			//else if (format == "MetaFilePict" ) return 3;
			//else if (format == "SymbolicLink" ) return 4;
			//else if (format == "DataInterchangeFormat" ) return 5;
			//else if (format == "Tiff" ) return 6;
			else if (format == "OEMText") return XInternAtom (DisplayHandle, "COMPOUND_TEXT", false);
			else if (format == "DeviceIndependentBitmap") return (int)Atom.XA_PIXMAP;
			else if (format == "Palette") return (int)Atom.XA_COLORMAP;	// Useless
			//else if (format == "PenData" ) return 10;
			//else if (format == "RiffAudio" ) return 11;
			//else if (format == "WaveAudio" ) return 12;
			else if (format == "UnicodeText") return XInternAtom (DisplayHandle, "UTF8_STRING", false);
			//else if (format == "EnhancedMetafile" ) return 14;
			//else if (format == "FileDrop" ) return 15;
			//else if (format == "Locale" ) return 16;
			
			return XInternAtom (DisplayHandle, format, false);
		}
		
		internal override IntPtr ClipboardOpen ()
		{
			return ClipMagic;
		}
		
		internal override object ClipboardRetrieve (IntPtr handle, int type, XplatUI.ClipboardToObject converter)
		{
			XConvertSelection (DisplayHandle, NetAtoms [(int)NA.CLIPBOARD], type, type, FosterParent, IntPtr.Zero);
			
			Clipboard.Retrieving = true;
			while (Clipboard.Retrieving) {
				UpdateMessageQueue ();
			}
			
			return Clipboard.Item;
		}
		
		internal override void ClipboardStore (IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter)
		{
			Clipboard.Item = obj;
			Clipboard.Type = type;
			Clipboard.Converter = converter;
			
			if (obj != null) {
				XSetSelectionOwner (DisplayHandle, NetAtoms [(int)NA.CLIPBOARD], FosterParent, IntPtr.Zero);
			} else {
				// Clearing the selection
				XSetSelectionOwner (DisplayHandle, NetAtoms [(int)NA.CLIPBOARD], IntPtr.Zero, IntPtr.Zero);
			}
		}
		
		internal override void CreateCaret (IntPtr handle, int width, int height)
		{
			XGCValues	gc_values;
			Hwnd		hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (Caret.Hwnd != IntPtr.Zero) {
				DestroyCaret (Caret.Hwnd);
			}
			
			Caret.Hwnd = handle;
			Caret.Window = hwnd.client_window;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = 0;
			Caret.On = false;
			
			gc_values = new XGCValues ();
			gc_values.line_width = width;
			
			Caret.gc = XCreateGC (DisplayHandle, Caret.Window, GCFunction.GCLineWidth, ref gc_values);
			if (Caret.gc == IntPtr.Zero) {
				Caret.Hwnd = IntPtr.Zero;
				return;
			}
			
			XSetFunction (DisplayHandle, Caret.gc, GXFunction.GXinvert);
		}
		
		internal override IntPtr CreateWindow (CreateParams cp) 
		{
			GdkWindowAttr 		gdk_window_attributes;
			GdkWindowAttributesType attributes_mask = 0;
			Hwnd			hwnd;
			int			X;
			int			Y;
			int			Width;
			int			Height;
			IntPtr 			GdkParentHandle;
			IntPtr 			GdkWholeWindow;
			IntPtr 			GdkClientWindow;
			Rectangle		ClientRect;
			GdkWindowType 		gdk_window_type;
			
			
			hwnd = new Hwnd ();
			
			gdk_window_attributes = new GdkWindowAttr ();
			
			X = cp.X;
			Y = cp.Y;
			Width = cp.Width;
			Height = cp.Height;
			
			if (Width < 1) Width = 1;
			if (Height < 1) Height = 1;
			
			gdk_window_type = GdkWindowType.GDK_WINDOW_CHILD;
			
			if (cp.Parent != IntPtr.Zero) {
				GdkParentHandle = gdk_window_lookup (Hwnd.ObjectFromHandle (cp.Parent).client_window);
			} else {
				if ((cp.Style & (int)WindowStyles.WS_CHILD) != 0) {
					// We need to use our foster parent window until this poor child gets it's parent assigned
					GdkParentHandle = GdkFosterParent;
				} else if ((cp.Style & (int)WindowStyles.WS_POPUP) != 0) {
					GdkParentHandle = GdkRootWindow;
				} else {
					// Default position on screen, if window manager doesn't place us somewhere else
					if (X < 1) X = 50;
					if (Y < 1) Y = 50;
					GdkParentHandle = GdkRootWindow;
				}
			}
			
//			ValueMask = SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity;
			
//			Attributes.bit_gravity = Gravity.NorthWestGravity;
//			Attributes.win_gravity = Gravity.NorthWestGravity;
			
			// FIXME: does gdk need that ?
			// Save what's under the toolwindow
			if ((cp.ExStyle & (int)WindowStyles.WS_EX_TOOLWINDOW) != 0) {
//				Attributes.save_under = true;
//				ValueMask |= SetWindowValuemask.SaveUnder;
			}
			
			// If we're a popup without caption we override the WM
			if ((cp.Style & ((int)WindowStyles.WS_POPUP)) != 0) {
				if ((cp.Style & (int)WindowStyles.WS_CAPTION) == 0) {
					gdk_window_attributes.override_redirect = true;
					attributes_mask |= GdkWindowAttributesType.GDK_WA_NOREDIR;
				}
			}
			
			hwnd.x = X;
			hwnd.y = Y;
			hwnd.width = Width;
			hwnd.height = Height;
			hwnd.parent = Hwnd.ObjectFromHandle (cp.Parent);
			
			if ((cp.Style & ((int)WindowStyles.WS_DISABLED)) != 0) {
				hwnd.enabled = false;
			}
			
			ClientRect = hwnd.ClientRect;
			GdkClientWindow = IntPtr.Zero;
			
			gdk_window_attributes.x = X;
			gdk_window_attributes.y = Y;
			gdk_window_attributes.width = Width;
			gdk_window_attributes.height = Height;
			gdk_window_attributes.window_type = gdk_window_type;
			
			attributes_mask |= GdkWindowAttributesType.GDK_WA_X | GdkWindowAttributesType.GDK_WA_Y;
			
			gdk_window_attributes.wclass = GdkWindowClass.GDK_INPUT_OUTPUT;
			
			lock (XlibLock) {
				GdkWholeWindow = gdk_window_new (GdkParentHandle, ref gdk_window_attributes, (int)attributes_mask);
				
				if (GdkWholeWindow != IntPtr.Zero) {
					attributes_mask &= ~GdkWindowAttributesType.GDK_WA_NOREDIR;
					
					if (GdkCustomVisual != IntPtr.Zero && GdkCustomColormap != IntPtr.Zero) {
						attributes_mask |= GdkWindowAttributesType.GDK_WA_COLORMAP | GdkWindowAttributesType.GDK_WA_VISUAL;
						gdk_window_attributes.colormap = GdkCustomColormap;
						gdk_window_attributes.visual = GdkCustomVisual;
					}
					
					gdk_window_attributes.x = ClientRect.X;
					gdk_window_attributes.y = ClientRect.Y;
					gdk_window_attributes.width = ClientRect.Width;
					gdk_window_attributes.height = ClientRect.Height;
					
					GdkClientWindow = gdk_window_new (GdkWholeWindow, ref gdk_window_attributes, (int)attributes_mask);
				}
			}
			
			if ((GdkWholeWindow == IntPtr.Zero) || (GdkClientWindow == IntPtr.Zero)) {
				throw new Exception ("Could not create X11 Gdk windows");
			}
			
			hwnd.WholeWindow = gdk_x11_drawable_get_xid (GdkWholeWindow);
			hwnd.ClientWindow = gdk_x11_drawable_get_xid (GdkClientWindow);
			
			#if DriverDebug
				Console.WriteLine("Created window {0:X} / {1:X} parent {2:X}", ClientWindow.ToInt32(), WholeWindow.ToInt32(), hwnd.parent != null ? hwnd.parent.Handle.ToInt32() : 0);
			#endif
			
			lock (XlibLock) {
				gdk_window_set_events (GdkWholeWindow, (int)GdkSelectInputMask);
				gdk_window_set_events (GdkClientWindow, (int)GdkSelectInputMask);
				
				if ((cp.Style & (int)WindowStyles.WS_VISIBLE) != 0) {
					gdk_window_show (GdkWholeWindow);
					gdk_window_show (GdkClientWindow);
					hwnd.visible = true;
				}
			}
			
			SetWMStyles (hwnd, cp);
			
			if ((cp.Style & (int)WindowStyles.WS_MINIMIZE) != 0) {
				SetWindowState (hwnd.Handle, FormWindowState.Minimized);
			} else if ((cp.Style & (int)WindowStyles.WS_MAXIMIZE) != 0) {
				SetWindowState (hwnd.Handle, FormWindowState.Maximized);
			}
			
			// for now make all windows dnd enabled
			Dnd.SetAllowDrop (hwnd, true);
			
			// Set caption/window title
			Text (hwnd.Handle, cp.Caption);
			
			return hwnd.Handle;
		}
		
		internal override IntPtr CreateWindow (IntPtr Parent, int X, int Y, int Width, int Height)
		{
			CreateParams create_params = new CreateParams ();
			
			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;
			
			create_params.ClassName = XplatUI.DefaultClassName;
			create_params.ClassStyle = 0;
			create_params.ExStyle = 0;
			create_params.Parent = IntPtr.Zero;
			create_params.Param = 0;
			
			return CreateWindow (create_params);
		}
		
		internal override IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot)
		{
			IntPtr	cursor;
			Bitmap	cursor_bitmap;
			Bitmap	cursor_mask;
			Byte[]	cursor_bits;
			Byte[]	mask_bits;
			Color	c_pixel;
			Color	m_pixel;
			int	width;
			int	height;
			IntPtr	cursor_pixmap;
			IntPtr	mask_pixmap;
			XColor	fg;
			XColor	bg;
			bool	and;
			bool	xor;
			
			Size cursor_size = CursorSize;
			width = cursor_size.Width;
			height = cursor_size.Height;
			
			// Win32 only allows creation cursors of a certain size
			if ((bitmap.Width != width) || (bitmap.Width != height)) {
				cursor_bitmap = new Bitmap (bitmap, new Size (width, height));
				cursor_mask = new Bitmap (mask, new Size (width, height));
			} else {
				cursor_bitmap = bitmap;
				cursor_mask = mask;
			}
			
			width = cursor_bitmap.Width;
			height = cursor_bitmap.Height;
			
			cursor_bits = new Byte [(width / 8) * height];
			mask_bits = new Byte [(width / 8) * height];
			
			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					c_pixel = cursor_bitmap.GetPixel (x, y);
					m_pixel = cursor_mask.GetPixel (x, y);
					
					and = c_pixel == cursor_pixel;
					xor = m_pixel == mask_pixel;
					
					if (!and && !xor) {
						// Black
						// cursor_bits[y * width / 8 + x / 8] &= (byte)~((1 << (x % 8)));	// The bit already is 0
						mask_bits [y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
					} else if (and && !xor) {
						// White
						cursor_bits [y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
						mask_bits [y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
						#if notneeded
					} else if (and && !xor) {
						// Screen
					} else if (and && xor) {
						// Inverse Screen

						// X11 doesn't know the 'reverse screen' concept, so we'll treat them the same
						// we want both to be 0 so nothing to be done
						//cursor_bits[y * width / 8 + x / 8] &= (byte)~((1 << (x % 8)));
						//mask_bits[y * width / 8 + x / 8] |= (byte)(01 << (x % 8));
						#endif
					}
				}
			}
			
			cursor_pixmap = XCreatePixmapFromBitmapData (DisplayHandle, RootWindow, cursor_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			mask_pixmap = XCreatePixmapFromBitmapData (DisplayHandle, RootWindow, mask_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			fg = new XColor ();
			bg = new XColor ();
			
			fg.pixel = XWhitePixel (DisplayHandle, ScreenNo);
			fg.red = (ushort)65535;
			fg.green = (ushort)65535;
			fg.blue = (ushort)65535;
			
			bg.pixel = XBlackPixel (DisplayHandle, ScreenNo);
			
			cursor = XCreatePixmapCursor (DisplayHandle, cursor_pixmap, mask_pixmap, ref fg, ref bg, xHotSpot, yHotSpot);
			
			XFreePixmap (DisplayHandle, cursor_pixmap);
			XFreePixmap (DisplayHandle, mask_pixmap);
			
			return cursor;
		}
		
		internal override IntPtr DefineStdCursor (StdCursor id)
		{
			CursorFontShape	shape;
			IntPtr		cursor;
			
			// FIXME - define missing shapes
			
			switch (id) {
			case StdCursor.AppStarting: {
					shape = CursorFontShape.XC_watch;
					break;
				}
				
			case StdCursor.Arrow: {
					return IntPtr.Zero;
				}
				
			case StdCursor.Cross: {
					shape = CursorFontShape.XC_crosshair;
					break;
				}
				
			case StdCursor.Default: {
					return IntPtr.Zero;
				}
				
			case StdCursor.Hand: {
					shape = CursorFontShape.XC_hand1;
					break;
				}
				
			case StdCursor.Help: {
					shape = CursorFontShape.XC_question_arrow;
					break;
				}
				
			case StdCursor.HSplit: {
                                        shape = CursorFontShape.XC_sb_v_double_arrow; 
					break;
				}
				
			case StdCursor.IBeam: {
					shape = CursorFontShape.XC_xterm; 
					break;
				}
				
			case StdCursor.No: {
					shape = CursorFontShape.XC_circle; 
					break;
				}
				
			case StdCursor.NoMove2D: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.NoMoveHoriz: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.NoMoveVert: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanEast: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanNE: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanNorth: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanNW: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanSE: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanSouth: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanSW: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.PanWest: {
					shape = CursorFontShape.XC_sizing; 
					break;
				}
				
			case StdCursor.SizeAll: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}
				
			case StdCursor.SizeNESW: {
					shape = CursorFontShape.XC_top_right_corner; 
					break;
				}
				
			case StdCursor.SizeNS: {
					shape = CursorFontShape.XC_sb_v_double_arrow;
					break;
				}
				
			case StdCursor.SizeNWSE: {
					shape = CursorFontShape.XC_top_left_corner; 
					break;
				}
				
			case StdCursor.SizeWE: {
					shape = CursorFontShape.XC_sb_h_double_arrow; 
					break;
				}
				
			case StdCursor.UpArrow: {
					shape = CursorFontShape.XC_center_ptr; 
					break;
				}
				
			case StdCursor.VSplit: {
                                        shape = CursorFontShape.XC_sb_h_double_arrow;
					break;
				}
				
			case StdCursor.WaitCursor: {
					shape = CursorFontShape.XC_watch; 
					break;
				}
				
			default: {
					return IntPtr.Zero;
				}
			}
			
			lock (XlibLock) {
				cursor = XCreateFontCursor (DisplayHandle, shape);
			}
			return cursor;
		}
		
		internal override IntPtr DefWndProc (ref Message msg)
		{
			return IntPtr.Zero;
		}
		
		internal override void DestroyCaret (IntPtr handle)
		{
			if (Caret.Hwnd == handle) {
				if (Caret.Visible == 1) {
					Caret.Timer.Stop ();
					HideCaret ();
				}
				if (Caret.gc != IntPtr.Zero) {
					XFreeGC (DisplayHandle, Caret.gc);
					Caret.gc = IntPtr.Zero;
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = 0;
				Caret.On = false;
			}
		}
		
		internal override void DestroyCursor (IntPtr cursor)
		{
			lock (XlibLock) {
				XFreeCursor (DisplayHandle, cursor);
			}
		}
		
		internal override void DestroyWindow (IntPtr handle)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd == null) {
				#if DriverDebug
					Console.WriteLine("window {0:X} already destroyed", handle.ToInt32());
				#endif
				return;
			}
			
			#if DriverDebug
				Console.WriteLine("Destroying window {0:X}", handle.ToInt32());
			#endif
			
			// Make sure if the caret is in the window, that we destroy the caret, too
			if (Caret.Hwnd == hwnd.client_window) {
				DestroyCaret (handle);
			}
			
			// Mark our children as gone as well
			DestroyChildWindow (Control.ControlNativeWindow.ControlFromHandle (handle));
			
			// Send destroy message
			SendMessage (handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
			
			lock (XlibLock) {
				if (hwnd.client_window != IntPtr.Zero) {
					gdk_window_destroy (gdk_window_lookup (hwnd.client_window));
				}
				
				if ((hwnd.whole_window != IntPtr.Zero) && (hwnd.whole_window != hwnd.client_window)) {
					gdk_window_destroy (gdk_window_lookup (hwnd.whole_window));
				}
			}
			hwnd.Dispose ();
		}
		
		internal override IntPtr DispatchMessage (ref MSG msg)
		{
			return NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}
		
		internal override void DrawReversibleRectangle (IntPtr handle, Rectangle rect, int line_width)
		{
			Hwnd		hwnd;
			XGCValues	gc_values;
			IntPtr		gc;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			gc_values = new XGCValues ();
			
			gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			gc_values.line_width = line_width;
			gc_values.foreground = XBlackPixel (DisplayHandle, ScreenNo);
			
			// This logic will give us true rubber bands: (libsx, SANE_XOR)
			//mask = foreground ^ background; 
			//XSetForeground(DisplayHandle, gc, 0xffffffff);
			//XSetBackground(DisplayHandle, gc, background);
			//XSetFunction(DisplayHandle,   gc, GXxor);
			//XSetPlaneMask(DisplayHandle,  gc, mask);
			
			
			gc = XCreateGC (DisplayHandle, hwnd.client_window, GCFunction.GCSubwindowMode | GCFunction.GCLineWidth | GCFunction.GCForeground, ref gc_values);
			uint foreground;
			uint background;
			
			Control control;
			control = Control.FromHandle (handle);
			
			XColor xcolor = new XColor ();
			
			xcolor.red = (ushort)(control.ForeColor.R * 257);
			xcolor.green = (ushort)(control.ForeColor.G * 257);
			xcolor.blue = (ushort)(control.ForeColor.B * 257);
			XAllocColor (DisplayHandle, DefaultColormap, ref xcolor);
			foreground = (uint)xcolor.pixel.ToInt32 ();
			
			xcolor.red = (ushort)(control.BackColor.R * 257);
			xcolor.green = (ushort)(control.BackColor.G * 257);
			xcolor.blue = (ushort)(control.BackColor.B * 257);
			XAllocColor (DisplayHandle, DefaultColormap, ref xcolor);
			background = (uint)xcolor.pixel.ToInt32 ();
			
			uint mask = foreground ^ background; 
			
			XSetForeground (DisplayHandle, gc, 0xffffffff);
			XSetBackground (DisplayHandle, gc, background);
			XSetFunction (DisplayHandle,   gc, GXFunction.GXxor);
			XSetPlaneMask (DisplayHandle,  gc, mask);
			
			if ((rect.Width > 0) && (rect.Height > 0)) {
				XDrawRectangle (DisplayHandle, hwnd.client_window, gc, rect.Left, rect.Top, rect.Width, rect.Height);
			} else {
				if (rect.Width > 0) {
					XDrawLine (DisplayHandle, hwnd.client_window, gc, rect.X, rect.Y, rect.Right, rect.Y);
				} else {
					XDrawLine (DisplayHandle, hwnd.client_window, gc, rect.X, rect.Y, rect.X, rect.Bottom);
				}
			}
			XFreeGC (DisplayHandle, gc);
		}
		
		internal override void DoEvents ()
		{
			MSG msg = new MSG ();
			
			if (OverrideCursorHandle != IntPtr.Zero) {
				OverrideCursorHandle = IntPtr.Zero;
			}
			
			while (PeekMessage (ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
				TranslateMessage (ref msg);
				DispatchMessage (ref msg);
			}
		}
		
		internal override void EnableWindow (IntPtr handle, bool Enable)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			if (hwnd != null) {
				hwnd.Enabled = Enable;
			}
		}
		
		internal override void EndLoop (Thread thread)
		{
			// This is where we one day will shut down the loop for the thread
		}
		
		
		internal override IntPtr GetActive ()
		{
			Atom	actual_atom;
			int	actual_format;
			int	nitems;
			int	bytes_after;
			IntPtr	prop = IntPtr.Zero;
			IntPtr	active = IntPtr.Zero;
			
			XGetWindowProperty (DisplayHandle, RootWindow, NetAtoms [(int)NA._NET_ACTIVE_WINDOW], 0, 1, false, Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if ((nitems > 0) && (prop != IntPtr.Zero)) {
				active = (IntPtr)Marshal.ReadInt32 (prop);
				XFree (prop);
			}
			
			if (active != IntPtr.Zero) {
				Hwnd	hwnd;
				
				hwnd = Hwnd.GetObjectFromWindow (active);
				if (hwnd != null) {
					active = hwnd.Handle;
				} else {
					active = IntPtr.Zero;
				}
			}
			return active;
		}
		
		internal override void GetCursorInfo (IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y)
		{
			throw new NotImplementedException ();
		}
		
		internal override void GetDisplaySize (out Size size)
		{
			XWindowAttributes	attributes=new XWindowAttributes ();
			
			lock (XlibLock) {
				// FIXME - use _NET_WM messages instead?
				XGetWindowAttributes (DisplayHandle, XRootWindow (DisplayHandle, ScreenNo), ref attributes);
			}
			
			size = new Size (attributes.width, attributes.height);
		}
		
		internal override SizeF GetAutoScaleSize (Font font)
		{
			Graphics	g;
			float		width;
			string		magic_string = "The quick brown fox jumped over the lazy dog.";
			double		magic_number = 44.549996948242189;
			
			g = Graphics.FromHwnd (FosterParent);
			
			width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			return new SizeF (width, font.Height);
		}
		
		internal override IntPtr GetParent (IntPtr handle)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			if (hwnd != null && hwnd.parent != null) {
				return hwnd.parent.Handle;
			}
			return IntPtr.Zero;
		}
		
		internal override void GetCursorPos (IntPtr handle, out int x, out int y)
		{
			IntPtr	use_handle;
			IntPtr	root;
			IntPtr	child;
			int	root_x;
			int	root_y;
			int	win_x;
			int	win_y;
			int	keys_buttons;
			
			if (handle != IntPtr.Zero) {
				use_handle = Hwnd.ObjectFromHandle (handle).client_window;
			} else {
				use_handle = RootWindow;
			}
			
			lock (XlibLock) {
				XQueryPointer (DisplayHandle, use_handle, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);
			}
			
			if (handle != IntPtr.Zero) {
				x = win_x;
				y = win_y;
			} else {
				x = root_x;
				y = root_y;
			}
		}
		
		internal override bool GetFontMetrics (Graphics g, Font font, out int ascent, out int descent)
		{
			return GetFontMetrics (g.GetHdc (), font.ToHfont (), out ascent, out descent);
		}
		
		internal override Point GetMenuOrigin (IntPtr handle)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd != null) {
				return hwnd.MenuOrigin;
			}
			return Point.Empty;
		}
		
		internal override bool GetMessage (ref MSG msg, IntPtr handle, int wFilterMin, int wFilterMax)
		{
			XEvent	xevent;
			bool	client;
			Hwnd	hwnd;
			
			ProcessNextMessage:
			
			if (MessageQueue.Count > 0) {
				xevent = (XEvent) MessageQueue.Dequeue ();
			} else {
				UpdateMessageQueue ();
				
				if (MessageQueue.Count > 0) {
					xevent = (XEvent) MessageQueue.Dequeue ();
				} else {
					if (!PostQuitState) {
						msg.hwnd = IntPtr.Zero;
						msg.message = Msg.WM_ENTERIDLE;
						return true;
					}
					
					// We reset ourselves so GetMessage can be called again
					PostQuitState = false;
					
					return false;
				}
			}
			
			// FIXME - handle filtering
			
			hwnd = Hwnd.GetObjectFromWindow (xevent.AnyEvent.window);
			
			// Handle messages for windows that are already or are about to be destroyed
			if (hwnd == null) {
				#if DriverDebug
					Console.WriteLine("GetMessage(): Got message {0} for non-existent or already destroyed window {1:X}", xevent.type, xevent.AnyEvent.window.ToInt32());
				#endif
				goto ProcessNextMessage;
			}
			
			if (hwnd.client_window == xevent.AnyEvent.window) {
				client = true;
				//Console.WriteLine("Client message, sending to window {0:X}", msg.hwnd.ToInt32());
			} else {
				client = false;
				//Console.WriteLine("Non-Client message, sending to window {0:X}", msg.hwnd.ToInt32());
			}
			
			msg.hwnd = hwnd.Handle;
			
			//
			// If you add a new event to this switch make sure to add it in
			// UpdateMessage also unless it is not coming through the X event system.
			//
			switch (xevent.type) {
			case XEventName.KeyPress: {
					Keyboard.KeyEvent (FocusWindow, xevent, ref msg);
					break;
				}
				
			case XEventName.KeyRelease: {
					Keyboard.KeyEvent (FocusWindow, xevent, ref msg);
					break;
				}
				
			case XEventName.ButtonPress: {
					switch (xevent.ButtonEvent.button) {
					case 1: {
							MouseState |= MouseButtons.Left;
							if (client) {
								msg.message = Msg.WM_LBUTTONDOWN;
							} else {
								msg.message = Msg.WM_NCLBUTTONDOWN;
								WholeToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							// TODO: For WM_NCLBUTTONDOWN wParam specifies a hit-test value not the virtual keys down
							msg.wParam = GetMousewParam (0);
							break;
						}
						
					case 2: {
							MouseState |= MouseButtons.Middle;
							if (client) {
								msg.message = Msg.WM_MBUTTONDOWN;
							} else {
								msg.message = Msg.WM_NCMBUTTONDOWN;
								WholeToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam = GetMousewParam (0);
							break;
						}
						
					case 3: {
							MouseState |= MouseButtons.Right;
							if (client) {
								msg.message = Msg.WM_RBUTTONDOWN;
							} else {
								msg.message = Msg.WM_NCRBUTTONDOWN;
								WholeToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam = GetMousewParam (0);
							break;
						}
						
					case 4: {
							msg.message = Msg.WM_MOUSEWHEEL;
							msg.wParam = GetMousewParam (120);
							break;
						}
						
					case 5: {
							msg.message = Msg.WM_MOUSEWHEEL;
							msg.wParam = GetMousewParam (-120);
							break;
						}
						
					}
					
					msg.lParam = (IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;
					
					if (!hwnd.Enabled) {
						IntPtr dummy;
						
						msg.hwnd = hwnd.EnabledHwnd;
						XTranslateCoordinates (DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle (msg.hwnd).ClientWindow, xevent.ButtonEvent.x, xevent.ButtonEvent.y, out xevent.ButtonEvent.x, out xevent.ButtonEvent.y, out dummy);
						msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
					}
					
					if (Grab.Hwnd != IntPtr.Zero) {
						msg.hwnd = Grab.Hwnd;
					}
					
					if (!ClickPending.Pending) {
						ClickPending.Pending = true;
						ClickPending.Hwnd = msg.hwnd;
						ClickPending.Message = msg.message;
						ClickPending.wParam = msg.wParam;
						ClickPending.lParam = msg.lParam;
						ClickPending.Time = (long)xevent.ButtonEvent.time;
					} else {
						if ((((long)xevent.ButtonEvent.time - ClickPending.Time) < DoubleClickInterval) && (msg.wParam == ClickPending.wParam) && (msg.lParam == ClickPending.lParam) && (msg.message == ClickPending.Message)) {
							// Looks like a genuine double click, clicked twice on the same spot with the same keys
							switch (xevent.ButtonEvent.button) {
							case 1: {
									msg.message = client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK;
									break;
								}
								
							case 2: {
									msg.message = client ? Msg.WM_MBUTTONDBLCLK : Msg.WM_NCMBUTTONDBLCLK;
									break;
								}
								
							case 3: {
									msg.message = client ? Msg.WM_RBUTTONDBLCLK : Msg.WM_NCRBUTTONDBLCLK;
									break;
								}
							}
						}
						ClickPending.Pending = false;
					}
					
					break;
				}
				
			case XEventName.ButtonRelease: {
					Dnd.HandleButtonRelease (ref xevent);
					switch (xevent.ButtonEvent.button) {
					case 1: {
							if (client) {
								msg.message = Msg.WM_LBUTTONUP;
							} else {
								msg.message = Msg.WM_NCLBUTTONUP;
								WholeToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam = GetMousewParam (0);
							MouseState &= ~MouseButtons.Left;
							break;
						}
						
					case 2: {
							if (client) {
								msg.message = Msg.WM_MBUTTONUP;
							} else {
								msg.message = Msg.WM_NCMBUTTONUP;
								WholeToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam = GetMousewParam (0);
							MouseState &= ~MouseButtons.Middle;
							break;
						}
						
					case 3: {
							if (client) {
								msg.message = Msg.WM_RBUTTONUP;
							} else {
								msg.message = Msg.WM_NCRBUTTONUP;
								WholeToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam = GetMousewParam (0);
							MouseState &= ~MouseButtons.Right;
							break;
						}
						
					case 4: {
							goto ProcessNextMessage;
						}
						
					case 5: {
							goto ProcessNextMessage;
						}
					}
					
					if (!hwnd.Enabled) {
						IntPtr dummy;
						
						msg.hwnd = hwnd.EnabledHwnd;
						XTranslateCoordinates (DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle (msg.hwnd).ClientWindow, xevent.ButtonEvent.x, xevent.ButtonEvent.y, out xevent.ButtonEvent.x, out xevent.ButtonEvent.y, out dummy);
						msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
					}
					
					if (Grab.Hwnd != IntPtr.Zero) {
						msg.hwnd = Grab.Hwnd;
					}
					
					msg.lParam = (IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;
					break;
				}
				
			case XEventName.MotionNotify: {
					if (client) {
						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): Window {0:X} MotionNotify x={1} y={2}", client ? hwnd.client_window.ToInt32() : hwnd.whole_window.ToInt32(), xevent.MotionEvent.x, xevent.MotionEvent.y);
						#endif
						
						if (Dnd.HandleMotionNotify (ref xevent))
							goto ProcessNextMessage;
						if (Grab.Hwnd != IntPtr.Zero) {
							msg.hwnd = Grab.Hwnd;
						} else {
							NativeWindow.WndProc (msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);
						}
						
						msg.message = Msg.WM_MOUSEMOVE;
						msg.wParam = GetMousewParam (0);
						msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x & 0xFFFF);
						
						if (!hwnd.Enabled) {
							IntPtr dummy;
							
							msg.hwnd = hwnd.EnabledHwnd;
							XTranslateCoordinates (DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle (msg.hwnd).ClientWindow, xevent.MotionEvent.x, xevent.MotionEvent.y, out xevent.MotionEvent.x, out xevent.MotionEvent.y, out dummy);
							msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
						}
						
						HoverState.X = MousePosition.X = xevent.MotionEvent.x;
						HoverState.Y = MousePosition.Y = xevent.MotionEvent.y;
						
						break;
					} else {
						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): non-client area {0:X} MotionNotify x={1} y={2}", client ? hwnd.client_window.ToInt32() : hwnd.whole_window.ToInt32(), xevent.MotionEvent.x, xevent.MotionEvent.y);
						#endif
						msg.message = Msg.WM_NCMOUSEMOVE;
						msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x & 0xFFFF);
						
						if (!hwnd.Enabled) {
							IntPtr dummy;
							
							msg.hwnd = hwnd.EnabledHwnd;
							XTranslateCoordinates (DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle (msg.hwnd).ClientWindow, xevent.MotionEvent.x, xevent.MotionEvent.y, out xevent.MotionEvent.x, out xevent.MotionEvent.y, out dummy);
							msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
						}
						
						#if notyet
							// Not sure we need this...
							HitTest	ht;
							ht = NativeWindow.WndProc(hwnd.client_window, Msg.WM_NCHITTEST, IntPtr.Zero, msg.lParam);
						#endif
						
						MousePosition.X = xevent.MotionEvent.x;
						MousePosition.Y = xevent.MotionEvent.y;
					}
					
					break;
				}
				
			case XEventName.EnterNotify: {
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						goto ProcessNextMessage;
					}
					msg.message = Msg.WM_MOUSE_ENTER;
					HoverState.Timer.Enabled = true;
					HoverState.Window = xevent.CrossingEvent.window;
					break;
				}
				
			case XEventName.LeaveNotify: {
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						goto ProcessNextMessage;
					}
					msg.message = Msg.WM_MOUSELEAVE;
					HoverState.Timer.Enabled = false;
					HoverState.Window = IntPtr.Zero;
					break;
				}
				
				#if later
				case XEventName.CreateNotify: {
					if (client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {
						msg.message = WM_CREATE;
						// Set up CreateStruct
					} else {
						goto ProcessNextMessage;
					}
					break;
				}
				#endif
				
				
			case XEventName.ReparentNotify: {
					if (hwnd.parent == null) {	// Toplevel
						if (xevent.ReparentEvent.parent != IntPtr.Zero) {
							// We need to adjust x/y
							int	dummy_int;
							
							hwnd.Reparented = true;
							
							gdk_window_get_geometry (gdk_window_lookup (hwnd.whole_window), out hwnd.x, out hwnd.y, out dummy_int, out dummy_int, out dummy_int);
							msg.message = Msg.WM_WINDOWPOSCHANGED;
							if (hwnd.opacity != 0xffffffff) {
								uint opacity;
								
								opacity = hwnd.opacity;
								XChangeProperty (DisplayHandle, XGetParent (hwnd.whole_window), NetAtoms [(int)NA._NET_WM_WINDOW_OPACITY], Atom.XA_CARDINAL, 32, PropertyMode.Replace, ref opacity, 1);
							}
						} else {
							hwnd.Reparented = false;
							goto ProcessNextMessage;
						}
					}
					break;
				}
				
			case XEventName.ConfigureNotify: {
					if (!client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {	// Ignore events for children (SubstructureNotify) and client areas
						XplatUIWin32.NCCALCSIZE_PARAMS	ncp;
						IntPtr				ptr;
						Rectangle			rect;
						
						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): Window {0:X} ConfigureNotify x={1} y={2} width={3} height={4}", hwnd.client_window.ToInt32(), xevent.ConfigureEvent.x, xevent.ConfigureEvent.y, xevent.ConfigureEvent.width, xevent.ConfigureEvent.height);
						#endif
						msg.message = Msg.WM_WINDOWPOSCHANGED;
						hwnd.configure_pending = false;
						
						// We need to adjust our client window to track the resize of whole_window
						rect = hwnd.DefaultClientRect;
						
						ncp = new XplatUIWin32.NCCALCSIZE_PARAMS ();
						ptr = Marshal.AllocHGlobal (Marshal.SizeOf (ncp));
						
						ncp.rgrc1.left = rect.Left;
						ncp.rgrc1.top = rect.Top;
						ncp.rgrc1.right = rect.Right;
						ncp.rgrc1.bottom = rect.Bottom;
						
						Marshal.StructureToPtr (ncp, ptr, true);
						NativeWindow.WndProc (hwnd.client_window, Msg.WM_NCCALCSIZE, (IntPtr)1, ptr);
						ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure (ptr, typeof(XplatUIWin32.NCCALCSIZE_PARAMS));
						Marshal.FreeHGlobal (ptr);
						
						// FIXME - debug this with Menus, need to set hwnd.ClientRect
						
						rect = new Rectangle (ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
						//Console.WriteLine("CreateOffscreenbuffer...");
//						CreateOffscreenBuffer (ref hwnd.client_offscreen, rect.Width, rect.Height);
						
						XMoveResizeWindow (DisplayHandle, hwnd.client_window, rect.X, rect.Y, rect.Width, rect.Height);
					} else {
						goto ProcessNextMessage;
					}
					
					msg.lParam = IntPtr.Zero;		// FIXME - Generate LPWINDOWPOS structure and pass on
					break;
				}
				
			case XEventName.FocusIn: {
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					msg.message = Msg.WM_SETFOCUS;
					msg.wParam = IntPtr.Zero;
					break;
				}
				
			case XEventName.FocusOut: {
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					msg.message = Msg.WM_KILLFOCUS;
					msg.wParam = IntPtr.Zero;
					break;
				}
				
			case XEventName.Expose: {
					if (PostQuitState) {
						goto ProcessNextMessage;
					}
					
					
					if (client) {
						if (!hwnd.expose_pending) {
							goto ProcessNextMessage;
						}
					} else {
						if (!hwnd.nc_expose_pending) {
							goto ProcessNextMessage;
						}
						
						switch (hwnd.border_style) {
						case FormBorderStyle.Fixed3D: {
								Graphics g;
								
								g = Graphics.FromHwnd (hwnd.whole_window);
								ControlPaint.DrawBorder3D (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height));
								g.Dispose ();
								break;
							}
							
						case FormBorderStyle.FixedSingle: {
								Graphics g;
								
								g = Graphics.FromHwnd (hwnd.whole_window);
								ControlPaint.DrawBorder (g, new Rectangle (0, 0, hwnd.Width, hwnd.Height), Color.Black, ButtonBorderStyle.Solid);
								g.Dispose ();
								break;
							}
						}
						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): Window {0:X} Exposed non-client area {1},{2} {3}x{4}", hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						#endif
						
						Rectangle rect = new Rectangle (xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						Region region = new Region (rect);
						IntPtr hrgn = region.GetHrgn (null); // Graphics object isn't needed
						msg.message = Msg.WM_NCPAINT;
						msg.wParam = hrgn;
						hwnd.nc_expose_pending = false;
						break;
					}
					#if DriverDebugExtra
						Console.WriteLine("GetMessage(): Window {0:X} Exposed area {1},{2} {3}x{4}", hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
					#endif
					if (Caret.Visible == 1) {
						Caret.Paused = true;
						HideCaret ();
					}
					
					if (Caret.Visible == 1) {
						ShowCaret ();
						Caret.Paused = false;
					}
					msg.message = Msg.WM_PAINT;
					break;
				}
				
			case XEventName.DestroyNotify: {
					
					// This is a bit tricky, we don't receive our own DestroyNotify, we only get those for our children
					hwnd = Hwnd.ObjectFromHandle (xevent.DestroyWindowEvent.window);
					
					// We may get multiple for the same window, act only one the first (when Hwnd still knows about it)
					if (hwnd.client_window == xevent.DestroyWindowEvent.window) {
						msg.hwnd = hwnd.client_window;
						msg.message = Msg.WM_DESTROY;
						hwnd.Dispose ();
						
						#if DriverDebug
							Console.WriteLine("Got DestroyNotify on Window {0:X}", msg.hwnd.ToInt32());
						#endif
					} else {
						goto ProcessNextMessage;
					}
					
					break;
				}
				
			case XEventName.ClientMessage: {
					if (Dnd.HandleClientMessage (ref xevent)) {
						goto ProcessNextMessage;
					}
					
					if (xevent.ClientMessageEvent.message_type == (IntPtr)AsyncAtom) {
						XplatUIDriverSupport.ExecuteClientMessage ((GCHandle)xevent.ClientMessageEvent.ptr1);
						break;
					}
					
					if (xevent.ClientMessageEvent.message_type == (IntPtr)HoverState.Atom) {
						msg.message = Msg.WM_MOUSEHOVER;
						msg.wParam = GetMousewParam (0);
						msg.lParam = (IntPtr) (xevent.ClientMessageEvent.ptr1);
						break;
					}
					
					if (xevent.ClientMessageEvent.message_type == (IntPtr)PostAtom) {
						msg.hwnd = xevent.ClientMessageEvent.ptr1;
						msg.message = (Msg) xevent.ClientMessageEvent.ptr2.ToInt32 ();
						msg.wParam = xevent.ClientMessageEvent.ptr3;
						msg.lParam = xevent.ClientMessageEvent.ptr4;
						break;
					}
					
					#if dontcare
					if  (xevent.ClientMessageEvent.message_type == (IntPtr)NetAtoms[(int)NA._XEMBED]) {
						Console.WriteLine("GOT EMBED MESSAGE {0:X}", xevent.ClientMessageEvent.ptr2.ToInt32());
						break;
					}
					#endif
					
					if  (xevent.ClientMessageEvent.message_type == (IntPtr)NetAtoms [(int)NA.WM_PROTOCOLS]) {
						if (xevent.ClientMessageEvent.ptr1 == (IntPtr)NetAtoms [(int)NA.WM_DELETE_WINDOW]) {
							msg.message = Msg.WM_CLOSE;
							Graphics.FromHdcInternal (IntPtr.Zero);
							break;
						}
						
						// We should not get this, but I'll leave the code in case we need it in the future
						if (xevent.ClientMessageEvent.ptr1 == (IntPtr)NetAtoms [(int)NA.WM_TAKE_FOCUS]) {
							goto ProcessNextMessage;
						}
					}
					break;
				}
				
			case XEventName.TimerNotify: {
					xevent.TimerNotifyEvent.handler (this, EventArgs.Empty);
					break;
				}
				
			default: {
					goto ProcessNextMessage;
				}
			}
			
			return true;
		}
		
		internal override bool GetText (IntPtr handle, out string text)
		{
			IntPtr	textptr;
			
			textptr = IntPtr.Zero;
			
			lock (XlibLock) {
				// FIXME - use _NET properties
				XFetchName (DisplayHandle, Hwnd.ObjectFromHandle (handle).whole_window, ref textptr);
			}
			if (textptr != IntPtr.Zero) {
				text = Marshal.PtrToStringAnsi (textptr);
				XFree (textptr);
				return true;
			} else {
				text = "";
				return false;
			}
		}
		
		internal override void GetWindowPos (IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height)
		{
			Hwnd		hwnd;
			Rectangle	rect;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd != null) {
				x = hwnd.x;
				y = hwnd.y;
				width = hwnd.width;
				height = hwnd.height;
				
				rect = Hwnd.GetClientRectangle (hwnd.border_style, hwnd.menu, hwnd.title_style, hwnd.caption_height, hwnd.tool_caption_height, width, height);
				
				client_width = rect.Width;
				client_height = rect.Height;
				
				return;
			}
			
			// Should we throw an exception or fail silently?
			// throw new ArgumentException("Called with an invalid window handle", "handle");
			
			x = 0;
			y = 0;
			width = 0;
			height = 0;
			client_width = 0;
			client_height = 0;
		}
		
		internal override FormWindowState GetWindowState (IntPtr handle)
		{
			Atom			actual_atom;
			int			actual_format;
			int			nitems;
			int			bytes_after;
			IntPtr			prop = IntPtr.Zero;
			IntPtr			atom;
			int			maximized;
			bool			minimized;
			XWindowAttributes	attributes;
			Hwnd			hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			maximized = 0;
			minimized = false;
			XGetWindowProperty (DisplayHandle, hwnd.whole_window, NetAtoms [(int)NA._NET_WM_STATE], 0, 256, false, Atom.XA_ATOM, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if ((nitems > 0) && (prop != IntPtr.Zero)) {
				for (int i = 0; i < nitems; i++) {
					atom = (IntPtr)Marshal.ReadInt32 (prop, i * 4);
					if ((atom == (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_HORZ]) || (atom == (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_VERT])) {
						maximized++;
					} else if (atom == (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_HIDDEN]) {
						minimized = true;
					}
				}
				XFree (prop);
			}
			
			if (minimized) {
				return FormWindowState.Minimized;
			} else if (maximized == 2) {
				return FormWindowState.Maximized;
			}
			
			attributes = new XWindowAttributes ();
			XGetWindowAttributes (DisplayHandle, handle, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped) {
				throw new NotSupportedException ("Cannot retrieve the state of an unmapped window");
			}
			
			
			return FormWindowState.Normal;
		}
		
		internal override void GrabInfo (out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea)
		{
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}
		
		internal override void GrabWindow (IntPtr handle, IntPtr confine_to_handle)
		{
			Hwnd	hwnd;
			IntPtr	confine_to_window;
			
			confine_to_window = IntPtr.Zero;
			
			if (confine_to_handle != IntPtr.Zero) {
				XWindowAttributes	attributes = new XWindowAttributes ();
				
				hwnd = Hwnd.ObjectFromHandle (confine_to_handle);
				
				lock (XlibLock) {
					XGetWindowAttributes (DisplayHandle, hwnd.client_window, ref attributes);
				}
				Grab.Area.X = attributes.x;
				Grab.Area.Y = attributes.y;
				Grab.Area.Width = attributes.width;
				Grab.Area.Height = attributes.height;
				Grab.Confined = true;
				confine_to_window = hwnd.client_window;
			}
			
			Grab.Hwnd = handle;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			lock (XlibLock) {
				XGrabPointer (DisplayHandle, hwnd.client_window, false, 
					      EventMask.ButtonPressMask | EventMask.ButtonMotionMask |
					      EventMask.ButtonReleaseMask | EventMask.PointerMotionMask,
					      GrabMode.GrabModeAsync, GrabMode.GrabModeAsync, confine_to_window, 0, 0);
			}
		}
		
		internal override void UngrabWindow (IntPtr hwnd)
		{
			lock (XlibLock) {
				XUngrabPointer (DisplayHandle, 0);
				XFlush (DisplayHandle);
			}
			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;
		}
		
		internal override void HandleException (Exception e)
		{
			StackTrace st = new StackTrace (e, true);
			Console.WriteLine ("Exception '{0}'", e.Message + st.ToString ());
			Console.WriteLine ("{0}{1}", e.Message, st.ToString ());
		}
		
		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear)
		{
			Hwnd	hwnd;
			XEvent	xevent;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			
			xevent = new XEvent ();
			xevent.type = XEventName.Expose;
			xevent.ExposeEvent.display = DisplayHandle;
			xevent.ExposeEvent.window = hwnd.client_window;
			
			if (clear) {
				xevent.ExposeEvent.x = hwnd.X;
				xevent.ExposeEvent.y = hwnd.Y;
				xevent.ExposeEvent.width = hwnd.Width;
				xevent.ExposeEvent.height = hwnd.Height;
			} else {
				xevent.ExposeEvent.x = rc.X;
				xevent.ExposeEvent.y = rc.Y;
				xevent.ExposeEvent.width = rc.Width;
				xevent.ExposeEvent.height = rc.Height;
			}
			
			AddExpose (xevent);
		}
		
		internal override bool IsEnabled (IntPtr handle)
		{
			return Hwnd.ObjectFromHandle (handle).Enabled;
		}
		
		internal override bool IsVisible (IntPtr handle)
		{
			return Hwnd.ObjectFromHandle (handle).visible;
		}
		
		internal override void KillTimer (Timer timer)
		{
			lock (TimerList) {
				TimerList.Remove (timer);
			}
		}
		
		internal override void MenuToScreen (IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, hwnd.whole_window, RootWindow, x, y, out dest_x_return, out dest_y_return, out child);
			}
			
			x = dest_x_return;
			y = dest_y_return;
		}
		
		internal override void OverrideCursor (IntPtr cursor)
		{
			OverrideCursorHandle = cursor;
		}
		
		internal override PaintEventArgs PaintEventStart (IntPtr handle, bool client)
		{
			PaintEventArgs	paint_event;
			Hwnd		hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (Caret.Visible == 1) {
				Caret.Paused = true;
				HideCaret ();
			}
			
			if (client) {
				// handle backing store
				IntPtr gdk_window = gdk_window_lookup (hwnd.client_window);
				IntPtr gdk_pixmap = NewPixmap (gdk_window, hwnd.ClientRect.Width, hwnd.ClientRect.Height);
				
				backing_store [gdk_window] = gdk_pixmap;
				
				hwnd.client_dc = Graphics.FromHwnd (gdk_x11_drawable_get_xid (gdk_pixmap));
				hwnd.client_dc.SetClip (hwnd.invalid);
				paint_event = new PaintEventArgs (hwnd.client_dc, hwnd.invalid);
				hwnd.expose_pending = false;
				
				return paint_event;
			} else {
				hwnd.client_dc = Graphics.FromHwnd (hwnd.whole_window);
				paint_event = new PaintEventArgs (hwnd.client_dc, new Rectangle (0, 0, hwnd.width, hwnd.height));
				hwnd.nc_expose_pending = false;
				
				return paint_event;
			}
		}
		
		internal override void PaintEventEnd (IntPtr handle, bool client)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			hwnd.client_dc.Flush ();
			
			if (client) {
				// clients are already drawn to a backing store pixmap
				IntPtr gdk_window = gdk_window_lookup (hwnd.client_window);
				IntPtr gdk_pixmap = (IntPtr)backing_store [gdk_window];
				
				BlitOffscreenPixmap (gdk_pixmap, gdk_window, hwnd.Invalid);
				
				g_object_unref (gdk_pixmap);
				backing_store.Remove (gdk_pixmap);
				
				hwnd.ClearInvalidArea ();
			}
			
			hwnd.client_dc.Dispose ();
			hwnd.client_dc = null;
			
			if (Caret.Visible == 1) {
				ShowCaret ();
				Caret.Paused = false;
			}
		}
		
		internal override bool PeekMessage (ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags)
		{
			bool	pending;
			
			// FIXME - imlement filtering
			
			if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0) {
				throw new NotImplementedException ("PeekMessage PM_NOREMOVE is not implemented yet");	// FIXME - Implement PM_NOREMOVE flag
			}
			
			pending = false;
			if (MessageQueue.Count > 0) {
				pending = true;
			} else {
				// Only call UpdateMessageQueue if real events are pending 
				// otherwise we go to sleep on the socket
				if (XPending (DisplayHandle) != 0) {
					UpdateMessageQueue ();
					pending = true;
				}
			}
			
			CheckTimers (DateTime.Now);
			
			if (!pending) {
				return false;
			}
			return GetMessage (ref msg, hWnd, wFilterMin, wFilterMax);
		}
		
		// FIXME - I think this should just enqueue directly
		internal override bool PostMessage (IntPtr handle, Msg message, IntPtr wparam, IntPtr lparam)
		{
			XEvent xevent = new XEvent ();
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			
			if (hwnd != null) {
				xevent.ClientMessageEvent.window = hwnd.whole_window;
			} else {
				xevent.ClientMessageEvent.window = IntPtr.Zero;
			}
			
			xevent.ClientMessageEvent.message_type = (IntPtr) PostAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = handle;
			xevent.ClientMessageEvent.ptr2 = (IntPtr) message;
			xevent.ClientMessageEvent.ptr3 = wparam;
			xevent.ClientMessageEvent.ptr4 = lparam;
			
			MessageQueue.Enqueue (xevent);
			
			return true;
		}
		
		internal override void PostQuitMessage (int exitCode)
		{
			XFlush (DisplayHandle);
			PostQuitState = true;
			
			// Remove our display handle from S.D
			Graphics.FromHdcInternal (IntPtr.Zero);
		}
		
		internal override void ScreenToClient (IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, RootWindow, hwnd.client_window, x, y, out dest_x_return, out dest_y_return, out child);
			}
			
			x = dest_x_return;
			y = dest_y_return;
		}
		
		internal override void ScreenToMenu (IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, RootWindow, hwnd.whole_window, x, y, out dest_x_return, out dest_y_return, out child);
			}
			
			x = dest_x_return;
			y = dest_y_return;
		}
		
		internal override void ScrollWindow (IntPtr handle, Rectangle area, int XAmount, int YAmount, bool with_children)
		{
			Hwnd		hwnd;
			IntPtr		gc;
			XGCValues	gc_values;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd.invalid != Rectangle.Empty) {
				// BIG FAT WARNING. This only works with how we use this function right now
				// where we basically still scroll the whole window, but work around areas
				// that are covered by our children
				
				hwnd.invalid.X += XAmount;
				hwnd.invalid.Y += YAmount;
				
				if (hwnd.invalid.X < 0) {
					hwnd.invalid.Width += hwnd.invalid.X;
					hwnd.invalid.X = 0;
				}
				
				if (hwnd.invalid.Y < 0) {
					hwnd.invalid.Height += hwnd.invalid.Y;
					hwnd.invalid.Y = 0;
				}
			}
			
			gc_values = new XGCValues ();
			
			if (with_children) {
				gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			}
			
			gc = XCreateGC (DisplayHandle, hwnd.client_window, 0, ref gc_values);
			
			XCopyArea (DisplayHandle, hwnd.client_window, hwnd.client_window, gc, area.X - XAmount, area.Y - YAmount, area.Width, area.Height, area.X, area.Y);
			
			// Generate an expose for the area exposed by the horizontal scroll
			if (XAmount > 0) {
				hwnd.AddInvalidArea (area.X, area.Y, XAmount, area.Height);
			} else if (XAmount < 0) {
				hwnd.AddInvalidArea (XAmount + area.X + area.Width, area.Y, -XAmount, area.Height);
			}
			
			// Generate an expose for the area exposed by the vertical scroll
			if (YAmount > 0) {
				hwnd.AddInvalidArea (area.X, area.Y, area.Width, YAmount);
			} else if (YAmount < 0) {
				hwnd.AddInvalidArea (area.X, YAmount + area.Y + area.Height, area.Width, -YAmount);
			}
			XFreeGC (DisplayHandle, gc);
			
			UpdateWindow (handle);
		}
		
		internal override void ScrollWindow (IntPtr handle, int XAmount, int YAmount, bool with_children)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.GetObjectFromWindow (handle);
			
			ScrollWindow (handle, hwnd.ClientRect, XAmount, YAmount, with_children);
		}
		
		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			XEvent xevent = new XEvent ();
			
			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = FosterParent;
			xevent.ClientMessageEvent.message_type = (IntPtr)AsyncAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) GCHandle.Alloc (method);
			
			MessageQueue.EnqueueLocked (xevent);
			
			WakeupMain ();
		}
		
		internal override IntPtr SendMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam)
		{
			return NativeWindow.WndProc (hwnd, message, wParam, lParam);
		}
		
		internal override void SetAllowDrop (IntPtr handle, bool value)
		{
			// We allow drop on all windows
		}
		
		internal override DragDropEffects StartDrag (IntPtr handle, object data,
							     DragDropEffects allowed_effects)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd == null)
				throw new ArgumentException ("Attempt to begin drag from invalid window handle (" + handle.ToInt32 () + ").");
			
			return Dnd.StartDrag (hwnd.client_window, data, allowed_effects);
		}
		
		internal override void SetBorderStyle (IntPtr handle, FormBorderStyle border_style)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			hwnd.border_style = border_style;
			
			XMoveResizeWindow (DisplayHandle, hwnd.client_window, hwnd.ClientRect.X, hwnd.ClientRect.Y, hwnd.ClientRect.Width, hwnd.ClientRect.Height);
			
			InvalidateWholeWindow (handle);
		}
		
		internal override void SetCaretPos (IntPtr handle, int x, int y)
		{
			if (Caret.Hwnd == handle) {
				Caret.Timer.Stop ();
				HideCaret ();
				
				Caret.X = x;
				Caret.Y = y;
				
				if (Caret.Visible == 1) {
					ShowCaret ();
					Caret.Timer.Start ();
				}
			}
		}
		
		internal override void SetCursor (IntPtr handle, IntPtr cursor)
		{
			Hwnd	hwnd;
			
			if (OverrideCursorHandle == IntPtr.Zero) {
				if ((LastCursorWindow == handle) && (LastCursorHandle == cursor)) {
					return;
				}
				
				LastCursorHandle = cursor;
				LastCursorWindow = handle;
				
				hwnd = Hwnd.ObjectFromHandle (handle);
				lock (XlibLock) {
					if (cursor != IntPtr.Zero) {
						XDefineCursor (DisplayHandle, hwnd.whole_window, cursor);
					} else {
						XUndefineCursor (DisplayHandle, hwnd.whole_window);
					}
				}
				return;
			}
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			lock (XlibLock) {
				XDefineCursor (DisplayHandle, hwnd.whole_window, OverrideCursorHandle);
			}
		}
		
		internal override void SetCursorPos (IntPtr handle, int x, int y)
		{
			if (handle == IntPtr.Zero) {
				lock (XlibLock) {
					XWarpPointer (DisplayHandle, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, x, y);
				}
				return;
			} else {
				Hwnd	hwnd;
				
				hwnd = Hwnd.ObjectFromHandle (handle);
				lock (XlibLock) {
					XWarpPointer (DisplayHandle, IntPtr.Zero, hwnd.client_window, 0, 0, 0, 0, x, y);
				}
				return;
			}
		}
		
		internal override void SetFocus (IntPtr handle)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (FocusWindow != IntPtr.Zero) {
				PostMessage (FocusWindow, Msg.WM_KILLFOCUS, hwnd.client_window, IntPtr.Zero);
			}
			PostMessage (hwnd.client_window, Msg.WM_SETFOCUS, FocusWindow, IntPtr.Zero);
			FocusWindow = hwnd.client_window;
			
			//XSetInputFocus(DisplayHandle, Hwnd.ObjectFromHandle(handle).client_window, RevertTo.None, IntPtr.Zero);
		}
		
		internal override void SetIcon (IntPtr handle, Icon icon)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			if (hwnd != null) {
				SetIcon (hwnd, icon);
			}
		}
		
		internal override void SetMenu (IntPtr handle, Menu menu)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			hwnd.menu = menu;
			
			// FIXME - do we need to trigger some resize?
		}
		
		internal override void SetModal (IntPtr handle, bool Modal)
		{
			if (Modal) {
				ModalWindows.Push (handle);
			} else {
				if (ModalWindows.Contains (handle)) {
					ModalWindows.Pop ();
				}
				if (ModalWindows.Count > 0) {
					Activate ((IntPtr)ModalWindows.Peek ());
				}
			}
		}
		
		internal override IntPtr SetParent (IntPtr handle, IntPtr parent)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			hwnd.parent = Hwnd.ObjectFromHandle (parent);
			
			lock (XlibLock) {
				#if DriverDebug
					Console.WriteLine("Parent for window {0:X} / {1:X} = {2:X} (Handle:{3:X})", hwnd.ClientWindow.ToInt32(), hwnd.WholeWindow.ToInt32(), hwnd.parent != null ? hwnd.parent.Handle.ToInt32() : 0, parent.ToInt32());
				#endif
				XReparentWindow (DisplayHandle, hwnd.whole_window, hwnd.parent.client_window, hwnd.x, hwnd.y);
			}
			
			return IntPtr.Zero;
		}
		
		internal override void SetTimer (Timer timer)
		{
			lock (TimerList) {
				TimerList.Add (timer);
			}
			WakeupMain ();
		}
		
		internal override bool SetTopmost (IntPtr handle, IntPtr handle_owner, bool enabled)
		{
			Hwnd	hwnd;
			Hwnd	hwnd_owner;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (handle_owner != IntPtr.Zero) {
				hwnd_owner = Hwnd.ObjectFromHandle (handle_owner);
			} else {
				hwnd_owner = null;
			}
			
			if (enabled) {
				lock (XlibLock) {
					if (hwnd_owner != null) {
						XSetTransientForHint (DisplayHandle, hwnd.whole_window, hwnd_owner.whole_window);
					} else {
						XSetTransientForHint (DisplayHandle, hwnd.whole_window, FosterParent);
					}
				}
			} else {
				lock (XlibLock) {
					XDeleteProperty (DisplayHandle, hwnd.whole_window, (int)Atom.XA_WM_TRANSIENT_FOR);
				}
			}
			return true;
		}
		
		internal override bool SetVisible (IntPtr handle, bool visible)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			hwnd.visible = visible;
			
			lock (XlibLock) {
				if (visible) {
					if (Control.FromHandle (handle) is Form) {
						FormWindowState	s;
						
						s = ((Form)Control.FromHandle (handle)).WindowState;
						
						XMapWindow (DisplayHandle, hwnd.whole_window);
						XMapWindow (DisplayHandle, hwnd.client_window);
						
						switch (s) {
						case FormWindowState.Minimized:	SetWindowState (handle, FormWindowState.Minimized); break;
						case FormWindowState.Maximized:	SetWindowState (handle, FormWindowState.Maximized); break;
						}
					} else {
						XMapWindow (DisplayHandle, hwnd.whole_window);
						XMapWindow (DisplayHandle, hwnd.client_window);
					}
				} else {
					XUnmapWindow (DisplayHandle, hwnd.whole_window);
				}
			}
			return true;
		}
		
		internal override void SetWindowMinMax (IntPtr handle, Rectangle maximized, Size min, Size max)
		{
			Hwnd		hwnd;
			XSizeHints	hints;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			if (hwnd == null) {
				return;
			}
			
			hints = new XSizeHints ();
			
			if (min != Size.Empty) {
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMinSize);
				hints.min_width = min.Width;
				hints.min_height = min.Height;
			}
			
			if (max != Size.Empty) {
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMaxSize);
				hints.max_width = max.Width;
				hints.max_height = max.Height;
			}
			
			XSetWMNormalHints (DisplayHandle, hwnd.whole_window, ref hints);
			
			if (maximized != Rectangle.Empty) {
				hints.flags = (IntPtr)XSizeHintsFlags.PPosition;
				hints.x = maximized.X;
				hints.y = maximized.Y;
				hints.width = maximized.Width;
				hints.height = maximized.Height;
				
				// Metacity does not seem to follow this constraint for maximized (zoomed) windows
				XSetZoomHints (DisplayHandle, hwnd.whole_window, ref hints);
			}
		}
		
		
		internal override void SetWindowPos (IntPtr handle, int x, int y, int width, int height)
		{
			Hwnd		hwnd;
			Rectangle	client_rect;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			// X requires a sanity check for width & height; otherwise it dies
			if (hwnd.zero_sized && width > 0 && height > 0) {
				if (hwnd.visible) {
					XMapWindow (DisplayHandle, hwnd.whole_window);
				}
				hwnd.zero_sized = false;
			}
			
			if (width < 1) {
				hwnd.zero_sized = true;
				XUnmapWindow (DisplayHandle, hwnd.whole_window);
			}
			
			if (height < 1) {
				hwnd.zero_sized = true;
				XUnmapWindow (DisplayHandle, hwnd.whole_window);
			}
			
			client_rect = Hwnd.GetClientRectangle (hwnd.border_style, hwnd.menu, hwnd.title_style, hwnd.caption_height, hwnd.tool_caption_height, width, height);
			
			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && 
			    (hwnd.width == width) && (hwnd.height == height) &&
			    (hwnd.ClientRect == client_rect)) {
				return;
			}
			
			if (!hwnd.zero_sized) {
				lock (XlibLock) {
					XMoveResizeWindow (DisplayHandle, hwnd.whole_window, x, y, width, height);
					XMoveResizeWindow (DisplayHandle, hwnd.client_window, client_rect.X, client_rect.Y, client_rect.Width, client_rect.Height);
				}
			}
			
			// Prevent an old queued ConfigureNotify from setting our width with outdated data, set it now
			hwnd.width = width;
			hwnd.height = height;
		}
		
		internal override void SetWindowState (IntPtr handle, FormWindowState state)
		{
			FormWindowState	current_state;
			Hwnd		hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			current_state = GetWindowState (handle);
			
			if (current_state == state) {
				return;
			}
			
			switch (state) {
			case FormWindowState.Normal: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Minimized) {
							XMapWindow (DisplayHandle, hwnd.whole_window);
							XMapWindow (DisplayHandle, hwnd.client_window);
						} else if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage (hwnd.whole_window, (IntPtr)(uint)NetAtoms [(int)NA._NET_WM_STATE], (IntPtr)2 /* toggle */, (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_HORZ], (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_VERT]);
						}
					}
					Activate (handle);
					return;
				}
				
			case FormWindowState.Minimized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage (hwnd.whole_window, (IntPtr)NetAtoms [(int)NA._NET_WM_STATE], (IntPtr)2 /* toggle */, (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_HORZ], (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_VERT]);
						}
						XIconifyWindow (DisplayHandle, hwnd.whole_window, ScreenNo);
					}
					return;
				}
				
			case FormWindowState.Maximized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Minimized) {
							XMapWindow (DisplayHandle, hwnd.whole_window);
							XMapWindow (DisplayHandle, hwnd.client_window);
						}
						
						SendNetWMMessage (hwnd.whole_window, (IntPtr)NetAtoms [(int)NA._NET_WM_STATE], (IntPtr)1 /* Add */, (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_HORZ], (IntPtr)NetAtoms [(int)NA._NET_WM_STATE_MAXIMIZED_VERT]);
					}
					Activate (handle);
					return;
				}
			}
		}
		
		internal override void SetWindowStyle (IntPtr handle, CreateParams cp)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			SetHwndStyles (hwnd, cp);
			SetWMStyles (hwnd, cp);
		}
		
		internal override void SetWindowTransparency (IntPtr handle, double transparency, Color key)
		{
			Hwnd	hwnd;
			uint	opacity;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd == null) {
				return;
			}
			
			hwnd.opacity = (uint)(0xffffffff * transparency);
			opacity = hwnd.opacity;
			
			if (hwnd.reparented) {
				XChangeProperty (DisplayHandle, XGetParent (hwnd.whole_window), NetAtoms [(int)NA._NET_WM_WINDOW_OPACITY], Atom.XA_CARDINAL, 32, PropertyMode.Replace, ref opacity, 1);
			}
		}
		
		internal override bool SetZOrder (IntPtr handle, IntPtr after_handle, bool top, bool bottom)
		{
			Hwnd	hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (top) {
				lock (XlibLock) {
					XRaiseWindow (DisplayHandle, hwnd.whole_window);
				}
				return true;
			} else if (!bottom) {
				Hwnd	after_hwnd = null;
				
				if (after_handle != IntPtr.Zero) {
					after_hwnd = Hwnd.ObjectFromHandle (after_handle);
				}
				
				XWindowChanges	values = new XWindowChanges ();
				
				if (after_hwnd == null) {
					throw new ArgumentNullException ("after_handle", "Need sibling to adjust z-order");
				}
				values.sibling = after_hwnd.whole_window;
				values.stack_mode = StackMode.Below;
				
				lock (XlibLock) {
					XConfigureWindow (DisplayHandle, hwnd.whole_window, ChangeWindowFlags.CWStackMode | ChangeWindowFlags.CWSibling, ref values);
				}
			} else {
				// Bottom
				lock (XlibLock) {
					XLowerWindow (DisplayHandle, hwnd.whole_window);
				}
				return true;
			}
			return false;
		}
		
		internal override void ShowCursor (bool show)
		{
			;	// FIXME - X11 doesn't 'hide' the cursor. we could create an empty cursor
		}
		
		internal override void StartLoop (Thread thread)
		{
			// Future place for prepping a new queue for this specific thread
		}
		
		internal override bool SupportsTransparency ()
		{
			// We need to check if the x compositing manager is running
			return true;
		}
		
		internal override bool SystrayAdd (IntPtr handle, string tip, Icon icon, out ToolTip tt)
		{
			GetSystrayManagerWindow ();
			
			if (SystrayMgrWindow != IntPtr.Zero) {
				uint[]		atoms;
				XSizeHints	size_hints;
				Hwnd		hwnd;
				
				hwnd = Hwnd.ObjectFromHandle (handle);
				#if DriverDebug
					Console.WriteLine("Adding Systray Whole:{0:X}, Client:{1:X}", hwnd.whole_window.ToInt32(), hwnd.client_window.ToInt32());
				#endif
				
				XUnmapWindow (DisplayHandle, hwnd.whole_window);
				XUnmapWindow (DisplayHandle, hwnd.client_window);
				
				// Oh boy.
				gdk_window_destroy (gdk_window_lookup (hwnd.client_window));
				hwnd.client_window = hwnd.whole_window;
				
				size_hints = new XSizeHints ();
				
				size_hints.flags = (IntPtr)(XSizeHintsFlags.PMinSize | XSizeHintsFlags.PMaxSize | XSizeHintsFlags.PBaseSize);
				size_hints.min_width = icon.Width;
				size_hints.min_height = icon.Height;
				
				size_hints.max_width = icon.Width;
				size_hints.max_height = icon.Height;
				
				size_hints.base_width = icon.Width;
				size_hints.base_height = icon.Height;
				XSetWMNormalHints (DisplayHandle, hwnd.whole_window, ref size_hints);
				
				atoms = new uint [2];
				atoms [0] = 1;	// Version 1
				atoms [1] = 0;	// We're not mapped
				
				// This line cost me 3 days...
				XChangeProperty (DisplayHandle, hwnd.whole_window, NetAtoms [(int)NA._XEMBED_INFO], NetAtoms [(int)NA._XEMBED_INFO], 32, PropertyMode.Replace, atoms, 2);
				
				// Need to pick some reasonable defaults
				tt = new ToolTip ();
				tt.AutomaticDelay = 100;
				tt.InitialDelay = 250;
				tt.ReshowDelay = 250;
				tt.ShowAlways = true;
				
				if ((tip != null) && (tip != string.Empty)) {
					tt.SetToolTip (Control.FromHandle (handle), tip);
					tt.Active = true;
				} else {
					tt.Active = false;
				}
				
				// Make sure the window exists
				XSync (DisplayHandle, hwnd.whole_window);
				
				SendNetClientMessage (SystrayMgrWindow, (IntPtr)NetAtoms [(int)NA._NET_SYSTEM_TRAY_OPCODE], IntPtr.Zero, (IntPtr)SystrayRequest.SYSTEM_TRAY_REQUEST_DOCK, hwnd.whole_window);
				return true;
			}
			
			tt = null;
			return false;
		}
		
		internal override bool SystrayChange (IntPtr handle, string tip, Icon icon, ref ToolTip tt)
		{
			Control	control;
			
			control = Control.FromHandle (handle);
			if (control != null && tt != null) {
				tt.SetToolTip (control, tip);
				tt.Active = true;
				return true;
			} else {
				return false;
			}
		}
		
		internal override void SystrayRemove (IntPtr handle, ref ToolTip tt)
		{
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			XUnmapWindow (DisplayHandle, hwnd.whole_window);
			SetParent (hwnd.whole_window, FosterParent);
			
			// The caller can now re-dock it later...
			if (tt != null) {
				tt.Dispose ();
				tt = null;
			}
		}
		
		internal override bool Text (IntPtr handle, string text)
		{
			lock (XlibLock) {
				gdk_window_set_title (gdk_window_lookup (Hwnd.ObjectFromHandle (handle).whole_window), text);
			}
			return true;
		}
		
		internal override bool TranslateMessage (ref MSG msg)
		{
			return Keyboard.TranslateMessage (ref msg);
		}
		
		internal override void UpdateWindow (IntPtr handle)
		{
			XEvent	xevent;
			Hwnd	hwnd;
			
			hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (!hwnd.visible || hwnd.expose_pending) {
				return;
			}
			
			#if not
			SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			#else
			xevent = new XEvent ();
			xevent.type = XEventName.Expose;
			xevent.ExposeEvent.display = DisplayHandle;
			xevent.ExposeEvent.window = hwnd.client_window;
			
			MessageQueue.Enqueue (xevent);
			hwnd.expose_pending = true;
			#endif
		}
		
		internal static IntPtr NewPixmap (IntPtr gdk_window, int width, int height)
		{
			return gdk_pixmap_new (gdk_window, width, height, 24); // FIXME: instead of 24, get the correct display depth
		}
		
		internal static void BlitOffscreenPixmap (IntPtr gdk_pixmap, IntPtr dest_drawable, Rectangle area)
		{
			IntPtr gdk_gc = gdk_gc_new (gdk_pixmap);
			
			gdk_draw_drawable (dest_drawable, gdk_gc, gdk_pixmap, area.X, area.Y, area.X, area.Y, area.Width, area.Height);
			
			g_object_unref (gdk_gc);
		}
		#endregion	// Public Static Methods
		
		#region Events
		internal override event EventHandler Idle;
		#endregion	// Events
		
		#region X11 Imports
		[DllImport ("libX11", EntryPoint="XSynchronize")]
		internal extern static IntPtr XSynchronize (IntPtr display, bool onoff);
		
		[DllImport ("libX11", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow (IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, SetWindowValuemask valuemask, ref XSetWindowAttributes attributes);
		[DllImport ("libX11", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow (IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int border, int background);
		[DllImport ("libX11", EntryPoint="XMapWindow")]
		internal extern static int XMapWindow (IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XUnmapWindow")]
		internal extern static int XUnmapWindow (IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XMapSubwindows")]
		internal extern static int XMapSubindows (IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XUnmapSubwindows")]
		internal extern static int XUnmapSubwindows (IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XRootWindow")]
		internal extern static IntPtr XRootWindow (IntPtr display, int screen_number);
		[DllImport ("libX11", EntryPoint="XNextEvent")]
		internal extern static IntPtr XNextEvent (IntPtr display, ref XEvent xevent);
		[DllImport ("libX11")]
		internal extern static int XConnectionNumber (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static int XPending (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static bool XCheckWindowEvent (IntPtr display, IntPtr window, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11")]
		internal extern static bool XCheckMaskEvent (IntPtr display, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput (IntPtr display, IntPtr window, EventMask mask);
		
		[DllImport ("libX11", EntryPoint="XReparentWindow")]
		internal extern static int XReparentWindow (IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		[DllImport ("libX11", EntryPoint="XMoveResizeWindow")]
		internal extern static int XMoveResizeWindow (IntPtr display, IntPtr window, int x, int y, int width, int height);
		
		[DllImport ("libX11", EntryPoint="XResizeWindow")]
		internal extern static int XResizeWindow (IntPtr display, IntPtr window, int width, int height);
		
		[DllImport ("libX11", EntryPoint="XGetWindowAttributes")]
		internal extern static int XGetWindowAttributes (IntPtr display, IntPtr window, ref XWindowAttributes attributes);
		
		[DllImport ("libX11", EntryPoint="XFlush")]
		internal extern static int XFlush (IntPtr display);
		
		[DllImport ("libX11", EntryPoint="XSetWMName")]
		internal extern static int XSetWMName (IntPtr display, IntPtr window, ref XTextProperty text_prop);
		
		[DllImport ("libX11", EntryPoint="XStoreName")]
		internal extern static int XStoreName (IntPtr display, IntPtr window, string window_name);
		
		[DllImport ("libX11", EntryPoint="XFetchName")]
		internal extern static int XFetchName (IntPtr display, IntPtr window, ref IntPtr window_name);
		
		[DllImport ("libX11", EntryPoint="XSendEvent")]
		internal extern static int XSendEvent (IntPtr display, IntPtr window, bool propagate, EventMask event_mask, ref XEvent send_event);
		
		[DllImport ("libX11", EntryPoint="XQueryTree")]
		internal extern static int XQueryTree (IntPtr display, IntPtr window, out IntPtr root_return, out IntPtr parent_return, out IntPtr children_return, out int nchildren_return);
		
		[DllImport ("libX11", EntryPoint="XFree")]
		internal extern static int XFree (IntPtr data);
		
		[DllImport ("libX11", EntryPoint="XRaiseWindow")]
		internal extern static int XRaiseWindow (IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XLowerWindow")]
		internal extern static uint XLowerWindow (IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XConfigureWindow")]
		internal extern static uint XConfigureWindow (IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values);
		
		[DllImport ("libX11", EntryPoint="XInternAtom")]
		internal extern static int XInternAtom (IntPtr display, string atom_name, bool only_if_exists);
		
		[DllImport ("libX11", EntryPoint="XSetWMProtocols")]
		internal extern static int XSetWMProtocols (IntPtr display, IntPtr window, uint[] protocols, int count);
		
		[DllImport ("libX11", EntryPoint="XGrabPointer")]
		internal extern static int XGrabPointer (IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, uint cursor, uint timestamp);
		
		[DllImport ("libX11", EntryPoint="XUngrabPointer")]
		internal extern static int XUngrabPointer (IntPtr display, uint timestamp);
		
		[DllImport ("libX11", EntryPoint="XQueryPointer")]
		internal extern static bool XQueryPointer (IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);
		
		[DllImport ("libX11", EntryPoint="XTranslateCoordinates")]
		internal extern static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return,	 out int dest_y_return, out IntPtr child_return);
		
		[DllImport ("libX11", EntryPoint="XWarpPointer")]
		internal extern static uint XWarpPointer (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);
		
		[DllImport ("libX11", EntryPoint="XClearWindow")]
		internal extern static int XClearWindow (IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XClearArea")]
		internal extern static int XClearArea (IntPtr display, IntPtr window, int x, int y, int width, int height, bool exposures);
		
		// Colormaps
		[DllImport ("libX11", EntryPoint="XDefaultScreenOfDisplay")]
		internal extern static IntPtr XDefaultScreenOfDisplay (IntPtr display);
		
		[DllImport ("libX11", EntryPoint="XScreenNumberOfScreen")]
		internal extern static int XScreenNumberOfScreen (IntPtr display, IntPtr Screen);
		
		[DllImport ("libX11", EntryPoint="XDefaultVisual")]
		internal extern static IntPtr XDefaultVisual (IntPtr display, int screen_number);
		
		[DllImport ("libX11", EntryPoint="XDefaultDepth")]
		internal extern static uint XDefaultDepth (IntPtr display, int screen_number);
		
		[DllImport ("libX11", EntryPoint="XDefaultColormap")]
		internal extern static IntPtr XDefaultColormap (IntPtr display, int screen_number);
		
		[DllImport ("libX11", EntryPoint="XLookupColor")]
		internal extern static int XLookupColor (IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);
		
		[DllImport ("libX11", EntryPoint="XAllocColor")]
		internal extern static int XAllocColor (IntPtr display, IntPtr Colormap, ref XColor colorcell_def);
		
		[DllImport ("libX11", EntryPoint="XSetTransientForHint")]
		internal extern static int XSetTransientForHint (IntPtr display, IntPtr window, IntPtr prop_window);
		
		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty (IntPtr display, IntPtr window, int property, int type, int format, PropertyMode  mode, ref MotifWmHints data, int nelements);
		
		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty (IntPtr display, IntPtr window, int property, Atom format, int type, PropertyMode  mode, uint[] atoms, int nelements);
		
		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty (IntPtr display, IntPtr window, int property, Atom format, int type, PropertyMode  mode, ref uint value, int nelements);
		
		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty (IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, uint[] atoms, int nelements);
		
		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty (IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, IntPtr atoms, int nelements);
		
		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty (IntPtr display, IntPtr window, int property, Atom format, int type, PropertyMode  mode, IntPtr atoms, int nelements);
		
		[DllImport ("libX11", EntryPoint="XChangeProperty", CharSet=CharSet.Ansi)]
		internal extern static int XChangeProperty (IntPtr display, IntPtr window, int property, int type, int format, PropertyMode  mode, string text, int text_length);
		
		[DllImport ("libX11", EntryPoint="XDeleteProperty")]
		internal extern static int XDeleteProperty (IntPtr display, IntPtr window, int property);
		
		// Drawing
		[DllImport ("libX11", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC (IntPtr display, IntPtr window, GCFunction valuemask, ref XGCValues values);
		
		[DllImport ("libX11", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC (IntPtr display, IntPtr gc);
		
		[DllImport ("libX11", EntryPoint="XSetFunction")]
		internal extern static int XSetFunction (IntPtr display, IntPtr gc, GXFunction function);
		
		[DllImport ("libX11", EntryPoint="XDrawLine")]
		internal extern static int XDrawLine (IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);
		
		[DllImport ("libX11", EntryPoint="XDrawRectangle")]
		internal extern static int XDrawRectangle (IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);
		
		[DllImport ("libX11", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground (IntPtr display, IntPtr window, IntPtr background);
		
		[DllImport ("libX11", EntryPoint="XCopyArea")]
		internal extern static int XCopyArea (IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);
		
		[DllImport ("libX11", EntryPoint="XGetAtomName")]
		internal extern static string XGetAtomName (IntPtr display, int atom);
		
		[DllImport ("libX11", EntryPoint="XGetWindowProperty")]
		internal extern static int XGetWindowProperty (IntPtr display, IntPtr window, int atom, int long_offset, int long_length, bool delete, Atom req_type, out Atom actual_type, out int actual_format, out int nitems, out int bytes_after, ref IntPtr prop);
		
		[DllImport ("libX11", EntryPoint="XSetInputFocus")]
		internal extern static int XSetInputFocus (IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);
		
		[DllImport ("libX11", EntryPoint="XIconifyWindow")]
		internal extern static int XIconifyWindow (IntPtr display, IntPtr window, int screen_number);
		
		[DllImport ("libX11", EntryPoint="XDefineCursor")]
		internal extern static int XDefineCursor (IntPtr display, IntPtr window, IntPtr cursor);
		
		[DllImport ("libX11", EntryPoint="XUndefineCursor")]
		internal extern static int XUndefineCursor (IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XFreeCursor")]
		internal extern static int XFreeCursor (IntPtr display, IntPtr cursor);
		
		[DllImport ("libX11", EntryPoint="XCreateFontCursor")]
		internal extern static IntPtr XCreateFontCursor (IntPtr display, CursorFontShape shape);
		
		[DllImport ("libX11", EntryPoint="XCreatePixmapCursor")]
		internal extern static IntPtr XCreatePixmapCursor (IntPtr display, IntPtr source, IntPtr mask, ref XColor foreground_color, ref XColor background_color, int x_hot, int y_hot);
		
		[DllImport ("libX11", EntryPoint="XCreatePixmapFromBitmapData")]
		internal extern static IntPtr XCreatePixmapFromBitmapData (IntPtr display, IntPtr drawable, byte[] data, int width, int height, IntPtr fg, IntPtr bg, int depth);
		
		[DllImport ("libX11", EntryPoint="XFreePixmap")]
		internal extern static IntPtr XFreePixmap (IntPtr display, IntPtr pixmap);
		
		[DllImport ("libX11", EntryPoint="XWhitePixel")]
		internal extern static IntPtr XWhitePixel (IntPtr display, int screen_no);
		
		[DllImport ("libX11", EntryPoint="XBlackPixel")]
		internal extern static IntPtr XBlackPixel (IntPtr display, int screen_no);
		
		[DllImport ("libX11", EntryPoint="XGrabServer")]
		internal extern static void XGrabServer (IntPtr display);
		
		[DllImport ("libX11", EntryPoint="XUngrabServer")]
		internal extern static void XUngrabServer (IntPtr display);
		
		[DllImport ("libX11", EntryPoint="XSetWMNormalHints")]
		internal extern static void XSetWMNormalHints (IntPtr display, IntPtr window, ref XSizeHints hints);
		
		[DllImport ("libX11", EntryPoint="XSetZoomHints")]
		internal extern static void XSetZoomHints (IntPtr display, IntPtr window, ref XSizeHints hints);
		
		[DllImport ("libX11", EntryPoint="XSetWMHints")]
		internal extern static void XSetWMHints (IntPtr display, IntPtr window, ref XWMHints wmhints);
		
		[DllImport ("libX11", EntryPoint="XSync")]
		internal extern static void XSync (IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XGetIconSizes")]
		internal extern static int XGetIconSizes (IntPtr display, IntPtr window, out IntPtr size_list, out int count);
		
		[DllImport ("libX11", EntryPoint="XSetErrorHandler")]
		internal extern static IntPtr XSetErrorHandler (XErrorHandler error_handler);
		
		[DllImport ("libX11", EntryPoint="XGetErrorText")]
		internal extern static IntPtr XGetErrorText (IntPtr display, byte code, StringBuilder buffer, int length);
		
		[DllImport ("libX11", EntryPoint="XInitThreads")]
		internal extern static int XInitThreads ();
		
		[DllImport ("libX11", EntryPoint="XConvertSelection")]
		internal extern static int XConvertSelection (IntPtr display, int selection, int target, int property, IntPtr requestor, IntPtr time);
		
		[DllImport ("libX11", EntryPoint="XGetSelectionOwner")]
		internal extern static IntPtr XGetSelectionOwner (IntPtr display, int selection);
		
		[DllImport ("libX11", EntryPoint="XSetSelectionOwner")]
		internal extern static int XSetSelectionOwner (IntPtr display, int selection, IntPtr owner, IntPtr time);
		
		[DllImport ("libX11", EntryPoint="XSetPlaneMask")]
		internal extern static int XSetPlaneMask (IntPtr display, IntPtr gc, uint mask);
		
		[DllImport ("libX11", EntryPoint="XSetForeground")]
		internal extern static int XSetForeground (IntPtr display, IntPtr gc, uint foreground);
		
		[DllImport ("libX11", EntryPoint="XSetBackground")]
		internal extern static int XSetBackground (IntPtr display, IntPtr gc, uint background);
		
		#endregion
		
		#region gdk imports
		[DllImport("libgdk-x11-2.0.so")]
		static extern bool gdk_init_check (IntPtr argc, IntPtr argv);
		
		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_x11_display_get_xdisplay (IntPtr display);
		
		[DllImport("libgdk-x11-2.0.so")]
		internal static extern IntPtr gdk_display_get_default ();
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_pixmap_new (IntPtr drawable, int width, int height, int depth);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_x11_drawable_get_xid (IntPtr gdkdrawable);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_draw_drawable (IntPtr drawable_dest, IntPtr gdk_gc, IntPtr drawable_src, int xsrc, int ysrc, int xdest, int ydest, int width, int height);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_gc_new (IntPtr drawable);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_window_foreign_new (IntPtr anid);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_x11_lookup_xdisplay (IntPtr xdisplay);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_display_close (IntPtr display);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_display_beep (IntPtr display);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_display_sync (IntPtr display);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_get_default_root_window ();
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_colormap_get_system ();
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_x11_colormap_get_xcolormap  (IntPtr gdk_colormap);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_destroy (IntPtr gdk_window);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_x11_grab_server ();
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_x11_ungrab_server ();
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_display_flush (IntPtr gdk_display);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_iconify (IntPtr gdk_window);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_deiconify (IntPtr gdk_window);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_set_decorations (IntPtr gdk_window, int decorations);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_screen_get_default ();
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern int gdk_screen_get_number (IntPtr gdk_screen);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_window_lookup (IntPtr anid);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_window_new (IntPtr gdk_parent, ref GdkWindowAttr gdk_window_attributes, int attributes_mask);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_set_events (IntPtr gdk_window, int event_mask);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_show (IntPtr window);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_set_title (IntPtr gdk_window, string title);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern int gdk_window_get_origin (IntPtr gdk_window, out int x, out int y);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_window_get_geometry (IntPtr gdk_window, out int x, out int y, out int width, out int height, out int depth);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_property_change (IntPtr gdk_window, /*GdkAtom*/IntPtr property, /*GdkAtom*/IntPtr type, int format, int gdk_prop_mode, /*const guchar **/ IntPtr data, int nelements);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern IntPtr gdk_window_get_parent (IntPtr gdk_window);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern void gdk_display_get_maximal_cursor_size (IntPtr gdk_display, out uint width, out uint height);
		
		[DllImport("libgdk-x11-2.0.so")]
		static extern int gdk_visual_get_best_depth ();
		#endregion
		
		#region gobject imports
		[DllImport("libglib-2.0.so")]
		static extern void g_free (IntPtr mem);
		
		[DllImport("libgobject-2.0.so")]
		static extern void g_object_unref (IntPtr nativeObject);
		#endregion
	}
}
