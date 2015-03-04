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

namespace System.Windows.Forms.CarbonInternal {
	internal class ApplicationHandler : EventHandlerBase, IEventHandler {
		internal const uint kEventAppActivated = 1;
		internal const uint kEventAppDeactivated = 2;
		internal const uint kEventAppQuit = 3;
		internal const uint kEventAppLaunchNotification = 4;
		internal const uint kEventAppLaunched = 5;
		internal const uint kEventAppTerminated = 6;
		internal const uint kEventAppFrontSwitched = 7;
		internal const uint kEventAppFocusMenuBar = 8;
		internal const uint kEventAppFocusNextDocumentWindow = 9;
		internal const uint kEventAppFocusNextFloatingWindow = 10;
		internal const uint kEventAppFocusToolbar = 11;
		internal const uint kEventAppFocusDrawer = 12;
		internal const uint kEventAppGetDockTileMenu = 20;
		internal const uint kEventAppIsEventInInstantMouser = 104;
		internal const uint kEventAppHidden = 107;
		internal const uint kEventAppShown = 108;
		internal const uint kEventAppSystemUIModeChanged = 109;
		internal const uint kEventAppAvailableWindowBoundsChanged = 110;
		internal const uint kEventAppActiveWindowChanged = 111;

		internal ApplicationHandler (XplatUICarbon driver) : base (driver) {}

		public bool ProcessEvent (IntPtr callref, IntPtr eventref, IntPtr handle, uint kind, ref MSG msg) {
			switch (kind) {
				case kEventAppActivated: {
					foreach (IntPtr utility_window in XplatUICarbon.UtilityWindows)
						if (!XplatUICarbon.IsWindowVisible (utility_window))
							XplatUICarbon.ShowWindow (utility_window);
					break;
				}
				case kEventAppDeactivated: {
					if (XplatUICarbon.FocusWindow != IntPtr.Zero) {
						Driver.SendMessage (XplatUICarbon.FocusWindow, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
					} 
					if (XplatUICarbon.Grab.Hwnd != IntPtr.Zero) {
						Driver.SendMessage (Hwnd.ObjectFromHandle (XplatUICarbon.Grab.Hwnd).Handle, Msg.WM_LBUTTONDOWN, (IntPtr)MsgButtons.MK_LBUTTON, (IntPtr) (Driver.MousePosition.X << 16 | Driver.MousePosition.Y));
					}
					foreach (IntPtr utility_window in XplatUICarbon.UtilityWindows)
						if (XplatUICarbon.IsWindowVisible (utility_window))
							XplatUICarbon.HideWindow (utility_window);
					break;
				}
			}

			return true;
		}
	}
}
