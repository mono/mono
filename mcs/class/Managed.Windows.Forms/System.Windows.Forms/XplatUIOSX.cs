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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@customerdna.com>
//
//

// This really doesn't work at all; please dont file bugs on it yet.

// MAJOR TODO:
//  Fix clipping of children
//  Wire up keyboard

using System;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// OSX Version
namespace System.Windows.Forms {

	// The Carbon Event callback delegate
	delegate int CarbonEventDelegate (IntPtr inCallRef, IntPtr inEvent, IntPtr userData);

	internal class XplatUIOSX : XplatUIDriver {
		
		#region Local Variables
		
		// General driver variables
		private static XplatUIOSX Instance;
		private static int RefCount;
		private static bool ThemesEnabled;
		private static IntPtr FocusWindow;

		// Mouse 
		private static MouseButtons MouseState;
		private static Point MousePosition;
		private static Hwnd MouseWindow;
		
		// OSX Specific
		private static GrabStruct Grab;
		private static OSXCaret Caret;
		private static OSXHover Hover;
		private CarbonEventDelegate CarbonEventHandler;
		private static Hashtable WindowMapping;
		private static Hashtable WindowBackgrounds;
		private static Hwnd GrabWindowHwnd;
		private static IntPtr FosterParent;
		private static int TitleBarHeight;
		private static int MenuBarHeight;
		private static EventTypeSpec [] viewEvents = new EventTypeSpec [] {
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlSetFocusPart), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlClick), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlContextualMenuClick), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlTrack), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlSimulateHit), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlBoundsChanged), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlDraw) 
									};
		private static EventTypeSpec [] windowEvents = new EventTypeSpec[] {
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseEntered),
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseExited),
									new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseMoved),
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseDragged),
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseWheelMoved),
									new EventTypeSpec (OSXConstants.kEventClassWindow, OSXConstants.kEventWindowBoundsChanged),
									new EventTypeSpec (OSXConstants.kEventClassWindow, OSXConstants.kEventWindowClose),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyDown),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyRepeat),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyUp)
									};
									
		
		// Message loop
		private static Queue MessageQueue;
		private static bool GetMessageResult;

		// Timers
		private ArrayList TimerList;
		
		// Event Handlers
		internal override event EventHandler Idle;

		#endregion
		
		#region Constructors
		private XplatUIOSX() {

			RefCount = 0;
			TimerList = new ArrayList ();
			MessageQueue = new Queue ();
			
			Initialize ();
		}

		~XplatUIOSX() {
			// FIXME: Clean up the FosterParent here.
		}

		#endregion

		#region Singleton specific code
		
		public static XplatUIOSX GetInstance() {
			lock (typeof (XplatUIOSX)) {
				if (Instance == null) {
					Instance = new XplatUIOSX ();
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
		
		#region Internal methods
		
		internal void Initialize () {

			// Initialize the Event Handler delegate
			CarbonEventHandler = new CarbonEventDelegate (EventCallback);
			
			// Initilize the mouse controls
			Hover.Interval = 500;
			Hover.Timer = new Timer ();
			Hover.Timer.Enabled = false;
			Hover.Timer.Interval = Hover.Interval;
			Hover.Timer.Tick += new EventHandler (HoverCallback);
			Hover.X = -1;
			Hover.Y = -1;
			MouseState = MouseButtons.None;
			MousePosition = Point.Empty;
				
			// Initialize the Caret
			Caret.Timer = new Timer ();
			Caret.Timer.Interval = 500;
			Caret.Timer.Tick += new EventHandler (CaretCallback);
			
			// Initialize the OSX Specific stuff
			WindowMapping = new Hashtable ();
			WindowBackgrounds = new Hashtable ();
			
			// Initialize the FosterParent
			IntPtr rect = IntPtr.Zero;
			SetRect (ref rect, (short)0, (short)0, (short)0, (short)0);
			CheckError (CreateNewWindow (WindowClass.kDocumentWindowClass, WindowAttributes.kWindowStandardHandlerAttribute | WindowAttributes.kWindowCloseBoxAttribute | WindowAttributes.kWindowFullZoomAttribute | WindowAttributes.kWindowCollapseBoxAttribute | WindowAttributes.kWindowResizableAttribute | WindowAttributes.kWindowCompositingAttribute, ref rect, ref FosterParent), "CreateFosterParent ()");
			
			// Get some values about bar heights
			Rect structRect = new Rect ();
			Rect contentRect = new Rect ();
			CheckError (GetWindowBounds (FosterParent, 32, ref structRect), "GetWindowBounds ()");
			CheckError (GetWindowBounds (FosterParent, 33, ref contentRect), "GetWindowBounds ()");
			
			TitleBarHeight = Math.Abs(structRect.top - contentRect.top);
			MenuBarHeight = GetMBarHeight ();
			
			// Focus
			FocusWindow = IntPtr.Zero;
			
			// Message loop
			GetMessageResult = true;
		}
		
		#endregion
		
		#region Private methods
		#endregion
		
		#region Callbacks
		
		private void CaretCallback (object sender, EventArgs e) {
			if (Caret.Paused) {
				return;
			}

			if (!Caret.On) {
				ShowCaret ();
			} else {
				HideCaret ();
			}
		}
		
		private void HoverCallback (object sender, EventArgs e) {
			if ((Hover.X == MousePosition.X) && (Hover.Y == MousePosition.Y)) {
				MSG msg = new MSG ();
				msg.hwnd = Hover.Hwnd;
				msg.message = Msg.WM_MOUSEHOVER;
				msg.wParam = GetMousewParam (0);
				msg.lParam = (IntPtr)((ushort)Hover.X << 16 | (ushort)Hover.X);
				MessageQueue.Enqueue (msg);
			}
		}
		
		internal int EventCallback (IntPtr inCallRef, IntPtr inEvent, IntPtr handle) {
			uint eventClass = GetEventClass (inEvent);
			uint eventKind = GetEventKind (inEvent);
			int retVal = 0;
			lock (MessageQueue) {
				switch (eventClass) {
					// keyboard
					case OSXConstants.kEventClassKeyboard: {
						retVal = ProcessKeyboardEvent (inEvent, eventKind, handle);
						break;
					}
					//window
					case OSXConstants.kEventClassWindow: {
						retVal = ProcessWindowEvent (inEvent, eventKind, handle);
						break;
					}
					// mouse
					case OSXConstants.kEventClassMouse: {
						retVal = ProcessMouseEvent (inEvent, eventKind, handle);
						break;
					}
					// control
					case OSXConstants.kEventClassControl: {
						retVal = ProcessControlEvent (inEvent, eventKind, handle);
						break;
					}
					default: {
						Console.WriteLine ("WARNING: Unhandled eventClass {0}", eventClass);
						break;
					}
				}
			}
			
			return retVal;
		}

		#endregion
		
		#region Private Methods
		
		// This sucks write a real driver
		private int ProcessKeyboardEvent (IntPtr inEvent, uint eventKind, IntPtr handle) {
			MSG msg = new MSG ();
			byte charCode = 0x00;
			GetEventParameter (inEvent, 1801676914, 1413830740, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref charCode);
			IntPtr cntrl = IntPtr.Zero;
			CheckError (GetKeyboardFocus (handle, ref cntrl), "GetKeyboardFocus()");
			msg.hwnd = cntrl;
			msg.lParam = IntPtr.Zero;
			switch (charCode) {
				case 28:
					charCode = 0x25;
					break;
				case 29:
					charCode = 0x27;
					break;
				case 30:
					charCode = 0x26;
					break;
				case 31:
					charCode = 0x28;
					break;
			}
			msg.wParam = (IntPtr)charCode;
			switch (eventKind) {
				// keydown
				case OSXConstants.kEventRawKeyDown: {
					msg.message = Msg.WM_KEYDOWN;
					break;
				}
				// repeat
				case OSXConstants.kEventRawKeyRepeat: {
					msg.message = Msg.WM_KEYDOWN;
					break;
				}
				// keyup
				case OSXConstants.kEventRawKeyUp: {
					msg.message = Msg.WM_KEYUP;
					break;
				}
			}
			MessageQueue.Enqueue (msg);
			return -9874;
		}

		private int ProcessWindowEvent (IntPtr inEvent, uint eventKind, IntPtr handle) {
			MSG msg = new MSG ();
			switch (eventKind) {
				// Someone closed a window
				case OSXConstants.kEventWindowClose: {
					// This is our real window; so we have to post to the corresponding view
					// FIXME: Should we doublehash the table to get the real window handle without this loop?
					IDictionaryEnumerator e = WindowMapping.GetEnumerator ();
					while (e.MoveNext ()) {
						if ((IntPtr)e.Value == handle) {
							NativeWindow.WndProc((IntPtr)e.Key, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
						}
					}
					return 0;
				}
				case OSXConstants.kEventWindowBoundsChanged: {
					// This is our real window; so we have to resize the corresponding view as well
					// FIXME: Should we doublehash the table to get the real window handle without this loop?
					
					IDictionaryEnumerator e = WindowMapping.GetEnumerator ();
					while (e.MoveNext ()) {
						if ((IntPtr)e.Value == handle) {
							Hwnd hwnd = Hwnd.ObjectFromHandle ((IntPtr) e.Key);
							// Get the bounds of the window
							Rect bounds = new Rect ();
							CheckError (GetWindowBounds (handle, 33, ref bounds), "GetWindowBounds ()");
							HIRect r = new HIRect ();
							
							// Get our frame for the Handle
							CheckError (HIViewGetFrame (hwnd.Handle, ref r), "HIViewGetFrame ()");
							r.size.width = bounds.right-bounds.left;
							r.size.height = bounds.bottom-bounds.top;
							// Set the view to the new size
         					CheckError (HIViewSetFrame (hwnd.WholeWindow, ref r), "HIViewSetFrame ()");
         					
         					// Update the hwnd internal size representation
							hwnd.x = (int)r.origin.x;
							hwnd.y = (int)r.origin.y;
							hwnd.width = (int)r.size.width;
							hwnd.height = (int)r.size.height;
							Rectangle client_rect = hwnd.ClientRect;
							
							r.size.width = client_rect.Width;
							r.size.height = client_rect.Height;
							r.origin.x = client_rect.X;
							r.origin.y = client_rect.Y;
							
							// Update the client area too
							CheckError (HIViewSetFrame (hwnd.ClientWindow, ref r));
							
							// Add the message to the queue
							msg.message = Msg.WM_WINDOWPOSCHANGED;
							msg.hwnd = hwnd.Handle;
							msg.wParam = IntPtr.Zero;
							msg.lParam = IntPtr.Zero;
							MessageQueue.Enqueue (msg);
							
							return 0;
						}
					}
					break;
				}
			}
			return -9874;
		}
				
		private int ProcessMouseEvent (IntPtr inEvent, uint eventKind, IntPtr handle) {
			MSG msg = new MSG ();		
			
			switch (eventKind) {
				case OSXConstants.kEventMouseMoved: {
					// Where is the mouse in global coordinates
					QDPoint pt = new QDPoint ();
					GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref pt);
					
					// Where is the mouse in the window
					Rect window_bounds = new Rect ();
					GetWindowBounds (handle, 33, ref window_bounds);
					CGPoint window_pt = new CGPoint ((short) (pt.x - window_bounds.left), (short) (pt.y - window_bounds.top));
					
					IntPtr window_handle = IntPtr.Zero;
					HIViewFindByID (HIViewGetRoot (handle), new HIViewID (OSXConstants.kEventClassWindow, 1), ref window_handle);
					
					// Determine which control was hit
					IntPtr view_handle = IntPtr.Zero;
					HIViewGetSubviewHit (window_handle, ref window_pt, true, ref view_handle);
					
					// Convert the point to view local coordinates
					HIViewConvertPoint (ref window_pt, window_handle, view_handle);
					
					Hwnd hwnd = Hwnd.ObjectFromHandle (view_handle);
					
					if (hwnd == null)
						return -9874;
						
					// Generate the message
					msg.hwnd = hwnd.Handle;
					msg.message = Msg.WM_MOUSEMOVE;
					msg.lParam = (IntPtr) ((ushort)window_pt.y << 16 | (ushort)window_pt.x);
					msg.wParam = GetMousewParam (0);
					MousePosition.X = (int)window_pt.x;
					MousePosition.Y = (int)window_pt.y;
					
					Hover.Hwnd = msg.hwnd;
					Hover.Timer.Enabled = true;
					MessageQueue.Enqueue (msg);
					return -9874;
				}
			}
			return -9874;
		}
					
		private int ProcessControlEvent (IntPtr inEvent, uint eventKind, IntPtr handle) {
			GetEventParameter (inEvent, 757935405, 1668575852, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref handle);
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			MSG msg = new MSG ();
					
			switch (eventKind) {
				case OSXConstants.kEventControlDraw: {
					
					if(!hwnd.visible)
						return 0;
					
					/*
					IntPtr rgnhandle = IntPtr.Zero;
					GetEventParameter (inEvent, 1919381096, 1919381096, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref rgnhandle);
					IntPtr duprgn = NewRgn ();
					CopyRgn (rgnhandle, duprgn);
					ClipRegions [hwnd.Handle] = duprgn;		
					*/
					
					// Get the dirty area
					HIRect bounds = new HIRect ();
					HIViewGetBounds (handle, ref bounds); 
					
					bool client = (hwnd.ClientWindow == handle ? true : false);
					
					if (!client && bounds.origin.x >= hwnd.ClientRect.X && bounds.origin.y >= hwnd.ClientRect.Y) {
						// This is a paint on WholeWindow inside the clientRect; we can safely discard this
						return 0;
					}
					
					hwnd.AddInvalidArea ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
					if (WindowBackgrounds [hwnd] != null) {
						Color c = (Color)WindowBackgrounds [hwnd];
						IntPtr contextref = IntPtr.Zero;
						GetEventParameter (inEvent, 1668183160, 1668183160, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref contextref);
						CGContextSetRGBFillColor (contextref, (float)c.R/255, (float)c.G/255, (float)c.B/255, (float)c.A/255);
						CGContextFillRect (contextref, bounds);
					}
					
					// Add a paint to the queue
					msg.hwnd = hwnd.Handle;
					msg.message = Msg.WM_PAINT;
					msg.wParam = IntPtr.Zero;
					msg.lParam = IntPtr.Zero;
					MessageQueue.Enqueue (msg);
			
					return 0;
				}
				case OSXConstants.kEventControlBoundsChanged: {
					// This can happen before our HWND is created so we need to check to make sure its not null
					if (hwnd != null) {
						// Get the bounds
						HIRect bounds = new HIRect ();
						HIViewGetFrame (handle, ref bounds); 
						// Update the hwnd size
						hwnd.x = (int)bounds.origin.x;
						hwnd.y = (int)bounds.origin.y;
						hwnd.width = (int)bounds.size.width;
						hwnd.height = (int)bounds.size.height;
						
						// TODO: Do we need to send a paint here or does BoundsChanged make a ControlDraw for the exposed area?
					}							
					return 0;
				}
				case OSXConstants.kEventControlTrack: {
					// get the point that was hit
					QDPoint point = new QDPoint ();
					CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
					MouseTrackingResult mousestatus = MouseTrackingResult.kMouseTrackingMouseDown;
					IntPtr modifiers = IntPtr.Zero;
					
					while (mousestatus != MouseTrackingResult.kMouseTrackingMouseUp) {
						CheckTimers (DateTime.Now);
						if (mousestatus == MouseTrackingResult.kMouseTrackingMouseDragged) {
							NativeWindow.WndProc (hwnd.Handle, Msg.WM_MOUSEMOVE, GetMousewParam (0), (IntPtr) ((ushort)point.y << 16 | (ushort)point.x));
						}
						// Process the rest of the event queue
						while (MessageQueue.Count > 0) {
							msg = (MSG)MessageQueue.Dequeue ();
							NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
						}
						TrackMouseLocationWithOptions (IntPtr.Zero, 0, 0.01, ref point, ref modifiers, ref mousestatus);
						int x = point.x;
						int y = point.y;
						ScreenToClient (hwnd.Handle, ref x, ref y);
						point.x = (short)x;
						point.y = (short)y;
					}
					
					msg.hwnd = hwnd.Handle;
					
					bool client = (hwnd.ClientWindow == handle ? true : false);
					
					int wparam = (int)GetMousewParam (0);
					switch (MouseState) {
						case MouseButtons.Left:
							MouseState &= ~MouseButtons.Left;
							msg.message = (client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP);
							wparam &= (int)MsgButtons.MK_LBUTTON;
							break;
						case MouseButtons.Middle:
							MouseState &= ~MouseButtons.Middle;
							msg.message = (client ? Msg.WM_MBUTTONUP : Msg.WM_NCMBUTTONUP);
							wparam &= (int)MsgButtons.MK_MBUTTON;
							break;
						case MouseButtons.Right:
							MouseState &= ~MouseButtons.Right;
							msg.message = (client ? Msg.WM_RBUTTONUP : Msg.WM_NCRBUTTONUP);
							wparam &= (int)MsgButtons.MK_RBUTTON;
							break;
					}
					msg.wParam = (IntPtr)wparam;
						
					msg.lParam = (IntPtr) ((ushort)point.y << 16 | (ushort)point.x);
					MousePosition.X = (int)point.x;
					MousePosition.Y = (int)point.y;
					//NativeWindow.WndProc (msg.hwnd, msg.message, msg.lParam, msg.wParam);
					MessageQueue.Enqueue (msg);
					
					IntPtr window = GetControlOwner (hwnd.Handle);
					SetKeyboardFocus (window, hwnd.Handle, 1);
					
					return 0;
				}
				case OSXConstants.kEventControlContextualMenuClick:
				case OSXConstants.kEventControlClick: {
					// get the point that was hit
					QDPoint point = new QDPoint ();
					CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
					int x = point.x;
					int y = point.y;
					ScreenToClient (hwnd.Handle, ref x, ref y);
					point.x = (short)x;
					point.y = (short)y;
					
					// which button was pressed?
					ushort button = 0;
					GetEventParameter (inEvent, 1835168878, 1835168878, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref button);
					if (button == 2) {
						point.x = (short)MousePosition.X;
						point.y = (short)MousePosition.Y;
					}
					
					msg.hwnd = hwnd.Handle;
					
					bool client = (hwnd.ClientWindow == handle ? true : false);
					
					int wparam = (int)GetMousewParam (0);
					switch (button) {
						case 1:
							MouseState |= MouseButtons.Left;
							msg.message = (client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN);
							wparam |= (int)MsgButtons.MK_LBUTTON;
							break;
						case 2:
							MouseState |= MouseButtons.Right;
							msg.message = (client ? Msg.WM_RBUTTONDOWN : Msg.WM_NCRBUTTONDOWN);
							wparam |= (int)MsgButtons.MK_RBUTTON;
							break;
						case 3:
							MouseState |= MouseButtons.Middle;
							msg.message = (client ? Msg.WM_MBUTTONDOWN : Msg.WM_NCMBUTTONDOWN);
							wparam |= (int)MsgButtons.MK_MBUTTON;
							break;
					}
					msg.wParam = (IntPtr)wparam;
						
					msg.lParam = (IntPtr) ((ushort)point.y << 16 | (ushort)point.x);
					MousePosition.X = (int)point.x;
					MousePosition.Y = (int)point.y;
					NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
					
					TrackControl (handle, point, IntPtr.Zero);
					return 0;
				}
				case OSXConstants.kEventControlSetFocusPart: {
					// This handles setting focus
					short pcode = 1;
					GetEventParameter (inEvent, 1668313716, 1668313716, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (short)), IntPtr.Zero, ref pcode);
					switch (pcode) {
						case 0:
						case -1:
						case -2:
							pcode = 0;
							break;
					}
					SetEventParameter (inEvent, 1668313716, 1668313716, (uint)Marshal.SizeOf (typeof (short)), ref pcode);
					return 0;
				}
			}
			return -9874;
		}
		private IntPtr GetMousewParam(int Delta) {
			int     result = 0;

			if ((MouseState & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((MouseState & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((MouseState & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}

			return (IntPtr)result;
		}

		private double NextTimeout ()
		{
			DateTime now = DateTime.Now;
			int timeout = 0x7FFFFFF;
			lock (TimerList) {
				foreach (Timer timer in TimerList) {
					int next = (int) (timer.Expires - now).TotalMilliseconds;
					if (next < 0)
						return 0;
					if (next < timeout)
						timeout = next;
				}
			}
			if (timeout < Timer.Minimum)
				timeout = Timer.Minimum;

			return (double)((double)timeout/1000);
		}
		
		private void CheckTimers (DateTime now)
		{
			lock (TimerList) {
				int count = TimerList.Count;
				if (count == 0)
					return;
				for (int i = 0; i < TimerList.Count; i++) {
					Timer timer = (Timer) TimerList [i];
					if (timer.Enabled && timer.Expires <= now) {
						timer.FireTick ();
						timer.Update (now);
					}
				}
			}
		}

		internal void InvertCaret () {
			IntPtr window = GetControlOwner (Caret.Hwnd);
			SetPortWindowPort (window);
			Rect r = new Rect ();
			GetWindowPortBounds (window, ref r);
			r.top += (short)Caret.Y;
			r.left += (short)Caret.X;
			r.bottom = (short)(r.top + Caret.Height);
			r.right = (short)(r.left + Caret.Width);
			InvertRect (ref r);
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
		
		internal void ShowCaret () {
			if (Caret.On)
				return;
			Caret.On = true;
			InvertCaret ();
		}

		internal void HideCaret () {
			if (!Caret.On)
				return;
			Caret.On = false;
			InvertCaret ();
		}
		
		internal void InstallTracking (Hwnd hwnd) {
			// This is currently not used
			
			/*
			if (hwnd.client_region_ptr != IntPtr.Zero) {
				ReleaseMouseTrackingRegion (hwnd.client_region_ptr);
				hwnd.client_region_ptr = IntPtr.Zero;
			}
			if (hwnd.whole_region_ptr != IntPtr.Zero) {
				ReleaseMouseTrackingRegion (hwnd.whole_region_ptr);
				hwnd.whole_region_ptr = IntPtr.Zero;
			}
			// Setup the new track region
			if (hwnd.visible) {
				HIRect client_bounds = new HIRect ();	
				HIViewGetBounds (hwnd.client_window, ref client_bounds);
				HIViewConvertRect (ref client_bounds, hwnd.client_window, IntPtr.Zero);
			
				IntPtr rgn = NewRgn ();
				SetRectRgn (rgn, (short)client_bounds.origin.x, (short)client_bounds.origin.y, (short)(client_bounds.origin.x+hwnd.ClientRect.Width), (short)(client_bounds.origin.y+hwnd.ClientRect.Height));
				CreateMouseTrackingRegion (GetControlOwner (hwnd.client_window), rgn, IntPtr.Zero, 0, hwnd.client_region_id, hwnd.client_window, IntPtr.Zero, ref hwnd.client_region_ptr);
				Console.WriteLine (hwnd.ClientRect);
				Console.WriteLine ("Created a mouse trcaking region on the client window @ {0}x{1} {2}x{3}", (short)client_bounds.origin.x, (short)client_bounds.origin.y, (short)(client_bounds.origin.x+hwnd.ClientRect.Width), (short)(client_bounds.origin.y+hwnd.ClientRect.Height));
				if (hwnd.ClientRect.X > 0 && hwnd.ClientRect.Y > 0) {
					HIRect window_bounds = new HIRect ();
					HIViewGetBounds (hwnd.whole_window, ref window_bounds);
					HIViewConvertRect (ref window_bounds, hwnd.whole_window, IntPtr.Zero);
					rgn = NewRgn ();
					SetRectRgn (rgn, (short)window_bounds.origin.x, (short)window_bounds.origin.y, (short)(window_bounds.origin.x+hwnd.ClientRect.X), (short)(window_bounds.origin.y+hwnd.ClientRect.Y));
					CreateMouseTrackingRegion (GetControlOwner (hwnd.whole_window), rgn, IntPtr.Zero, 0, hwnd.whole_region_id, hwnd.whole_window, IntPtr.Zero, ref hwnd.whole_region_ptr);
					Console.WriteLine ("Created a mouse trcaking region on the whole window @ {0}x{1} {2}x{3}", (short)window_bounds.origin.x, (short)window_bounds.origin.y, (short)(window_bounds.origin.x+hwnd.ClientRect.X), (short)(window_bounds.origin.y+hwnd.ClientRect.Y));
				}
			}
			*/
		}

		internal void CheckError (int result, string error) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::" + error + "() Carbon subsystem threw an error: " + result);
		}

		internal void CheckError (int result) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::Carbon subsystem threw an error: " + result);
		}

		#endregion 
		
		#region Public Methods

		internal override IntPtr InitializeDriver() {
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
		}

		internal override void EnableThemes() {
			ThemesEnabled = true;
		}

		internal override void Activate(IntPtr handle) {
			ActivateWindow (GetControlOwner (handle), true);
		}
		
		internal override void CaretVisible (IntPtr hwnd, bool visible) {
			if (Caret.Hwnd == hwnd) {
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
		
		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, int ExStyle, IntPtr MenuHandle, out Rectangle WindowRect) {
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
			CGPoint pt = new CGPoint ();
			Rect wBounds = new Rect ();
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			pt.x = x;
			pt.y = y;

			GetWindowBounds (GetControlOwner (hwnd.client_window), 32, ref wBounds);
			HIViewConvertPoint (ref pt, handle, IntPtr.Zero);

			x = (int)(pt.x+wBounds.left);
			y = (int)(pt.y+wBounds.top);
		}
		
		internal override void CreateCaret (IntPtr hwnd, int width, int height) {
			if (Caret.Hwnd != IntPtr.Zero)
				DestroyCaret (Caret.Hwnd);

			Caret.Hwnd = hwnd;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = 0;
			Caret.On = false;
		}
		
		internal override IntPtr CreateWindow(CreateParams cp) {
			IntPtr windowHnd = IntPtr.Zero;
			IntPtr parentHnd = cp.Parent;
			bool realWindow = false;
			Rectangle clientRect;
			Hwnd hwnd = new Hwnd ();
			
			SetHwndStyles (hwnd, cp);
			
			if (parentHnd == IntPtr.Zero) {
				if ((cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
					// This is a child view that is going to be parentless;
					realWindow = false;
					CheckError (HIViewFindByID (HIViewGetRoot (FosterParent), new HIViewID (OSXConstants.kEventClassWindow, 1), ref parentHnd), "HIViewFindByID ()");
				} else if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
					// This is a popup window that will be real.
					if (cp.X < 1) cp.X = 0;
					if (cp.Y < 1) cp.Y = 0;
					realWindow = true;
				} else {
					// This is a real root window too
					if (cp.X < 1) cp.X = 0;
					if (cp.Y < 1) cp.Y = 0;
					realWindow = true;
				}
			} else {
				realWindow = false;
			}

			if (realWindow) {
				WindowClass windowklass = WindowClass.kOverlayWindowClass;
				WindowAttributes attributes = WindowAttributes.kWindowCompositingAttribute | WindowAttributes.kWindowStandardHandlerAttribute;
				if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) { 
					attributes |= WindowAttributes.kWindowCollapseBoxAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
					attributes |= WindowAttributes.kWindowResizableAttribute | WindowAttributes.kWindowHorizontalZoomAttribute | WindowAttributes.kWindowVerticalZoomAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
					attributes |= WindowAttributes.kWindowCloseBoxAttribute;
				}
				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					attributes = WindowAttributes.kWindowStandardHandlerAttribute | WindowAttributes.kWindowCompositingAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_CAPTION)) != 0) {
					windowklass = WindowClass.kDocumentWindowClass;
				}
					
				IntPtr rect = IntPtr.Zero;
				IntPtr viewHnd = IntPtr.Zero;
				SetRect (ref rect, (short)cp.X, (short)(cp.Y + MenuBarHeight + TitleBarHeight), (short)(cp.Width+cp.X), (short)(cp.Height+cp.Y+MenuBarHeight+TitleBarHeight));
				CheckError (CreateNewWindow (windowklass, attributes, ref rect, ref windowHnd), "CreateNewWindow ()");

				CheckError (InstallEventHandler (GetWindowEventTarget (windowHnd), CarbonEventHandler, (uint)windowEvents.Length, windowEvents, windowHnd, IntPtr.Zero), "InstallEventHandler ()");
				CheckError (HIViewFindByID (HIViewGetRoot (windowHnd), new HIViewID (OSXConstants.kEventClassWindow, 1), ref viewHnd), "HIViewFindByID ()");
				parentHnd = viewHnd;
			}
			hwnd.X = cp.X;
			hwnd.Y = cp.Y;
			hwnd.Width = cp.Width;
			hwnd.Height = cp.Height;
			hwnd.Parent = Hwnd.ObjectFromHandle (cp.Parent);
			hwnd.visible = false;
			clientRect = hwnd.ClientRect;
			
			HIRect r = new HIRect (0, 0, cp.Width, cp.Height);
			CheckError (HIObjectCreate (__CFStringMakeConstantString ("com.apple.hiview"), 0, ref hwnd.whole_window), "HIObjectCreate ()");
			CheckError (InstallEventHandler (GetControlEventTarget (hwnd.whole_window), CarbonEventHandler, (uint)viewEvents.Length, viewEvents, hwnd.whole_window, IntPtr.Zero), "InstallEventHandler ()");
			CheckError (HIViewChangeFeatures (hwnd.whole_window, 1 << 1, 0), "HIViewChangeFeatures ()");
			CheckError (HIViewSetFrame (hwnd.whole_window, ref r), "HIViewSetFrame ()");
			hwnd.WholeWindow = hwnd.whole_window;
			
			r = new HIRect (0, 0, clientRect.Width, clientRect.Height);
			CheckError (HIObjectCreate (__CFStringMakeConstantString ("com.apple.hiview"), 0, ref hwnd.client_window), "HIObjectCreate ()");
			CheckError (InstallEventHandler (GetControlEventTarget (hwnd.client_window), CarbonEventHandler, (uint)viewEvents.Length, viewEvents, hwnd.client_window, IntPtr.Zero), "InstallEventHandler ()");
			CheckError (HIViewChangeFeatures (hwnd.client_window, 1 << 1, 0), "HIViewChangeFeatures ()");
			CheckError (HIViewSetFrame (hwnd.client_window, ref r), "HIViewSetFrame ()");
			hwnd.ClientWindow = hwnd.client_window;
			
			CheckError (HIViewAddSubview (hwnd.whole_window, hwnd.client_window));
			CheckError (HIViewPlaceInSuperviewAt (hwnd.client_window, clientRect.X, clientRect.Y));
			
			if (parentHnd != IntPtr.Zero && parentHnd != hwnd.WholeWindow) {
				CheckError (HIViewAddSubview (parentHnd, hwnd.whole_window), "HIViewAddSubview ()");
				CheckError (HIViewPlaceInSuperviewAt (hwnd.whole_window, cp.X, cp.Y), "HIPlaceInSuperviewAt ()");
				if ((cp.Style & (int)(WindowStyles.WS_VISIBLE))!=0) {
					CheckError (HIViewSetVisible (hwnd.whole_window, true), "HIViewSetVisible ()");
					CheckError (HIViewSetVisible (hwnd.client_window, true), "HIViewSetVisible ()");
					hwnd.visible = true;
				} else {
					CheckError (HIViewSetVisible (hwnd.whole_window, false), "HIViewSetVisible ()");
					CheckError (HIViewSetVisible (hwnd.client_window, false), "HIViewSetVisible ()");
					hwnd.visible = false;
				}
			}
			if (realWindow) {
				WindowMapping [hwnd.Handle] = windowHnd;
				if ((cp.Style & (int)(WindowStyles.WS_VISIBLE))!=0) {
					CheckError (ShowWindow (windowHnd));
					CheckError (HIViewSetVisible (hwnd.whole_window, true), "HIViewSetVisible ()");
					CheckError (HIViewSetVisible (hwnd.client_window, true), "HIViewSetVisible ()");
					hwnd.visible = true;
				}
				if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
					CheckError (HIViewSetVisible (hwnd.whole_window, true), "HIViewSetVisible ()");
					CheckError (HIViewSetVisible (hwnd.client_window, true), "HIViewSetVisible ()");
					hwnd.visible = true;
				}
			}	
			
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

		[MonoTODO]
		internal override IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		internal override IntPtr DefineStdCursor(StdCursor id) {
			switch (id) {
				case StdCursor.AppStarting:
					return (IntPtr)ThemeCursor.kThemeSpinningCursor;
				case StdCursor.Arrow:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.Cross:
					return (IntPtr)ThemeCursor.kThemeCrossCursor;
				case StdCursor.Default:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.Hand:
					return (IntPtr)ThemeCursor.kThemeOpenHandCursor;
				case StdCursor.Help:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.HSplit:
					return (IntPtr)ThemeCursor.kThemeResizeLeftRightCursor;
				case StdCursor.IBeam:
					return (IntPtr)ThemeCursor.kThemeIBeamCursor;
				case StdCursor.No:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.NoMove2D:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.NoMoveHoriz:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.NoMoveVert:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.PanEast:
					return (IntPtr)ThemeCursor.kThemeResizeRightCursor;
				case StdCursor.PanNE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanNorth:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanNW:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanSE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanSouth:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanSW:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanWest:
					return (IntPtr)ThemeCursor.kThemeResizeLeftCursor;
				case StdCursor.SizeAll:
					return (IntPtr)ThemeCursor.kThemeResizeLeftRightCursor;
				case StdCursor.SizeNESW:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.SizeNS:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.SizeNWSE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.SizeWE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
                                case StdCursor.UpArrow:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.VSplit:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.WaitCursor:
					return (IntPtr)ThemeCursor.kThemeSpinningCursor;
                                default:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
			}
		}
		
		internal override IntPtr DefWndProc(ref Message msg) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (msg.HWnd);
			switch ((Msg)msg.Msg) {
				case Msg.WM_DESTROY: {
					if (WindowMapping [hwnd.Handle] != null)

						XplatUI.Exit ();
					break;
				}
			}
			return IntPtr.Zero;
		}

		internal override void DestroyCaret (IntPtr hwnd) {
			if (Caret.Hwnd == hwnd) {
				if (Caret.Visible == 1) {
					Caret.Timer.Stop ();
					HideCaret ();
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = 0;
				Caret.On = false;
			}
		}
		
		[MonoTODO]
		internal override void DestroyCursor(IntPtr cursor) {
			throw new NotImplementedException ();
		}
	
		internal override void DestroyWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			
			if ((hwnd.whole_window != IntPtr.Zero) && HIViewGetSuperview (hwnd.whole_window) != IntPtr.Zero)
				CheckError (HIViewRemoveFromSuperview (handle), "HIViewRemoveFromSuperview ()");

			if (WindowMapping [hwnd.Handle] != null) {
				DisposeWindow ((IntPtr)(WindowMapping [hwnd.Handle]));
			}
			CFRelease (hwnd.ClientWindow);
			CFRelease (hwnd.WholeWindow);
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}
		
		internal override void DoEvents() {
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			//Like X11 we need not do anything here
		}

		internal override void EraseWindowBackground(IntPtr handle, IntPtr wParam) {
			throw new NotImplementedException();
		}


		internal override void Exit() {
			GetMessageResult = false;
			ExitToShell ();
		}
		
		internal override IntPtr GetActive() {
			foreach (DictionaryEntry entry in WindowMapping)
				if (IsWindowActive ((IntPtr)(entry.Value)))
					return (IntPtr)(entry.Key);

			return IntPtr.Zero;
		}
		
		[MonoTODO]
		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			throw new NotImplementedException ();
		}
		
		internal override void GetDisplaySize(out Size size) {
			HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
			size = new Size ((int)bounds.size.width, (int)bounds.size.height);
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
			QDPoint pt = new QDPoint ();
			GetGlobalMouse (ref pt);
			x = pt.x;
			y = pt.y;
		}
		
		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			return GetFontMetrics(g.GetHdc(), font.ToHfont(), out ascent, out descent);
		}
		
		[MonoTODO]
		internal override Graphics GetMenuDC(IntPtr hwnd, IntPtr ncpaint_region) {
			throw new NotImplementedException();
		}
		
		[MonoTODO]
		internal override Point GetMenuOrigin(IntPtr hwnd) {
			throw new NotImplementedException();
		}

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			IntPtr evtRef = IntPtr.Zero;
			IntPtr target = GetEventDispatcherTarget();
			CheckTimers (DateTime.Now);
			ReceiveNextEvent (0, IntPtr.Zero, 0, true, ref evtRef);
			if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
				SendEventToEventTarget (evtRef, target);
				ReleaseEvent (evtRef);
			}
			
			lock (MessageQueue) {
				if (MessageQueue.Count <= 0) {
					if (Idle != null) 
						Idle (this, EventArgs.Empty);
					else if (TimerList.Count == 0) {
						ReceiveNextEvent (0, IntPtr.Zero, Convert.ToDouble ("0." + Timer.Minimum), true, ref evtRef);
						if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
							SendEventToEventTarget (evtRef, target);
							ReleaseEvent (evtRef);
						}
					} else {
						ReceiveNextEvent (0, IntPtr.Zero, NextTimeout (), true, ref evtRef);
						if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
							SendEventToEventTarget (evtRef, target);
							ReleaseEvent (evtRef);
						}
					}
					msg.hwnd = IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return GetMessageResult;
	            }
				msg = (MSG) MessageQueue.Dequeue ();
			}
			return GetMessageResult;
		}
		
		[MonoTODO]
		internal override bool GetText(IntPtr handle, out string text) {
			throw new NotImplementedException ();
		}
		
		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Rectangle rect = hwnd.ClientRect;
			
			x = hwnd.x;
			y = hwnd.y;
			width = hwnd.width;
			height = hwnd.height;

			client_width = rect.Width;
			client_height = rect.Height;
		}
		
		internal override FormWindowState GetWindowState(IntPtr hwnd) {
			IntPtr window = GetControlOwner (hwnd);

			if (IsWindowCollapsed (window))
				return FormWindowState.Minimized;
			if (IsWindowInStandardState (window, IntPtr.Zero, IntPtr.Zero))
				return FormWindowState.Maximized;

			return FormWindowState.Normal;
		}
		
		internal override void GrabInfo(out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea) {
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}
		
		internal override void GrabWindow(IntPtr handle, IntPtr confine_to_handle) {
			GrabWindowHwnd = Hwnd.ObjectFromHandle (handle);
		}
		
		internal override void UngrabWindow(IntPtr hwnd) {
			GrabWindowHwnd = null;
			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;
		}
		
		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}
		
		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd.visible) {
				MSG msg = new MSG ();
				msg.hwnd = hwnd.Handle;
				msg.wParam = IntPtr.Zero;
				msg.lParam = IntPtr.Zero;
				msg.message = Msg.WM_PAINT;
				MessageQueue.Enqueue (msg);
				// This is currently causing some graphics corruption
				//hwnd.AddInvalidArea (rc.X, rc.Y, rc.Width, rc.Height);
				hwnd.AddInvalidArea (0, 0, hwnd.ClientRect.Width, hwnd.ClientRect.Height);
				hwnd.expose_pending = true;
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
		
		[MonoTODO]
		internal override void OverrideCursor(IntPtr cursor) {
			throw new NotImplementedException ();
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
			hwnd.expose_pending = false;
			
			if (Caret.Visible == 1) {
				ShowCaret();
				Caret.Paused = false;
			}
		}
		
		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			Console.WriteLine("XplatUIOSX.PeekMessage");
			return true;
		}

		public static void PostMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			MSG msg = new MSG();
			msg.hwnd = hwnd;
			msg.message = message;
			msg.wParam = wParam;
			msg.lParam = lParam;
			MessageQueue.Enqueue (msg);
		}
		
		[MonoTODO]
		internal override void ReleaseMenuDC(IntPtr hwnd, Graphics dc) {
			throw new NotImplementedException();
		}
		
		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			CGPoint pt = new CGPoint ();
			Rect wBounds = new Rect ();

			GetWindowBounds (GetControlOwner (handle), 32, ref wBounds);
			pt.x = (x-wBounds.left);
			pt.y = (y-wBounds.top);
			HIViewConvertPoint (ref pt, IntPtr.Zero, handle);

			x = (int)pt.x;
			y = (int)pt.y;
		}

		internal override void ScrollWindow(IntPtr handle, Rectangle area, int XAmount, int YAmount, bool clear) {
			//IntPtr rect = IntPtr.Zero;
			//HIRect vBounds = new HIRect ();
         	   
            Hwnd hwnd = Hwnd.ObjectFromHandle(handle);
          
			/*
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
			}*/
			
			HIRect scrollrect = new HIRect ();
			scrollrect.origin.x = area.X;
			scrollrect.origin.y = area.Y;
			scrollrect.size.width = area.Width;
			scrollrect.size.height = area.Height;
			HIViewScrollRect (hwnd.Handle, ref scrollrect, (float)XAmount, (float)-YAmount);
			/*
            HIViewGetBounds (hwnd.client_window, ref vBounds);
			HIViewConvertRect (ref vBounds, hwnd.client_window, IntPtr.Zero);
			SetRect (ref rect, (short)(vBounds.origin.x+area.X), (short)(vBounds.origin.y-TitleBarHeight+area.Y), (short)(vBounds.origin.x+area.Width), (short)(vBounds.origin.y+area.Height-TitleBarHeight));
			ScrollRect (ref rect, (short)XAmount, (short)-YAmount, IntPtr.Zero);
			*/
			// Generate an expose for the area exposed by the horizontal scroll
			/*
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
			
			UpdateWindow (handle);
			*/
		}
		
		
		internal override void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool clear) {
			throw new NotImplementedException("");
		}
		
		[MonoTODO]
		internal override void SendAsyncMethod (AsyncMethodData method) {
			throw new NotImplementedException ();
		}
		
		internal override void SetCaretPos (IntPtr hwnd, int x, int y) {
			if (Caret.Hwnd == hwnd) {
				CGPoint cpt = new CGPoint ();
				cpt.x = x;
				cpt.y = y;
				HIViewConvertPoint (ref cpt, hwnd, IntPtr.Zero);
				Caret.Timer.Stop ();
				HideCaret ();
				Caret.X = (int)cpt.x;
				Caret.Y = (int)cpt.y-23;
				if (Caret.Visible == 1) {
					ShowCaret ();
					Caret.Timer.Start ();
				}
			}
		}
		
		internal override void SetCursor(IntPtr window, IntPtr cursor) {
			SetThemeCursor ((uint) cursor);
		}
		
		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			CGDisplayMoveCursorToPoint (CGMainDisplayID (), new CGPoint (x, y));
		}
		
		internal override void SetFocus(IntPtr handle) {
			if (FocusWindow != IntPtr.Zero) {
				PostMessage(FocusWindow, Msg.WM_KILLFOCUS, handle, IntPtr.Zero);
			}
			PostMessage(handle, Msg.WM_SETFOCUS, FocusWindow, IntPtr.Zero);
			FocusWindow = handle;
		}

		[MonoTODO]
		internal override void SetIcon(IntPtr handle, Icon icon) {
			throw new NotImplementedException();
		}

		
		internal override void SetModal(IntPtr handle, bool Modal) {
			IntPtr hWnd = GetControlOwner (Hwnd.ObjectFromHandle (handle).WholeWindow);
			if (Modal)
				BeginAppModalStateForWindow (hWnd);
			else
				EndAppModalStateForWindow (hWnd);
			return;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			hwnd.parent = Hwnd.ObjectFromHandle (parent);
			if (HIViewGetSuperview (hwnd.whole_window) != IntPtr.Zero) {
				CheckError (HIViewRemoveFromSuperview (hwnd.whole_window), "HIViewRemoveFromSuperview ()");
			}
			CheckError (HIViewAddSubview (hwnd.parent.client_window, hwnd.whole_window));
			CheckError (HIViewAddSubview (hwnd.whole_window, hwnd.client_window));
			HIViewPlaceInSuperviewAt (hwnd.client_window, hwnd.ClientRect.X, hwnd.ClientRect.Y);
			
			return IntPtr.Zero;
		}
		
		internal override void SetTimer (Timer timer) {
			lock (TimerList) {
				TimerList.Add (timer);
			}
		}
		
		internal override bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			HIViewSetZOrder (hWnd, 1, IntPtr.Zero);
			return true;
		}
		
		
		internal override bool SetVisible(IntPtr handle, bool visible) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			object window = WindowMapping [hwnd.Handle];
			if (window != null)
				if (visible)
					ShowWindow ((IntPtr)window);
				else
					HideWindow ((IntPtr)window);
					
			HIViewSetVisible (hwnd.whole_window, visible);
			HIViewSetVisible (hwnd.client_window, visible);
			hwnd.visible = visible;
			return true;
		}
		
		internal override void SetWindowBackground(IntPtr handle, Color color) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (WindowMapping [hwnd.Handle] != null) {
				RGBColor backColor = new RGBColor ();
				backColor.red = (short)(color.R * 257); 
				backColor.green = (short)(color.G * 257);
				backColor.blue = (short)(color.B * 257);

				CheckError (SetWindowContentColor ((IntPtr) WindowMapping [hwnd.Handle], ref backColor));
			} else {
				WindowBackgrounds [hwnd] = color;
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
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Rectangle client_rect = Hwnd.GetClientRectangle(hwnd.border_style, hwnd.menu_handle, hwnd.title_style, width, height);

			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && (hwnd.width == width) && (hwnd.height == height)) {
				return;
			}


			if (WindowMapping [hwnd.Handle] != null) {
				if (y <= MenuBarHeight+TitleBarHeight) {
					y+=MenuBarHeight+TitleBarHeight;
				}
				IntPtr rect = IntPtr.Zero;
				SetRect (ref rect, (short)x, (short)y, (short)(x+width), (short)(y+height));
				CheckError (SetWindowBounds ((IntPtr) WindowMapping [hwnd.Handle], 33, ref rect), "SetWindowBounds ()");
				HIRect r = new HIRect (0, 0, width, height);
				CheckError (HIViewSetFrame (hwnd.whole_window, ref r), "HIViewSetFrame ()");
				r = new HIRect (client_rect.X, client_rect.Y, client_rect.X+client_rect.Width, client_rect.Y+client_rect.Height);
				CheckError (HIViewSetFrame (hwnd.client_window, ref r), "HIViewSetFrame ()");
			} else {
				HIRect r = new HIRect (x, y, width, height);
				CheckError (HIViewSetFrame (hwnd.whole_window, ref r), "HIViewSetFrame ()");
				r = new HIRect (client_rect.X, client_rect.Y, client_rect.X+client_rect.Width, client_rect.Y+client_rect.Height);
				CheckError (HIViewSetFrame (hwnd.client_window, ref r), "HIViewSetFrame ()");
			}			
		}
		
		internal override void SetWindowState(IntPtr hwnd, FormWindowState state) {
			IntPtr window = GetControlOwner (hwnd);

			switch (state) {
				case FormWindowState.Minimized: {
					CollapseWindow (window, true);
					break;
				}
				case FormWindowState.Normal: {
					ZoomWindow (window, 7, false);
					break;
				}
				case FormWindowState.Maximized: {
					ZoomWindow (window, 8, false);
					break;
				}
			}
		}
		
		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			SetHwndStyles(hwnd, cp);
			
			if (WindowMapping [hwnd.Handle] != null) {
				WindowAttributes attributes = WindowAttributes.kWindowCompositingAttribute | WindowAttributes.kWindowStandardHandlerAttribute;
				if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) { 
					attributes |= WindowAttributes.kWindowCollapseBoxAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
					attributes |= WindowAttributes.kWindowResizableAttribute | WindowAttributes.kWindowHorizontalZoomAttribute | WindowAttributes.kWindowVerticalZoomAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
					attributes |= WindowAttributes.kWindowCloseBoxAttribute;
				}
				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					attributes = WindowAttributes.kWindowStandardHandlerAttribute | WindowAttributes.kWindowCompositingAttribute;
				}

				WindowAttributes outAttributes = WindowAttributes.kWindowNoAttributes;
				GetWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], ref outAttributes);
				ChangeWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], attributes, outAttributes);
			}
		}
		
		internal override bool SetZOrder(IntPtr handle, IntPtr after_handle, bool Top, bool Bottom) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (Top) {
				HIViewSetZOrder (hwnd.whole_window, 2, IntPtr.Zero);
				return true;
			} else if (!Bottom) {
				Hwnd after_hwnd = Hwnd.ObjectFromHandle (after_handle);
				HIViewSetZOrder (hwnd.whole_window, 2, after_hwnd.whole_window);
			} else {
				HIViewSetZOrder (hwnd.whole_window, 1, IntPtr.Zero);
				return true;
			}
			return false;
		}

		internal override void ShowCursor(bool show) {
			if (show)
				CGDisplayShowCursor (CGMainDisplayID ());
			else
				CGDisplayHideCursor (CGMainDisplayID ());
		}
		
		[MonoTODO]
		internal override bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		internal override bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		internal override void SystrayRemove(IntPtr hwnd, ref ToolTip tt) {
			throw new NotImplementedException();
		}
		
		internal override bool Text(IntPtr handle, string text) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			if (WindowMapping [hwnd.Handle] != null) {
				CheckError (SetWindowTitleWithCFString ((IntPtr)(WindowMapping [hwnd.Handle]), __CFStringMakeConstantString (text)));
			}
			CheckError (SetControlTitleWithCFString (hwnd.whole_window, __CFStringMakeConstantString (text)));
			CheckError (SetControlTitleWithCFString (hwnd.client_window, __CFStringMakeConstantString (text)));
			return true;
		}
		
		internal override void UpdateWindow(IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd.visible && !hwnd.expose_pending) {
				MSG msg = new MSG ();
				msg.message = Msg.WM_PAINT;
				msg.hwnd = hwnd.Handle;
				msg.lParam = IntPtr.Zero;
				msg.wParam = IntPtr.Zero;
				MessageQueue.Enqueue (msg);
			}
		}
		
		internal override bool TranslateMessage(ref MSG msg) {
			bool res = false;
			Hwnd hwnd = Hwnd.ObjectFromHandle (msg.hwnd);
					
			switch (msg.message) {
				case Msg.WM_MOUSEMOVE: {
					// We're grabbed
					if (GrabWindowHwnd != null) {
						if (GrabWindowHwnd.Handle != hwnd.Handle) {
							return false;
						}
					} else {
						if (MouseWindow != null) {
							if (MouseWindow.Handle != hwnd.Handle) {
								PostMessage (MouseWindow.Handle, Msg.WM_MOUSE_LEAVE, IntPtr.Zero, IntPtr.Zero);
								PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
								MouseWindow = hwnd;
							}
						} else {
							MouseWindow = hwnd;
						}
					}
					break;
				}
				case Msg.WM_SETFOCUS: {
					break;	 
				}					
				
			}
			
			// This is a hideous temporary keyboard hack to bind some keys	
			if (msg.message >= Msg.WM_KEYFIRST && msg.message <= Msg.WM_KEYLAST)
				res = true;

			if (msg.message != Msg.WM_KEYDOWN && msg.message != Msg.WM_SYSKEYDOWN)
				return res;

			if ((int)msg.wParam >= (int)'0' && (int)msg.wParam <= (int)'z') {
				Msg message;
				message = Msg.WM_CHAR;
				PostMessage (msg.hwnd, message, msg.wParam, msg.lParam);
			}
			return true;
		}
		
		#endregion
		
		#region System information
		internal override int KeyboardSpeed { get{ throw new NotImplementedException(); } } 
		internal override int KeyboardDelay { get{ throw new NotImplementedException(); } } 

		internal override  Size CursorSize { get{ throw new NotImplementedException(); } }
		internal override  bool DragFullWindows { get{ throw new NotImplementedException(); } }
		internal override  Size DragSize { get{ throw new NotImplementedException(); } }
		internal override  Size IconSize { get{ throw new NotImplementedException(); } }
		internal override  Size MaxWindowTrackSize { get{ throw new NotImplementedException(); } }
		internal override  Size MinimizedWindowSize { get{ throw new NotImplementedException(); } }
		internal override  Size MinimizedWindowSpacingSize { get{ throw new NotImplementedException(); } }
		internal override  Size MinimumWindowSize { get{ throw new NotImplementedException(); } }
		internal override  Size MinWindowTrackSize { get{ throw new NotImplementedException(); } }
		internal override  Size SmallIconSize { get{ throw new NotImplementedException(); } }
		internal override  int MouseButtonCount { get{ throw new NotImplementedException(); } }
		internal override  bool MouseButtonsSwapped { get{ throw new NotImplementedException(); } }
		internal override  bool MouseWheelPresent { get{ throw new NotImplementedException(); } }
		internal override  Rectangle VirtualScreen { get{ throw new NotImplementedException(); } }
		internal override  Rectangle WorkingArea { get{ throw new NotImplementedException(); } }
		#endregion
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetNeedsDisplayInRegion (IntPtr view, IntPtr rgn, bool needsDisplay);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetSubviewHit (IntPtr contentView, ref CGPoint point, bool tval, ref IntPtr outPtr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetViewForMouseEvent (IntPtr inView, IntPtr inEvent, ref IntPtr outView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewConvertPoint (ref CGPoint point, IntPtr pView, IntPtr cView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewChangeFeatures (IntPtr aView, ulong bitsin, ulong bitsout);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewFindByID (IntPtr rootWnd, HIViewID id, ref IntPtr outPtr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetRoot (IntPtr hWnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIObjectCreate (IntPtr cfStr, uint what, ref IntPtr hwnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetNeedsDisplay (IntPtr viewHnd, bool update);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetFrame (IntPtr viewHnd, ref HIRect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetFrame (IntPtr viewHnd, ref HIRect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewPlaceInSuperviewAt (IntPtr view, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewAddSubview (IntPtr parentHnd, IntPtr childHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetNextView (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetPreviousView (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetFirstSubview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetSuperview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewRemoveFromSuperview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetVisible (IntPtr vHnd, bool visible);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern bool HIViewIsVisible (IntPtr vHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetBounds (IntPtr vHnd, ref HIRect r);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewScrollRect (IntPtr vHnd, ref HIRect rect, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewScrollRect (IntPtr vHnd, IntPtr rect, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetZOrder (IntPtr hWnd, int cmd, IntPtr oHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetBoundsOrigin (IntPtr vHnd, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewConvertRect (ref HIRect r, IntPtr a, IntPtr b);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void ScrollRect (ref IntPtr r, short dh, short dv, IntPtr rgnHandle);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void SetRect (ref IntPtr r, short left, short top, short right, short bottom);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int CreateEvent (IntPtr allocator, uint classid, uint kind, double when, uint attributes, ref IntPtr outEvent);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                static extern int InstallEventHandler (IntPtr window, CarbonEventDelegate handlerProc, uint numtypes, EventTypeSpec [] typeList, IntPtr userData, IntPtr handlerRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlOwner (IntPtr aView);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int ActivateWindow (IntPtr windowHnd, bool inActivate);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern bool IsWindowActive (IntPtr windowHnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetKeyboardFocus (IntPtr windowHdn, IntPtr cntrlHnd, short partcode);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetKeyboardFocus (IntPtr handle, ref IntPtr cntrl);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetWindowEventTarget (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlEventTarget (IntPtr aControl);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetEventDispatcherTarget ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SendEventToEventTarget (IntPtr evt, IntPtr target);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ReleaseEvent (IntPtr evt);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ReceiveNextEvent (uint evtCount, IntPtr evtTypes, double timeout, bool processEvt, ref IntPtr evt);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern uint GetEventClass (IntPtr eventRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern uint GetEventKind (IntPtr eventRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, uint inName, uint inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref byte outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, uint inName, uint inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref IntPtr outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, uint inName, uint inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref ushort outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, uint inName, uint inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref short outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, uint inName, uint inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref QDPoint outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetEventParameter (IntPtr evt, uint inName, uint inType, uint bufSize, ref short outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetEventParameter (IntPtr evt, uint inName, uint inType, uint bufSize, ref IntPtr outData);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextFlush (IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextFillRect (IntPtr cgc, HIRect r);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern CGAffineTransform CGContextGetTextMatrix (IntPtr cgContext);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetTextMatrix (IntPtr cgContext, CGAffineTransform ctm);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetRGBFillColor (IntPtr cgContext, float r, float g, float b, float alpha);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetRGBStrokeColor (IntPtr cgContext, float r, float g, float b, float alpha);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetTextDrawingMode (IntPtr cgContext, int drawingMode);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSelectFont (IntPtr cgContext, string fontName, float size, int textEncoding);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextShowTextAtPoint (IntPtr cgContext, float x, float y, string text, int length);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextClipToRect (IntPtr cgContext, HIRect clip);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CreateCGContextForPort (IntPtr port, ref IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern bool IsWindowCollapsed (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern bool IsWindowInStandardState (IntPtr hWnd, IntPtr a, IntPtr b);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CollapseWindow (IntPtr hWnd, bool collapse);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void ZoomWindow (IntPtr hWnd, short partCode, bool front);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowAttributes (IntPtr hWnd, ref WindowAttributes outAttributes);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ChangeWindowAttributes (IntPtr hWnd, WindowAttributes inAttributes, WindowAttributes outAttributes);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetPortWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetGlobalMouse (ref QDPoint outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GlobalToLocal (ref QDPoint outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int LocalToGlobal (ref QDPoint outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int TrackControl (IntPtr handle, QDPoint point, IntPtr data);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int BeginAppModalStateForWindow (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int EndAppModalStateForWindow (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CreateNewWindow (WindowClass klass, WindowAttributes attributes, ref IntPtr r, ref IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int DisposeWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ShowWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HideWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SetWindowBounds (IntPtr wHnd, uint reg, ref IntPtr rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowPortBounds (IntPtr wHnd, ref Rect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowBounds (IntPtr wHnd, uint reg, ref Rect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int InvertRect (ref Rect r);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SetControlTitleWithCFString (IntPtr hWnd, IntPtr titleCFStr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SetWindowTitleWithCFString (IntPtr hWnd, IntPtr titleCFStr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr __CFStringMakeConstantString (string cString);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextRestoreGState (IntPtr ctx);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextSaveGState (IntPtr ctx);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextTranslateCTM (IntPtr ctx, double tx, double ty);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextScaleCTM (IntPtr ctx, double tx, double ty);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetWindowContentColor (IntPtr hWnd, ref RGBColor backColor);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int TrackMouseLocationWithOptions (IntPtr port, int options, double eventtimeout, ref QDPoint point, ref IntPtr modifier, ref MouseTrackingResult status);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int CreateMouseTrackingRegion (IntPtr windowref, IntPtr rgn, IntPtr clip, int options, MouseTrackingRegionID rid, IntPtr refcon, IntPtr evttargetref, ref IntPtr mousetrackref);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int ReleaseMouseTrackingRegion (IntPtr region_handle);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CFRelease (IntPtr wHnd);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr NewRgn ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CopyRgn (IntPtr srcrgn, IntPtr destrgn);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void SetRectRgn (IntPtr rgn, short left, short top, short right, short bottom);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void DisposeRgn (IntPtr rgn);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void ExitToShell ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static short GetMBarHeight ();
		
		#region Cursor imports
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static HIRect CGDisplayBounds (IntPtr displayID);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr CGMainDisplayID ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CGDisplayShowCursor (IntPtr display);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CGDisplayHideCursor (IntPtr display);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CGDisplayMoveCursorToPoint (IntPtr display, CGPoint point);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void SetThemeCursor (uint inCursor);
		#endregion

		[DllImport ("gdiplus", EntryPoint="GetFontMetrics")]
		internal extern static bool GetFontMetrics(IntPtr graphicsObject, IntPtr nativeObject, out int ascent, out int descent);
	}

}
