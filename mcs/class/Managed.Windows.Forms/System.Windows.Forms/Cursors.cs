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
//	Peter Bartok	pbartok@novell.com
//
//

// COMPLETE

namespace System.Windows.Forms {
	public sealed class Cursors {
		#region Local Variables
		internal static Cursor	app_starting;
		internal static Cursor	arrow;
		internal static Cursor	cross;
		internal static Cursor	def;
		internal static Cursor	hand;
		internal static Cursor	help;
		internal static Cursor	hsplit;
		internal static Cursor	ibeam;
		internal static Cursor	no;
		internal static Cursor	no_move_2d;
		internal static Cursor	no_move_horiz;
		internal static Cursor	no_move_vert;
		internal static Cursor	pan_east;
		internal static Cursor	pan_ne;
		internal static Cursor	pan_north;
		internal static Cursor	pan_nw;
		internal static Cursor	pan_se;
		internal static Cursor	pan_south;
		internal static Cursor	pan_sw;
		internal static Cursor	pan_west;
		internal static Cursor	size_all;
		internal static Cursor	size_nesw;
		internal static Cursor	size_ns;
		internal static Cursor	size_nwse;
		internal static Cursor	size_we;
		internal static Cursor	up_arrow;
		internal static Cursor	vsplit;
		internal static Cursor	wait_cursor;
		#endregion	// Local Variables

		#region Constructors
		private Cursors() {
		}
		#endregion	// Constructors

		#region Public Static Properties
		public static Cursor AppStarting {
			get {
				if (app_starting == null) {
					app_starting = new Cursor(StdCursor.AppStarting);
					app_starting.name = "AppStarting";
				}
				return app_starting;
			}
		}

		public static Cursor Arrow {
			get {
				if (arrow == null) {
					arrow = new Cursor(StdCursor.Arrow);
					arrow.name = "Arrow";
				}
				return arrow;
			}
		}

		public static Cursor Cross {
			get {
				if (cross == null) {
					cross = new Cursor(StdCursor.Cross);
					cross.name = "Cross";
				}
				return cross;
			}
		}

		public static Cursor Default {
			get {
				if (def == null) {
					def = new Cursor(StdCursor.Default);
					def.name = "Default";
				}
				return def;
			}
		}

		public static Cursor Hand {
			get {
				if (hand == null) {
					hand = new Cursor(StdCursor.Hand);
					hand.name = "Hand";
				}
				return hand;
			}
		}

		public static Cursor Help {
			get {
				if (help == null) {
					help = new Cursor(StdCursor.Help);
					help.name = "Help";
				}
				return help;
			}
		}

		public static Cursor HSplit {
			get {
				if (hsplit == null) {
					hsplit = new Cursor(typeof(Splitter), "SplitterNS.cur");
					hsplit.name = "HSplit";
				}
				return hsplit;
			}
		}

		public static Cursor IBeam {
			get {
				if (ibeam == null) {
					ibeam = new Cursor(StdCursor.IBeam);
					ibeam.name = "IBeam";
				}
				return ibeam;
			}
		}

		public static Cursor No {
			get {
				if (no == null) {
					no = new Cursor(StdCursor.No);
					no.name = "No";
				}
				return no;
			}
		}

		public static Cursor NoMove2D {
			get {
				if (no_move_2d == null) {
					no_move_2d = new Cursor(StdCursor.NoMove2D);
					no_move_2d.name = "NoMove2D";
				}
				return no_move_2d;
			}
		}

		public static Cursor NoMoveHoriz {
			get {
				if (no_move_horiz == null) {
					no_move_horiz = new Cursor(StdCursor.NoMoveHoriz);
					no_move_horiz.name = "NoMoveHoriz";
				}
				return no_move_horiz;
			}
		}

		public static Cursor NoMoveVert {
			get {
				if (no_move_vert == null) {
					no_move_vert = new Cursor(StdCursor.NoMoveVert);
					no_move_vert.name = "NoMoveVert";
				}
				return no_move_vert;
			}
		}

