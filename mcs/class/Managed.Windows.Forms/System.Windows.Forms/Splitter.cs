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
//	Jackson Harper (jackson@ximian.com)
//


using System;
using System.Drawing;


namespace System.Windows.Forms {

	public class Splitter : Control {

		private int min_extra;
		private int min_size;
		private int move_start_x;
		private int move_start_y;

		private int thickness;
		private bool moving;
		private bool horz;

		private SplitterEventHandler on_splitter_moved;
		private SplitterEventHandler on_splitter_moving;

		private Control adjacent;

		public Splitter ()
		{
			SetStyle (ControlStyles.UserPaint, true);
			SetStyle (ControlStyles.StandardClick, true);
			SetStyle (ControlStyles.StandardDoubleClick, true);
			SetStyle (ControlStyles.AllPaintingInWmPaint, true);

			Dock = DockStyle.Left;
		}

		public override DockStyle Dock {
			get { return base.Dock; }
			set {
				if (value == base.Dock)
					return;

				switch (value) {
				case DockStyle.Bottom:
				case DockStyle.Top:
					horz = true;
					break;
				case DockStyle.Left:
				case DockStyle.Right:
					horz = false;
					break;
				default:
					throw new ArgumentException ("A splitter control must be docked left, right, top, or bottom.");
				}
				base.Dock = value;
			}
		}

		public int MinExtra {
			get { return min_extra; }
			set {
				if (value < 0)
					value = 0;
				min_extra = value;
			}
		}

		public int MinSize {
			get {
				return min_size;
			}
			set {
				if(value < 0)
					value = 0;
				min_size = value;
			}
		}

		public int SplitPosition {
			get {
				Control adjacent = FindAdjacentControl ();
				if (adjacent == null)
					return -1;

				if (horz)
					return adjacent.Width;
				return adjacent.Height;
			}
			set {
				adjacent = FindAdjacentControl ();
				if (adjacent == null)
					return;

				if (horz) {
					if (adjacent.Height == value)
						return;
					OnSplitterMoved (new SplitterEventArgs (Left, Top, Left, value));
					return;
				}
				if (adjacent.Width == value)
					return;
				OnSplitterMoved (new SplitterEventArgs (adjacent.Width / 2, adjacent.Height / 2, value, Top));	      
				adjacent = null;
			}
		}

		
		protected override Size DefaultSize {
			get {
				return new Size (3, 3);
			}
		}

		protected virtual void OnSplitterMoved (SplitterEventArgs e)
		{
			if (on_splitter_moved != null)
				on_splitter_moved (this, e);
			Move (e.SplitX, e.SplitY);
		}

		protected virtual void OnSplitterMoving (SplitterEventArgs e)
		{
			if (on_splitter_moving != null)
				on_splitter_moving (this, e);
			Move (e.SplitX, e.SplitY);
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);

			if (!moving && e.Button == MouseButtons.Left) {
				adjacent = FindAdjacentControl ();

				move_start_x = e.X;
				move_start_y = e.Y;

				moving = true;
				Capture = true;
			}
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{

			base.OnMouseMove (e);
			if (moving) {
				int x_move = e.X - move_start_x;
				int y_move = e.Y - move_start_y;

				move_start_x = e.X;
				move_start_y = e.Y;

				if (horz) {
					Top = Top + y_move;
				} else {
					Left = Left + x_move;	     
				}

				OnSplitterMoving (new SplitterEventArgs (e.X, e.Y, Left, Top));
			}
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseDown (e);
			moving = false;
			Capture = false;
			adjacent = null;
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (horz) {
				if (height <= 0)
					thickness = 3;
				else
					thickness = height;
			} else {
				if (width <= 0)
					thickness = 3;
				else
					thickness = width;
			}

			base.SetBoundsCore (x, y, width, height, specified);
		}

		private void Draw ()
		{
			using (Graphics pdc = Parent.CreateGraphics ()) {
				pdc.FillRectangle (new SolidBrush (Color.Red), ClientRectangle);
			}
		}

		private void Move (int x, int y)
		{
			if (adjacent == null)
				return;

			if (horz) {
				if (adjacent.Height == y)
					return;
				adjacent.Height = y;
				return;
			}

			if (adjacent.Width == x)
				return;
			
			adjacent.Width = x;

			Draw ();
		}

		private Control FindAdjacentControl ()
		{
			if (Parent == null)
				return null;

			foreach (Control sibling in Parent.Controls) {

				if (!sibling.Visible)
					continue;

				switch (Dock) {

				case DockStyle.Left:
					if (sibling.Right == Left)
						return sibling;
					break;

				case DockStyle.Right:
					if (sibling.Left == Right)
						return sibling;
					break;

				case DockStyle.Top:
					if (sibling.Bottom == Top)
						return sibling;
					break;

				case DockStyle.Bottom:
					if (sibling.Top == Bottom)
						return sibling;
					break;
				}
			}

			return null;
		}

                public new event EventHandler BackgroundImageChanged {
                        add { base.BackgroundImageChanged += value; }
                        remove { base.BackgroundImageChanged -= value; }
                }

                public new event EventHandler Enter {
                        add { base.Enter += value; }
                        remove { base.Enter -= value; }
                }

                public new event EventHandler FontChanged {
                        add { base.FontChanged += value; }
                        remove { base.FontChanged -= value; }
                }

                public new event EventHandler ForeColorChanged {
                        add { base.ForeColorChanged += value; }
                        remove { base.ForeColorChanged -= value; }
                }

                public new event EventHandler ImeModeChanged {
                        add { base.ImeModeChanged += value; }
                        remove { base.ImeModeChanged -= value; }
                }

                public new event KeyEventHandler KeyDown {
                        add { base.KeyDown += value; }
                        remove { base.KeyDown -= value; }
                }

                public new event KeyPressEventHandler KeyPress {
                        add { base.KeyPress += value; }
                        remove { base.KeyPress -= value; }
                }

                public new event KeyEventHandler KeyUp {
                        add { base.KeyUp += value; }
                        remove { base.KeyUp -= value; }
                }

                public new event EventHandler Leave {
                        add { base.Leave += value; }
                        remove { base.Leave -= value; }
                }

                public new event EventHandler TabStopChanged {
                        add { base.TabStopChanged += value; }
                        remove { base.TabStopChanged -= value; }
                }

                public new event EventHandler TextChanged {
                        add { base.TextChanged += value; }
                        remove { base.TextChanged -= value; }
                }

                public event SplitterEventHandler SplitterMoved {
                        add { on_splitter_moved += value; }
                        remove { on_splitter_moved -= value; }
                }

                public event SplitterEventHandler SplitterMoving {
                        add { on_splitter_moving += value; }
                        remove { on_splitter_moving -= value; }
                }
	}
}


