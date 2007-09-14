//
// System.Windows.Forms.Design.SplitContainerDesigner
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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

#if NET_2_0

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;



namespace System.Windows.Forms.Design
{

	internal class SplitContainerDesigner : ParentControlDesigner
	{

		public SplitContainerDesigner ()
		{
		}

		public override void Initialize (IComponent component)
		{
			base.Initialize (component);
			SplitContainer container = (SplitContainer) component;
			base.EnableDesignMode (container.Panel1, "Panel1");
			base.EnableDesignMode (container.Panel2, "Panel2");
		}

		public override ControlDesigner InternalControlDesigner (int internalControlIndex)
		{
			switch (internalControlIndex) {
				case 0:
					return GetDesigner (((SplitContainer)this.Control).Panel1);
				case 1:
					return GetDesigner (((SplitContainer)this.Control).Panel2);
			}
			return null;
		}

		private ControlDesigner GetDesigner (IComponent component)
		{
			IDesignerHost host = this.GetService (typeof (IDesignerHost)) as IDesignerHost;
			if (host != null)
				return host.GetDesigner (component) as ControlDesigner;
			else
				return null;
		}

		public override int NumberOfInternalControlDesigners ()
		{
			return 2;
		}
	}
}
#endif
