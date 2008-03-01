//
// System.Windows.Forms.Design.FormDocumentDesigner
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2008 Ivan N. Zlatev

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
	internal class FormDocumentDesigner : DocumentDesigner
	{
		
		public FormDocumentDesigner ()
		{
		}

		public override void Initialize (IComponent component)
		{
			Form form = component as Form;
			if (form == null)
				throw new NotSupportedException ("FormDocumentDesigner can be initialized only with Forms");

			form.TopLevel = false;
			form.Visible = true;
			base.Initialize (component);
		}

		public override bool CanParent (Control control)
		{
			if (control is Form)
				return false;
			return base.CanParent (control);
		}

		protected override void WndProc (ref Message m)
		{
			// Filter out titlebar clicks
			//
			switch ((Native.Msg) m.Msg) {
			case Native.Msg.WM_NCLBUTTONDBLCLK:
			case Native.Msg.WM_NCLBUTTONDOWN:
			case Native.Msg.WM_NCMBUTTONDBLCLK:
			case Native.Msg.WM_NCMBUTTONDOWN:
			case Native.Msg.WM_NCRBUTTONDBLCLK:
			case Native.Msg.WM_NCRBUTTONDOWN:
				ISelectionService selectionServ = this.GetService (typeof (ISelectionService)) as ISelectionService;
				if (selectionServ != null)
					selectionServ.SetSelectedComponents (new object[] { this.Component });
				break;
			default:
				base.WndProc (ref m);
				break;
			}
		}
	}
}
