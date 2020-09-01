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

// NOT COMPLETE

// define to log Window handles and relationships to stdout
#undef DriverDebug

// Extra detailed debug
#undef DriverDebugExtra
#undef DriverDebugParent
#undef DriverDebugCreate
#undef DriverDebugDestroy
#undef DriverDebugThreads
#undef DriverDebugXEmbed

//#define TRACE
//#define DEBUG

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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Mono.Unix.Native;
using Mono.Unix;

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11 : XplatUIDriver {
		#region Local Variables
		// General
		static volatile XplatUIX11	Instance;
		static int		RefCount;
		static object		XlibLock;		// Our locking object
		static bool		themes_enabled;

		// General X11
		static IntPtr		DisplayHandle;		// X11 handle to display
		static int		ScreenNo;		// Screen number used
		static IntPtr		DefaultColormap;	// Colormap for screen
		static IntPtr		CustomVisual;		// Visual for window creation
		static IntPtr		CustomColormap;		// Colormap for window creation
		static IntPtr		RootWindow;		// Handle of the root window for the screen/display
		static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		static XErrorHandler	ErrorHandler;		// Error handler delegate
		static bool		ErrorExceptions;	// Throw exceptions on X errors
		int                     render_major_opcode;
		int                     render_first_event;
		int                     render_first_error;

		// Clipboard
		static IntPtr 		ClipMagic;
		static ClipboardData	Clipboard;		// Our clipboard

		// Communication
		static IntPtr		PostAtom;		// PostMessage atom
		static IntPtr		AsyncAtom;		// Support for async messages

		// Message Loop
		static Hashtable	MessageQueues;		// Holds our thread-specific XEventQueues
		static ArrayList	unattached_timer_list; // holds timers that are enabled but not attached to a window.
		static Pollfd[]		pollfds;		// For watching the X11 socket
		static bool wake_waiting;
		static object wake_waiting_lock = new object ();
		static X11Keyboard	Keyboard;		//
		static X11Dnd		Dnd;
		static UnixStream	wake;			//
		static UnixStream	wake_receive;		//
		static byte[]		network_buffer;		//
		static bool		detectable_key_auto_repeat;

		// Focus tracking
		static IntPtr		ActiveWindow;		// Handle of the active window
		static IntPtr		FocusWindow;		// Handle of the window with keyboard focus (if any)

		// Modality support
		static Stack		ModalWindows;		// Stack of our modal windows

		// Systray
		static IntPtr		SystrayMgrWindow;	// Handle of the Systray Manager window

		// Cursors
		static IntPtr		LastCursorWindow;	// The last window we set the cursor on
		static IntPtr		LastCursorHandle;	// The handle that was last set on LastCursorWindow
		static IntPtr		OverrideCursorHandle;	// The cursor that is set to override any other cursors

		// Caret
		static CaretStruct	Caret;			//

		// Last window containing the pointer
		static IntPtr		LastPointerWindow;	// The last window containing the pointer

		// Shape extension
		bool? hasShapeExtension;

		// Our atoms
		static IntPtr WM_PROTOCOLS;
		static IntPtr WM_DELETE_WINDOW;
		static IntPtr WM_TAKE_FOCUS;
		//static IntPtr _NET_SUPPORTED;
		//static IntPtr _NET_CLIENT_LIST;
		//static IntPtr _NET_NUMBER_OF_DESKTOPS;
		static IntPtr _NET_DESKTOP_GEOMETRY;
		//static IntPtr _NET_DESKTOP_VIEWPORT;
		static IntPtr _NET_CURRENT_DESKTOP;
		//static IntPtr _NET_DESKTOP_NAMES;
		static IntPtr _NET_ACTIVE_WINDOW;
		static IntPtr _NET_WORKAREA;
		//static IntPtr _NET_SUPPORTING_WM_CHECK;
		//static IntPtr _NET_VIRTUAL_ROOTS;
		//static IntPtr _NET_DESKTOP_LAYOUT;
		//static IntPtr _NET_SHOWING_DESKTOP;
		//static IntPtr _NET_CLOSE_WINDOW;
		//static IntPtr _NET_MOVERESIZE_WINDOW;
		static IntPtr _NET_WM_MOVERESIZE;
		//static IntPtr _NET_RESTACK_WINDOW;
		//static IntPtr _NET_REQUEST_FRAME_EXTENTS;
		static IntPtr _NET_WM_NAME;
		//static IntPtr _NET_WM_VISIBLE_NAME;
		//static IntPtr _NET_WM_ICON_NAME;
		//static IntPtr _NET_WM_VISIBLE_ICON_NAME;
		//static IntPtr _NET_WM_DESKTOP;
		static IntPtr _NET_WM_WINDOW_TYPE;
		static IntPtr _NET_WM_STATE;
		//static IntPtr _NET_WM_ALLOWED_ACTIONS;
		//static IntPtr _NET_WM_STRUT;
		//static IntPtr _NET_WM_STRUT_PARTIAL;
		//static IntPtr _NET_WM_ICON_GEOMETRY;
		static IntPtr _NET_WM_ICON;
		//static IntPtr _NET_WM_PID;
		//static IntPtr _NET_WM_HANDLED_ICONS;
		static IntPtr _NET_WM_USER_TIME;
		static IntPtr _NET_FRAME_EXTENTS;
		//static IntPtr _NET_WM_PING;
		//static IntPtr _NET_WM_SYNC_REQUEST;
		static IntPtr _NET_SYSTEM_TRAY_S;
		//static IntPtr _NET_SYSTEM_TRAY_ORIENTATION;
		static IntPtr _NET_SYSTEM_TRAY_OPCODE;
		static IntPtr _NET_WM_STATE_MAXIMIZED_HORZ;
		static IntPtr _NET_WM_STATE_MAXIMIZED_VERT;
		static IntPtr _XEMBED;
		static IntPtr _XEMBED_INFO;
		static IntPtr _MOTIF_WM_HINTS;
		static IntPtr _NET_WM_STATE_SKIP_TASKBAR;
		static IntPtr _NET_WM_STATE_ABOVE;
		static IntPtr _NET_WM_STATE_MODAL;
		static IntPtr _NET_WM_STATE_HIDDEN;
		static IntPtr _NET_WM_CONTEXT_HELP;
		static IntPtr _NET_WM_WINDOW_OPACITY;
		//static IntPtr _NET_WM_WINDOW_TYPE_DESKTOP;
		//static IntPtr _NET_WM_WINDOW_TYPE_DOCK;
		//static IntPtr _NET_WM_WINDOW_TYPE_TOOLBAR;
		//static IntPtr _NET_WM_WINDOW_TYPE_MENU;
		static IntPtr _NET_WM_WINDOW_TYPE_UTILITY;
		//static IntPtr _NET_WM_WINDOW_TYPE_SPLASH;
		// static IntPtr _NET_WM_WINDOW_TYPE_DIALOG;
		static IntPtr _NET_WM_WINDOW_TYPE_NORMAL;
		static IntPtr CLIPBOARD;
		static IntPtr PRIMARY;
		//static IntPtr DIB;
		static IntPtr OEMTEXT;
		static IntPtr UTF8_STRING;
		static IntPtr UTF16_STRING;
		static IntPtr RICHTEXTFORMAT;
		static IntPtr TARGETS;

		// mouse hover message generation
		static HoverStruct	HoverState;		//

		// double click message generation
		static ClickStruct	ClickPending;		//

		// Support for mouse grab
		static GrabStruct	Grab;			//

		// State
		Point		mouse_position;		// Last position of mouse, in screen coords
		internal static MouseButtons	MouseState;		// Last state of mouse buttons
		internal static bool in_doevents;
		// 'Constants'
		static int		DoubleClickInterval;	// msec; max interval between clicks to count as double click

		const EventMask SelectInputMask = (EventMask.ButtonPressMask | 
						   EventMask.ButtonReleaseMask | 
						   EventMask.KeyPressMask | 
						   EventMask.KeyReleaseMask | 
						   EventMask.EnterWindowMask | 
						   EventMask.LeaveWindowMask |
						   EventMask.ExposureMask |
						   EventMask.FocusChangeMask |
						   EventMask.PointerMotionMask | 
						   EventMask.PointerMotionHintMask | 
						   EventMask.SubstructureNotifyMask);

		static readonly object lockobj = new object ();

		// messages WaitForHwndMwssage is waiting on
		static Hashtable	messageHold;

		#endregion	// Local Variables
		#region Constructors
		XplatUIX11()
		{
			// Handle singleton stuff first
			RefCount = 0;
			in_doevents = false;

			// Now regular initialization
			XlibLock = new object ();
			X11Keyboard.XlibLock = XlibLock;
			MessageQueues = Hashtable.Synchronized (new Hashtable(7));
			unattached_timer_list = ArrayList.Synchronized (new ArrayList (3));
			messageHold = Hashtable.Synchronized (new Hashtable(3));
			Clipboard = new ClipboardData ();
			XInitThreads();

			ErrorExceptions = false;

			// X11 Initialization
			SetDisplay(XOpenDisplay(IntPtr.Zero));
			X11DesktopColors.Initialize();

			
			// Disable keyboard autorepeat
			try {
				XkbSetDetectableAutoRepeat (DisplayHandle, true,  IntPtr.Zero);
				detectable_key_auto_repeat = true;
			} catch {
				Console.Error.WriteLine ("Could not disable keyboard auto repeat, will attempt to disable manually.");
				detectable_key_auto_repeat = false;
			}

			// Handle any upcoming errors; we re-set it here, X11DesktopColor stuff might have stolen it (gtk does)
			ErrorHandler = new XErrorHandler(HandleError);
			XSetErrorHandler(ErrorHandler);
		}

		~XplatUIX11() {
			// Remove our display handle from S.D
			Graphics.FromHdcInternal (IntPtr.Zero);
		}

		#endregion	// Constructors

		#region Singleton Specific Code
		public static XplatUIX11 GetInstance() {
			lock (lockobj) {
				if (Instance == null) {
					Instance=new XplatUIX11();
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
				XplatUIX11.GetInstance().SetDisplay(value);
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

#if DEBUG_shana
		internal static IntPtr DefaultColorMap {
			get {
				return DefaultColormap;
			}
		}
#endif
		#endregion

		#region XExceptionClass
		internal class XException : ApplicationException {
			IntPtr		Display;
			IntPtr		ResourceID;
			IntPtr		Serial;
			XRequest	RequestCode;
			byte		ErrorCode;
			byte		MinorCode;

			public XException(IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode) {
				this.Display = Display;
				this.ResourceID = ResourceID;
				this.Serial = Serial;
				this.RequestCode = RequestCode;
				this.ErrorCode = ErrorCode;
				this.MinorCode = MinorCode;
			}

			public override string Message {
				get {
					return GetMessage(Display, ResourceID, Serial, ErrorCode, RequestCode, MinorCode);
				}
			}

			public static string GetMessage(IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode) {
				StringBuilder	sb;
				string		x_error_text;
				string		error;
				string		hwnd_text;
				string		control_text;
				Hwnd		hwnd;
				Control		c;

				sb = new StringBuilder(160);
				XGetErrorText(Display, ErrorCode, sb, sb.Capacity);
				x_error_text = sb.ToString();
				hwnd = Hwnd.ObjectFromHandle(ResourceID);
				if (hwnd != null) {
					hwnd_text = hwnd.ToString();
					c = Control.FromHandle(hwnd.Handle);
					if (c != null) {
						control_text = c.ToString();
					} else {
						control_text = String.Format("<handle {0:X} non-existant>", hwnd.Handle.ToInt32());
					}
				} else {
					hwnd_text = "<null>";
					control_text = "<null>";
				}


				error = String.Format("\n  Error: {0}\n  Request:     {1:D} ({2})\n  Resource ID: 0x{3:X}\n  Serial:      {4}\n  Hwnd:        {5}\n  Control:     {6}", x_error_text, RequestCode, MinorCode, ResourceID.ToInt32(), Serial, hwnd_text, control_text);
				return error;
			}
		}
		#endregion	// XExceptionClass

		#region Internal Methods
		internal void SetDisplay(IntPtr display_handle)
		{
			if (display_handle != IntPtr.Zero) {
				Hwnd	hwnd;

				if ((DisplayHandle != IntPtr.Zero) && (FosterParent != IntPtr.Zero)) {
					hwnd = Hwnd.ObjectFromHandle(FosterParent);
					XDestroyWindow(DisplayHandle, FosterParent);
					hwnd.Dispose();
				}

				if (DisplayHandle != IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
				}

				DisplayHandle=display_handle;

				// We need to tell System.Drawing our DisplayHandle. FromHdcInternal has
				// been hacked to do this for us.
				Graphics.FromHdcInternal (DisplayHandle);

				// query for the render extension so
				// we can ignore the spurious
				// BadPicture errors that are
				// generated by cairo/render.
				XQueryExtension (DisplayHandle, "RENDER",
						 ref render_major_opcode, ref render_first_event, ref render_first_error);

				// Debugging support
				if (Environment.GetEnvironmentVariable ("MONO_XSYNC") != null) {
					XSynchronize(DisplayHandle, true);
				}

				if (Environment.GetEnvironmentVariable ("MONO_XEXCEPTIONS") != null) {
					ErrorExceptions = true;
				}

				// Generic X11 setup
				ScreenNo = XDefaultScreen(DisplayHandle);
				RootWindow = XRootWindow(DisplayHandle, ScreenNo);
				DefaultColormap = XDefaultColormap(DisplayHandle, ScreenNo);

				// Create the foster parent
				// it is important that border_width is kept in synch with the other XCreateWindow calls
				FosterParent=XCreateSimpleWindow(DisplayHandle, RootWindow, 0, 0, 1, 1, 0, UIntPtr.Zero, UIntPtr.Zero);
				if (FosterParent==IntPtr.Zero) {
					Console.WriteLine("XplatUIX11 Constructor failed to create FosterParent");
				}

				DebugHelper.WriteLine ("FosterParent created 0x{0:x}", FosterParent.ToInt32());

				hwnd = new Hwnd();
				hwnd.Queue = ThreadQueue(Thread.CurrentThread);
				hwnd.WholeWindow = FosterParent;
				hwnd.ClientWindow = FosterParent;

				// Create a HWND for RootWIndow as well, so our queue doesn't eat the events
				hwnd = new Hwnd();
				hwnd.Queue = ThreadQueue(Thread.CurrentThread);
				hwnd.whole_window = RootWindow;
				hwnd.ClientWindow = RootWindow;

				// To wake up when a timer is ready
				network_buffer = new byte[10];

				int[] pipefds = new int[2];
				Syscall.pipe (pipefds);
				wake = new UnixStream (pipefds [1]);

				// Make this non-blocking, so it doesn't
				// deadlock if too many wakes are sent
				// before the wake_receive end is polled
				Syscall.fcntl (pipefds [1], FcntlCommand.F_SETFL, Syscall.fcntl (pipefds [1], FcntlCommand.F_GETFL) | (int) OpenFlags.O_NONBLOCK);

				wake_receive = new UnixStream (pipefds [0]);

				pollfds = new Pollfd [2];
				pollfds [0] = new Pollfd ();
				pollfds [0].fd = XConnectionNumber (DisplayHandle);
				pollfds [0].events = PollEvents.POLLIN;

				pollfds [1] = new Pollfd ();
				pollfds [1].fd = pipefds [0];
				pollfds [1].events = PollEvents.POLLIN;

				Keyboard = new X11Keyboard(DisplayHandle, FosterParent);
				Dnd = new X11Dnd (DisplayHandle, Keyboard);

				DoubleClickInterval = 500;

				HoverState.Interval = 500;
				HoverState.Timer = new Timer();
				HoverState.Timer.Enabled = false;
				HoverState.Timer.Interval = HoverState.Interval;
				HoverState.Timer.Tick += new EventHandler(MouseHover);
				HoverState.Size = new Size(4, 4);
				HoverState.X = -1;
				HoverState.Y = -1;

				ActiveWindow = IntPtr.Zero;
				FocusWindow = IntPtr.Zero;
				ModalWindows = new Stack(3);

				MouseState = MouseButtons.None;
				mouse_position = new Point(0, 0);

				Caret.Timer = new Timer();
				Caret.Timer.Interval = 500;		// FIXME - where should this number come from?
				Caret.Timer.Tick += new EventHandler(CaretCallback);

				SetupAtoms();

				// Grab atom changes off the root window to catch certain WM events
				XSelectInput(DisplayHandle, RootWindow, new IntPtr ((int) (EventMask.PropertyChangeMask | Keyboard.KeyEventMask)));

				// Handle any upcoming errors
				ErrorHandler = new XErrorHandler(HandleError);
				XSetErrorHandler(ErrorHandler);
			} else {
				throw new ArgumentNullException("Display", "Could not open display (X-Server required. Check your DISPLAY environment variable)");
			}
		}
		#endregion	// Internal Methods

		#region Methods
		[Conditional ("DriverDebug")]
		static void DriverDebug (string format, params object [] args)
		{
			Console.WriteLine (String.Format (format, args));
		}
		
		int unixtime() {
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));

			return (int) t.TotalSeconds;
		}

		static void SetupAtoms() {
			// make sure this array stays in sync with the statements below
			string [] atom_names = new string[] {
				"WM_PROTOCOLS",
				"WM_DELETE_WINDOW",
				"WM_TAKE_FOCUS",
				//"_NET_SUPPORTED",
				//"_NET_CLIENT_LIST",
				//"_NET_NUMBER_OF_DESKTOPS",
				"_NET_DESKTOP_GEOMETRY",
				//"_NET_DESKTOP_VIEWPORT",
				"_NET_CURRENT_DESKTOP",
				//"_NET_DESKTOP_NAMES",
				"_NET_ACTIVE_WINDOW",
				"_NET_WORKAREA",
				//"_NET_SUPPORTING_WM_CHECK",
				//"_NET_VIRTUAL_ROOTS",
				//"_NET_DESKTOP_LAYOUT",
				//"_NET_SHOWING_DESKTOP",
				//"_NET_CLOSE_WINDOW",
				//"_NET_MOVERESIZE_WINDOW",
				"_NET_WM_MOVERESIZE",
				//"_NET_RESTACK_WINDOW",
				//"_NET_REQUEST_FRAME_EXTENTS",
				"_NET_WM_NAME",
				//"_NET_WM_VISIBLE_NAME",
				//"_NET_WM_ICON_NAME",
				//"_NET_WM_VISIBLE_ICON_NAME",
				//"_NET_WM_DESKTOP",
				"_NET_WM_WINDOW_TYPE",
				"_NET_WM_STATE",
				//"_NET_WM_ALLOWED_ACTIONS",
				//"_NET_WM_STRUT",
				//"_NET_WM_STRUT_PARTIAL",
				//"_NET_WM_ICON_GEOMETRY",
				"_NET_WM_ICON",
				//"_NET_WM_PID",
				//"_NET_WM_HANDLED_ICONS",
				"_NET_WM_USER_TIME",
				"_NET_FRAME_EXTENTS",
				//"_NET_WM_PING",
				//"_NET_WM_SYNC_REQUEST",
				"_NET_SYSTEM_TRAY_OPCODE",
				//"_NET_SYSTEM_TRAY_ORIENTATION",
				"_NET_WM_STATE_MAXIMIZED_HORZ",
				"_NET_WM_STATE_MAXIMIZED_VERT",
				"_NET_WM_STATE_HIDDEN",
				"_XEMBED",
				"_XEMBED_INFO",
				"_MOTIF_WM_HINTS",
				"_NET_WM_STATE_SKIP_TASKBAR",
				"_NET_WM_STATE_ABOVE",
				"_NET_WM_STATE_MODAL",
				"_NET_WM_CONTEXT_HELP",
				"_NET_WM_WINDOW_OPACITY",
				//"_NET_WM_WINDOW_TYPE_DESKTOP",
				//"_NET_WM_WINDOW_TYPE_DOCK",
				//"_NET_WM_WINDOW_TYPE_TOOLBAR",
				//"_NET_WM_WINDOW_TYPE_MENU",
				"_NET_WM_WINDOW_TYPE_UTILITY",
				// "_NET_WM_WINDOW_TYPE_DIALOG",
				//"_NET_WM_WINDOW_TYPE_SPLASH",
				"_NET_WM_WINDOW_TYPE_NORMAL",
				"CLIPBOARD",
				"PRIMARY",
				"COMPOUND_TEXT",
				"UTF8_STRING",
				"UTF16_STRING",
				"RICHTEXTFORMAT",
				"TARGETS",
				"_SWF_AsyncAtom",
				"_SWF_PostMessageAtom",
				"_SWF_HoverAtom" };

			IntPtr[] atoms = new IntPtr [atom_names.Length];;

			XInternAtoms (DisplayHandle, atom_names, atom_names.Length, false, atoms);

			int off = 0;
			WM_PROTOCOLS = atoms [off++];
			WM_DELETE_WINDOW = atoms [off++];
			WM_TAKE_FOCUS = atoms [off++];
			//_NET_SUPPORTED = atoms [off++];
			//_NET_CLIENT_LIST = atoms [off++];
			//_NET_NUMBER_OF_DESKTOPS = atoms [off++];
			_NET_DESKTOP_GEOMETRY = atoms [off++];
			//_NET_DESKTOP_VIEWPORT = atoms [off++];
			_NET_CURRENT_DESKTOP = atoms [off++];
			//_NET_DESKTOP_NAMES = atoms [off++];
			_NET_ACTIVE_WINDOW = atoms [off++];
			_NET_WORKAREA = atoms [off++];
			//_NET_SUPPORTING_WM_CHECK = atoms [off++];
			//_NET_VIRTUAL_ROOTS = atoms [off++];
			//_NET_DESKTOP_LAYOUT = atoms [off++];
			//_NET_SHOWING_DESKTOP = atoms [off++];
			//_NET_CLOSE_WINDOW = atoms [off++];
			//_NET_MOVERESIZE_WINDOW = atoms [off++];
			_NET_WM_MOVERESIZE = atoms [off++];
			//_NET_RESTACK_WINDOW = atoms [off++];
			//_NET_REQUEST_FRAME_EXTENTS = atoms [off++];
			_NET_WM_NAME = atoms [off++];
			//_NET_WM_VISIBLE_NAME = atoms [off++];
			//_NET_WM_ICON_NAME = atoms [off++];
			//_NET_WM_VISIBLE_ICON_NAME = atoms [off++];
			//_NET_WM_DESKTOP = atoms [off++];
			_NET_WM_WINDOW_TYPE = atoms [off++];
			_NET_WM_STATE = atoms [off++];
			//_NET_WM_ALLOWED_ACTIONS = atoms [off++];
			//_NET_WM_STRUT = atoms [off++];
			//_NET_WM_STRUT_PARTIAL = atoms [off++];
			//_NET_WM_ICON_GEOMETRY = atoms [off++];
			_NET_WM_ICON = atoms [off++];
			//_NET_WM_PID = atoms [off++];
			//_NET_WM_HANDLED_ICONS = atoms [off++];
			_NET_WM_USER_TIME = atoms [off++];
			_NET_FRAME_EXTENTS = atoms [off++];
			//_NET_WM_PING = atoms [off++];
			//_NET_WM_SYNC_REQUEST = atoms [off++];
			_NET_SYSTEM_TRAY_OPCODE = atoms [off++];
			//_NET_SYSTEM_TRAY_ORIENTATION = atoms [off++];
			_NET_WM_STATE_MAXIMIZED_HORZ = atoms [off++];
			_NET_WM_STATE_MAXIMIZED_VERT = atoms [off++];
			_NET_WM_STATE_HIDDEN = atoms [off++];
			_XEMBED = atoms [off++];
			_XEMBED_INFO = atoms [off++];
			_MOTIF_WM_HINTS = atoms [off++];
			_NET_WM_STATE_SKIP_TASKBAR = atoms [off++];
			_NET_WM_STATE_ABOVE = atoms [off++];
			_NET_WM_STATE_MODAL = atoms [off++];
			_NET_WM_CONTEXT_HELP = atoms [off++];
			_NET_WM_WINDOW_OPACITY = atoms [off++];
			//_NET_WM_WINDOW_TYPE_DESKTOP = atoms [off++];
			//_NET_WM_WINDOW_TYPE_DOCK = atoms [off++];
			//_NET_WM_WINDOW_TYPE_TOOLBAR = atoms [off++];
			//_NET_WM_WINDOW_TYPE_MENU = atoms [off++];
			_NET_WM_WINDOW_TYPE_UTILITY = atoms [off++];
			// _NET_WM_WINDOW_TYPE_DIALOG = atoms [off++];
			//_NET_WM_WINDOW_TYPE_SPLASH = atoms [off++];
			_NET_WM_WINDOW_TYPE_NORMAL = atoms [off++];
			CLIPBOARD = atoms [off++];
			PRIMARY = atoms [off++];
			OEMTEXT = atoms [off++];
			UTF8_STRING = atoms [off++];
			UTF16_STRING = atoms [off++];
			RICHTEXTFORMAT = atoms [off++];
			TARGETS = atoms [off++];
			AsyncAtom = atoms [off++];
			PostAtom = atoms [off++];
			HoverState.Atom = atoms [off++];

			//DIB = (IntPtr)Atom.XA_PIXMAP;
			_NET_SYSTEM_TRAY_S = XInternAtom (DisplayHandle, "_NET_SYSTEM_TRAY_S" + ScreenNo.ToString(), false);
		}

		void GetSystrayManagerWindow() {
			XGrabServer(DisplayHandle);
			SystrayMgrWindow = XGetSelectionOwner(DisplayHandle, _NET_SYSTEM_TRAY_S);
			XUngrabServer(DisplayHandle);
			XFlush(DisplayHandle);
		}

		void SendNetWMMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2) {
			SendNetWMMessage (window, message_type, l0, l1, l2, IntPtr.Zero);
		}

		void SendNetWMMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2, IntPtr l3) {
			XEvent	xev;

			xev = new XEvent();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;
			xev.ClientMessageEvent.ptr4 = l3;
			XSendEvent(DisplayHandle, RootWindow, false, new IntPtr ((int) (EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask)), ref xev);
		}

		void SendNetClientMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2) {
			XEvent	xev;

			xev = new XEvent();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;
			XSendEvent(DisplayHandle, window, false, new IntPtr ((int)EventMask.NoEventMask), ref xev);
		}

		// For WM_LBUTTONDOWN, WM_MBUTTONDOWN, WM_RBUTTONDOWN, WM_XBUTTONDOWN
		//     WM_CREATE and WM_DESTROY causes
		void SendParentNotify(IntPtr child, Msg cause, int x, int y)
		{	
			Hwnd hwnd;
			
			if (child == IntPtr.Zero) {
				return;
			}
			
			hwnd = Hwnd.GetObjectFromWindow (child);
			
			if (hwnd == null) {
				return;
			}
			
			if (hwnd.Handle == IntPtr.Zero) {
				return;
			}
			
			if (ExStyleSet ((int) hwnd.initial_ex_style, WindowExStyles.WS_EX_NOPARENTNOTIFY)) {
				return;
			}
			
			if (hwnd.Parent == null) {
				return;
			}
			
			if (hwnd.Parent.Handle == IntPtr.Zero) {
				return;
			}

			if (cause == Msg.WM_CREATE || cause == Msg.WM_DESTROY) {
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), child);
			} else {
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Control.MakeParam(x, y));
			}
			
			SendParentNotify (hwnd.Parent.Handle, cause, x, y);
		}
		
		bool StyleSet (int s, WindowStyles ws)
		{
			return (s & (int)ws) == (int)ws;
		}

		bool ExStyleSet (int ex, WindowExStyles exws)
		{
			return (ex & (int)exws) == (int)exws;
		}

		internal static Rectangle TranslateClientRectangleToXClientRectangle (Hwnd hwnd)
		{
			return TranslateClientRectangleToXClientRectangle (hwnd, Control.FromHandle (hwnd.Handle));
		}
		
		internal static Rectangle TranslateClientRectangleToXClientRectangle (Hwnd hwnd, Control ctrl)
		{
			/* 
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Rectangle rect = hwnd.ClientRect;
			Form form = ctrl as Form;
			CreateParams cp = null;

			if (form != null)
				cp = form.GetCreateParams ();

			if (form != null && (form.window_manager == null && !cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Rectangle xrect = rect;

				xrect.Y -= borders.top;
				xrect.X -= borders.left;
				xrect.Width += borders.left + borders.right;
				xrect.Height += borders.top + borders.bottom;

				rect = xrect;
			}
			
			if (rect.Width < 1 || rect.Height < 1) {
				rect.Width = 1;
				rect.Height = 1;
				rect.X = -5;
				rect.Y = -5;
			}
			
			return rect;
		}

		internal static Size TranslateWindowSizeToXWindowSize (CreateParams cp)
		{
			return TranslateWindowSizeToXWindowSize (cp, new Size (cp.Width, cp.Height));
		}

		internal static Size TranslateWindowSizeToXWindowSize (CreateParams cp, Size size)
		{
			/* 
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null && !cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size xrect = size;

				xrect.Width -= borders.left + borders.right;
				xrect.Height -= borders.top + borders.bottom;

				size = xrect;
			}
			if (size.Height == 0)
				size.Height = 1;
			if (size.Width == 0)
				size.Width = 1;
			return size;
		}

		internal static Size TranslateXWindowSizeToWindowSize (CreateParams cp, int xWidth, int xHeight)
		{
			/* 
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Size rect = new Size (xWidth, xHeight);
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null && !cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size xrect = rect;

				xrect.Width += borders.left + borders.right;
				xrect.Height += borders.top + borders.bottom;

				rect = xrect;
			}
			return rect;
		}
		
		internal static Point GetTopLevelWindowLocation (Hwnd hwnd)
		{
			IntPtr dummy; 
			int x, y;
			Hwnd.Borders frame;

			XTranslateCoordinates (DisplayHandle, hwnd.whole_window, RootWindow, 0, 0, out x, out y, out dummy);
			frame = FrameExtents (hwnd.whole_window);

			x -= frame.left;
			y -= frame.top;
			
			return new Point (x, y);
		}
		
		void DeriveStyles(int Style, int ExStyle, out FormBorderStyle border_style, out bool border_static, out TitleStyle title_style, out int caption_height, out int tool_caption_height) {

			caption_height = 0;
			tool_caption_height = 19;
			border_static = false;

			if (StyleSet (Style, WindowStyles.WS_CHILD)) {
				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
				} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
					border_static = true;
				} else if (!StyleSet (Style, WindowStyles.WS_BORDER)) {
					border_style = FormBorderStyle.None;
				} else {
					border_style = FormBorderStyle.FixedSingle;
				}
				title_style = TitleStyle.None;
				
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					caption_height = 19;
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_MDICHILD)) {
					caption_height = 19;

					if (StyleSet (Style, WindowStyles.WS_OVERLAPPEDWINDOW) ||
					    ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = (FormBorderStyle) 0xFFFF;
					} else {
						border_style = FormBorderStyle.None;
					}
				}

			} else {
				title_style = TitleStyle.None;
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				border_style = FormBorderStyle.None;

				if (StyleSet (Style, WindowStyles.WS_THICKFRAME)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = FormBorderStyle.SizableToolWindow;
					} else {
						border_style = FormBorderStyle.Sizable;
					}
				} else {
					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
							border_static = true;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME)) {
							border_style = FormBorderStyle.FixedDialog;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
							border_style = FormBorderStyle.FixedToolWindow;
						} else if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					} else {
						if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					}
				}
			}
		}

		void SetHwndStyles(Hwnd hwnd, CreateParams cp) {
			DeriveStyles(cp.Style, cp.ExStyle, out hwnd.border_style, out hwnd.border_static, out hwnd.title_style, out hwnd.caption_height, out hwnd.tool_caption_height);
		}

		void SetWMStyles(Hwnd hwnd, CreateParams cp) {
			MotifWmHints		mwmHints;
			MotifFunctions		functions;
			MotifDecorations	decorations;
			int[]			atoms;
			int			atom_count;
			Rectangle		client_rect;
			Form			form;
			IntPtr			window_type;
			bool			hide_from_taskbar;
			IntPtr			transient_for_parent;
			
			// Windows we manage ourselves don't need WM window styles.
			if (cp.HasWindowManager && !cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW)) {
				return;
			}

			atoms = new int[8];
			mwmHints = new MotifWmHints();
			functions = 0;
			decorations = 0;
			window_type = _NET_WM_WINDOW_TYPE_NORMAL;
			transient_for_parent = IntPtr.Zero;

			mwmHints.flags = (IntPtr)(MotifFlags.Functions | MotifFlags.Decorations);
			mwmHints.functions = (IntPtr)0;
			mwmHints.decorations = (IntPtr)0;

			form = cp.control as Form;

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
				/* tool windows get no window manager
				   decorations.
				*/

				/* just because the window doesn't get any decorations doesn't
				   mean we should disable the functions.  for instance, without
				   MotifFunctions.Maximize, changing the windowstate to Maximized
				   is ignored by metacity. */
				functions |= MotifFunctions.Move | MotifFunctions.Resize | MotifFunctions.Minimize | MotifFunctions.Maximize;
			} else if (form != null && form.FormBorderStyle == FormBorderStyle.None) {
				/* allow borderless window to be maximized */
				functions |= MotifFunctions.All | MotifFunctions.Resize;
			} else {
				if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
					functions |= MotifFunctions.Move;
					decorations |= MotifDecorations.Title | MotifDecorations.Menu;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_THICKFRAME)) {
					functions |= MotifFunctions.Move | MotifFunctions.Resize;
					decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZEBOX)) {
					functions |= MotifFunctions.Minimize;
					decorations |= MotifDecorations.Minimize;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZEBOX)) {
					functions |= MotifFunctions.Maximize;
					decorations |= MotifDecorations.Maximize;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_SIZEBOX)) {
					functions |= MotifFunctions.Resize;
					decorations |= MotifDecorations.ResizeH;
				}

				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME)) {
					decorations |= MotifDecorations.Border;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_BORDER)) {
					decorations |= MotifDecorations.Border;
				}
			
				if (StyleSet (cp.Style, WindowStyles.WS_DLGFRAME)) {
					decorations |= MotifDecorations.Border;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_SYSMENU)) {
					functions |= MotifFunctions.Close;
				}
				else {
					functions &= ~(MotifFunctions.Maximize | MotifFunctions.Minimize | MotifFunctions.Close);
					decorations &= ~(MotifDecorations.Menu | MotifDecorations.Maximize | MotifDecorations.Minimize);
					if (cp.Caption == "") {
						functions &= ~MotifFunctions.Move;
						decorations &= ~(MotifDecorations.Title | MotifDecorations.ResizeH);
					}
				}
			}

			if ((functions & MotifFunctions.Resize) == 0) {
				hwnd.fixed_size = true;
				Rectangle fixed_rectangle = new Rectangle (cp.X, cp.Y, cp.Width, cp.Height);
				SetWindowMinMax(hwnd.Handle, fixed_rectangle, fixed_rectangle.Size, fixed_rectangle.Size, cp);
			} else {
				hwnd.fixed_size = false;
			}

			mwmHints.functions = (IntPtr)functions;
			mwmHints.decorations = (IntPtr)decorations;

			DriverDebug ("SetWMStyles ({0}, {1}) functions = {2}, decorations = {3}", hwnd, cp, functions, decorations);

			if (cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW)) {
				// needed! map toolwindows to _NET_WM_WINDOW_TYPE_UTILITY to make newer metacity versions happy
				// and get those windows in front of their parents
				window_type = _NET_WM_WINDOW_TYPE_UTILITY;
			} else {
				window_type = _NET_WM_WINDOW_TYPE_NORMAL;
			}
			
			if (!cp.IsSet (WindowExStyles.WS_EX_APPWINDOW)) {
				hide_from_taskbar = true;
			} else if (cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW) &&  form != null && form.Parent != null && !form.ShowInTaskbar) {
				hide_from_taskbar = true;
			} else {
				hide_from_taskbar = false;
			}

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
				if (form != null && !hwnd.reparented) {
					if (form.Owner != null && form.Owner.Handle != IntPtr.Zero) {
						Hwnd owner_hwnd = Hwnd.ObjectFromHandle (form.Owner.Handle);
						if (owner_hwnd != null)
							transient_for_parent = owner_hwnd.whole_window;
					}
				}
			} 
			if (StyleSet (cp.Style, WindowStyles.WS_POPUP) && (hwnd.parent != null) && (hwnd.parent.whole_window != IntPtr.Zero)) {
				transient_for_parent = hwnd.parent.whole_window;
			}
			
			FormWindowState current_state = GetWindowState (hwnd.Handle);
			if (current_state == (FormWindowState)(-1))
				current_state = FormWindowState.Normal;

			client_rect = TranslateClientRectangleToXClientRectangle (hwnd);

			lock (XlibLock) {
				atom_count = 0;

				atoms [0] = window_type.ToInt32 ();
				XChangeProperty (DisplayHandle, hwnd.whole_window, _NET_WM_WINDOW_TYPE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);

				XChangeProperty(DisplayHandle, hwnd.whole_window, _MOTIF_WM_HINTS, _MOTIF_WM_HINTS, 32, PropertyMode.Replace, ref mwmHints, 5);

				if (transient_for_parent != IntPtr.Zero) {
					XSetTransientForHint (DisplayHandle, hwnd.whole_window, transient_for_parent);
				}

				MoveResizeWindow(DisplayHandle, hwnd.client_window, client_rect.X, client_rect.Y, client_rect.Width, client_rect.Height);

				if (hide_from_taskbar) {
					/* this line keeps the window from showing up in gnome's taskbar */
					atoms[atom_count++] = _NET_WM_STATE_SKIP_TASKBAR.ToInt32();
				}
				/* we need to add these atoms in the
				 * event we're maximized, since we're
				 * replacing the existing
				 * _NET_WM_STATE here.  If we don't
				 * add them, future calls to
				 * GetWindowState will return Normal
				 * for a window which is maximized. */
				if (current_state == FormWindowState.Maximized) {
					atoms[atom_count++] = _NET_WM_STATE_MAXIMIZED_HORZ.ToInt32();
					atoms[atom_count++] = _NET_WM_STATE_MAXIMIZED_VERT.ToInt32();
				}
				
				if (form != null && form.Modal) {
					atoms[atom_count++] = _NET_WM_STATE_MODAL.ToInt32 ();
				}
				
				XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, atom_count);

				atom_count = 0;
				IntPtr[] atom_ptrs = new IntPtr[2];
				atom_ptrs[atom_count++] = WM_DELETE_WINDOW;
				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_CONTEXTHELP)) {
					atom_ptrs[atom_count++] = _NET_WM_CONTEXT_HELP;
				}

				XSetWMProtocols(DisplayHandle, hwnd.whole_window, atom_ptrs, atom_count);
			}
		}

		void SetIcon(Hwnd hwnd, Icon icon)
		{
			if (icon == null) {
				// XXX

				// This really needs to do whatever it
				// takes to remove the window manager
				// menu, not just delete the ICON
				// property.  This will cause metacity
				// to use the "no icon set" icon, and
				// we'll still have an icon.
				XDeleteProperty (DisplayHandle, hwnd.whole_window, _NET_WM_ICON);
			}
			else {
				Bitmap		bitmap;
				int		size;
				IntPtr[]	data;
				int		index;

				bitmap = icon.ToBitmap();
				index = 0;
				size = bitmap.Width * bitmap.Height + 2;
				data = new IntPtr[size];

				data[index++] = (IntPtr)bitmap.Width;
				data[index++] = (IntPtr)bitmap.Height;

				for (int y = 0; y < bitmap.Height; y++) {
					for (int x = 0; x < bitmap.Width; x++) {
						data[index++] = (IntPtr)bitmap.GetPixel (x, y).ToArgb ();
					}
				}

				XChangeProperty (DisplayHandle, hwnd.whole_window,
						 _NET_WM_ICON, (IntPtr)Atom.XA_CARDINAL, 32,
						 PropertyMode.Replace, data, size);
			}
		}

		void WakeupMain () {
			try {
				wake.Write (new byte [] { 0xFF }, 0, 1);
			} catch (SocketException ex) {
				if (ex.SocketErrorCode != SocketError.WouldBlock) {
					throw;
				}
			}
		}

		XEventQueue ThreadQueue(Thread thread) {
			XEventQueue	queue;

			queue = (XEventQueue)MessageQueues[thread];
			if (queue == null) {
				queue = new XEventQueue(thread);
				MessageQueues[thread] = queue;
			}

			return queue;
		}

		void TranslatePropertyToClipboard(IntPtr property) {
			IntPtr			actual_atom;
			int			actual_format;
			IntPtr			nitems;
			IntPtr			bytes_after;
			IntPtr			prop = IntPtr.Zero;

			Clipboard.Item = null;

			XGetWindowProperty(DisplayHandle, FosterParent, property, IntPtr.Zero, new IntPtr (0x7fffffff), true, (IntPtr)Atom.AnyPropertyType, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

			if ((long)nitems > 0) {
				if (property == (IntPtr)Atom.XA_STRING) {
					// Xamarin-5116: PtrToStringAnsi expects to get UTF-8, but we might have
					// Latin-1 instead, in which case it will return null.
					var s = Marshal.PtrToStringAnsi (prop);
					if (string.IsNullOrEmpty (s)) {
						var sb = new StringBuilder ();
						for (int i = 0; i < (int)nitems; i++) {
							var b = Marshal.ReadByte (prop, i);
							sb.Append ((char)b);
						}
						s = sb.ToString ();
					}
					// Some X managers/apps pass unicode chars as escaped strings, so
					// we may need to unescape them.
					Clipboard.Item = UnescapeUnicodeFromAnsi (s);
				} else if (property == (IntPtr)Atom.XA_BITMAP) {
					// FIXME - convert bitmap to image
				} else if (property == (IntPtr)Atom.XA_PIXMAP) {
					// FIXME - convert pixmap to image
				} else if (property == OEMTEXT) {
					Clipboard.Item = UnescapeUnicodeFromAnsi (Marshal.PtrToStringAnsi(prop));
				} else if (property == UTF8_STRING) {
					byte [] buffer = new byte [(int)nitems];
					for (int i = 0; i < (int)nitems; i++)
						buffer [i] = Marshal.ReadByte (prop, i);
					Clipboard.Item = Encoding.UTF8.GetString (buffer);
				} else if (property == UTF16_STRING) {
					byte [] buffer = new byte [(int)nitems];
					for (int i = 0; i < (int)nitems; i++)
						buffer [i] = Marshal.ReadByte (prop, i);
					Clipboard.Item = Encoding.Unicode.GetString (buffer);
				} else if (property == RICHTEXTFORMAT)
					Clipboard.Item = Marshal.PtrToStringAnsi(prop);
				else if (DataFormats.ContainsFormat (property.ToInt32 ())) {
					if (DataFormats.GetFormat (property.ToInt32 ()).is_serializable) {
						MemoryStream memory_stream = new MemoryStream ((int)nitems);
						for (int i = 0; i < (int)nitems; i++)
							memory_stream.WriteByte (Marshal.ReadByte (prop, i));

						memory_stream.Position = 0;
						BinaryFormatter formatter = new BinaryFormatter ();
						Clipboard.Item = formatter.Deserialize (memory_stream);
						memory_stream.Close ();
					}
				}

				XFree(prop);
			}
		}

		string UnescapeUnicodeFromAnsi (string value)
		{
			if (value == null || value.IndexOf ("\\u") == -1)
				return value;

			StringBuilder sb = new StringBuilder (value.Length);
			int start, pos;

			start = pos = 0;
			while (start < value.Length) {
				pos = value.IndexOf ("\\u", start);
				if (pos == -1)
					break;

				sb.Append (value, start, pos - start);
				pos += 2;
				start = pos;

				int length = 0;
				while (pos < value.Length && length < 4) {
					if (!ValidHexDigit (value [pos]))
						break;
					length++;
					pos++;
				}

				int res;
				if (!Int32.TryParse (value.Substring (start, length), System.Globalization.NumberStyles.HexNumber, 
							null, out res))
					return value; // Error, return the unescaped original value.
				
				sb.Append ((char)res);
				start = pos;
			}

			// Append any remaining data.
			if (start < value.Length)
				sb.Append (value, start, value.Length - start);

			return sb.ToString ();
		}

		private static bool ValidHexDigit (char e)
		{
			return Char.IsDigit (e) || (e >= 'A' && e <= 'F') || (e >= 'a' && e <= 'f');
		}

		void AddExpose (Hwnd hwnd, bool client, int x, int y, int width, int height) {
			// Don't waste time
			if ((hwnd == null) || (x > hwnd.Width) || (y > hwnd.Height) || ((x + width) < 0) || ((y + height) < 0)) {
				return;
			}

			// Keep the invalid area as small as needed
			if ((x + width) > hwnd.width) {
				width = hwnd.width - x;
			}

			if ((y + height) > hwnd.height) {
				height = hwnd.height - y;
			}

			if (client) {
				hwnd.AddInvalidArea(x, y, width, height);
				if (!hwnd.expose_pending) {
					if (!hwnd.nc_expose_pending) {
						hwnd.Queue.Paint.Enqueue(hwnd);
					}
					hwnd.expose_pending = true;
				}
			} else {
				hwnd.AddNcInvalidArea (x, y, width, height);
				
				if (!hwnd.nc_expose_pending) {
					if (!hwnd.expose_pending) {
						hwnd.Queue.Paint.Enqueue(hwnd);
					}
					hwnd.nc_expose_pending = true;
				}
			}
		}

		static Hwnd.Borders FrameExtents (IntPtr window)
		{
			IntPtr actual_atom;
			int actual_format;
			IntPtr nitems;
			IntPtr bytes_after;
			IntPtr prop = IntPtr.Zero;
			Hwnd.Borders rect = new Hwnd.Borders ();

			XGetWindowProperty (DisplayHandle, window, _NET_FRAME_EXTENTS, IntPtr.Zero, new IntPtr (16), false, (IntPtr)Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if (prop != IntPtr.Zero) {
				if (nitems.ToInt32 () == 4) {
					rect.left = Marshal.ReadInt32 (prop, 0);
					rect.right = Marshal.ReadInt32 (prop, IntPtr.Size);
					rect.top = Marshal.ReadInt32 (prop, 2 * IntPtr.Size);
					rect.bottom = Marshal.ReadInt32 (prop, 3 * IntPtr.Size);
				}
				XFree (prop);
			}
			
			return rect;
		}

		void AddConfigureNotify (XEvent xevent) {
			Hwnd	hwnd;

			hwnd = Hwnd.GetObjectFromWindow(xevent.ConfigureEvent.window);

			// Don't waste time
			if (hwnd == null || hwnd.zombie) {
				return;
			}
			if ((xevent.ConfigureEvent.window == hwnd.whole_window)/* && (xevent.ConfigureEvent.window == xevent.ConfigureEvent.xevent)*/) {
				if (hwnd.parent == null) {
					// The location given by the event is not reliable between different wm's, 
					// so use an alternative way of getting it.
					Point location = GetTopLevelWindowLocation (hwnd);
					hwnd.x = location.X;
					hwnd.y = location.Y;
				}

				// XXX this sucks.  this isn't thread safe
				Control ctrl = Control.FromHandle (hwnd.Handle);
				Size TranslatedSize;
				if (ctrl != null) {
					TranslatedSize = TranslateXWindowSizeToWindowSize (ctrl.GetCreateParams (), xevent.ConfigureEvent.width, xevent.ConfigureEvent.height);
				} else {
					TranslatedSize = new Size (xevent.ConfigureEvent.width, xevent.ConfigureEvent.height);
				}
				hwnd.width = TranslatedSize.Width;
				hwnd.height = TranslatedSize.Height;
				hwnd.ClientRect = Rectangle.Empty;

				DriverDebug ("AddConfigureNotify (hwnd.Handle = {1}, final hwnd.rect = {0}, reported rect={2})",
					     new Rectangle (hwnd.x, hwnd.y, hwnd.width, hwnd.height), hwnd.Handle,
					     new Rectangle (xevent.ConfigureEvent.x, xevent.ConfigureEvent.y, xevent.ConfigureEvent.width, xevent.ConfigureEvent.width));
				lock (hwnd.configure_lock) {
					if (!hwnd.configure_pending) {
						hwnd.Queue.EnqueueLocked (xevent);
						hwnd.configure_pending = true;
					}
				}
			}
			// We drop configure events for Client windows
		}

		void ShowCaret() {
			if ((Caret.gc == IntPtr.Zero) || Caret.On) {
				return;
			}
			Caret.On = true;

			lock (XlibLock) {
				XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}

		void HideCaret() {
			if ((Caret.gc == IntPtr.Zero) || !Caret.On) {
				return;
			}
			Caret.On = false;

			lock (XlibLock) {
				XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}

		int NextTimeout (ArrayList timers, DateTime now) {
			int timeout = int.MaxValue; 

			foreach (Timer timer in timers) {
				int next = (int) (timer.Expires - now).TotalMilliseconds;
				if (next < 0) {
					return 0; // Have a timer that has already expired
				}

				if (next < timeout) {
					timeout = next;
				}
			}
			if (timeout < Timer.Minimum) {
				timeout = Timer.Minimum;
			}

			if (timeout > 1000)
				timeout = 1000;
			return timeout;
		}

		void CheckTimers (ArrayList timers, DateTime now) {
			int count;

			count = timers.Count;

			if (count == 0)
				return;

			for (int i = 0; i < timers.Count; i++) {
				Timer timer;

				timer = (Timer) timers [i];

				if (timer.Enabled && timer.Expires <= now && !timer.Busy) {
					// Timer ticks:
					//  - Before MainForm.OnLoad if DoEvents () is called.
					//  - After MainForm.OnLoad if not.
					//
					if (in_doevents ||
					    (Application.MWFThread.Current.Context != null && 
					     (Application.MWFThread.Current.Context.MainForm == null || 
					      Application.MWFThread.Current.Context.MainForm.IsLoaded))) {
						timer.Busy = true;
						timer.Update (now);
						timer.FireTick ();
						timer.Busy = false;
					}
				}
			}
		}

		void WaitForHwndMessage (Hwnd hwnd, Msg message) {
			WaitForHwndMessage (hwnd, message, false);

		}

		void WaitForHwndMessage (Hwnd hwnd, Msg message, bool process) {
			MSG msg = new MSG ();
			XEventQueue queue;

			queue = ThreadQueue(Thread.CurrentThread);

			queue.DispatchIdle = false;

			bool done = false;
			string key = hwnd.Handle + ":" + message;
			if (!messageHold.ContainsKey (key))	
				messageHold.Add (key, 1);
			else
				messageHold[key] = ((int)messageHold[key]) + 1;
			
					
			do {

				DebugHelper.WriteLine  ("Waiting for message " + message + " on hwnd " + String.Format("0x{0:x}", hwnd.Handle.ToInt32 ()));
				DebugHelper.Indent ();
				
				if (PeekMessage(queue, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
					if ((Msg)msg.message == Msg.WM_QUIT) {
						PostQuitMessage (0);
						done = true;
					}
					else {
						
						DebugHelper.WriteLine  ("PeekMessage got " + msg);
						
						if (msg.hwnd == hwnd.Handle) {
							if ((Msg)msg.message == message) {
								if (process) {
									TranslateMessage (ref msg);
									DispatchMessage (ref msg);
								}
								break;
							}
							else if ((Msg)msg.message == Msg.WM_DESTROY)
								done = true;
						}

						TranslateMessage (ref msg);
						DispatchMessage (ref msg);
					}
				}
				
				done = !messageHold.ContainsKey (key) || ((int)messageHold[key] < 1) || done;
			} while (!done);
						
			messageHold.Remove (key);

			DebugHelper.Unindent ();
			DebugHelper.WriteLine  ("Finished waiting for " + key);			

			queue.DispatchIdle = true;

		}

		void MapWindow(Hwnd hwnd, WindowType windows) {
			if (!hwnd.mapped) {
				Form f = Control.FromHandle(hwnd.Handle) as Form;
				if (f != null) {
					if (f.WindowState == FormWindowState.Normal) {
						f.waiting_showwindow = true;
						SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
					}
				}

				// it's possible that our Hwnd is no
				// longer valid after making that
				// SendMessage call, so check here.
				if (hwnd.zombie)
					return;

				if (hwnd.topmost) {
					// Most window managers will respect the _NET_WM_STATE property.
					// If not, use XMapRaised to map the window at the top level as
					// a last ditch effort.
					if ((windows & WindowType.Whole) != 0) {
						XMapRaised(DisplayHandle, hwnd.whole_window);
					}
					if ((windows & WindowType.Client) != 0) {
						XMapRaised(DisplayHandle, hwnd.client_window);
					}
				} else {
					if ((windows & WindowType.Whole) != 0) {
						XMapWindow(DisplayHandle, hwnd.whole_window);
					}
					if ((windows & WindowType.Client) != 0) {
						XMapWindow(DisplayHandle, hwnd.client_window);
					}
				}

				hwnd.mapped = true;

				if (f != null) {
					if (f.waiting_showwindow) {
						WaitForHwndMessage (hwnd, Msg.WM_SHOWWINDOW);
						CreateParams cp = f.GetCreateParams();
						if (!ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_MDICHILD) &&
						    !StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
							WaitForHwndMessage (hwnd, Msg.WM_ACTIVATE, true);
						}
					}
				}
			}
		}

		void UnmapWindow(Hwnd hwnd, WindowType windows) {
			if (hwnd.mapped) {
				Form f = null;
				if (Control.FromHandle(hwnd.Handle) is Form) {
					f = Control.FromHandle(hwnd.Handle) as Form;
					if (f.WindowState == FormWindowState.Normal) {
						f.waiting_showwindow = true;
						SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, IntPtr.Zero, IntPtr.Zero);
					}
				}

				// it's possible that our Hwnd is no
				// longer valid after making that
				// SendMessage call, so check here.
				// FIXME: it is likely wrong, as it has already sent WM_SHOWWINDOW
				if (hwnd.zombie)
					return;

				if ((windows & WindowType.Client) != 0) {
					XUnmapWindow(DisplayHandle, hwnd.client_window);
				}
				if ((windows & WindowType.Whole) != 0) {
					XUnmapWindow(DisplayHandle, hwnd.whole_window);
				}

				hwnd.mapped = false;

				if (f != null) {
					if (f.waiting_showwindow) {
						WaitForHwndMessage (hwnd, Msg.WM_SHOWWINDOW);
						CreateParams cp = f.GetCreateParams();
						if (!ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_MDICHILD) &&
						    !StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
							WaitForHwndMessage (hwnd, Msg.WM_ACTIVATE, true);
						}
					}
				}
			}
		}

		void UpdateMessageQueue (XEventQueue queue) {
			UpdateMessageQueue(queue, true);
		}

		void UpdateMessageQueue (XEventQueue queue, bool allowIdle) {
			DateTime	now;
			int		pending;
			Hwnd		hwnd;

			now = DateTime.UtcNow;

			lock (XlibLock) {
				pending = XPending (DisplayHandle);
			}

			if (pending == 0 && allowIdle) {
				if ((queue == null || queue.DispatchIdle) && Idle != null) {
					Idle (this, EventArgs.Empty);
				}

				lock (XlibLock) {
					pending = XPending (DisplayHandle);
				}
			}

			if (pending == 0) {
				int	timeout = 0;

				if (queue != null) {
					if (queue.Paint.Count > 0)
						return;

					timeout = NextTimeout (queue.timer_list, now);
				}

				if (timeout > 0) {
					int length = pollfds.Length - 1;
					lock (wake_waiting_lock) {
						if (wake_waiting == false) {
							length ++;
							wake_waiting = true;
						}
					}

					Syscall.poll (pollfds, (uint)length, timeout);
					// Clean out buffer, so we're not busy-looping on the same data
					if (length == pollfds.Length) {
						if (pollfds[1].revents != 0)
							wake_receive.Read(network_buffer, 0, 1);
						lock (wake_waiting_lock) {
							wake_waiting = false;
						}
					}
					lock (XlibLock) {
						pending = XPending (DisplayHandle);
					}
				}
			}

			if (queue != null)
				CheckTimers (queue.timer_list, now);

			while (true) {
				XEvent xevent = new XEvent ();

				lock (XlibLock) {
					if (XPending (DisplayHandle) == 0)
						break;

					XNextEvent (DisplayHandle, ref xevent);

					if (xevent.AnyEvent.type == XEventName.KeyPress ||
					    xevent.AnyEvent.type == XEventName.KeyRelease) {
						// PreFilter() handles "shift key state updates.
						Keyboard.PreFilter (xevent);
						if (XFilterEvent (ref xevent, Keyboard.ClientWindow)) {
							// probably here we could raise WM_IME_KEYDOWN and
							// WM_IME_KEYUP, but I'm not sure it is worthy.
							continue;
						}
					}
					else if (XFilterEvent (ref xevent, IntPtr.Zero))
						continue;
				}

				hwnd = Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);
				if (hwnd == null)
					continue;

				DebugHelper.WriteLine  ("UpdateMessageQueue got Event: " + xevent.ToString ());

				switch (xevent.type) {
				case XEventName.Expose:
					AddExpose (hwnd, xevent.ExposeEvent.window == hwnd.ClientWindow, xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
					break;

				case XEventName.SelectionClear: {
					// Should we do something?
					break;
				}

				case XEventName.SelectionRequest: {
					if (Dnd.HandleSelectionRequestEvent (ref xevent))
						break;
					XEvent sel_event;

					sel_event = new XEvent();
					sel_event.SelectionEvent.type = XEventName.SelectionNotify;
					sel_event.SelectionEvent.send_event = true;
					sel_event.SelectionEvent.display = DisplayHandle;
					sel_event.SelectionEvent.selection = xevent.SelectionRequestEvent.selection;
					sel_event.SelectionEvent.target = xevent.SelectionRequestEvent.target;
					sel_event.SelectionEvent.requestor = xevent.SelectionRequestEvent.requestor;
					sel_event.SelectionEvent.time = xevent.SelectionRequestEvent.time;
					sel_event.SelectionEvent.property = IntPtr.Zero;

					IntPtr format_atom = xevent.SelectionRequestEvent.target;

					// Seems that some apps support asking for supported types
					if (format_atom == TARGETS) {
						IntPtr[]	atoms;
						int	atom_count;

						atoms = new IntPtr[5];
						atom_count = 0;

						if (Clipboard.IsSourceText) {
							atoms[atom_count++] = (IntPtr)Atom.XA_STRING;
							atoms[atom_count++] = (IntPtr)OEMTEXT;
							atoms[atom_count++] = (IntPtr)UTF8_STRING;
							atoms[atom_count++] = (IntPtr)UTF16_STRING;
							atoms[atom_count++] = (IntPtr)RICHTEXTFORMAT;
						} else if (Clipboard.IsSourceImage) {
							atoms[atom_count++] = (IntPtr)Atom.XA_PIXMAP;
							atoms[atom_count++] = (IntPtr)Atom.XA_BITMAP;
						} else {
							// FIXME - handle other types
						}

						XChangeProperty(DisplayHandle, xevent.SelectionRequestEvent.requestor, (IntPtr)xevent.SelectionRequestEvent.property, 
								(IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, atom_count);
						sel_event.SelectionEvent.property = xevent.SelectionRequestEvent.property;
					} else if (format_atom == (IntPtr)RICHTEXTFORMAT) {
						string rtf_text = Clipboard.GetRtfText ();
						if (rtf_text != null) {
							// The RTF spec mentions that ascii is enough to contain it
							Byte [] bytes = Encoding.ASCII.GetBytes (rtf_text);
							int buflen = bytes.Length;
							IntPtr buffer = Marshal.AllocHGlobal (buflen);

							for (int i = 0; i < buflen; i++)
								Marshal.WriteByte (buffer, i, bytes[i]);

							XChangeProperty(DisplayHandle, xevent.SelectionRequestEvent.requestor, (IntPtr)xevent.SelectionRequestEvent.property,
									(IntPtr)xevent.SelectionRequestEvent.target, 8, PropertyMode.Replace, buffer, buflen);
							sel_event.SelectionEvent.property = xevent.SelectionRequestEvent.property;
							Marshal.FreeHGlobal(buffer);
						}
					} else if (Clipboard.IsSourceText && 
					           (format_atom == (IntPtr)Atom.XA_STRING 
					            || format_atom == OEMTEXT
					            || format_atom == UTF16_STRING
					            || format_atom == UTF8_STRING)) {
						IntPtr	buffer = IntPtr.Zero;
						int	buflen;
						Encoding encoding = null;

						buflen = 0;

						// Select an encoding depending on the target
						IntPtr target_atom = xevent.SelectionRequestEvent.target;
						if (target_atom == (IntPtr)Atom.XA_STRING || target_atom == OEMTEXT)
							// FIXME - EOMTEXT should encode into ISO2022
							encoding = Encoding.ASCII;
						else if (target_atom == UTF16_STRING)
							encoding = Encoding.Unicode;
						else if (target_atom == UTF8_STRING)
							encoding = Encoding.UTF8;

						Byte [] bytes;

						bytes = encoding.GetBytes (Clipboard.GetPlainText ());
						buffer = Marshal.AllocHGlobal (bytes.Length);
						buflen = bytes.Length;

						for (int i = 0; i < buflen; i++)
							Marshal.WriteByte (buffer, i, bytes [i]);

						if (buffer != IntPtr.Zero) {
							XChangeProperty(DisplayHandle, xevent.SelectionRequestEvent.requestor, (IntPtr)xevent.SelectionRequestEvent.property, (IntPtr)xevent.SelectionRequestEvent.target, 8, PropertyMode.Replace, buffer, buflen);
							sel_event.SelectionEvent.property = xevent.SelectionRequestEvent.property;
							Marshal.FreeHGlobal(buffer);
						}
					} else if (Clipboard.GetSource (format_atom.ToInt32 ()) != null) { // check if we have an available value of this format
						if (DataFormats.GetFormat (format_atom.ToInt32 ()).is_serializable) {
							object serializable = Clipboard.GetSource (format_atom.ToInt32 ());

							BinaryFormatter formatter = new BinaryFormatter ();
							MemoryStream memory_stream = new MemoryStream ();
							formatter.Serialize (memory_stream, serializable);

							int buflen = (int)memory_stream.Length;
							IntPtr buffer = Marshal.AllocHGlobal (buflen);
							memory_stream.Position = 0;
							for (int i = 0; i < buflen; i++)
								Marshal.WriteByte (buffer, i, (byte)memory_stream.ReadByte ());
							memory_stream.Close ();

							XChangeProperty (DisplayHandle, xevent.SelectionRequestEvent.requestor, (IntPtr)xevent.SelectionRequestEvent.property, (IntPtr)xevent.SelectionRequestEvent.target,
									8, PropertyMode.Replace, buffer, buflen);
							sel_event.SelectionEvent.property = xevent.SelectionRequestEvent.property;
							Marshal.FreeHGlobal (buffer);
						}

					} else if (Clipboard.IsSourceImage) {
						if (xevent.SelectionEvent.target == (IntPtr)Atom.XA_PIXMAP) {
							// FIXME - convert image and store as property
						} else if (xevent.SelectionEvent.target == (IntPtr)Atom.XA_PIXMAP) {
							// FIXME - convert image and store as property
						}
					}

					XSendEvent(DisplayHandle, xevent.SelectionRequestEvent.requestor, false, new IntPtr ((int)EventMask.NoEventMask), ref sel_event);
					break;
				}

				case XEventName.SelectionNotify: {
					if (Clipboard.Enumerating) {
						Clipboard.Enumerating = false;
						if (xevent.SelectionEvent.property != IntPtr.Zero) {
							XDeleteProperty(DisplayHandle, FosterParent, (IntPtr)xevent.SelectionEvent.property);
							if (!Clipboard.Formats.Contains(xevent.SelectionEvent.property)) {
								Clipboard.Formats.Add(xevent.SelectionEvent.property);
								DriverDebug("Got supported clipboard atom format: {0}", xevent.SelectionEvent.property);
							}
						}
					} else if (Clipboard.Retrieving) {
						Clipboard.Retrieving = false;
						if (xevent.SelectionEvent.property != IntPtr.Zero) {
							TranslatePropertyToClipboard(xevent.SelectionEvent.property);
						} else {
							Clipboard.ClearSources ();
							Clipboard.Item = null;
						}
					} else {
						Dnd.HandleSelectionNotifyEvent (ref xevent);
					}
					break;
				}

				case XEventName.KeyRelease:
					if (!detectable_key_auto_repeat && XPending (DisplayHandle) != 0) {
						XEvent nextevent = new XEvent ();

						XPeekEvent (DisplayHandle, ref nextevent);

						if (nextevent.type == XEventName.KeyPress &&
						    nextevent.KeyEvent.keycode == xevent.KeyEvent.keycode &&
						    nextevent.KeyEvent.time == xevent.KeyEvent.time) {
							continue;
						}
					}
					goto case XEventName.KeyPress;
					
				case XEventName.MotionNotify: {
					XEvent peek;

					/* we can't do motion compression across threads, so just punt if we don't match up */
					if (Thread.CurrentThread == hwnd.Queue.Thread && hwnd.Queue.Count > 0) {
						peek = hwnd.Queue.Peek();
						if (peek.AnyEvent.type == XEventName.MotionNotify) {
							continue;
						}
					}
					goto case XEventName.KeyPress;
				}

				case XEventName.KeyPress:
					hwnd.Queue.EnqueueLocked (xevent);
					/* Process KeyPresses immediately. Otherwise multiple Compose messages as a result of a
					 * single physical keypress are not processed correctly */
					return;
				case XEventName.ButtonPress:
				case XEventName.ButtonRelease:
				case XEventName.EnterNotify:
				case XEventName.LeaveNotify:
				case XEventName.CreateNotify:
				case XEventName.DestroyNotify:
				case XEventName.FocusIn:
				case XEventName.FocusOut:
				case XEventName.ClientMessage:
				case XEventName.ReparentNotify:
				case XEventName.MapNotify:
				case XEventName.UnmapNotify:
					hwnd.Queue.EnqueueLocked (xevent);
					break;

				case XEventName.ConfigureNotify:
					AddConfigureNotify(xevent);
					break;

				case XEventName.PropertyNotify:
					DriverDebug ("UpdateMessageQueue (), got Event: {0}", xevent.ToString ());
					if (xevent.PropertyEvent.atom == _NET_ACTIVE_WINDOW) {
						IntPtr	actual_atom;
						int	actual_format;
						IntPtr	nitems;
						IntPtr	bytes_after;
						IntPtr	prop = IntPtr.Zero;
						IntPtr	prev_active;

						prev_active = ActiveWindow;
						XGetWindowProperty(DisplayHandle, RootWindow, _NET_ACTIVE_WINDOW, IntPtr.Zero, new IntPtr (1), false, (IntPtr)Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
						if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
							ActiveWindow = Hwnd.GetHandleFromWindow((IntPtr)Marshal.ReadInt32(prop));							
							XFree(prop);

							DebugHelper.WriteLine ("PropertyNotify: _NET_ACTIVE_WINDOW: previous = 0x{0:x}, new = 0x{1:x}", prev_active.ToInt32 (), ActiveWindow.ToInt32 ());
							
							if (prev_active != ActiveWindow) {
								if (prev_active != IntPtr.Zero) {
									PostMessage(prev_active, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
								}
								if (ActiveWindow != IntPtr.Zero) {
									PostMessage(ActiveWindow, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
								}
							}
							if (ModalWindows.Count == 0) {
								break;
							} else {
								// Modality Handling
								// 
								// If there is a modal window on the stack and the new active 
								// window is MWF window, but not the modal one and not a non-modal 
								// child of the modal one, switch back to the modal window.
								//
								// To identify if a non-modal form is child of a modal form 
								// we match their ApplicationContexts, which will be the same.
								// This is because each modal form runs the loop with a 
								// new ApplicationContext, which is inherited by the non-modal 
								// forms.

								Form activeForm = Control.FromHandle (ActiveWindow) as Form;
								if (activeForm != null) {
									Form modalForm = Control.FromHandle ((IntPtr)ModalWindows.Peek()) as Form;
									if (ActiveWindow != (IntPtr)ModalWindows.Peek() && 
									    (modalForm == null || activeForm.context == modalForm.context)) {
										Activate((IntPtr)ModalWindows.Peek());
									}
								}
								break;
							}
						}
					}
					else if (xevent.PropertyEvent.atom == _NET_WM_STATE) {
						// invalidate our cache - we'll query again the next time someone does GetWindowState.
						hwnd.cached_window_state = (FormWindowState)(-1);
						PostMessage (hwnd.Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
					}
					break;

				}
			}
		}

		IntPtr GetMousewParam(int Delta) {
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
		IntPtr XGetParent(IntPtr handle) {
			IntPtr	Root;
			IntPtr	Parent;
			IntPtr	Children;
			int	ChildCount;

			lock (XlibLock) {
				XQueryTree(DisplayHandle, handle, out Root, out Parent, out Children, out ChildCount);
			}

			if (Children!=IntPtr.Zero) {
				lock (XlibLock) {
					XFree(Children);
				}
			}
			return Parent;
		}

		int HandleError (IntPtr display, ref XErrorEvent error_event)
		{
			// we need to workaround a problem with the
			// ordering of destruction of Drawables and
			// Pictures that exists between cairo and
			// RENDER on the server.
			if (error_event.request_code == (XRequest)render_major_opcode
			    && error_event.minor_code == 7 /* X_RenderFreePicture from render.h */
			    && error_event.error_code == render_first_error + 1 /* BadPicture from render.h */) {
				return 0;
			}

			if (ErrorExceptions) {
				XUngrabPointer (display, IntPtr.Zero);
				throw new XException (error_event.display, error_event.resourceid,
						      error_event.serial, error_event.error_code,
						      error_event.request_code, error_event.minor_code);
			} else {
				Console.WriteLine("X11 Error encountered: {0}{1}\n",
						  XException.GetMessage (error_event.display, error_event.resourceid,
									 error_event.serial, error_event.error_code,
									 error_event.request_code, error_event.minor_code),
						  Environment.StackTrace);
			}
			return 0;
		}

		void AccumulateDestroyedHandles (Control c, ArrayList list)
		{
			DebugHelper.Enter ();
			if (c != null) {
				
				Control[] controls = c.Controls.GetAllControls ();
				
				DebugHelper.WriteLine  ("Checking control:0x{0:x}", c.IsHandleCreated ? c.Handle.ToInt32() : 0);

				if (c.IsHandleCreated && !c.IsDisposed) {
					Hwnd hwnd = Hwnd.ObjectFromHandle(c.Handle);

					DriverDebug (" + adding {0} to the list of zombie windows", XplatUI.Window (hwnd.Handle));
					DriverDebug (" + parent X window is {0:X}", XGetParent (hwnd.whole_window).ToInt32());

					list.Add (hwnd);
					CleanupCachedWindows (hwnd);
				}

				for (int  i = 0; i < controls.Length; i ++) {
					AccumulateDestroyedHandles (controls[i], list);
				}				 
			}
			DebugHelper.Leave ();
		}

		void CleanupCachedWindows (Hwnd hwnd)
		{
			if (ActiveWindow == hwnd.Handle) {
				SendMessage(hwnd.client_window, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
				ActiveWindow = IntPtr.Zero;
			}

			if (FocusWindow == hwnd.Handle) {
				SendMessage(hwnd.client_window, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
				FocusWindow = IntPtr.Zero;
			}

			if (Grab.Hwnd == hwnd.Handle) {
				Grab.Hwnd = IntPtr.Zero;
				Grab.Confined = false;
			}

			DestroyCaret (hwnd.Handle);
		}

		void PerformNCCalc(Hwnd hwnd) {
			XplatUIWin32.NCCALCSIZE_PARAMS	ncp;
			IntPtr				ptr;
			Rectangle			rect;

			rect = new Rectangle (0, 0, hwnd.Width, hwnd.Height);

			ncp = new XplatUIWin32.NCCALCSIZE_PARAMS();
			ptr = Marshal.AllocHGlobal(Marshal.SizeOf(ncp));

			ncp.rgrc1.left = rect.Left;
			ncp.rgrc1.top = rect.Top;
			ncp.rgrc1.right = rect.Right;
			ncp.rgrc1.bottom = rect.Bottom;

			Marshal.StructureToPtr(ncp, ptr, true);
			NativeWindow.WndProc(hwnd.client_window, Msg.WM_NCCALCSIZE, (IntPtr)1, ptr);
			ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(ptr, typeof(XplatUIWin32.NCCALCSIZE_PARAMS));
			Marshal.FreeHGlobal(ptr);


			rect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
			hwnd.ClientRect = rect;
		
			rect = TranslateClientRectangleToXClientRectangle (hwnd);

			if (hwnd.visible) {
				MoveResizeWindow (DisplayHandle, hwnd.client_window, rect.X, rect.Y, rect.Width, rect.Height);
			}

			AddExpose (hwnd, hwnd.WholeWindow == hwnd.ClientWindow, 0, 0, hwnd.Width, hwnd.Height);
		}
		#endregion	// Methods

		#region	Callbacks
		void MouseHover(object sender, EventArgs e) {
			XEvent	xevent;
			Hwnd	hwnd;

			HoverState.Timer.Enabled = false;

			if (HoverState.Window != IntPtr.Zero) {
				hwnd = Hwnd.GetObjectFromWindow(HoverState.Window);
				if (hwnd != null) {
					xevent = new XEvent ();

					xevent.type = XEventName.ClientMessage;
					xevent.ClientMessageEvent.display = DisplayHandle;
					xevent.ClientMessageEvent.window = HoverState.Window;
					xevent.ClientMessageEvent.message_type = HoverState.Atom;
					xevent.ClientMessageEvent.format = 32;
					xevent.ClientMessageEvent.ptr1 = (IntPtr) (HoverState.Y << 16 | HoverState.X);

					hwnd.Queue.EnqueueLocked (xevent);

					WakeupMain ();
				}
			}
		}

		void CaretCallback(object sender, EventArgs e) {
			if (Caret.Paused) {
				return;
			}
			Caret.On = !Caret.On;

			XDrawLine(DisplayHandle, Caret.Hwnd, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
		}
		#endregion	// Callbacks

		#region Public Properties

		internal override int CaptionHeight {
			get {
				return 19;
			}
		}

		internal override  Size CursorSize {
			get {
				int	x;
				int	y;

				if (XQueryBestCursor(DisplayHandle, RootWindow, 32, 32, out x, out y) != 0) {
					return new Size(x, y);
				} else {
					return new Size(16, 16);
				}
			}
		} 

		internal override  bool DragFullWindows {
			get {
				return true;
			}
		} 

		internal override  Size DragSize {
			get {
				return new Size(4, 4);
			}
		}

		internal override  Size FrameBorderSize { 
			get {
				return new Size (4, 4);
			}
		}

		internal override  Size IconSize {
			get {
				IntPtr		list;
				XIconSize	size;
				int		count;

				if (XGetIconSizes(DisplayHandle, RootWindow, out list, out count) != 0) {
					long		current;
					int		largest;

					current = (long)list;
					largest = 0;

					size = new XIconSize();

					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure((IntPtr)current, size.GetType());
						current += Marshal.SizeOf(size);

						// Look for our preferred size
						if (size.min_width == 32) {
							XFree(list);
							return new Size(32, 32);
						}

						if (size.max_width == 32) {
							XFree(list);
							return new Size(32, 32);
						}

						if (size.min_width < 32 && size.max_width > 32) {
							int	x;

							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 32) {
									XFree(list);
									return new Size(32, 32);
								}
							}
						}

						if (largest < size.max_width) {
							largest = size.max_width;
						}
					}

					// We didn't find a match or we wouldn't be here
					return new Size(largest, largest);

				} else {
					return new Size(32, 32);
				}
			}
		} 

		internal override int KeyboardSpeed {
			get{
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

		internal override bool MenuAccessKeysUnderlined {
			get {
				return false;
			}
		}

		internal override  Size MinimizedWindowSpacingSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override  Size MinimumWindowSize {
			get {
				return new Size(110, 22);
			}
		} 

		internal override Size MinimumFixedToolWindowSize {
			get { return new Size (27, 22); }
		}

		internal override Size MinimumSizeableToolWindowSize {
			get { return new Size (37, 22); }
		}

		internal override Size MinimumNoBorderWindowSize {
			get { return new Size (2, 2); }
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

				if (XGetIconSizes(DisplayHandle, RootWindow, out list, out count) != 0) {
					long		current;
					int		smallest;

					current = (long)list;
					smallest = 0;

					size = new XIconSize();

					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure((IntPtr)current, size.GetType());
						current += Marshal.SizeOf(size);

						// Look for our preferred size
						if (size.min_width == 16) {
							XFree(list);
							return new Size(16, 16);
						}

						if (size.max_width == 16) {
							XFree(list);
							return new Size(16, 16);
						}

						if (size.min_width < 16 && size.max_width > 16) {
							int	x;

							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 16) {
									XFree(list);
									return new Size(16, 16);
								}
							}
						}

						if (smallest == 0 || smallest > size.min_width) {
							smallest = size.min_width;
						}
					}

					// We didn't find a match or we wouldn't be here
					return new Size(smallest, smallest);

				} else {
					return new Size(16, 16);
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

		internal override Point MousePosition {
			get {
				return mouse_position;
			}
		}

		internal override Size MouseHoverSize {
			get {
				return new Size (1, 1);
			}
		}

		internal override int MouseHoverTime {
			get {
				return HoverState.Interval;
			}
		}



		internal override  bool MouseWheelPresent {
			get {
				return true;	// FIXME - how to detect?
			}
		} 

		internal override MouseButtons MouseButtons {
			get {
				return MouseState;
			}
		}

		internal override  Rectangle VirtualScreen {
			get {
				IntPtr			actual_atom;
				int			actual_format;
				IntPtr			nitems;
				IntPtr			bytes_after;
				IntPtr			prop = IntPtr.Zero;
				int			width;
				int			height;

				XGetWindowProperty(DisplayHandle, RootWindow, _NET_DESKTOP_GEOMETRY, IntPtr.Zero, new IntPtr (256), false, (IntPtr)Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				if ((long)nitems < 2)
					goto failsafe;

				width = Marshal.ReadIntPtr(prop, 0).ToInt32();
				height = Marshal.ReadIntPtr(prop, IntPtr.Size).ToInt32();

				XFree(prop);

				return new Rectangle(0, 0, width, height);

			failsafe:
				XWindowAttributes	attributes=new XWindowAttributes();

				lock (XlibLock) {
					XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
				}

				return new Rectangle(0, 0, attributes.width, attributes.height);
			}
		} 

		internal override  Rectangle WorkingArea {
			get {
				IntPtr			actual_atom;
				int			actual_format;
				IntPtr			nitems;
				IntPtr			bytes_after;
				IntPtr			prop = IntPtr.Zero;
				int			width;
				int			height;
				int			current_desktop;
				int			x;
				int			y;

				XGetWindowProperty(DisplayHandle, RootWindow, _NET_CURRENT_DESKTOP, IntPtr.Zero, new IntPtr(1), false, (IntPtr)Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				if ((long)nitems < 1) {
					goto failsafe;
				}

				current_desktop = Marshal.ReadIntPtr(prop, 0).ToInt32();
				XFree(prop);

				XGetWindowProperty(DisplayHandle, RootWindow, _NET_WORKAREA, IntPtr.Zero, new IntPtr (256), false, (IntPtr)Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				if ((long)nitems < 4 * (current_desktop + 1)) {
					goto failsafe;
				}

				x = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop).ToInt32();
				y = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size).ToInt32();
				width = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size * 2).ToInt32();
				height = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size * 3).ToInt32();
				XFree(prop);

				return new Rectangle(x, y, width, height);

			failsafe:
				XWindowAttributes	attributes=new XWindowAttributes();

				lock (XlibLock) {
					XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
				}

				return new Rectangle(0, 0, attributes.width, attributes.height);
			}
		}

		internal override Screen[] AllScreens {
			get {
				if (!XineramaIsActive (DisplayHandle))
					return null;
				int nScreens;
				IntPtr xineramaScreens = XineramaQueryScreens (DisplayHandle, out nScreens);
				var screens = new Screen [nScreens];
				IntPtr current = xineramaScreens;
				for (int i = 0; i < nScreens; i++) {
					var screen = (XineramaScreenInfo)Marshal.PtrToStructure (current,
						typeof (XineramaScreenInfo));
					var screenRect = new Rectangle (screen.x_org, screen.y_org, screen.width,
						screen.height);
					var name = string.Format ("Display {0}", screen.screen_number);
					screens [i] = new Screen (i == 0, name, screenRect, screenRect);
					current = (IntPtr)( (ulong)current + (ulong)Marshal.SizeOf(typeof (XineramaScreenInfo)));
				}
				XFree (xineramaScreens);
				return screens;
			}
		}

		internal override bool ThemesEnabled {
			get {
				return XplatUIX11.themes_enabled;
			}
		}
 

		#endregion	// Public properties

		#region Public Static Methods
		internal override void RaiseIdle (EventArgs e)
		{
			if (Idle != null)
				Idle (this, e);
		}
		
		internal override IntPtr InitializeDriver()
		{
			lock (this) {
				if (DisplayHandle==IntPtr.Zero) {
					SetDisplay(XOpenDisplay(IntPtr.Zero));
				}
			}
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token)
		{
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}

		internal override void EnableThemes()
		{
			themes_enabled = true;
		}


		internal override void Activate(IntPtr handle)
		{
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				lock (XlibLock) {
					if (true /* the window manager supports NET_ACTIVE_WINDOW */) {
						SendNetWMMessage(hwnd.whole_window, _NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero);
						XEventQueue q = null;
						lock (unattached_timer_list) {
							foreach (Timer t in unattached_timer_list) {
								if (q == null)
									q= (XEventQueue) MessageQueues [Thread.CurrentThread];
								t.thread = q.Thread;
								q.timer_list.Add (t);
							}
							unattached_timer_list.Clear ();
						}
					}
// 					else {
// 						XRaiseWindow(DisplayHandle, handle);
// 					}
				}
			}
		}

		internal override void AudibleAlert(AlertType alert)
		{
			XBell(DisplayHandle, 0);
			return;
		}


		internal override void CaretVisible(IntPtr handle, bool visible)
		{
			if (Caret.Hwnd == handle) {
				if (visible) {
					if (!Caret.Visible) {
						Caret.Visible = true;
						ShowCaret();
						Caret.Timer.Start();
					}
				} else {
					Caret.Visible = false;
					Caret.Timer.Stop();
					HideCaret();
				}
			}
		}

		internal override bool CalculateWindowRect(ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect)
		{
			WindowRect = Hwnd.GetWindowRectangle (cp, menu, ClientRect);
			return true;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates(DisplayHandle, hwnd.client_window, RootWindow, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override int[] ClipboardAvailableFormats(IntPtr handle)
		{
			DataFormats.Format	f;
			int[]			result;

			f = DataFormats.Format.List;

			if (XGetSelectionOwner(DisplayHandle, CLIPBOARD) == IntPtr.Zero) {
				return null;
			}

			Clipboard.Formats = new ArrayList();

			while (f != null) {
				XConvertSelection(DisplayHandle, CLIPBOARD, (IntPtr)f.Id, (IntPtr)f.Id, FosterParent, IntPtr.Zero);

				var timeToWaitForSelectionFormats = TimeSpan.FromSeconds(4);
				var startTime = DateTime.Now;
				Clipboard.Enumerating = true;
				while (Clipboard.Enumerating) {
					UpdateMessageQueue(null, false);

					if (DateTime.Now - startTime > timeToWaitForSelectionFormats)
						break;
				}
				f = f.Next;
			}

			result = new int[Clipboard.Formats.Count];

			for (int i = 0; i < Clipboard.Formats.Count; i++) {
				result[i] = ((IntPtr)Clipboard.Formats[i]).ToInt32 ();
			}

			Clipboard.Formats = null;
			return result;
		}

		internal override void ClipboardClose(IntPtr handle)
		{
			if (handle != ClipMagic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}
			return;
		}

		internal override int ClipboardGetID(IntPtr handle, string format)
		{
			if (handle != ClipMagic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}

			if (format == "Text" ) return (int)Atom.XA_STRING;
			else if (format == "Bitmap" ) return (int)Atom.XA_BITMAP;
			//else if (format == "MetaFilePict" ) return 3;
			//else if (format == "SymbolicLink" ) return 4;
			//else if (format == "DataInterchangeFormat" ) return 5;
			//else if (format == "Tiff" ) return 6;
			else if (format == "OEMText" ) return OEMTEXT.ToInt32();
			else if (format == "DeviceIndependentBitmap" ) return (int)Atom.XA_PIXMAP;
			else if (format == "Palette" ) return (int)Atom.XA_COLORMAP;	// Useless
			//else if (format == "PenData" ) return 10;
			//else if (format == "RiffAudio" ) return 11;
			//else if (format == "WaveAudio" ) return 12;
			else if (format == "UnicodeText" ) return UTF8_STRING.ToInt32();
			//else if (format == "EnhancedMetafile" ) return 14;
			//else if (format == "FileDrop" ) return 15;
			//else if (format == "Locale" ) return 16;
			else if (format == "Rich Text Format") return RICHTEXTFORMAT.ToInt32 ();

			return XInternAtom(DisplayHandle, format, false).ToInt32();
		}

		internal override IntPtr ClipboardOpen(bool primary_selection)
		{
			if (!primary_selection)
				ClipMagic = CLIPBOARD;
			else
				ClipMagic = PRIMARY;
			return ClipMagic;
		}

		internal override object ClipboardRetrieve(IntPtr handle, int type, XplatUI.ClipboardToObject converter)
		{
			XConvertSelection(DisplayHandle, handle, (IntPtr)type, (IntPtr)type, FosterParent, IntPtr.Zero);

			Clipboard.Retrieving = true;
			while (Clipboard.Retrieving) {
				UpdateMessageQueue(null, false);
			}

			return Clipboard.Item;
		}

		internal override void ClipboardStore (IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter, bool copy)
		{
			Clipboard.Converter = converter;

			if (obj != null) {
				Clipboard.AddSource (type, obj);
				XSetSelectionOwner (DisplayHandle, CLIPBOARD, FosterParent, IntPtr.Zero);

				if (copy) {
					try {
						var clipboardAtom = gdk_atom_intern ("CLIPBOARD", true);
						var clipboard = gtk_clipboard_get (clipboardAtom);
						if (clipboard != IntPtr.Zero) {
							// for now we only store text
							var text = Clipboard.GetRtfText ();
							if (string.IsNullOrEmpty (text))
								text = Clipboard.GetPlainText ();
							if (!string.IsNullOrEmpty (text)) {
								gtk_clipboard_set_text (clipboard, text, text.Length);
								gtk_clipboard_store (clipboard);
							}
						}
					} catch {
						// ignore any errors - most likely because gtk isn't installed?
					}
				}
			} else {
				// Clearing the selection
				Clipboard.ClearSources ();
				XSetSelectionOwner (DisplayHandle, CLIPBOARD, IntPtr.Zero, IntPtr.Zero);
			}
		}

		internal override void CreateCaret (IntPtr handle, int width, int height)
		{
			XGCValues	gc_values;
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (Caret.Hwnd != IntPtr.Zero) {
				DestroyCaret(Caret.Hwnd);
			}

			Caret.Hwnd = handle;
			Caret.Window = hwnd.client_window;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = false;
			Caret.On = false;

			gc_values = new XGCValues();
			gc_values.line_width = width;

			Caret.gc = XCreateGC(DisplayHandle, Caret.Window, new IntPtr ((int)GCFunction.GCLineWidth), ref gc_values);
			if (Caret.gc == IntPtr.Zero) {
				Caret.Hwnd = IntPtr.Zero;
				return;
			}

			XSetFunction(DisplayHandle, Caret.gc, GXFunction.GXinvert);
		}

		internal override IntPtr CreateWindow (CreateParams cp)
		{
			XSetWindowAttributes	Attributes;
			Hwnd			hwnd;
			Hwnd			parent_hwnd = null;
			int			X;
			int			Y;
			int			Width;
			int			Height;
			IntPtr			ParentHandle;
			IntPtr			WholeWindow;
			IntPtr			ClientWindow;
			SetWindowValuemask	ValueMask;
			int[]			atoms;

			hwnd = new Hwnd();

			Attributes = new XSetWindowAttributes();
			X = cp.X;
			Y = cp.Y;
			Width = cp.Width;
			Height = cp.Height;

			if (Width<1) Width=1;
			if (Height<1) Height=1;

			if (cp.Parent != IntPtr.Zero) {
				parent_hwnd = Hwnd.ObjectFromHandle(cp.Parent);
				ParentHandle = parent_hwnd.client_window;
			} else {
				if (StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
					// We need to use our foster parent window until this poor child gets it's parent assigned
					ParentHandle=FosterParent;
				} else {
					ParentHandle=RootWindow;
				}
			}

			// Set the default location location for forms.
			if (cp.control is Form && cp.X == int.MinValue && cp.Y == int.MinValue) {
				Point next = Hwnd.GetNextStackedFormLocation (cp);
				X = next.X;
				Y = next.Y;
			}
			ValueMask = SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity;

			Attributes.bit_gravity = Gravity.NorthWestGravity;
			Attributes.win_gravity = Gravity.NorthWestGravity;

			// Save what's under the toolwindow
			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
				Attributes.save_under = true;
				ValueMask |= SetWindowValuemask.SaveUnder;
			}


			// If we're a popup without caption we override the WM
			if (StyleSet (cp.Style, WindowStyles.WS_POPUP) && !StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
				Attributes.override_redirect = true;
				ValueMask |= SetWindowValuemask.OverrideRedirect;
			}

			hwnd.x = X;
			hwnd.y = Y;
			hwnd.width = Width;
			hwnd.height = Height;
			hwnd.parent = Hwnd.ObjectFromHandle(cp.Parent);
			hwnd.initial_style = cp.WindowStyle;
			hwnd.initial_ex_style = cp.WindowExStyle;

			if (StyleSet (cp.Style, WindowStyles.WS_DISABLED)) {
				hwnd.enabled = false;
			}

			ClientWindow = IntPtr.Zero;

			Size XWindowSize = TranslateWindowSizeToXWindowSize (cp);
			Rectangle XClientRect = TranslateClientRectangleToXClientRectangle (hwnd, cp.control);
				
			lock (XlibLock) {
				WholeWindow = XCreateWindow(DisplayHandle, ParentHandle, X, Y, XWindowSize.Width, XWindowSize.Height, 0, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, IntPtr.Zero, new UIntPtr ((uint)ValueMask), ref Attributes);
				if (WholeWindow != IntPtr.Zero) {
					ValueMask &= ~(SetWindowValuemask.OverrideRedirect | SetWindowValuemask.SaveUnder);

					if (CustomVisual != IntPtr.Zero && CustomColormap != IntPtr.Zero) {
						ValueMask = SetWindowValuemask.ColorMap;
						Attributes.colormap = CustomColormap;
					}
					ClientWindow = XCreateWindow(DisplayHandle, WholeWindow, XClientRect.X, XClientRect.Y, XClientRect.Width, XClientRect.Height, 0, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, CustomVisual, new UIntPtr ((uint)ValueMask), ref Attributes);
				}
			}

			if ((WholeWindow == IntPtr.Zero) || (ClientWindow == IntPtr.Zero)) {
				throw new Exception("Could not create X11 windows");
			}

			hwnd.Queue = ThreadQueue(Thread.CurrentThread);
			hwnd.WholeWindow = WholeWindow;
			hwnd.ClientWindow = ClientWindow;

			DriverDebug("Created window {0:X} / {1:X} parent {2:X}, Style {3}, ExStyle {4}", ClientWindow.ToInt32(), WholeWindow.ToInt32(), hwnd.parent != null ? hwnd.parent.Handle.ToInt32() : 0, (WindowStyles)cp.Style, (WindowExStyles)cp.ExStyle);
			
			if (!StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
				if ((X != unchecked((int)0x80000000)) && (Y != unchecked((int)0x80000000))) {
					XSizeHints	hints;

					hints = new XSizeHints();
					hints.x = X;
					hints.y = Y;
					hints.flags = (IntPtr)(XSizeHintsFlags.USPosition | XSizeHintsFlags.PPosition);
					XSetWMNormalHints(DisplayHandle, WholeWindow, ref hints);
				}
			}

			lock (XlibLock) {
				XSelectInput(DisplayHandle, hwnd.whole_window, new IntPtr ((int)(SelectInputMask | EventMask.StructureNotifyMask | EventMask.PropertyChangeMask | Keyboard.KeyEventMask)));
				if (hwnd.whole_window != hwnd.client_window)
					XSelectInput(DisplayHandle, hwnd.client_window, new IntPtr ((int)(SelectInputMask | EventMask.StructureNotifyMask | Keyboard.KeyEventMask)));
			}

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOPMOST))
				SetTopmost(hwnd.whole_window, true);

			SetWMStyles(hwnd, cp);
			
			// set the group leader
			XWMHints wm_hints = new XWMHints ();
			
			wm_hints.flags = (IntPtr)(XWMHintsFlags.InputHint | XWMHintsFlags.StateHint | XWMHintsFlags.WindowGroupHint);
			wm_hints.input = !StyleSet (cp.Style, WindowStyles.WS_DISABLED);
			wm_hints.initial_state = StyleSet (cp.Style, WindowStyles.WS_MINIMIZE) ? XInitialState.IconicState : XInitialState.NormalState;
			
			if (ParentHandle != RootWindow) {
				wm_hints.window_group = hwnd.whole_window;
			} else {
				wm_hints.window_group = ParentHandle;
			}
			
			lock (XlibLock) {
				XSetWMHints(DisplayHandle, hwnd.whole_window, ref wm_hints );
			}

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE)) {
				SetWindowState(hwnd.Handle, FormWindowState.Minimized);
			} else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE)) {
				SetWindowState(hwnd.Handle, FormWindowState.Maximized);
			}

			// for now make all windows dnd enabled
			Dnd.SetAllowDrop (hwnd, true);

			// Set caption/window title
			Text(hwnd.Handle, cp.Caption);

			SendMessage (hwnd.Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			SendParentNotify (hwnd.Handle, Msg.WM_CREATE, int.MaxValue, int.MaxValue);

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
				hwnd.visible = true;
				MapWindow(hwnd, WindowType.Both);
				if (!(Control.FromHandle(hwnd.Handle) is Form))
					SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
			}

			return hwnd.zombie ? IntPtr.Zero : hwnd.Handle;
		}

		internal override IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height)
		{
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName=XplatUI.GetDefaultClassName (GetType ());
			create_params.ClassStyle = 0;
			create_params.ExStyle=0;
			create_params.Parent=IntPtr.Zero;
			create_params.Param=0;

			return CreateWindow(create_params);
		}

		internal override IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot)
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

			if (XQueryBestCursor(DisplayHandle, RootWindow, bitmap.Width, bitmap.Height, out width, out height) == 0) {
				return IntPtr.Zero;
			}

			// Win32 only allows creation cursors of a certain size
			if ((bitmap.Width != width) || (bitmap.Width != height)) {
				cursor_bitmap = new Bitmap(bitmap, new Size(width, height));
				cursor_mask = new Bitmap(mask, new Size(width, height));
			} else {
				cursor_bitmap = bitmap;
				cursor_mask = mask;
			}

			width = cursor_bitmap.Width;
			height = cursor_bitmap.Height;

			cursor_bits = new Byte[(width / 8) * height];
			mask_bits = new Byte[(width / 8) * height];

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					c_pixel = cursor_bitmap.GetPixel(x, y);
					m_pixel = cursor_mask.GetPixel(x, y);

					and = c_pixel == cursor_pixel;
					xor = m_pixel == mask_pixel;

					if (!and && !xor) {
						// Black
						// cursor_bits[y * width / 8 + x / 8] &= (byte)~((1 << (x % 8)));	// The bit already is 0
						mask_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
					} else if (and && !xor) {
						// White
						cursor_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
						mask_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
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

			cursor_pixmap = XCreatePixmapFromBitmapData(DisplayHandle, RootWindow, cursor_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			mask_pixmap = XCreatePixmapFromBitmapData(DisplayHandle, RootWindow, mask_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			fg = new XColor();
			bg = new XColor();

			fg.pixel = XWhitePixel(DisplayHandle, ScreenNo);
			fg.red = (ushort)65535;
			fg.green = (ushort)65535;
			fg.blue = (ushort)65535;

			bg.pixel = XBlackPixel(DisplayHandle, ScreenNo);

			cursor = XCreatePixmapCursor(DisplayHandle, cursor_pixmap, mask_pixmap, ref fg, ref bg, xHotSpot, yHotSpot);

			XFreePixmap(DisplayHandle, cursor_pixmap);
			XFreePixmap(DisplayHandle, mask_pixmap);

			return cursor;
		}

		internal override Bitmap DefineStdCursorBitmap (StdCursor id)
		{
			CursorFontShape	shape;
			string name;
			IntPtr theme;
			int size;
			Bitmap bmp = null;
			
			try {
				shape = StdCursorToFontShape (id);
				name = shape.ToString ().Replace ("XC_", string.Empty);
				size = XcursorGetDefaultSize (DisplayHandle);
				theme = XcursorGetTheme (DisplayHandle);
				IntPtr images_ptr = XcursorLibraryLoadImages (name, theme, size);
				DriverDebug ("DefineStdCursorBitmap, id={0}, #id={1}, name{2}, size={3}, theme: {4}, images_ptr={5}", id, (int) id, name, size, Marshal.PtrToStringAnsi (theme), images_ptr);

				if (images_ptr == IntPtr.Zero) {
					return null;
				}

				XcursorImages images = (XcursorImages) Marshal.PtrToStructure (images_ptr, typeof (XcursorImages));
				DriverDebug ("DefineStdCursorBitmap, cursor has {0} images", images.nimage);

				if (images.nimage > 0) {			
					// We only care about the first image.
					XcursorImage image = (XcursorImage)Marshal.PtrToStructure (Marshal.ReadIntPtr (images.images), typeof (XcursorImage));
					
					DriverDebug ("DefineStdCursorBitmap, loaded image <size={0}, height={1}, width={2}, xhot={3}, yhot={4}, pixels={5}", image.size, image.height, image.width, image.xhot, image.yhot, image.pixels);
					// A sanity check
					if (image.width <= short.MaxValue && image.height <= short.MaxValue) {
						int [] pixels = new int [image.width * image.height];
						Marshal.Copy (image.pixels, pixels, 0, pixels.Length);
						bmp = new Bitmap (image.width, image.height);
						for (int w = 0; w < image.width; w++) {
							for (int h = 0; h < image.height; h++) {
								bmp.SetPixel (w, h, Color.FromArgb (pixels [h * image.width + w]));
							}
						}
					}
				}
				
				XcursorImagesDestroy (images_ptr);
				
			} catch (DllNotFoundException ex) {
				Console.WriteLine ("Could not load libXcursor: " + ex.Message + " (" + ex.GetType ().Name + ")");
				return null;
			}
			
			return bmp;
		}

		
		internal override IntPtr DefineStdCursor(StdCursor id)
		{
			CursorFontShape	shape;
			IntPtr		cursor;

			shape = StdCursorToFontShape (id);

			lock (XlibLock) {
				cursor = XCreateFontCursor(DisplayHandle, shape);
			}
			return cursor;
		}

		internal static CursorFontShape StdCursorToFontShape (StdCursor id)
		{
			CursorFontShape shape;
			// FIXME - define missing shapes

			switch (id) {
				case StdCursor.AppStarting: {
					shape = CursorFontShape.XC_watch;
					break;
				}

				case StdCursor.Arrow: {
					shape = CursorFontShape.XC_top_left_arrow;
					break;
				}

				case StdCursor.Cross: {
					shape = CursorFontShape.XC_crosshair;
					break;
				}

				case StdCursor.Default: {
					shape = CursorFontShape.XC_top_left_arrow;
					break;
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
					shape = (CursorFontShape) 0;
					break;
				}
			}
			
			return shape;
		}

		internal override IntPtr DefWndProc(ref Message msg)
		{
			switch ((Msg)msg.Msg) {
				
				case Msg.WM_IME_COMPOSITION:
					string s = Keyboard.GetCompositionString ();
					foreach (char c in s)
						SendMessage (msg.HWnd, Msg.WM_IME_CHAR, (IntPtr) c, msg.LParam);
					return IntPtr.Zero;

				case Msg.WM_IME_CHAR:
					// On Windows API it sends two WM_CHAR messages for each byte, but
					// I wonder if it is worthy to emulate it (also no idea how to 
					// reconstruct those bytes into chars).
					SendMessage (msg.HWnd, Msg.WM_CHAR, msg.WParam, msg.LParam);
					return IntPtr.Zero;

				case Msg.WM_PAINT: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
					if (hwnd != null) {
						hwnd.expose_pending = false;
					}

					return IntPtr.Zero;
				}

				case Msg.WM_NCPAINT: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
					if (hwnd != null) {
						hwnd.nc_expose_pending = false;
					}

					return IntPtr.Zero;
				}

				case Msg.WM_NCCALCSIZE: {
					Hwnd hwnd;

					if (msg.WParam == (IntPtr)1) {
						hwnd = Hwnd.GetObjectFromWindow (msg.HWnd);
						
						XplatUIWin32.NCCALCSIZE_PARAMS ncp;
						ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure (msg.LParam, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

						// Add all the stuff X is supposed to draw.
						Control ctrl = Control.FromHandle (hwnd.Handle);
						
						if (ctrl != null) {
							Hwnd.Borders rect = Hwnd.GetBorders (ctrl.GetCreateParams (), null);
							
							ncp.rgrc1.top += rect.top;
							ncp.rgrc1.bottom -= rect.bottom;
							ncp.rgrc1.left += rect.left;
							ncp.rgrc1.right -= rect.right;
							
							Marshal.StructureToPtr (ncp, msg.LParam, true);
						}
					}

					return IntPtr.Zero;
				}

				case Msg.WM_CONTEXTMENU: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);

					if ((hwnd != null) && (hwnd.parent != null)) {
						SendMessage(hwnd.parent.client_window, Msg.WM_CONTEXTMENU, msg.WParam, msg.LParam);
					}
					return IntPtr.Zero;
				}

				case Msg.WM_MOUSEWHEEL: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);

					if ((hwnd != null) && (hwnd.parent != null)) {
						SendMessage(hwnd.parent.client_window, Msg.WM_MOUSEWHEEL, msg.WParam, msg.LParam);
						if (msg.Result == IntPtr.Zero) {
							return IntPtr.Zero;
						}
					}
					return IntPtr.Zero;
				}

				case Msg.WM_SETCURSOR: {
					Hwnd	hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
					if (hwnd == null)
						break; // not sure how this happens, but it does

					// Pass to parent window first
					while ((hwnd.parent != null) && (msg.Result == IntPtr.Zero)) {
						hwnd = hwnd.parent;
						msg.Result = NativeWindow.WndProc(hwnd.Handle, Msg.WM_SETCURSOR, msg.HWnd, msg.LParam);
					}

					if (msg.Result == IntPtr.Zero) {
						IntPtr handle;

						switch((HitTest)(msg.LParam.ToInt32() & 0xffff)) {
							case HitTest.HTBOTTOM:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTBORDER:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTBOTTOMLEFT:	handle = Cursors.SizeNESW.handle; break;
							case HitTest.HTBOTTOMRIGHT:	handle = Cursors.SizeNWSE.handle; break;
							case HitTest.HTERROR:		if ((msg.LParam.ToInt32() >> 16) == (int)Msg.WM_LBUTTONDOWN) {
												AudibleAlert(AlertType.Default);
											}
											handle = Cursors.Default.handle;
											break;

							case HitTest.HTHELP:		handle = Cursors.Help.handle; break;
							case HitTest.HTLEFT:		handle = Cursors.SizeWE.handle; break;
							case HitTest.HTRIGHT:		handle = Cursors.SizeWE.handle; break;
							case HitTest.HTTOP:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTTOPLEFT:		handle = Cursors.SizeNWSE.handle; break;
							case HitTest.HTTOPRIGHT:	handle = Cursors.SizeNESW.handle; break;

							#if SameAsDefault
							case HitTest.HTGROWBOX:
							case HitTest.HTSIZE:
							case HitTest.HTZOOM:
							case HitTest.HTVSCROLL:
							case HitTest.HTSYSMENU:
							case HitTest.HTREDUCE:
							case HitTest.HTNOWHERE:
							case HitTest.HTMAXBUTTON:
							case HitTest.HTMINBUTTON:
							case HitTest.HTMENU:
							case HitTest.HSCROLL:
							case HitTest.HTBOTTOM:
							case HitTest.HTCAPTION:
							case HitTest.HTCLIENT:
							case HitTest.HTCLOSE:
							#endif
							default: handle = Cursors.Default.handle; break;
						}
						SetCursor(msg.HWnd, handle);
					}
					return (IntPtr)1;
				}
			}
			return IntPtr.Zero;
		}

		internal override void DestroyCaret(IntPtr handle)
		{
			if (Caret.Hwnd == handle) {
				if (Caret.Visible) {
					HideCaret ();
					Caret.Timer.Stop();
				}
				if (Caret.gc != IntPtr.Zero) {
					XFreeGC(DisplayHandle, Caret.gc);
					Caret.gc = IntPtr.Zero;
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = false;
				Caret.On = false;
			}
		}

		internal override void DestroyCursor(IntPtr cursor)
		{
			lock (XlibLock) {
				XFreeCursor(DisplayHandle, cursor);
			}
		}

		internal override void DestroyWindow(IntPtr handle)
		{
			Hwnd	hwnd;
			hwnd = Hwnd.ObjectFromHandle(handle);
			
			// The window should never ever be a zombie here, since we should
			// wait until it's completely dead before returning from 
			// "destroying" calls, but just in case....
			if (hwnd == null || hwnd.zombie) {
				DriverDebug ("window {0:X} already destroyed", handle.ToInt32());
				return;
			}

			DriverDebug ("Destroying window {0}", XplatUI.Window(hwnd.client_window));

			SendParentNotify (hwnd.Handle, Msg.WM_DESTROY, int.MaxValue, int.MaxValue);
				
			CleanupCachedWindows (hwnd);

			ArrayList windows = new ArrayList ();

			AccumulateDestroyedHandles (Control.ControlNativeWindow.ControlFromHandle(hwnd.Handle), windows);


			foreach (Hwnd h in windows) {
				SendMessage (h.Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
				h.zombie = true;				
			}

			lock (XlibLock) {
				if (hwnd.whole_window != IntPtr.Zero) {
					DriverDebug ("XDestroyWindow (whole_window = {0:X})", hwnd.whole_window.ToInt32());
					Keyboard.DestroyICForWindow (hwnd.whole_window);
					XDestroyWindow(DisplayHandle, hwnd.whole_window);
				}
				else if (hwnd.client_window != IntPtr.Zero) {
					DriverDebug ("XDestroyWindow (client_window = {0:X})", hwnd.client_window.ToInt32());
					Keyboard.DestroyICForWindow (hwnd.client_window);
					XDestroyWindow(DisplayHandle, hwnd.client_window);
				}

			}
		}

		internal override IntPtr DispatchMessage(ref MSG msg)
		{
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		IntPtr GetReversibleScreenGC (Color backColor)
		{
			XGCValues	gc_values;
			IntPtr		gc;
			uint pixel;

			XColor xcolor = new XColor();
			xcolor.red = (ushort)(backColor.R * 257);
			xcolor.green = (ushort)(backColor.G * 257);
			xcolor.blue = (ushort)(backColor.B * 257);
			XAllocColor(DisplayHandle, DefaultColormap, ref xcolor);
			pixel = (uint)xcolor.pixel.ToInt32();


			gc_values = new XGCValues();

			gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			gc_values.foreground = (IntPtr)pixel;

			gc = XCreateGC(DisplayHandle, RootWindow, new IntPtr ((int) (GCFunction.GCSubwindowMode | GCFunction.GCForeground)), ref gc_values);
			XSetForeground(DisplayHandle, gc, (UIntPtr)pixel);
			XSetFunction(DisplayHandle,   gc, GXFunction.GXxor);

			return gc;
		}

		IntPtr GetReversibleControlGC (Control control, int line_width)
		{
			XGCValues	gc_values;
			IntPtr		gc;

			gc_values = new XGCValues();

			gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			gc_values.line_width = line_width;
			gc_values.foreground = XBlackPixel(DisplayHandle, ScreenNo);

			// This logic will give us true rubber bands: (libsx, SANE_XOR)
			//mask = foreground ^ background; 
			//XSetForeground(DisplayHandle, gc, 0xffffffff);
			//XSetBackground(DisplayHandle, gc, background);
			//XSetFunction(DisplayHandle,   gc, GXxor);
			//XSetPlaneMask(DisplayHandle,  gc, mask);


			gc = XCreateGC(DisplayHandle, control.Handle, new IntPtr ((int) (GCFunction.GCSubwindowMode | GCFunction.GCLineWidth | GCFunction.GCForeground)), ref gc_values);
			uint foreground;
			uint background;

			XColor xcolor = new XColor();

			xcolor.red = (ushort)(control.ForeColor.R * 257);
			xcolor.green = (ushort)(control.ForeColor.G * 257);
			xcolor.blue = (ushort)(control.ForeColor.B * 257);
			XAllocColor(DisplayHandle, DefaultColormap, ref xcolor);
			foreground = (uint)xcolor.pixel.ToInt32();

			xcolor.red = (ushort)(control.BackColor.R * 257);
			xcolor.green = (ushort)(control.BackColor.G * 257);
			xcolor.blue = (ushort)(control.BackColor.B * 257);
			XAllocColor(DisplayHandle, DefaultColormap, ref xcolor);
			background = (uint)xcolor.pixel.ToInt32();

			uint mask = foreground ^ background; 

			XSetForeground(DisplayHandle, gc, (UIntPtr)0xffffffff);
			XSetBackground(DisplayHandle, gc, (UIntPtr)background);
			XSetFunction(DisplayHandle,   gc, GXFunction.GXxor);
			XSetPlaneMask(DisplayHandle,  gc, (IntPtr)mask);

			return gc;
		}

		internal override void DrawReversibleLine(Point start, Point end, Color backColor)
		{
			if (backColor.GetBrightness() < 0.5)
				backColor = Color.FromArgb(255 - backColor.R, 255 - backColor.G, 255 - backColor.B);

			IntPtr gc = GetReversibleScreenGC (backColor);

			XDrawLine (DisplayHandle, RootWindow, gc, start.X, start.Y, end.X, end.Y);

			XFreeGC(DisplayHandle, gc);
		}

		internal override void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style)
		{
			if (backColor.GetBrightness() < 0.5)
				backColor = Color.FromArgb(255 - backColor.R, 255 - backColor.G, 255 - backColor.B);

			IntPtr gc = GetReversibleScreenGC (backColor);

			if (rectangle.Width < 0) {
				rectangle.X += rectangle.Width;
				rectangle.Width = -rectangle.Width;
			}
			if (rectangle.Height < 0) {
				rectangle.Y += rectangle.Height;
				rectangle.Height = -rectangle.Height;
			}

			int line_width = 1;
			GCLineStyle line_style = GCLineStyle.LineSolid;
			GCCapStyle cap_style = GCCapStyle.CapButt;
			GCJoinStyle join_style = GCJoinStyle.JoinMiter;

			switch (style) {
			case FrameStyle.Dashed:
				line_style = GCLineStyle.LineOnOffDash;
				break;
			case FrameStyle.Thick:
				line_width = 2;
				break;
			}

			XSetLineAttributes (DisplayHandle, gc, line_width, line_style, cap_style, join_style);

			XDrawRectangle(DisplayHandle, RootWindow, gc, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

			XFreeGC(DisplayHandle, gc);
		}

		internal override void FillReversibleRectangle (Rectangle rectangle, Color backColor) 
		{
			if (backColor.GetBrightness() < 0.5)
				backColor = Color.FromArgb(255 - backColor.R, 255 - backColor.G, 255 - backColor.B);

			IntPtr gc = GetReversibleScreenGC (backColor);

			if (rectangle.Width < 0) {
				rectangle.X += rectangle.Width;
				rectangle.Width = -rectangle.Width;
			}
			if (rectangle.Height < 0) {
				rectangle.Y += rectangle.Height;
				rectangle.Height = -rectangle.Height;
			}
			XFillRectangle(DisplayHandle, RootWindow, gc, rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);

			XFreeGC(DisplayHandle, gc);
		}

		internal override void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width)
		{
			IntPtr		gc;
			Control control = Control.FromHandle(handle);

			gc = GetReversibleControlGC (control, line_width);

			if ((rect.Width > 0) && (rect.Height > 0)) {
				XDrawRectangle(DisplayHandle, control.Handle, gc, rect.Left, rect.Top, rect.Width, rect.Height);
			} else {
				if (rect.Width > 0) {
					XDrawLine(DisplayHandle, control.Handle, gc, rect.X, rect.Y, rect.Right, rect.Y);
				} else {
					XDrawLine(DisplayHandle, control.Handle, gc, rect.X, rect.Y, rect.X, rect.Bottom);
				}
			}
			XFreeGC(DisplayHandle, gc);
		}

		internal override void DoEvents()
		{
			DebugHelper.Enter ();

			MSG	msg = new MSG ();
			XEventQueue queue;

			if (OverrideCursorHandle != IntPtr.Zero) {
				OverrideCursorHandle = IntPtr.Zero;
			}

			queue = ThreadQueue(Thread.CurrentThread);

			queue.DispatchIdle = false;
			in_doevents = true;

			while (PeekMessage(queue, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
				Message m = Message.Create (msg.hwnd, (int)msg.message, msg.wParam, msg.lParam);

				if (Application.FilterMessage (ref m))
					continue;

				TranslateMessage (ref msg);
				DispatchMessage (ref msg);

				string key = msg.hwnd + ":" + msg.message;				
				if (messageHold[key] != null) {
					messageHold[key] = ((int)messageHold[key]) - 1;
					DebugHelper.WriteLine  ("Got " + msg + " for " + key);
				}
			}

			in_doevents = false;
			queue.DispatchIdle = true;

			DebugHelper.Leave ();
		}

		internal override void EnableWindow(IntPtr handle, bool Enable)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				hwnd.Enabled = Enable;
			}
		}

		internal override void EndLoop(Thread thread)
		{
			// This is where we one day will shut down the loop for the thread
		}

		internal override IntPtr GetActive()
		{
			IntPtr	actual_atom;
			int	actual_format;
			IntPtr	nitems;
			IntPtr	bytes_after;
			IntPtr	prop = IntPtr.Zero;
			IntPtr	active = IntPtr.Zero;

			XGetWindowProperty(DisplayHandle, RootWindow, _NET_ACTIVE_WINDOW, IntPtr.Zero, new IntPtr (1), false, (IntPtr)Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
				active = (IntPtr)Marshal.ReadInt32(prop);
				XFree(prop);
			} else {
				// The window manager does not support _NET_ACTIVE_WINDOW.  Fall back to XGetInputFocus.
				IntPtr	revert_to = IntPtr.Zero;
				XGetInputFocus(DisplayHandle, out active, out revert_to);
			}

			if (active != IntPtr.Zero) {
				Hwnd	hwnd;

				hwnd = Hwnd.GetObjectFromWindow(active);
				if (hwnd != null) {
					active = hwnd.Handle;
				} else {
					active = IntPtr.Zero;
				}
			}
			return active;
		}

		internal override Region GetClipRegion(IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				return hwnd.UserClip;
			}

			return null;
		}

		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y)
		{
			width = 20;
			height = 20;
			hotspot_x = 0;
			hotspot_y = 0;
		}

		internal override void GetDisplaySize(out Size size)
		{
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (XlibLock) {
				// FIXME - use _NET_WM messages instead?
				XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
			}

			size = new Size(attributes.width, attributes.height);
		}

		internal override SizeF GetAutoScaleSize(Font font)
		{
			Graphics	g;
			float		width;
			string		magic_string = "The quick brown fox jumped over the lazy dog.";
			double		magic_number = 44.549996948242189;

			g = Graphics.FromHwnd(FosterParent);

			width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			return new SizeF(width, font.Height);
		}

		internal override IntPtr GetParent(IntPtr handle, bool with_owner)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				if (hwnd.parent != null) {
					return hwnd.parent.Handle;
				}
				if (hwnd.owner != null && with_owner) {
					return hwnd.owner.Handle;
				}
			}
			return IntPtr.Zero;
		}
		
		// This is a nop on win32 and x11
		internal override IntPtr GetPreviousWindow(IntPtr handle)
		{
			return handle;
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y)
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
				use_handle = Hwnd.ObjectFromHandle(handle).client_window;
			} else {
				use_handle = RootWindow;
			}

			lock (XlibLock) {
				QueryPointer (DisplayHandle, use_handle, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);
			}

			if (handle != IntPtr.Zero) {
				x = win_x;
				y = win_y;
			} else {
				x = root_x;
				y = root_y;
			}
		}

		internal override IntPtr GetFocus()
		{
			return FocusWindow;
		}


		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent)
		{
			FontFamily ff = font.FontFamily;
			ascent = ff.GetCellAscent (font.Style);
			descent = ff.GetCellDescent (font.Style);
			return true;
		}

		internal override Point GetMenuOrigin(IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				return hwnd.MenuOrigin;
			}
			return Point.Empty;
		}

		[MonoTODO("Implement filtering")]
		internal override bool GetMessage(Object queue_id, ref MSG msg, IntPtr handle, int wFilterMin, int wFilterMax)
		{
			XEvent	xevent;
			bool	client;
			Hwnd	hwnd;

			ProcessNextMessage:

			if (((XEventQueue)queue_id).Count > 0) {
				xevent = (XEvent) ((XEventQueue)queue_id).Dequeue ();
			} else {
				UpdateMessageQueue ((XEventQueue)queue_id);

				if (((XEventQueue)queue_id).Count > 0) {
					xevent = (XEvent) ((XEventQueue)queue_id).Dequeue ();
				} else if (((XEventQueue)queue_id).Paint.Count > 0) {
					xevent = ((XEventQueue)queue_id).Paint.Dequeue();
				} else {
					msg.hwnd= IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return true;
				}
			}

			hwnd = Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);

