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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
// 	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//


using System;
using System.Drawing;

namespace System.Windows.Forms
{
	public sealed class ListViewInsertionMark 
	{
		ListView listview_owner;
		bool appears_after_item;
		Rectangle bounds;
		Color? color;
		int index = 0;

		internal ListViewInsertionMark (ListView listview)
		{
			listview_owner = listview;
		}

		public bool AppearsAfterItem {
			get {
				return appears_after_item;
			}
			set {
				if (value == appears_after_item)
					return;

				appears_after_item = value;

				listview_owner.item_control.Invalidate (bounds);
				UpdateBounds ();
				listview_owner.item_control.Invalidate (bounds);
			}
		}

		public Rectangle Bounds {
			get {
				return bounds;
			}
		}

		public Color Color {
			get {
				return color == null ? listview_owner.ForeColor : color.Value;
			}
			set {
				color = value;
			}
		}

		public int Index {
			get {
				return index;
			}
			set {
				if (value == index)
					return;

				index = value;

				listview_owner.item_control.Invalidate (bounds);
				UpdateBounds ();
				listview_owner.item_control.Invalidate (bounds);
			}
		}

		void UpdateBounds ()
		{
			if (index < 0 || index >= listview_owner.Items.Count) {
				bounds = Rectangle.Empty;
				return;
			}

			Rectangle item_bounds = listview_owner.Items [index].Bounds;
			int x_origin = (appears_after_item ? item_bounds.Right : item_bounds.Left) - 2;
			int height = item_bounds.Height + ThemeEngine.Current.ListViewVerticalSpacing;

			bounds = new Rectangle (x_origin, item_bounds.Top, 7, height);
		}

		public int NearestIndex (Point pt)
		{
			double distance = Double.MaxValue;
			int nearest = -1;

			for (int i = 0; i < listview_owner.Items.Count; i++) {
				Point pos = listview_owner.GetItemLocation (i);
				double d = Math.Pow (pos.X - pt.X, 2) + Math.Pow (pos.Y - pt.Y, 2);
				if (d < distance) {
					distance = d;
					nearest = i;
				}
			}

			if (listview_owner.item_control.dragged_item_index == nearest)
				return -1;

			return nearest;
		}

		internal PointF [] TopTriangle {
			get {
				PointF p1 = new PointF (bounds.X, bounds.Y);
				PointF p2 = new PointF (bounds.Right, bounds.Y);
				PointF p3 = new PointF (bounds.X + (bounds.Right - bounds.X) / 2, bounds.Y + 5);

				return new PointF [] {p1, p2, p3};
			}
		}

		internal PointF [] BottomTriangle {
			get {
				PointF p1 = new PointF (bounds.X, bounds.Bottom);
				PointF p2 = new PointF (bounds.Right, bounds.Bottom);
				PointF p3 = new PointF (bounds.X + (bounds.Right - bounds.X) / 2, bounds.Bottom - 5);

				return new PointF [] {p1, p2, p3};
			}
		}

		internal Rectangle Line {
			get {
				return new Rectangle (bounds.X + 2, bounds.Y + 2, 2, bounds.Height - 5);
			}
		}
	}
}
