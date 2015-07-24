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
		internal const uint kEventMouseScroll = 11;

		internal const uint kEventParamMouseLocation = 1835822947;
		internal const uint kEventParamMouseButton = 1835168878;
		internal const uint kEventParamMouseWheelAxis = 1836540280;
		internal const uint kEventParamMouseWheelDelta = 1836541036;
		internal const uint typeLongInteger = 1819242087;
		internal const uint typeMouseWheelAxis = 1836540280;
		internal const uint typeMouseButton = 1835168878;
		internal const uint typeQDPoint = 1363439732;

		internal const uint kEventMouseWheelAxisX = 0;
		internal const uint kEventMouseWheelAxisY = 1;

		internal const uint DoubleClickInterval = 7500000;
		internal static ClickStruct ClickPending;
		
		internal MouseHandler (XplatUICarbon driver) : base (driver) {}

		public bool ProcessEvent (IntPtr callref, IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
			QDPoint qdpoint = new QDPoint ();
			CGPoint point = new CGPoint ();
			Rect window_bounds = new Rect ();
			IntPtr view_handle = IntPtr.Zero;
			IntPtr window_handle = IntPtr.Zero;
			bool client = true;
			ushort button = 0;
			Hwnd hwnd;

			GetEventParameter (eventref, kEventParamMouseLocation, typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref qdpoint);
			GetEventParameter (eventref, kEventParamMouseButton, typeMouseButton, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref button);
			
			if (button == 1 && ((Driver.ModifierKeys & Keys.Control) != 0))
				button = 2;

			point.x = qdpoint.x;
			point.y = qdpoint.y;

			if (FindWindow (qdpoint, ref window_handle) == 5)
				return true;

			GetWindowBounds (handle, 33, ref window_bounds);
			HIViewFindByID (HIViewGetRoot (handle), new HIViewID (EventHandler.kEventClassWindow, 1), ref window_handle);

			point.x -= window_bounds.left;
			point.y -= window_bounds.top;

			HIViewGetSubviewHit (window_handle, ref point, true, ref view_handle);
			HIViewConvertPoint (ref point, window_handle, view_handle);

			hwnd = Hwnd.ObjectFromHandle (view_handle);

			if (hwnd != null)
				client = (hwnd.ClientWindow == view_handle ? true : false);

			if (XplatUICarbon.Grab.Hwnd != IntPtr.Zero) {
				hwnd = Hwnd.ObjectFromHandle (XplatUICarbon.Grab.Hwnd); 
				client = true;
			}
			if (hwnd == null)
				return true;
			
			if (client) {
				qdpoint.x = (short) point.x;
				qdpoint.y = (short) point.y;

				Driver.ScreenToClient (hwnd.Handle, ref qdpoint);
			} else {
				point.x = qdpoint.x;
				point.y = qdpoint.y;
			}

			msg.hwnd = hwnd.Handle;
			msg.lParam = (IntPtr) ((ushort) point.y << 16 | (ushort) point.x);

			switch (kind) {
				case kEventMouseDown:
					UpdateMouseState (button, true);
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE) + ((button - 1) * 3) + 1;
					msg.wParam = Driver.GetMousewParam (0);
					if (ClickPending.Pending && (((DateTime.Now.Ticks - ClickPending.Time) < DoubleClickInterval) && (msg.hwnd == ClickPending.Hwnd) && (msg.wParam == ClickPending.wParam) && (msg.lParam == ClickPending.lParam) && (msg.message == ClickPending.Message))) {
						msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE) + ((button - 1) * 3) + 3;
						ClickPending.Pending = false;
					} else {
						ClickPending.Pending = true;
						ClickPending.Hwnd = msg.hwnd;
						ClickPending.Message = msg.message;
						ClickPending.wParam = msg.wParam;
						ClickPending.lParam = msg.lParam;
						ClickPending.Time = DateTime.Now.Ticks;
					}
					break;
				case kEventMouseUp:
					UpdateMouseState (button, false);
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE) + ((button - 1) * 3) + 2;
					msg.wParam = Driver.GetMousewParam (0);
					break;
				case kEventMouseDragged:
				case kEventMouseMoved:
					if (XplatUICarbon.Grab.Hwnd == IntPtr.Zero) {
						IntPtr ht = IntPtr.Zero;
						if (client) {
							ht = (IntPtr) HitTest.HTCLIENT;
							NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);
						} else {
	                                                ht = (IntPtr) NativeWindow.WndProc (hwnd.client_window, Msg.WM_NCHITTEST, IntPtr.Zero, msg.lParam).ToInt32 ();
							NativeWindow.WndProc(hwnd.client_window, Msg.WM_SETCURSOR, msg.hwnd, ht);
						}
					}
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE);
					msg.wParam = Driver.GetMousewParam (0);
					break;
				case kEventMouseWheelMoved:
				case kEventMouseScroll:
					UInt16 axis = 0;
					Int32 delta = 0;

					GetEventParameter (eventref, kEventParamMouseWheelAxis, typeMouseWheelAxis, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (UInt16)), IntPtr.Zero, ref axis);
					GetEventParameter (eventref, kEventParamMouseWheelDelta, typeLongInteger, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (Int32)), IntPtr.Zero, ref delta);

					if (axis == kEventMouseWheelAxisY) {
						msg.hwnd = XplatUICarbon.FocusWindow;
						msg.message = Msg.WM_MOUSEWHEEL;
						msg.wParam = Driver.GetMousewParam (delta*40);
						return true;
					}
					break;
				default:
					return false;
			}
			Driver.mouse_position.X = (int) point.x;
			Driver.mouse_position.Y = (int) point.y;
			return true;
		}
		
		internal bool TranslateMessage (ref MSG msg) {
			if (msg.message == Msg.WM_MOUSEMOVE || msg.message == Msg.WM_NCMOUSEMOVE) {
				Hwnd hwnd = Hwnd.ObjectFromHandle (msg.hwnd);
				if (XplatUICarbon.MouseHwnd == null) { 
					Driver.PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
					Cursor.SetCursor (hwnd.Cursor);
				} else if (XplatUICarbon.MouseHwnd.Handle != hwnd.Handle) {
					Driver.PostMessage (XplatUICarbon.MouseHwnd.Handle, Msg.WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero);
					Driver.PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
					Cursor.SetCursor (hwnd.Cursor);
				}
				XplatUICarbon.MouseHwnd = hwnd;
			}
			
			return false;
		}

		private void UpdateMouseState (int button, bool down) {
			switch (button) {
				case 1:
					if (down) XplatUICarbon.MouseState |= MouseButtons.Left; else XplatUICarbon.MouseState &= ~MouseButtons.Left;
					break;
				case 2:
					if (down) XplatUICarbon.MouseState |= MouseButtons.Right; else XplatUICarbon.MouseState &= ~MouseButtons.Right;
					break;
				case 3:
					if (down) XplatUICarbon.MouseState |= MouseButtons.Middle; else XplatUICarbon.MouseState &= ~MouseButtons.Middle;
					break;
			}
		}

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref QDPoint data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref Int32 data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref ushort data);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern short FindWindow (QDPoint point, ref IntPtr handle);
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
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetCurrentEventButtonState ();
	}
}
