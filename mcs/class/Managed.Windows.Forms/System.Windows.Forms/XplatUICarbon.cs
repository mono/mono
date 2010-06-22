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
// Copyright (c) 2004-2007 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@novell.com>
//
//

using System;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Carbon = System.Windows.Forms.CarbonInternal;

/// Carbon Version
namespace System.Windows.Forms {
	internal delegate Rectangle [] HwndDelegate (IntPtr handle);

	internal class XplatUICarbon : XplatUIDriver {
		#region Local Variables
		// General driver variables
		private static XplatUICarbon Instance;
		private static int RefCount;
		private static bool themes_enabled;

		// Internal members available to the event handler sub-system
		internal static IntPtr FocusWindow;
		internal static IntPtr ActiveWindow;
		internal static IntPtr ReverseWindow;
		internal static IntPtr CaretWindow;

		internal static Hwnd MouseHwnd;

		internal static MouseButtons MouseState;
		internal static Carbon.Hover Hover;

		internal static HwndDelegate HwndDelegate = new HwndDelegate (GetClippingRectangles);
		// Instance members
		internal Point mouse_position;

		// Event handlers
		internal Carbon.ApplicationHandler ApplicationHandler;
		internal Carbon.ControlHandler ControlHandler;
		internal Carbon.HIObjectHandler HIObjectHandler;
		internal Carbon.KeyboardHandler KeyboardHandler;
		internal Carbon.MouseHandler MouseHandler;
		internal Carbon.WindowHandler WindowHandler;
		
		// Carbon Specific
		internal static GrabStruct Grab;
		internal static Carbon.Caret Caret;
		private static Carbon.Dnd Dnd;
		private static Hashtable WindowMapping;
		private static Hashtable HandleMapping;
		private static IntPtr FosterParent;
		private static IntPtr Subclass;
		private static int MenuBarHeight;
		internal static ArrayList UtilityWindows;

		// Message loop
		private static Queue MessageQueue;
		private static bool GetMessageResult;

		private static bool ReverseWindowMapped;

		// Timers
		private ArrayList TimerList;
		private static bool in_doevents;
		
		static readonly object instancelock = new object ();
		static readonly object queuelock = new object ();
		
		// Event Handlers
		internal override event EventHandler Idle;
		#endregion
		
		#region Constructors
		private XplatUICarbon() {

			RefCount = 0;
			TimerList = new ArrayList ();
			in_doevents = false;
			MessageQueue = new Queue ();
			
			Initialize ();
		}

		~XplatUICarbon() {
			// FIXME: Clean up the FosterParent here.
		}
		#endregion

		#region Singleton specific code
		public static XplatUICarbon GetInstance() {
			lock (instancelock) {
				if (Instance == null) {
					Instance = new XplatUICarbon ();
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
		internal void AddExpose (Hwnd hwnd, bool client, Carbon.HIRect rect) {
			AddExpose (hwnd, client, (int) rect.origin.x, (int) rect.origin.y, (int) rect.size.width, (int) rect.size.height);
		}
		
		internal void AddExpose (Hwnd hwnd, bool client, Rectangle rect) {
			AddExpose (hwnd, client, (int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height);
		}

		internal void FlushQueue () {
			CheckTimers (DateTime.UtcNow);
			lock (queuelock) {
				while (MessageQueue.Count > 0) {
					object queueobj = MessageQueue.Dequeue ();
					if (queueobj is GCHandle) {
						XplatUIDriverSupport.ExecuteClientMessage((GCHandle)queueobj);
					} else {
						MSG msg = (MSG)queueobj;
						NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
					}
				}
			}
		}

		internal static Rectangle [] GetClippingRectangles (IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			if (hwnd == null)
				return null;
 			if (hwnd.Handle != handle)
				return new Rectangle [] {hwnd.ClientRect};

			return (Rectangle []) hwnd.GetClippingRectangles ().ToArray (typeof (Rectangle));
		}

		internal IntPtr GetMousewParam(int Delta) {
			int	 result = 0;

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

		internal IntPtr HandleToWindow (IntPtr handle) {
			if (HandleMapping [handle] != null)
				return (IntPtr) HandleMapping [handle];
			return IntPtr.Zero;
		}

		internal void Initialize () {
			// Initialize the event handlers	
			Carbon.EventHandler.Driver = this;
			ApplicationHandler = new Carbon.ApplicationHandler (this);
			ControlHandler = new Carbon.ControlHandler (this);
			HIObjectHandler = new Carbon.HIObjectHandler (this);
			KeyboardHandler = new Carbon.KeyboardHandler (this);
			MouseHandler = new Carbon.MouseHandler (this);
			WindowHandler = new Carbon.WindowHandler (this);
			
			// Initilize the mouse controls
			Hover.Interval = 500;
			Hover.Timer = new Timer ();
			Hover.Timer.Enabled = false;
			Hover.Timer.Interval = Hover.Interval;
			Hover.Timer.Tick += new EventHandler (HoverCallback);
			Hover.X = -1;
			Hover.Y = -1;
			MouseState = MouseButtons.None;
			mouse_position = Point.Empty;
				
			// Initialize the Caret
			Caret.Timer = new Timer ();
			Caret.Timer.Interval = 500;
			Caret.Timer.Tick += new EventHandler (CaretCallback);

			// Initialize the D&D
			Dnd = new Carbon.Dnd (); 
			
			// Initialize the Carbon Specific stuff
			WindowMapping = new Hashtable ();
			HandleMapping = new Hashtable ();
			UtilityWindows = new ArrayList ();
			
			// Initialize the FosterParent
			Carbon.Rect rect = new Carbon.Rect ();
			SetRect (ref rect, (short)0, (short)0, (short)0, (short)0);
			Carbon.ProcessSerialNumber psn = new Carbon.ProcessSerialNumber();

			GetCurrentProcess( ref psn );
			TransformProcessType (ref psn, 1);
			SetFrontProcess (ref psn);

			HIObjectRegisterSubclass (__CFStringMakeConstantString ("com.novell.mwfview"), __CFStringMakeConstantString ("com.apple.hiview"), 0, Carbon.EventHandler.EventHandlerDelegate, (uint)Carbon.EventHandler.HIObjectEvents.Length, Carbon.EventHandler.HIObjectEvents, IntPtr.Zero, ref Subclass);

			Carbon.EventHandler.InstallApplicationHandler ();

			CreateNewWindow (Carbon.WindowClass.kDocumentWindowClass, Carbon.WindowAttributes.kWindowStandardHandlerAttribute | Carbon.WindowAttributes.kWindowCloseBoxAttribute | Carbon.WindowAttributes.kWindowFullZoomAttribute | Carbon.WindowAttributes.kWindowCollapseBoxAttribute | Carbon.WindowAttributes.kWindowResizableAttribute | Carbon.WindowAttributes.kWindowCompositingAttribute, ref rect, ref FosterParent);
			
			CreateNewWindow (Carbon.WindowClass.kOverlayWindowClass, Carbon.WindowAttributes.kWindowNoUpdatesAttribute | Carbon.WindowAttributes.kWindowNoActivatesAttribute, ref rect, ref ReverseWindow);
			CreateNewWindow (Carbon.WindowClass.kOverlayWindowClass, Carbon.WindowAttributes.kWindowNoUpdatesAttribute | Carbon.WindowAttributes.kWindowNoActivatesAttribute, ref rect, ref CaretWindow);
			
			// Get some values about bar heights
			Carbon.Rect structRect = new Carbon.Rect ();
			Carbon.Rect contentRect = new Carbon.Rect ();
			GetWindowBounds (FosterParent, 32, ref structRect);
			GetWindowBounds (FosterParent, 33, ref contentRect);
			
			MenuBarHeight = GetMBarHeight ();
			
			// Focus
			FocusWindow = IntPtr.Zero;
			
			// Message loop
			GetMessageResult = true;
			
			ReverseWindowMapped = false;
		}
		
		internal void PerformNCCalc(Hwnd hwnd) {
			XplatUIWin32.NCCALCSIZE_PARAMS  ncp;
			IntPtr ptr;
			Rectangle rect;

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

			rect = TranslateClientRectangleToQuartzClientRectangle (hwnd);

			if (hwnd.visible) {
				Carbon.HIRect r = new Carbon.HIRect (rect.X, rect.Y, rect.Width, rect.Height);
				HIViewSetFrame (hwnd.client_window, ref r);
			}
	
			AddExpose (hwnd, false, 0, 0, hwnd.Width, hwnd.Height);
		}
		
		internal void ScreenToClient(IntPtr handle, ref Carbon.QDPoint point) {
			int x = (int) point.x;
			int y = (int) point.y;

			ScreenToClient (handle, ref x, ref y);

			point.x = (short) x;
			point.y = (short) y;
		}
		
		internal static Rectangle TranslateClientRectangleToQuartzClientRectangle (Hwnd hwnd) {
			return TranslateClientRectangleToQuartzClientRectangle (hwnd, Control.FromHandle (hwnd.Handle));
		}

		internal static Rectangle TranslateClientRectangleToQuartzClientRectangle (Hwnd hwnd, Control ctrl) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Rectangle rect = hwnd.ClientRect;
			Form form = ctrl as Form;
			CreateParams cp = null;

			if (form != null)
				cp = form.GetCreateParams ();

			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Rectangle qrect = rect;
				
				qrect.Y -= borders.top;
				qrect.X -= borders.left;
				qrect.Width += borders.left + borders.right;
				qrect.Height += borders.top + borders.bottom;
				
				rect = qrect;
			}
			
			if (rect.Width < 1 || rect.Height < 1) {
				rect.Width = 1;
				rect.Height = 1;
				rect.X = -5;
				rect.Y = -5;
			}
			
			return rect;
		}

		internal static Size TranslateWindowSizeToQuartzWindowSize (CreateParams cp) {
			return TranslateWindowSizeToQuartzWindowSize (cp, new Size (cp.Width, cp.Height));
		}

		internal static Size TranslateWindowSizeToQuartzWindowSize (CreateParams cp, Size size) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size qsize = size;

				qsize.Width -= borders.left + borders.right;
				qsize.Height -= borders.top + borders.bottom; 
				
				size = qsize;
			}

			if (size.Height == 0)
				size.Height = 1;
			if (size.Width == 0)
				size.Width = 1;
			return size;
		}
			
		internal static Size TranslateQuartzWindowSizeToWindowSize (CreateParams cp, int width, int height) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Size size = new Size (width, height);
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size qsize = size;

				qsize.Width += borders.left + borders.right;
				qsize.Height += borders.top + borders.bottom;
				
				size = qsize;
			}

