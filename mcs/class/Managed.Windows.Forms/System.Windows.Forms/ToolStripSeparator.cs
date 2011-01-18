//
// ToolStripSeparator.cs
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
using System.Windows.Forms.Design;

namespace System.Windows.Forms
{
	[ToolStripItemDesignerAvailability (ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.ContextMenuStrip)]
	public class ToolStripSeparator : ToolStripItem
	{
		public ToolStripSeparator () : base ()
		{
		}

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool AutoToolTip {
			get { return base.AutoToolTip; }
			set { base.AutoToolTip = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}
		
		public override bool CanSelect { get { return false; } }

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new ToolStripItemDisplayStyle DisplayStyle {
			get { return base.DisplayStyle; }
			set { base.DisplayStyle = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool DoubleClickEnabled {
			get { return base.DoubleClickEnabled; }
			set { base.DoubleClickEnabled = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Image Image {
			get { return base.Image; }
			set { base.Image = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new ContentAlignment ImageAlign {
			get { return base.ImageAlign; }
			set { base.ImageAlign = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public new int ImageIndex {
			get { return base.ImageIndex; }
			set { base.ImageIndex = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new string ImageKey {
			get { return base.ImageKey; }
			set { base.ImageKey = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new ToolStripItemImageScaling ImageScaling {
			get { return base.ImageScaling; }
			set { base.ImageScaling = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Color ImageTransparentColor {
			get { return base.ImageTransparentColor; }
			set { base.ImageTransparentColor = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool RightToLeftAutoMirrorImage {
			get { return base.RightToLeftAutoMirrorImage; }
			set { base.RightToLeftAutoMirrorImage = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new ContentAlignment TextAlign {
			get { return base.TextAlign; }
			set { base.TextAlign = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (ToolStripTextDirection.Horizontal)]
		public override ToolStripTextDirection TextDirection {
			get { return base.TextDirection; }
			set { base.TextDirection = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new TextImageRelation TextImageRelation {
			get { return base.TextImageRelation; }
			set { base.TextImageRelation = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new string ToolTipText {
			get { return base.ToolTipText; }
			set { base.ToolTipText = value; }
		}
		#endregion

		#region Protected Properties
		protected internal override Padding DefaultMargin { get { return new Padding(); } }
		protected override Size DefaultSize { get { return new Size(6, 6); } }
		#endregion

		#region Public Methods
		public override Size GetPreferredSize (Size constrainingSize)
		{
			return new Size(6, 6);
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			ToolStripItemAccessibleObject ao = new ToolStripItemAccessibleObject (this);

			ao.default_action = "Press";
			ao.role = AccessibleRole.Separator;
			ao.state = AccessibleStates.None;

			return ao;
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
			
			if (this.Owner != null)
			{
				if (this.IsOnDropDown)
					this.Owner.Renderer.DrawSeparator (new ToolStripSeparatorRenderEventArgs (e.Graphics, this, this.Owner.Orientation == Orientation.Horizontal ? false : true));
				else
					this.Owner.Renderer.DrawSeparator (new ToolStripSeparatorRenderEventArgs (e.Graphics, this, this.Owner.Orientation == Orientation.Horizontal ? true : false));
			}
		}

		protected internal override void SetBounds (Rectangle rect)
		{
			base.SetBounds (rect);
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DisplayStyleChanged {
			add { base.DisplayStyleChanged += value; }
			remove { base.DisplayStyleChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler EnabledChanged {
			add { base.EnabledChanged += value; }
			remove { base.EnabledChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion

		#region Internal Method/Properties
		internal override ToolStripTextDirection DefaultTextDirection { get { return ToolStripTextDirection.Horizontal; } }
		#endregion
	}
}