#if DriverDebugDestroy			
			if (hwnd != null)
				if (hwnd.zombie)
					Console.WriteLine ( "GetMessage zombie, got Event: " + xevent.ToString () + " for 0x{0:x}", hwnd.Handle.ToInt32());
				else	
					Console.WriteLine ( "GetMessage, got Event: " + xevent.ToString () + " for 0x{0:x}", hwnd.Handle.ToInt32());
#endif
			// Handle messages for windows that are already or are about to be destroyed.

			// we need a special block for this because unless we remove the hwnd from the paint
			// queue it will always stay there (since we don't handle the expose), and we'll
			// effectively loop infinitely trying to repaint a non-existant window.
			if (hwnd != null && hwnd.zombie && xevent.type == XEventName.Expose) {
				hwnd.expose_pending = hwnd.nc_expose_pending = false;
				hwnd.Queue.Paint.Remove (hwnd);
				goto ProcessNextMessage;
			}

			// We need to make sure we only allow DestroyNotify events through for zombie
			// hwnds, since much of the event handling code makes requests using the hwnd's
			// client_window, and that'll result in BadWindow errors if there's some lag
			// between the XDestroyWindow call and the DestroyNotify event.
			if (hwnd == null || hwnd.zombie && xevent.AnyEvent.type != XEventName.ClientMessage) {
				DriverDebug("GetMessage(): Got message {0} for non-existent or already destroyed window {1:X}", xevent.type, xevent.AnyEvent.window.ToInt32());
				goto ProcessNextMessage;
			}


			// If we get here, that means the window is no more but there are Client Messages
			// to be processed, probably a Posted message (for instance, an WM_ACTIVATE message) 
			// We don't want anything else to run but the ClientMessage block, so reset all hwnd
			// properties that might cause other processing to occur.
			if (hwnd.zombie) {
				hwnd.resizing_or_moving = false;
			}

			if (hwnd.client_window == xevent.AnyEvent.window) {
				client = true;
				//Console.WriteLine("Client message {1}, sending to window {0:X}", msg.hwnd.ToInt32(), xevent.type);
			} else {
				client = false;
				//Console.WriteLine("Non-Client message, sending to window {0:X}", msg.hwnd.ToInt32());
			}

			msg.hwnd = hwnd.Handle;

			// Windows sends WM_ENTERSIZEMOVE when a form resize/move operation starts and WM_EXITSIZEMOVE 
			// when it is done. The problem in X11 is that there is no concept of start-end of a moving/sizing.
			// Configure events ("this window has resized/moved") are sent for each step of the resize. We send a
			// WM_ENTERSIZEMOVE when we get the first Configure event. The problem is the WM_EXITSIZEMOVE.
			// 
			//  - There is no way for us to know which is the last Configure event. We can't traverse the events 
			//    queue, because the next configure event might not be pending yet.
			//  - We can't get ButtonPress/Release events for the window decorations, because they are not part 
			//    of the window(s) we manage.
			//  - We can't rely on the mouse state to change to "up" before the last Configure event. It doesn't.
			// 
			// We are almost 100% guaranteed to get another event (e.g Expose or other), but we can't know for sure 
			// which, so we have here to check if the mouse buttons state is "up" and send the WM_EXITSIZEMOVE
			//
			if (hwnd.resizing_or_moving) {
				int root_x, root_y, win_x, win_y, keys_buttons;
				IntPtr  root, child;
				XQueryPointer (DisplayHandle, hwnd.Handle, out root, out child, out root_x, out root_y, 
					       out win_x, out win_y, out keys_buttons);
				if ((keys_buttons & (int)MouseKeyMasks.Button1Mask) == 0 &&
				    (keys_buttons & (int)MouseKeyMasks.Button2Mask) == 0 &&
				    (keys_buttons & (int)MouseKeyMasks.Button3Mask) == 0) {
					hwnd.resizing_or_moving = false;
					SendMessage (hwnd.Handle, Msg.WM_EXITSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
				}
			}

			//
			// If you add a new event to this switch make sure to add it in
			// UpdateMessage also unless it is not coming through the X event system.
			//
			switch(xevent.type) {
				case XEventName.KeyPress: {
					Keyboard.KeyEvent (FocusWindow, xevent, ref msg);

					// F1 key special case - WM_HELP sending
					if (msg.wParam == (IntPtr)VirtualKeys.VK_F1 || msg.wParam == (IntPtr)VirtualKeys.VK_HELP) {
						// Send wM_HELP and then return it as a keypress message in
						// case it needs to be preproccessed.
						HELPINFO helpInfo = new HELPINFO ();
						GetCursorPos (IntPtr.Zero, out helpInfo.MousePos.x, out helpInfo.MousePos.y);
						IntPtr helpInfoPtr = Marshal.AllocHGlobal (Marshal.SizeOf (helpInfo));
						Marshal.StructureToPtr (helpInfo, helpInfoPtr, true);
						NativeWindow.WndProc (FocusWindow, Msg.WM_HELP, IntPtr.Zero, helpInfoPtr);
						Marshal.FreeHGlobal (helpInfoPtr);
					}
					break;
				}

				case XEventName.KeyRelease: {
					Keyboard.KeyEvent (FocusWindow, xevent, ref msg);
					break;
				}

				case XEventName.ButtonPress: {
					switch(xevent.ButtonEvent.button) {
						case 1: {
							MouseState |= MouseButtons.Left;
							if (client) {
								msg.message = Msg.WM_LBUTTONDOWN;
								msg.wParam = GetMousewParam (0);
							} else {
								msg.message = Msg.WM_NCLBUTTONDOWN;
								msg.wParam = (IntPtr) NCHitTest (hwnd, xevent.MotionEvent.x, xevent.MotionEvent.y);
								MenuToScreen (xevent.AnyEvent.window, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							break;
						}

						case 2: {
							MouseState |= MouseButtons.Middle;
							if (client) {
								msg.message = Msg.WM_MBUTTONDOWN;
								msg.wParam = GetMousewParam (0);
							} else {
								msg.message = Msg.WM_NCMBUTTONDOWN;
								msg.wParam = (IntPtr) NCHitTest (hwnd, xevent.MotionEvent.x, xevent.MotionEvent.y);
								MenuToScreen (xevent.AnyEvent.window, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							break;
						}

						case 3: {
							MouseState |= MouseButtons.Right;
							if (client) {
								msg.message = Msg.WM_RBUTTONDOWN;
								msg.wParam = GetMousewParam (0);
							} else {
								msg.message = Msg.WM_NCRBUTTONDOWN;
								msg.wParam = (IntPtr) NCHitTest (hwnd, xevent.MotionEvent.x, xevent.MotionEvent.y);
								MenuToScreen (xevent.AnyEvent.window, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							break;
						}

						case 4: {
							msg.hwnd = FocusWindow;
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(120);
							break;
						}

						case 5: {
							msg.hwnd = FocusWindow;
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(-120);
							break;
						}

					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					mouse_position.X = xevent.ButtonEvent.x;
					mouse_position.Y = xevent.ButtonEvent.y;

					if (!hwnd.Enabled) {
						IntPtr dummy;

						msg.hwnd = hwnd.EnabledHwnd;
						XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.ButtonEvent.x, xevent.ButtonEvent.y, out xevent.ButtonEvent.x, out xevent.ButtonEvent.y, out dummy);
						msg.lParam = (IntPtr)(mouse_position.Y << 16 | mouse_position.X);
					}

					if (Grab.Hwnd != IntPtr.Zero) {
						msg.hwnd = Grab.Hwnd;
					}

					if (ClickPending.Pending && ((((long)xevent.ButtonEvent.time - ClickPending.Time) < DoubleClickInterval) && (msg.wParam == ClickPending.wParam) && (msg.lParam == ClickPending.lParam) && (msg.message == ClickPending.Message))) {
						// Looks like a genuine double click, clicked twice on the same spot with the same keys
						switch(xevent.ButtonEvent.button) {
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
						ClickPending.Pending = false;
					} else {
						ClickPending.Pending = true;
						ClickPending.Hwnd = msg.hwnd;
						ClickPending.Message = msg.message;
						ClickPending.wParam = msg.wParam;
						ClickPending.lParam = msg.lParam;
						ClickPending.Time = (long)xevent.ButtonEvent.time;
					}
					
					if (msg.message == Msg.WM_LBUTTONDOWN || msg.message == Msg.WM_MBUTTONDOWN || msg.message == Msg.WM_RBUTTONDOWN) {
						SendParentNotify(msg.hwnd, msg.message, mouse_position.X, mouse_position.Y);
					}
					
					break;
				}

				case XEventName.ButtonRelease: {
					switch(xevent.ButtonEvent.button) {
						case 1: {
							if (client) {
								msg.message = Msg.WM_LBUTTONUP;
							} else {
								msg.message = Msg.WM_NCLBUTTONUP;
								msg.wParam = (IntPtr) NCHitTest (hwnd, xevent.MotionEvent.x, xevent.MotionEvent.y);
								MenuToScreen (xevent.AnyEvent.window, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							MouseState &= ~MouseButtons.Left;
							msg.wParam = GetMousewParam (0);
							break;
						}

						case 2: {
							if (client) {
								msg.message = Msg.WM_MBUTTONUP;
							} else {
								msg.message = Msg.WM_NCMBUTTONUP;
								msg.wParam = (IntPtr) NCHitTest (hwnd, xevent.MotionEvent.x, xevent.MotionEvent.y);
								MenuToScreen (xevent.AnyEvent.window, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							MouseState &= ~MouseButtons.Middle;
							msg.wParam = GetMousewParam (0);
							break;
						}

						case 3: {
							if (client) {
								msg.message = Msg.WM_RBUTTONUP;
							} else {
								msg.message = Msg.WM_NCRBUTTONUP;
								msg.wParam = (IntPtr) NCHitTest (hwnd, xevent.MotionEvent.x, xevent.MotionEvent.y);
								MenuToScreen (xevent.AnyEvent.window, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							MouseState &= ~MouseButtons.Right;
							msg.wParam = GetMousewParam (0);
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
						XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.ButtonEvent.x, xevent.ButtonEvent.y, out xevent.ButtonEvent.x, out xevent.ButtonEvent.y, out dummy);
						msg.lParam = (IntPtr)(mouse_position.Y << 16 | mouse_position.X);
					}

					if (Grab.Hwnd != IntPtr.Zero) {
						msg.hwnd = Grab.Hwnd;
					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					mouse_position.X = xevent.ButtonEvent.x;
					mouse_position.Y = xevent.ButtonEvent.y;

					// Win32 splurts MouseMove events all over the place, regardless of whether the mouse is actually moving or
					// not, especially after mousedown and mouseup. To support apps relying on mousemove events between and after 
					// mouse clicks to repaint or whatever, we generate a mousemove event here. *sigh*
					if (msg.message == Msg.WM_LBUTTONUP || msg.message == Msg.WM_MBUTTONUP || msg.message == Msg.WM_RBUTTONUP) {
						XEvent motionEvent = new XEvent ();
						motionEvent.type = XEventName.MotionNotify;
						motionEvent.MotionEvent.display = DisplayHandle;
						motionEvent.MotionEvent.window = xevent.ButtonEvent.window;
						motionEvent.MotionEvent.x = xevent.ButtonEvent.x;
						motionEvent.MotionEvent.y = xevent.ButtonEvent.y;
						hwnd.Queue.EnqueueLocked (motionEvent);
					}
					break;
				}

				case XEventName.MotionNotify: {
					if (client) {
						DriverDebug("GetMessage(): Window {0:X} MotionNotify x={1} y={2}",
							    client ? hwnd.client_window.ToInt32() : hwnd.whole_window.ToInt32(),
							    xevent.MotionEvent.x, xevent.MotionEvent.y);

						if (Grab.Hwnd != IntPtr.Zero) {
							msg.hwnd = Grab.Hwnd;
						} else {
							if (hwnd.Enabled) {
								NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);
							}
						}

						if (xevent.MotionEvent.is_hint != 0)
						{
							IntPtr root, child;
							int mask;
							XQueryPointer (DisplayHandle, xevent.AnyEvent.window,
											out root, out child,
											out xevent.MotionEvent.x_root, 
											out xevent.MotionEvent.y_root,
											out xevent.MotionEvent.x,      
											out xevent.MotionEvent.y, out mask);
						}

						msg.message = Msg.WM_MOUSEMOVE;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x & 0xFFFF);

						if (!hwnd.Enabled) {
							IntPtr dummy;

							msg.hwnd = hwnd.EnabledHwnd;
							XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.MotionEvent.x, xevent.MotionEvent.y, out xevent.MotionEvent.x, out xevent.MotionEvent.y, out dummy);
							msg.lParam = (IntPtr)(mouse_position.Y << 16 | mouse_position.X);
						}

						mouse_position.X = xevent.MotionEvent.x;
						mouse_position.Y = xevent.MotionEvent.y;

						if ((HoverState.Timer.Enabled) &&
						    (((mouse_position.X + HoverState.Size.Width) < HoverState.X) ||
						    ((mouse_position.X - HoverState.Size.Width) > HoverState.X) ||
						    ((mouse_position.Y + HoverState.Size.Height) < HoverState.Y) ||
						    ((mouse_position.Y - HoverState.Size.Height) > HoverState.Y))) {
							HoverState.Timer.Stop();
							HoverState.Timer.Start();
							HoverState.X = mouse_position.X;
							HoverState.Y = mouse_position.Y;
						}

						break;
					} else {
						HitTest	ht;
						IntPtr dummy;

						DriverDebug("GetMessage(): non-client area {0:X} MotionNotify x={1} y={2}",
							    client ? hwnd.client_window.ToInt32() : hwnd.whole_window.ToInt32(),
							    xevent.MotionEvent.x, xevent.MotionEvent.y);
						msg.message = Msg.WM_NCMOUSEMOVE;

						if (!hwnd.Enabled) {
							msg.hwnd = hwnd.EnabledHwnd;
							XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.MotionEvent.x, xevent.MotionEvent.y, out xevent.MotionEvent.x, out xevent.MotionEvent.y, out dummy);
							msg.lParam = (IntPtr)(mouse_position.Y << 16 | mouse_position.X);
						}

						ht = NCHitTest (hwnd, xevent.MotionEvent.x, xevent.MotionEvent.y);
						NativeWindow.WndProc(hwnd.client_window, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)ht);

						mouse_position.X = xevent.MotionEvent.x;
						mouse_position.Y = xevent.MotionEvent.y;
					}

					break;
				}

				case XEventName.EnterNotify: {
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					if (xevent.CrossingEvent.mode == NotifyMode.NotifyGrab || xevent.AnyEvent.window != hwnd.client_window) {
						goto ProcessNextMessage;
					}
					if (xevent.CrossingEvent.mode == NotifyMode.NotifyUngrab) { // Pseudo motion caused by grabbing
						if (LastPointerWindow == xevent.AnyEvent.window)
							goto ProcessNextMessage;

						if (LastPointerWindow != IntPtr.Zero) {
							Point enter_loc = new Point (xevent.ButtonEvent.x, xevent.ButtonEvent.y);

							// We need this due to EnterNotify being fired on all the parent controls
							// of the Control being grabbed, and obviously in that scenario we are not
							// actuallty entering them
							Control ctrl = Control.FromHandle (hwnd.client_window);
							foreach (Control child_control in ctrl.Controls.GetAllControls ())
								if (child_control.Bounds.Contains (enter_loc))
									goto ProcessNextMessage;

							// A MouseLeave/LeaveNotify event is sent to the previous window
							// until the mouse is ungrabbed, not when actually leaving its bounds
							int x = xevent.CrossingEvent.x_root;
							int y = xevent.CrossingEvent.y_root;
							ScreenToClient (LastPointerWindow, ref x, ref y);

							XEvent leaveEvent = new XEvent ();
							leaveEvent.type = XEventName.LeaveNotify;
							leaveEvent.CrossingEvent.display = DisplayHandle;
							leaveEvent.CrossingEvent.window = LastPointerWindow;
							leaveEvent.CrossingEvent.x = x;
							leaveEvent.CrossingEvent.y = y;
							leaveEvent.CrossingEvent.mode = NotifyMode.NotifyNormal;
							Hwnd last_pointer_hwnd = Hwnd.ObjectFromHandle (LastPointerWindow);
							last_pointer_hwnd.Queue.EnqueueLocked (leaveEvent);
						}
					}

					LastPointerWindow = xevent.AnyEvent.window;

					msg.message = Msg.WM_MOUSE_ENTER;
					HoverState.X = xevent.CrossingEvent.x;
					HoverState.Y = xevent.CrossingEvent.y;
					HoverState.Timer.Enabled = true;
					HoverState.Window = xevent.CrossingEvent.window;

					// Win32 sends a WM_MOUSEMOVE after mouse enter
					XEvent motionEvent = new XEvent ();
					motionEvent.type = XEventName.MotionNotify;
					motionEvent.MotionEvent.display = DisplayHandle;
					motionEvent.MotionEvent.window = xevent.ButtonEvent.window;
					motionEvent.MotionEvent.x = xevent.ButtonEvent.x;
					motionEvent.MotionEvent.y = xevent.ButtonEvent.y;
					hwnd.Queue.EnqueueLocked (motionEvent);
					break;
				}

				case XEventName.LeaveNotify: {
					if (xevent.CrossingEvent.mode == NotifyMode.NotifyUngrab) {
						WindowUngrabbed (hwnd.Handle);
						goto ProcessNextMessage;
					}
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					if ((xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) || (xevent.CrossingEvent.window != hwnd.client_window)) {
						goto ProcessNextMessage;
					}
					// If a grab is taking place, ignore it - we handle it in EnterNotify
					if (Grab.Hwnd != IntPtr.Zero)
						goto ProcessNextMessage;

					// Reset the cursor explicitly on X11.
					// X11 remembers the last set cursor for the window and in cases where 
					// the control won't get a WM_SETCURSOR	X11 will restore the last 
					// known cursor, which we don't want.
					// 
					SetCursor (hwnd.client_window, IntPtr.Zero);

					msg.message=Msg.WM_MOUSELEAVE;
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
						if ((xevent.ReparentEvent.parent != IntPtr.Zero) && (xevent.ReparentEvent.window == hwnd.whole_window)) {
							hwnd.Reparented = true;

							// The location given by the event is not reliable between different wm's, 
							// so use an alternative way of getting it.
							Point location = GetTopLevelWindowLocation (hwnd);
							hwnd.X = location.X;
							hwnd.Y = location.Y;

							if (hwnd.opacity != 0xffffffff) {
								IntPtr opacity;

								opacity = (IntPtr)(Int32)hwnd.opacity;
								XChangeProperty(DisplayHandle, XGetParent(hwnd.whole_window), _NET_WM_WINDOW_OPACITY, (IntPtr)Atom.XA_CARDINAL, 32, PropertyMode.Replace, ref opacity, 1);
							}
							SendMessage(msg.hwnd, Msg.WM_WINDOWPOSCHANGED, msg.wParam, msg.lParam);
							goto ProcessNextMessage;
						} else {
							hwnd.Reparented = false;
							goto ProcessNextMessage;
						}
					}
					goto ProcessNextMessage;
				}

				case XEventName.ConfigureNotify: {
					if (!client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {	// Ignore events for children (SubstructureNotify) and client areas
						DriverDebug("GetMessage(): Window {0:X} ConfigureNotify x={1} y={2} width={3} height={4}",
							    hwnd.client_window.ToInt32(), xevent.ConfigureEvent.x,
							    xevent.ConfigureEvent.y, xevent.ConfigureEvent.width, xevent.ConfigureEvent.height);

						lock (hwnd.configure_lock) {
							Form form = Control.FromHandle (hwnd.client_window) as Form;
							if (form != null && !hwnd.resizing_or_moving) {
								if (hwnd.x != form.Bounds.X || hwnd.y != form.Bounds.Y) {
									SendMessage (form.Handle, Msg.WM_SYSCOMMAND, (IntPtr)SystemCommands.SC_MOVE, IntPtr.Zero);
									hwnd.resizing_or_moving = true;
								} else if (hwnd.width != form.Bounds.Width || hwnd.height != form.Bounds.Height) {
									SendMessage (form.Handle, Msg.WM_SYSCOMMAND, (IntPtr)SystemCommands.SC_SIZE, IntPtr.Zero);
									hwnd.resizing_or_moving = true;
								}
								if (hwnd.resizing_or_moving)
									SendMessage (form.Handle, Msg.WM_ENTERSIZEMOVE, IntPtr.Zero, IntPtr.Zero);
							}
	
							SendMessage(msg.hwnd, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
							hwnd.configure_pending = false;
	
							// We need to adjust our client window to track the resize of whole_window
							if (hwnd.whole_window != hwnd.client_window)
								PerformNCCalc(hwnd);
						}
					}
					goto ProcessNextMessage;
				}

				case XEventName.FocusIn: {
					// We received focus. We use X11 focus only to know if the app window does or does not have focus
					// We do not track the actual focussed window via it. Instead, this is done via FocusWindow internally
					// Receiving focus means we've gotten activated and therefore we need to let the actual FocusWindow know 
					// about it having focus again
					if (xevent.FocusChangeEvent.detail != NotifyDetail.NotifyNonlinear) {
						goto ProcessNextMessage;
					}

					
					if (FocusWindow == IntPtr.Zero) {
						Control c = Control.FromHandle (hwnd.client_window);

						if (c == null)
							goto ProcessNextMessage;						
						Form form = c.FindForm ();
						if (form == null)
							goto ProcessNextMessage;
					
						if (ActiveWindow != form.Handle) {
							ActiveWindow = form.Handle;
							SendMessage (ActiveWindow, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
						}
						goto ProcessNextMessage;
					}
					Keyboard.FocusIn (FocusWindow);
					SendMessage(FocusWindow, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
					goto ProcessNextMessage;
				}

				case XEventName.FocusOut: {
					// Se the comment for our FocusIn handler
					if (xevent.FocusChangeEvent.detail != NotifyDetail.NotifyNonlinear) {
						goto ProcessNextMessage;
					}

					while (Keyboard.ResetKeyState(FocusWindow, ref msg)) {
						SendMessage(FocusWindow, msg.message, msg.wParam, msg.lParam);
					}

					Keyboard.FocusOut(hwnd.client_window);
					SendMessage(FocusWindow, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
					goto ProcessNextMessage;
				}

				// We are already firing WM_SHOWWINDOW messages in the proper places, but I'm leaving this code
				// in case we break a scenario not taken into account in the tests
				case XEventName.MapNotify: {
					/*if (client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {	// Ignore events for children (SubstructureNotify) and client areas
						hwnd.mapped = true;
						msg.message = Msg.WM_SHOWWINDOW;
						msg.wParam = (IntPtr) 1;
						// XXX we're missing the lParam..
						break;
					}*/
					goto ProcessNextMessage;
				}

				case XEventName.UnmapNotify: {
					/*if (client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {	// Ignore events for children (SubstructureNotify) and client areas
						hwnd.mapped = false;
						msg.message = Msg.WM_SHOWWINDOW;
						msg.wParam = (IntPtr) 0;
						// XXX we're missing the lParam..
						break;
					}*/
					goto ProcessNextMessage;
				}

				case XEventName.Expose: {
					if (!hwnd.Mapped) {
						if (client) {
							hwnd.expose_pending = false;
						} else {
							hwnd.nc_expose_pending = false;
						}
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

								g = Graphics.FromHwnd(hwnd.whole_window);
								if (hwnd.border_static)
									ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Border3DStyle.SunkenOuter);
								else
									ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Border3DStyle.Sunken);
								g.Dispose();
								break;
							}

							case FormBorderStyle.FixedSingle: {
								Graphics g;

								g = Graphics.FromHwnd(hwnd.whole_window);
								ControlPaint.DrawBorder(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Color.Black, ButtonBorderStyle.Solid);
								g.Dispose();
								break;
							}
						}
						DriverDebug("GetMessage(): Window {0:X} Exposed non-client area {1},{2} {3}x{4}",
							    hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y,
							    xevent.ExposeEvent.width, xevent.ExposeEvent.height);

						Rectangle rect = new Rectangle (xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						Region region = new Region (rect);
						IntPtr hrgn = region.GetHrgn (null); // Graphics object isn't needed
						msg.message = Msg.WM_NCPAINT;
						msg.wParam = hrgn == IntPtr.Zero ? (IntPtr)1 : hrgn;
						msg.refobject = region;
						break;
					}
					DriverDebug("GetMessage(): Window {0:X} Exposed area {1},{2} {3}x{4}",
						    hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y,
						    xevent.ExposeEvent.width, xevent.ExposeEvent.height);
					if (Caret.Visible == true) {
						Caret.Paused = true;
						HideCaret();
					}

					if (Caret.Visible == true) {
						ShowCaret();
						Caret.Paused = false;
					}
					msg.message = Msg.WM_PAINT;
					break;
				}

				case XEventName.DestroyNotify: {

					// This is a bit tricky, we don't receive our own DestroyNotify, we only get those for our children
					hwnd = Hwnd.ObjectFromHandle(xevent.DestroyWindowEvent.window);

					// We may get multiple for the same window, act only one the first (when Hwnd still knows about it)
					if ((hwnd != null) && (hwnd.client_window == xevent.DestroyWindowEvent.window)) {
						CleanupCachedWindows (hwnd);

						DriverDebug("Received X11 Destroy Notification for {0}", XplatUI.Window(hwnd.client_window));

						msg.hwnd = hwnd.client_window;
						msg.message=Msg.WM_DESTROY;
						hwnd.Dispose();
					} else {
						goto ProcessNextMessage;
					}

					break;
				}

				case XEventName.ClientMessage: {
					if (Dnd.HandleClientMessage (ref xevent)) {
						goto ProcessNextMessage;
					}

					if (xevent.ClientMessageEvent.message_type == AsyncAtom) {
						XplatUIDriverSupport.ExecuteClientMessage((GCHandle)xevent.ClientMessageEvent.ptr1);
						goto ProcessNextMessage;
					}

					if (xevent.ClientMessageEvent.message_type == HoverState.Atom) {
						msg.message = Msg.WM_MOUSEHOVER;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.ClientMessageEvent.ptr1);
						return true;
					}

					if (xevent.ClientMessageEvent.message_type == (IntPtr)PostAtom) {						
						DebugHelper.Indent ();
						DebugHelper.WriteLine (String.Format ("Posted message:" + (Msg) xevent.ClientMessageEvent.ptr2.ToInt32 () + " for 0x{0:x}", xevent.ClientMessageEvent.ptr1.ToInt32 ()));
						DebugHelper.Unindent ();
						msg.hwnd = xevent.ClientMessageEvent.ptr1;
						msg.message = (Msg) xevent.ClientMessageEvent.ptr2.ToInt32 ();
						msg.wParam = xevent.ClientMessageEvent.ptr3;
						msg.lParam = xevent.ClientMessageEvent.ptr4;
						if (msg.message == (Msg)Msg.WM_QUIT)
							return false;
						else
							return true;
					}

					if  (xevent.ClientMessageEvent.message_type == _XEMBED) {
#if DriverDebugXEmbed
						Console.WriteLine("GOT EMBED MESSAGE {0:X}, detail {1:X}", xevent.ClientMessageEvent.ptr2.ToInt32(), xevent.ClientMessageEvent.ptr3.ToInt32());
#endif

						if (xevent.ClientMessageEvent.ptr2.ToInt32() == (int)XEmbedMessage.EmbeddedNotify) {
							XSizeHints hints = new XSizeHints();
							IntPtr dummy;

							XGetWMNormalHints(DisplayHandle, hwnd.whole_window, ref hints, out dummy);

							hwnd.width = hints.max_width;
							hwnd.height = hints.max_height;
							hwnd.ClientRect = Rectangle.Empty;
							SendMessage(msg.hwnd, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
						}
					}

					if  (xevent.ClientMessageEvent.message_type == WM_PROTOCOLS) {
						if (xevent.ClientMessageEvent.ptr1 == WM_DELETE_WINDOW) {
							SendMessage (msg.hwnd, Msg.WM_SYSCOMMAND, (IntPtr)SystemCommands.SC_CLOSE, IntPtr.Zero);
							msg.message = Msg.WM_CLOSE;
							return true;
						}

						// We should not get this, but I'll leave the code in case we need it in the future
						if (xevent.ClientMessageEvent.ptr1 == WM_TAKE_FOCUS) {
							goto ProcessNextMessage;
						}
					}
					goto ProcessNextMessage;
				}

				default: {
					goto ProcessNextMessage;
				}
			}

			return true;
		}

		HitTest NCHitTest (Hwnd hwnd, int x, int y)
		{
			// The hit test is sent in screen coordinates
			IntPtr dummy;
			int screen_x, screen_y;
			XTranslateCoordinates (DisplayHandle, hwnd.WholeWindow, RootWindow, x, y, out screen_x, out screen_y, out dummy);
			return (HitTest) NativeWindow.WndProc (hwnd.client_window, Msg.WM_NCHITTEST, IntPtr.Zero, 
							       (IntPtr) (screen_y << 16 | screen_x & 0xFFFF));
		}

		// Our very basic implementation of MoveResize - we can extend it later
		// *if* needed
		internal override void BeginMoveResize (IntPtr handle)
		{
			// We *need* to ungrab the pointer in the current display
			XplatUI.UngrabWindow (Grab.Hwnd);

			int x_root, y_root;
			GetCursorPos (IntPtr.Zero, out x_root, out y_root);

			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			SendNetWMMessage (hwnd.whole_window, _NET_WM_MOVERESIZE, (IntPtr) x_root, (IntPtr) y_root,
					(IntPtr) NetWmMoveResize._NET_WM_MOVERESIZE_SIZE_BOTTOMRIGHT, 
					(IntPtr) 1); // left button
		}

		internal override bool GetText(IntPtr handle, out string text)
		{

			lock (XlibLock) {
				IntPtr actual_atom;
				int actual_format;
				IntPtr nitems;
				IntPtr bytes_after;
				IntPtr prop = IntPtr.Zero;

				XGetWindowProperty(DisplayHandle, handle,
						   _NET_WM_NAME, IntPtr.Zero, new IntPtr (1), false,
						   UTF8_STRING, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

				if ((long)nitems > 0 && prop != IntPtr.Zero) {
					text = Marshal.PtrToStringUni (prop, (int)nitems);
					XFree (prop);
					return true;
				}
				else {
					// fallback on the non-_NET property
					IntPtr	textptr;

					textptr = IntPtr.Zero;

					XFetchName(DisplayHandle, Hwnd.ObjectFromHandle(handle).whole_window, ref textptr);
					if (textptr != IntPtr.Zero) {
						text = Marshal.PtrToStringAnsi(textptr);
						XFree(textptr);
						return true;
					} else {
						text = "";
						return false;
					}
				}
			}
		}

		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height)
		{
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				x = hwnd.x;
				y = hwnd.y;
				width = hwnd.width;
				height = hwnd.height;

				PerformNCCalc(hwnd);

				client_width = hwnd.ClientRect.Width;
				client_height = hwnd.ClientRect.Height;

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

		internal override FormWindowState GetWindowState(IntPtr handle)
		{
			Hwnd			hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd.cached_window_state == (FormWindowState)(-1))
				hwnd.cached_window_state = UpdateWindowState (handle);

			return hwnd.cached_window_state;
		}

		FormWindowState UpdateWindowState (IntPtr handle) {
			IntPtr			actual_atom;
			int			actual_format;
			IntPtr			nitems;
			IntPtr			bytes_after;
			IntPtr			prop = IntPtr.Zero;
			IntPtr			atom;
			int			maximized;
			bool			minimized;
			XWindowAttributes	attributes;
			Hwnd			hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			maximized = 0;
			minimized = false;
			XGetWindowProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE, IntPtr.Zero, new IntPtr (256), false, (IntPtr)Atom.XA_ATOM, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
				for (int i = 0; i < (long)nitems; i++) {
					atom = (IntPtr)Marshal.ReadInt32(prop, i * 4);
					if ((atom == _NET_WM_STATE_MAXIMIZED_HORZ) || (atom == _NET_WM_STATE_MAXIMIZED_VERT)) {
						maximized++;
					} else if (atom == _NET_WM_STATE_HIDDEN) {
						minimized = true;
					}
				}
				XFree(prop);
			}

			if (minimized) {
				return FormWindowState.Minimized;
			} else if (maximized == 2) {
				return FormWindowState.Maximized;
			}

			attributes = new XWindowAttributes();
			XGetWindowAttributes(DisplayHandle, hwnd.client_window, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped) {
				return (FormWindowState)(-1);
			}


			return FormWindowState.Normal;
		}

		internal override void GrabInfo(out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea)
		{
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}

		internal override void GrabWindow(IntPtr handle, IntPtr confine_to_handle)
		{
			Hwnd	hwnd;
			IntPtr	confine_to_window;

			confine_to_window = IntPtr.Zero;

			if (confine_to_handle != IntPtr.Zero) {
				XWindowAttributes	attributes = new XWindowAttributes();

				hwnd = Hwnd.ObjectFromHandle(confine_to_handle);

				lock (XlibLock) {
					XGetWindowAttributes(DisplayHandle, hwnd.client_window, ref attributes);
				}
				Grab.Area.X = attributes.x;
				Grab.Area.Y = attributes.y;
				Grab.Area.Width = attributes.width;
				Grab.Area.Height = attributes.height;
				Grab.Confined = true;
				confine_to_window = hwnd.client_window;
			}

			Grab.Hwnd = handle;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XGrabPointer(DisplayHandle, hwnd.client_window, false, 
					EventMask.ButtonPressMask | EventMask.ButtonMotionMask |
					EventMask.ButtonReleaseMask | EventMask.PointerMotionMask | 
					EventMask.PointerMotionHintMask | EventMask.LeaveWindowMask,
					GrabMode.GrabModeAsync, GrabMode.GrabModeAsync, confine_to_window, IntPtr.Zero, IntPtr.Zero);
			}
		}

		internal override void UngrabWindow(IntPtr hwnd)
		{
			lock (XlibLock) {
				XUngrabPointer(DisplayHandle, IntPtr.Zero);
				XFlush(DisplayHandle);
			}
			WindowUngrabbed (hwnd);			
		}
		
		void WindowUngrabbed (IntPtr hwnd) {
			bool was_grabbed = Grab.Hwnd != IntPtr.Zero;
			
			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;
			
			if (was_grabbed) {
				// lparam should be the handle to the window gaining the mouse capture,
				// but X doesn't seem to give us that information.
				// Also only generate WM_CAPTURECHANGED if the window actually was grabbed.
				// X will send a NotifyUngrab, but since it comes late sometimes we're
				// calling WindowUngrabbed directly from UngrabWindow in order to send
				// this WM right away.
				SendMessage (hwnd, Msg.WM_CAPTURECHANGED, IntPtr.Zero, IntPtr.Zero);
			}
		}

		internal override void HandleException(Exception e)
		{
			StackTrace st = new StackTrace(e, true);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (clear) {
				AddExpose (hwnd, true, hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
			} else {
				AddExpose (hwnd, true, rc.X, rc.Y, rc.Width, rc.Height);
			}
		}

		internal override void InvalidateNC (IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			AddExpose (hwnd, hwnd.WholeWindow == hwnd.ClientWindow, 0, 0, hwnd.Width, hwnd.Height);
		}

		internal override bool IsEnabled(IntPtr handle)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			return (hwnd != null && hwnd.Enabled);
		}
		
		internal override bool IsVisible(IntPtr handle)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			return (hwnd != null && hwnd.visible);
		}

		internal override void KillTimer(Timer timer)
		{
			XEventQueue queue = (XEventQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue. In this case, remove the timer
				// from the list of unattached timers (if it was enabled).
				lock (unattached_timer_list) {
					if (unattached_timer_list.Contains (timer))
						unattached_timer_list.Remove (timer);
				}
				return;
			}
			queue.timer_list.Remove (timer);
		}

		internal override void MenuToScreen(IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates(DisplayHandle, hwnd.whole_window, RootWindow, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void OverrideCursor(IntPtr cursor)
		{
			if (Grab.Hwnd != IntPtr.Zero) {
				XChangeActivePointerGrab (DisplayHandle,
						EventMask.ButtonMotionMask |
						EventMask.PointerMotionMask |
						EventMask.PointerMotionHintMask |
						EventMask.ButtonPressMask |
						EventMask.ButtonReleaseMask,
						cursor, IntPtr.Zero);
				return;
			}

			OverrideCursorHandle = cursor;
		}

		internal override PaintEventArgs PaintEventStart(ref Message msg, IntPtr handle, bool client)
		{
			PaintEventArgs	paint_event;
			Hwnd		hwnd;
			Hwnd		paint_hwnd;
			
			// 
			// handle  (and paint_hwnd) refers to the window that is should be painted.
			// msg.HWnd (and hwnd) refers to the window that got the paint message.
			// 
			
			hwnd = Hwnd.ObjectFromHandle(msg.HWnd);
			if (msg.HWnd == handle) {
				paint_hwnd = hwnd;
			} else {
				paint_hwnd = Hwnd.ObjectFromHandle (handle);
			}
	
			if (Caret.Visible == true) {
				Caret.Paused = true;
				HideCaret();
			}

			Graphics dc;

			if (client) {
				dc = Graphics.FromHwnd (paint_hwnd.client_window);

				Region clip_region = new Region ();
				clip_region.MakeEmpty();

				foreach (Rectangle r in hwnd.ClipRectangles) {
					/* Expand the region slightly.
					 * See bug 464464.
					 */
					Rectangle r2 = Rectangle.FromLTRB (r.Left, r.Top, r.Right, r.Bottom + 1);
					clip_region.Union (r2);
				}

				if (hwnd.UserClip != null) {
					clip_region.Intersect(hwnd.UserClip);
				}

				dc.Clip = clip_region;
				paint_event = new PaintEventArgs(dc, hwnd.Invalid);
				hwnd.expose_pending = false;

				hwnd.ClearInvalidArea();

				return paint_event;
			} else {
				dc = Graphics.FromHwnd (paint_hwnd.whole_window);

				if (!hwnd.nc_invalid.IsEmpty) {
					dc.SetClip (hwnd.nc_invalid);
					paint_event = new PaintEventArgs(dc, hwnd.nc_invalid);
				} else {
					paint_event = new PaintEventArgs(dc, new Rectangle(0, 0, hwnd.width, hwnd.height));
				}
				hwnd.nc_expose_pending = false;

				hwnd.ClearNcInvalidArea ();

				return paint_event;
			}
		}

		internal override void PaintEventEnd(ref Message msg, IntPtr handle, bool client, PaintEventArgs pevent)
		{
			if (pevent.Graphics != null)
				pevent.Graphics.Dispose();
			pevent.SetGraphics(null);
			pevent.Dispose();

			if (Caret.Visible == true) {
				ShowCaret();
				Caret.Paused = false;
			}
		}

		[MonoTODO("Implement filtering and PM_NOREMOVE")]
		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags)
		{
			XEventQueue queue = (XEventQueue) queue_id;
			bool	pending;

			if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0) {
				throw new NotImplementedException("PeekMessage PM_NOREMOVE is not implemented yet");	// FIXME - Implement PM_NOREMOVE flag
			}

			pending = false;
			if (queue.Count > 0) {
				pending = true;
			} else {
				// Only call UpdateMessageQueue if real events are pending 
				// otherwise we go to sleep on the socket
				if (XPending(DisplayHandle) != 0) {
					UpdateMessageQueue((XEventQueue)queue_id);
					pending = true;
				} else if (((XEventQueue)queue_id).Paint.Count > 0) {
					pending = true;
				}
			}

			CheckTimers(queue.timer_list, DateTime.UtcNow);

			if (!pending) {
				return false;
			}
			return GetMessage(queue_id, ref msg, hWnd, wFilterMin, wFilterMax);
		}

		internal override bool PostMessage (IntPtr handle, Msg message, IntPtr wparam, IntPtr lparam)
		{
			XEvent xevent = new XEvent ();
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

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

			if (hwnd != null)
				hwnd.Queue.EnqueueLocked (xevent);
			else
				ThreadQueue(Thread.CurrentThread).EnqueueLocked (xevent);

			return true;
		}

		internal override void PostQuitMessage(int exitCode)
		{
			ApplicationContext ctx = Application.MWFThread.Current.Context;
			Form f = ctx != null ? ctx.MainForm : null;
			if (f != null)
				PostMessage (Application.MWFThread.Current.Context.MainForm.window.Handle, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
			else
				PostMessage (FosterParent, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
			XFlush(DisplayHandle);
		}

		internal override void RequestAdditionalWM_NCMessages(IntPtr hwnd, bool hover, bool leave)
		{
			// TODO
		}

		internal override void RequestNCRecalc(IntPtr handle)
		{
			Hwnd				hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			PerformNCCalc(hwnd);
			SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			InvalidateNC(handle);
		}

		internal override void ResetMouseHover(IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd == null) {
				return;
			}

			HoverState.Timer.Enabled = true;
			HoverState.X = mouse_position.X;
			HoverState.Y = mouse_position.Y;
			HoverState.Window = handle;
		}


		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, RootWindow, hwnd.client_window, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void ScreenToMenu(IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, RootWindow, hwnd.whole_window, x, y, out dest_x_return, out dest_y_return, out child);
			}

			Form form = Control.FromHandle (handle) as Form;
			if (form != null && form.window_manager != null) {
				dest_y_return -= form.window_manager.TitleBarHeight;
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		bool GraphicsExposePredicate (IntPtr display, ref XEvent xevent, IntPtr arg)
		{
			return (xevent.type == XEventName.GraphicsExpose || xevent.type == XEventName.NoExpose) &&
				arg == xevent.GraphicsExposeEvent.drawable;
		}

		delegate bool EventPredicate (IntPtr display, ref XEvent xevent, IntPtr arg);

		void ProcessGraphicsExpose (Hwnd hwnd)
		{
			XEvent xevent = new XEvent ();
			IntPtr handle = Hwnd.HandleFromObject (hwnd);
			EventPredicate predicate = GraphicsExposePredicate;

			for (;;) {
				XIfEvent (Display, ref xevent, predicate, handle);
				if (xevent.type != XEventName.GraphicsExpose)
					break;

				AddExpose (hwnd, xevent.ExposeEvent.window == hwnd.ClientWindow, xevent.GraphicsExposeEvent.x, xevent.GraphicsExposeEvent.y,
						xevent.GraphicsExposeEvent.width, xevent.GraphicsExposeEvent.height);

				if (xevent.GraphicsExposeEvent.count == 0)
					break;
			}
		}

		internal override void ScrollWindow(IntPtr handle, Rectangle area, int XAmount, int YAmount, bool with_children)
		{
			Hwnd		hwnd;
			IntPtr		gc;
			XGCValues	gc_values;

			hwnd = Hwnd.ObjectFromHandle(handle);

			Rectangle r = Rectangle.Intersect (hwnd.Invalid, area);
			if (!r.IsEmpty) {
				/* We have an invalid area in the window we're scrolling. 
				   Adjust our stored invalid rectangle to to match the scrolled amount */

				r.X += XAmount;
				r.Y += YAmount;

				if (r.X < 0) {
					r.Width += r.X;
					r.X =0;
				}

				if (r.Y < 0) {
					r.Height += r.Y;
					r.Y =0;
				}

				if (area.Contains (hwnd.Invalid))
					hwnd.ClearInvalidArea ();
				hwnd.AddInvalidArea(r);
			}

			gc_values = new XGCValues();

			if (with_children) {
				gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			}

			gc = XCreateGC(DisplayHandle, hwnd.client_window, IntPtr.Zero, ref gc_values);

			Rectangle visible_rect = GetTotalVisibleArea (hwnd.client_window);
			visible_rect.Intersect (area);

			Rectangle dest_rect = visible_rect;
			dest_rect.Y += YAmount;
			dest_rect.X += XAmount;
			dest_rect.Intersect (area);

			Point src = new Point (dest_rect.X - XAmount, dest_rect.Y - YAmount);
			XCopyArea (DisplayHandle, hwnd.client_window, hwnd.client_window, gc, src.X, src.Y, 
					dest_rect.Width, dest_rect.Height, dest_rect.X, dest_rect.Y);

			Rectangle dirty_area = GetDirtyArea (area, dest_rect, XAmount, YAmount);
			AddExpose (hwnd, true, dirty_area.X, dirty_area.Y, dirty_area.Width, dirty_area.Height);

			ProcessGraphicsExpose (hwnd);

			XFreeGC(DisplayHandle, gc);
		}

		internal override void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children)
		{
			Hwnd		hwnd;
			Rectangle	rect;

			hwnd = Hwnd.GetObjectFromWindow(handle);

			rect = hwnd.ClientRect;
			rect.X = 0;
			rect.Y = 0;
			ScrollWindow(handle, rect, XAmount, YAmount, with_children);
		}

		Rectangle GetDirtyArea (Rectangle total_area, Rectangle valid_area, int XAmount, int YAmount)
		{
			Rectangle dirty_area = total_area;

			if (YAmount > 0)
				dirty_area.Height -= valid_area.Height;
			else if (YAmount < 0) {
				dirty_area.Height -= valid_area.Height;
				dirty_area.Y += valid_area.Height;
			}

			if (XAmount > 0)
				dirty_area.Width -= valid_area.Width;
			else if (XAmount < 0) {
				dirty_area.Width -= valid_area.Width;
				dirty_area.X += valid_area.Width;
			}

			return dirty_area;
		}

		Rectangle GetTotalVisibleArea (IntPtr handle)
		{
			Control c = Control.FromHandle (handle);

			Rectangle visible_area = c.ClientRectangle;
			visible_area.Location = c.PointToScreen (Point.Empty);

			for (Control parent = c.Parent; parent != null; parent = parent.Parent) {
				if (!parent.IsHandleCreated || !parent.Visible)
					return visible_area; // Non visible, not need to finish computations

				Rectangle r = parent.ClientRectangle;
				r.Location = parent.PointToScreen (Point.Empty);

				visible_area.Intersect (r);
			}

			visible_area.Location = c.PointToClient (visible_area.Location);
			return visible_area;
		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			Hwnd	hwnd;
			XEvent	xevent = new XEvent ();

			hwnd = Hwnd.ObjectFromHandle(method.Handle);

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = method.Handle;
			xevent.ClientMessageEvent.message_type = (IntPtr)AsyncAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) GCHandle.Alloc (method);

			hwnd.Queue.EnqueueLocked (xevent);

			WakeupMain ();
		}

		delegate IntPtr WndProcDelegate (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);

		internal override IntPtr SendMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam)
		{
			Hwnd	h;
			h = Hwnd.ObjectFromHandle(hwnd);

			if (h != null && h.queue != ThreadQueue (Thread.CurrentThread)) {
				AsyncMethodResult	result;
				AsyncMethodData		data;

				result = new AsyncMethodResult ();
				data = new AsyncMethodData ();

				data.Handle = hwnd;
				data.Method = new WndProcDelegate (NativeWindow.WndProc);
				data.Args = new object[] { hwnd, message, wParam, lParam };
				data.Result = result;
				
				SendAsyncMethod (data);
				DriverDebug("Sending {0} message across.", message);

				return IntPtr.Zero;
			}
			string key = hwnd + ":" + message;
			if (messageHold[key] != null)
				messageHold[key] = ((int)messageHold[key]) - 1;
			return NativeWindow.WndProc(hwnd, message, wParam, lParam);
		}

		internal override int SendInput(IntPtr handle, Queue keys)
		{ 
			if (handle == IntPtr.Zero)
				return 0;

			int count = keys.Count;
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

			while (keys.Count > 0) {
			
				MSG msg = (MSG)keys.Dequeue();

				XEvent xevent = new XEvent ();

				xevent.type = (msg.message == Msg.WM_KEYUP ? XEventName.KeyRelease : XEventName.KeyPress);
				xevent.KeyEvent.display = DisplayHandle;

				if (hwnd != null) {
					xevent.KeyEvent.window = hwnd.whole_window;
				} else {
					xevent.KeyEvent.window = IntPtr.Zero;
				}

				xevent.KeyEvent.keycode = Keyboard.ToKeycode((int)msg.wParam);

				hwnd.Queue.EnqueueLocked (xevent);
			}
			return count;
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

		internal override void SetBorderStyle(IntPtr handle, FormBorderStyle border_style)
		{
			Form form = Control.FromHandle (handle) as Form;
			if (form != null && form.window_manager == null) {
				CreateParams cp = form.GetCreateParams ();
				if (border_style == FormBorderStyle.FixedToolWindow ||
				     border_style == FormBorderStyle.SizableToolWindow || 
				     cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW)) {
					form.window_manager = new ToolWindowManager (form);
				}
			}
			
			RequestNCRecalc(handle);
		}

		internal override void SetCaretPos(IntPtr handle, int x, int y)
		{
			if (Caret.Hwnd == handle) {
				Caret.Timer.Stop();
				HideCaret();

				Caret.X = x;
				Caret.Y = y;

				Keyboard.SetCaretPos (Caret, handle, x, y);

				if (Caret.Visible == true) {
					ShowCaret();
					Caret.Timer.Start();
				}
			}
		}

		internal bool HasShapeExtension {
			get {
				if (!hasShapeExtension.HasValue) {
					try {
						hasShapeExtension = XShapeQueryExtension(DisplayHandle, out _, out _);
					} catch {
						hasShapeExtension = false;
					}
				}
				
				return hasShapeExtension.Value;
			}
		}

		internal override bool UserClipWontExposeParent {
			get {
				return !HasShapeExtension;
			}
		}

		internal override void SetClipRegion(IntPtr handle, Region region)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd == null) {
				return;
			}

			if (hwnd.UserClip != region) {
				hwnd.UserClip = region;

				if (!HasShapeExtension)
					return;

				XRectangle[] rects = null;;
				if (region == null) {
					rects = new XRectangle[1];
					rects[0].X = 0;
					rects[0].Y = 0;
					rects[0].Width = (ushort)hwnd.Width;
					rects[0].Height = (ushort)hwnd.Height;
				} else {
					RectangleF[] scans;
					using (var m = new Matrix())
						scans = region.GetRegionScans(m);
					rects = new XRectangle[scans.Length];
					for (int i = 0; i < scans.Length; i++) {
						rects[i].X = (short) Math.Clamp(scans[i].X, short.MinValue, short.MaxValue);
						rects[i].Y = (short) Math.Clamp(scans[i].Y, short.MinValue, short.MaxValue);
						rects[i].Width = (ushort) Math.Clamp(scans[i].Width, ushort.MinValue, ushort.MaxValue);
						rects[i].Height = (ushort) Math.Clamp(scans[i].Height, ushort.MinValue, ushort.MaxValue);
					}
				}
				XShapeCombineRectangles(DisplayHandle, hwnd.WholeWindow, XShapeKind.ShapeBounding, 0, 0, rects, rects.Length, XShapeOperation.ShapeSet, XOrdering.Unsorted);
			}
		}

		internal override void SetCursor(IntPtr handle, IntPtr cursor)
		{
			Hwnd	hwnd;

			if (OverrideCursorHandle == IntPtr.Zero) {
				if ((LastCursorWindow == handle) && (LastCursorHandle == cursor)) {
					return;
				}

				LastCursorHandle = cursor;
				LastCursorWindow = handle;

				hwnd = Hwnd.ObjectFromHandle(handle);
				lock (XlibLock) {
					if (cursor != IntPtr.Zero) {
						XDefineCursor(DisplayHandle, hwnd.whole_window, cursor);
					} else {
						XUndefineCursor(DisplayHandle, hwnd.whole_window);
					}
					XFlush(DisplayHandle);
				}
				return;
			}

			hwnd = Hwnd.ObjectFromHandle(handle);
			lock (XlibLock) {
				XDefineCursor(DisplayHandle, hwnd.whole_window, OverrideCursorHandle);
			}
		}

		void QueryPointer (IntPtr display, IntPtr w, out IntPtr root, out IntPtr child,
					   out int root_x, out int root_y, out int child_x, out int child_y,
					   out int mask)
		{
			/* this code was written with the help of
			glance at gdk.  I never would have realized we
			needed a loop in order to traverse down in the
			hierarchy.  I would have assumed you'd get the
			most deeply nested child and have to do
			XQueryTree to move back up the hierarchy..
			stupid me, of course. */
			IntPtr c;

			XGrabServer (display);

			XQueryPointer(display, w, out root, out c,
				      out root_x, out root_y, out child_x, out child_y,
				      out mask);

			if (root != w)
				c = root;

			IntPtr child_last = IntPtr.Zero;
			while (c != IntPtr.Zero) {
				child_last = c;
				XQueryPointer(display, c, out root, out c,
					      out root_x, out root_y, out child_x, out child_y,
					      out mask);
			}
			XUngrabServer (display);
			XFlush (display);

			child = child_last;
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y)
		{
			if (handle == IntPtr.Zero) {
				lock (XlibLock) {
					IntPtr root, child;
					int root_x, root_y, child_x, child_y, mask;

					/* we need to do a
					 * QueryPointer before warping
					 * because if the warp is on
					 * the RootWindow, the x/y are
					 * relative to the current
					 * mouse position
					 */
					QueryPointer (DisplayHandle, RootWindow,
						      out root,
						      out child,
						      out root_x, out root_y,
						      out child_x, out child_y,
						      out mask);

					XWarpPointer(DisplayHandle, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, x - root_x, y - root_y);

					XFlush (DisplayHandle);

					/* then we need to a
					 * QueryPointer after warping
					 * to manually generate a
					 * motion event for the window
					 * we move into.
					 */
					QueryPointer (DisplayHandle, RootWindow,
						      out root,
						      out child,
						      out root_x, out root_y,
						      out child_x, out child_y,
						      out mask);

					Hwnd child_hwnd = Hwnd.ObjectFromHandle(child);
					if (child_hwnd == null) {
						return;
					}

					XEvent xevent = new XEvent ();

					xevent.type = XEventName.MotionNotify;
					xevent.MotionEvent.display = DisplayHandle;
					xevent.MotionEvent.window = child_hwnd.client_window;
					xevent.MotionEvent.root = RootWindow;
					xevent.MotionEvent.x = child_x;
					xevent.MotionEvent.y = child_y;
					xevent.MotionEvent.x_root = root_x;
					xevent.MotionEvent.y_root = root_y;
					xevent.MotionEvent.state = mask;

					child_hwnd.Queue.EnqueueLocked (xevent);
				}
			} else {
				Hwnd	hwnd;

				hwnd = Hwnd.ObjectFromHandle(handle);
				lock (XlibLock) {
					XWarpPointer(DisplayHandle, IntPtr.Zero, hwnd.client_window, 0, 0, 0, 0, x, y);
				}
			}
		}

		internal override void SetFocus(IntPtr handle)
		{
			Hwnd	hwnd;
			IntPtr	prev_focus_window;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd.client_window == FocusWindow) {
				return;
			}

			// Win32 doesn't do anything if disabled
			if (!hwnd.enabled)
				return;

			prev_focus_window = FocusWindow;
			FocusWindow = hwnd.client_window;

			if (prev_focus_window != IntPtr.Zero) {
				SendMessage(prev_focus_window, Msg.WM_KILLFOCUS, FocusWindow, IntPtr.Zero);
			}			
			Keyboard.FocusIn (FocusWindow);
			SendMessage(FocusWindow, Msg.WM_SETFOCUS, prev_focus_window, IntPtr.Zero);

			//XSetInputFocus(DisplayHandle, Hwnd.ObjectFromHandle(handle).client_window, RevertTo.None, IntPtr.Zero);
		}

		internal override void SetIcon(IntPtr handle, Icon icon)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				SetIcon(hwnd, icon);
			}
		}

		internal override void SetMenu(IntPtr handle, Menu menu)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.menu = menu;

			RequestNCRecalc(handle);
		}

		internal override void SetModal(IntPtr handle, bool Modal)
		{
			if (Modal) {
				ModalWindows.Push(handle);
			} else {
				if (ModalWindows.Contains(handle)) {
					ModalWindows.Pop();
				}
				if (ModalWindows.Count > 0) {
					Activate((IntPtr)ModalWindows.Peek());
				}
			}

			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Control ctrl = Control.FromHandle (handle);
			SetWMStyles (hwnd, ctrl.GetCreateParams ());
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.parent = Hwnd.ObjectFromHandle(parent);

			lock (XlibLock) {
				DriverDebug("Parent for window {0} = {1}", XplatUI.Window(hwnd.Handle), XplatUI.Window(hwnd.parent != null ? hwnd.parent.Handle : IntPtr.Zero));
				XReparentWindow(DisplayHandle, hwnd.whole_window, hwnd.parent == null ? FosterParent : hwnd.parent.client_window, hwnd.x, hwnd.y);
			}

			return IntPtr.Zero;
		}

		internal override void SetTimer (Timer timer)
		{
			XEventQueue queue = (XEventQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue at this stage (it will be
				// enabled when a window is activated).
				unattached_timer_list.Add (timer);
				return;
			}
			queue.timer_list.Add (timer);
			WakeupMain ();
		}

		internal override bool SetTopmost(IntPtr handle, bool enabled)
		{

			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.topmost = enabled;

			if (enabled) {
				lock (XlibLock) {
					if (hwnd.Mapped) {
						SendNetWMMessage(hwnd.WholeWindow, _NET_WM_STATE, (IntPtr) NetWmStateRequest._NET_WM_STATE_ADD, _NET_WM_STATE_ABOVE, IntPtr.Zero);
					} else {
						int[] atoms = new int[8];
						atoms[0] = _NET_WM_STATE_ABOVE.ToInt32();
						XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);
					}
				}
			} else {
				lock (XlibLock) {
					if (hwnd.Mapped)
						SendNetWMMessage(hwnd.WholeWindow, _NET_WM_STATE, (IntPtr) NetWmStateRequest._NET_WM_STATE_REMOVE, _NET_WM_STATE_ABOVE, IntPtr.Zero);
					else
						XDeleteProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE);
				}
			}
			return true;
		}

		internal override bool SetOwner(IntPtr handle, IntPtr handle_owner)
		{
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (handle_owner != IntPtr.Zero) {
				hwnd.owner = Hwnd.ObjectFromHandle(handle_owner);
				lock (XlibLock) {
					int[]	atoms;

					atoms = new int[8];

					atoms[0] = _NET_WM_WINDOW_TYPE_NORMAL.ToInt32();
					XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_WINDOW_TYPE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);

					if (hwnd.owner != null) {
						XSetTransientForHint(DisplayHandle, hwnd.whole_window, hwnd.owner.whole_window);
					} else {
						XSetTransientForHint(DisplayHandle, hwnd.whole_window, RootWindow);
					}
				}
			} else {
				hwnd.owner = null;
				lock (XlibLock) {
					XDeleteProperty(DisplayHandle, hwnd.whole_window, (IntPtr)Atom.XA_WM_TRANSIENT_FOR);
				}
			}
			return true;
		}

		internal override bool SetVisible (IntPtr handle, bool visible, bool activate)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.visible = visible;

			lock (XlibLock) {
				if (visible) {
					MapWindow(hwnd, WindowType.Both);

					if (Control.FromHandle(handle) is Form) {
						FormWindowState	s;

						s = ((Form)Control.FromHandle(handle)).WindowState;

						switch(s) {
							case FormWindowState.Minimized:	SetWindowState(handle, FormWindowState.Minimized); break;
							case FormWindowState.Maximized:	SetWindowState(handle, FormWindowState.Maximized); break;
						}
					}

					SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
				}
				else {
					UnmapWindow(hwnd, WindowType.Both);
				}
			}
			return true;
		}

		internal override void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max)
		{
			Control ctrl = Control.FromHandle (handle);
			SetWindowMinMax (handle, maximized, min, max, ctrl != null ? ctrl.GetCreateParams () : null);
		}

		internal void SetWindowMinMax (IntPtr handle, Rectangle maximized, Size min, Size max, CreateParams cp)
		{
			Hwnd		hwnd;
			XSizeHints	hints;
			IntPtr		dummy;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd == null) {
				return;
			}

			min.Width = Math.Max (min.Width, SystemInformation.MinimumWindowSize.Width);
			min.Height = Math.Max (min.Height, SystemInformation.MinimumWindowSize.Height);
			
			hints = new XSizeHints();

			XGetWMNormalHints(DisplayHandle, hwnd.whole_window, ref hints, out dummy);
			if ((min != Size.Empty) && (min.Width > 0) && (min.Height > 0)) {
				if (cp != null)
					min = TranslateWindowSizeToXWindowSize (cp, min);
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMinSize);
				hints.min_width = min.Width;
				hints.min_height = min.Height;
			}

			if ((max != Size.Empty) && (max.Width > 0) && (max.Height > 0)) {
				if (cp != null)
					max = TranslateWindowSizeToXWindowSize (cp, max);
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMaxSize);
				hints.max_width = max.Width;
				hints.max_height = max.Height;
			}

			if (hints.flags != IntPtr.Zero) {
				// The Metacity team has decided that they won't care about this when clicking the maximize icon, 
				// they will maximize the window to fill the screen/parent no matter what.
				// http://bugzilla.ximian.com/show_bug.cgi?id=80021
				XSetWMNormalHints(DisplayHandle, hwnd.whole_window, ref hints);
			}

			if ((maximized != Rectangle.Empty) && (maximized.Width > 0) && (maximized.Height > 0)) {
				if (cp != null)
					maximized.Size = TranslateWindowSizeToXWindowSize (cp);
				hints.flags = (IntPtr)XSizeHintsFlags.PPosition;
				hints.x = maximized.X;
				hints.y = maximized.Y;
				hints.width = maximized.Width;
				hints.height = maximized.Height;

				// Metacity does not seem to follow this constraint for maximized (zoomed) windows
				XSetZoomHints(DisplayHandle, hwnd.whole_window, ref hints);
			}
		}


		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height)
		{
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			// Win32 automatically changes negative width/height to 0.
			if (width < 0)
				width = 0;
			if (height < 0)
				height = 0;
				
			// X requires a sanity check for width & height; otherwise it dies
			if (hwnd.zero_sized && width > 0 && height > 0) {
				if (hwnd.visible) {
					MapWindow(hwnd, WindowType.Whole);
				}
				hwnd.zero_sized = false;
			}

			if ((width < 1) || (height < 1)) {
				hwnd.zero_sized = true;
				UnmapWindow(hwnd, WindowType.Whole);
			}

			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && 
				(hwnd.width == width) && (hwnd.height == height)) {
				return;
			}

			hwnd.x = x;
			hwnd.y = y;
			hwnd.width = width;
			hwnd.height = height;

			if (!hwnd.zero_sized) {
				if (hwnd.fixed_size) {
					SetWindowMinMax(handle, Rectangle.Empty, new Size(width, height), new Size(width, height));
				}

				lock (XlibLock) {
					Control ctrl = Control.FromHandle (handle);
					Size TranslatedSize = TranslateWindowSizeToXWindowSize (ctrl.GetCreateParams (), new Size (width, height));
					MoveResizeWindow (DisplayHandle, hwnd.whole_window, x, y, TranslatedSize.Width, TranslatedSize.Height);
					PerformNCCalc(hwnd);
				}
			}

			SendMessage(hwnd.client_window, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
		}

		internal override void SetWindowState(IntPtr handle, FormWindowState state)
		{
			FormWindowState	current_state;
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			current_state = GetWindowState(handle);

			if (current_state == state) {
				return;
			}

			switch(state) {
				case FormWindowState.Normal: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Minimized) {
							MapWindow(hwnd, WindowType.Both);
						} else if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)2 /* toggle */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
						}
					}
					Activate(handle);
					return;
				}

				case FormWindowState.Minimized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)2 /* toggle */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
						}
						XIconifyWindow(DisplayHandle, hwnd.whole_window, ScreenNo);
					}
					return;
				}

				case FormWindowState.Maximized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Minimized) {
							MapWindow(hwnd, WindowType.Both);
						}

						SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)1 /* Add */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
					}
					Activate(handle);
					return;
				}
			}
		}

		internal override void SetWindowStyle(IntPtr handle, CreateParams cp)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			SetHwndStyles(hwnd, cp);
			SetWMStyles(hwnd, cp);
		}

		internal override double GetWindowTransparency(IntPtr handle)
		{
			return 1.0;
		}

		internal override void SetWindowTransparency(IntPtr handle, double transparency, Color key)
		{
			Hwnd	hwnd;
			IntPtr	opacity;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			hwnd.opacity = (uint)(0xffffffff * transparency);
			opacity = (IntPtr)hwnd.opacity;

			if (transparency >= 1.0) {
				XDeleteProperty (DisplayHandle, hwnd.whole_window, _NET_WM_WINDOW_OPACITY);
			} else {
				XChangeProperty (DisplayHandle, hwnd.whole_window, _NET_WM_WINDOW_OPACITY, (IntPtr)Atom.XA_CARDINAL, 32, PropertyMode.Replace, ref opacity, 1);
			}
		}

		internal override bool SetZOrder(IntPtr handle, IntPtr after_handle, bool top, bool bottom)
		{
			Hwnd	hwnd = Hwnd.ObjectFromHandle(handle);

			if (!hwnd.mapped) {
				return false;
			}

			if (top) {
				lock (XlibLock) {
					XRaiseWindow(DisplayHandle, hwnd.whole_window);
				}
				return true;
			} else if (!bottom) {
				Hwnd	after_hwnd = null;

				if (after_handle != IntPtr.Zero) {
					after_hwnd = Hwnd.ObjectFromHandle(after_handle);
				}

				XWindowChanges	values = new XWindowChanges();

				if (after_hwnd == null) {
					// Work around metacity 'issues'
					int[]	atoms;

					atoms = new int[2];
					atoms[0] = unixtime();
					XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_USER_TIME, (IntPtr)Atom.XA_CARDINAL, 32, PropertyMode.Replace, atoms, 1);

					XRaiseWindow(DisplayHandle, hwnd.whole_window);
					SendNetWMMessage(hwnd.whole_window, _NET_ACTIVE_WINDOW, (IntPtr)1, IntPtr.Zero, IntPtr.Zero);
					return true;
					//throw new ArgumentNullException("after_handle", "Need sibling to adjust z-order");
				}

				values.sibling = after_hwnd.whole_window;
				values.stack_mode = StackMode.Below;

				lock (XlibLock) {
					XConfigureWindow(DisplayHandle, hwnd.whole_window, ChangeWindowFlags.CWStackMode | ChangeWindowFlags.CWSibling, ref values);
				}
			} else {
				// Bottom
				lock (XlibLock) {
					XLowerWindow(DisplayHandle, hwnd.whole_window);
				}
				return true;
			}
			return false;
		}

		internal override void ShowCursor(bool show)
		{
			;	// FIXME - X11 doesn't 'hide' the cursor. we could create an empty cursor
		}

		internal override object StartLoop(Thread thread)
		{
			XEventQueue q = ThreadQueue(thread);
			return q;
		}

		internal override TransparencySupport SupportsTransparency()
		{
			// We need to check if the x compositing manager is running
			return TransparencySupport.Set;
		}

		internal override bool SystrayAdd(IntPtr handle, string tip, Icon icon, out ToolTip tt)
		{
			GetSystrayManagerWindow();

			if (SystrayMgrWindow != IntPtr.Zero) {
				XSizeHints	size_hints;
				Hwnd		hwnd;

				hwnd = Hwnd.ObjectFromHandle(handle);
				DriverDebug("Adding Systray Whole:{0:X}, Client:{1:X}", hwnd.whole_window.ToInt32(), hwnd.client_window.ToInt32());

				// Oh boy.
				if (hwnd.client_window != hwnd.whole_window) {
					Keyboard.DestroyICForWindow (hwnd.client_window);
					XDestroyWindow(DisplayHandle, hwnd.client_window);
					hwnd.client_window = hwnd.whole_window;
				}	

				/* by virtue of the way the tests are ordered when determining if it's PAINT
				   or NCPAINT, client_window == whole_window will always be PAINT.  So, if we're
				   waiting on an nc_expose, drop it and remove the hwnd from the list (unless
				   there's a pending expose). */
				if (hwnd.nc_expose_pending) {
					hwnd.nc_expose_pending = false;
					if (!hwnd.expose_pending)
						hwnd.Queue.Paint.Remove (hwnd);
				}

				// We are going to be directly mapped by the system tray, so mark as mapped
				// so we can later properly unmap it.
				hwnd.mapped = true;

				size_hints = new XSizeHints();

				size_hints.flags = (IntPtr)(XSizeHintsFlags.PMinSize | XSizeHintsFlags.PMaxSize | XSizeHintsFlags.PBaseSize);

 				size_hints.min_width = 24;
 				size_hints.min_height = 24;
 				size_hints.max_width = 24;
 				size_hints.max_height = 24;
 				size_hints.base_width = 24;
 				size_hints.base_height = 24;

				XSetWMNormalHints(DisplayHandle, hwnd.whole_window, ref size_hints);

				IntPtr[] atoms = new IntPtr[2];
				atoms [0] = (IntPtr)1;			// Version 1
				atoms [1] = (IntPtr)1;			// we want to be mapped

				// This line cost me 3 days...
				XChangeProperty(DisplayHandle, hwnd.whole_window, _XEMBED_INFO, _XEMBED_INFO, 32, PropertyMode.Replace, atoms, 2);

				// Need to pick some reasonable defaults
				tt = new ToolTip();
				tt.AutomaticDelay = 350;
				tt.InitialDelay = 250;
				tt.ReshowDelay = 250;
				tt.ShowAlways = true;

				if ((tip != null) && (tip != string.Empty)) {
					tt.SetToolTip(Control.FromHandle(handle), tip);
					tt.Active = true;
				} else {
					tt.Active = false;
				}

				SendNetClientMessage(SystrayMgrWindow, _NET_SYSTEM_TRAY_OPCODE, IntPtr.Zero, (IntPtr)SystrayRequest.SYSTEM_TRAY_REQUEST_DOCK, hwnd.whole_window);

				return true;
			}
			tt = null;
			return false;
		}

		internal override bool SystrayChange(IntPtr handle, string tip, Icon icon, ref ToolTip tt)
		{
			Control	control;

			control = Control.FromHandle(handle);
			if (control != null && tt != null) {
				tt.SetToolTip(control, tip);
				tt.Active = true;
				SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
				return true;
			} else {
				return false;
			}
		}

		internal override void SystrayRemove(IntPtr handle, ref ToolTip tt)
		{

			SetVisible (handle, false, false);

			// The caller can now re-dock it later...
			if (tt != null) {
				tt.Dispose();
				tt = null;
			}
			// Close any balloon window *we* fired.
			ThemeEngine.Current.HideBalloonWindow (handle);
		}

		internal override void SystrayBalloon(IntPtr handle, int timeout, string title, string text, ToolTipIcon icon)
		{
			ThemeEngine.Current.ShowBalloonWindow (handle, timeout, title, text, icon);
			SendMessage(handle, Msg.WM_USER, IntPtr.Zero, (IntPtr) Msg.NIN_BALLOONSHOW);	
		}

		internal override bool Text(IntPtr handle, string text)
{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_NAME, UTF8_STRING, 8,
						PropertyMode.Replace, text, Encoding.UTF8.GetByteCount (text));

				// XXX this has problems with UTF8.
				// we need to either use the actual
				// text if it's latin-1, or convert it
				// to compound text if it's in a
				// different charset.
				XStoreName(DisplayHandle, Hwnd.ObjectFromHandle(handle).whole_window, text);
			}
			return true;
		}

		internal override bool TranslateMessage(ref MSG msg)
		{
			return Keyboard.TranslateMessage (ref msg);
		}

		internal override void UpdateWindow(IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (!hwnd.visible || !hwnd.expose_pending || !hwnd.Mapped) {
				return;
			}

			SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			hwnd.Queue.Paint.Remove(hwnd);
		}

		internal override void CreateOffscreenDrawable (IntPtr handle,
								int width, int height,
								out object offscreen_drawable)
		{
			IntPtr root_out;
			int x_out, y_out, width_out, height_out, border_width_out, depth_out;

			XGetGeometry (DisplayHandle, handle,
				      out root_out,
				      out x_out, out y_out,
				      out width_out, out height_out,
				      out border_width_out, out depth_out);

			IntPtr pixmap = XCreatePixmap (DisplayHandle, handle, width, height, depth_out);

			offscreen_drawable = pixmap;

		}

		internal override void DestroyOffscreenDrawable (object offscreen_drawable)
		{
			XFreePixmap (DisplayHandle, (IntPtr)offscreen_drawable);
		}

		internal override Graphics GetOffscreenGraphics (object offscreen_drawable)
		{
			return Graphics.FromHwnd ((IntPtr) offscreen_drawable);
		}
		
		internal override void BlitFromOffscreen (IntPtr dest_handle,
							  Graphics dest_dc,
							  object offscreen_drawable,
							  Graphics offscreen_dc,
							  Rectangle r)
		{
			XGCValues gc_values;
			IntPtr gc;

			gc_values = new XGCValues();

			gc = XCreateGC (DisplayHandle, dest_handle, IntPtr.Zero, ref gc_values);

			XCopyArea (DisplayHandle, (IntPtr)offscreen_drawable, dest_handle,
				   gc, r.X, r.Y, r.Width, r.Height, r.X, r.Y);

			XFreeGC (DisplayHandle, gc);
		}

		#endregion	// Public Static Methods

		#region Events
		internal override event EventHandler Idle;
		#endregion	// Events

		
