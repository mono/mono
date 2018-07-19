//
// FlowLayoutSettings.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System.Windows.Forms.Layout;
using System.ComponentModel;
using System.Collections.Generic;

namespace System.Windows.Forms
{
	[DefaultProperty ("FlowDirection")]
	public class FlowLayoutSettings : LayoutSettings
	{
		private FlowDirection flow_direction;
		private bool wrap_contents;
		private Dictionary<object, bool> flow_breaks;
		private Control owner;

		internal FlowLayoutSettings () : this (null)
		{
		}

		internal FlowLayoutSettings (Control owner)
		{
			flow_breaks = new Dictionary<object, bool> ();
			wrap_contents = true;
			flow_direction = FlowDirection.LeftToRight;
			this.owner = owner;
		}

		#region Public Properties
		[DefaultValue (FlowDirection.LeftToRight)]
		public FlowDirection FlowDirection {
			get { return this.flow_direction; }
			set { 
				if (this.flow_direction != value) {
					this.flow_direction = value;
					if (owner != null)
						owner.PerformLayout (owner, "FlowDirection");
				}
			}
		}

		public override LayoutEngine LayoutEngine {
			get { 
				return System.Windows.Forms.Layout.DefaultLayout.Instance;
			}
		}

		[DefaultValue (true)]
		public bool WrapContents {
			get { return this.wrap_contents; }
			set { 
				if (this.wrap_contents != value) {
					this.wrap_contents = value;
					if (owner != null)
						owner.PerformLayout (owner, "WrapContents");
				}
			}
		}
		#endregion

		#region Public Methods
		public bool GetFlowBreak (Object child)
		{
			bool retval;

			if (flow_breaks.TryGetValue (child, out retval))
				return retval;

			return false;
		}

		public void SetFlowBreak (Object child, bool value)
		{
			flow_breaks[child] = value;
			if (owner != null)
				owner.PerformLayout ((Control)child, "FlowBreak");
		}
		#endregion
	}
}
