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

#if NET_2_0
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
		private LayoutEngine layout_engine;
		private Dictionary<object, bool> flow_breaks;

		internal FlowLayoutSettings ()
		{
			flow_breaks = new Dictionary<object, bool> ();
			wrap_contents = true;
			flow_direction = FlowDirection.LeftToRight;
		}

		#region Public Properties
		[DefaultValue (FlowDirection.LeftToRight)]
		public FlowDirection FlowDirection {
			get { return this.flow_direction; }
			set { this.flow_direction = value; }
		}

		public override LayoutEngine LayoutEngine {
			get { 
				if (this.layout_engine == null)
					this.layout_engine = new FlowLayout ();
					
				return this.layout_engine;
			}
		}

		[DefaultValue (true)]
		public bool WrapContents {
			get { return this.wrap_contents; }
			set { this.wrap_contents = value; }
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
		}
		#endregion
	}
}
#endif