#if TRACE && false
		
#region Xcursor imports
		[DllImport ("libXcursor", EntryPoint = "XcursorLibraryLoadCursor")]
		internal extern static IntPtr XcursorLibraryLoadCursor (IntPtr display, [MarshalAs (UnmanagedType.LPStr)] string name);

		[DllImport ("libXcursor", EntryPoint = "XcursorLibraryLoadImages")]
		internal extern static IntPtr XcursorLibraryLoadImages ([MarshalAs (UnmanagedType.LPStr)] string file, IntPtr theme, int size);
		
		[DllImport ("libXcursor", EntryPoint = "XcursorImagesDestroy")]
		internal extern static void XcursorImagesDestroy (IntPtr images);
		
		[DllImport ("libXcursor", EntryPoint = "XcursorGetDefaultSize")]
		internal extern static int XcursorGetDefaultSize (IntPtr display);

		[DllImport ("libXcursor", EntryPoint = "XcursorImageLoadCursor")]
		internal extern static IntPtr XcursorImageLoadCursor (IntPtr display, IntPtr image);

		[DllImport ("libXcursor", EntryPoint = "XcursorGetTheme")]
		internal extern static IntPtr XcursorGetTheme (IntPtr display);
#endregion
#region X11 Imports
		[DllImport ("libX11", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11", EntryPoint="XCloseDisplay")]
		internal extern static int XCloseDisplay(IntPtr display);						    
		[DllImport ("libX11", EntryPoint="XSynchronize")]
		internal extern static IntPtr XSynchronize(IntPtr display, bool onoff);

		[DllImport ("libX11", EntryPoint="XCreateWindow")]
		internal extern static IntPtr _XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, UIntPtr valuemask, ref XSetWindowAttributes attributes);
		internal static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, UIntPtr valuemask, ref XSetWindowAttributes attributes)
		{
			DebugHelper.TraceWriteLine ("XCreateWindow");
			return _XCreateWindow(display, parent, x, y, width, height, 
			               border_width, depth, xclass, visual, valuemask, ref attributes);
		}
		[DllImport ("libX11", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr _XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, UIntPtr border, UIntPtr background);
		internal static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, UIntPtr border, UIntPtr background)
		{
			DebugHelper.TraceWriteLine ("XCreateSimpleWindow");
			return _XCreateSimpleWindow(display, parent, x, y, width, height, border_width, border, background);
		}
		[DllImport ("libX11", EntryPoint="XMapWindow")]
		internal extern static int _XMapWindow(IntPtr display, IntPtr window);
		internal static int XMapWindow(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XMapWindow");
			return _XMapWindow(display, window);
		}
		[DllImport ("libX11", EntryPoint="XMapRaised")]
		internal extern static int _XMapRaised(IntPtr display, IntPtr window);
		internal static int XMapRaised(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XMapRaised");
			return _XMapRaised(display, window);
		}
		[DllImport ("libX11", EntryPoint="XUnmapWindow")]
		internal extern static int _XUnmapWindow(IntPtr display, IntPtr window);
		internal static int XUnmapWindow(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XUnmapWindow");
			return _XUnmapWindow(display, window);
		}
		[DllImport ("libX11", EntryPoint="XMapSubwindows")]
		internal extern static int _XMapSubindows(IntPtr display, IntPtr window);
		internal static int XMapSubindows(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XMapSubindows");
			return _XMapSubindows(display, window);
		}
		[DllImport ("libX11", EntryPoint="XUnmapSubwindows")]
		internal extern static int _XUnmapSubwindows(IntPtr display, IntPtr window);
		internal static int XUnmapSubwindows(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XUnmapSubwindows");
			return _XUnmapSubwindows(display, window);
		}
		[DllImport ("libX11", EntryPoint="XRootWindow")]
		internal extern static IntPtr _XRootWindow(IntPtr display, int screen_number);
		internal static IntPtr XRootWindow(IntPtr display, int screen_number)
		{
			DebugHelper.TraceWriteLine ("XRootWindow");
			return _XRootWindow(display, screen_number);
		}
		[DllImport ("libX11", EntryPoint="XNextEvent")]
		internal extern static IntPtr _XNextEvent(IntPtr display, ref XEvent xevent);
		internal static IntPtr XNextEvent(IntPtr display, ref XEvent xevent)
		{
			DebugHelper.TraceWriteLine ("XNextEvent");
			return _XNextEvent(display, ref xevent);
		}
		[DllImport ("libX11", EntryPoint="XConnectionNumber")]
		internal extern static int _XConnectionNumber (IntPtr display);
		internal static int XConnectionNumber (IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XConnectionNumber");
			return _XConnectionNumber (display);
		}
		[DllImport ("libX11", EntryPoint="XPending")]
		internal extern static int _XPending (IntPtr display);
		internal static int XPending (IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XPending");
			DebugHelper.DumpCallers (3);
			return _XPending (display);
		}
		[DllImport ("libX11", EntryPoint="XSelectInput")]
		internal extern static IntPtr _XSelectInput(IntPtr display, IntPtr window, IntPtr mask);
		internal static IntPtr XSelectInput(IntPtr display, IntPtr window, IntPtr mask)
		{
			DebugHelper.TraceWriteLine ("XSelectInput");
			return _XSelectInput(display, window, mask);
		}

		[DllImport ("libX11", EntryPoint="XDestroyWindow")]
		internal extern static int _XDestroyWindow(IntPtr display, IntPtr window);
		internal static int XDestroyWindow(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XDestroyWindow 0x{0:x}", window.ToInt32());
			return _XDestroyWindow(display, window);
		}

		[DllImport ("libX11", EntryPoint="XReparentWindow")]
		internal extern static int _XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		internal static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y)
		{
			DebugHelper.TraceWriteLine ("XReparentWindow");
			return _XReparentWindow(display, window, parent, x, y);
		}
		
		[DllImport ("libX11", EntryPoint="XMoveResizeWindow")]
		extern static int _XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);
		static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height) {
			DebugHelper.TraceWriteLine ("XMoveResizeWindow");
			return _XMoveResizeWindow(display, window, x, y, width, height);
		}

		internal static int MoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height)
		{
			int ret = XMoveResizeWindow (display, window, x, y, width, height);
			Keyboard.MoveCurrentCaretPos ();
			return ret;
		}

		[DllImport ("libX11", EntryPoint="XResizeWindow")]
		internal extern static int _XResizeWindow(IntPtr display, IntPtr window, int width, int height);
		internal static int XResizeWindow(IntPtr display, IntPtr window, int width, int height)
		{
			DebugHelper.TraceWriteLine ("XResizeWindow");
			return _XResizeWindow(display, window, width, height);
		}

		[DllImport ("libX11", EntryPoint="XGetWindowAttributes")]
		internal extern static int _XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);
		internal static int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes)
		{
			DebugHelper.TraceWriteLine ("XGetWindowAttributes");
			return _XGetWindowAttributes(display, window, ref attributes);
		}

		[DllImport ("libX11", EntryPoint="XFlush")]
		internal extern static int _XFlush(IntPtr display);
		internal static int XFlush(IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XFlush");
			return _XFlush(display);
		}

		[DllImport ("libX11", EntryPoint="XSetWMName")]
		internal extern static int _XSetWMName(IntPtr display, IntPtr window, ref XTextProperty text_prop);
		internal static int XSetWMName(IntPtr display, IntPtr window, ref XTextProperty text_prop)
		{
			DebugHelper.TraceWriteLine ("XSetWMName");
			return _XSetWMName(display, window, ref text_prop);
		}

		[DllImport ("libX11", EntryPoint="XStoreName")]
		internal extern static int _XStoreName(IntPtr display, IntPtr window, string window_name);
		internal static int XStoreName(IntPtr display, IntPtr window, string window_name)
		{
			DebugHelper.TraceWriteLine ("XStoreName");
			return _XStoreName(display, window, window_name);
		}

		[DllImport ("libX11", EntryPoint="XFetchName")]
		internal extern static int _XFetchName(IntPtr display, IntPtr window, ref IntPtr window_name);
		internal static int XFetchName(IntPtr display, IntPtr window, ref IntPtr window_name)
		{
			DebugHelper.TraceWriteLine ("XFetchName");
			return _XFetchName(display, window, ref window_name);
		}

		[DllImport ("libX11", EntryPoint="XSendEvent")]
		internal extern static int _XSendEvent(IntPtr display, IntPtr window, bool propagate, IntPtr event_mask, ref XEvent send_event);
		internal static int XSendEvent(IntPtr display, IntPtr window, bool propagate, IntPtr event_mask, ref XEvent send_event)
		{
			DebugHelper.TraceWriteLine ("XSendEvent");
			return _XSendEvent(display, window, propagate, event_mask, ref send_event);
		}

		[DllImport ("libX11", EntryPoint="XQueryTree")]
		internal extern static int _XQueryTree(IntPtr display, IntPtr window, out IntPtr root_return, out IntPtr parent_return, out IntPtr children_return, out int nchildren_return);
		internal static int XQueryTree(IntPtr display, IntPtr window, out IntPtr root_return, out IntPtr parent_return, out IntPtr children_return, out int nchildren_return)
		{
			DebugHelper.TraceWriteLine ("XQueryTree");
			return _XQueryTree(display, window, out root_return, out parent_return, out children_return, out nchildren_return);
		}

		[DllImport ("libX11", EntryPoint="XFree")]
		internal extern static int _XFree(IntPtr data);
		internal static int XFree(IntPtr data)
		{
			DebugHelper.TraceWriteLine ("XFree");
			return _XFree(data);
		}

		[DllImport ("libX11", EntryPoint="XRaiseWindow")]
		internal extern static int _XRaiseWindow(IntPtr display, IntPtr window);
		internal static int XRaiseWindow(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XRaiseWindow");
			return _XRaiseWindow(display, window);
		}

		[DllImport ("libX11", EntryPoint="XLowerWindow")]
		internal extern static uint _XLowerWindow(IntPtr display, IntPtr window);
		internal static uint XLowerWindow(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XLowerWindow");
			return _XLowerWindow(display, window);
		}

		[DllImport ("libX11", EntryPoint="XConfigureWindow")]
		internal extern static uint _XConfigureWindow(IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values);
		internal static uint XConfigureWindow(IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values)
		{
			DebugHelper.TraceWriteLine ("XConfigureWindow");
			return _XConfigureWindow(display, window, value_mask, ref values);
		}

		[DllImport ("libX11", EntryPoint="XInternAtom")]
		internal extern static IntPtr _XInternAtom(IntPtr display, string atom_name, bool only_if_exists);
		internal static IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists)
		{
			DebugHelper.TraceWriteLine ("XInternAtom");
			return _XInternAtom(display, atom_name, only_if_exists);
		}

		[DllImport ("libX11", EntryPoint="XInternAtoms")]
		internal extern static int _XInternAtoms(IntPtr display, string[] atom_names, int atom_count, bool only_if_exists, IntPtr[] atoms);
		internal static int XInternAtoms(IntPtr display, string[] atom_names, int atom_count, bool only_if_exists, IntPtr[] atoms)
		{
			DebugHelper.TraceWriteLine ("XInternAtoms");
			return _XInternAtoms(display, atom_names, atom_count, only_if_exists, atoms);
		}

		[DllImport ("libX11", EntryPoint="XSetWMProtocols")]
		internal extern static int _XSetWMProtocols(IntPtr display, IntPtr window, IntPtr[] protocols, int count);
		internal static int XSetWMProtocols(IntPtr display, IntPtr window, IntPtr[] protocols, int count)
		{
			DebugHelper.TraceWriteLine ("XSetWMProtocols");
			return _XSetWMProtocols(display, window, protocols, count);
		}

		[DllImport ("libX11", EntryPoint="XGrabPointer")]
		internal extern static int _XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, IntPtr cursor, IntPtr timestamp);
		internal static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, IntPtr cursor, IntPtr timestamp)
		{
			DebugHelper.TraceWriteLine ("XGrabPointer");
			return _XGrabPointer(display, window, owner_events, event_mask, pointer_mode, keyboard_mode, confine_to, cursor, timestamp);
		}

		[DllImport ("libX11", EntryPoint="XUngrabPointer")]
		internal extern static int _XUngrabPointer(IntPtr display, IntPtr timestamp);
		internal static int XUngrabPointer(IntPtr display, IntPtr timestamp)
		{
			DebugHelper.TraceWriteLine ("XUngrabPointer");
			return _XUngrabPointer(display, timestamp);
		}

		[DllImport ("libX11", EntryPoint="XQueryPointer")]
		internal extern static bool _XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);
		internal static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons)
		{
			DebugHelper.TraceWriteLine ("XQueryPointer");
			return _XQueryPointer(display, window, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);
		}

		[DllImport ("libX11", EntryPoint="XTranslateCoordinates")]
		internal extern static bool _XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return, out int dest_y_return, out IntPtr child_return);
		internal static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return,	 out int dest_y_return, out IntPtr child_return)
		{
			DebugHelper.TraceWriteLine ("XTranslateCoordinates");
			return _XTranslateCoordinates (display, src_w, dest_w, src_x, src_y, out intdest_x_return, out dest_y_return, out child_return);
		}

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool _XGetGeometry(IntPtr display, IntPtr window, out IntPtr root, out int x, out int y, out int width, out int height, out int border_width, out int depth);
		internal static bool XGetGeometry(IntPtr display, IntPtr window, out IntPtr root, out int x, out int y, out int width, out int height, out int border_width, out int depth)
		{
			DebugHelper.TraceWriteLine ("XGetGeometry");
			return _XGetGeometry(display, window, out root, out x, out y, out width, out height, out border_width, out depth);
		}

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool _XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, out int width, out int height, IntPtr border_width, IntPtr depth);
		internal static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, out int width, out int height, IntPtr border_width, IntPtr depth)
		{
			DebugHelper.TraceWriteLine ("XGetGeometry");
			return _XGetGeometry(display, window, root, out x, out y, out width, out height, border_width, depth);
		}

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool _XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, IntPtr width, IntPtr height, IntPtr border_width, IntPtr depth);
		internal static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, IntPtr width, IntPtr height, IntPtr border_width, IntPtr depth)
		{
			DebugHelper.TraceWriteLine ("XGetGeometry");
			return _XGetGeometry(display, window, root, out x, out y, width, height, border_width, depth);
		}

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool _XGetGeometry(IntPtr display, IntPtr window, IntPtr root, IntPtr x, IntPtr y, out int width, out int height, IntPtr border_width, IntPtr depth);
		internal static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, IntPtr x, IntPtr y, out int width, out int height, IntPtr border_width, IntPtr depth)
		{
			DebugHelper.TraceWriteLine ("XGetGeometry");
			return _XGetGeometry(display, window, root, x, y, out width, out height, border_width, depth);
		}

		[DllImport ("libX11", EntryPoint="XWarpPointer")]
		internal extern static uint _XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);
		internal static uint XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y)
		{
			DebugHelper.TraceWriteLine ("XWarpPointer");
			return _XWarpPointer(display, src_w, dest_w, src_x, src_y, src_width, src_height, dest_x, dest_y);
		}

		[DllImport ("libX11", EntryPoint="XClearWindow")]
		internal extern static int _XClearWindow(IntPtr display, IntPtr window);
		internal static int XClearWindow(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XClearWindow");
			return _XClearWindow(display, window);
		}

		[DllImport ("libX11", EntryPoint="XClearArea")]
		internal extern static int _XClearArea(IntPtr display, IntPtr window, int x, int y, int width, int height, bool exposures);
		internal static int XClearArea(IntPtr display, IntPtr window, int x, int y, int width, int height, bool exposures)
		{
			DebugHelper.TraceWriteLine ("XClearArea");
			return _XClearArea(display, window, x, y, width, height, exposures);
		}

		// Colormaps
		[DllImport ("libX11", EntryPoint="XDefaultScreenOfDisplay")]
		internal extern static IntPtr _XDefaultScreenOfDisplay(IntPtr display);
		internal static IntPtr XDefaultScreenOfDisplay(IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XDefaultScreenOfDisplay");
			return _XDefaultScreenOfDisplay(display);
		}

		[DllImport ("libX11", EntryPoint="XScreenNumberOfScreen")]
		internal extern static int _XScreenNumberOfScreen(IntPtr display, IntPtr Screen);
		internal static int XDefaultScreenOfDisplay(IntPtr display, IntPtr Screen)
		{
			DebugHelper.TraceWriteLine ("XDefaultScreenOfDisplay");
			return _XScreenNumberOfScreen(display, Screen);
		}

		[DllImport ("libX11", EntryPoint="XDefaultVisual")]
		internal extern static IntPtr _XDefaultVisual(IntPtr display, int screen_number);
		internal static IntPtr XDefaultScreenOfDisplay(IntPtr display, int screen_number)
		{
			DebugHelper.TraceWriteLine ("XDefaultScreenOfDisplay");
			return _XDefaultVisual(display, screen_number);
		}

		[DllImport ("libX11", EntryPoint="XDefaultDepth")]
		internal extern static uint _XDefaultDepth(IntPtr display, int screen_number);
		internal static uint XDefaultDepth(IntPtr display, int screen_number)
		{
			DebugHelper.TraceWriteLine ("XDefaultDepth");
			return _XDefaultDepth(display, screen_number);
		}

		[DllImport ("libX11", EntryPoint="XDefaultScreen")]
		internal extern static int _XDefaultScreen(IntPtr display);
		internal static int XDefaultScreen(IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XDefaultScreen");
			return _XDefaultScreen(display);
		}

		[DllImport ("libX11", EntryPoint="XDefaultColormap")]
		internal extern static IntPtr _XDefaultColormap(IntPtr display, int screen_number);
		internal static IntPtr XDefaultColormap(IntPtr display, int screen_number)
		{
			DebugHelper.TraceWriteLine ("XDefaultColormap");
			return _XDefaultColormap(display, screen_number);
		}

		[DllImport ("libX11", EntryPoint="XLookupColor")]
		internal extern static int _XLookupColor(IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);
		internal static int XLookupColor(IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color)
		{
			DebugHelper.TraceWriteLine ("XLookupColor");
			return _XLookupColor(display, Colormap, Coloranem, ref exact_def_color, ref screen_def_color);
		}

		[DllImport ("libX11", EntryPoint="XAllocColor")]
		internal extern static int _XAllocColor(IntPtr display, IntPtr Colormap, ref XColor colorcell_def);
		internal static int XAllocColor(IntPtr display, IntPtr Colormap, ref XColor colorcell_def)
		{
			DebugHelper.TraceWriteLine ("XAllocColor");
			return _XAllocColor(display, Colormap, ref colorcell_def);
		}

		[DllImport ("libX11", EntryPoint="XSetTransientForHint")]
		internal extern static int _XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window);
		internal static int XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window)
		{
			DebugHelper.TraceWriteLine ("XSetTransientForHint");
			return _XSetTransientForHint(display, window, prop_window);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref MotifWmHints data, int nelements);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref MotifWmHints data, int nelements)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, ref data, nelements);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref uint value, int nelements);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref uint value, int nelements)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, ref value, nelements);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref IntPtr value, int nelements);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref IntPtr value, int nelements)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, ref value, nelements);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, uint[] data, int nelements);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, uint[] data, int nelements)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, data, nelements);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, int[] data, int nelements);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, int[] data, int nelements)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, data, nelements);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr[] data, int nelements);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr[] data, int nelements)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, data, nelements);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr atoms, int nelements);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr atoms, int nelements)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, atoms, nelements);
		}

		[DllImport ("libX11", EntryPoint="XChangeProperty", CharSet=CharSet.Ansi)]
		internal extern static int _XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, string text, int text_length);
		internal static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, string text, int text_length)
		{
			DebugHelper.TraceWriteLine ("XChangeProperty");
			return _XChangeProperty(display, window, property, type, format, mode, text, text_length);
		}

		[DllImport ("libX11", EntryPoint="XDeleteProperty")]
		internal extern static int _XDeleteProperty(IntPtr display, IntPtr window, IntPtr property);
		internal static int XDeleteProperty(IntPtr display, IntPtr window, IntPtr property)
		{
			DebugHelper.TraceWriteLine ("XDeleteProperty");
			return _XDeleteProperty(display, window, property);
		}

		// Drawing
		[DllImport ("libX11", EntryPoint="XCreateGC")]
		internal extern static IntPtr _XCreateGC(IntPtr display, IntPtr window, IntPtr valuemask, ref XGCValues values);
		internal static IntPtr XCreateGC(IntPtr display, IntPtr window, IntPtr valuemask, ref XGCValues values)
		{
			DebugHelper.TraceWriteLine ("XCreateGC");
			return _XCreateGC(display, window, valuemask, ref values);
		}

		[DllImport ("libX11", EntryPoint="XFreeGC")]
		internal extern static int _XFreeGC(IntPtr display, IntPtr gc);
		internal static int XFreeGC(IntPtr display, IntPtr gc)
		{
			DebugHelper.TraceWriteLine ("XFreeGC");
			return _XFreeGC(display, gc);
		}

		[DllImport ("libX11", EntryPoint="XSetFunction")]
		internal extern static int _XSetFunction(IntPtr display, IntPtr gc, GXFunction function);
		internal static int XSetFunction(IntPtr display, IntPtr gc, GXFunction function)
		{
			DebugHelper.TraceWriteLine ("XSetFunction");
			return _XSetFunction(display, gc, function);
		}

		[DllImport ("libX11", EntryPoint="XSetLineAttributes")]
		internal extern static int _XSetLineAttributes(IntPtr display, IntPtr gc, int line_width, GCLineStyle line_style, GCCapStyle cap_style, GCJoinStyle join_style);
		internal static int XSetLineAttributes(IntPtr display, IntPtr gc, int line_width, GCLineStyle line_style, GCCapStyle cap_style, GCJoinStyle join_style)
		{
			DebugHelper.TraceWriteLine ("XSetLineAttributes");
			return _XSetLineAttributes(display, gc, line_width, line_style, cap_style, join_style);
		}

		[DllImport ("libX11", EntryPoint="XDrawLine")]
		internal extern static int _XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);
		internal static int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2)
		{
			DebugHelper.TraceWriteLine ("XDrawLine");
			return _XDrawLine(display, drawable, gc, x1, y1, x2, y2);
		}

		[DllImport ("libX11", EntryPoint="XDrawRectangle")]
		internal extern static int _XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);
		internal static int XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height)
		{
			DebugHelper.TraceWriteLine ("XDrawRectangle");
			return _XDrawRectangle(display, drawable, gc, x1, y1, width, height);
		}

		[DllImport ("libX11", EntryPoint="XFillRectangle")]
		internal extern static int _XFillRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);
		internal static int XFillRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height)
		{
			DebugHelper.TraceWriteLine ("XFillRectangle");
			return _XFillRectangle(display, drawable, gc, x1, y1, width, height);
		}

		[DllImport ("libX11", EntryPoint="XSetWindowBackground")]
		internal extern static int _XSetWindowBackground(IntPtr display, IntPtr window, IntPtr background);
		internal static int XSetWindowBackground(IntPtr display, IntPtr window, IntPtr background)
		{
			DebugHelper.TraceWriteLine ("XSetWindowBackground");
			return _XSetWindowBackground(display, window, background);
		}

		[DllImport ("libX11", EntryPoint="XCopyArea")]
		internal extern static int _XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);
		internal static int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y)
		{
			DebugHelper.TraceWriteLine ("XCopyArea");
			return _XCopyArea(display, src, dest, gc, src_x, src_y, width, height, dest_x, dest_y);
		}

		[DllImport ("libX11", EntryPoint="XGetWindowProperty")]
		internal extern static int _XGetWindowProperty(IntPtr display, IntPtr window, IntPtr atom, IntPtr long_offset, IntPtr long_length, bool delete, IntPtr req_type, out IntPtr actual_type, out int actual_format, out IntPtr nitems, out IntPtr bytes_after, ref IntPtr prop);
		internal static int XGetWindowProperty(IntPtr display, IntPtr window, IntPtr atom, IntPtr long_offset, IntPtr long_length, bool delete, IntPtr req_type, out IntPtr actual_type, out int actual_format, out IntPtr nitems, out IntPtr bytes_after, ref IntPtr prop)
		{
			DebugHelper.TraceWriteLine ("XGetWindowProperty");
			return _XGetWindowProperty(display, window, atom, long_offset, long_length, delete, req_type, out actual_type, out actual_format, out nitems, out bytes_after, ref prop);
		}

		[DllImport ("libX11", EntryPoint="XSetInputFocus")]
		internal extern static int _XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);
		internal static int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time)
		{
			DebugHelper.TraceWriteLine ("XSetInputFocus");
			return _XSetInputFocus(display, window, revert_to, time);
		}

		[DllImport ("libX11", EntryPoint="XIconifyWindow")]
		internal extern static int _XIconifyWindow(IntPtr display, IntPtr window, int screen_number);
		internal static int XIconifyWindow(IntPtr display, IntPtr window, int screen_number)
		{
			DebugHelper.TraceWriteLine ("XIconifyWindow");
			return _XIconifyWindow(display, window, screen_number);
		}

		[DllImport ("libX11", EntryPoint="XDefineCursor")]
		internal extern static int _XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);
		internal static int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor)
		{
			DebugHelper.TraceWriteLine ("XDefineCursor");
			return _XDefineCursor(display, window, cursor);
		}

		[DllImport ("libX11", EntryPoint="XUndefineCursor")]
		internal extern static int _XUndefineCursor(IntPtr display, IntPtr window);
		internal static int XUndefineCursor(IntPtr display, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XUndefineCursor");
			return _XUndefineCursor(display, window);
		}

		[DllImport ("libX11", EntryPoint="XFreeCursor")]
		internal extern static int _XFreeCursor(IntPtr display, IntPtr cursor);
		internal static int XFreeCursor(IntPtr display, IntPtr cursor)
		{
			DebugHelper.TraceWriteLine ("XFreeCursor");
			return _XFreeCursor(display, cursor);
		}

		[DllImport ("libX11", EntryPoint="XCreateFontCursor")]
		internal extern static IntPtr _XCreateFontCursor(IntPtr display, CursorFontShape shape);
		internal static IntPtr XCreateFontCursor(IntPtr display, CursorFontShape shape)
		{
			DebugHelper.TraceWriteLine ("XCreateFontCursor");
			return _XCreateFontCursor(display, shape);
		}

		[DllImport ("libX11", EntryPoint="XCreatePixmapCursor")]
		internal extern static IntPtr _XCreatePixmapCursor(IntPtr display, IntPtr source, IntPtr mask, ref XColor foreground_color, ref XColor background_color, int x_hot, int y_hot);
		internal static IntPtr XCreatePixmapCursor(IntPtr display, IntPtr source, IntPtr mask, ref XColor foreground_color, ref XColor background_color, int x_hot, int y_hot)
		{
			DebugHelper.TraceWriteLine ("XCreatePixmapCursor");
			return _XCreatePixmapCursor(display, source, mask, ref foreground_color, ref background_color, x_hot, y_hot);
		}

		[DllImport ("libX11", EntryPoint="XCreatePixmapFromBitmapData")]
		internal extern static IntPtr _XCreatePixmapFromBitmapData(IntPtr display, IntPtr drawable, byte[] data, int width, int height, IntPtr fg, IntPtr bg, int depth);
		internal static IntPtr XCreatePixmapFromBitmapData(IntPtr display, IntPtr drawable, byte[] data, int width, int height, IntPtr fg, IntPtr bg, int depth)
		{
			DebugHelper.TraceWriteLine ("XCreatePixmapFromBitmapData");
			return _XCreatePixmapFromBitmapData(display, drawable, data, width, height, fg, bg, depth);
		}

		[DllImport ("libX11", EntryPoint="XCreatePixmap")]
		internal extern static IntPtr _XCreatePixmap(IntPtr display, IntPtr d, int width, int height, int depth);
		internal static IntPtr XCreatePixmap(IntPtr display, IntPtr d, int width, int height, int depth)
		{
			DebugHelper.TraceWriteLine ("XCreatePixmap");
			return _XCreatePixmap(display, d, width, height, depth);
		}

		[DllImport ("libX11", EntryPoint="XFreePixmap")]
		internal extern static IntPtr _XFreePixmap(IntPtr display, IntPtr pixmap);
		internal static IntPtr XFreePixmap(IntPtr display, IntPtr pixmap)
		{
			DebugHelper.TraceWriteLine ("XFreePixmap");
			return _XFreePixmap(display, pixmap);
		}

		[DllImport ("libX11", EntryPoint="XQueryBestCursor")]
		internal extern static int _XQueryBestCursor(IntPtr display, IntPtr drawable, int width, int height, out int best_width, out int best_height);
		internal static int XQueryBestCursor(IntPtr display, IntPtr drawable, int width, int height, out int best_width, out int best_height)
		{
			DebugHelper.TraceWriteLine ("XQueryBestCursor");
			return _XQueryBestCursor(display, drawable, width, height, out best_width, out best_height);
		}

		[DllImport ("libX11", EntryPoint="XQueryExtension")]
		internal extern static int _XQueryExtension(IntPtr display, string extension_name, ref int major, ref int first_event, ref int first_error);
		internal static int XQueryExtension(IntPtr display, string extension_name, ref int major, ref int first_event, ref int first_error)
		{
			DebugHelper.TraceWriteLine ("XQueryExtension");
			return _XQueryExtension(display, extension_name, ref major, ref first_event, ref first_error);
		}

		[DllImport ("libX11", EntryPoint="XWhitePixel")]
		internal extern static IntPtr _XWhitePixel(IntPtr display, int screen_no);
		internal static IntPtr XWhitePixel(IntPtr display, int screen_no)
		{
			DebugHelper.TraceWriteLine ("XWhitePixel");
			return _XWhitePixel(display, screen_no);
		}

		[DllImport ("libX11", EntryPoint="XBlackPixel")]
		internal extern static IntPtr _XBlackPixel(IntPtr display, int screen_no);
		internal static IntPtr XBlackPixel(IntPtr display, int screen_no)
		{
			DebugHelper.TraceWriteLine ("XBlackPixel");
			return _XBlackPixel(display, screen_no);
		}

		[DllImport ("libX11", EntryPoint="XGrabServer")]
		internal extern static void _XGrabServer(IntPtr display);
		internal static void XGrabServer(IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XGrabServer");
			_XGrabServer(display);
		}

		[DllImport ("libX11", EntryPoint="XUngrabServer")]
		internal extern static void _XUngrabServer(IntPtr display);
		internal static void XUngrabServer(IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XUngrabServer");
			_XUngrabServer(display);
		}

		[DllImport ("libX11", EntryPoint="XGetWMNormalHints")]
		internal extern static void _XGetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints, out IntPtr supplied_return);
		internal static void XGetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints, out IntPtr supplied_return)
		{
			DebugHelper.TraceWriteLine ("XGetWMNormalHints");
			_XGetWMNormalHints(display, window, ref hints, out supplied_return);
		}

		[DllImport ("libX11", EntryPoint="XSetWMNormalHints")]
		internal extern static void _XSetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints);
		internal static void XSetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints)
		{
			DebugHelper.TraceWriteLine ("XSetWMNormalHints");
			_XSetWMNormalHints(display, window, ref hints);
		}

		[DllImport ("libX11", EntryPoint="XSetZoomHints")]
		internal extern static void _XSetZoomHints(IntPtr display, IntPtr window, ref XSizeHints hints);
		internal static void XSetZoomHints(IntPtr display, IntPtr window, ref XSizeHints hints)
		{
			DebugHelper.TraceWriteLine ("XSetZoomHints");
			_XSetZoomHints(display, window, ref hints);
		}

		[DllImport ("libX11", EntryPoint="XSetWMHints")]
		internal extern static void _XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmhints);
		internal static void XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmhints)
		{
			DebugHelper.TraceWriteLine ("XSetWMHints");
			_XSetWMHints(display, window, ref wmhints);
		}

		[DllImport ("libX11", EntryPoint="XGetIconSizes")]
		internal extern static int _XGetIconSizes(IntPtr display, IntPtr window, out IntPtr size_list, out int count);
		internal static int XGetIconSizes(IntPtr display, IntPtr window, out IntPtr size_list, out int count)
		{
			DebugHelper.TraceWriteLine ("XGetIconSizes");
			return _XGetIconSizes(display, window, out size_list, out count);
		}

		[DllImport ("libX11", EntryPoint="XSetErrorHandler")]
		internal extern static IntPtr _XSetErrorHandler(XErrorHandler error_handler);
		internal static IntPtr XSetErrorHandler(XErrorHandler error_handler)
		{
			DebugHelper.TraceWriteLine ("XSetErrorHandler");
			return _XSetErrorHandler(error_handler);
		}

		[DllImport ("libX11", EntryPoint="XGetErrorText")]
		internal extern static IntPtr _XGetErrorText(IntPtr display, byte code, StringBuilder buffer, int length);
		internal static IntPtr XGetErrorText(IntPtr display, byte code, StringBuilder buffer, int length)
		{
			DebugHelper.TraceWriteLine ("XGetErrorText");
			return _XGetErrorText(display, code, buffer, length);
		}

		[DllImport ("libX11", EntryPoint="XInitThreads")]
		internal extern static int _XInitThreads();
		internal static int XInitThreads()
		{
			DebugHelper.TraceWriteLine ("XInitThreads");
			return _XInitThreads();
		}

		[DllImport ("libX11", EntryPoint="XConvertSelection")]
		internal extern static int _XConvertSelection(IntPtr display, IntPtr selection, IntPtr target, IntPtr property, IntPtr requestor, IntPtr time);
		internal static int XConvertSelection(IntPtr display, IntPtr selection, IntPtr target, IntPtr property, IntPtr requestor, IntPtr time)
		{
			DebugHelper.TraceWriteLine ("XConvertSelection");
			return _XConvertSelection(display, selection, target, property, requestor, time);
		}

		[DllImport ("libX11", EntryPoint="XGetSelectionOwner")]
		internal extern static IntPtr _XGetSelectionOwner(IntPtr display, IntPtr selection);
		internal static IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection)
		{
			DebugHelper.TraceWriteLine ("XGetSelectionOwner");
			return _XGetSelectionOwner(display, selection);
		}

		[DllImport ("libX11", EntryPoint="XSetSelectionOwner")]
		internal extern static int _XSetSelectionOwner(IntPtr display, IntPtr selection, IntPtr owner, IntPtr time);
		internal static int XSetSelectionOwner(IntPtr display, IntPtr selection, IntPtr owner, IntPtr time)
		{
			DebugHelper.TraceWriteLine ("XSetSelectionOwner");
			return _XSetSelectionOwner(display, selection, owner, time);
		}

		[DllImport ("libX11", EntryPoint="XSetPlaneMask")]
		internal extern static int _XSetPlaneMask(IntPtr display, IntPtr gc, IntPtr mask);
		internal static int XSetPlaneMask(IntPtr display, IntPtr gc, IntPtr mask)
		{
			DebugHelper.TraceWriteLine ("XSetPlaneMask");
			return _XSetPlaneMask(display, gc, mask);
		}

		[DllImport ("libX11", EntryPoint="XSetForeground")]
		internal extern static int _XSetForeground(IntPtr display, IntPtr gc, UIntPtr foreground);
		internal static int XSetForeground(IntPtr display, IntPtr gc, UIntPtr foreground)
		{
			DebugHelper.TraceWriteLine ("XSetForeground");
			return _XSetForeground(display, gc, foreground);
		}

		[DllImport ("libX11", EntryPoint="XSetBackground")]
		internal extern static int _XSetBackground(IntPtr display, IntPtr gc, UIntPtr background);
		internal static int XSetBackground(IntPtr display, IntPtr gc, UIntPtr background)
		{
			DebugHelper.TraceWriteLine ("XSetBackground");
			return _XSetBackground(display, gc, background);
		}

		[DllImport ("libX11", EntryPoint="XBell")]
		internal extern static int _XBell(IntPtr display, int percent);
		internal static int XBell(IntPtr display, int percent)
		{
			DebugHelper.TraceWriteLine ("XBell");
			return _XBell(display, percent);
		}

		[DllImport ("libX11", EntryPoint="XChangeActivePointerGrab")]
		internal extern static int _XChangeActivePointerGrab (IntPtr display, EventMask event_mask, IntPtr cursor, IntPtr time);
		internal static int XChangeActivePointerGrab (IntPtr display, EventMask event_mask, IntPtr cursor, IntPtr time)
		{
			DebugHelper.TraceWriteLine ("XChangeActivePointerGrab");
			return _XChangeActivePointerGrab (display, event_mask, cursor, time);
		}

		[DllImport ("libX11", EntryPoint="XFilterEvent")]
		internal extern static bool _XFilterEvent(ref XEvent xevent, IntPtr window);
		internal static bool XFilterEvent(ref XEvent xevent, IntPtr window)
		{
			DebugHelper.TraceWriteLine ("XFilterEvent");
			return _XFilterEvent(ref xevent, window);
		}

		[DllImport ("libX11", EntryPoint="XkbSetDetectableAutoRepeat")]
		internal extern static void _XkbSetDetectableAutoRepeat (IntPtr display, bool detectable, IntPtr supported);
		internal static void XkbSetDetectableAutoRepeat (IntPtr display, bool detectable, IntPtr supported)
		{
			DebugHelper.TraceWriteLine ("XkbSetDetectableAutoRepeat");
			_XkbSetDetectableAutoRepeat (display, detectable, supported);
		}

		[DllImport ("libX11", EntryPoint="XPeekEvent")]
		internal extern static void _XPeekEvent (IntPtr display, ref XEvent xevent);
		internal static void XPeekEvent (IntPtr display, ref XEvent xevent)
		{
			DebugHelper.TraceWriteLine ("XPeekEvent");
			_XPeekEvent (display, ref xevent);
		}

		[DllImport ("libX11", EntryPoint="XIfEvent")]
		internal extern static void _XIfEvent (IntPtr display, ref XEvent xevent, Delegate event_predicate, IntPtr arg);
		internal static void XIfEvent (IntPtr display, ref XEvent xevent, Delegate event_predicate, IntPtr arg)
		{
			DebugHelper.TraceWriteLine ("XIfEvent");
			_XIfEvent (display, ref xevent, event_predicate, arg);
		}
