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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)

// TODO: Eventually we need to handle the cursor and resizing the parent

using System;
using System.Drawing;

namespace System.Windows.Forms {

	internal class SizeGrip : Control {

		private bool redraw = true;
		
		public SizeGrip ()
		{
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			if (redraw)
				Draw ();
			pe.Graphics.DrawImage (ImageBuffer, pe.ClipRectangle, pe.ClipRectangle, GraphicsUnit.Pixel);
		}

		protected override void OnSizeChanged (EventArgs e)
		{
			redraw = true;
		}

		private void Draw ()
		{
			ControlPaint.DrawSizeGrip (DeviceContext, BackColor, ClientRectangle);
			redraw = false;
		}
	}

}


