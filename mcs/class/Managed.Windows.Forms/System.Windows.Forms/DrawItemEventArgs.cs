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


using System;
using System.Drawing;

namespace System.Windows.Forms {

	public class DrawItemEventArgs : EventArgs {

		Graphics graphics;
		Font font;
		Rectangle rect;
		int index;
		DrawItemState state;
		Color fore_color;
		Color back_color;

		public DrawItemEventArgs (Graphics graphics, Font font,
				Rectangle rect, int index, DrawItemState state) :
			this (graphics, font, rect, index, state,
					Control.DefaultForeColor, Control.DefaultBackColor)
		{

		}

		public DrawItemEventArgs (Graphics graphics, Font font,
				Rectangle rect, int index, DrawItemState state,
				Color fore_color, Color back_color)
		{
			this.graphics = graphics;
			this.font = font;
			this.rect = rect;
			this.index = index;
			this.state = state;
			this.fore_color = fore_color;
			this.back_color = back_color;
		}

		public Graphics Graphics {
			get { return graphics; }
		}

		public Font Font {
			get { return font; }
		}

		public Rectangle Bounds {
			get { return rect; }
		}

		public int Index {
			get { return index; }
		}

		public DrawItemState State {
			get { return state; }
		}

		public Color BackColor {
			get { return back_color; }
		}

		public Color ForeColor {
			get { return fore_color; }
		}

		public virtual void DrawBackground ()
		{
			ThemeEngine.Current.DrawOwnerDrawBackground (this);
		}

		public virtual void DrawFocusRectangle ()
		{
			ThemeEngine.Current.DrawOwnerDrawFocusRectangle (this);
		}
	}
}