#endregion

#region Shape extension imports
		[DllImport("libXext", EntryPoint="XShapeQueryExtension")]
		internal extern static bool _XShapeQueryExtension(IntPtr display, out int event_base, out int error_base);
		internal static bool XShapeQueryExtension(IntPtr display, out int event_base, out int error_base) {
			DebugHelper.TraceWriteLine (nameof(XShapeQueryExtension));
			return _XShapeQueryExtension(display, out event_base, out error_base);
		}

		[DllImport("libXext", EntryPoint="XShapeCombineRectangles")]
		internal extern static void _XShapeCombineRectangles(IntPtr display, IntPtr window, XShapeKind dest_kind, int x_off, int y_off, XRectangle[] rectangles, int n_rects, XShapeOperation op, XOrdering ordering);
		internal static void XShapeCombineRectangles(IntPtr display, IntPtr window, int dest_kind, int x_off, int y_off, XRectangle[] rectangles, int n_rects, int op, int ordering) {
			DebugHelper.TraceWriteLine (nameof(XShapeCombineRectangles));
			_XShapeCombineRectangles(display, window, dest_kind, x_off, y_off, rectangles, n_rects, op, ordering);
		}
#endregion

#region Xinerama imports
		[DllImport ("libXinerama", EntryPoint="XineramaQueryScreens")]
		extern static IntPtr _XineramaQueryScreens (IntPtr display, out int number);
		internal static IntPtr XineramaQueryScreens (IntPtr display, out int number)
		{
			DebugHelper.TraceWriteLine ("XineramaQueryScreens");
			return _XineramaQueryScreens (display, out number);
		}

		[DllImport ("libXinerama", EntryPoint="XineramaIsActive")]
		extern static bool _XineramaIsActive (IntPtr display);
		static bool XineramaNotInstalled;

		internal static bool XineramaIsActive (IntPtr display)
		{
			DebugHelper.TraceWriteLine ("XineramaIsActive");

			if (XineramaNotInstalled)
				return false;
			try {
				return _XineramaIsActive (display);
			} catch (DllNotFoundException) {
				// Xinerama isn't installed
				XineramaNotInstalled = true;
				return false;
			}
		}
