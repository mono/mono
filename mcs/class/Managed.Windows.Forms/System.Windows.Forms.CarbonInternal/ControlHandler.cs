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
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.CarbonInternal {
	internal class ControlHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventControlInitialize = 1000;
		internal const uint kEventControlDispose = 1001;
		internal const uint kEventControlGetOptimalBounds = 1003;
		internal const uint kEventControlDefInitialize = kEventControlInitialize;
		internal const uint kEventControlDefDispose = kEventControlDispose;
		internal const uint kEventControlHit = 1;
		internal const uint kEventControlSimulateHit = 2;
		internal const uint kEventControlHitTest = 3;
		internal const uint kEventControlDraw = 4;
		internal const uint kEventControlApplyBackground = 5;
		internal const uint kEventControlApplyTextColor = 6;
		internal const uint kEventControlSetFocusPart = 7;
		internal const uint kEventControlGetFocusPart = 8;
		internal const uint kEventControlActivate = 9;
		internal const uint kEventControlDeactivate = 10;
		internal const uint kEventControlSetCursor = 11;
		internal const uint kEventControlContextualMenuClick = 12;
		internal const uint kEventControlClick = 13;
		internal const uint kEventControlGetNextFocusCandidate = 14;
		internal const uint kEventControlGetAutoToggleValue = 15;
		internal const uint kEventControlInterceptSubviewClick = 16;
		internal const uint kEventControlGetClickActivation = 17;
		internal const uint kEventControlDragEnter = 18;
		internal const uint kEventControlDragWithin = 19;
		internal const uint kEventControlDragLeave = 20;
		internal const uint kEventControlDragReceive = 21;
		internal const uint kEventControlInvalidateForSizeChange = 22;
		internal const uint kEventControlTrackingAreaEntered = 23;
		internal const uint kEventControlTrackingAreaExited = 24;
		internal const uint kEventControlTrack = 51;
		internal const uint kEventControlGetScrollToHereStartPoint = 52;
		internal const uint kEventControlGetIndicatorDragConstraint = 53;
		internal const uint kEventControlIndicatorMoved = 54;
		internal const uint kEventControlGhostingFinished = 55;
		internal const uint kEventControlGetActionProcPart = 56;
		internal const uint kEventControlGetPartRegion = 101;
		internal const uint kEventControlGetPartBounds = 102;
		internal const uint kEventControlSetData = 103;
		internal const uint kEventControlGetData = 104;
		internal const uint kEventControlGetSizeConstraints= 105;
		internal const uint kEventControlGetFrameMetrics = 106;
		internal const uint kEventControlValueFieldChanged = 151;
		internal const uint kEventControlAddedSubControl = 152;
		internal const uint kEventControlRemovingSubControl = 153;
		internal const uint kEventControlBoundsChanged = 154;
		internal const uint kEventControlVisibilityChanged = 157;
		internal const uint kEventControlTitleChanged = 158;
		internal const uint kEventControlOwningWindowChanged = 159;
		internal const uint kEventControlHiliteChanged = 160;
		internal const uint kEventControlEnabledStateChanged = 161;
		internal const uint kEventControlLayoutInfoChanged = 162;
		internal const uint kEventControlArbitraryMessage = 201;

		internal const uint kEventParamCGContextRef = 1668183160;
		internal const uint kEventParamDirectObject = 757935405;
		internal const uint kEventParamMouseButton = 1835168878;
		internal const uint kEventParamMouseLocation = 1835822947;
		internal const uint typeControlRef = 1668575852;
		internal const uint typeCGContextRef = 1668183160;
		internal const uint typeMouseButton = 1835168878;
		internal const uint typeQDPoint = 1363439732;

		internal ControlHandler (XplatUICarbon driver) : base (driver) {}

		public bool ProcessEvent (IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
			Hwnd hwnd;
			bool client;

			GetEventParameter (eventref, kEventParamDirectObject, typeControlRef, IntPtr.Zero, (uint) Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref handle);
			hwnd = Hwnd.ObjectFromHandle (handle);

			if (hwnd == null)
				return false;

			msg.hwnd = hwnd.Handle;
			client = (hwnd.ClientWindow == handle ? true : false);

			switch (kind) {
				case kEventControlDraw: {
					HIRect view_bounds = new HIRect ();

					HIViewGetBounds (handle, ref view_bounds);
					Driver.AddExpose (hwnd, client, view_bounds);
					DrawBackground (hwnd, eventref, view_bounds);
					if (!client)
						DrawBorders (hwnd);
					
					break;
				}
				case kEventControlBoundsChanged: {
					HIRect view_frame = new HIRect ();

					HIViewGetFrame (handle, ref view_frame);
					if (!client) {
						hwnd.X = (int) view_frame.origin.x;
						hwnd.Y = (int) view_frame.origin.y;
						hwnd.Width = (int) view_frame.size.width;
						hwnd.Height = (int) view_frame.size.height;
						Driver.PerformNCCalc (hwnd);
					}
					return false;
				}
				case kEventControlClick: {
					QDPoint point = new QDPoint ();
					ushort button = 0;

					if (XplatUICarbon.GrabHwnd != null) {
						hwnd = XplatUICarbon.GrabHwnd; 
						msg.hwnd = hwnd.Handle;
						client = true;
					}
					
					GetEventParameter (eventref, kEventParamMouseLocation, typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point);
					GetEventParameter (eventref, kEventParamMouseButton, typeMouseButton, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref button);

					Driver.ScreenToClient (hwnd.Handle, ref point);

					msg.wParam = ButtonTowParam (button);
					msg.lParam = (IntPtr) ((ushort) point.y << 16 | (ushort) point.x);
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE) + ((button - 1) * 3) + 1;

					Driver.mouse_position.X = (int) point.x;
					Driver.mouse_position.Y = (int) point.y;

					NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
					
					HandleControlClick (handle, point, 0, IntPtr.Zero);
					break;
				}
				case kEventControlTrack: {
					QDPoint point = new QDPoint ();
					MouseTrackingResult mousestatus = MouseTrackingResult.kMouseTrackingMouseDown;

					if (XplatUICarbon.GrabHwnd != null) {
						hwnd = XplatUICarbon.GrabHwnd; 
						client = true;
						msg.hwnd = hwnd.Handle;
					}

					GetEventParameter (eventref, kEventParamMouseLocation, typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point);
					
					while (mousestatus != MouseTrackingResult.kMouseTrackingMouseUp) {
						uint modifiers = 0; 

						if (mousestatus == MouseTrackingResult.kMouseTrackingMouseDragged) {
							Driver.ScreenToClient (hwnd.Handle, ref point);
							NativeWindow.WndProc (hwnd.Handle, (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE), Driver.GetMousewParam (0), (IntPtr) ((ushort) point.y << 16 | (ushort) point.x));
						}
						Driver.FlushQueue ();

						TrackMouseLocationWithOptions (IntPtr.Zero, 0, 0.01, ref point, ref modifiers, ref mousestatus);
					}
					Driver.ScreenToClient (hwnd.Handle, ref point);
					
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE) + ((StateToButton (XplatUICarbon.MouseState) - 1) * 3) + 2;
					msg.wParam = StateTowParam (XplatUICarbon.MouseState);
					msg.lParam = (IntPtr) ((ushort) point.y << 16 | (ushort) point.x);

					Driver.mouse_position.X = (int) point.x;
					Driver.mouse_position.Y = (int) point.y;

					return true;
				}
			}
			return false;
		}

		private int StateToButton (MouseButtons state) {
			int button = 0;

			switch (state) {
				case MouseButtons.Left: 
					button = 1;
					break;
				case MouseButtons.Right: 
					button = 2;
					break;
				case MouseButtons.Middle:
					button = 3;
					break;
			}
			
			return button;
		}

		private IntPtr StateTowParam (MouseButtons state) {
			int wparam = (int) Driver.GetMousewParam (0);

			switch (state) {
				case MouseButtons.Left: 
					XplatUICarbon.MouseState &= ~MouseButtons.Left;
					wparam &= (int)MsgButtons.MK_LBUTTON;
					break;
				case MouseButtons.Right: 
					XplatUICarbon.MouseState &= ~MouseButtons.Right;
					wparam &= (int)MsgButtons.MK_RBUTTON;
					break;
				case MouseButtons.Middle:
					XplatUICarbon.MouseState &= ~MouseButtons.Middle;
					wparam &= (int)MsgButtons.MK_MBUTTON;
					break;
			}
			
			return (IntPtr) wparam;
		}

		private IntPtr ButtonTowParam (ushort button) {
			int wparam = (int) Driver.GetMousewParam (0);

			switch (button) {
				case 1:
					XplatUICarbon.MouseState |= MouseButtons.Left;
					wparam |= (int)MsgButtons.MK_LBUTTON;
					break;
				case 2:
					XplatUICarbon.MouseState |= MouseButtons.Right;
					wparam |= (int)MsgButtons.MK_RBUTTON;
					break;
				case 3:
					XplatUICarbon.MouseState |= MouseButtons.Middle;
					wparam |= (int)MsgButtons.MK_MBUTTON;
					break;
			}
			
			return (IntPtr) wparam;
		}

		private void DrawBackground (Hwnd hwnd, IntPtr eventref, HIRect bounds) {
			if (XplatUICarbon.WindowBackgrounds [hwnd] != null) {
				IntPtr context = IntPtr.Zero;
				Color color = (Color) XplatUICarbon.WindowBackgrounds [hwnd];

				GetEventParameter (eventref, kEventParamCGContextRef, typeCGContextRef, IntPtr.Zero, (uint) Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref context); 
				
				CGContextSetRGBFillColor (context, (float) color.R / 255, (float) color.G / 255, (float) color.B / 255, (float) color.A / 255);
				CGContextFillRect (context, bounds);
			}
		}

		private void DrawBorders (Hwnd hwnd) {
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
		}
			
