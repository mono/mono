//
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
// $Log: Button.cs,v $
// Revision 1.1  2004/09/01 20:39:41  pbartok
// - Functional initial check-in
//
//

// COMPLETE

using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
	public class Button : ButtonBase, IButtonControl {
		#region Local variables
		DialogResult	dialog_result;
		#endregion	// Local variables

		#region Public Constructors
		public Button() {
			dialog_result = DialogResult.None;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public DialogResult DialogResult {		// IButtonControl
			get {
				return dialog_result;
			}
			set {
				dialog_result = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				SetStyle(ControlStyles.UserPaint, true);

				return base.CreateParams;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public virtual void NotifyDefault(bool value) {	// IButtonControl
			this.IsDefault = value;
		}

		public void PerformClick() {			// IButtonControl
			OnClick(EventArgs.Empty);
		}

		public override string ToString() {
			return base.ToString() + ", Text: " + this.text;
		}
		#endregion	// Public Instance Methods

		#region	Protected Instance Methods
		protected override void OnClick(EventArgs e) {
			if (dialog_result != DialogResult.None) {
				Form p = Parent as Form;

				if (p != null) {
					p.DialogResult = dialog_result;
				}
			}
			base.OnClick(e);
			Redraw();
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
		}

		protected override bool ProcessMnemonic(char charCode) {
			return base.ProcessMnemonic (charCode);
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}
		#endregion	// Protected Instance Methods

	}
}