#endregion

#else //no TRACE defined

#region Xcursor imports
		[DllImport ("libXcursor", EntryPoint = "XcursorLibraryLoadCursor")]
		internal extern static IntPtr XcursorLibraryLoadCursor (IntPtr display, [MarshalAs (UnmanagedType.LPStr)] string name);

		[DllImport ("libXcursor", EntryPoint = "XcursorLibraryLoadImages")]
		internal extern static IntPtr XcursorLibraryLoadImages ([MarshalAs (UnmanagedType.LPStr)] string file, IntPtr theme, int size);
		
		[DllImport ("libXcursor", EntryPoint = "XcursorImagesDestroy")]
		internal extern static void XcursorImagesDestroy (IntPtr images);
		
		[DllImport ("libXcursor", EntryPoint = "XcursorGetDefaultSize")]
		internal extern static int XcursorGetDefaultSize (IntPtr display);

		[DllImport ("libXcursor", EntryPoint = "XcursorImageLoadCursor")]
		internal extern static IntPtr XcursorImageLoadCursor (IntPtr display, IntPtr image);

		[DllImport ("libXcursor", EntryPoint = "XcursorGetTheme")]
		internal extern static IntPtr XcursorGetTheme (IntPtr display);
#endregion
		#region X11 Imports
		[DllImport ("libX11", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11", EntryPoint="XCloseDisplay")]
		internal extern static int XCloseDisplay(IntPtr display);						    
		[DllImport ("libX11", EntryPoint="XSynchronize")]
		internal extern static IntPtr XSynchronize(IntPtr display, bool onoff);

		[DllImport ("libX11", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, UIntPtr valuemask, ref XSetWindowAttributes attributes);
		
		[DllImport ("libX11", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, UIntPtr border, UIntPtr background);
		
		[DllImport ("libX11", EntryPoint="XMapWindow")]
		internal extern static int XMapWindow(IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XMapRaised")]
		internal extern static int XMapRaised(IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XUnmapWindow")]
		internal extern static int XUnmapWindow(IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XMapSubwindows")]
		internal extern static int XMapSubindows(IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XUnmapSubwindows")]
		internal extern static int XUnmapSubwindows(IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XRootWindow")]
		internal extern static IntPtr XRootWindow(IntPtr display, int screen_number);
		
		[DllImport ("libX11", EntryPoint="XNextEvent")]
		internal extern static IntPtr XNextEvent(IntPtr display, ref XEvent xevent);

		[DllImport ("libX11", EntryPoint="XConnectionNumber")]
		internal extern static int XConnectionNumber (IntPtr display);
		
		[DllImport ("libX11", EntryPoint="XPending")]
		internal extern static int XPending (IntPtr display);
		
		[DllImport ("libX11", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput(IntPtr display, IntPtr window, IntPtr mask);
		
		[DllImport ("libX11", EntryPoint="XDestroyWindow")]
		internal extern static int XDestroyWindow(IntPtr display, IntPtr window);
		
		[DllImport ("libX11", EntryPoint="XReparentWindow")]
		internal extern static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		
		[DllImport ("libX11", EntryPoint="XMoveResizeWindow")]
		extern static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);
		internal static int MoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height)
		{
			int ret = XMoveResizeWindow (display, window, x, y, width, height);
			Keyboard.MoveCurrentCaretPos ();
			return ret;
		}

		[DllImport ("libX11", EntryPoint="XResizeWindow")]
		internal extern static int XResizeWindow(IntPtr display, IntPtr window, int width, int height);

		[DllImport ("libX11", EntryPoint="XGetWindowAttributes")]
		internal extern static int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

		[DllImport ("libX11", EntryPoint="XFlush")]
		internal extern static int XFlush(IntPtr display);

		[DllImport ("libX11", EntryPoint="XSetWMName")]
		internal extern static int XSetWMName(IntPtr display, IntPtr window, ref XTextProperty text_prop);

		[DllImport ("libX11", EntryPoint="XStoreName")]
		internal extern static int XStoreName(IntPtr display, IntPtr window, string window_name);

		[DllImport ("libX11", EntryPoint="XFetchName")]
		internal extern static int XFetchName(IntPtr display, IntPtr window, ref IntPtr window_name);

		[DllImport ("libX11", EntryPoint="XSendEvent")]
		internal extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, IntPtr event_mask, ref XEvent send_event);

		[DllImport ("libX11", EntryPoint="XQueryTree")]
		internal extern static int XQueryTree(IntPtr display, IntPtr window, out IntPtr root_return, out IntPtr parent_return, out IntPtr children_return, out int nchildren_return);

		[DllImport ("libX11", EntryPoint="XFree")]
		internal extern static int XFree(IntPtr data);

		[DllImport ("libX11", EntryPoint="XRaiseWindow")]
		internal extern static int XRaiseWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XLowerWindow")]
		internal extern static uint XLowerWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XConfigureWindow")]
		internal extern static uint XConfigureWindow(IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values);

		[DllImport ("libX11", EntryPoint="XInternAtom")]
		internal extern static IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

		[DllImport ("libX11", EntryPoint="XInternAtoms")]
		internal extern static int XInternAtoms(IntPtr display, string[] atom_names, int atom_count, bool only_if_exists, IntPtr[] atoms);

		[DllImport ("libX11", EntryPoint="XSetWMProtocols")]
		internal extern static int XSetWMProtocols(IntPtr display, IntPtr window, IntPtr[] protocols, int count);

		[DllImport ("libX11", EntryPoint="XGrabPointer")]
		internal extern static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, IntPtr cursor, IntPtr timestamp);

		[DllImport ("libX11", EntryPoint="XUngrabPointer")]
		internal extern static int XUngrabPointer(IntPtr display, IntPtr timestamp);

		[DllImport ("libX11", EntryPoint="XQueryPointer")]
		internal extern static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

		[DllImport ("libX11", EntryPoint="XTranslateCoordinates")]
		internal extern static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return, out int dest_y_return, out IntPtr child_return);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, out IntPtr root, out int x, out int y, out int width, out int height, out int border_width, out int depth);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, out int width, out int height, IntPtr border_width, IntPtr depth);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, IntPtr width, IntPtr height, IntPtr border_width, IntPtr depth);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, IntPtr x, IntPtr y, out int width, out int height, IntPtr border_width, IntPtr depth);

		[DllImport ("libX11", EntryPoint="XWarpPointer")]
		internal extern static uint XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

		[DllImport ("libX11", EntryPoint="XClearWindow")]
		internal extern static int XClearWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XClearArea")]
		internal extern static int XClearArea(IntPtr display, IntPtr window, int x, int y, int width, int height, bool exposures);

		// Colormaps
		[DllImport ("libX11", EntryPoint="XDefaultScreenOfDisplay")]
		internal extern static IntPtr XDefaultScreenOfDisplay(IntPtr display);

		[DllImport ("libX11", EntryPoint="XScreenNumberOfScreen")]
		internal extern static int XScreenNumberOfScreen(IntPtr display, IntPtr Screen);

		[DllImport ("libX11", EntryPoint="XDefaultVisual")]
		internal extern static IntPtr XDefaultVisual(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefaultDepth")]
		internal extern static uint XDefaultDepth(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefaultScreen")]
		internal extern static int XDefaultScreen(IntPtr display);

		[DllImport ("libX11", EntryPoint="XDefaultColormap")]
		internal extern static IntPtr XDefaultColormap(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XLookupColor")]
		internal extern static int XLookupColor(IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);

		[DllImport ("libX11", EntryPoint="XAllocColor")]
		internal extern static int XAllocColor(IntPtr display, IntPtr Colormap, ref XColor colorcell_def);

		[DllImport ("libX11", EntryPoint="XSetTransientForHint")]
		internal extern static int XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref MotifWmHints data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref uint value, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref IntPtr value, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, uint[] data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, int[] data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr[] data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr atoms, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty", CharSet=CharSet.Ansi)]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, string text, int text_length);

		[DllImport ("libX11", EntryPoint="XDeleteProperty")]
		internal extern static int XDeleteProperty(IntPtr display, IntPtr window, IntPtr property);

		// Drawing
		[DllImport ("libX11", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC(IntPtr display, IntPtr window, IntPtr valuemask, ref XGCValues values);

		[DllImport ("libX11", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC(IntPtr display, IntPtr gc);

		[DllImport ("libX11", EntryPoint="XSetFunction")]
		internal extern static int XSetFunction(IntPtr display, IntPtr gc, GXFunction function);

		[DllImport ("libX11", EntryPoint="XSetLineAttributes")]
		internal extern static int XSetLineAttributes(IntPtr display, IntPtr gc, int line_width, GCLineStyle line_style, GCCapStyle cap_style, GCJoinStyle join_style);

		[DllImport ("libX11", EntryPoint="XDrawLine")]
		internal extern static int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);

		[DllImport ("libX11", EntryPoint="XDrawRectangle")]
		internal extern static int XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);

		[DllImport ("libX11", EntryPoint="XFillRectangle")]
		internal extern static int XFillRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);

		[DllImport ("libX11", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground(IntPtr display, IntPtr window, IntPtr background);

		[DllImport ("libX11", EntryPoint="XCopyArea")]
		internal extern static int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);

		[DllImport ("libX11", EntryPoint="XGetWindowProperty")]
		internal extern static int XGetWindowProperty(IntPtr display, IntPtr window, IntPtr atom, IntPtr long_offset, IntPtr long_length, bool delete, IntPtr req_type, out IntPtr actual_type, out int actual_format, out IntPtr nitems, out IntPtr bytes_after, ref IntPtr prop);

		[DllImport ("libX11", EntryPoint="XSetInputFocus")]
		internal extern static int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);

		[DllImport ("libX11", EntryPoint="XIconifyWindow")]
		internal extern static int XIconifyWindow(IntPtr display, IntPtr window, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefineCursor")]
		internal extern static int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);

		[DllImport ("libX11", EntryPoint="XUndefineCursor")]
		internal extern static int XUndefineCursor(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XFreeCursor")]
		internal extern static int XFreeCursor(IntPtr display, IntPtr cursor);

		[DllImport ("libX11", EntryPoint="XCreateFontCursor")]
		internal extern static IntPtr XCreateFontCursor(IntPtr display, CursorFontShape shape);

		[DllImport ("libX11", EntryPoint="XCreatePixmapCursor")]
		internal extern static IntPtr XCreatePixmapCursor(IntPtr display, IntPtr source, IntPtr mask, ref XColor foreground_color, ref XColor background_color, int x_hot, int y_hot);

		[DllImport ("libX11", EntryPoint="XCreatePixmapFromBitmapData")]
		internal extern static IntPtr XCreatePixmapFromBitmapData(IntPtr display, IntPtr drawable, byte[] data, int width, int height, IntPtr fg, IntPtr bg, int depth);

		[DllImport ("libX11", EntryPoint="XCreatePixmap")]
		internal extern static IntPtr XCreatePixmap(IntPtr display, IntPtr d, int width, int height, int depth);

		[DllImport ("libX11", EntryPoint="XFreePixmap")]
		internal extern static IntPtr XFreePixmap(IntPtr display, IntPtr pixmap);

		[DllImport ("libX11", EntryPoint="XQueryBestCursor")]
		internal extern static int XQueryBestCursor(IntPtr display, IntPtr drawable, int width, int height, out int best_width, out int best_height);

		[DllImport ("libX11", EntryPoint="XQueryExtension")]
		internal extern static int XQueryExtension(IntPtr display, string extension_name, ref int major, ref int first_event, ref int first_error);

		[DllImport ("libX11", EntryPoint="XWhitePixel")]
		internal extern static IntPtr XWhitePixel(IntPtr display, int screen_no);

		[DllImport ("libX11", EntryPoint="XBlackPixel")]
		internal extern static IntPtr XBlackPixel(IntPtr display, int screen_no);

		[DllImport ("libX11", EntryPoint="XGrabServer")]
		internal extern static void XGrabServer(IntPtr display);

		[DllImport ("libX11", EntryPoint="XUngrabServer")]
		internal extern static void XUngrabServer(IntPtr display);

		[DllImport ("libX11", EntryPoint="XGetWMNormalHints")]
		internal extern static void XGetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints, out IntPtr supplied_return);

		[DllImport ("libX11", EntryPoint="XSetWMNormalHints")]
		internal extern static void XSetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints);

		[DllImport ("libX11", EntryPoint="XSetZoomHints")]
		internal extern static void XSetZoomHints(IntPtr display, IntPtr window, ref XSizeHints hints);

		[DllImport ("libX11", EntryPoint="XSetWMHints")]
		internal extern static void XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmhints);

		[DllImport ("libX11", EntryPoint="XGetIconSizes")]
		internal extern static int XGetIconSizes(IntPtr display, IntPtr window, out IntPtr size_list, out int count);

		[DllImport ("libX11", EntryPoint="XSetErrorHandler")]
		internal extern static IntPtr XSetErrorHandler(XErrorHandler error_handler);

		[DllImport ("libX11", EntryPoint="XGetErrorText")]
		internal extern static IntPtr XGetErrorText(IntPtr display, byte code, StringBuilder buffer, int length);

		[DllImport ("libX11", EntryPoint="XInitThreads")]
		internal extern static int XInitThreads();

		[DllImport ("libX11", EntryPoint="XConvertSelection")]
		internal extern static int XConvertSelection(IntPtr display, IntPtr selection, IntPtr target, IntPtr property, IntPtr requestor, IntPtr time);

		[DllImport ("libX11", EntryPoint="XGetSelectionOwner")]
		internal extern static IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection);

		[DllImport ("libX11", EntryPoint="XSetSelectionOwner")]
		internal extern static int XSetSelectionOwner(IntPtr display, IntPtr selection, IntPtr owner, IntPtr time);

		[DllImport ("libX11", EntryPoint="XSetPlaneMask")]
		internal extern static int XSetPlaneMask(IntPtr display, IntPtr gc, IntPtr mask);

		[DllImport ("libX11", EntryPoint="XSetForeground")]
		internal extern static int XSetForeground(IntPtr display, IntPtr gc, UIntPtr foreground);

		[DllImport ("libX11", EntryPoint="XSetBackground")]
		internal extern static int XSetBackground(IntPtr display, IntPtr gc, UIntPtr background);

		[DllImport ("libX11", EntryPoint="XBell")]
		internal extern static int XBell(IntPtr display, int percent);

		[DllImport ("libX11", EntryPoint="XChangeActivePointerGrab")]
		internal extern static int XChangeActivePointerGrab (IntPtr display, EventMask event_mask, IntPtr cursor, IntPtr time);

		[DllImport ("libX11", EntryPoint="XFilterEvent")]
		internal extern static bool XFilterEvent(ref XEvent xevent, IntPtr window);

		[DllImport ("libX11", EntryPoint="XkbSetDetectableAutoRepeat")]
		internal extern static void XkbSetDetectableAutoRepeat (IntPtr display, bool detectable, IntPtr supported);

		[DllImport ("libX11", EntryPoint="XPeekEvent")]
		internal extern static void XPeekEvent (IntPtr display, ref XEvent xevent);

		[DllImport ("libX11", EntryPoint="XIfEvent")]
		internal extern static void XIfEvent (IntPtr display, ref XEvent xevent, Delegate event_predicate, IntPtr arg);

		[DllImport ("libX11", EntryPoint="XGetInputFocus")]
		internal extern static void XGetInputFocus (IntPtr display, out IntPtr focus, out IntPtr revert_to);
		#endregion
#region Gtk/Gdk imports
		[DllImport("libgdk-x11-2.0")]
		internal extern static IntPtr gdk_atom_intern (string atomName, bool onlyIfExists);

		[DllImport("libgtk-x11-2.0")]
		internal extern static IntPtr gtk_clipboard_get (IntPtr atom);

		[DllImport("libgtk-x11-2.0")]
		internal extern static void gtk_clipboard_store (IntPtr clipboard);

		[DllImport("libgtk-x11-2.0")]
		internal extern static void gtk_clipboard_set_text (IntPtr clipboard, string text, int len);
#endregion

#region Shape extension imports
		[DllImport("libXext")]
		internal extern static bool XShapeQueryExtension(IntPtr display, out int event_base, out int error_base);

		[DllImport("libXext")]
		internal extern static void XShapeCombineRectangles(IntPtr display, IntPtr window, XShapeKind dest_kind, int x_off, int y_off, XRectangle[] rectangles, int n_rects, XShapeOperation op, XOrdering ordering);
#endregion

#region Xinerama imports
		[DllImport ("libXinerama")]
		internal extern static IntPtr XineramaQueryScreens (IntPtr display, out int number);

		[DllImport ("libXinerama", EntryPoint = "XineramaIsActive")]
		extern static bool _XineramaIsActive (IntPtr display);
		static bool XineramaNotInstalled;

		internal static bool XineramaIsActive (IntPtr display)
		{
			if (XineramaNotInstalled)
				return false;
			try {
				return _XineramaIsActive (display);
			} catch (DllNotFoundException) {
				// Xinerama isn't installed
				XineramaNotInstalled = true;
				return false;
			}
		}
#endregion

#endif
	}
}
