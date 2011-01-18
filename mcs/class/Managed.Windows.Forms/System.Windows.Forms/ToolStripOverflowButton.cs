//
// ToolStripOverflowButton.cs
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
using System.Windows.Forms.Design;

namespace System.Windows.Forms
{
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.None)]
	public class ToolStripOverflowButton : ToolStripDropDownButton
	{
		#region Internal Constructor
		internal ToolStripOverflowButton (ToolStrip ts)
		{
			this.InternalOwner = ts;
			this.Parent = ts;
			this.Visible = false;
		}
		#endregion
		
		#region Public Properties
		public override bool HasDropDownItems {
			get { 
				if (this.drop_down == null)
					return false;
					
				return this.DropDown.DisplayedItems.Count > 0; 
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool RightToLeftAutoMirrorImage {
			get { return base.RightToLeftAutoMirrorImage; }
			set { base.RightToLeftAutoMirrorImage = value; }
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin {
			get { return new Padding (0, 1, 0, 2); }
		}
		#endregion

		#region Public Methods
		public override Size GetPreferredSize (Size constrainingSize)
		{
			return new Size (16, this.Parent.Height);
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripOverflowButtonAccessibleObject ();
		}
		
		protected override ToolStripDropDown CreateDefaultDropDown ()
		{
			ToolStripDropDown tsdd = new ToolStripOverflow (this);
			tsdd.DefaultDropDownDirection = ToolStripDropDownDirection.BelowLeft;
			tsdd.OwnerItem = this;
			return tsdd;
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			if (this.Owner != null)
				this.Owner.Renderer.DrawOverflowButtonBackground (new ToolStripItemRenderEventArgs (e.Graphics, this));
		}

		protected internal override void SetBounds (Rectangle bounds)
		{
			base.SetBounds (bounds);
		}
		#endregion

		#region ToolStripOverflowButtonAccessibleObject Class
		private class ToolStripOverflowButtonAccessibleObject : AccessibleObject
		{
		}
		#endregion
	}
}
