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

	delegate int ViewEventHandler (IntPtr inCallRef, IntPtr inEvent, IntPtr userData);

	internal class XplatUIOSX : XplatUIDriver {
		
		private static XplatUIOSX instance;
		private static int ref_count;

		private static Hashtable handle_data;
		private static Queue carbonEvents;
		private ViewEventHandler viewEventHandler;
		private static Hashtable view_window_mapping;
		private static IntPtr cgContext;

		private static EventTypeSpec [] viewEvents = new EventTypeSpec [] {
									new EventTypeSpec (1668183148, 4) 
									};
		[MonoTODO]
		internal override Keys ModifierKeys {
			get {
				throw new NotImplementedException ();
			}
		}

		internal override MouseButtons MouseButtons {
			get {
				throw new NotImplementedException ();
			}
		}

		internal override Point MousePosition {
			get {
				throw new NotImplementedException ();
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
			Console.WriteLine ("creating Queue()");
			viewEventHandler = new ViewEventHandler (ViewHandler);
			ref_count = 0;
			handle_data = new Hashtable ();
			carbonEvents = new Queue ();
			view_window_mapping = new Hashtable ();
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
			
Console.WriteLine ("CreateWindow call parent dump: {0:x}", (int)parentHnd);
			if (parentHnd == IntPtr.Zero) {
				if ((cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
					// This is a child view that is going to be parentless;
					realWindow = false;
				} else if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
					// This is a popup window that will be real.
					realWindow = true;
				} else {
					// This is a real root window too
					if (cp.X < 1) cp.X = 50;
					if (cp.Y < 1) cp.Y = 50;
					realWindow = true;
				}
			} else {
				realWindow = false;
			}

			if (realWindow) {
				IntPtr rect = IntPtr.Zero;
				IntPtr viewHnd = IntPtr.Zero;
				SetRect (ref rect, (short)cp.X, (short)cp.Y, (short)(cp.Width+cp.X), (short)(cp.Height+cp.Y));
				CheckError (CreateNewWindow (6, 33554432 | 31 | 524288, ref rect, ref windowHnd), "CreateNewWindow ()");
				CheckError (HIViewFindByID (HIViewGetRoot (windowHnd), new HIViewID (2003398244, 1), ref viewHnd), "HIViewFindByID ()");
				parentHnd = viewHnd;
			}
			HIRect r = new HIRect (cp.X, cp.Y, cp.Width, cp.Height);
			Console.WriteLine ("Creating a view @ {0} {1} of {2} {3}", cp.X, cp.Y, cp.Width, cp.Height);
			CheckError (HIObjectCreate (__CFStringMakeConstantString ("com.apple.hiview"), 0, ref hWnd), "HIObjectCreate ()");
			CheckError (InstallEventHandler (GetControlEventTarget (hWnd), viewEventHandler, (uint)viewEvents.Length, viewEvents, hWnd, IntPtr.Zero), "InstallEventHandler ()");
			CheckError (HIViewChangeFeatures (hWnd, 1 << 1, 0), "HIViewChangeFeatures ()");
			CheckError (HIViewSetFrame (hWnd, ref r), "HIViewSetFrame ()");
			CheckError (HIViewSetVisible (hWnd, true), "HIViewSetVisible ()");
			if (parentHnd != IntPtr.Zero && parentHnd != hWnd) {
				Console.WriteLine ("Adding a subview to {0:x} of {1:x}", (int)parentHnd, (int)hWnd);
				CheckError (HIViewAddSubview (parentHnd, hWnd), "HIViewAddSubview ()");
			}
			if (realWindow) {
				view_window_mapping [hWnd] = windowHnd;
				CheckError (ShowWindow (windowHnd));
				Console.Write ("WINDOW VIEW: ");
			} else {
				Console.Write ("VIEW: ");
			}
			Console.WriteLine ("Returning a new window/view of {0:x}", (int)hWnd);
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
			// FIXME
		}

		[MonoTODO]
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

			HIRect bounds = new HIRect ();
			HIViewGetBounds (handle, ref bounds); 
			CGContextTranslateCTM (cgContext, 0, bounds.size.height);
			CGContextScaleCTM (cgContext, 1.0, -1.0);
			data.DeviceContext = Graphics.FromHwndWithSize (cgContext, (int)bounds.size.width, (int)bounds.size.height);
			paint_event = new PaintEventArgs((Graphics)data.DeviceContext, data.InvalidArea);

			return paint_event;
		}

		[MonoTODO]
		internal override void PaintEventEnd(IntPtr handle) {
			Console.WriteLine ("Paint on {0:x} finished", (int)handle);
			HandleData data = (HandleData) handle_data [handle];
			if (data == null)
				throw new Exception ("null data on PaintEventEnd");
			data.ClearInvalidArea ();
			Graphics g = (Graphics) data.DeviceContext;
			g.Flush ();
			g.Dispose ();
			CGContextRestoreGState (cgContext);
                }

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			if (view_window_mapping [handle] != null) {
				IntPtr rect = IntPtr.Zero;
				SetRect (ref rect, (short)x, (short)y, (short)(x+width), (short)(y+height));
				CheckError (SetWindowBounds ((IntPtr) view_window_mapping [handle], 32, ref rect), "SetWindowBounds ()");
			}
			HIRect r = new HIRect (x, y, width, height);
                        CheckError (HIViewSetFrame (handle, ref r), "HIViewSetFrame ()");
		}

		[MonoTODO]
		internal override void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void Activate(IntPtr handle) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void EnableWindow(IntPtr handle, bool Enable) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void SetModal(IntPtr handle, bool Modal) {
			// FIXME: What do we do here on OSX?
			return;
		}

		[MonoTODO]
		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			// FIXME: What do we do here
