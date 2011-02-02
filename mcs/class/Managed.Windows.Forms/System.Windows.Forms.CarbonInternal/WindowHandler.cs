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
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace System.Windows.Forms.CarbonInternal {
	internal class WindowHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventWindowUpdate = 1;
		internal const uint kEventWindowDrawContent = 2;
		internal const uint kEventWindowActivated = 5;
		internal const uint kEventWindowDeactivated = 6;
		internal const uint kEventWindowGetClickActivation = 7;
		internal const uint kEventWindowShowing = 22;
		internal const uint kEventWindowHiding = 23;
		internal const uint kEventWindowShown = 24;
		internal const uint kEventWindowHidden = 25;
		internal const uint kEventWindowCollapsing = 86;
		internal const uint kEventWindowExpanding = 87;
		internal const uint kEventWindowZoomed = 76;
		internal const uint kEventWindowBoundsChanging = 26;
		internal const uint kEventWindowBoundsChanged = 27;
		internal const uint kEventWindowResizeStarted = 28;
		internal const uint kEventWindowResizeCompleted = 29;
		internal const uint kEventWindowDragStarted = 30;
		internal const uint kEventWindowDragCompleted = 31;
		internal const uint kEventWindowTransitionStarted = 88;
		internal const uint kEventWindowTransitionCompleted = 89;
		internal const uint kEventWindowClickDragRgn = 32;
		internal const uint kEventWindowClickResizeRgn = 33;
		internal const uint kEventWindowClickCollapseRgn = 34;
		internal const uint kEventWindowClickCloseRgn = 35;
		internal const uint kEventWindowClickZoomRgn = 36;
		internal const uint kEventWindowClickContentRgn = 37;
		internal const uint kEventWindowClickProxyIconRgn = 38;
		internal const uint kEventWindowClickToolbarButtonRgn = 41;
		internal const uint kEventWindowClickStructureRgn = 42;
		internal const uint kEventWindowCursorChange = 40;
		internal const uint kEventWindowCollapse = 66;
		internal const uint kEventWindowCollapsed = 67;
		internal const uint kEventWindowCollapseAll = 68;
		internal const uint kEventWindowExpand = 69;
		internal const uint kEventWindowExpanded = 70;
		internal const uint kEventWindowExpandAll = 71;
		internal const uint kEventWindowClose = 72;
		internal const uint kEventWindowClosed = 73;
		internal const uint kEventWindowCloseAll = 74;
		internal const uint kEventWindowZoom = 75;
		internal const uint kEventWindowZoomAll = 77;
		internal const uint kEventWindowContextualMenuSelect = 78;
		internal const uint kEventWindowPathSelect = 79;
		internal const uint kEventWindowGetIdealSize = 80;
		internal const uint kEventWindowGetMinimumSize = 81;
		internal const uint kEventWindowGetMaximumSize = 82;
		internal const uint kEventWindowConstrain = 83;
		internal const uint kEventWindowHandleContentClick = 85;
		internal const uint kEventWindowGetDockTileMenu = 90;
		internal const uint kEventWindowHandleActivate = 91;
		internal const uint kEventWindowHandleDeactivate = 92;
		internal const uint kEventWindowProxyBeginDrag = 128;
		internal const uint kEventWindowProxyEndDrag = 129;
		internal const uint kEventWindowToolbarSwitchMode = 150;
		internal const uint kEventWindowFocusAcquired = 200;
		internal const uint kEventWindowFocusRelinquish = 201;
		internal const uint kEventWindowFocusContent = 202;
		internal const uint kEventWindowFocusToolbar = 203;
		internal const uint kEventWindowDrawerOpening = 220;
		internal const uint kEventWindowDrawerOpened = 221;
		internal const uint kEventWindowDrawerClosing = 222;
		internal const uint kEventWindowDrawerClosed = 223;
		internal const uint kEventWindowDrawFrame = 1000;
		internal const uint kEventWindowDrawPart = 1001;
		internal const uint kEventWindowGetRegion = 1002;
		internal const uint kEventWindowHitTest = 1003;
		internal const uint kEventWindowInit = 1004;
		internal const uint kEventWindowDispose = 1005;
		internal const uint kEventWindowDragHilite = 1006;
		internal const uint kEventWindowModified = 1007;
		internal const uint kEventWindowSetupProxyDragImage = 1008;
		internal const uint kEventWindowStateChanged = 1009;
		internal const uint kEventWindowMeasureTitle = 1010;
		internal const uint kEventWindowDrawGrowBox = 1011;
		internal const uint kEventWindowGetGrowImageRegion = 1012;
		internal const uint kEventWindowPaint = 1013;
		
		internal WindowHandler (XplatUICarbon driver) : base (driver) {}

		public bool ProcessEvent (IntPtr callref, IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
			IntPtr window = Driver.HandleToWindow (handle);
			Hwnd hwnd = Hwnd.ObjectFromHandle (window);
			if (window != IntPtr.Zero) {
				switch (kind) {
					case kEventWindowActivated: {
						Control c = Control.FromHandle (hwnd.client_window);
						if (c != null) {
							Form form = c.FindForm ();
							if (form != null && !form.IsDisposed) {
								Driver.SendMessage (form.Handle, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
								XplatUICarbon.ActiveWindow = hwnd.client_window;
							}
						}

						foreach (IntPtr utility_window in XplatUICarbon.UtilityWindows) {
							if (utility_window != handle && !XplatUICarbon.IsWindowVisible (utility_window))
								XplatUICarbon.ShowWindow (utility_window);
						}	
						break;
					}
					case kEventWindowExpanding:
						foreach (IntPtr utility_window in XplatUICarbon.UtilityWindows) {
							if (utility_window != handle && !XplatUICarbon.IsWindowVisible (utility_window))
								XplatUICarbon.ShowWindow (utility_window);
						}
						msg.hwnd = hwnd.Handle;
						msg.message = Msg.WM_ENTERSIZEMOVE;
						return true;
					case kEventWindowExpanded:
						NativeWindow.WndProc (hwnd.Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
						msg.hwnd = hwnd.Handle;
						msg.message = Msg.WM_EXITSIZEMOVE;
						return true;
					case kEventWindowDeactivated: {
						Control c = Control.FromHandle (hwnd.client_window);
						if (c != null) {
							Form form = c.FindForm ();
							if (form != null && XplatUICarbon.UnactiveWindow != form.Handle) {
								Driver.SendMessage (form.Handle, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
								XplatUICarbon.ActiveWindow = IntPtr.Zero;
							}
						}
						foreach (IntPtr utility_window in XplatUICarbon.UtilityWindows) {
							if (utility_window != handle && XplatUICarbon.IsWindowVisible (utility_window))
								XplatUICarbon.HideWindow (utility_window);
						}
						break;
					}
					case kEventWindowCollapsing:
						foreach (IntPtr utility_window in XplatUICarbon.UtilityWindows) {
							if (utility_window != handle && XplatUICarbon.IsWindowVisible (utility_window))
								XplatUICarbon.HideWindow (utility_window);
						}	
						msg.hwnd = hwnd.Handle;
						msg.message = Msg.WM_ENTERSIZEMOVE;
						return true;
					case kEventWindowCollapsed:
						NativeWindow.WndProc (hwnd.Handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
						msg.hwnd = hwnd.Handle;
						msg.message = Msg.WM_EXITSIZEMOVE;
						return true;
					case kEventWindowClose:
						NativeWindow.WndProc (hwnd.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
						return false;
					case kEventWindowShown: { 
						msg.message = Msg.WM_SHOWWINDOW;
						msg.lParam = (IntPtr) 1;
						msg.wParam = (IntPtr) 0;
						msg.hwnd = hwnd.Handle;

						return true;
					}
					case kEventWindowResizeStarted: {
						msg.message = Msg.WM_ENTERSIZEMOVE;
						msg.hwnd = hwnd.Handle;
						return true;
					}
					case kEventWindowResizeCompleted: {
						msg.message = Msg.WM_EXITSIZEMOVE;
						msg.hwnd = hwnd.Handle;

						return true;
					}
					case kEventWindowBoundsChanged: {
						Rect window_bounds = new Rect ();
						HIRect view_bounds = new HIRect ();
						Size size;

						GetWindowBounds (handle, 33, ref window_bounds);
						
						view_bounds.size.width = window_bounds.right - window_bounds.left;
						view_bounds.size.height = window_bounds.bottom - window_bounds.top;

						HIViewSetFrame (hwnd.WholeWindow, ref view_bounds);

						size = XplatUICarbon.TranslateQuartzWindowSizeToWindowSize (Control.FromHandle (hwnd.Handle).GetCreateParams (), (int)view_bounds.size.width, (int)view_bounds.size.height);

						hwnd.X = (int) window_bounds.left;
						hwnd.Y = (int) window_bounds.top;
						hwnd.Width = (int) size.Width;
						hwnd.Height = (int) size.Height;

						Driver.PerformNCCalc (hwnd);

						msg.hwnd = hwnd.Handle;
						msg.message = Msg.WM_WINDOWPOSCHANGED;
						Driver.SetCaretPos (XplatUICarbon.Caret.Hwnd, XplatUICarbon.Caret.X, XplatUICarbon.Caret.Y);

						return true;
					}
				}
			}
			return false;
		}

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetWindowBounds (IntPtr handle, uint region, ref Rect bounds);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int HIViewSetFrame (IntPtr handle, ref HIRect bounds);
	}
}