		public static Cursor PanEast {
			get {
				if (pan_east == null) {
					pan_east = new Cursor(StdCursor.PanEast);
					pan_east.name = "PanEast";
				}
				return pan_east;
			}
		}




		public static Cursor PanNE {
			get {
				if (pan_ne == null) {
					pan_ne = new Cursor(StdCursor.PanNE);
					pan_ne.name = "PanNE";
				}
				return pan_ne;
			}
		}


		public static Cursor PanNorth {
			get {
				if (pan_north == null) {
					pan_north = new Cursor(StdCursor.PanNorth);
					pan_north.name = "PanNorth";
				}
				return pan_north;
			}
		}

		public static Cursor PanNW {
			get {
				if (pan_nw == null) {
					pan_nw = new Cursor(StdCursor.PanNW);
					pan_nw.name = "PanNW";
				}
				return pan_nw;
			}
		}

		public static Cursor PanSE {
			get {
				if (pan_se == null) {
					pan_se = new Cursor(StdCursor.PanSE);
					pan_se.name = "PanSE";
				}
				return pan_se;
			}
		}

		public static Cursor PanSouth {
			get {
				if (pan_south == null) {
					pan_south = new Cursor(StdCursor.PanSouth);
					pan_south.name = "PanSouth";
				}
				return pan_south;
			}
		}

		public static Cursor PanSW {
			get {
				if (pan_sw == null) {
					pan_sw = new Cursor(StdCursor.PanSW);
					pan_sw.name = "PanSW";
				}
				return pan_sw;
			}
		}

		public static Cursor PanWest {
			get {
				if (pan_west == null) {
					pan_west = new Cursor(StdCursor.PanWest);
					pan_west.name = "PanWest";
				}
				return pan_west;
			}
		}

		public static Cursor SizeAll {
			get {
				if (size_all == null) {
					size_all = new Cursor(StdCursor.SizeAll);
					size_all.name = "SizeAll";
				}
				return size_all;
			}
		}

		public static Cursor SizeNESW {
			get {
				if (size_nesw == null) {
					if (XplatUI.RunningOnUnix) {
						size_nesw = new Cursor(typeof(Cursor), "NESW.cur");
						size_nesw.name = "SizeNESW";
					} else {
						size_nesw = new Cursor(StdCursor.SizeNESW);
						size_nesw.name = "SizeNESW";
					}
				}
				return size_nesw;
			}
		}

		public static Cursor SizeNS {
			get {
				if (size_ns == null) {
					size_ns = new Cursor(StdCursor.SizeNS);
					size_ns.name = "SizeNS";
				}
				return size_ns;
			}
		}

		public static Cursor SizeNWSE {
			get {
				if (size_nwse == null) {
					if (XplatUI.RunningOnUnix) {
						size_nwse = new Cursor(typeof(Cursor), "NWSE.cur");
						size_nwse.name = "SizeNWSE";
					} else {
						size_nwse = new Cursor(StdCursor.SizeNWSE);
						size_nwse.name = "SizeNWSE";
					}
				}
				return size_nwse;
			}
		}

		public static Cursor SizeWE {
			get {
				if (size_we == null) {
					size_we = new Cursor(StdCursor.SizeWE);
					size_we.name = "SizeWE";
				}
				return size_we;
			}
		}

		public static Cursor UpArrow {
			get {
				if (up_arrow == null) {
					up_arrow = new Cursor(StdCursor.UpArrow);
					up_arrow.name = "UpArrow";
				}
				return up_arrow;
			}
		}

		public static Cursor VSplit {
			get {
				if (vsplit == null) {
					vsplit = new Cursor(typeof(Cursor), "SplitterWE.cur");
					vsplit.name = "VSplit";
				}
				return vsplit;
			}
		}

		public static Cursor WaitCursor {
			get {
				if (wait_cursor == null) {
					wait_cursor = new Cursor(StdCursor.WaitCursor);
					wait_cursor.name = "WaitCursor";
				}
				return wait_cursor;
			}
		}
		#endregion	// Public Static Properties
	}
}