			return size;
		}
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
			if ((Hover.X == mouse_position.X) && (Hover.Y == mouse_position.Y)) {
				MSG msg = new MSG ();
				msg.hwnd = Hover.Hwnd;
				msg.message = Msg.WM_MOUSEHOVER;
				msg.wParam = GetMousewParam (0);
				msg.lParam = (IntPtr)((ushort)Hover.X << 16 | (ushort)Hover.X);
				EnqueueMessage (msg);
			}
		}
		#endregion
		
		#region Private Methods
		private Point ConvertScreenPointToClient (IntPtr handle, Point point) {
			Point converted_point = new Point ();
			Carbon.Rect window_bounds = new Carbon.Rect ();
			Carbon.CGPoint native_point = new Carbon.CGPoint ();

			GetWindowBounds (HIViewGetWindow (handle), 32, ref window_bounds);
			
			native_point.x = (point.X - window_bounds.left);
			native_point.y = (point.Y - window_bounds.top);

			HIViewConvertPoint (ref native_point, IntPtr.Zero, handle);

			converted_point.X = (int)native_point.x;
			converted_point.Y = (int)native_point.y;

			return converted_point;
		}
		
		private Point ConvertClientPointToScreen (IntPtr handle, Point point) {
			Point converted_point = new Point ();
			Carbon.Rect window_bounds = new Carbon.Rect ();
			Carbon.CGPoint native_point = new Carbon.CGPoint ();

			GetWindowBounds (HIViewGetWindow (handle), 32, ref window_bounds);
			
			native_point.x = point.X;
			native_point.y = point.Y;

			HIViewConvertPoint (ref native_point, handle, IntPtr.Zero);

			converted_point.X = (int)(native_point.x + window_bounds.left);
			converted_point.Y = (int)(native_point.y + window_bounds.top);

			return converted_point;
		}

		private double NextTimeout () {
			DateTime now = DateTime.UtcNow;
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
		
		private void CheckTimers (DateTime now) {
			lock (TimerList) {
				int count = TimerList.Count;
				if (count == 0)
					return;
				for (int i = 0; i < TimerList.Count; i++) {
					Timer timer = (Timer) TimerList [i];
					if (timer.Enabled && timer.Expires <= now) {
						// Timer ticks:
						//  - Before MainForm.OnLoad if DoEvents () is called.
						//  - After MainForm.OnLoad if not.
						//
						if (in_doevents ||
						    (Application.MWFThread.Current.Context != null && 
						     Application.MWFThread.Current.Context.MainForm != null && 
						     Application.MWFThread.Current.Context.MainForm.IsLoaded)) {
							timer.FireTick ();
							timer.Update (now);
						}
					}
				}
			}
		}
		
		private void WaitForHwndMessage (Hwnd hwnd, Msg message) {
			MSG msg = new MSG ();

			bool done = false;
			do {
				if (GetMessage(null, ref msg, IntPtr.Zero, 0, 0)) {
					if ((Msg)msg.message == Msg.WM_QUIT) {
						PostQuitMessage (0);
						done = true;
					}
					else {
						if (msg.hwnd == hwnd.Handle) {
							if ((Msg)msg.message == message)
								break;
							else if ((Msg)msg.message == Msg.WM_DESTROY)
								done = true;
						}

						TranslateMessage (ref msg);
						DispatchMessage (ref msg);
					}
				}
			} while (!done);
		}

		private void SendParentNotify(IntPtr child, Msg cause, int x, int y) {
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

		private bool StyleSet (int s, WindowStyles ws) {
			return (s & (int)ws) == (int)ws;
		}

		private bool ExStyleSet (int ex, WindowExStyles exws) {
			return (ex & (int)exws) == (int)exws;
		}

		private void DeriveStyles(int Style, int ExStyle, out FormBorderStyle border_style, out bool border_static, out TitleStyle title_style, out int caption_height, out int tool_caption_height) {

			caption_height = 0;
			tool_caption_height = 0;
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
					caption_height = 0;
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_MDICHILD)) {
					caption_height = 0;

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
		
		private void SetHwndStyles(Hwnd hwnd, CreateParams cp) {
			DeriveStyles(cp.Style, cp.ExStyle, out hwnd.border_style, out hwnd.border_static, out hwnd.title_style, out hwnd.caption_height, out hwnd.tool_caption_height);
		}
		
		private void ShowCaret () {
			if (Caret.On)
				return;
			Caret.On = true;
			ShowWindow (CaretWindow);
			Graphics g = Graphics.FromHwnd (HIViewGetRoot (CaretWindow));

			g.FillRectangle (new SolidBrush (Color.Black), new Rectangle (0, 0, Caret.Width, Caret.Height));

			g.Dispose ();
		}

		private void HideCaret () {
			if (!Caret.On)
				return;
			Caret.On = false;
			HideWindow (CaretWindow);
		}
		
		private void AccumulateDestroyedHandles (Control c, ArrayList list) {
			if (c != null) {
				Control[] controls = c.Controls.GetAllControls ();

				if (c.IsHandleCreated && !c.IsDisposed) {
					Hwnd hwnd = Hwnd.ObjectFromHandle(c.Handle);

					list.Add (hwnd);
					CleanupCachedWindows (hwnd);
				}

				for (int  i = 0; i < controls.Length; i ++) {
					AccumulateDestroyedHandles (controls[i], list);
				}
			}
			
		}

		private void CleanupCachedWindows (Hwnd hwnd) {
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

		private void AddExpose (Hwnd hwnd, bool client, int x, int y, int width, int height) {
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
				if (!hwnd.expose_pending && hwnd.visible) {
					MSG msg = new MSG ();
					msg.message = Msg.WM_PAINT;
					msg.hwnd = hwnd.Handle;
					EnqueueMessage (msg);
					hwnd.expose_pending = true;
				}
			} else {
				hwnd.AddNcInvalidArea (x, y, width, height);
				if (!hwnd.nc_expose_pending && hwnd.visible) {
					MSG msg = new MSG ();
					Region rgn = new Region (hwnd.Invalid);
					IntPtr hrgn = rgn.GetHrgn (null); // Graphics object isn't needed
					msg.message = Msg.WM_NCPAINT;
					msg.wParam = hrgn == IntPtr.Zero ? (IntPtr)1 : hrgn;
					msg.refobject = rgn;
					msg.hwnd = hwnd.Handle;
					EnqueueMessage (msg);
					hwnd.nc_expose_pending = true;

				}
			}
		}
		#endregion 

		#region Public Methods
		internal void EnqueueMessage (MSG msg) {
			lock (queuelock) {
				MessageQueue.Enqueue (msg);
			}
		}

		internal override void RaiseIdle (EventArgs e)
		{
			if (Idle != null)
				Idle (this, e);
		}

		internal override IntPtr InitializeDriver() {
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
		}

		internal override void EnableThemes() {
			themes_enabled = true;
		}

		internal override void Activate(IntPtr handle) {
			if (ActiveWindow != IntPtr.Zero) {
				ActivateWindow (HIViewGetWindow (ActiveWindow), false);
			}
			ActivateWindow (HIViewGetWindow (handle), true);
			ActiveWindow = handle;
		}

		internal override void AudibleAlert(AlertType alert) {
			AlertSoundPlay ();
		}

		internal override void BeginMoveResize (IntPtr handle) {
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
		
		internal override bool CalculateWindowRect(ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect) {
			WindowRect = Hwnd.GetWindowRectangle (cp, menu, ClientRect);
			return true;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertClientPointToScreen (hwnd.ClientWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}
		
		internal override void MenuToScreen(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertClientPointToScreen (hwnd.ClientWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}

		internal override int[] ClipboardAvailableFormats(IntPtr handle) {
			ArrayList list = new ArrayList ();
			DataFormats.Format f = DataFormats.Format.List;

			while (f != null) {
				list.Add (f.Id);
				f = f.Next;
			}

			return (int [])list.ToArray (typeof (int));
		}

		internal override void ClipboardClose(IntPtr handle) {
		}

		//TODO: Map our internal formats to the right os code where we can
		internal override int ClipboardGetID(IntPtr handle, string format) {
			return (int)__CFStringMakeConstantString (format);
		}

		internal override IntPtr ClipboardOpen(bool primary_selection) {
			if (primary_selection)
				return Carbon.Pasteboard.Primary;
			return Carbon.Pasteboard.Application;
		}

		internal override object ClipboardRetrieve(IntPtr handle, int type, XplatUI.ClipboardToObject converter) {
			return Carbon.Pasteboard.Retrieve (handle, type);
		}

		internal override void ClipboardStore(IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter) {
			Carbon.Pasteboard.Store (handle, obj, type);
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
			Hwnd hwnd;
			Hwnd parent_hwnd = null;
			int X;
			int Y;
			int Width;
			int Height;
			IntPtr ParentHandle;
			IntPtr WindowHandle;
			IntPtr WholeWindow;
			IntPtr ClientWindow;
			IntPtr WholeWindowTracking;
			IntPtr ClientWindowTracking;

			hwnd = new Hwnd ();

			X = cp.X;
			Y = cp.Y;
			Width = cp.Width;
			Height = cp.Height;
			ParentHandle = IntPtr.Zero;
			WindowHandle = IntPtr.Zero;
			WholeWindow = IntPtr.Zero;
			ClientWindow = IntPtr.Zero;
			WholeWindowTracking = IntPtr.Zero;
			ClientWindowTracking = IntPtr.Zero;

			if (Width < 1) Width = 1;	
			if (Height < 1) Height = 1;	

			if (cp.Parent != IntPtr.Zero) {
				parent_hwnd = Hwnd.ObjectFromHandle (cp.Parent);
				ParentHandle = parent_hwnd.client_window;
			} else {
				if (StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
					HIViewFindByID (HIViewGetRoot (FosterParent), new Carbon.HIViewID (Carbon.EventHandler.kEventClassWindow, 1), ref ParentHandle);
				}
			}

			Point next;
			if (cp.control is Form) {
				next = Hwnd.GetNextStackedFormLocation (cp, parent_hwnd);
				X = next.X;
				Y = next.Y;
			}

			hwnd.x = X;
			hwnd.y = Y;
			hwnd.width = Width;
			hwnd.height = Height;
			hwnd.Parent = Hwnd.ObjectFromHandle (cp.Parent);
			hwnd.initial_style = cp.WindowStyle;
			hwnd.initial_ex_style = cp.WindowExStyle;
			hwnd.visible = false;

			if (StyleSet (cp.Style, WindowStyles.WS_DISABLED)) {
				hwnd.enabled = false;
			}

			ClientWindow = IntPtr.Zero;

			Size QWindowSize = TranslateWindowSizeToQuartzWindowSize (cp);
			Rectangle QClientRect = TranslateClientRectangleToQuartzClientRectangle (hwnd, cp.control);

			SetHwndStyles(hwnd, cp);
/* FIXME */
			if (ParentHandle == IntPtr.Zero) {
				IntPtr WindowView = IntPtr.Zero;
				IntPtr GrowBox = IntPtr.Zero;
				Carbon.WindowClass windowklass = Carbon.WindowClass.kOverlayWindowClass;
				Carbon.WindowAttributes attributes = Carbon.WindowAttributes.kWindowCompositingAttribute | Carbon.WindowAttributes.kWindowStandardHandlerAttribute;
				if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZEBOX)) {
					attributes |= Carbon.WindowAttributes.kWindowCollapseBoxAttribute;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZEBOX)) {
					attributes |= Carbon.WindowAttributes.kWindowResizableAttribute | Carbon.WindowAttributes.kWindowHorizontalZoomAttribute | Carbon.WindowAttributes.kWindowVerticalZoomAttribute;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_SYSMENU)) {
					attributes |= Carbon.WindowAttributes.kWindowCloseBoxAttribute;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
					windowklass = Carbon.WindowClass.kDocumentWindowClass;
				}
				if (hwnd.border_style == FormBorderStyle.FixedToolWindow) {
					windowklass = Carbon.WindowClass.kUtilityWindowClass;
				} else if (hwnd.border_style == FormBorderStyle.SizableToolWindow) {
					attributes |= Carbon.WindowAttributes.kWindowResizableAttribute;
					windowklass = Carbon.WindowClass.kUtilityWindowClass;
				}
				if (windowklass == Carbon.WindowClass.kOverlayWindowClass) {
					attributes = Carbon.WindowAttributes.kWindowCompositingAttribute | Carbon.WindowAttributes.kWindowStandardHandlerAttribute;
				}
				attributes |= Carbon.WindowAttributes.kWindowLiveResizeAttribute;

				Carbon.Rect rect = new Carbon.Rect ();
				if (StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
					SetRect (ref rect, (short)X, (short)(Y), (short)(X + QWindowSize.Width), (short)(Y + QWindowSize.Height));
				} else {
					SetRect (ref rect, (short)X, (short)(Y + MenuBarHeight), (short)(X + QWindowSize.Width), (short)(Y + MenuBarHeight + QWindowSize.Height));
				}

				CreateNewWindow (windowklass, attributes, ref rect, ref WindowHandle);

				Carbon.EventHandler.InstallWindowHandler (WindowHandle);
				HIViewFindByID (HIViewGetRoot (WindowHandle), new Carbon.HIViewID (Carbon.EventHandler.kEventClassWindow, 1), ref WindowView);
				HIViewFindByID (HIViewGetRoot (WindowHandle), new Carbon.HIViewID (Carbon.EventHandler.kEventClassWindow, 7), ref GrowBox);
				HIGrowBoxViewSetTransparent (GrowBox, true);
				SetAutomaticControlDragTrackingEnabledForWindow (WindowHandle, true);
				ParentHandle = WindowView;
			}

			HIObjectCreate (__CFStringMakeConstantString ("com.novell.mwfview"), 0, ref WholeWindow);
			HIObjectCreate (__CFStringMakeConstantString ("com.novell.mwfview"), 0, ref ClientWindow);

			Carbon.EventHandler.InstallControlHandler (WholeWindow);
			Carbon.EventHandler.InstallControlHandler (ClientWindow);

			// Enable embedding on controls
			HIViewChangeFeatures (WholeWindow, 1<<1, 0);
			HIViewChangeFeatures (ClientWindow, 1<<1, 0);

			HIViewNewTrackingArea (WholeWindow, IntPtr.Zero, (UInt64)WholeWindow, ref WholeWindowTracking);
			HIViewNewTrackingArea (ClientWindow, IntPtr.Zero, (UInt64)ClientWindow, ref ClientWindowTracking);
			Carbon.HIRect WholeRect;
			if (WindowHandle != IntPtr.Zero) {
				WholeRect = new Carbon.HIRect (0, 0, QWindowSize.Width, QWindowSize.Height);
			} else {
				WholeRect = new Carbon.HIRect (X, Y, QWindowSize.Width, QWindowSize.Height);
			}
			Carbon.HIRect ClientRect = new Carbon.HIRect (QClientRect.X, QClientRect.Y, QClientRect.Width, QClientRect.Height);
			HIViewSetFrame (WholeWindow, ref WholeRect);
			HIViewSetFrame (ClientWindow, ref ClientRect);

			HIViewAddSubview (ParentHandle, WholeWindow);
			HIViewAddSubview (WholeWindow, ClientWindow);

			hwnd.WholeWindow = WholeWindow;
			hwnd.ClientWindow = ClientWindow;

			if (WindowHandle != IntPtr.Zero) {
				WindowMapping [hwnd.Handle] = WindowHandle;
				HandleMapping [WindowHandle] = hwnd.Handle;
				if (hwnd.border_style == FormBorderStyle.FixedToolWindow || hwnd.border_style == FormBorderStyle.SizableToolWindow) {
					UtilityWindows.Add (WindowHandle);
				}
			}

			// Allow dnd on controls
			Dnd.SetAllowDrop (hwnd, true);

			Text (hwnd.Handle, cp.Caption);
			
			SendMessage (hwnd.Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			SendParentNotify (hwnd.Handle, Msg.WM_CREATE, int.MaxValue, int.MaxValue);

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
				if (WindowHandle != IntPtr.Zero) {
					if (Control.FromHandle(hwnd.Handle) is Form) {
						Form f = Control.FromHandle(hwnd.Handle) as Form;
						if (f.WindowState == FormWindowState.Normal) {
							SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
						}
					}
					ShowWindow (WindowHandle);
					WaitForHwndMessage (hwnd, Msg.WM_SHOWWINDOW);
				}
				HIViewSetVisible (WholeWindow, true);
				HIViewSetVisible (ClientWindow, true);
				hwnd.visible = true;
				if (!(Control.FromHandle(hwnd.Handle) is Form)) {
					SendMessage(hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
				}
			}

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE)) {
				SetWindowState(hwnd.Handle, FormWindowState.Minimized);
			} else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE)) {
				SetWindowState(hwnd.Handle, FormWindowState.Maximized);
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

		internal override Bitmap DefineStdCursorBitmap (StdCursor id) {
			return Carbon.Cursor.DefineStdCursorBitmap (id);
		}

		internal override IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			return Carbon.Cursor.DefineCursor (bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);
		}
		
		internal override IntPtr DefineStdCursor (StdCursor id) {
			return Carbon.Cursor.DefineStdCursor (id);
		}
		
		internal override IntPtr DefWndProc(ref Message msg) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (msg.HWnd);
			switch ((Msg)msg.Msg) {
				case Msg.WM_IME_COMPOSITION:
					string s = KeyboardHandler.ComposedString;
					foreach (char c in s)
						SendMessage (msg.HWnd, Msg.WM_IME_CHAR, (IntPtr) c, msg.LParam);
					break;
				case Msg.WM_IME_CHAR:
					// On Windows API it sends two WM_CHAR messages for each byte, but
					// I wonder if it is worthy to emulate it (also no idea how to 
					// reconstruct those bytes into chars).
					SendMessage (msg.HWnd, Msg.WM_CHAR, msg.WParam, msg.LParam);
					return IntPtr.Zero;
				case Msg.WM_QUIT: {
					if (WindowMapping [hwnd.Handle] != null)

						Exit ();
					break;
				}
				case Msg.WM_PAINT: {
					hwnd.expose_pending = false;
					break;
				}
				case Msg.WM_NCPAINT: {
					hwnd.nc_expose_pending = false;
					break;
				}  
				case Msg.WM_NCCALCSIZE: {
					if (msg.WParam == (IntPtr)1) {
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
					break;
				}
				case Msg.WM_SETCURSOR: {
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
												//FIXME: AudibleAlert();
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

			if (hwnd == null) {
				return;
			}

			SendParentNotify (hwnd.Handle, Msg.WM_DESTROY, int.MaxValue, int.MaxValue);
				
			CleanupCachedWindows (hwnd);

			ArrayList windows = new ArrayList ();

			AccumulateDestroyedHandles (Control.ControlNativeWindow.ControlFromHandle(hwnd.Handle), windows);


			foreach (Hwnd h in windows) {
				SendMessage (h.Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
				h.zombie = true;
			}

			// TODO: This is crashing swf-messageboxes
			/*
			if (false && hwnd.whole_window != IntPtr.Zero)
				CFRelease (hwnd.whole_window);
			if (false && hwnd.client_window != IntPtr.Zero)
				CFRelease (hwnd.client_window);
			*/

			if (WindowMapping [hwnd.Handle] != null) { 
				DisposeWindow ((IntPtr)(WindowMapping [hwnd.Handle]));
				WindowMapping.Remove (hwnd.Handle);
			}
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}
		
		internal override void DoEvents() {
                        MSG     msg = new MSG ();

			in_doevents = true;
			while (PeekMessage (null, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
                                TranslateMessage (ref msg);
                                DispatchMessage (ref msg);
                        }
			in_doevents = false;

		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			//Like X11 we need not do anything here
		}

		internal override void EndLoop(Thread thread) {
		}

		internal void Exit () {
			GetMessageResult = false;
		}
		
		internal override IntPtr GetActive() {
			return ActiveWindow;
		}

		internal override Region GetClipRegion(IntPtr hwnd) {
			return null;
		}

		[MonoTODO]
		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			width = 12;
			height = 12;
			hotspot_x = 0;
			hotspot_y = 0;
		}
		
		internal override void GetDisplaySize(out Size size) {
			Carbon.HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
			size = new Size ((int)bounds.size.width, (int)bounds.size.height);
		}

		internal override IntPtr GetParent(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null && hwnd.Parent != null) {
				return hwnd.Parent.Handle;
			}
			return IntPtr.Zero;
		}

		internal override IntPtr GetPreviousWindow(IntPtr handle) {
			return HIViewGetPreviousView(handle);
		}
		
		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			Carbon.QDPoint pt = new Carbon.QDPoint ();
			GetGlobalMouse (ref pt);
			x = pt.x;
			y = pt.y;
		}

		internal override IntPtr GetFocus() {
			return FocusWindow;
		}

		
		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			FontFamily ff = font.FontFamily;
			ascent = ff.GetCellAscent (font.Style);
			descent = ff.GetCellDescent (font.Style);
			return true;
		}
		
		internal override Point GetMenuOrigin(IntPtr handle) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				return hwnd.MenuOrigin;
			}
			return Point.Empty;
		}

		internal override bool GetMessage(object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			IntPtr evtRef = IntPtr.Zero;
			IntPtr target = GetEventDispatcherTarget();
			CheckTimers (DateTime.UtcNow);
			ReceiveNextEvent (0, IntPtr.Zero, 0, true, ref evtRef);
			if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
				SendEventToEventTarget (evtRef, target);
				ReleaseEvent (evtRef);
			}
			
			object queueobj;
			loop:
			lock (queuelock) {

				if (MessageQueue.Count <= 0) {
					if (Idle != null) 
						Idle (this, EventArgs.Empty);
					else if (TimerList.Count == 0) {
						ReceiveNextEvent (0, IntPtr.Zero, 0.15, true, ref evtRef);
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
				queueobj = MessageQueue.Dequeue ();
			}
			if (queueobj is GCHandle) {
				XplatUIDriverSupport.ExecuteClientMessage((GCHandle)queueobj);
				goto loop;
			} else {
				msg = (MSG)queueobj;
			}
			return GetMessageResult;
		}
		
		[MonoTODO]
		internal override bool GetText(IntPtr handle, out string text) {
			throw new NotImplementedException ();
		}
		
		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
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
		
		internal override FormWindowState GetWindowState(IntPtr hwnd) {
			IntPtr window = HIViewGetWindow (hwnd);

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
			Grab.Hwnd = handle;
			Grab.Confined = confine_to_handle != IntPtr.Zero;
			/* FIXME: Set the Grab.Area */
		}
		
		internal override void UngrabWindow(IntPtr hwnd) {
                        bool was_grabbed = Grab.Hwnd != IntPtr.Zero;

                        Grab.Hwnd = IntPtr.Zero;
                        Grab.Confined = false;

                        if (was_grabbed) {
                                // lparam should be the handle to the window gaining the mouse capture,
                                // but we dont have that information like X11.
                                // Also only generate WM_CAPTURECHANGED if the window actually was grabbed.
                                SendMessage (hwnd, Msg.WM_CAPTURECHANGED, IntPtr.Zero, IntPtr.Zero);
                        }
		}
		
		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}
		
		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (clear) {
				AddExpose (hwnd, true, hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
			} else {
				AddExpose (hwnd, true, rc.X, rc.Y, rc.Width, rc.Height);
			} 
		}

		internal override void InvalidateNC (IntPtr handle)
		{
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			AddExpose (hwnd, false, 0, 0, hwnd.Width, hwnd.Height); 
		}
		
		internal override bool IsEnabled(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).Enabled;
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
		}

		internal override PaintEventArgs PaintEventStart(ref Message msg, IntPtr handle, bool client) {
			PaintEventArgs	paint_event;
			Hwnd		hwnd;
			Hwnd		paint_hwnd; 
			
			hwnd = Hwnd.ObjectFromHandle(msg.HWnd);
			if (msg.HWnd == handle) {
				paint_hwnd = hwnd;
			} else {
				paint_hwnd = Hwnd.ObjectFromHandle (handle);
			}
			
			if (Caret.Visible == 1) {
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

				// FIXME: Clip region is hosed
				dc.Clip = clip_region;
				paint_event = new PaintEventArgs(dc, hwnd.Invalid);
				hwnd.expose_pending = false;
				hwnd.ClearInvalidArea();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			} else {
				dc = Graphics.FromHwnd (paint_hwnd.whole_window);

				if (!hwnd.nc_invalid.IsEmpty) {
					// FIXME: Clip region is hosed
					dc.SetClip (hwnd.nc_invalid);
					paint_event = new PaintEventArgs(dc, hwnd.nc_invalid);
				} else {
					paint_event = new PaintEventArgs(dc, new Rectangle(0, 0, hwnd.width, hwnd.height));
				}
				hwnd.nc_expose_pending = false;
				hwnd.ClearNcInvalidArea ();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			}

			return paint_event;
		}
		
		internal override void PaintEventEnd(ref Message msg, IntPtr handle, bool client) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			// FIXME: Pop is causing invalid stack ops sometimes; race condition?
			try {
				Graphics dc = (Graphics)hwnd.drawing_stack.Pop();
				dc.Flush ();
				dc.Dispose ();
			
				PaintEventArgs pe = (PaintEventArgs)hwnd.drawing_stack.Pop();
				pe.SetGraphics (null);
				pe.Dispose ();  
			} catch {}

			if (Caret.Visible == 1) {
				ShowCaret();
				Caret.Paused = false;
			}
		}
		
		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			IntPtr evtRef = IntPtr.Zero;
			IntPtr target = GetEventDispatcherTarget();
			CheckTimers (DateTime.UtcNow);
			ReceiveNextEvent (0, IntPtr.Zero, 0, true, ref evtRef);
			if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
				SendEventToEventTarget (evtRef, target);
				ReleaseEvent (evtRef);
			}
			
			lock (queuelock) {
				if (MessageQueue.Count <= 0) {
					return false;
				} else {
					object queueobj;
					if (flags == (uint)PeekMessageFlags.PM_REMOVE)
						queueobj = MessageQueue.Dequeue ();
					else
						queueobj = MessageQueue.Peek ();

					if (queueobj is GCHandle) {
						XplatUIDriverSupport.ExecuteClientMessage((GCHandle)queueobj);
						return false;
					}
					msg = (MSG)queueobj;
					return true;
				}
			}
		}

		internal override bool PostMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			MSG msg = new MSG();
			msg.hwnd = hwnd;
			msg.message = message;
			msg.wParam = wParam;
			msg.lParam = lParam;
			EnqueueMessage (msg);
			return true;
		}

		internal override void PostQuitMessage(int exitCode) {
			PostMessage (FosterParent, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
		}

		internal override void RequestAdditionalWM_NCMessages(IntPtr hwnd, bool hover, bool leave) {
		}

		internal override void RequestNCRecalc(IntPtr handle) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			PerformNCCalc(hwnd);
			SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			InvalidateNC(handle);
		}

		[MonoTODO]		
		internal override void ResetMouseHover(IntPtr handle) {
			throw new NotImplementedException();
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertScreenPointToClient (hwnd.ClientWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}

		internal override void ScreenToMenu(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertScreenPointToClient (hwnd.WholeWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}

		internal override void ScrollWindow(IntPtr handle, Rectangle area, int XAmount, int YAmount, bool clear) {
			/*
			 * This used to use a HIViewScrollRect but this causes issues with the fact that we dont coalesce
			 * updates properly with our short-circuiting of the window manager.  For now we'll do a less
			 * efficient invalidation of the entire handle which appears to fix the problem
			 * see bug #381084
			 */
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Invalidate (handle, new Rectangle (0, 0, hwnd.Width, hwnd.Height), false);
		}
		
		
		internal override void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool clear) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Invalidate (handle, new Rectangle (0, 0, hwnd.Width, hwnd.Height), false);
		}
		
		[MonoTODO]
		internal override void SendAsyncMethod (AsyncMethodData method) {
			// Fake async
			lock (queuelock) {
				MessageQueue.Enqueue (GCHandle.Alloc (method));
			}
		}

		[MonoTODO]
		internal override IntPtr SendMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			return NativeWindow.WndProc(hwnd, message, wParam, lParam);
		}
		
		internal override int SendInput(IntPtr hwnd, Queue keys) {
			return 0;
		}


		internal override void SetCaretPos (IntPtr hwnd, int x, int y) {
			if (hwnd != IntPtr.Zero && hwnd == Caret.Hwnd) {
				Caret.X = x;
				Caret.Y = y;
				ClientToScreen (hwnd, ref x, ref y);
				SizeWindow (new Rectangle (x, y, Caret.Width, Caret.Height), CaretWindow);
				Caret.Timer.Stop ();
				HideCaret ();
				if (Caret.Visible == 1) {
					ShowCaret ();
					Caret.Timer.Start ();
				}
			}
		}

		internal override void SetClipRegion(IntPtr hwnd, Region region) {
			throw new NotImplementedException();
		}
		
		internal override void SetCursor(IntPtr window, IntPtr cursor) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (window);

			hwnd.Cursor = cursor;
		}
		
		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			CGDisplayMoveCursorToPoint (CGMainDisplayID (), new Carbon.CGPoint (x, y));
		}
		
		internal override void SetFocus(IntPtr handle) {
			if (FocusWindow != IntPtr.Zero) {
				PostMessage(FocusWindow, Msg.WM_KILLFOCUS, handle, IntPtr.Zero);
			}
			PostMessage(handle, Msg.WM_SETFOCUS, FocusWindow, IntPtr.Zero);
			FocusWindow = handle;
		}

		internal override void SetIcon(IntPtr handle, Icon icon) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			// FIXME: we need to map the icon for active window switches
			if (WindowMapping [hwnd.Handle] != null) {
				if (icon == null) { 
					RestoreApplicationDockTileImage ();
				} else {
					Bitmap		bitmap;
					int		size;
					IntPtr[]	data;
					int		index;
	
					bitmap = new Bitmap (128, 128);
					using (Graphics g = Graphics.FromImage (bitmap)) {
						g.DrawImage (icon.ToBitmap (), 0, 0, 128, 128);
					}
					index = 0;
					size = bitmap.Width * bitmap.Height;
					data = new IntPtr[size];
	
					for (int y = 0; y < bitmap.Height; y++) {
						for (int x = 0; x < bitmap.Width; x++) {
							int pixel = bitmap.GetPixel (x, y).ToArgb ();
							if (BitConverter.IsLittleEndian) {
								byte a = (byte) ((pixel >> 24) & 0xFF);
								byte r = (byte) ((pixel >> 16) & 0xFF);
								byte g = (byte) ((pixel >> 8) & 0xFF);
								byte b = (byte) (pixel & 0xFF);
								data[index++] = (IntPtr)(a + (r << 8) + (g << 16) + (b << 24));
							} else {
								data[index++] = (IntPtr)pixel;
							}
						}
					}

					IntPtr provider = CGDataProviderCreateWithData (IntPtr.Zero, data, size*4, IntPtr.Zero);
					IntPtr image = CGImageCreate (128, 128, 8, 32, 4*128, CGColorSpaceCreateDeviceRGB (), 4, provider, IntPtr.Zero, 0, 0);
					SetApplicationDockTileImage (image);
				}
			}
		}

		
		internal override void SetModal(IntPtr handle, bool Modal) {
			IntPtr hWnd = HIViewGetWindow (Hwnd.ObjectFromHandle (handle).WholeWindow);
			if (Modal)
				BeginAppModalStateForWindow (hWnd);
			else
				EndAppModalStateForWindow (hWnd);
			return;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			IntPtr ParentHandle = IntPtr.Zero;
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			hwnd.Parent = Hwnd.ObjectFromHandle (parent);
			if (HIViewGetSuperview (hwnd.whole_window) != IntPtr.Zero) {
				HIViewRemoveFromSuperview (hwnd.whole_window);
			}
			if (hwnd.parent == null)
				HIViewFindByID (HIViewGetRoot (FosterParent), new Carbon.HIViewID (Carbon.EventHandler.kEventClassWindow, 1), ref ParentHandle);
			HIViewAddSubview (hwnd.parent == null ? ParentHandle : hwnd.Parent.client_window, hwnd.whole_window);
			HIViewPlaceInSuperviewAt (hwnd.whole_window, hwnd.X, hwnd.Y);
			HIViewAddSubview (hwnd.whole_window, hwnd.client_window);
			HIViewPlaceInSuperviewAt (hwnd.client_window, hwnd.ClientRect.X, hwnd.ClientRect.Y);
			
			return IntPtr.Zero;
		}
		
		internal override void SetTimer (Timer timer) {
			lock (TimerList) {
				TimerList.Add (timer);
			}
		}
		
		internal override bool SetTopmost(IntPtr hWnd, bool Enabled) {
			HIViewSetZOrder (hWnd, 1, IntPtr.Zero);
			return true;
		}
		
		internal override bool SetOwner(IntPtr hWnd, IntPtr hWndOwner) {
			// TODO: Set window owner. 
			return true;
		}
		
		internal override bool SetVisible(IntPtr handle, bool visible, bool activate) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			object window = WindowMapping [hwnd.Handle];
			if (window != null)
				if (visible)
					ShowWindow ((IntPtr)window);
				else
					HideWindow ((IntPtr)window);
			
			if (visible)
				SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
					
			HIViewSetVisible (hwnd.whole_window, visible);
			HIViewSetVisible (hwnd.client_window, visible);

			hwnd.visible = visible;
			hwnd.Mapped = true;
			return true;
		}
		
		internal override void SetAllowDrop (IntPtr handle, bool value) {
			// Like X11 we allow drop on al windows and filter in our handler
		}

		internal override DragDropEffects StartDrag (IntPtr handle, object data, DragDropEffects allowed_effects) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (hwnd == null)
				throw new ArgumentException ("Attempt to begin drag from invalid window handle (" + handle.ToInt32 () + ").");

			return Dnd.StartDrag (hwnd.client_window, data, allowed_effects);
		}

		internal override void SetBorderStyle(IntPtr handle, FormBorderStyle border_style) {
			Form form = Control.FromHandle (handle) as Form;
			if (form != null && form.window_manager == null && (border_style == FormBorderStyle.FixedToolWindow ||
				border_style == FormBorderStyle.SizableToolWindow)) {
				form.window_manager = new ToolWindowManager (form);
			}

			RequestNCRecalc(handle);
		}

		internal override void SetMenu(IntPtr handle, Menu menu) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.menu = menu;

			RequestNCRecalc(handle);
		}
		
		internal override void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max) {
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

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
					HIViewSetVisible(hwnd.WholeWindow, true);
				}
				hwnd.zero_sized = false;
			}

			if ((width < 1) || (height < 1)) {
				hwnd.zero_sized = true;
				HIViewSetVisible(hwnd.WholeWindow, false);
			}

			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && (hwnd.width == width) && (hwnd.height == height)) {
				return;
			}

			if (!hwnd.zero_sized) {
				hwnd.x = x;
				hwnd.y = y;
				hwnd.width = width;
				hwnd.height = height;
				SendMessage(hwnd.client_window, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

				Control ctrl = Control.FromHandle (handle);
				CreateParams cp = ctrl.GetCreateParams ();
				Size TranslatedSize = TranslateWindowSizeToQuartzWindowSize (cp, new Size (width, height));
				Carbon.Rect rect = new Carbon.Rect ();

				if (WindowMapping [hwnd.Handle] != null) {
					if (StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
						SetRect (ref rect, (short)x, (short)y, (short)(x+TranslatedSize.Width), (short)(y+TranslatedSize.Height));
					} else {
						SetRect (ref rect, (short)x, (short)(y+MenuBarHeight), (short)(x+TranslatedSize.Width), (short)(y+MenuBarHeight+TranslatedSize.Height));
					}
					SetWindowBounds ((IntPtr) WindowMapping [hwnd.Handle], 33, ref rect);
					Carbon.HIRect frame_rect = new Carbon.HIRect (0, 0, TranslatedSize.Width, TranslatedSize.Height);
					HIViewSetFrame (hwnd.whole_window, ref frame_rect);
					SetCaretPos (Caret.Hwnd, Caret.X, Caret.Y);
				} else {
					Carbon.HIRect frame_rect = new Carbon.HIRect (x, y, TranslatedSize.Width, TranslatedSize.Height);
					HIViewSetFrame (hwnd.whole_window, ref frame_rect);
				}
				PerformNCCalc(hwnd);
			}

			hwnd.x = x;
			hwnd.y = y;
			hwnd.width = width;
			hwnd.height = height;
		}
		
		internal override void SetWindowState(IntPtr handle, FormWindowState state) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			IntPtr window = HIViewGetWindow (handle);

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
					Form form = Control.FromHandle (hwnd.Handle) as Form;
					if (form != null && form.FormBorderStyle == FormBorderStyle.None) {
						Carbon.Rect rect = new Carbon.Rect ();
						Carbon.HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
						SetRect (ref rect, (short)0, (short)0, (short)bounds.size.width, (short)bounds.size.height);
						SetWindowBounds ((IntPtr) WindowMapping [hwnd.Handle], 33, ref rect);
						HIViewSetFrame (hwnd.whole_window, ref bounds);
					} else {
						ZoomWindow (window, 8, false);
					}
					break;
				}
			}
		}
		
		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			SetHwndStyles(hwnd, cp);
			
			if (WindowMapping [hwnd.Handle] != null) {
				Carbon.WindowAttributes attributes = Carbon.WindowAttributes.kWindowCompositingAttribute | Carbon.WindowAttributes.kWindowStandardHandlerAttribute;
				if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) { 
					attributes |= Carbon.WindowAttributes.kWindowCollapseBoxAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
					attributes |= Carbon.WindowAttributes.kWindowResizableAttribute | Carbon.WindowAttributes.kWindowHorizontalZoomAttribute | Carbon.WindowAttributes.kWindowVerticalZoomAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
					attributes |= Carbon.WindowAttributes.kWindowCloseBoxAttribute;
				}
				if ((cp.ExStyle & ((int)WindowExStyles.WS_EX_TOOLWINDOW)) != 0) {
					attributes = Carbon.WindowAttributes.kWindowStandardHandlerAttribute | Carbon.WindowAttributes.kWindowCompositingAttribute;
				}
				attributes |= Carbon.WindowAttributes.kWindowLiveResizeAttribute;

				Carbon.WindowAttributes outAttributes = Carbon.WindowAttributes.kWindowNoAttributes;
				GetWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], ref outAttributes);
				ChangeWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], attributes, outAttributes);
			}
		}

		internal override void SetWindowTransparency(IntPtr handle, double transparency, Color key) {
		}

		internal override double GetWindowTransparency(IntPtr handle)
		{
			return 1.0;
		}

		internal override TransparencySupport SupportsTransparency() {
			return TransparencySupport.None;
		}
		
		internal override bool SetZOrder(IntPtr handle, IntPtr after_handle, bool Top, bool Bottom) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (Top) {
				HIViewSetZOrder (hwnd.whole_window, 2, IntPtr.Zero);
				return true;
			} else if (!Bottom) {
				Hwnd after_hwnd = Hwnd.ObjectFromHandle (after_handle);
				HIViewSetZOrder (hwnd.whole_window, 2, (after_handle == IntPtr.Zero ? IntPtr.Zero : after_hwnd.whole_window));
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

		internal override object StartLoop(Thread thread) {
			return new object ();
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

#if NET_2_0
		[MonoTODO]
		internal override void SystrayBalloon(IntPtr hwnd, int timeout, string title, string text, ToolTipIcon icon)
		{
			throw new NotImplementedException ();
		}
#endif
		
		internal override bool Text(IntPtr handle, string text) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			if (WindowMapping [hwnd.Handle] != null) {
				SetWindowTitleWithCFString ((IntPtr)(WindowMapping [hwnd.Handle]), __CFStringMakeConstantString (text));
			}
			SetControlTitleWithCFString (hwnd.whole_window, __CFStringMakeConstantString (text));
			SetControlTitleWithCFString (hwnd.client_window, __CFStringMakeConstantString (text));
			return true;
		}
		
		internal override void UpdateWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (!hwnd.visible || !HIViewIsVisible (handle)) {
				return;
			}

			SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
		}
		
		internal override bool TranslateMessage(ref MSG msg) {
			return Carbon.EventHandler.TranslateMessage (ref msg);
		}
		
		#region Reversible regions
		/* 
		 * Quartz has no concept of XOR drawing due to its compositing nature
		 * We fake this by mapping a overlay window on the first draw and mapping it on the second.
		 * This has some issues with it because its POSSIBLE for ControlPaint.DrawReversible* to actually
		 * reverse two regions at once.  We dont do this in MWF, but this behaviour woudn't work.
		 * We could in theory cache the Rectangle/Color combination to handle this behaviour.
		 *
		 * PROBLEMS: This has some flicker / banding
		 */
		internal void SizeWindow (Rectangle rect, IntPtr window) {
			Carbon.Rect qrect = new Carbon.Rect ();

			SetRect (ref qrect, (short)rect.X, (short)rect.Y, (short)(rect.X+rect.Width), (short)(rect.Y+rect.Height));

			SetWindowBounds (window, 33, ref qrect);
		}

		internal override void DrawReversibleLine(Point start, Point end, Color backColor) {
//			throw new NotImplementedException();
		}

		internal override void FillReversibleRectangle (Rectangle rectangle, Color backColor) {
//			throw new NotImplementedException();
		}

		internal override void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style) {
//			throw new NotImplementedException();
		}

		internal override void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width) {
			Rectangle size_rect = rect;
			int new_x = 0;
			int new_y = 0;

			if (ReverseWindowMapped) {
				HideWindow (ReverseWindow);
				ReverseWindowMapped = false;
			} else {
				ClientToScreen(handle, ref new_x, ref new_y);

				size_rect.X += new_x;
				size_rect.Y += new_y;

				SizeWindow (size_rect, ReverseWindow);
				ShowWindow (ReverseWindow);

				rect.X = 0;
				rect.Y = 0;
				rect.Width -= 1;
				rect.Height -= 1;

				Graphics g = Graphics.FromHwnd (HIViewGetRoot (ReverseWindow));

				for (int i = 0; i < line_width; i++) {
					g.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (Color.Black), rect);
					rect.X += 1;
					rect.Y += 1;
					rect.Width -= 1;
					rect.Height -= 1;
				}
	
				g.Flush ();
				g.Dispose ();
				
				ReverseWindowMapped = true;
			}
		}
		#endregion

		internal override SizeF GetAutoScaleSize(Font font) {
			Graphics        g;
			float           width;
			string          magic_string = "The quick brown fox jumped over the lazy dog.";
			double          magic_number = 44.549996948242189;

			g = Graphics.FromImage (new Bitmap (1, 1));

			width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			return new SizeF(width, font.Height);
		}

		internal override Point MousePosition {
			get {
				return mouse_position;
			}
		}
		#endregion
		
		#region System information
		internal override int KeyboardSpeed { get{ throw new NotImplementedException(); } } 
		internal override int KeyboardDelay { get{ throw new NotImplementedException(); } } 

		internal override int CaptionHeight {
			get {
				return 19;
			}
		}

		internal override  Size CursorSize { get{ throw new NotImplementedException(); } }
		internal override  bool DragFullWindows { get{ throw new NotImplementedException(); } }
		internal override  Size DragSize {
			get {
				return new Size(4, 4);
			}
		}

		internal override  Size FrameBorderSize {
			get {
				return new Size (2, 2);
			}
		}

		internal override  Size IconSize { get{ throw new NotImplementedException(); } }
		internal override  Size MaxWindowTrackSize { get{ throw new NotImplementedException(); } }
		internal override bool MenuAccessKeysUnderlined {
			get {
				return false;
			}
		}
		internal override Size MinimizedWindowSpacingSize { get{ throw new NotImplementedException(); } }

		internal override Size MinimumWindowSize {
			get {
				return new Size(110, 22);
			}
		}

		internal override Keys ModifierKeys {
			get {
				return KeyboardHandler.ModifierKeys;
			}
		}
		internal override Size SmallIconSize { get{ throw new NotImplementedException(); } }
		internal override int MouseButtonCount { get{ throw new NotImplementedException(); } }
		internal override bool MouseButtonsSwapped { get{ throw new NotImplementedException(); } }
		internal override bool MouseWheelPresent { get{ throw new NotImplementedException(); } }

		internal override MouseButtons MouseButtons {
			get {
				return MouseState;
			}
		}

		internal override Rectangle VirtualScreen {
			get {
				return WorkingArea;
			}
		}

		internal override Rectangle WorkingArea { 
			get { 
				Carbon.HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
				return new Rectangle ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
			}
		}
		internal override bool ThemesEnabled {
			get {
				return XplatUICarbon.themes_enabled;
			}
		}
 

		#endregion
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewConvertPoint (ref Carbon.CGPoint point, IntPtr pView, IntPtr cView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewChangeFeatures (IntPtr aView, ulong bitsin, ulong bitsout);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewFindByID (IntPtr rootWnd, Carbon.HIViewID id, ref IntPtr outPtr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIGrowBoxViewSetTransparent (IntPtr GrowBox, bool transparency);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr HIViewGetRoot (IntPtr hWnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIObjectCreate (IntPtr cfStr, uint what, ref IntPtr hwnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIObjectRegisterSubclass (IntPtr classid, IntPtr superclassid, uint options, Carbon.EventDelegate upp, uint count, Carbon.EventTypeSpec [] list, IntPtr state, ref IntPtr cls);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewPlaceInSuperviewAt (IntPtr view, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewAddSubview (IntPtr parentHnd, IntPtr childHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr HIViewGetPreviousView (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr HIViewGetSuperview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewRemoveFromSuperview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewSetVisible (IntPtr vHnd, bool visible);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static bool HIViewIsVisible (IntPtr vHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewGetBounds (IntPtr vHnd, ref Carbon.HIRect r);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewScrollRect (IntPtr vHnd, ref Carbon.HIRect rect, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewSetZOrder (IntPtr hWnd, int cmd, IntPtr oHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewNewTrackingArea (IntPtr inView, IntPtr inShape, UInt64 inID, ref IntPtr outRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr HIViewGetWindow (IntPtr aView);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int HIViewSetFrame (IntPtr view_handle, ref Carbon.HIRect bounds);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int HIViewSetNeedsDisplayInRect (IntPtr view_handle, ref Carbon.HIRect rect, bool needs_display);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void SetRect (ref Carbon.Rect r, short left, short top, short right, short bottom);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int ActivateWindow (IntPtr windowHnd, bool inActivate);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern bool IsWindowActive (IntPtr windowHnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetAutomaticControlDragTrackingEnabledForWindow (IntPtr window, bool enabled);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr GetEventDispatcherTarget ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int SendEventToEventTarget (IntPtr evt, IntPtr target);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int ReleaseEvent (IntPtr evt);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int ReceiveNextEvent (uint evtCount, IntPtr evtTypes, double timeout, bool processEvt, ref IntPtr evt);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static bool IsWindowCollapsed (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static bool IsWindowInStandardState (IntPtr hWnd, IntPtr a, IntPtr b);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void CollapseWindow (IntPtr hWnd, bool collapse);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void ZoomWindow (IntPtr hWnd, short partCode, bool front);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int GetWindowAttributes (IntPtr hWnd, ref Carbon.WindowAttributes outAttributes);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int ChangeWindowAttributes (IntPtr hWnd, Carbon.WindowAttributes inAttributes, Carbon.WindowAttributes outAttributes);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int GetGlobalMouse (ref Carbon.QDPoint outData);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int BeginAppModalStateForWindow (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int EndAppModalStateForWindow (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int CreateNewWindow (Carbon.WindowClass klass, Carbon.WindowAttributes attributes, ref Carbon.Rect r, ref IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int DisposeWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int ShowWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int HideWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static bool IsWindowVisible (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int SetWindowBounds (IntPtr wHnd, uint reg, ref Carbon.Rect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int GetWindowBounds (IntPtr wHnd, uint reg, ref Carbon.Rect rect);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int SetControlTitleWithCFString (IntPtr hWnd, IntPtr titleCFStr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int SetWindowTitleWithCFString (IntPtr hWnd, IntPtr titleCFStr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr __CFStringMakeConstantString (string cString);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int CFRelease (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static short GetMBarHeight ();
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void AlertSoundPlay ();

		#region Cursor imports
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static Carbon.HIRect CGDisplayBounds (IntPtr displayID);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr CGMainDisplayID ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void CGDisplayShowCursor (IntPtr display);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void CGDisplayHideCursor (IntPtr display);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void CGDisplayMoveCursorToPoint (IntPtr display, Carbon.CGPoint point);
		#endregion

		#region Process imports
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int GetCurrentProcess (ref Carbon.ProcessSerialNumber psn);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int TransformProcessType (ref Carbon.ProcessSerialNumber psn, uint type);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static int SetFrontProcess (ref Carbon.ProcessSerialNumber psn);
		#endregion

		#region Dock tile imports
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr CGColorSpaceCreateDeviceRGB();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr CGDataProviderCreateWithData (IntPtr info, IntPtr [] data, int size, IntPtr releasefunc);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static IntPtr CGImageCreate (int width, int height, int bitsPerComponent, int bitsPerPixel, int bytesPerRow, IntPtr colorspace, uint bitmapInfo, IntPtr provider, IntPtr decode, int shouldInterpolate, int intent);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void SetApplicationDockTileImage(IntPtr imageRef);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		extern static void RestoreApplicationDockTileImage();
		#endregion
	}
}
