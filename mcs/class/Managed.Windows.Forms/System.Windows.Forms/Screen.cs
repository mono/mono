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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOTE: We made a concious decision to have only a single 'screen'
// due to the differences in platforms. On Win32 we could gather
// all information, but not for X11 (and possibly Mac). So for now
// we'll stick with a single screen, but the functions are still 
// written to support multiple screens, simply beef up the all_screens 
// assignment to get multiples
// To support multiples, we need to use GetMonitorInfo API on Win32

using System;
using System.Drawing;

namespace System.Windows.Forms {
	public class Screen {
		#region Local Variables
		private static Screen[] all_screens = { new Screen(true, "Mono MWF Primary Display", SystemInformation.VirtualScreen, SystemInformation.WorkingArea) };
		private bool		primary;
		private Rectangle	bounds;
		private Rectangle	workarea;
		private string		name;
		private int		bits_per_pixel;
		#endregion	// Local Variables

		#region	Constructors
		private Screen() {
			this.primary = true;
			this.bounds = SystemInformation.WorkingArea;
		}

		private Screen(bool primary, string name, Rectangle bounds, Rectangle workarea) {
			this.primary = primary;
			this.name = name;
			this.bounds = bounds;
			this.workarea = workarea;
			this.bits_per_pixel = 32;
		}
		#endregion	// Constructors

		#region Public Static Properties
		public static Screen[] AllScreens {
			get {
				return all_screens;
			}
		}

		public static Screen PrimaryScreen {
			get {
				return all_screens[0];
			}
		}
		#endregion	// Public Static Properties

		#region Public Instance Properties
		[MonoTODO ("Stub, always returns 32")]
		public int BitsPerPixel {
			get { return bits_per_pixel; }
		}

		public Rectangle Bounds {
			get {
				return this.bounds;
			}
		}

		public string DeviceName {
			get {
				return this.name;
			}
		}

		public bool Primary {
			get {
				return this.primary;
			}
		}

		public Rectangle WorkingArea {
			get {
				return this.workarea;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static Screen FromControl(Control control) {
			var point = control.Parent != null ? control.Parent.PointToScreen(control.Location) : control.Location;
			return Screen.FromPoint(point);
		}

		public static Screen FromHandle(IntPtr hwnd) {
			Control control;

			control = Control.FromHandle(hwnd);
			if (control != null) {
				var point = control.Parent != null ? control.Parent.PointToScreen(control.Location) : control.Location;
				return Screen.FromPoint(point);
			}
			return Screen.PrimaryScreen;
		}

		public static Screen FromPoint(Point point) {
			for (int i = 0; i < all_screens.Length; i++) {
				if (all_screens[i].Bounds.Contains(point)) {
					return all_screens[i];
				}
			}
			return Screen.PrimaryScreen;
		}

		public static Screen FromRectangle(Rectangle rect) {
			return Screen.FromPoint(new Point(rect.Left, rect.Top));
		}

		public static Rectangle GetBounds(Control ctl) {
			return Screen.FromControl(ctl).Bounds;
		}

		public static Rectangle GetBounds(Point pt) {
			return Screen.FromPoint(pt).Bounds;
		}

		public static Rectangle GetBounds(Rectangle rect) {
			return Screen.FromRectangle(rect).Bounds;
		}

		public static Rectangle GetWorkingArea(Control ctl) {
			return Screen.FromControl(ctl).WorkingArea;
		}

		public static Rectangle GetWorkingArea(Point pt) {
			return Screen.FromPoint(pt).WorkingArea;
		}

		public static Rectangle GetWorkingArea(Rectangle rect) {
			return Screen.FromRectangle(rect).WorkingArea;
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public override bool Equals(object obj) {
			if (obj is Screen) {
				Screen s = (Screen)obj;

				if (name.Equals(s.name) && (primary == s.primary) && (bounds.Equals(s.Bounds)) && (workarea.Equals(s.workarea))) {
					return true;
				}
			}
			return false;
		}

		public override int GetHashCode() {
			return base.GetHashCode ();
		}

		public override string ToString() {
			return "Screen[Bounds={" + this.Bounds + "} WorkingArea={" + this.WorkingArea + "} Primary={" + this.Primary + "} DeviceName=" + this.DeviceName;
		}


		#endregion	// Public Instance Methods
	}
}
