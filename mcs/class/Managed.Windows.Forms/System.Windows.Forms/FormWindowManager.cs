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
//	Rolf Bjarne Kvinge  (RKvinge@novell.com)
//
//


using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	internal class FormWindowManager : InternalWindowManager
	{
		private bool pending_activation;
		public FormWindowManager (Form form)  : base (form)
		{

			form.MouseCaptureChanged += new EventHandler (HandleCaptureChanged);
		}

		void HandleCaptureChanged (object sender, EventArgs e)
		{
			if (pending_activation && !form.Capture) {
				form.BringToFront ();
				pending_activation = false;
			}
		}

		public override void PointToClient (ref int x, ref int y)
		{
			XplatUI.ScreenToClient (Form.Parent.Handle, ref x, ref y);
		}


		protected override bool HandleNCLButtonDown (ref Message m)
		{
			// MS seems to be doing this on mouse up, but we don't get WM_NCLBUTTONUP when anything is captured
			// so work around this using MouseCaptureChanged.
			pending_activation = true;
			
			return base.HandleNCLButtonDown (ref m);
		}

		protected override void HandleTitleBarDoubleClick (int x, int y)
		{
			if (IconRectangleContains (x, y)) {
				form.Close ();
			} else if (form.WindowState == FormWindowState.Maximized) {
				form.WindowState = FormWindowState.Normal;
			} else {
				form.WindowState = FormWindowState.Maximized;
			}
			base.HandleTitleBarDoubleClick (x, y);
		}

		internal override Rectangle MaximizedBounds {
			get {
				Rectangle result = base.MaximizedBounds;
				int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
				result.Inflate (bw, bw);
				return result;
			}
		}
	}
}
