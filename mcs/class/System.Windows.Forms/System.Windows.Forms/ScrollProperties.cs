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
// Authors:
//	Olivier Dufour  olivier.duff@free.fr
//	Jonathan Pobst  monkey@jpobst.com
//

using System.ComponentModel;

namespace System.Windows.Forms
{
	public abstract class ScrollProperties
	{
		#region Private Fields
		private ScrollableControl parentControl;
		internal ScrollBar scroll_bar;
		#endregion

		#region constructor
		protected ScrollProperties (ScrollableControl container)
		{
			parentControl = container;
		}
		#endregion

		#region Public Properties
		[DefaultValue (true)]
		public bool Enabled {
			get { return scroll_bar.Enabled; }
			set { scroll_bar.Enabled = value; }
		}
		
		[DefaultValue (10)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int LargeChange {
			get { return scroll_bar.LargeChange; }
			set { scroll_bar.LargeChange = value; }
		}

		[DefaultValue (100)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int Maximum {
			get { return scroll_bar.Maximum; }
			set { scroll_bar.Maximum = value; }
		}

		[DefaultValue (0)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int Minimum {
			get { return scroll_bar.Minimum; }
			set { scroll_bar.Minimum = value; }
		}

		[DefaultValue (1)]
		public int SmallChange {
			get { return scroll_bar.SmallChange; }
			set { scroll_bar.SmallChange = value; }
		}

		[DefaultValue (0)]
		[BindableAttribute (true)]
		public int Value {
			get { return scroll_bar.Value; }
			set { scroll_bar.Value = value; }
		}

		[DefaultValue (false)]
		public bool Visible {
			get { return scroll_bar.Visible; }
			set { scroll_bar.Visible = value; }
		}
		#endregion

		#region Protected Properties
		protected ScrollableControl ParentControl {
			get { return parentControl; }
		}
		#endregion
	}
}
