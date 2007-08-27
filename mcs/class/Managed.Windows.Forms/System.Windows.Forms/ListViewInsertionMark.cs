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

#if NET_2_0

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
		int index = -1;

		internal ListViewInsertionMark (ListView listview)
		{
			listview_owner = listview;
		}

		public bool AppearsAfterItem {
			get {
				return appears_after_item;
			}
			set {
				appears_after_item = value;
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
				index = value;
			}
		}

		public int NearestIndex (Point pt)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif

