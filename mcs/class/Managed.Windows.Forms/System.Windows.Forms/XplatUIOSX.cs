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

using System;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// OSX Version
namespace System.Windows.Forms {

	delegate int CarbonEventHandler (IntPtr inCallRef, IntPtr inEvent, IntPtr userData);

	internal class XplatUIOSX : XplatUIDriver {
		
		private static XplatUIOSX instance;
		private static int ref_count;

		internal static MouseButtons mouse_state;
		internal static Point mouse_position;
		internal static OSXCaret caret;

		internal static IntPtr mouseInWindow;
		private static Queue carbonEvents;
		private CarbonEventHandler viewEventHandler;
		private CarbonEventHandler windowEventHandler;
		private static Hashtable view_window_mapping;
		private static Hashtable view_backgrounds;
		private static IntPtr grabWindow;
		private static IntPtr fosterParent;

		private static OSXHover hover;
		private static bool getmessage_ret;

		private static EventTypeSpec [] viewEvents = new EventTypeSpec [] {
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlSetFocusPart), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlClick), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlSimulateHit), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlBoundsChanged), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlDraw) 
									};
		private static EventTypeSpec [] windowEvents = new EventTypeSpec[] {
									new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseDown),
									new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseUp),
									new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseMoved),
									new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseDragged),
									new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseWheelMoved),
									new EventTypeSpec (OSXConstants.kEventClassWindow, OSXConstants.kEventWindowBoundsChanged),
									new EventTypeSpec (OSXConstants.kEventClassWindow, OSXConstants.kEventWindowClose),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyDown),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyRepeat),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyUp)
									};

		private ArrayList timer_list;

		[MonoTODO]
		internal override Keys ModifierKeys {
			get {
				return Keys.None;
			}
		}

		internal override MouseButtons MouseButtons {
			get {
				return mouse_state;
			}
		}

		internal override Point MousePosition {
			get {
				return mouse_position;
			}
		}

		[MonoTODO]
		internal override bool DropTarget {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size CursorSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override bool DragFullWindows {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size DragSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size IconSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size MaxWindowTrackSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size MinimizedWindowSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size MinimizedWindowSpacingSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size MinimumWindowSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size MinWindowTrackSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Size SmallIconSize {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override int MouseButtonCount {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override bool MouseButtonsSwapped {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override bool MouseWheelPresent {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Rectangle VirtualScreen {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override Rectangle WorkingArea {
			get {
				throw new NotImplementedException ();
			}
		}

		private XplatUIOSX() {
			viewEventHandler = new CarbonEventHandler (ViewHandler);
			windowEventHandler = new CarbonEventHandler (WindowHandler);
			ref_count = 0;
			mouseInWindow = IntPtr.Zero;
			carbonEvents = new Queue ();
			grabWindow = IntPtr.Zero;
			view_window_mapping = new Hashtable ();
			view_backgrounds = new Hashtable ();
			mouse_state=MouseButtons.None;
			mouse_position=Point.Empty;
			
			IntPtr rect = IntPtr.Zero;
			SetRect (ref rect, (short)0, (short)0, (short)0, (short)0);
			CheckError (CreateNewWindow (WindowClass.kDocumentWindowClass, WindowAttributes.kWindowStandardHandlerAttribute | WindowAttributes.kWindowCloseBoxAttribute | WindowAttributes.kWindowFullZoomAttribute | WindowAttributes.kWindowCollapseBoxAttribute | WindowAttributes.kWindowResizableAttribute | WindowAttributes.kWindowCompositingAttribute, ref rect, ref fosterParent), "CreateFosterParent ()");
			timer_list = new ArrayList ();

			hover.interval = 500;
			hover.timer = new Timer ();
			hover.timer.Enabled = false;
			hover.timer.Interval = hover.interval;
			hover.timer.Tick += new EventHandler (MouseHover);
			hover.x = -1;
			hover.y = -1;
			caret.timer = new Timer ();
			caret.timer.Interval = 500;
			caret.timer.Tick += new EventHandler (CaretCallback);

			getmessage_ret = true;
		}

		~XplatUIOSX() {
			// FIXME: Do we need to tear anything down here?
		}

		public static XplatUIOSX GetInstance() {
			lock (typeof (XplatUIOSX)) {
				if (instance == null) {
					instance = new XplatUIOSX ();
				}
				ref_count++;
			}
			return instance;
		}

		internal override event EventHandler Idle;

		private void MouseHover (object sender, EventArgs e) {
			if ((hover.x == mouse_position.X) && (hover.y == mouse_position.Y)) {
				MSG msg = new MSG ();
				msg.hwnd = hover.hwnd;
				msg.message = Msg.WM_MOUSEHOVER;
				msg.wParam = GetMousewParam (0);
				msg.lParam = (IntPtr)((ushort)hover.y << 16 | (ushort)hover.x);
				carbonEvents.Enqueue (msg);
			}
		}

		public int Reference {
			get {
				return ref_count;
			}
		}

		private void CaretCallback (object sender, EventArgs e) {
			if (caret.paused) {
				return;
			}

			if (!caret.on) {
				ShowCaret ();
			} else {
				HideCaret ();
			}
		}

		internal override IntPtr InitializeDriver() {
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
		}

		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			getmessage_ret = false;
			// Hackaround a codeman bad access
			ExitToShell ();
		}

		internal override void GetDisplaySize(out Size size) {
			// FIXME:
			HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
			size = new Size ((int)bounds.size.width, (int)bounds.size.height);
		}

		[MonoTODO]
		internal override void EnableThemes() {
			throw new NotImplementedException ();
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			IntPtr hWnd = IntPtr.Zero;
			IntPtr windowHnd = IntPtr.Zero;
			IntPtr parentHnd = cp.Parent;
			bool realWindow = false;
			
			if (parentHnd == IntPtr.Zero) {
				if ((cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
					// This is a child view that is going to be parentless;
					realWindow = false;
					CheckError (HIViewFindByID (HIViewGetRoot (fosterParent), new HIViewID (OSXConstants.kEventClassWindow, 1), ref parentHnd), "HIViewFindByID ()");
				} else if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
					// This is a popup window that will be real.
					realWindow = true;
				} else {
					// This is a real root window too
					if (cp.X < 1) cp.X = 0;
					if (cp.Y < 1) cp.Y = 44;
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
				SetRect (ref rect, (short)cp.X, (short)cp.Y, (short)(cp.Width+cp.X), (short)(cp.Height+cp.Y));
				CheckError (CreateNewWindow (windowklass, attributes, ref rect, ref windowHnd), "CreateNewWindow ()");
				CheckError (InstallEventHandler (GetWindowEventTarget (windowHnd), windowEventHandler, (uint)windowEvents.Length, windowEvents, windowHnd, IntPtr.Zero), "InstallEventHandler ()");
				CheckError (HIViewFindByID (HIViewGetRoot (windowHnd), new HIViewID (OSXConstants.kEventClassWindow, 1), ref viewHnd), "HIViewFindByID ()");
				parentHnd = viewHnd;
			}
			HIRect r = new HIRect (0, 0, cp.Width, cp.Height);
			CheckError (HIObjectCreate (__CFStringMakeConstantString ("com.apple.hiview"), 0, ref hWnd), "HIObjectCreate ()");
			CheckError (InstallEventHandler (GetControlEventTarget (hWnd), viewEventHandler, (uint)viewEvents.Length, viewEvents, hWnd, IntPtr.Zero), "InstallEventHandler ()");
			CheckError (HIViewChangeFeatures (hWnd, 1 << 1, 0), "HIViewChangeFeatures ()");
//			CheckError (HIViewChangeFeatures (hWnd, 1 << 25, 0), "HIViewChangeFeatures ()");
			CheckError (HIViewSetFrame (hWnd, ref r), "HIViewSetFrame ()");
			if (parentHnd != IntPtr.Zero && parentHnd != hWnd) {
				CheckError (HIViewAddSubview (parentHnd, hWnd), "HIViewAddSubview ()");
				CheckError (HIViewPlaceInSuperviewAt (hWnd, cp.X, cp.Y), "HIPlaceInSuperviewAt ()");
				if ((cp.Style & (int)(WindowStyles.WS_VISIBLE))!=0) {
					CheckError (HIViewSetVisible (hWnd, true), "HIViewSetVisible ()");
				} else {
					CheckError (HIViewSetVisible (hWnd, false), "HIViewSetVisible ()");
				}
			}
			if (realWindow) {
				view_window_mapping [hWnd] = windowHnd;
				if ((cp.Style & (int)(WindowStyles.WS_VISIBLE))!=0) {
					CheckError (ShowWindow (windowHnd));
					CheckError (HIViewSetVisible (hWnd, true), "HIViewSetVisible ()");
				} else if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
//					CheckError (ShowWindow (windowHnd));
					CheckError (HIViewSetVisible (hWnd, true), "HIViewSetVisible ()");
				}
			}
			return hWnd;
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

		internal override void DestroyWindow(IntPtr handle) {
//			if (HIViewGetSuperview (handle) != IntPtr.Zero)
//				CheckError (HIViewRemoveFromSuperview (handle), "HIViewRemoveFromSuperview ()");
			if (view_window_mapping [handle] != null) {
				DisposeWindow ((IntPtr)(view_window_mapping [handle]));
			} else {
//				CFRelease (handle);
			}
		}

		internal override FormWindowState GetWindowState(IntPtr hwnd) {
			IntPtr window = GetControlOwner (hwnd);

			if (IsWindowCollapsed (window))
				return FormWindowState.Minimized;
			if (IsWindowInStandardState (window, IntPtr.Zero, IntPtr.Zero))
				return FormWindowState.Maximized;

			return FormWindowState.Normal;
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
			if (view_window_mapping [handle] != null) {
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
				GetWindowAttributes ((IntPtr)view_window_mapping [handle], ref outAttributes);
				ChangeWindowAttributes ((IntPtr)view_window_mapping [handle], attributes, outAttributes);
			}
		}

#if obsolete
		internal override void RefreshWindow(IntPtr handle) {
			HIRect r = new HIRect ();
			CheckError (HIViewGetFrame (handle, ref r), "HIViewGetFrame ()");
			Invalidate (handle, new Rectangle (0, 0, (int)r.size.width, (int)r.size.height), true);
		}
#endif

		internal override void UpdateWindow(IntPtr handle) {
			// Force a redraw of previously invalidated areas (ie send a WM_PAINT)
			HIRect r = new HIRect ();
			CheckError (HIViewGetFrame (handle, ref r), "HIViewGetFrame ()");
			Invalidate (handle, new Rectangle (0, 0, (int)r.size.width, (int)r.size.height), true);
		}


		[MonoTODO("Find a way to make all the views do this; not just the window view")]
		internal override void SetWindowBackground(IntPtr handle, Color color) {
			if (view_window_mapping [handle] != null) {
				RGBColor backColor = new RGBColor ();
				backColor.red = (short)(color.R * 257); 
				backColor.green = (short)(color.G * 257);
				backColor.blue = (short)(color.B * 257);

				CheckError (SetWindowContentColor ((IntPtr) view_window_mapping [handle], ref backColor));
			} else {
				view_backgrounds [handle] = color;
			}
		}

		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			PaintEventArgs  paint_event;
			HandleData	data = HandleData.Handle(handle);
 
			if (caret.visible == 1) {
				caret.paused = true;
				HideCaret ();
			}

			data.DeviceContext = Graphics.FromHwnd (handle);
			paint_event = new PaintEventArgs((Graphics)data.DeviceContext, data.InvalidArea);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			HandleData	data = HandleData.Handle(handle);

			if (data == null)
				throw new Exception ("null data on PaintEventEnd");
			data.ClearInvalidArea ();
			Graphics g = (Graphics) data.DeviceContext;
			g.Flush ();
			g.Dispose ();

			if (caret.visible == 1) {
				caret.paused = false;
				ShowCaret ();
			}
                }

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			if (view_window_mapping [handle] != null) {
				if (x == 0 && y == 0) {
					Rect bounds = new Rect ();
					CheckError (GetWindowBounds ((IntPtr) view_window_mapping [handle], 32, ref bounds), "GetWindowBounds ()");
					x += bounds.left;
					y += bounds.top;
				}
				IntPtr rect = IntPtr.Zero;
				SetRect (ref rect, (short)x, (short)y, (short)(x+width), (short)(y+height));
				CheckError (SetWindowBounds ((IntPtr) view_window_mapping [handle], 32, ref rect), "SetWindowBounds ()");
				HIRect r = new HIRect (0, 0, width, height);
				CheckError (HIViewSetFrame (handle, ref r), "HIViewSetFrame ()");
			} else {
				HIRect r = new HIRect (x, y, width, height);
				CheckError (HIViewSetFrame (handle, ref r), "HIViewSetFrame ()");
			}
		}

		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			width = 0;
			height = 0;
			if (view_window_mapping [handle] != null) {
				Rect bounds = new Rect ();
				CheckError (GetWindowBounds ((IntPtr)(view_window_mapping [handle]), 32, ref bounds), "GetWindowBounds ()");
				x = bounds.left;
				y = bounds.top;
				width = bounds.right-bounds.left;
				height = bounds.bottom-bounds.top;
			} else {
				HIRect r = new HIRect ();
				CheckError (HIViewGetFrame (handle, ref r), "HIViewGetFrame ()");

				x = (int)r.origin.x;
				y = (int)r.origin.y;
				width = (int)r.size.width;
				height = (int)r.size.height;

			}
			client_width = width;
			client_height = height;
		}

		[MonoTODO]
		internal override void Activate(IntPtr handle) {
			ActivateWindow (GetControlOwner (handle), true);
		}

		[MonoTODO]
		internal override void EnableWindow(IntPtr handle, bool Enable) {
			//Like X11 we need not do anything here
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
			IntPtr hWnd = IntPtr.Zero;
			if (view_window_mapping [handle] != null) 
				hWnd = ((IntPtr)(view_window_mapping [handle]));
			else 
				hWnd = GetControlOwner (handle);

			if (Modal)
				BeginAppModalStateForWindow (hWnd);
			else
				EndAppModalStateForWindow (hWnd);
			return;
		}

		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			// FIXME: We need to resolve the painting issues exposed by HIViewSetNeedsDisplayInRegion with listview
			// This method isn't exactly right as some other event may AddToInvalidArea before this paint goes through
			MSG msg = new MSG ();
			msg.hwnd = handle;
			msg.message = Msg.WM_PAINT;
			msg.wParam = IntPtr.Zero;
			msg.lParam = IntPtr.Zero;
			HandleData.Handle(handle).AddToInvalidArea (rc.X, rc.Y, rc.Width, rc.Height);
			carbonEvents.Enqueue (msg);
	
/*
			IntPtr evt = IntPtr.Zero;
			IntPtr rgn = NewRgn ();
			SetRectRgn (rgn, (short)rc.X, (short)rc.Y, (short)(rc.X+rc.Width), (short)(rc.Y+rc.Height));
			CreateEvent (IntPtr.Zero, OSXConstants.kEventClassControl, 4, 0, 1, ref evt); 
                        IntPtr target = GetEventDispatcherTarget();
			SendEventToEventTarget (target, evt);

			HIViewSetNeedsDisplayInRegion (handle, rgn, true);
			DisposeRgn (rgn);
*/
		}

		[MonoTODO]
		internal override IntPtr DefWndProc(ref Message msg) {
			switch ((Msg)msg.Msg) {
				case Msg.WM_DESTROY: {
					if (view_window_mapping [msg.HWnd] != null)

						XplatUI.Exit ();
					break;
				}
			}
			return IntPtr.Zero;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
                        Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
                        Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void DoEvents() {
//			Console.WriteLine("XplatUIOSX.DoEvents");
		}

		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			Console.WriteLine("XplatUIOSX.PeekMessage");
			return true;
		}

		internal int WindowHandler (IntPtr inCallRef, IntPtr inEvent, IntPtr controlHnd) {
			uint eventClass = GetEventClass (inEvent);
			uint eventKind = GetEventKind (inEvent);
			MSG msg = new MSG ();
			msg.hwnd = IntPtr.Zero; 
			lock (carbonEvents) {
				switch (eventClass) {
					// keyboard
					case OSXConstants.kEventClassKeyboard: {
						byte charCode = 0x00;
						GetEventParameter (inEvent, 1801676914, 1413830740, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (byte)), IntPtr.Zero, ref charCode);
						IntPtr cntrl = IntPtr.Zero;
						CheckError (GetKeyboardFocus (controlHnd, ref cntrl), "GetKeyboardFocus()");
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
						carbonEvents.Enqueue (msg);
						return -9874;
					}
					case OSXConstants.kEventClassWindow: {
						switch (eventKind) {
							case OSXConstants.kEventWindowClose: {
								// This is our real window; so we have to post to the corresponding view
								IDictionaryEnumerator e = view_window_mapping.GetEnumerator ();
								while (e.MoveNext ()) {
									if ((IntPtr)e.Value == controlHnd) {
										NativeWindow.WndProc((IntPtr)e.Key, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
									}
								}
								return 0;
							}
							case OSXConstants.kEventWindowBoundsChanged: {
								// This is our real window; so we have to resize the corresponding view as well
								IDictionaryEnumerator e = view_window_mapping.GetEnumerator ();
								while (e.MoveNext ()) {
									if ((IntPtr)e.Value == controlHnd) {
										Rect bounds = new Rect ();
										CheckError (GetWindowBounds (controlHnd, 32, ref bounds), "GetWindowBounds ()");
										HIRect r = new HIRect ();
										CheckError (HIViewGetFrame ((IntPtr)e.Key, ref r), "HIViewGetFrame ()");
										r.size.width = bounds.right-bounds.left;
										r.size.height = bounds.bottom-bounds.top;
                        							CheckError (HIViewSetFrame ((IntPtr)e.Key, ref r), "HIViewSetFrame ()");
										msg.message = Msg.WM_WINDOWPOSCHANGED;
										msg.hwnd = (IntPtr)e.Key;
										msg.wParam = IntPtr.Zero;
										msg.lParam = IntPtr.Zero;
										carbonEvents.Enqueue (msg);
										return 0;
									}
								}
								break;
							}
						}
						break;
					}
					case OSXConstants.kEventClassMouse: {
						switch (eventKind) {
							case OSXConstants.kEventMouseDown: 
							case OSXConstants.kEventMouseUp: {
								QDPoint point = new QDPoint ();
								IntPtr pView = IntPtr.Zero;
								IntPtr rView = IntPtr.Zero;
								int wParam = 0;
								ushort btn = 0;
								GetEventParameter (inEvent, 1835168878, 1835168878, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref btn);
								CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
								// Translate this point
								Rect bounds = new Rect ();
								CheckError (GetWindowBounds (controlHnd, 33, ref bounds), "GetWindowBounds ()");
								point.x = (short)(point.x-bounds.left);
								point.y = (short)(point.y-bounds.top);
								// Swizzle it so its pointed at the right control
								CGPoint hiPoint = new CGPoint (point.x, point.y);
								CheckError (HIViewFindByID (HIViewGetRoot (controlHnd), new HIViewID (OSXConstants.kEventClassWindow, 1), ref pView), "HIViewFindByID ()");
								CheckError (HIViewGetSubviewHit (pView, ref hiPoint, true, ref rView));
								HIViewConvertPoint (ref hiPoint, pView, rView);
								if (grabWindow == IntPtr.Zero)
									msg.hwnd = rView;
								else
									msg.hwnd = grabWindow;
								switch (btn) {
									case 1:
										if (eventKind == OSXConstants.kEventMouseDown) {
											mouse_state |= MouseButtons.Left;
											msg.message = Msg.WM_LBUTTONDOWN;
											wParam |= (int)MsgButtons.MK_LBUTTON;
										} else {
											mouse_state &= MouseButtons.Left;
											msg.message = Msg.WM_LBUTTONUP;
											wParam |= (int)MsgButtons.MK_LBUTTON;
										}
										break;
									case 2:
										if (eventKind == OSXConstants.kEventMouseDown) {
											mouse_state |= MouseButtons.Middle;
											msg.message = Msg.WM_MBUTTONDOWN;
											wParam |= (int)MsgButtons.MK_MBUTTON;
										} else {
											mouse_state &= MouseButtons.Middle;
											msg.message = Msg.WM_MBUTTONUP;
											wParam |= (int)MsgButtons.MK_MBUTTON;
										}
										break;
									case 3:
										if (eventKind == OSXConstants.kEventMouseDown) {
											mouse_state |= MouseButtons.Right;
											msg.message = Msg.WM_RBUTTONDOWN;
											wParam |= (int)MsgButtons.MK_RBUTTON;
										} else {
											mouse_state &= MouseButtons.Right;
											msg.message = Msg.WM_RBUTTONUP;
											wParam |= (int)MsgButtons.MK_RBUTTON;
										}
										break;
								}
								msg.wParam = (IntPtr)wParam;
								msg.lParam = (IntPtr) ((ushort)hiPoint.y << 16 | (ushort)hiPoint.x);
								mouse_position.X = (int)hiPoint.x;
								mouse_position.Y = (int)hiPoint.y;
								carbonEvents.Enqueue (msg);
								return -9874;
							}
							case OSXConstants.kEventMouseMoved: {
								QDPoint point = new QDPoint ();
								IntPtr pView = IntPtr.Zero;
								IntPtr rView = IntPtr.Zero;								
								ushort btn = 0;
								GetEventParameter (inEvent, 1835168878, 1835168878, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref btn);
								CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
								// Translate this point
								Rect bounds = new Rect ();
								CheckError (GetWindowBounds (controlHnd, 33, ref bounds), "GetWindowBounds ()");
								point.x = (short)(point.x-bounds.left);
								point.y = (short)(point.y-bounds.top);
								// Swizzle it so its pointed at the right control
								CGPoint hiPoint = new CGPoint (point.x, point.y);
								CheckError (HIViewFindByID (HIViewGetRoot (controlHnd), new HIViewID (OSXConstants.kEventClassWindow, 1), ref pView), "HIViewFindByID ()");
								CheckError (HIViewGetSubviewHit (pView, ref hiPoint, true, ref rView));
								HIViewConvertPoint (ref hiPoint, pView, rView);
								if (mouseInWindow != rView && grabWindow == IntPtr.Zero) {
									msg.hwnd = mouseInWindow;
									msg.message = Msg.WM_MOUSE_LEAVE;
									carbonEvents.Enqueue (msg);
									msg.hwnd = rView;
									msg.message = Msg.WM_MOUSE_ENTER;
									carbonEvents.Enqueue (msg);
									mouseInWindow = rView;
									if (rView == IntPtr.Zero) {
										hover.timer.Enabled = false;
									} else {
//										hover.timer.Enabled = true;
									}
									hover.hwnd = rView;
									return -9874;
								}
								if (grabWindow == IntPtr.Zero)
									msg.hwnd = rView;
								else 
									msg.hwnd = grabWindow;
								NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);
								msg.message = Msg.WM_MOUSEMOVE;
								msg.lParam = (IntPtr) ((ushort)hiPoint.y << 16 | (ushort)hiPoint.x);
								msg.wParam = GetMousewParam (0);
								mouse_position.X = (int)hiPoint.x;
								mouse_position.Y = (int)hiPoint.y;
								carbonEvents.Enqueue (msg);
								hover.x = mouse_position.X;
								hover.y = mouse_position.Y;
								hover.timer.Interval = hover.interval;
								return -9874;
							}
							case OSXConstants.kEventMouseDragged: {
								QDPoint point = new QDPoint ();
								IntPtr pView = IntPtr.Zero;
								IntPtr rView = IntPtr.Zero;								
								ushort btn = 0;
								GetEventParameter (inEvent, 1835168878, 1835168878, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref btn);
								CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
								// Translate this point
								Rect bounds = new Rect ();
								CheckError (GetWindowBounds (controlHnd, 33, ref bounds), "GetWindowBounds ()");
								point.x = (short)(point.x-bounds.left);
								point.y = (short)(point.y-bounds.top);
								// Swizzle it so its pointed at the right control
								CGPoint hiPoint = new CGPoint (point.x, point.y);
								CheckError (HIViewFindByID (HIViewGetRoot (controlHnd), new HIViewID (OSXConstants.kEventClassWindow, 1), ref pView), "HIViewFindByID ()");
								CheckError (HIViewGetSubviewHit (pView, ref hiPoint, true, ref rView));
								HIViewConvertPoint (ref hiPoint, pView, rView);
								if (mouseInWindow != rView && grabWindow != IntPtr.Zero) {
									msg.hwnd = mouseInWindow;
									msg.message = Msg.WM_MOUSE_LEAVE;
									carbonEvents.Enqueue (msg);
									msg.hwnd = rView;
									msg.message = Msg.WM_MOUSE_ENTER;
									carbonEvents.Enqueue (msg);
									mouseInWindow = rView;
									if (rView == IntPtr.Zero) {
										hover.timer.Enabled = false;
									} else {
//										hover.timer.Enabled = true;
									}
									hover.hwnd = rView;
									return -9874;
								}
								if (grabWindow == IntPtr.Zero)
									msg.hwnd = rView;
								else 
									msg.hwnd = grabWindow;
								NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);
								msg.message = Msg.WM_MOUSEMOVE;
								msg.lParam = (IntPtr) ((ushort)hiPoint.y << 16 | (ushort)hiPoint.x);
								msg.wParam = GetMousewParam (0);
								mouse_position.X = (int)hiPoint.x;
								mouse_position.Y = (int)hiPoint.y;
								carbonEvents.Enqueue (msg);
								hover.x = mouse_position.X;
								hover.y = mouse_position.Y;
								hover.timer.Interval = hover.interval;
								return -9874;
							}
							default:
								Console.WriteLine (eventKind);
								break;
						}
						break;
					}
				}
				msg.message = Msg.WM_ENTERIDLE;
				carbonEvents.Enqueue (msg);
			}
			return -9874;
		}
		internal int ViewHandler (IntPtr inCallRef, IntPtr inEvent, IntPtr controlHnd) {
			uint eventClass = GetEventClass (inEvent);
			uint eventKind = GetEventKind (inEvent);
			MSG msg = new MSG ();
			msg.hwnd = controlHnd;
			lock (carbonEvents) {
				switch (eventClass) {
					case OSXConstants.kEventClassControl:
						switch (eventKind) {
							case OSXConstants.kEventControlDraw: {
								HIRect bounds = new HIRect ();
								HIViewGetBounds (controlHnd, ref bounds); 
								HandleData.Handle(controlHnd).AddToInvalidArea ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
								msg.message = Msg.WM_PAINT;
								msg.wParam = IntPtr.Zero;
								msg.lParam = IntPtr.Zero;
								if (view_backgrounds [controlHnd] != null) {
									Color c = (Color)(view_backgrounds [controlHnd]);
									IntPtr cgContext = IntPtr.Zero;
									GetEventParameter (inEvent, 1668183160, 1668183160, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref cgContext);

									CGContextSetRGBFillColor (cgContext, (float)c.R/255, (float)c.G/255, (float)c.B/255, (float)c.A/255);
									CGContextFillRect (cgContext, bounds);
								}
								carbonEvents.Enqueue (msg);
								return 0;
							}
							case OSXConstants.kEventControlSimulateHit: {
								Console.WriteLine ("Unicode thingie on {0:x}", msg.hwnd);
								return 0;
							}
							case OSXConstants.kEventControlBoundsChanged: {
/*
								HandleData.Handle(msg.hwnd).AddToInvalidArea ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
								msg.message = Msg.WM_PAINT;
								msg.wParam = IntPtr.Zero;
								msg.lParam = IntPtr.Zero;
								carbonEvents.Enqueue (msg);
*/
								return 0;
							}
							case OSXConstants.kEventControlClick: {
								IntPtr window = GetControlOwner (msg.hwnd);
								SetKeyboardFocus (window, msg.hwnd, 1); 
								return 0;
							}
							case OSXConstants.kEventControlSetFocusPart: {
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
						break;
				}
				msg.message = Msg.WM_ENTERIDLE;
				carbonEvents.Enqueue (msg);
			}
			
			return 0;
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
			
			lock (carbonEvents) {
				if (carbonEvents.Count <= 0) {
					if (Idle != null) 
						Idle (this, EventArgs.Empty);
					else if (timer_list.Count == 0) {
						ReceiveNextEvent (0, IntPtr.Zero, Convert.ToDouble ("0." + Timer.Minimum), true, ref evtRef);
						if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
							SendEventToEventTarget (evtRef, target);
							ReleaseEvent (evtRef);
						}
					} else {
						// Fix the hard loop here; we'll let carbon event loop do its stuff until the next timer timeout
						// Fixme; this string convert blows
						ReceiveNextEvent (0, IntPtr.Zero, Convert.ToDouble ("0." + NextTimeout (DateTime.Now))/1.5, true, ref evtRef);
						if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
							SendEventToEventTarget (evtRef, target);
							ReleaseEvent (evtRef);
						}
					}
					msg.hwnd = IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return getmessage_ret;
                                }
				msg = (MSG) carbonEvents.Dequeue ();
			}
			return getmessage_ret;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			bool res = false;

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

		public static void PostMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			MSG msg = new MSG();
			msg.hwnd = hwnd;
			msg.message = message;
			msg.wParam = wParam;
			msg.lParam = lParam;
			carbonEvents.Enqueue (msg);
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		internal override bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom) {
			if (Top) {
				HIViewSetZOrder (hWnd, 2, IntPtr.Zero);
				return true;
			} else if (!Bottom) {
				HIViewSetZOrder (hWnd, 2, AfterhWnd);
			} else {
				HIViewSetZOrder (hWnd, 1, IntPtr.Zero);
				return true;
			}
			return false;
		}

		internal override bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			HIViewSetZOrder (hWnd, 1, IntPtr.Zero);
			return true;
		}

		internal override bool Text(IntPtr handle, string text) {
			if (view_window_mapping [handle] != null) {
				CheckError (SetWindowTitleWithCFString ((IntPtr)(view_window_mapping [handle]), __CFStringMakeConstantString (text)));
			}
			CheckError (SetControlTitleWithCFString (handle, __CFStringMakeConstantString (text)));
			return true;
		}

		[MonoTODO]
		internal override bool GetText(IntPtr handle, out string text) {
			throw new NotImplementedException ();
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			// We dont have "windows" on OSX, we have views; so we toggle the view visibility here	
			HandleData data = HandleData.Handle(handle);

			if (view_window_mapping [handle] != null && visible == true) {
				ShowWindow ((IntPtr) view_window_mapping [handle]);
			}
			if (view_window_mapping [handle] != null && visible == false) {
				HideWindow ((IntPtr) view_window_mapping [handle]);
			}
			data.IsVisible = visible;

			CheckError (HIViewSetVisible (handle, visible));
/*
DEBUG THIS:
			IntPtr child = HIViewGetFirstSubview (handle);
			if (child != IntPtr.Zero) {
				HIViewSetVisible (child, visible);
				while (true) {
					child = HIViewGetNextView (child);
					if (child == IntPtr.Zero)
						break;
					HIViewSetVisible (child, visible);
				}
			}
*/
			
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			return HandleData.Handle(handle).IsVisible;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			if (HIViewGetSuperview (handle) != IntPtr.Zero)
				CheckError (HIViewRemoveFromSuperview (handle), "HIViewRemoveFromSuperview ()");
			CheckError (HIViewAddSubview (parent, handle));
//			SetVisible (handle, IsVisible (parent));
			return IntPtr.Zero;
		}

		internal override IntPtr GetParent(IntPtr handle) {
			return HIViewGetSuperview (handle);
		}

		internal override void GrabWindow(IntPtr hWnd, IntPtr confine_hwnd) {
			grabWindow = hWnd;
		}

		[MonoTODO]
		internal override void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			throw new NotImplementedException ();
		}

		internal override void ReleaseWindow(IntPtr hWnd) {
			grabWindow = IntPtr.Zero;
		}

		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect) {
			WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
			return true;
		}

		internal override void SetCursor(IntPtr window, IntPtr cursor) {
			SetThemeCursor ((uint) cursor);
		}

		internal override void ShowCursor(bool show) {
			if (show)
				CGDisplayShowCursor (CGMainDisplayID ());
			else
				CGDisplayHideCursor (CGMainDisplayID ());
		}

		[MonoTODO]
		internal override void OverrideCursor(IntPtr cursor) {
			throw new NotImplementedException ();
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

		[MonoTODO]
		internal override void DestroyCursor(IntPtr cursor) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			throw new NotImplementedException ();
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			CGDisplayMoveCursorToPoint (CGMainDisplayID (), new CGPoint (x, y));
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			QDPoint pt = new QDPoint ();
			GetGlobalMouse (ref pt);
			x = pt.x;
			y = pt.y;
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

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			CGPoint pt = new CGPoint ();
			Rect wBounds = new Rect ();
			pt.x = x;
			pt.y = y;

			GetWindowBounds (GetControlOwner (handle), 32, ref wBounds);
			HIViewConvertPoint (ref pt, handle, IntPtr.Zero);

			x = (int)(pt.x+wBounds.left);
			y = (int)(pt.y+wBounds.top);
		}

		[MonoTODO]
		internal override void SendAsyncMethod (AsyncMethodData method) {
			throw new NotImplementedException ();
		}

		private IntPtr GetMousewParam(int Delta) {
			int     result = 0;

			if ((mouse_state & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((mouse_state & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((mouse_state & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}

			return (IntPtr)result;
                }

		private int NextTimeout (DateTime now)
		{
			int timeout = 0x7FFFFFF;
			lock (timer_list) {
				foreach (Timer timer in timer_list) {
					int next = (int) (timer.Expires - now).TotalMilliseconds;
					if (next < 0)
						return 0;
					if (next < timeout)
						timeout = next;
				}
			}
			if (timeout < Timer.Minimum)
				timeout = Timer.Minimum;

			return timeout;
		}
		private void CheckTimers (DateTime now)
                {
			lock (timer_list) {
				int count = timer_list.Count;
				if (count == 0)
					return;
				for (int i = 0; i < timer_list.Count; i++) {
					Timer timer = (Timer) timer_list [i];
					if (timer.Enabled && timer.Expires <= now) {
						timer.FireTick ();
						timer.Update (now);
					}
				}
			}
		}

		internal override void SetTimer (Timer timer) {
			lock (timer_list) {
				timer_list.Add (timer);
			}
		}

		internal override void KillTimer (Timer timer)
		{
			lock (timer_list) {
				timer_list.Remove (timer);
			}
		}

		internal void InvertCaret () {
			IntPtr window = GetControlOwner (caret.hwnd);
			SetPortWindowPort (window);
			Rect r = new Rect ();
			GetWindowPortBounds (window, ref r);
			r.top += (short)caret.y;
			r.left += (short)caret.x;
			r.bottom = (short)(r.top + caret.height);
			r.right = (short)(r.left + caret.width);
			InvertRect (ref r);
		}
		internal void ShowCaret () {
			if (caret.on)
				return;
			caret.on = true;
			InvertCaret ();
		}

		internal void HideCaret () {
			if (!caret.on)
				return;
			caret.on = false;
			// Fixme; this will kill what was underneath it before
			InvertCaret ();
		}

		internal override void CreateCaret (IntPtr hwnd, int width, int height) {
			if (caret.hwnd != IntPtr.Zero)
				DestroyCaret (caret.hwnd);

			caret.hwnd = hwnd;
			caret.width = width;
			caret.height = height;
			caret.visible = 0;
			caret.on = false;
		}

		internal override void DestroyCaret (IntPtr hwnd) {
			if (caret.hwnd == hwnd) {
				if (caret.visible == 1) {
					caret.timer.Stop ();
					HideCaret ();
				}
				caret.hwnd = IntPtr.Zero;
				caret.visible = 0;
				caret.on = false;
			}
		}

		internal override void SetCaretPos (IntPtr hwnd, int x, int y) {
			if (caret.hwnd == hwnd) {
				CGPoint cpt = new CGPoint ();
				cpt.x = x;
				cpt.y = y;
				HIViewConvertPoint (ref cpt, hwnd, IntPtr.Zero);
				caret.timer.Stop ();
				HideCaret ();
				caret.x = (int)cpt.x;
				caret.y = (int)cpt.y-23;
				if (caret.visible == 1) {
					ShowCaret ();
					caret.timer.Start ();
				}
			}
		}

		internal override void CaretVisible (IntPtr hwnd, bool visible) {
			if (caret.hwnd == hwnd) {
				if (visible) {
					if (caret.visible < 1) {
						caret.visible++;
						caret.on = false;
						if (caret.visible == 1) {
							ShowCaret ();
							caret.timer.Start ();
						}
					}
				} else {
					caret.visible--;
					if (caret.visible == 0) {
						caret.timer.Stop ();
						HideCaret ();
					}
				}
			}
		}

		internal override void SetFocus(IntPtr hwnd) {
			IntPtr window = GetControlOwner (hwnd);
			SetKeyboardFocus (window, hwnd, 1); 
		}

		internal override IntPtr GetActive() {
			foreach (DictionaryEntry entry in view_window_mapping)
				if (IsWindowActive ((IntPtr)(entry.Value)))
					return (IntPtr)(entry.Key);

			return IntPtr.Zero;
		}

		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			return GetFontMetrics(g.GetHdc(), font.ToHfont(), out ascent, out descent);
		}

		internal override void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool clear) {
			throw new NotImplementedException("Need to implement the overload that provides the rectangle for ScrollWindow");
		}

		internal override void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool clear) {
			IntPtr rect = IntPtr.Zero;
			HIRect vBounds = new HIRect ();
                        HIViewGetBounds (hwnd, ref vBounds);
			HIViewConvertRect (ref vBounds, hwnd, IntPtr.Zero);
			SetRect (ref rect, (short)vBounds.origin.x, (short)(vBounds.origin.y-22), (short)(vBounds.origin.x+vBounds.size.width), (short)(vBounds.origin.y+vBounds.size.height-22));
			ScrollRect (ref rect, (short)XAmount, (short)-YAmount, IntPtr.Zero);

			if (YAmount > 0) {
				Invalidate (hwnd, new Rectangle (0, YAmount, (int)vBounds.size.width, (int)(vBounds.size.height)), clear);
			} else if (YAmount < 0) {
				Invalidate (hwnd, new Rectangle (0, 0, (int)vBounds.size.width, -YAmount), clear);
			}

			if (XAmount > 0) {
				Invalidate (hwnd, new Rectangle (0, 0, XAmount, (int)vBounds.size.height), clear);
			} else if (XAmount < 0) {
				Invalidate (hwnd, new Rectangle ((int)(vBounds.size.width+XAmount), 0, (int)vBounds.size.width, (int)vBounds.size.height), clear);
			}
		}


		[MonoTODO]
		internal override int KeyboardSpeed {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		internal override int KeyboardDelay {
			get {
				throw new NotImplementedException ();
			}
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

		internal void CheckError (int result, string error) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::" + error + "() Carbon subsystem threw an error: " + result);
		}
		internal void CheckError (int result) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::Carbon subsystem threw an error: " + result);
		}

		// Santa's little helper 
		static void Where()
		{
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetNeedsDisplayInRegion (IntPtr view, IntPtr rgn, bool needsDisplay);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetSubviewHit (IntPtr contentView, ref CGPoint point, bool tval, ref IntPtr outPtr);
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
                static extern int InstallEventHandler (IntPtr window, CarbonEventHandler handlerProc, uint numtypes, EventTypeSpec [] typeList, IntPtr userData, IntPtr handlerRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlOwner (IntPtr aView);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int ActivateWindow (IntPtr windowHnd, bool inActivate);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern bool IsWindowActive (IntPtr windowHnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetKeyboardFocus (IntPtr windowHdn, IntPtr cntrlHnd, ushort partcode);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetKeyboardFocus (IntPtr controlHnd, ref IntPtr cntrl);

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
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CFRelease (IntPtr wHnd);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr NewRgn ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void SetRectRgn (IntPtr rgn, short left, short top, short right, short bottom);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void DisposeRgn (IntPtr rgn);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void ExitToShell ();
		
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
