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

// NOT COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms {

	public class Panel : ScrollableControl {
		private BorderStyle border_style;

		#region	Constructors & Destructors
		public Panel () {
			base.TabStop = false;
		}
		#endregion	// Constructors & Destructors

		#region Public Instance Properties
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

		public override string Text {
			get { return base.Text; }
			set {
				if (value == Text)
					return;
				base.Text = value;
				Refresh ();
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get { return new Size (200, 100); }
		}
		#endregion	// Proteced Instance Properties

		#region Public Instance Methods
		public override string ToString ()
		{
			return base.ToString () + ", BorderStyle: " + BorderStyle;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void OnResize(EventArgs e) {
			base.OnResize (e);
		}

		#endregion	// Protected Instance Methods

		#region Events
		public event KeyEventHandler		KeyDown;
		public event KeyPressEventHandler	KeyPress;
		public event KeyEventHandler		KeyUp;
		#endregion
	}
}

