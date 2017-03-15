//
// System.Windows.Forms.Design.ScrollableControlDesigner
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;



namespace System.Windows.Forms.Design
{

	public class ScrollableControlDesigner : ParentControlDesigner
	{

		public ScrollableControlDesigner ()
		{
		}

		private const int HTHSCROLL = 6;
		private const int HTVSCROLL = 7;

		protected override bool GetHitTest (Point pt)
		{
			if (base.GetHitTest (pt)) {
				return true;
			}

			// Check if the user has clicked on the scroll bars and forward the message to
			// the ScrollableControl. (Don't filter out the scrolling.). Keep in mind that scrollbars
			// will be shown only if ScrollableControl.AutoScroll = true
			//
			if (this.Control is ScrollableControl && ((ScrollableControl)Control).AutoScroll) {
				int hitTestResult = (int) Native.SendMessage (this.Control.Handle,
																	 Native.Msg.WM_NCHITTEST,
																	 IntPtr.Zero,
																	(IntPtr) Native.LParam (pt.X, pt.Y));
				if (hitTestResult == HTHSCROLL || hitTestResult == HTVSCROLL)
					return true;
			}
			return false;
		}


		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
			if (m.Msg == (int)Native.Msg.WM_HSCROLL || m.Msg == (int)Native.Msg.WM_VSCROLL)
				this.DefWndProc (ref m);
		}
	}
}