//			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override IntPtr DefWndProc(ref Message msg) {
			return IntPtr.Zero;
		}

		[MonoTODO]
		internal override void HandleException(Exception e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void DoEvents() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			throw new NotImplementedException ();
		}

		internal int ViewHandler (IntPtr inCallRef, IntPtr inEvent, IntPtr controlHnd) {
			Console.WriteLine ("ViewEvent on {0:x} {1} {2}", (int)controlHnd, GetEventClass (inEvent), GetEventKind (inEvent));
			GetEventParameter (inEvent, 1668183160, 1668183160, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref cgContext);
			Console.WriteLine ("\tcgContext: {0:x}", (int)cgContext);
			CGContextSaveGState (cgContext);
			int eventClass = GetEventClass (inEvent);
			int eventKind = GetEventKind (inEvent);
			MSG msg = new MSG ();
			msg.hwnd = controlHnd;
			lock (carbonEvents) {
				if (eventClass == 1668183148 && eventKind == 4) {
					if (handle_data [controlHnd] == null) {
						handle_data [controlHnd] = new HandleData ();
						HIRect bounds = new HIRect ();
						HIViewGetBounds (controlHnd, ref bounds); 
						((HandleData) handle_data [controlHnd]).AddToInvalidArea ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
					}
					msg.message = Msg.WM_PAINT;
					msg.wParam = IntPtr.Zero;
					msg.lParam = IntPtr.Zero;
					DispatchMessage (ref msg);
					return 0;
				} else {
					msg.message = Msg.WM_ENTERIDLE;
				}
				carbonEvents.Enqueue (msg);
				Console.WriteLine ("queue: {0}", carbonEvents.Count);
			}
			
			return 0;
		}

		[MonoTODO]
		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			IntPtr evtRef = IntPtr.Zero;
			IntPtr target = GetEventDispatcherTarget();
			ReceiveNextEvent (0, IntPtr.Zero, 1, true, ref evtRef);
			if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
				SendEventToEventTarget (evtRef, target);
				ReleaseEvent (evtRef);
			}
			
			lock (carbonEvents) {
				if (carbonEvents.Count <= 0) {
					msg.hwnd = IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return true;
                                }
				MSG viewEvent = (MSG) carbonEvents.Dequeue ();
				msg.hwnd = viewEvent.hwnd;
				switch (viewEvent.message) {
					case Msg.WM_PAINT:
						NativeWindow.WndProc (msg.hwnd, Msg.WM_ERASEBKGND, msg.hwnd, IntPtr.Zero);
						msg.message = Msg.WM_PAINT;
						msg.wParam = IntPtr.Zero;
						msg.lParam = IntPtr.Zero;
						break;
					default:
						Console.WriteLine ("WARNING: Unknown view event kind on {0:x}: {1}", (int)msg.hwnd, viewEvent.message);
						msg.message = Msg.WM_ENTERIDLE;
						break;
				}
			}
			return true;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return true;
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			if (msg.message == Msg.WM_PAINT)
				Console.WriteLine ("Dispatching a WM_PAINT on {0:x}", (int)msg.hwnd);
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
			data.IsVisible = visible;

			CheckError (HIViewSetVisible (handle, visible));
			
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
			HIViewSetVisible (handle, true);
			HIViewAddSubview (parent, handle);
			Console.WriteLine ("Reparented {0:x} to {1:x}", (int)handle, (int)parent);
			return IntPtr.Zero;
		}

		internal override IntPtr GetParent(IntPtr handle) {
			return HIViewGetSuperview (handle);
		}

		[MonoTODO]
		internal override void GrabWindow(IntPtr hWnd, IntPtr confine_hwnd) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void ReleaseWindow(IntPtr hWnd) {
			throw new NotImplementedException ();
		}

		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect) {
			WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
			return true;
		}

		[MonoTODO]
		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void SendAsyncMethod (AsyncMethodData method) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void SetTimer (Timer timer) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override void KillTimer (Timer timer)
		{
			throw new NotImplementedException ();
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
		internal static extern int HIViewChangeFeatures (IntPtr aView, ulong bitsin, ulong bitsout);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewFindByID (IntPtr rootWnd, HIViewID id, ref IntPtr outPtr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetRoot (IntPtr hWnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIObjectCreate (IntPtr cfStr, uint what, ref IntPtr hwnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetFrame (IntPtr viewHnd, ref HIRect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewAddSubview (IntPtr parentHnd, IntPtr childHnd);
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
		internal static extern void SetRect (ref IntPtr r, short left, short top, short right, short bottom);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                static extern int InstallEventHandler (IntPtr window, ViewEventHandler handlerProc, uint numtypes, EventTypeSpec [] typeList, IntPtr userData, IntPtr handlerRef);
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
		static extern int GetEventParameter (IntPtr evt, uint inName, uint inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref IntPtr outData);


		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CreateNewWindow (int klass, uint attributes, ref IntPtr r, ref IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ShowWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SetWindowBounds (IntPtr wHnd, uint reg, ref IntPtr rect);

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
	}

	internal struct CGSize {
		public float width;
		public float height;

		public CGSize (int w, int h) {
			this.width = (float)w;
			this.height = (float)h;
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
}	
