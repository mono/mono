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
#define DriverDebug

// Extra detailed debug
#define	DriverDebugExtra

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Only do the poll when building with mono for now
#if __MonoCS__
using Mono.Unix;
#endif

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11 : XplatUIDriver {
		#region Local Variables
		// General
		private static XplatUIX11	Instance;
		private static int		RefCount;
		private static object		XlibLock;		// Our locking object
		private static bool		ThemesEnabled;

		// General X11
		private static IntPtr		DisplayHandle;		// X11 handle to display
		private static int		ScreenNo;		// Screen number used
		private static IntPtr		DefaultColormap;	// Colormap for screen
		private static IntPtr		RootWindow;		// Handle of the root window for the screen/display
		private static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		private static XErrorHandler	ErrorHandler;		// Error handler delegate
		private static bool		ErrorExceptions;	// Throw exceptions on X errors

		// Communication
		private static int		PostAtom;		// PostMessage atom
		private static int		AsyncAtom;		// Support for async messages

		// Message Loop
		private static bool		GetMessageResult;	// Value returned by GetMessage()
		private static XEventQueue	MessageQueue;		// Holds our queued up events
		#if __MonoCS__						//
		private static Pollfd[]		pollfds;		// For watching the X11 socket
		#endif							//
		private static X11Keyboard	Keyboard;		//
		private static Socket		listen;			//
		private static Socket		wake;			//


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
		private static MouseButtons	MouseState;		// Last state of mouse buttons

		// Timers
		private static ArrayList	TimerList;		// Holds SWF.Timers

		// 'Constants'
		private static int		DoubleClickInterval;	// msec; max interval between clicks to count as double click

		private static readonly EventMask  SelectInputMask =	EventMask.ButtonPressMask | 
									EventMask.ButtonReleaseMask | 
									EventMask.KeyPressMask | 
									EventMask.KeyReleaseMask | 
									EventMask.EnterWindowMask | 
									EventMask.LeaveWindowMask |
									EventMask.ExposureMask |
									EventMask.FocusChangeMask |
									EventMask.PointerMotionMask | 
									EventMask.VisibilityChangeMask |
									EventMask.SubstructureNotifyMask |
									EventMask.StructureNotifyMask;
		#endregion	// Local Variables

		#region Constructors
		private XplatUIX11() {
			// Handle singleton stuff first
			RefCount = 0;

			// Now regular initialization
			XlibLock = new object ();
			MessageQueue = new XEventQueue ();
			TimerList = new ArrayList ();

			ErrorExceptions = false;

			// X11 Initialization
			SetDisplay(XOpenDisplay(IntPtr.Zero));
		}

		~XplatUIX11() {
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}
		#endregion	// Constructors

		#region Singleton Specific Code
		public static XplatUIX11 GetInstance() {
			lock (typeof(XplatUIX11)) {
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

				sb = new StringBuilder(160);
				XGetErrorText(Display, ErrorCode, sb, sb.Capacity);
				x_error_text = sb.ToString();

				error = String.Format("\n  Error: {0}\n  Request:     {1:D} ({2})\n  Resource ID: 0x{3:x}\n  Serial:      {4}", x_error_text, RequestCode, RequestCode, ResourceID.ToInt32(), Serial);
				return error;
			}
		}
		#endregion	// XExceptionClass

		#region Internal Methods
		internal void SetDisplay(IntPtr display_handle) {
			if (display_handle != IntPtr.Zero) {
				if ((DisplayHandle != IntPtr.Zero) && (FosterParent != IntPtr.Zero)) {
					XDestroyWindow(DisplayHandle, FosterParent);
				}

				if (DisplayHandle != IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
				}

				DisplayHandle=display_handle;

				// We need to tell System.Drawing our DisplayHandle. FromHdcInternal has
				// been hacked to do this for us.
				Graphics.FromHdcInternal (DisplayHandle);

				// Debugging support
				if (Environment.GetEnvironmentVariable ("MONO_XSYNC") != null) {
					XSynchronize(DisplayHandle, true);
				}

				if (Environment.GetEnvironmentVariable ("MONO_XEXCEPTIONS") != null) {
					ErrorExceptions = true;
				}

				// Generic X11 setup
				ScreenNo = 0;
				RootWindow = XRootWindow(DisplayHandle, ScreenNo);
				DefaultColormap = XDefaultColormap(DisplayHandle, ScreenNo);

				// Create the foster parent
				FosterParent=XCreateSimpleWindow(DisplayHandle, RootWindow, 0, 0, 1, 1, 4, 0, 0);
				if (FosterParent==IntPtr.Zero) {
					Console.WriteLine("XplatUIX11 Constructor failed to create FosterParent");
				}


				// For sleeping on the X11 socket
				listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
				IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 0);
				listen.Bind(ep);
				listen.Listen(1);

				// To wake up when a timer is ready
				wake = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
				wake.Connect(listen.LocalEndPoint);

				#if __MonoCS__
				pollfds = new Pollfd [2];
				pollfds [0] = new Pollfd ();
				pollfds [0].fd = XConnectionNumber (DisplayHandle);
				pollfds [0].events = PollEvents.POLLIN;

				pollfds [1] = new Pollfd ();
				pollfds [1].fd = wake.Handle.ToInt32 ();
				pollfds [1].events = PollEvents.POLLIN;
				#endif

				Keyboard = new X11Keyboard(DisplayHandle);

				GetMessageResult = true;

				DoubleClickInterval = 500;

				HoverState.Interval = 500;
				HoverState.Timer = new Timer();
				HoverState.Timer.Enabled = false;
				HoverState.Timer.Interval = HoverState.Interval;
				HoverState.Timer.Tick +=new EventHandler(MouseHover);
				HoverState.X = -1;
				HoverState.Y = -1;

				ActiveWindow = IntPtr.Zero;
				FocusWindow = IntPtr.Zero;
				ModalWindows = new Stack(3);

				MouseState = MouseButtons.None;
				MousePosition = new Point(0, 0);

				Caret.Timer = new Timer();
				Caret.Timer.Interval = 500;		// FIXME - where should this number come from?
				Caret.Timer.Tick += new EventHandler(CaretCallback);

				SetupAtoms();

				// Grab atom changes off the root window to catch certain WM events
				XSelectInput(DisplayHandle, RootWindow, EventMask.PropertyChangeMask);

				// Handle any upcoming errors
				ErrorHandler = new XErrorHandler(HandleError);
				XSetErrorHandler(ErrorHandler);
			} else {
				throw new ArgumentNullException("Display", "Could not open display (X-Server required. Check you DISPLAY environment variable)");
			}
		}

		internal static void Where() {
			Console.WriteLine("Here: {0}\n", WhereString());
		}

		internal static string WhereString() {
			StackTrace	stack;
			StackFrame	frame;
			string		newline;
			string		unknown;
			StringBuilder	sb;
			MethodBase	method;

			newline = String.Format("{0}\t {1} ", Environment.NewLine, Locale.GetText("at"));
			unknown = Locale.GetText("<unknown method>");
			sb = new StringBuilder();
			stack = new StackTrace();

			for (int i = 0; i < stack.FrameCount; i++) {
				frame = stack.GetFrame(i);
				sb.Append(newline);

				method = frame.GetMethod();
				if (method != null) {
					#if not
						sb.AppendFormat(frame.ToString());
					#endif

					if (frame.GetFileLineNumber() != 0) {
						sb.AppendFormat("{0}.{1} () [{2}:{3}]", method.DeclaringType.FullName, method.Name, frame.GetFileName(), frame.GetFileLineNumber());
					} else {
						sb.AppendFormat("{0}.{1} ()", method.DeclaringType.FullName, method.Name);
					}
				} else { 
					sb.Append(unknown);
				}
			}
			return sb.ToString();
 		}
		#endregion	// Internal Methods

		#region Private Methods
		private static void SetupAtoms() {
			NetAtoms = new int[(int)NA.LAST_NET_ATOM];

			NetAtoms[(int)NA.WM_PROTOCOLS] = XInternAtom(DisplayHandle, "WM_PROTOCOLS", false);
			NetAtoms[(int)NA.WM_DELETE_WINDOW] = XInternAtom(DisplayHandle, "WM_DELETE_WINDOW", false);
			NetAtoms[(int)NA.WM_TAKE_FOCUS] = XInternAtom(DisplayHandle, "WM_TAKE_FOCUS", false);

			NetAtoms[(int)NA._NET_SUPPORTED] = XInternAtom(DisplayHandle, "_NET_SUPPORTED", false);
			NetAtoms[(int)NA._NET_CLIENT_LIST] = XInternAtom(DisplayHandle, "_NET_CLIENT_LIST", false);
			NetAtoms[(int)NA._NET_NUMBER_OF_DESKTOPS] = XInternAtom(DisplayHandle, "_NET_NUMBER_OF_DESKTOPS", false);
			NetAtoms[(int)NA._NET_DESKTOP_GEOMETRY] = XInternAtom(DisplayHandle, "_NET_DESKTOP_GEOMETRY", false);
			NetAtoms[(int)NA._NET_DESKTOP_VIEWPORT] = XInternAtom(DisplayHandle, "_NET_DESKTOP_VIEWPORT", false);
			NetAtoms[(int)NA._NET_CURRENT_DESKTOP] = XInternAtom(DisplayHandle, "_NET_CURRENT_DESKTOP", false);
			NetAtoms[(int)NA._NET_DESKTOP_NAMES] = XInternAtom(DisplayHandle, "_NET_DESKTOP_NAMES", false);
			NetAtoms[(int)NA._NET_ACTIVE_WINDOW] = XInternAtom(DisplayHandle, "_NET_ACTIVE_WINDOW", false);
			NetAtoms[(int)NA._NET_WORKAREA] = XInternAtom(DisplayHandle, "_NET_WORKAREA", false);
			NetAtoms[(int)NA._NET_SUPPORTING_WM_CHECK] = XInternAtom(DisplayHandle, "_NET_SUPPORTING_WM_CHECK", false);
			NetAtoms[(int)NA._NET_VIRTUAL_ROOTS] = XInternAtom(DisplayHandle, "_NET_VIRTUAL_ROOTS", false);
			NetAtoms[(int)NA._NET_DESKTOP_LAYOUT] = XInternAtom(DisplayHandle, "_NET_DESKTOP_LAYOUT", false);
			NetAtoms[(int)NA._NET_SHOWING_DESKTOP] = XInternAtom(DisplayHandle, "_NET_SHOWING_DESKTOP", false);

			NetAtoms[(int)NA._NET_CLOSE_WINDOW] = XInternAtom(DisplayHandle, "_NET_CLOSE_WINDOW", false);
			NetAtoms[(int)NA._NET_MOVERESIZE_WINDOW] = XInternAtom(DisplayHandle, "_NET_MOVERESIZE_WINDOW", false);
			NetAtoms[(int)NA._NET_WM_MOVERESIZE] = XInternAtom(DisplayHandle, "_NET_WM_MOVERESIZE", false);
			NetAtoms[(int)NA._NET_RESTACK_WINDOW] = XInternAtom(DisplayHandle, "_NET_RESTACK_WINDOW", false);
			NetAtoms[(int)NA._NET_REQUEST_FRAME_EXTENTS] = XInternAtom(DisplayHandle, "_NET_REQUEST_FRAME_EXTENTS", false);

			NetAtoms[(int)NA._NET_WM_NAME] = XInternAtom(DisplayHandle, "_NET_WM_NAME", false);
			NetAtoms[(int)NA._NET_WM_VISIBLE_NAME] = XInternAtom(DisplayHandle, "_NET_WM_VISIBLE_NAME", false);
			NetAtoms[(int)NA._NET_WM_ICON_NAME] = XInternAtom(DisplayHandle, "_NET_WM_ICON_NAME", false);
			NetAtoms[(int)NA._NET_WM_VISIBLE_ICON_NAME] = XInternAtom(DisplayHandle, "_NET_WM_VISIBLE_ICON_NAME", false);
			NetAtoms[(int)NA._NET_WM_DESKTOP] = XInternAtom(DisplayHandle, "_NET_WM_DESKTOP", false);
			NetAtoms[(int)NA._NET_WM_WINDOW_TYPE] = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE", false);
			NetAtoms[(int)NA._NET_WM_STATE] = XInternAtom(DisplayHandle, "_NET_WM_STATE", false);
			NetAtoms[(int)NA._NET_WM_ALLOWED_ACTIONS] = XInternAtom(DisplayHandle, "_NET_WM_ALLOWED_ACTIONS", false);
			NetAtoms[(int)NA._NET_WM_STRUT] = XInternAtom(DisplayHandle, "_NET_WM_STRUT", false);
			NetAtoms[(int)NA._NET_WM_STRUT_PARTIAL] = XInternAtom(DisplayHandle, "_NET_WM_STRUT_PARTIAL", false);
			NetAtoms[(int)NA._NET_WM_ICON_GEOMETRY] = XInternAtom(DisplayHandle, "_NET_WM_ICON_GEOMETRY", false);
			NetAtoms[(int)NA._NET_WM_ICON] = XInternAtom(DisplayHandle, "_NET_WM_ICON", false);
			NetAtoms[(int)NA._NET_WM_PID] = XInternAtom(DisplayHandle, "_NET_WM_PID", false);
			NetAtoms[(int)NA._NET_WM_HANDLED_ICONS] = XInternAtom(DisplayHandle, "_NET_WM_HANDLED_ICONS", false);
			NetAtoms[(int)NA._NET_WM_USER_TIME] = XInternAtom(DisplayHandle, "_NET_WM_USER_TIME", false);
			NetAtoms[(int)NA._NET_FRAME_EXTENTS] = XInternAtom(DisplayHandle, "_NET_FRAME_EXTENTS", false);

			NetAtoms[(int)NA._NET_WM_PING] = XInternAtom(DisplayHandle, "_NET_WM_PING", false);
			NetAtoms[(int)NA._NET_WM_SYNC_REQUEST] = XInternAtom(DisplayHandle, "_NET_WM_SYNC_REQUEST", false);

			NetAtoms[(int)NA._NET_SYSTEM_TRAY_S] = XInternAtom(DisplayHandle, "_NET_SYSTEM_TRAY_S" + ScreenNo.ToString(), false);
			NetAtoms[(int)NA._NET_SYSTEM_TRAY_OPCODE] = XInternAtom(DisplayHandle, "_NET_SYSTEM_TRAY_OPCODE", false);
			NetAtoms[(int)NA._NET_SYSTEM_TRAY_ORIENTATION] = XInternAtom(DisplayHandle, "_NET_SYSTEM_TRAY_ORIENTATION", false);

			NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_HORZ] = XInternAtom(DisplayHandle, "_NET_WM_STATE_MAXIMIZED_HORZ", false);
			NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_VERT] = XInternAtom(DisplayHandle, "_NET_WM_STATE_MAXIMIZED_VERT", false);

			NetAtoms[(int)NA._XEMBED] = XInternAtom(DisplayHandle, "_XEMBED", false);
			NetAtoms[(int)NA._XEMBED_INFO] = XInternAtom(DisplayHandle, "_XEMBED_INFO", false);

			NetAtoms[(int)NA._MOTIF_WM_HINTS] = XInternAtom(DisplayHandle, "_MOTIF_WM_HINTS", false);

			NetAtoms[(int)NA._NET_WM_STATE_NO_TASKBAR] = XInternAtom(DisplayHandle, "_NET_WM_STATE_NO_TASKBAR", false);
			NetAtoms[(int)NA._NET_WM_STATE_ABOVE] = XInternAtom(DisplayHandle, "_NET_WM_STATE_ABOVE", false);
			NetAtoms[(int)NA._NET_WM_STATE_MODAL] = XInternAtom(DisplayHandle, "_NET_WM_STATE_MODAL", false);
			NetAtoms[(int)NA._NET_WM_CONTEXT_HELP] = XInternAtom(DisplayHandle, "_NET_WM_CONTEXT_HELP", false);

			// Special Atoms
			AsyncAtom = XInternAtom(DisplayHandle, "_SWF_AsyncAtom", false);
			PostAtom = XInternAtom (DisplayHandle, "_SWF_PostMessageAtom", false);
			HoverState.Atom = XInternAtom(DisplayHandle, "_SWF_HoverAtom", false);
		}

		private void GetSystrayManagerWindow() {
			XGrabServer(DisplayHandle);
			SystrayMgrWindow = XGetSelectionOwner(DisplayHandle, (IntPtr)NetAtoms[(int)NA._NET_SYSTEM_TRAY_S]);
			XUngrabServer(DisplayHandle);
			XFlush(DisplayHandle);
		}

		private void SendNetWMMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2) {
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
			XSendEvent(DisplayHandle, RootWindow, false, EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask, ref xev);
		}

		private void SendNetClientMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2) {
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
			XSendEvent(DisplayHandle, window, false, EventMask.NoEventMask, ref xev);
		}

		private void SetHwndStyles(Hwnd hwnd, CreateParams cp) {
			if ((cp.Style & (int)WindowStyles.WS_CHILD) != 0) {
				if ((cp.Style & (int)WindowStyles.WS_THICKFRAME) != 0) {
					hwnd.BorderStyle = BorderStyle.Fixed3D;
				} else if ((cp.Style & (int)WindowStyles.WS_BORDER) != 0) {
					hwnd.BorderStyle = BorderStyle.FixedSingle;
				}
			}

			if ((cp.ExStyle & (int)WindowStyles.WS_EX_CLIENTEDGE) != 0) {
				hwnd.edge_style = Border3DStyle.Sunken;
			} else if ((cp.ExStyle & (int)WindowStyles.WS_EX_STATICEDGE) != 0) {
				hwnd.edge_style = Border3DStyle.Flat;
			} else if ((cp.ExStyle & (int)WindowStyles.WS_EX_WINDOWEDGE) != 0) {
				hwnd.edge_style = Border3DStyle.Raised;
			} else if ((cp.ExStyle & (int)WindowStyles.WS_EX_WINDOWEDGE) != 0) {
				hwnd.edge_style = Border3DStyle.Raised;
			}

			if ((cp.Style & (int)WindowStyles.WS_CAPTION) != 0) {
				if ((cp.ExStyle & (int)WindowStyles.WS_EX_TOOLWINDOW) != 0) {
					hwnd.title_style = TitleStyle.Tool;
				} else {
					hwnd.title_style = TitleStyle.Normal;
				}
			}
		}

		private void SetWMStyles(Hwnd hwnd, CreateParams cp) {
			MotifWmHints		mwmHints;
			MotifFunctions		functions;
			MotifDecorations	decorations;
			uint[]			atoms;
			int			atom_count;
			Rectangle		client_rect;

			mwmHints = new MotifWmHints();
			functions = 0;
			decorations = 0;

			mwmHints.flags = (IntPtr)(MotifFlags.Functions | MotifFlags.Decorations);
			mwmHints.functions = (IntPtr)0;
			mwmHints.decorations = (IntPtr)0;

			if ((cp.Style & (int)WindowStyles.WS_CAPTION) != 0) {
				functions |= MotifFunctions.Move;
				decorations |= MotifDecorations.Title | MotifDecorations.Menu;
			}

			if ((cp.Style & ((int)WindowStyles.WS_THICKFRAME)) != 0) {
				functions |= MotifFunctions.Move | MotifFunctions.Resize;
				decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
			}

			if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) {
				functions |= MotifFunctions.Minimize;
				decorations |= MotifDecorations.Minimize;
			}

			if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
				functions |= MotifFunctions.Maximize;
				decorations |= MotifDecorations.Maximize;
			}

			if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
				functions |= MotifFunctions.Close;
			}

			if ((cp.ExStyle & ((int)WindowStyles.WS_EX_DLGMODALFRAME)) != 0) {
				decorations |= MotifDecorations.Border;
			}

			if ((cp.Style & ((int)WindowStyles.WS_DLGFRAME)) != 0) {
				decorations |= MotifDecorations.Border;
			}

			if ((cp.Style & ((int)WindowStyles.WS_BORDER)) != 0) {
				decorations |= MotifDecorations.Border;
			}

			if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
				functions = 0;
				decorations = 0;
			}

			mwmHints.functions = (IntPtr)functions;
			mwmHints.decorations = (IntPtr)decorations;


			client_rect= hwnd.ClientRect;
			lock (XlibLock) {
				XChangeProperty(DisplayHandle, hwnd.whole_window, NetAtoms[(int)NA._MOTIF_WM_HINTS], NetAtoms[(int)NA._MOTIF_WM_HINTS], 32, PropertyMode.Replace, ref mwmHints, 5);

				if (((cp.Style & (int)WindowStyles.WS_POPUP) != 0)  && (hwnd.parent != null) && (hwnd.parent.whole_window != IntPtr.Zero)) {
					XSetTransientForHint(DisplayHandle, hwnd.whole_window, hwnd.parent.whole_window);
				}
				XMoveResizeWindow(DisplayHandle, hwnd.client_window, client_rect.X, client_rect.Y, client_rect.Width, client_rect.Height);

				atoms = new uint[8];
				atom_count = 0;

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					atoms[atom_count++] = (uint)NetAtoms[(int)NA._NET_WM_STATE_ABOVE];
					atoms[atom_count++] = (uint)NetAtoms[(int)NA._NET_WM_STATE_NO_TASKBAR];
				}

				XChangeProperty(DisplayHandle, hwnd.whole_window, NetAtoms[(int)NA._NET_WM_STATE], Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, atom_count);

				atom_count = 0;
				atoms[atom_count++] = (uint)NetAtoms[(int)NA.WM_DELETE_WINDOW];
				if ((cp.ExStyle & (int)WindowStyles.WS_EX_CONTEXTHELP) != 0) {
					atoms[atom_count++] = (uint)NetAtoms[(int)NA._NET_WM_CONTEXT_HELP];
				}

				XSetWMProtocols(DisplayHandle, hwnd.whole_window, atoms, atom_count);
			}
		}

		private void WakeupMain () {
			wake.BeginSend (new byte [] { 0xFF }, 0, 1, SocketFlags.None, null, null);
		}
		private void AddExpose (XEvent xevent) {
			Hwnd	hwnd;

			hwnd = Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);

			// Don't waste time
			if (hwnd == null || !hwnd.visible) {
				return;
			}

			if (xevent.AnyEvent.window == hwnd.client_window) {
				hwnd.AddInvalidArea(xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);

				if (!hwnd.expose_pending) {
					MessageQueue.Enqueue(xevent);
					hwnd.expose_pending = true;
				}
			} else {
				if (!hwnd.nc_expose_pending) {
					MessageQueue.Enqueue(xevent);
					hwnd.nc_expose_pending = true;
				}
			}
		}
		private void ShowCaret() {
			if ((Caret.gc == IntPtr.Zero) || Caret.On) {
				return;
			}
			Caret.On = true;

			lock (XlibLock) {
				XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}

		private void HideCaret() {
			if ((Caret.gc == IntPtr.Zero) || !Caret.On) {
				return;
			}
			Caret.On = false;

			lock (XlibLock) {
				XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}

		private int NextTimeout (DateTime now) {
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

		private void CheckTimers (DateTime now) {
			lock (TimerList) {
				int count;

				count = TimerList.Count;

				if (count == 0) {
					return;
				}

				for (int i = 0; i < TimerList.Count; i++) {
					Timer timer;

					timer = (Timer) TimerList[i];

					if (timer.Enabled && timer.Expires <= now) {
						timer.FireTick ();
						timer.Update (now);
					}
				}
			}
		}

		private void UpdateMessageQueue () {
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
					#if __MonoCS__
					Syscall.poll (pollfds, (uint) pollfds.Length, timeout);
					#endif
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
				
				switch (xevent.type) {
					case XEventName.Expose:
						AddExpose (xevent);
						break;

					case XEventName.KeyPress:
					case XEventName.KeyRelease:
					case XEventName.ButtonPress:
					case XEventName.ButtonRelease:
					case XEventName.MotionNotify:
					case XEventName.EnterNotify:
					case XEventName.LeaveNotify:
					case XEventName.CreateNotify:
					case XEventName.ConfigureNotify:
					case XEventName.DestroyNotify:
					case XEventName.FocusIn:
					case XEventName.FocusOut:
					case XEventName.ClientMessage:
						MessageQueue.Enqueue (xevent);
						break;

					case XEventName.PropertyNotify:
						if (xevent.PropertyEvent.atom == NetAtoms[(int)NA._NET_ACTIVE_WINDOW]) {
							Atom	actual_atom;
							int	actual_format;
							int	nitems;
							int	bytes_after;
							IntPtr	prop = IntPtr.Zero;
							IntPtr	prev_active;;

							prev_active = ActiveWindow;
							XGetWindowProperty(DisplayHandle, RootWindow, NetAtoms[(int)NA._NET_ACTIVE_WINDOW], 0, 1, false, Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
							if ((nitems > 0) && (prop != IntPtr.Zero)) {
								ActiveWindow = Hwnd.GetHandleFromWindow((IntPtr)Marshal.ReadInt32(prop));
								XFree(prop);

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
									// Modality handling, if we are modal and the new active window is one
									// of ours but not the modal one, switch back to the modal window

									if (NativeWindow.FindWindow(ActiveWindow) != null) {
										if (ActiveWindow != (IntPtr)ModalWindows.Peek()) {
											Activate((IntPtr)ModalWindows.Peek());
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

		private IntPtr GetMousewParam(int Delta) {
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
		private IntPtr XGetParent(IntPtr handle) {
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

		private int HandleError(IntPtr display, ref XErrorEvent error_event) {
			if (ErrorExceptions) {
				throw new XException(error_event.display, error_event.resourceid, error_event.serial, error_event.error_code, error_event.request_code, error_event.minor_code);
			} else {
				Console.WriteLine("X11 Error encountered: {0}{1}\n", XException.GetMessage(error_event.display, error_event.resourceid, error_event.serial, error_event.error_code, error_event.request_code, error_event.minor_code), WhereString());
			}
			return 0;
		}
		#endregion	// Private Methods

		#region	Callbacks
		private void MouseHover(object sender, EventArgs e) {
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

		private void CaretCallback(object sender, EventArgs e) {
			if (Caret.Paused) {
				return;
			}
			Caret.On = !Caret.On;

			XDrawLine(DisplayHandle, Caret.Hwnd, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
		}
		#endregion	// Callbacks

		#region Public Properties
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

		internal override  Size MinimizedWindowSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override  Size MinimizedWindowSpacingSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override  Size MinimumWindowSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override  Size MinWindowTrackSize {
			get {
				return new Size(1, 1);
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

				XGetWindowProperty(DisplayHandle, RootWindow, NetAtoms[(int)NA._NET_DESKTOP_GEOMETRY], 0, 256, false, Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				if ((nitems == 2) && (prop != IntPtr.Zero)) {
					width = Marshal.ReadInt32(prop, 0);
					height = Marshal.ReadInt32(prop, 4);

					XFree(prop);
					return new Rectangle(0, 0, width, height);
				} else {
					int	x;
					int	y;
					int	client_width;
					int	client_height;

					GetWindowPos(RootWindow, true, out x, out y, out width, out height, out client_width, out client_height);
					return new Rectangle(x, y, width, height);
				}
			}
		} 
		#endregion	// Public properties

		#region Public Static Methods
		internal override IntPtr InitializeDriver() {
			lock (this) {
				if (DisplayHandle==IntPtr.Zero) {
					SetDisplay(XOpenDisplay(IntPtr.Zero));
				}
			}
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}

		internal override void EnableThemes() {
			ThemesEnabled = true;
		}


		internal override void Activate(IntPtr handle) {
			lock (XlibLock) {
				SendNetWMMessage(Hwnd.ObjectFromHandle(handle).whole_window, (IntPtr)NetAtoms[(int)NA._NET_ACTIVE_WINDOW], IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
				//XRaiseWindow(DisplayHandle, handle);
			}
			return;
		}

		internal override void CaretVisible(IntPtr handle, bool visible) {
			// Visible is cumulative; two hides require two shows before the caret is visible again
			if (Caret.Hwnd == handle) {
				if (visible) {
					if (Caret.Visible < 1) {
						Caret.Visible++;
						Caret.On = false;
						if (Caret.Visible == 1) {
							ShowCaret();
							Caret.Timer.Start();
						}
					}
				} else {
					Caret.Visible--;
					if (Caret.Visible == 0) {
						Caret.Timer.Stop();
						HideCaret();
					}
				}
			}
		}

		internal override bool CalculateWindowRect(IntPtr handle, ref Rectangle ClientRect, int Style, int ExStyle, IntPtr MenuHandle, out Rectangle WindowRect) {
			BorderStyle	border_style;
			TitleStyle	title_style;

			title_style = TitleStyle.None;
			if ((Style & (int)WindowStyles.WS_CAPTION) != 0) {
				if ((ExStyle & (int)WindowStyles.WS_EX_TOOLWINDOW) != 0) {
					title_style = TitleStyle.Tool;
				} else {
					title_style = TitleStyle.Normal;
				}
			}

			border_style = BorderStyle.None;
			if ((Style & (int)WindowStyles.WS_CHILD) != 0) {
				if ((Style & (int)WindowStyles.WS_THICKFRAME) != 0) {
					border_style = BorderStyle.Fixed3D;
				} else if ((Style & (int)WindowStyles.WS_BORDER) != 0) {
					border_style = BorderStyle.FixedSingle;
				}
			}

			WindowRect = Hwnd.GetWindowRectangle(border_style, MenuHandle, title_style, ClientRect);

			return true;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
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

		internal override void CreateCaret(IntPtr handle, int width, int height) {
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
			Caret.Visible = 0;
			Caret.On = false;

			gc_values = new XGCValues();
			gc_values.line_width = width;

			Caret.gc = XCreateGC(DisplayHandle, Caret.Window, GCFunction.GCLineWidth, ref gc_values);
			if (Caret.gc == IntPtr.Zero) {
				Caret.Hwnd = IntPtr.Zero;
				return;
			}

			XSetFunction(DisplayHandle, Caret.gc, GXFunction.GXinvert);
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			XSetWindowAttributes	Attributes;
			Hwnd			hwnd;
			int			X;
			int			Y;
			int			Width;
			int			Height;
			IntPtr			ParentHandle;
			IntPtr			WholeWindow;
			IntPtr			ClientWindow;
			Rectangle		ClientRect;


			hwnd = new Hwnd();

			SetHwndStyles(hwnd, cp);

			Attributes = new XSetWindowAttributes();
			X = cp.X;
			Y = cp.Y;
			Width = cp.Width;
			Height = cp.Height;

			if (Width<1) Width=1;
			if (Height<1) Height=1;

			if (cp.Parent != IntPtr.Zero) {
				ParentHandle = Hwnd.ObjectFromHandle(cp.Parent).client_window;
			} else {
				if ((cp.Style & (int)WindowStyles.WS_CHILD) != 0) {
					// We need to use our foster parent window until this poor child gets it's parent assigned
					ParentHandle=FosterParent;
				} else if ((cp.Style & (int)WindowStyles.WS_POPUP) != 0) {
					ParentHandle=RootWindow;
				} else {
					// Default position on screen, if window manager doesn't place us somewhere else
					if (X<1) X = 50;
					if (Y<1) Y = 50;
					ParentHandle=RootWindow;
				}
			}

			// Save what's under the toolwindow
			if ((cp.ExStyle & (int)WindowStyles.WS_EX_TOOLWINDOW) != 0) {
				Attributes.save_under = true;
			}


			// If we're a popup without caption we override the WM
			if ((cp.Style & ((int)WindowStyles.WS_POPUP)) != 0) {
				if ((cp.Style & (int)WindowStyles.WS_CAPTION) == 0) {
					Attributes.override_redirect = true;
				}
			}
			
			Attributes.bit_gravity = Gravity.NorthWestGravity;
			Attributes.win_gravity = Gravity.NorthWestGravity;

			hwnd.x = X;
			hwnd.y = Y;
			hwnd.width = Width;
			hwnd.height = Height;
			hwnd.parent = Hwnd.ObjectFromHandle(cp.Parent);

			ClientRect = hwnd.ClientRect;
			ClientWindow = IntPtr.Zero;

			lock (XlibLock) {
				WholeWindow = XCreateWindow(DisplayHandle, ParentHandle, X, Y, Width, Height, 0, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, IntPtr.Zero, SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity | SetWindowValuemask.SaveUnder | SetWindowValuemask.OverrideRedirect, ref Attributes);
				if (WholeWindow != IntPtr.Zero) {
					ClientWindow = XCreateWindow(DisplayHandle, WholeWindow, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height, 0, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, IntPtr.Zero, SetWindowValuemask.Nothing, ref Attributes);
				}
			}

			if ((WholeWindow == IntPtr.Zero) || (ClientWindow == IntPtr.Zero)) {
				throw new Exception("Could not create X11 windows");
			}

			hwnd.WholeWindow = WholeWindow;
			hwnd.ClientWindow = ClientWindow;

			#if DriverDebug
				Console.WriteLine("Created window {0:X} / {1:X} parent {2:X}", ClientWindow.ToInt32(), WholeWindow.ToInt32(), hwnd.parent != null ? hwnd.parent.Handle.ToInt32() : 0);
			#endif
				       
			lock (XlibLock) {
				XSelectInput(DisplayHandle, hwnd.whole_window, SelectInputMask);
				XSelectInput(DisplayHandle, hwnd.client_window, SelectInputMask);

				if ((cp.Style & (int)WindowStyles.WS_VISIBLE) != 0) {
					XMapWindow(DisplayHandle, hwnd.whole_window);
					XMapWindow(DisplayHandle, hwnd.client_window);
					hwnd.visible = true;
				}
			}

			SetWMStyles(hwnd, cp);

			return hwnd.Handle;
		}

		internal override IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName=XplatUI.DefaultClassName;
			create_params.ClassStyle = 0;
			create_params.ExStyle=0;
			create_params.Parent=IntPtr.Zero;
			create_params.Param=0;

			return CreateWindow(create_params);
		}

		internal override IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
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
					} else if (!and && xor) {
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

		internal override IntPtr DefineStdCursor(StdCursor id) {
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
					shape = CursorFontShape.XC_hand2;
					break;
				}

				case StdCursor.Help: {
					shape = CursorFontShape.XC_question_arrow;
					break;
				}

				case StdCursor.HSplit: {
					shape = CursorFontShape.XC_sb_h_double_arrow;
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
					shape = CursorFontShape.XC_sizing; 
					break;
				}

				case StdCursor.SizeNS: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.SizeNWSE: {
					shape = CursorFontShape.XC_bottom_right_corner; 
					break;
				}

				case StdCursor.SizeWE: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.UpArrow: {
					shape = CursorFontShape.XC_center_ptr; 
					break;
				}

				case StdCursor.VSplit: {
					shape = CursorFontShape.XC_sb_v_double_arrow; 
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
				cursor = XCreateFontCursor(DisplayHandle, shape);
			}
			return cursor;
		}

		internal override IntPtr DefWndProc(ref Message msg) {
			switch((Msg)msg.Msg) {
				case Msg.WM_ERASEBKGND: {
					Hwnd	hwnd;

					hwnd = Hwnd.ObjectFromHandle(msg.HWnd);
					XClearArea(DisplayHandle, hwnd.client_window, hwnd.invalid.X, hwnd.invalid.Y, hwnd.invalid.Width, hwnd.invalid.Height, false);

					return IntPtr.Zero;
				}
			}
			return IntPtr.Zero;
		}

		internal override void DestroyCaret(IntPtr handle) {
			if (Caret.Hwnd == handle) {
				if (Caret.Visible == 1) {
					Caret.Timer.Stop();
					HideCaret();
				}
				if (Caret.gc != IntPtr.Zero) {
					XFreeGC(DisplayHandle, Caret.gc);
					Caret.gc = IntPtr.Zero;
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = 0;
				Caret.On = false;
			}
		}

		internal override void DestroyCursor(IntPtr cursor) {
			lock (XlibLock) {
				XFreeCursor(DisplayHandle, cursor);
			}
		}

		internal override void DestroyWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

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
				DestroyCaret(handle);
			}

			lock (XlibLock) {
				if (hwnd.client_window != IntPtr.Zero) {
					XDestroyWindow(DisplayHandle, hwnd.client_window);
				}

				if (hwnd.whole_window != IntPtr.Zero) {
					XDestroyWindow(DisplayHandle, hwnd.whole_window);
				}
			}

			hwnd.Dispose();
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		internal override void DoEvents() {
			MSG msg = new MSG ();
			while (PeekMessage(ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
				if (msg.message == Msg.WM_PAINT) {
					TranslateMessage (ref msg);
					DispatchMessage (ref msg);
				}
			}
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			// We do nothing; On X11 SetModal is used to create modal dialogs, on Win32 this function is used (see comment there)
		}

		internal override void Exit() {
			GetMessageResult = false;
		}

		internal override IntPtr GetActive() {
			Atom	actual_atom;
			int	actual_format;
			int	nitems;
			int	bytes_after;
			IntPtr	prop = IntPtr.Zero;
			IntPtr	active = IntPtr.Zero;

			XGetWindowProperty(DisplayHandle, RootWindow, NetAtoms[(int)NA._NET_ACTIVE_WINDOW], 0, 1, false, Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if ((nitems > 0) && (prop != IntPtr.Zero)) {
				active = (IntPtr)Marshal.ReadInt32(prop);
				XFree(prop);
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

		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			throw new NotImplementedException ();
		}

		internal override void GetDisplaySize(out Size size) {
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (XlibLock) {
				// FIXME - use _NET_WM messages instead?
				XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
			}

			size = new Size(attributes.width, attributes.height);
		}

		internal override IntPtr GetParent(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null && hwnd.parent != null) {
				return hwnd.parent.Handle;
			}
			return IntPtr.Zero;
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
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
				XQueryPointer(DisplayHandle, use_handle, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);
			}

			if (handle != IntPtr.Zero) {
				x = win_x;
				y = win_y;
			} else {
				x = root_x;
				y = root_y;
			}
		}

		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			return GetFontMetrics(g.GetHdc(), font.ToHfont(), out ascent, out descent);
		}

		internal override Graphics GetMenuDC(IntPtr hwnd, IntPtr ncpaint_region) {
			throw new NotImplementedException();
		}

		internal override bool GetMessage(ref MSG msg, IntPtr handle, int wFilterMin, int wFilterMax) {
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
					msg.hwnd= IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return true;
				}
			}

			// FIXME - handle filtering

			hwnd = Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);

			// Handle messages for windows that are already destroyed
			if (hwnd == null) {
				#if DriverDebug
					Console.WriteLine("GetMessage(): Got message for non-existent window {0:X}", xevent.AnyEvent.window.ToInt32());
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
			switch(xevent.type) {
				case XEventName.KeyPress: {
					Keyboard.KeyEvent (FocusWindow, xevent, ref msg);
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
							msg.message = client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							MouseState |= MouseButtons.Middle;
							msg.message = client ? Msg.WM_MBUTTONDOWN : Msg.WM_NCMBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							MouseState |= MouseButtons.Right;
							msg.message = client ? Msg.WM_RBUTTONDOWN : Msg.WM_NCRBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(120);
							break;
						}

						case 5: {
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(-120);
							break;
						}

					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;

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
						}
						ClickPending.Pending = false;
					}

					break;
				}

				case XEventName.ButtonRelease: {
					switch(xevent.ButtonEvent.button) {
						case 1: {
							MouseState &= ~MouseButtons.Left;
							msg.message = client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							MouseState &= ~MouseButtons.Middle;
							msg.message = client ? Msg.WM_MBUTTONUP : Msg.WM_NCMBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							MouseState &= ~MouseButtons.Right;
							msg.message = client ? Msg.WM_RBUTTONUP : Msg.WM_NCRBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							return true;
						}

						case 5: {
							return true;
						}
					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;
					break;
				}

				case XEventName.MotionNotify: {
					if (client) {
						NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);

						msg.message = Msg.WM_MOUSEMOVE;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x);

						HoverState.X = MousePosition.X = xevent.MotionEvent.x;
						HoverState.Y = MousePosition.Y = xevent.MotionEvent.y;

						break;
					} else {
						msg.message = Msg.WM_NCHITTEST;
						msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x);

						MousePosition.X = xevent.MotionEvent.x;
						MousePosition.Y = xevent.MotionEvent.y;
					}

					break;
				}

				case XEventName.EnterNotify: {
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						return true;
					}
					msg.message = Msg.WM_MOUSE_ENTER;
					HoverState.Timer.Enabled = true;
					HoverState.Window = xevent.CrossingEvent.window;
					break;
				}

				case XEventName.LeaveNotify: {
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						return true;
					}
					msg.message=Msg.WM_MOUSE_LEAVE;
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

				case XEventName.ConfigureNotify: {
					if (!client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {	// Ignore events for children (SubstructureNotify) and client areas
						Rectangle rect;

						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): Window {0:X} ConfigureNotify x={1} y={2} width={3} height={4}", hwnd.client_window.ToInt32(), xevent.ConfigureEvent.x, xevent.ConfigureEvent.y, xevent.ConfigureEvent.width, xevent.ConfigureEvent.height);
						#endif

						if ((hwnd.x != xevent.ConfigureEvent.x) || (hwnd.y != xevent.ConfigureEvent.y) ||
							(hwnd.width != xevent.ConfigureEvent.width) || (hwnd.height != xevent.ConfigureEvent.height)) {
							msg.message=Msg.WM_WINDOWPOSCHANGED;

							if (hwnd.parent != null) {
								hwnd.x = xevent.ConfigureEvent.x;
								hwnd.y = xevent.ConfigureEvent.y;
							} else {
								IntPtr	child;
								// We need to 'discount' the window the WM has put us in
								XTranslateCoordinates(DisplayHandle, XGetParent(hwnd.whole_window), RootWindow, xevent.ConfigureEvent.x, xevent.ConfigureEvent.y, out hwnd.x, out hwnd.y, out child);
							}
							hwnd.width = xevent.ConfigureEvent.width;
							hwnd.height = xevent.ConfigureEvent.height;
						} else {
							goto ProcessNextMessage;
						}

						// We need to adjust our client window to track the resize of whole_window
						rect = hwnd.ClientRect;
						XMoveResizeWindow(DisplayHandle, hwnd.client_window, rect.X, rect.Y, rect.Width, rect.Height);
					} else {
						goto ProcessNextMessage;
					}

					msg.lParam=IntPtr.Zero;		// FIXME - Generated LPWINDOWPOS structure and pass
					break;
				}

				case XEventName.FocusIn: {
					msg.message=Msg.WM_SETFOCUS;
					msg.wParam=IntPtr.Zero;
					break;
				}

				case XEventName.FocusOut: {
					msg.message=Msg.WM_KILLFOCUS;
					msg.wParam=IntPtr.Zero;
					break;
				}

				case XEventName.Expose: {
					#if DriverDebugExtra
						Console.WriteLine("GetMessage(): Window {0:X} Exposed area {1},{2} {3}x{4}", hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
					#endif
					if (Caret.Visible == 1) {
						Caret.Paused = true;
						HideCaret();
					}

					NativeWindow.WndProc(msg.hwnd, Msg.WM_ERASEBKGND, IntPtr.Zero, IntPtr.Zero);

					if (Caret.Visible == 1) {
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
					if (hwnd != null) {
						msg.hwnd = hwnd.client_window;
						msg.message=Msg.WM_DESTROY;
						hwnd.Dispose();

						#if DriverDebug
							Console.WriteLine("Got DestroyNotify on Window {0:X}", msg.hwnd.ToInt32());
						#endif
					} else {
						goto ProcessNextMessage;
					}

					break;
				}

				case XEventName.ClientMessage: {
					if (xevent.ClientMessageEvent.message_type == (IntPtr)AsyncAtom) {
						GCHandle		gchandle;
						AsyncMethodData		data;
						AsyncMethodResult	result;
						object			ret;

						gchandle = (GCHandle)xevent.ClientMessageEvent.ptr1;
						data = (AsyncMethodData)gchandle.Target;
						result = data.Result.Target as AsyncMethodResult;
						ret = data.Method.DynamicInvoke (data.Args);

						if (result != null) {
							result.Complete (ret);
						}
						gchandle.Free ();
						break;
					}

					if (xevent.ClientMessageEvent.message_type == (IntPtr)HoverState.Atom) {
						msg.message = Msg.WM_MOUSEHOVER;
						msg.wParam = GetMousewParam(0);
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

					if  (xevent.ClientMessageEvent.message_type == (IntPtr)NetAtoms[(int)NA.WM_PROTOCOLS]) {
						if (xevent.ClientMessageEvent.ptr1 == (IntPtr)NetAtoms[(int)NA.WM_DELETE_WINDOW]) {
							msg.message = Msg.WM_CLOSE;
							break;
						}

						// We should not get this, but I'll leave the code in case we need it in the future
						if (xevent.ClientMessageEvent.ptr1 == (IntPtr)NetAtoms[(int)NA.WM_TAKE_FOCUS]) {
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

			return GetMessageResult;
		}

		internal override bool GetText(IntPtr handle, out string text) {
			IntPtr	textptr;

			textptr = IntPtr.Zero;

			lock (XlibLock) {
				// FIXME - use _NET properties
				XFetchName(DisplayHandle, Hwnd.ObjectFromHandle(handle).whole_window, ref textptr);
			}
			if (textptr != IntPtr.Zero) {
				text = Marshal.PtrToStringAnsi(textptr);
				XFree(textptr);
				return true;
			} else {
				text = "";
				return false;
			}
		}

		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			Hwnd		hwnd;
			Rectangle	rect;

			hwnd = Hwnd.ObjectFromHandle(handle);
			rect = hwnd.ClientRect;

			x = hwnd.x;
			y = hwnd.y;
			width = hwnd.width;
			height = hwnd.height;

			client_width = rect.Width;
			client_height = rect.Height;
		}

		internal override FormWindowState GetWindowState(IntPtr handle) {
			Atom			actual_atom;
			int			actual_format;
			int			nitems;
			int			bytes_after;
			IntPtr			prop = IntPtr.Zero;
			IntPtr			atom;
			int			maximized;
			XWindowAttributes	attributes;
			Hwnd			hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			maximized = 0;
			XGetWindowProperty(DisplayHandle, hwnd.whole_window, NetAtoms[(int)NA._NET_WM_STATE], 0, 256, false, Atom.XA_ATOM, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if ((nitems > 0) && (prop != IntPtr.Zero)) {
				for (int i = 0; i < nitems; i++) {
					atom = (IntPtr)Marshal.ReadInt32(prop, i * 4);
					if ((atom == (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_HORZ]) || (atom == (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_VERT])) {
						maximized++;
					}
				}
				XFree(prop);
			}

			if (maximized == 2) {
				return FormWindowState.Maximized;
			}


			attributes = new XWindowAttributes();
			XGetWindowAttributes(DisplayHandle, handle, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped) {
				return FormWindowState.Minimized;
			}

			return FormWindowState.Normal;
		}

		internal override void GrabInfo(out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea) {
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}

		internal override void GrabWindow(IntPtr handle, IntPtr confine_to_handle) {
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
					EventMask.ButtonReleaseMask | EventMask.PointerMotionMask,
					GrabMode.GrabModeAsync, GrabMode.GrabModeAsync, confine_to_window, 0, 0);
			}
		}

		internal override void UngrabWindow(IntPtr hwnd) {
			lock (XlibLock) {
				XUngrabPointer(DisplayHandle, 0);
			}
			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			if (clear) {
				lock (XlibLock) {
					XClearArea (DisplayHandle, Hwnd.ObjectFromHandle(handle).client_window, rc.Left, rc.Top, rc.Width, rc.Height, true);
				}
			} else {
				XEvent xevent;

				xevent = new XEvent ();
				xevent.type = XEventName.Expose;
				xevent.ExposeEvent.display = DisplayHandle;
				xevent.ExposeEvent.window = Hwnd.ObjectFromHandle(handle).client_window;
				xevent.ExposeEvent.x = rc.X;
				xevent.ExposeEvent.y = rc.Y;
				xevent.ExposeEvent.width = rc.Width;
				xevent.ExposeEvent.height = rc.Height;

				AddExpose (xevent);
			}
		}

		internal override bool IsVisible(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).visible;
		}

		internal override void KillTimer(Timer timer) {
			lock (TimerList) {
				TimerList.Remove(timer);
			}
		}

		internal override void OverrideCursor(IntPtr cursor) {
			OverrideCursorHandle = cursor;
		}

		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			PaintEventArgs	paint_event;
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (Caret.Visible == 1) {
				Caret.Paused = true;
				HideCaret();
			}

			hwnd.client_dc  = Graphics.FromHwnd (hwnd.client_window);
			paint_event = new PaintEventArgs(hwnd.client_dc, hwnd.invalid);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			hwnd.ClearInvalidArea();

			hwnd.client_dc.Flush();
			hwnd.client_dc.Dispose();
			hwnd.client_dc = null;

			if (Caret.Visible == 1) {
				ShowCaret();
				Caret.Paused = false;
			}
		}

		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			bool	 pending;

			// FIXME - imlement filtering

			if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0) {		
				throw new NotImplementedException("PeekMessage PM_NOREMOVE is not implemented yet");	// FIXME - Implement PM_NOREMOVE flag
			}

			pending = false;
			if (MessageQueue.Count > 0) {
				pending = true;
			} else {
				// Only call UpdateMessageQueue if real events are pending 
				// otherwise we go to sleep on the socket
				if (XPending(DisplayHandle) != 0) {
					UpdateMessageQueue();
					pending = true;
				}
			}
			if (!pending) {
				return false;
			}
			return GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
		}

		internal static void PostMessage (IntPtr handle, Msg message, IntPtr wparam, IntPtr lparam) {
			XEvent xevent = new XEvent ();

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = Hwnd.ObjectFromHandle(handle).whole_window;
			xevent.ClientMessageEvent.message_type = (IntPtr) PostAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = handle;
			xevent.ClientMessageEvent.ptr2 = (IntPtr) message;
			xevent.ClientMessageEvent.ptr3 = wparam;
			xevent.ClientMessageEvent.ptr4 = lparam;

			MessageQueue.Enqueue (xevent);
		}

		internal override void ReleaseMenuDC(IntPtr hwnd, Graphics dc) {
			throw new NotImplementedException();
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y) {
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

		internal override void ScrollWindow(IntPtr handle, Rectangle area, int XAmount, int YAmount, bool with_children) {
			Hwnd		hwnd;
			IntPtr		gc;
			XGCValues	gc_values;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd.invalid != Rectangle.Empty) {
				// BIG FAT WARNING. This only works with how we use this function right now
				// where we basically still scroll the whole window, but work around areas
				// that are covered by our children

				hwnd.invalid.X += XAmount;
				hwnd.invalid.Y += YAmount;

				if (hwnd.invalid.X < 0) {
					hwnd.invalid.Width += hwnd.invalid.X;
					hwnd.invalid.X =0;
				}

				if (hwnd.invalid.Y < 0) {
					hwnd.invalid.Height += hwnd.invalid.Y;
					hwnd.invalid.Y =0;
				}
			}

			gc_values = new XGCValues();

			if (with_children) {
				gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			}

			gc = XCreateGC(DisplayHandle, hwnd.client_window, 0, ref gc_values);

			XCopyArea(DisplayHandle, hwnd.client_window, hwnd.client_window, gc, area.X - XAmount, area.Y - YAmount, area.Width, area.Height, area.X, area.Y);
			
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
			XFreeGC(DisplayHandle, gc);

			UpdateWindow(handle);
		}

		internal override void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children) {
			Hwnd	hwnd;

			hwnd = Hwnd.GetObjectFromWindow(handle);
	
			ScrollWindow(handle, hwnd.ClientRect, XAmount, YAmount, with_children);
		}

		internal override void SendAsyncMethod (AsyncMethodData method) {
			XEvent xevent = new XEvent ();

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = IntPtr.Zero;
			xevent.ClientMessageEvent.message_type = (IntPtr)AsyncAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) GCHandle.Alloc (method);

			MessageQueue.EnqueueLocked (xevent);

			WakeupMain ();
		}

		internal override void SetCaretPos(IntPtr handle, int x, int y) {
			if (Caret.Hwnd == handle) {
				Caret.Timer.Stop();
				HideCaret();

				Caret.X = x;
				Caret.Y = y;

				if (Caret.Visible == 1) {
					ShowCaret();
					Caret.Timer.Start();
				}
			}
		}

		internal override void SetCursor(IntPtr handle, IntPtr cursor) {
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
				}
				return;
			}

			hwnd = Hwnd.ObjectFromHandle(handle);
			lock (XlibLock) {
				XDefineCursor(DisplayHandle, hwnd.whole_window, OverrideCursorHandle);
			}
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			if (handle == IntPtr.Zero) {
				lock (XlibLock) {
					XWarpPointer(DisplayHandle, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, x, y);
				}
				return;
			} else {
				Hwnd	hwnd;

				hwnd = Hwnd.ObjectFromHandle(handle);
				lock (XlibLock) {
					XWarpPointer(DisplayHandle, IntPtr.Zero, hwnd.client_window, 0, 0, 0, 0, x, y);
				}
				return;
			}
		}

		internal override void SetFocus(IntPtr handle) {
			if (FocusWindow != IntPtr.Zero) {
				PostMessage(FocusWindow, Msg.WM_KILLFOCUS, handle, IntPtr.Zero);
			}
			PostMessage(handle, Msg.WM_SETFOCUS, FocusWindow, IntPtr.Zero);
			FocusWindow = handle;

			//XSetInputFocus(DisplayHandle, Hwnd.ObjectFromHandle(handle).client_window, RevertTo.None, IntPtr.Zero);
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
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
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.parent = Hwnd.ObjectFromHandle(parent);

			lock (XlibLock) {
				#if DriverDebug
					Console.WriteLine("Parent for window {0:X} / {1:X} = {2:X} (Handle:{3:X})", hwnd.ClientWindow.ToInt32(), hwnd.WholeWindow.ToInt32(), hwnd.parent != null ? hwnd.parent.Handle.ToInt32() : 0, parent.ToInt32());
				#endif
				XReparentWindow(DisplayHandle, hwnd.whole_window, hwnd.parent.client_window, hwnd.x, hwnd.y);
			}

			return IntPtr.Zero;
		}

		internal override void SetTimer (Timer timer) {
			lock (TimerList) {
				TimerList.Add(timer);
			}
			WakeupMain ();
		}

		internal override bool SetTopmost(IntPtr handle, IntPtr handle_owner, bool enabled) {
			Hwnd	hwnd;
			Hwnd	hwnd_owner;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (handle_owner != IntPtr.Zero) {
				hwnd_owner = Hwnd.ObjectFromHandle(handle_owner);
			} else {
				hwnd_owner = null;
			}

			if (enabled) {
				lock (XlibLock) {
					if (hwnd_owner != null) {
						XSetTransientForHint(DisplayHandle, hwnd.whole_window, hwnd_owner.whole_window);
					} else {
						XSetTransientForHint(DisplayHandle, hwnd.whole_window, FosterParent);
					}
				}
			} else {
				lock (XlibLock) {
					XDeleteProperty(DisplayHandle, hwnd.whole_window, (int)Atom.XA_WM_TRANSIENT_FOR);
				}
			}
			return true;
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.visible = visible;

			lock (XlibLock) {
				if (visible) {
					XMapWindow(DisplayHandle, hwnd.whole_window);
					XMapWindow(DisplayHandle, hwnd.client_window);
				} else {
					XUnmapWindow(DisplayHandle, hwnd.whole_window);
				}
			}
			return true;
		}

		internal override void SetWindowBackground(IntPtr handle, Color color) {
			XColor	xcolor;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			xcolor = new XColor();

			xcolor.red = (ushort)(color.R * 257);
			xcolor.green = (ushort)(color.G * 257);
			xcolor.blue = (ushort)(color.B * 257);

			lock (XlibLock) {
				XAllocColor(DisplayHandle, DefaultColormap, ref xcolor);
				XSetWindowBackground(DisplayHandle, hwnd.client_window, xcolor.pixel);
				XClearWindow(DisplayHandle, hwnd.client_window);
			}
		}

		internal override void SetBorderStyle(IntPtr handle, BorderStyle border_style) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.border_style = border_style;

			// FIXME - do we need to trigger some resize?
		}

		internal override void SetMenu(IntPtr handle, IntPtr menu_handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.menu_handle = menu_handle;

			// FIXME - do we need to trigger some resize?
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Hwnd		hwnd;
			Rectangle	client_rect;

			hwnd = Hwnd.ObjectFromHandle(handle);

			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && (hwnd.width == width) && (hwnd.height == height)) {
				return;
			}

			// X requires a sanity check for width & height; otherwise it dies
			if (width < 1) {
				width = 1;
			}

			if (height < 1) {
				height = 1;
			}

			client_rect = Hwnd.GetClientRectangle(hwnd.border_style, hwnd.menu_handle, hwnd.title_style, width, height);

			lock (XlibLock) {
				XMoveResizeWindow(DisplayHandle, hwnd.whole_window, x, y, width, height);
				XMoveResizeWindow(DisplayHandle, hwnd.client_window, client_rect.X, client_rect.Y, client_rect.Width, client_rect.Height);
			}
		}

		internal override void SetWindowState(IntPtr handle, FormWindowState state) {
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
							XMapWindow(DisplayHandle, hwnd.whole_window);
							XMapWindow(DisplayHandle, hwnd.client_window);
						} else if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage(hwnd.whole_window, (IntPtr)(uint)NetAtoms[(int)NA._NET_WM_STATE], (IntPtr)2 /* toggle */, (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_HORZ], (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_VERT]);
						}
					}
					Activate(handle);
					return;
				}

				case FormWindowState.Minimized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage(hwnd.whole_window, (IntPtr)NetAtoms[(int)NA._NET_WM_STATE], (IntPtr)2 /* toggle */, (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_HORZ], (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_VERT]);
						}
						XIconifyWindow(DisplayHandle, hwnd.whole_window, 0);
					}
					return;
				}

				case FormWindowState.Maximized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Minimized) {
							XMapWindow(DisplayHandle, hwnd.whole_window);
							XMapWindow(DisplayHandle, hwnd.client_window);
						}

						SendNetWMMessage(hwnd.whole_window, (IntPtr)NetAtoms[(int)NA._NET_WM_STATE], (IntPtr)1 /* Add */, (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_HORZ], (IntPtr)NetAtoms[(int)NA._NET_WM_STATE_MAXIMIZED_VERT]);
					}
					Activate(handle);
					return;
				}
			}
		}

		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			SetHwndStyles(hwnd, cp);
			SetWMStyles(hwnd, cp);
		}
		internal override bool SetZOrder(IntPtr handle, IntPtr after_handle, bool top, bool bottom) {
			Hwnd	hwnd;
			Hwnd	after_hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (after_handle != IntPtr.Zero) {
				after_hwnd = Hwnd.ObjectFromHandle(after_handle);
			} else {
				after_hwnd = null;
			}

			if (top) {
				lock (XlibLock) {
					XRaiseWindow(DisplayHandle, hwnd.whole_window);
				}
				return true;
			} else if (!bottom) {
				XWindowChanges	values = new XWindowChanges();

				if (after_hwnd == null) {
					throw new ArgumentNullException("after_handle", "Need sibling to adjust z-order");
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

		internal override void ShowCursor(bool show) {
			;	// FIXME - X11 doesn't 'hide' the cursor. we could create an empty cursor
		}

		internal override bool SystrayAdd(IntPtr handle, string tip, Icon icon, out ToolTip tt) {
			GetSystrayManagerWindow();

			if (SystrayMgrWindow != IntPtr.Zero) {
				uint[]		atoms;
				XSizeHints	size_hints;
				Hwnd		hwnd;

				hwnd = Hwnd.ObjectFromHandle(handle);

				size_hints = new XSizeHints();

				size_hints.flags = (IntPtr)(XSizeHintsFlags.PMinSize | XSizeHintsFlags.PMaxSize | XSizeHintsFlags.PBaseSize);
				size_hints.min_width = icon.Width;
				size_hints.min_height = icon.Height;

				size_hints.max_width = icon.Width;
				size_hints.max_height = icon.Height;

				size_hints.base_width = icon.Width;
				size_hints.base_height = icon.Height;
				XSetWMNormalHints(DisplayHandle, hwnd.whole_window, ref size_hints);

				atoms = new uint[2];
				atoms[0] = 1;	// Version 1
				atoms[1] = 1;	// We're not mapped

				// This line cost me 3 days...
				XChangeProperty(DisplayHandle, hwnd.whole_window, NetAtoms[(int)NA._XEMBED_INFO], NetAtoms[(int)NA._XEMBED_INFO], 32, PropertyMode.Replace, atoms, 2);

				// Need to pick some reasonable defaults
				tt = new ToolTip();
				tt.AutomaticDelay = 100;
				tt.InitialDelay = 250;
				tt.ReshowDelay = 250;
				tt.ShowAlways = true;

				if ((tip != null) && (tip != string.Empty)) {
					tt.SetToolTip(Control.FromHandle(handle), tip);
					tt.Active = true;
				} else {
					tt.Active = false;
				}

				// Make sure the window exists
				XSync(DisplayHandle, hwnd.whole_window);

				SendNetClientMessage(SystrayMgrWindow, (IntPtr)NetAtoms[(int)NA._NET_SYSTEM_TRAY_OPCODE], IntPtr.Zero, (IntPtr)SystrayRequest.SYSTEM_TRAY_REQUEST_DOCK, hwnd.whole_window);
				return true;
			}
			tt = null;
			return false;
		}

		internal override bool SystrayChange(IntPtr handle, string tip, Icon icon, ref ToolTip tt) {
			Control	control;

			control = Control.FromHandle(handle);
			if (control != null && tt != null) {
				tt.SetToolTip(control, tip);
				tt.Active = true;
				return true;
			} else {
				return false;
			}
		}

		internal override void SystrayRemove(IntPtr handle, ref ToolTip tt) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			XUnmapWindow(DisplayHandle, hwnd.whole_window);
			SetParent(hwnd.whole_window, FosterParent);

			// The caller can now re-dock it later...
			if (tt != null) {
				tt.Dispose();
				tt = null;
			}
		}

		internal override bool Text(IntPtr handle, string text) {
			lock (XlibLock) {
				// FIXME - use _NET properties
				XStoreName(DisplayHandle, Hwnd.ObjectFromHandle(handle).whole_window, text);
			}
			return true;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return Keyboard.TranslateMessage (ref msg);
		}

		internal override void UpdateWindow(IntPtr handle) {
			XEvent	xevent;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (!hwnd.visible || hwnd.expose_pending) {
				return;
			}

			xevent = new XEvent();
			xevent.type = XEventName.Expose;
			xevent.ExposeEvent.display = DisplayHandle;
			xevent.ExposeEvent.window = hwnd.client_window;

			MessageQueue.Enqueue(xevent);
			hwnd.expose_pending = true;
		}
		#endregion	// Public Static Methods

		#region Events
		internal override event EventHandler Idle;
		#endregion	// Events

		#region X11 Imports
		[DllImport ("libX11", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11", EntryPoint="XCloseDisplay")]
		internal extern static int XCloseDisplay(IntPtr display);						    
		[DllImport ("libX11", EntryPoint="XSynchronize")]
		internal extern static IntPtr XSynchronize(IntPtr display, bool onoff);

		[DllImport ("libX11", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, SetWindowValuemask valuemask, ref XSetWindowAttributes attributes);
		[DllImport ("libX11", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int border, int background);
		[DllImport ("libX11", EntryPoint="XMapWindow")]
		internal extern static int XMapWindow(IntPtr display, IntPtr window);
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
		[DllImport ("libX11")]
		internal extern static int XConnectionNumber (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static int XPending (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static bool XCheckWindowEvent (IntPtr display, IntPtr window, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11")]
		internal extern static bool XCheckMaskEvent (IntPtr display, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput(IntPtr display, IntPtr window, EventMask mask);

		[DllImport ("libX11", EntryPoint="XDestroyWindow")]
		internal extern static int XDestroyWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XReparentWindow")]
		internal extern static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		[DllImport ("libX11", EntryPoint="XMoveResizeWindow")]
		internal extern static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);

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
		internal extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, EventMask event_mask, ref XEvent send_event);

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
		internal extern static int XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

		[DllImport ("libX11", EntryPoint="XSetWMProtocols")]
		internal extern static int XSetWMProtocols(IntPtr display, IntPtr window, uint[] protocols, int count);

		[DllImport ("libX11", EntryPoint="XGrabPointer")]
		internal extern static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, uint cursor, uint timestamp);

		[DllImport ("libX11", EntryPoint="XUngrabPointer")]
		internal extern static int XUngrabPointer(IntPtr display, uint timestamp);

		[DllImport ("libX11", EntryPoint="XQueryPointer")]
		internal extern static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

		[DllImport ("libX11", EntryPoint="XTranslateCoordinates")]
		internal extern static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return,	 out int dest_y_return, out IntPtr child_return);

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

		[DllImport ("libX11", EntryPoint="XDefaultColormap")]
		internal extern static IntPtr XDefaultColormap(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XLookupColor")]
		internal extern static int XLookupColor(IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);

		[DllImport ("libX11", EntryPoint="XAllocColor")]
		internal extern static int XAllocColor(IntPtr display, IntPtr Colormap, ref XColor colorcell_def);

		[DllImport ("libX11", EntryPoint="XSetTransientForHint")]
		internal extern static int XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int type, int format, PropertyMode  mode, ref MotifWmHints data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, Atom format, int type, PropertyMode  mode, uint[] atoms, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, uint[] atoms, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, IntPtr atoms, int nelements);

		[DllImport ("libX11", EntryPoint="XDeleteProperty")]
		internal extern static int XDeleteProperty(IntPtr display, IntPtr window, int property);

		[DllImport ("gdiplus", EntryPoint="GetFontMetrics")]
		internal extern static bool GetFontMetrics(IntPtr graphicsObject, IntPtr nativeObject, out int ascent, out int descent);

		// Drawing
		[DllImport ("libX11", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC(IntPtr display, IntPtr window, GCFunction valuemask, ref XGCValues values);

		[DllImport ("libX11", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC(IntPtr display, IntPtr gc);

		[DllImport ("libX11", EntryPoint="XSetFunction")]
		internal extern static int XSetFunction(IntPtr display, IntPtr gc, GXFunction function);

		[DllImport ("libX11", EntryPoint="XDrawLine")]
		internal extern static int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);

		[DllImport ("libX11", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground(IntPtr display, IntPtr window, IntPtr background);

		[DllImport ("libX11", EntryPoint="XCopyArea")]
		internal extern static int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);

		[DllImport ("libX11", EntryPoint="XGetAtomName")]
		internal extern static string XGetAtomName(IntPtr display, int atom);

		[DllImport ("libX11", EntryPoint="XGetWindowProperty")]
		internal extern static int XGetWindowProperty(IntPtr display, IntPtr window, int atom, int long_offset, int long_length, bool delete, Atom req_type, out Atom actual_type, out int actual_format, out int nitems, out int bytes_after, ref IntPtr prop);

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

		[DllImport ("libX11", EntryPoint="XFreePixmap")]
		internal extern static IntPtr XFreePixmap(IntPtr display, IntPtr pixmap);

		[DllImport ("libX11", EntryPoint="XQueryBestCursor")]
		internal extern static int XQueryBestCursor(IntPtr display, IntPtr drawable, int width, int height, out int best_width, out int best_height);

		[DllImport ("libX11", EntryPoint="XWhitePixel")]
		internal extern static IntPtr XWhitePixel(IntPtr display, int screen_no);

		[DllImport ("libX11", EntryPoint="XBlackPixel")]
		internal extern static IntPtr XBlackPixel(IntPtr display, int screen_no);

		[DllImport ("libX11", EntryPoint="XGrabServer")]
		internal extern static void XGrabServer(IntPtr display);

		[DllImport ("libX11", EntryPoint="XUngrabServer")]
		internal extern static void XUngrabServer(IntPtr display);

		[DllImport ("libX11", EntryPoint="XGetSelectionOwner")]
		internal extern static IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection);

		[DllImport ("libX11", EntryPoint="XSetWMNormalHints")]
		internal extern static void XSetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints);

		[DllImport ("libX11", EntryPoint="XSetWMHints")]
		internal extern static void XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmhints);

		[DllImport ("libX11", EntryPoint="XSync")]
		internal extern static void XSync(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XGetIconSizes")]
		internal extern static int XGetIconSizes(IntPtr display, IntPtr window, out IntPtr size_list, out int count);

		[DllImport ("libX11", EntryPoint="XSetErrorHandler")]
		internal extern static IntPtr XSetErrorHandler(XErrorHandler error_handler);

		[DllImport ("libX11", EntryPoint="XGetErrorText")]
		internal extern static IntPtr XGetErrorText(IntPtr display, byte code, StringBuilder buffer, int length);
		#endregion
	}
}
