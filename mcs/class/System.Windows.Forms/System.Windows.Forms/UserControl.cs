//
// System.Windows.Forms.UpDownEventHandler.cs
//
// Authors:
//	//unknown
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

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
using System;
using System.Drawing;

namespace System.Windows.Forms {

	public class UserControl : ContainerControl{
		// --- Events ---
		public event EventHandler Load;

		// --- Properties ---
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

		// --- Constructor ---
		public UserControl()
		{
			// Nothing to do at this time
		}

		// --- Methods ---
		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			OnLoad(EventArgs.Empty);
		}

		protected virtual void OnLoad(EventArgs e)
		{
			if (Load!=null) {
				Load(this, e);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
		}

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);
		}
	}
}
