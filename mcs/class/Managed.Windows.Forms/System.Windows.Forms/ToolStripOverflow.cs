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

#if NET_2_0
using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	public class ToolStripOverflow : ToolStripDropDown, IComponent, IDisposable
	{
		private LayoutEngine layout_engine;
		
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
		
		public override LayoutEngine LayoutEngine {
			get {
				if (this.layout_engine == null)
					this.layout_engine = new FlowLayout ();
					
				return base.LayoutEngine;
			}
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
			return base.GetToolStripPreferredSize (constrainingSize);
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripOverflowAccessibleObject ();
		}
		
		[MonoInternalNote ("This should stack in rows of ~3, but for now 1 column will work.")]
		protected override void OnLayout (LayoutEventArgs e)
		{
			SetDisplayedItems ();
			
			// Find the widest menu item
			int widest = 0;

			foreach (ToolStripItem tsi in this.DisplayedItems) {
				if (!tsi.Available)
					continue;
				if (tsi.GetPreferredSize (Size.Empty).Width > widest)
					widest = tsi.GetPreferredSize (Size.Empty).Width;
			}

			int x = this.Padding.Left;
			widest += this.Padding.Horizontal;
			int y = this.Padding.Top;

			foreach (ToolStripItem tsi in this.DisplayedItems) {
				if (!tsi.Available)
					continue;

				y += tsi.Margin.Top;

				int height = 0;

				if (tsi is ToolStripSeparator)
					height = 7;
				else
					height = tsi.GetPreferredSize (Size.Empty).Height;

				tsi.SetBounds (new Rectangle (x, y, widest, height));
				y += tsi.Height + tsi.Margin.Bottom;
			}

			this.Size = new Size (widest + this.Padding.Horizontal, y + this.Padding.Bottom);// + 2);
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
		#endregion

		#region ToolStripOverflowAccessibleObject Class
		private class ToolStripOverflowAccessibleObject : AccessibleObject
		{
		}
		#endregion
	}
}
#endif
