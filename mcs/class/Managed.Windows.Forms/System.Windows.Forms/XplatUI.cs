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
//	Peter Bartok	pbartok@novell.com
//
//
// $Revision: 1.16 $
// $Modtime: $
// $Log: XplatUI.cs,v $
// Revision 1.16  2004/08/20 20:03:20  pbartok
// - Added method for setting the window background
//
// Revision 1.15  2004/08/20 19:14:35  jackson
// Expose functionality to send async messages through the driver
//
// Revision 1.14  2004/08/13 21:42:15  pbartok
// - Changed signature for GetCursorPos
//
// Revision 1.13  2004/08/13 19:00:15  jordi
// implements PointToClient (ScreenToClient)
//
// Revision 1.12  2004/08/13 18:53:14  pbartok
// - Changed GetWindowPos to also provide client area size
//
// Revision 1.11  2004/08/12 22:59:03  pbartok
// - Implemented method to get current mouse position
//
// Revision 1.10  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.9  2004/08/11 19:19:44  pbartok
// - We had SetWindowPos and MoveWindow to set window positions and size,
//   removed MoveWindow. We have GetWindowPos, so it made sense to keep
//   SetWindowPos as matching counterpart
// - Added some X11 sanity checking
//
// Revision 1.8  2004/08/11 18:55:46  pbartok
// - Added method to calculate difference between decorated window and raw
//   client area
//
// Revision 1.7  2004/08/10 17:39:22  pbartok
// - Added GetWindowPos method
//
// Revision 1.6  2004/08/09 20:55:59  pbartok
// - Removed Run method, was only required for initial development
//
// Revision 1.5  2004/08/09 20:51:25  pbartok
// - Implemented GrabWindow/ReleaseWindow methods to allow pointer capture
//
// Revision 1.4  2004/08/09 17:02:29  jackson
// Get default window properties from the theme
//
// Revision 1.3  2004/08/09 15:56:44  jackson
// Remove defaults, these are handled by the theme now.
//
// Revision 1.2  2004/08/04 20:11:24  pbartok
// - Added Invalidate handling
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

//
// Random architecture notes
// We need
// * windows
//   - create
//   - set location/size
//   - define cursor
//   - destroy
//   - reparent?
//   - show/hide
// * Keyboard
// * Mouse
//

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUI {
		#region Local Variables
		static XplatUIDriver		driver;
		static String			default_class_name;
		#endregion	// Local Variables

		#region Subclasses

		public class State {
			static public Keys ModifierKeys {
				get {
					return driver.ModifierKeys;
				}
			}

			static public MouseButtons MouseButtons {
				get {
					return driver.MouseButtons;
				}
			}

			static public Point MousePosition {
				get {
					return driver.MousePosition;
				}
			}

			static public bool DropTarget {
				get {
					return driver.DropTarget;
				}

				set {
					driver.DropTarget=value;
				}
			}
		}
		#endregion	// Subclasses

		#region Constructor & Destructor
		static XplatUI() {
			// Don't forget to throw the mac in here somewhere, too
			default_class_name="SWFClass";

			if (Environment.OSVersion.Platform == (PlatformID)128) {
				driver=XplatUIX11.GetInstance();
			} else {
				driver=XplatUIWin32.GetInstance();
			}

			Console.WriteLine("#region #line XplatUI Constructor called");
		}

		~XplatUI() {
			Console.WriteLine("XplatUI Destructor called");
		}
		#endregion	// Constructor & Destructor

		#region Public Static Properties
		internal static string DefaultClassName {
			get {
				return default_class_name;
			}

			set {
				default_class_name=value;
			}
		}
		#endregion	// Public Static Properties

		#region Public Static Methods
		internal static void Exit() {
			driver.Exit();
		}

		internal static void EnableThemes() {
			driver.EnableThemes();
		}

		internal static bool Text(IntPtr hWnd, string text) {
			return driver.Text(hWnd, text);
		}

		internal static bool SetVisible(IntPtr hWnd, bool visible) {
			return driver.SetVisible(hWnd, visible);
		}

		internal static bool IsVisible(IntPtr hWnd) {
			return driver.IsVisible(hWnd);
		}

		internal static IntPtr SetParent(IntPtr hWnd, IntPtr hParent) {
			return driver.SetParent(hWnd, hParent);
		}

		internal static IntPtr GetParent(IntPtr hWnd) {
			return driver.GetParent(hWnd);
		}

		internal static void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal static IntPtr CreateWindow(CreateParams cp) {
			return driver.CreateWindow(cp);
		}

		internal static IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			return driver.CreateWindow(Parent, X, Y, Width, Height);
		}

		internal static void DestroyWindow(IntPtr handle) {
			driver.DestroyWindow(handle);
		}

		internal static void RefreshWindow(IntPtr handle) {
			driver.RefreshWindow(handle);
		}

		internal static void SetWindowBackground(IntPtr handle, Color color) {
			driver.SetWindowBackground(handle, color);
		}
			
		internal static PaintEventArgs PaintEventStart(IntPtr handle) {
			return driver.PaintEventStart(handle);
		}

		internal static void PaintEventEnd(IntPtr handle) {
			driver.PaintEventEnd(handle);
		}

		internal static void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			driver.SetWindowPos(handle, x, y, width, height);
		}

		internal static void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			driver.GetWindowPos(handle, out x, out y, out width, out height, out client_width, out client_height);
		}

		internal static void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			driver.Invalidate(handle, rc, clear);
		}

		internal static void Activate(IntPtr handle) {
			driver.Activate(handle);
		}

		internal static IntPtr DefWndProc(ref Message msg) {
			return driver.DefWndProc(ref msg);
		}

		internal static void HandleException(Exception e) {
			driver.HandleException(e);
		}

		internal static void DoEvents() {
			driver.DoEvents();
		}

		internal static bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return driver.PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal static bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			return driver.GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
		}

		internal static bool TranslateMessage(ref MSG msg) {
			return driver.TranslateMessage(ref msg);
		}

		internal static bool DispatchMessage(ref MSG msg) {
			return driver.DispatchMessage(ref msg);
		}

		internal static void GrabWindow(IntPtr hWnd) {
			driver.GrabWindow(hWnd);
		}

		internal static void ReleaseWindow(IntPtr hWnd) {
			driver.ReleaseWindow(hWnd);
		}

		internal static bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect) {
			return driver.CalculateWindowRect(hWnd, ref ClientRect, Style, HasMenu, out WindowRect);
		}

		internal static void GetCursorPos(IntPtr handle, out int x, out int y) {
			driver.GetCursorPos(handle, out x, out y);
		}

		internal static void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			driver.ScreenToClient (handle, ref x, ref y);
		}

		internal static void SendAsyncMethod (AsyncMethodData data) {
			driver.SendAsyncMethod (data);
		}

		// Santa's little helper
		internal static void Where() {
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		#endregion	// Public Static Methods

	}
}
