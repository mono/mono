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

	public class Panel : ScrollableControl {

		private BorderStyle border_style;

		public Panel ()
		{
			base.TabStop = false;
		}

		public override string Text {
			get { return base.Text; }
			set {
				if (value == Text)
					return;
				base.Text = value;
				Refresh ();
			}
		}

		public BorderStyle BorderStyle {
			get { return border_style; }
			set {
				if (value == border_style)
					return;
				border_style = value;
				Refresh ();
			}
		}

		public new bool TabStop {
			get { return base.TabStop; }
			set {
				if (value == TabStop)
					return;
				base.TabStop = value;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (200, 100); }
		}

		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
				createParams.ClassName = String.Empty;
				createParams.Style = (int) (WindowStyles.WS_CHILD |
					WindowStyles.WS_VISIBLE);
				return createParams;
			}
		}

		public override string ToString ()
		{
			return base.ToString () + ", BorderStyle: " + BorderStyle;
		}
		
	}
}

