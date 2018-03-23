//
// ToolStripOverflow.cs
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
// Copyright (c) 2007 Novell
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	public class ToolStripOverflow : ToolStripDropDown, IComponent, IDisposable, IArrangedContainer
	{
		#region Public Constructors
		public ToolStripOverflow (ToolStripItem parentItem)
		{
			this.OwnerItem = parentItem;
		}
		#endregion
		
		#region Public Properties
		// XXX - This probably adds ToolStripOverflowButton to the returned collection
		public override ToolStripItemCollection Items {
			get { return base.Items; }
		}
		#endregion

		#region Protected Properties
		protected internal override ToolStripItemCollection DisplayedItems {
			get { return base.DisplayedItems; }
		}
		#endregion

		#region Public Methods
		public override Size GetPreferredSize (Size constrainingSize)
		{
			constrainingSize.Width = 200;
			return base.GetPreferredSize (constrainingSize);
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripOverflowAccessibleObject ();
		}

		protected override LayoutSettings CreateLayoutSettings(ToolStripLayoutStyle style) {
			LayoutSettings layout_settings = base.CreateLayoutSettings (style);
			if (style == ToolStripLayoutStyle.Flow) {
				((FlowLayoutSettings)layout_settings).FlowDirection = FlowDirection.LeftToRight;
				((FlowLayoutSettings)layout_settings).WrapContents = true;
			}
			return layout_settings;
		}
		
		protected override void SetDisplayedItems ()
		{
			this.displayed_items.ClearInternal ();

			if (this.OwnerItem != null && this.OwnerItem.Parent != null)
				foreach (ToolStripItem tsi in this.OwnerItem.Parent.Items)
					if (tsi.Placement == ToolStripItemPlacement.Overflow && tsi.Available && !(tsi is ToolStripSeparator)) {
						this.displayed_items.AddNoOwnerOrLayout (tsi);
						//tsi.Parent = this;
					}

			this.PerformLayout ();
		}
		#endregion

		#region Internal Methods
		internal ToolStrip ParentToolStrip {
			get { return (ToolStrip)this.OwnerItem.Parent; }
		}

		ArrangedElementCollection IArrangedContainer.Controls {
			get { return DisplayedItems; }
		}

		#endregion

		#region ToolStripOverflowAccessibleObject Class
		private class ToolStripOverflowAccessibleObject : AccessibleObject
		{
		}
		#endregion
	}
}
