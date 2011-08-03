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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>


using System;
using System.Collections.Generic;
using System.Text;

namespace System.Windows.Forms.WebBrowserDialogs
{
	internal class AlertCheck : Generic
	{
		private bool check;
		public bool Checked {
			get { return check; }
		}

		public AlertCheck (string title, string text, string checkMessage, bool checkState)
			: base (title)
		{
			InitTable (3, 1);

			AddLabel (0, 0, 0, text, -1, -1);
			AddCheck (1, 0, 0, checkMessage, checkState, -1, -1, new EventHandler (CheckedChanged));
			AddButton (2, 0, 0, "OK", -1, -1, true, false, new EventHandler (OkClick));
		}

		private void OkClick (object sender, EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close ();
		}

		private void CheckedChanged (object sender, EventArgs e)
		{
			CheckBox c = sender as CheckBox;
			check = c.Checked;
		}
	}
}
