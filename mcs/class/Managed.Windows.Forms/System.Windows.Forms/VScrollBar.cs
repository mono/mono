//
// System.Windows.Forms.HScrollBar.cs
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
// Copyright (C) 2004-2005, Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez	jordi@ximian.com
//


using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms 
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class VScrollBar : ScrollBar 
	{		
		#region events
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public new event EventHandler RightToLeftChanged {
			add { base.RightToLeftChanged += value; }
			remove { base.RightToLeftChanged -= value; }
		}
		
		#endregion Events

		public VScrollBar()
		{			
			vert = true;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set {
				if (RightToLeft == value)
					return;

				base.RightToLeft = value;

				OnRightToLeftChanged (EventArgs.Empty);
			}
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.VScrollBarDefaultSize; }
		}	

		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}		
	}
}
