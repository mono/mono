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
// $Log: UserControl.cs,v $
// Revision 1.1  2004/08/22 18:00:49  pbartok
// - Implemented UserControl (complete)
//
//
//

// COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms {
	public class UserControl : ContainerControl {
		#region Public Constructors
		public UserControl() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		protected override Size DefaultSize {
			get {
				return new Size(150, 150);
			}
		}


		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override void OnCreateControl() {
			base.OnCreateControl();

			// The OnCreateControl isn't neccessarily raised *before* it
			// becomes first visible, but that's the best we've got
			OnLoad(EventArgs.Empty);
		}

		protected virtual void OnLoad(EventArgs e) {
			if (Load != null) Load(this, e);
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown(e);
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler	Load;
		#endregion	// Events
	}
}
