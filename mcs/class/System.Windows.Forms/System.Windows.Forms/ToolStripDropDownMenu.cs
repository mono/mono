//
// ToolStripDropDownMenu.cs
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

using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Designer ("System.Windows.Forms.Design.ToolStripDropDownDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ToolStripDropDownMenu : ToolStripDropDown
	{
		private bool show_check_margin;
		private bool show_image_margin;

		#region Public Constructors
		public ToolStripDropDownMenu () : base ()
		{
			base.LayoutStyle = ToolStripLayoutStyle.Flow;
			this.show_image_margin = true;
		}
		#endregion

		#region Public Properties
		public override Rectangle DisplayRectangle {
			get { return base.DisplayRectangle; }
		}

		public override LayoutEngine LayoutEngine {
			get { return base.LayoutEngine; }
		}
		
		[DefaultValue (ToolStripLayoutStyle.Flow)]
		public new ToolStripLayoutStyle LayoutStyle {
			get { return base.LayoutStyle; }
			set { base.LayoutStyle = value; }
		}
		
		[DefaultValue (false)]
		public bool ShowCheckMargin {
			get { return this.show_check_margin; }
			set { 
				if (this.show_check_margin != value) {
					this.show_check_margin = value;
					PerformLayout (this, "ShowCheckMargin");
				}
			}
		}

		[DefaultValue (true)]
		public bool ShowImageMargin {
			get { return this.show_image_margin; }
			set { 
				if (this.show_image_margin != value) {
					this.show_image_margin = value;
					PerformLayout (this, "ShowImageMargin");
				}
			}
		}
		#endregion

		#region Protected Properties
		protected override Padding DefaultPadding {
			get { return base.DefaultPadding; }
		}

		protected internal override Size MaxItemSize {
			get { return Size; }
		}

		#endregion

		#region Protected Methods
		protected internal override ToolStripItem CreateDefaultItem (string text, Image image, EventHandler onClick)
		{
			return base.CreateDefaultItem (text, image, onClick);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			// Find the widest menu item
			int widest = 0;

			foreach (ToolStripItem tsi in this.Items) {
				if (!tsi.Available)
					continue;

				tsi.SetPlacement (ToolStripItemPlacement.Main);

				widest = Math.Max (widest, tsi.GetPreferredSize (Size.Empty).Width);
			}

			int x = this.Padding.Left;
			
			if (show_check_margin || show_image_margin)
				widest += 68 - this.Padding.Horizontal;
			else
				widest += 47 - this.Padding.Horizontal;
			
			int y = this.Padding.Top;

			foreach (ToolStripItem tsi in this.Items) {
				if (!tsi.Available)
					continue;

				y += tsi.Margin.Top;

				int height = 0;
	
				Size preferred_size = tsi.GetPreferredSize (Size.Empty);

				if (preferred_size.Height > 22)
					height = preferred_size.Height;
				else if (tsi is ToolStripSeparator)
					height = 7;
				else
					height = 22;

				tsi.SetBounds (new Rectangle (x, y, widest, height));
				y += height + tsi.Margin.Bottom;
			}

			this.Size = new Size (widest + this.Padding.Horizontal, y + this.Padding.Bottom);// + 2);
			this.SetDisplayedItems ();
			this.OnLayoutCompleted (EventArgs.Empty);
			this.Invalidate ();
		}
		
		protected override void OnPaintBackground (PaintEventArgs e)
		{
			Rectangle affected_bounds = new Rectangle (Point.Empty, this.Size);

			ToolStripRenderEventArgs tsrea = new ToolStripRenderEventArgs (e.Graphics, this, affected_bounds, SystemColors.Control);
			tsrea.InternalConnectedArea = CalculateConnectedArea ();

			this.Renderer.DrawToolStripBackground (tsrea);
			
			if (this.ShowCheckMargin || this.ShowImageMargin) {
				tsrea = new ToolStripRenderEventArgs (e.Graphics, this, new Rectangle (tsrea.AffectedBounds.Location, new Size (25, tsrea.AffectedBounds.Height)), SystemColors.Control);
				this.Renderer.DrawImageMargin (tsrea);
			}
		}

		protected override void SetDisplayedItems ()
		{
			base.SetDisplayedItems ();
		}
		#endregion

		#region Internal Methods
		internal override Rectangle CalculateConnectedArea ()
		{
			if (this.OwnerItem != null && !this.OwnerItem.IsOnDropDown && !(this.OwnerItem is MdiControlStrip.SystemMenuItem)) {
				Point owner_screen_loc = OwnerItem.GetCurrentParent ().PointToScreen (OwnerItem.Location);
				return new Rectangle (owner_screen_loc.X - Left, 0, this.OwnerItem.Width - 1, 2);
			}

			return base.CalculateConnectedArea ();
		}
		#endregion
	}
}