/*



				case Constants.kEventControlSetFocusPart: {
					// This handles setting focus
					short pcode = 1;
					GetEventParameter (inEvent, Constants.EventParamName.kEventParamControlPart, Constants.EventParamType.typeControlPartCode, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (short)), IntPtr.Zero, ref pcode);
					switch (pcode) {
						case 0:
						case -1:
						case -2:
							pcode = 0;
							break;
					}
					SetEventParameter (inEvent, Constants.EventParamName.kEventParamControlPart, Constants.EventParamType.typeControlPartCode, (uint)Marshal.SizeOf (typeof (short)), ref pcode);
					return 0;
				}
			}
			return -9874;
		}
*/
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref IntPtr data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref ushort data);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr eventref, uint name, uint type, IntPtr outtype, uint size, IntPtr outsize, ref QDPoint data);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int HIViewGetBounds (IntPtr handle, ref HIRect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int HIViewGetFrame (IntPtr handle, ref HIRect rect);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern void HandleControlClick (IntPtr handle, QDPoint point, uint modifiers, IntPtr callback);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern void TrackMouseLocationWithOptions (IntPtr port, int options, double eventtimeout, ref QDPoint point, ref uint modifier, ref MouseTrackingResult status);
		
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int CGContextSetRGBFillColor (IntPtr cgContext, float r, float g, float b, float alpha);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int CGContextFillRect (IntPtr context, HIRect rect);

	}
}
