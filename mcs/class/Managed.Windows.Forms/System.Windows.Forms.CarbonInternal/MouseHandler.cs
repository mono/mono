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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@novell.com>
//
//

using System;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.CarbonInternal {
	internal class MouseHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventMouseDown = 1;
		internal const uint kEventMouseUp = 2;
		internal const uint kEventMouseMoved = 5;
		internal const uint kEventMouseDragged = 6;
		internal const uint kEventMouseEntered = 8;
		internal const uint kEventMouseExited = 9;
		internal const uint kEventMouseWheelMoved = 10;

		internal const uint kEventParamMouseLocation = 1835822947;
		internal const uint typeQDPoint = 1363439732;
		
		internal MouseHandler (XplatUICarbon driver) : base (driver) {}

		public bool ProcessEvent (IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
			QDPoint qdpoint = new QDPoint ();
			CGPoint point = new CGPoint ();

			GetEventParameter (eventref, kEventParamMouseLocation, typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref qdpoint);

			point.x = qdpoint.x;
			point.y = qdpoint.y;

			switch (kind) {
				case kEventMouseMoved: {
					Rect window_bounds = new Rect ();
					IntPtr view_handle = IntPtr.Zero;
					IntPtr window_handle = IntPtr.Zero;
					bool client = true;
					Hwnd hwnd;

					GetWindowBounds (handle, 33, ref window_bounds);
					HIViewFindByID (HIViewGetRoot (handle), new HIViewID (EventHandler.kEventClassWindow, 1), ref window_handle);

					point.x -= window_bounds.left;
					point.y -= window_bounds.top;

					HIViewGetSubviewHit (window_handle, ref point, true, ref view_handle);
					HIViewConvertPoint (ref point, window_handle, view_handle);

					hwnd = Hwnd.ObjectFromHandle (view_handle);
					if (XplatUICarbon.GrabHwnd != null)
						hwnd = XplatUICarbon.GrabHwnd; 
					if (hwnd == null)
						return false;

					client = (hwnd.ClientWindow == view_handle ? true : hwnd == XplatUICarbon.GrabHwnd ? true : false);

					msg.hwnd = hwnd.Handle;
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE);
					msg.lParam = (IntPtr) ((ushort)point.y << 16 | (ushort)point.x);
					msg.wParam = Driver.GetMousewParam (0);
					
					Driver.mouse_position.X = (int)point.x;
					Driver.mouse_position.Y = (int)point.y;
					XplatUICarbon.Hover.Hwnd = hwnd.Handle;
					XplatUICarbon.Hover.Timer.Enabled = true;
					
					return true;
				}
			}
			return false;
		}
		
		internal bool TranslateMessage (ref MSG msg) {
			if (msg.message == Msg.WM_MOUSEMOVE || msg.message == Msg.WM_NCMOUSEMOVE) {
				Hwnd hwnd = Hwnd.ObjectFromHandle (msg.hwnd);
				if (XplatUICarbon.MouseHwnd == null) 
					Driver.PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
				else if (XplatUICarbon.MouseHwnd.Handle != hwnd.Handle) {
					Driver.PostMessage (XplatUICarbon.MouseHwnd.Handle, Msg.WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero);
					Driver.PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
				}
				XplatUICarbon.MouseHwnd = hwnd;
			}
			
			return false;
		}

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref QDPoint data);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowBounds (IntPtr handle, uint region, ref Rect bounds);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewConvertPoint (ref CGPoint point, IntPtr source_view, IntPtr target_view);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetRoot (IntPtr handle);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetSubviewHit (IntPtr content_view, ref CGPoint point, bool tval, ref IntPtr hit_view);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewFindByID (IntPtr root_window, HIViewID id, ref IntPtr view_handle);
	}
}
