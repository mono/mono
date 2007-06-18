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
#if NET_2_0
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
		private ToolStripLayoutStyle layout_style;
		private bool show_check_margin;
		private bool show_image_margin;

		#region Public Constructors
		public ToolStripDropDownMenu () : base ()
		{
			this.layout_style = ToolStripLayoutStyle.Flow;
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
			get { return this.layout_style; }
			set { this.layout_style = value; }
		}
		
		[DefaultValue (false)]
		public bool ShowCheckMargin {
			get { return this.show_check_margin; }
			set { this.show_check_margin = value; }
		}

		[DefaultValue (true)]
		public bool ShowImageMargin {
			get { return this.show_image_margin; }
			set { this.show_image_margin = value; }
		}
		#endregion

		#region Protected Properties
		protected override Padding DefaultPadding {
			get { return base.DefaultPadding; }
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
			base.OnLayout (e);
		}
		
		protected override void OnPaintBackground (PaintEventArgs pevent)
		{
			Rectangle affected_bounds = new Rectangle (Point.Empty, this.Size);

			ToolStripRenderEventArgs e = new ToolStripRenderEventArgs (pevent.Graphics, this, affected_bounds, SystemColors.Control);
			e.InternalConnectedArea = CalculateConnectedArea ();

			this.Renderer.DrawToolStripBackground (e);
			
			if (this.ShowCheckMargin || this.ShowImageMargin)
				this.Renderer.DrawImageMargin (e);
		}

		protected override void SetDisplayedItems ()
		{
			base.SetDisplayedItems ();
		}
		#endregion

		#region Internal Methods
		internal override Rectangle CalculateConnectedArea ()
		{
			if (this.OwnerItem != null && !this.OwnerItem.IsOnDropDown)
				return new Rectangle (1, 0, this.OwnerItem.Width - 2, 2);

			return base.CalculateConnectedArea ();
		}
		#endregion
	}
}
#endif
