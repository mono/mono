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

// FIXME: Caret can have colors?
		internal static Pen caretOnPen = new Pen (Color.FromArgb (0,0,0));
		internal static SolidBrush caretOnBrush = new SolidBrush (Color.FromArgb (0,0,0));
// FIXME: When we figure out HIView backgrounds this will change on a per-caret basis
		internal static Pen caretOffPen = new Pen (Color.FromArgb (255,255,255));
		internal static SolidBrush caretOffBrush = new SolidBrush (Color.FromArgb (255,255,255));

		internal static IntPtr mouseInWindow;
		private static Hashtable handle_data;
		private static Queue carbonEvents;
		private CarbonEventHandler viewEventHandler;
		private CarbonEventHandler windowEventHandler;
		private static Hashtable view_window_mapping;
		private static IntPtr grabWindow;
		private static IntPtr fosterParent;

		private static EventTypeSpec [] viewEvents = new EventTypeSpec [] {
									new EventTypeSpec (1668183148, 7), 
									new EventTypeSpec (1668183148, 13), 
									new EventTypeSpec (1668183148, 2), 
									new EventTypeSpec (1668183148, 4) 
									};
		private static EventTypeSpec [] windowEvents = new EventTypeSpec[] {
									new EventTypeSpec (1836021107, 1),
									new EventTypeSpec (1836021107, 2),
									new EventTypeSpec (1836021107, 5),
									new EventTypeSpec (1836021107, 6),
									new EventTypeSpec (1836021107, 10),
									new EventTypeSpec (2003398244, 27),
									new EventTypeSpec (1801812322, 1),
									new EventTypeSpec (1801812322, 2),
									new EventTypeSpec (1801812322, 3)
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

		private XplatUIOSX() {
			viewEventHandler = new CarbonEventHandler (ViewHandler);
			windowEventHandler = new CarbonEventHandler (WindowHandler);
			ref_count = 0;
			mouseInWindow = IntPtr.Zero;
			handle_data = new Hashtable ();
			carbonEvents = new Queue ();
			grabWindow = IntPtr.Zero;
			view_window_mapping = new Hashtable ();
			mouse_state=MouseButtons.None;
			mouse_position=Point.Empty;
			
			IntPtr rect = IntPtr.Zero;
			SetRect (ref rect, (short)0, (short)0, (short)0, (short)0);
			CheckError (CreateNewWindow (6, 33554432 | 31 | 524288 , ref rect, ref fosterParent), "CreateFosterParent ()");
			timer_list = new ArrayList ();

			caret.timer = new Timer ();
			caret.timer.Interval = 500;
			caret.timer.Tick += new EventHandler (CaretCallback);
		}

		[MonoTODO]
		~XplatUIOSX() {
			throw new NotImplementedException ();
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

		[MonoTODO]
		public int Reference {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		private void MouseHover (object sender, EventArgs e) {
			throw new NotImplementedException ();
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

		[MonoTODO]
		internal override IntPtr InitializeDriver() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void ShutdownDriver(IntPtr token) {
			throw new NotImplementedException ();
		}

		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			Console.WriteLine("XplatUIOSX.Exit");
		}

		[MonoTODO]
		internal override void GetDisplaySize(out Size size) {
			// FIXME:
			size = new Size (1024, 768);
//			throw new NotImplementedException ();
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
					CheckError (HIViewFindByID (HIViewGetRoot (fosterParent), new HIViewID (2003398244, 1), ref parentHnd), "HIViewFindByID ()");
				} else if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
					// This is a popup window that will be real.
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
				IntPtr rect = IntPtr.Zero;
				IntPtr viewHnd = IntPtr.Zero;
				SetRect (ref rect, (short)cp.X, (short)cp.Y, (short)(cp.Width+cp.X), (short)(cp.Height+cp.Y));
				CheckError (CreateNewWindow (6, 33554432 | 31 | 524288 , ref rect, ref windowHnd), "CreateNewWindow ()");
				CheckError (InstallEventHandler (GetWindowEventTarget (windowHnd), windowEventHandler, (uint)windowEvents.Length, windowEvents, windowHnd, IntPtr.Zero), "InstallEventHandler ()");
				CheckError (HIViewFindByID (HIViewGetRoot (windowHnd), new HIViewID (2003398244, 1), ref viewHnd), "HIViewFindByID ()");
				parentHnd = viewHnd;
			}
			HIRect r = new HIRect (0, 0, cp.Width, cp.Height);
			CheckError (HIObjectCreate (__CFStringMakeConstantString ("com.apple.hiview"), 0, ref hWnd), "HIObjectCreate ()");
			CheckError (InstallEventHandler (GetControlEventTarget (hWnd), viewEventHandler, (uint)viewEvents.Length, viewEvents, hWnd, IntPtr.Zero), "InstallEventHandler ()");
			CheckError (HIViewChangeFeatures (hWnd, 1 << 1, 0), "HIViewChangeFeatures ()");
			CheckError (HIViewSetFrame (hWnd, ref r), "HIViewSetFrame ()");
			if (parentHnd != IntPtr.Zero && parentHnd != hWnd) {
				CheckError (HIViewAddSubview (parentHnd, hWnd), "HIViewAddSubview ()");
				if ((cp.Style & (int)(WindowStyles.WS_VISIBLE))!=0) {
					CheckError (HIViewSetVisible (hWnd, true), "HIViewSetVisible ()");
				} else {
					CheckError (HIViewSetVisible (hWnd, IsVisible (parentHnd)), "HIViewSetVisible ()");
				}
			}
			if (realWindow) {
				view_window_mapping [hWnd] = windowHnd;
				if ((cp.Style & (int)(WindowStyles.WS_VISIBLE))!=0) {
					CheckError (ShowWindow (windowHnd));
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

		[MonoTODO]
		internal override void DestroyWindow(IntPtr handle) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void RefreshWindow(IntPtr handle) {
			if (handle_data [handle] == null)
				handle_data [handle] = new HandleData ();
			HIRect bounds = new HIRect ();
			HIViewGetBounds (handle, ref bounds);
			((HandleData) handle_data [handle]).AddToInvalidArea ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
			MSG msg = new MSG ();
			msg.message = Msg.WM_PAINT;
			msg.hwnd = handle;
			msg.wParam = IntPtr.Zero;
			msg.lParam = IntPtr.Zero;
			if ((int)bounds.size.width > 0 && (int)bounds.size.height > 0)
				carbonEvents.Enqueue (msg);
/*
Console.WriteLine ("Invalidating {0:x}", (int)handle);
			if (HIViewGetSuperview (handle) != IntPtr.Zero)
				HIViewSetNeedsDisplay (HIViewGetSuperview (handle), true);
			else
				HIViewSetNeedsDisplay (handle, true);
*/
		}

		[MonoTODO("Find a way to make all the views do this; not just the window view")]
		internal override void SetWindowBackground(IntPtr handle, Color color) {
			if (view_window_mapping [handle] != null) {
				RGBColor backColor = new RGBColor ();
				backColor.red = (short)(color.R * 257); 
				backColor.green = (short)(color.G * 257);
				backColor.blue = (short)(color.B * 257);

				CheckError (SetWindowContentColor ((IntPtr) view_window_mapping [handle], ref backColor));
			}
		}

		[MonoTODO]
		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			PaintEventArgs  paint_event;

			HandleData data = (HandleData) handle_data [handle];
			if (data == null) {
				throw new Exception ("null data on paint event start: " + handle);
			}

			if (caret.visible == 1) {
				caret.paused = true;
				HideCaret ();
			}

			data.DeviceContext = Graphics.FromHwnd (handle);
			paint_event = new PaintEventArgs((Graphics)data.DeviceContext, data.InvalidArea);

			return paint_event;
		}

		[MonoTODO]
		internal override void PaintEventEnd(IntPtr handle) {
			HandleData data = (HandleData) handle_data [handle];
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

		[MonoTODO]
		internal override void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			HIRect r = new HIRect ();
			CheckError (HIViewGetFrame (handle, ref r), "HIViewGetFrame ()");
			x = (int)r.origin.x;
			y = (int)r.origin.y;
			width = (int)r.size.width;
			height = (int)r.size.height;
			client_width = width;
			client_height = height;
		}

		[MonoTODO]
		internal override void Activate(IntPtr handle) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void EnableWindow(IntPtr handle, bool Enable) {
			//throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void SetModal(IntPtr handle, bool Modal) {
			// FIXME: What do we do here on OSX?
			return;
		}

		[MonoTODO]
		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			((HandleData) handle_data [handle]).AddToInvalidArea (rc.X, rc.Y, rc.Width, rc.Height);
			MSG msg = new MSG ();
			msg.hwnd = handle;
			msg.message = Msg.WM_PAINT;
			msg.wParam = IntPtr.Zero;
			msg.lParam = IntPtr.Zero;
			carbonEvents.Enqueue (msg);
		}

		[MonoTODO]
		internal override IntPtr DefWndProc(ref Message msg) {
			return IntPtr.Zero;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
                        Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
                        Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		[MonoTODO]
		internal override void DoEvents() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			throw new NotImplementedException ();
		}

		internal int WindowHandler (IntPtr inCallRef, IntPtr inEvent, IntPtr controlHnd) {
			int eventClass = GetEventClass (inEvent);
			int eventKind = GetEventKind (inEvent);
			MSG msg = new MSG ();
			msg.hwnd = IntPtr.Zero; 
			lock (carbonEvents) {
				switch (eventClass) {
					// keyboard
					case 1801812322: {
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
							case 1: {
								msg.message = Msg.WM_KEYDOWN;
								break;
							}
							// repeat
							case 2: {
								msg.message = Msg.WM_KEYDOWN;
								break;
							}
							// keyup
							case 3: {
								msg.message = Msg.WM_KEYUP;
								break;
							}
						}
						carbonEvents.Enqueue (msg);
						return -9874;
					}
					case 2003398244: {
						switch (eventKind) {
							case 27: {
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
					case 1836021107: {
						switch (eventKind) {
							case 1: 
							case 2: {
								QDPoint point = new QDPoint ();
								IntPtr pView = IntPtr.Zero;
								IntPtr rView = IntPtr.Zero;
								int wParam = 0;
								ushort btn = 0;
								GetEventParameter (inEvent, 1835168878, 1835168878, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref btn);
								CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
								SetPortWindowPort (controlHnd);
								GlobalToLocal (ref point);
								// Translate this point
								Rect bounds = new Rect ();
								CheckError (GetWindowBounds (controlHnd, 32, ref bounds), "GetWindowBounds ()");
								// Swizzle it so its pointed at the right control
								CGPoint hiPoint = new CGPoint (point.y, point.x);
								CheckError (HIViewFindByID (HIViewGetRoot (controlHnd), new HIViewID (2003398244, 1), ref pView), "HIViewFindByID ()");
								CheckError (HIViewGetSubviewHit (pView, ref hiPoint, true, ref rView));
								HIViewConvertPoint (ref hiPoint, pView, rView);
								if (grabWindow == IntPtr.Zero)
									msg.hwnd = rView;
								else
									msg.hwnd = grabWindow;
								switch (btn) {
									case 1:
										if (eventKind == 1) {
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
										if (eventKind == 1) {
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
										if (eventKind == 1) {
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
							case 5: {
								QDPoint point = new QDPoint ();
								IntPtr pView = IntPtr.Zero;
								IntPtr rView = IntPtr.Zero;								
								ushort btn = 0;
								GetEventParameter (inEvent, 1835168878, 1835168878, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref btn);
								CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
								SetPortWindowPort (controlHnd);
								GlobalToLocal (ref point);
								// Translate this point
								Rect bounds = new Rect ();
								CheckError (GetWindowBounds (controlHnd, 32, ref bounds), "GetWindowBounds ()");
								// Swizzle it so its pointed at the right control
								CGPoint hiPoint = new CGPoint (point.y, point.x);
								CheckError (HIViewFindByID (HIViewGetRoot (controlHnd), new HIViewID (2003398244, 1), ref pView), "HIViewFindByID ()");
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
									return -9874;
								}
								return -9874;
							}
							case 6: {
								QDPoint point = new QDPoint ();
								IntPtr pView = IntPtr.Zero;
								IntPtr rView = IntPtr.Zero;								
								ushort btn = 0;
								GetEventParameter (inEvent, 1835168878, 1835168878, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref btn);
								CheckError (GetEventParameter (inEvent, 1835822947, 1363439732, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
								SetPortWindowPort (controlHnd);
								GlobalToLocal (ref point);
								// Translate this point
								Rect bounds = new Rect ();
								CheckError (GetWindowBounds (controlHnd, 32, ref bounds), "GetWindowBounds ()");
								// Swizzle it so its pointed at the right control
								CGPoint hiPoint = new CGPoint (point.y, point.x);
								CheckError (HIViewFindByID (HIViewGetRoot (controlHnd), new HIViewID (2003398244, 1), ref pView), "HIViewFindByID ()");
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
									return -9874;
								}
								if (grabWindow == IntPtr.Zero)
									msg.hwnd = rView;
								else 
									msg.hwnd = grabWindow;
								msg.message = Msg.WM_MOUSEMOVE;
								msg.lParam = (IntPtr) ((ushort)hiPoint.y << 16 | (ushort)hiPoint.x);
								msg.wParam = GetMousewParam (0);
								mouse_position.X = (int)hiPoint.x;
								mouse_position.Y = (int)hiPoint.y;
								carbonEvents.Enqueue (msg);
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
			int eventClass = GetEventClass (inEvent);
			int eventKind = GetEventKind (inEvent);
			MSG msg = new MSG ();
			msg.hwnd = controlHnd;
			lock (carbonEvents) {
				switch (eventClass) {
					case 1668183148:
						switch (eventKind) {
							case 4: {
								if (handle_data [controlHnd] == null)
									handle_data [controlHnd] = new HandleData ();
								HIRect bounds = new HIRect ();
								HIViewGetBounds (controlHnd, ref bounds); 
								((HandleData) handle_data [controlHnd]).AddToInvalidArea ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
								msg.message = Msg.WM_PAINT;
								msg.wParam = IntPtr.Zero;
								msg.lParam = IntPtr.Zero;
//								DispatchMessage (ref msg);
								carbonEvents.Enqueue (msg);
								return 0;
							}
							case 2: {
								Console.WriteLine ("Unicode thingie on {0:x}", msg.hwnd);
								return 0;
							}
							case 13: {
								IntPtr window = GetControlOwner (msg.hwnd);
								SetKeyboardFocus (window, msg.hwnd, 1); 
								return 0;
							}
							case 7: {
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
						ReceiveNextEvent (0, IntPtr.Zero, 0x7FFFFFFF, true, ref evtRef);
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
					return true;
                                }
				msg = (MSG) carbonEvents.Dequeue ();
			}
			return true;
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

		[MonoTODO]
		internal override bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			// FIXME
			return true;
		}

		internal override bool Text(IntPtr handle, string text) {
			// FIXME: We need to set the window as well if this is the root control.
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
			HandleData data = (HandleData) handle_data [handle];

			if (data == null) {
				data = new HandleData ();
				handle_data [handle] = data;
			}
			if (view_window_mapping [handle] != null && visible == true) {
				ShowWindow ((IntPtr) view_window_mapping [handle]);
			}
			if (view_window_mapping [handle] != null && visible == false) {
				HideWindow ((IntPtr) view_window_mapping [handle]);
			}
			data.IsVisible = visible;

			CheckError (HIViewSetVisible (handle, visible));
			// We have to manually reset all children as well
			
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			HandleData data = (HandleData) handle_data [handle];

			if (data == null || data.IsVisible == true) {
				return true;
			}
			return false;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			if (HIViewGetSuperview (handle) != IntPtr.Zero)
				CheckError (HIViewRemoveFromSuperview (handle), "HIViewRemoveFromSuperview ()");
			CheckError (HIViewAddSubview (parent, handle));
			SetVisible (handle, IsVisible (parent));
			return IntPtr.Zero;
		}

		internal override IntPtr GetParent(IntPtr handle) {
			return HIViewGetSuperview (handle);
		}

		[MonoTODO]
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

		[MonoTODO]
		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			throw new NotImplementedException ();
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			x = mouse_position.X;
			y = mouse_position.Y;
		}

		[MonoTODO]
		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y) {
//			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
//			throw new NotImplementedException ();
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

		internal void DrawText () {
			IntPtr cgContext = IntPtr.Zero;
			IntPtr window = GetControlOwner (caret.hwnd);
			// Get the port of the window
			IntPtr port = GetWindowPort (window);
			// Create a CGContext ref
			CreateCGContextForPort (port, ref cgContext);

			// Get the bounds of the window
			Rect wBounds = new Rect ();
			GetWindowBounds (window, 32, ref wBounds);

			// Get the bounds of the view
			HIRect vBounds = new HIRect ();
			HIViewGetBounds (caret.hwnd, ref vBounds);

			// Convert the view local bounds to window coordinates
			HIViewConvertRect (ref vBounds, caret.hwnd, IntPtr.Zero);
			CGContextTranslateCTM (cgContext, vBounds.origin.x, (wBounds.bottom-wBounds.top)-(vBounds.origin.y+vBounds.size.height));
			CGContextSelectFont (cgContext, "Zapfino", 24, 1);
			CGContextSetRGBFillColor (cgContext, (float)0.25, (float)0.25, (float)0.25, (float)0.5);
			CGContextSetRGBStrokeColor (cgContext, (float)0.25, (float)0.25, (float)0.25, (float)0.5);
			CGContextSetTextDrawingMode (cgContext, 0);
			CGAffineTransform ctm = CGContextGetTextMatrix (cgContext);
			ctm.a = 1.0f;
			ctm.d = -1.0f;
			CGContextSetTextMatrix (cgContext, ctm);
			CGContextShowTextAtPoint (cgContext, 200, 200, "Quartz Text", 11);
			CGContextFlush (cgContext);
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
				caret.timer.Stop ();
				HideCaret ();
				caret.x = x;
				caret.y = y;
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

		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			return GetFontMetrics(g.GetHdc(), font.ToHfont(), out ascent, out descent);
		}

		[MonoTODO]
		internal override void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount) {
			;
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

		internal void CheckError (int result, string error) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::" + error + "() Carbon subsystem threw an error: " + result);
		}
		internal void CheckError (int result) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::Carbon subsystem threw an error: " + result);
		}

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
		internal static extern int HIViewConvertRect (ref HIRect r, IntPtr a, IntPtr b);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void SetRect (ref IntPtr r, short left, short top, short right, short bottom);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int CreateEvent (IntPtr allocator, uint classid, uint kind, double when, uint attributes, ref IntPtr outEvent);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                static extern int InstallEventHandler (IntPtr window, CarbonEventHandler handlerProc, uint numtypes, EventTypeSpec [] typeList, IntPtr userData, IntPtr handlerRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlOwner (IntPtr aView);
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
		static extern int GetEventClass (IntPtr eventRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventKind (IntPtr eventRef);
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
		internal static extern IntPtr GetWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetPortWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GlobalToLocal (ref QDPoint outData);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CreateNewWindow (int klass, uint attributes, ref IntPtr r, ref IntPtr window);
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

		[DllImport ("gdiplus", EntryPoint="GetFontMetrics")]
		internal extern static bool GetFontMetrics(IntPtr graphicsObject, IntPtr nativeObject, out int ascent, out int descent);
	}

	internal struct CGSize {
		public float width;
		public float height;

		public CGSize (int w, int h) {
			this.width = (float)w;
			this.height = (float)h;
		}
	}

	internal struct QDPoint {
		public short x;
		public short y;

		public QDPoint (short x, short y) {
			this.x = x;
			this.y = y;
		}
	}
	internal struct CGPoint {
		public float x;
		public float y;

		public CGPoint (int x, int y) {
			this.x = (float)x;
			this.y = (float)y;
		}
	}

	internal struct HIRect {
		public CGPoint origin;
		public CGSize size;

		public HIRect (int x, int y, int w, int h) {
			this.origin = new CGPoint (x, y);
			this.size = new CGSize (w, h);
		}
	}

	internal struct HIViewID {
		public uint type;
		public uint id;

		public HIViewID (uint type, uint id) {
			this.type = type;
			this.id = id;
		}
	}
	
	internal struct EventTypeSpec
        {
		public UInt32 eventClass;
		public UInt32 eventKind;

		public EventTypeSpec (UInt32 eventClass, UInt32 eventKind)
		{
			this.eventClass = eventClass;
			this.eventKind = eventKind;
		}
	}
	
	internal struct CarbonEvent
        {
		public IntPtr hWnd;
		public IntPtr evt;

		public CarbonEvent (IntPtr hWnd, IntPtr evt)
		{
			this.hWnd = hWnd;
			this.evt = evt;
		}
	}
	
	internal struct RGBColor
	{
		public short red;
		public short green;
		public short blue;
	}

	internal struct Rect
	{
		public short top;
		public short left;
		public short bottom;
		public short right;
	}

	internal struct OSXCaret
	{
		internal Timer timer;
		internal IntPtr hwnd;
		internal int x;
		internal int y;
		internal int width;
		internal int height;
		internal int visible;
		internal bool on;
		internal bool paused;
	}

	internal struct CGAffineTransform
	{
		internal float a;
		internal float b;
		internal float c;
		internal float d;
		internal float tx;
		internal float ty;
	}
}	
