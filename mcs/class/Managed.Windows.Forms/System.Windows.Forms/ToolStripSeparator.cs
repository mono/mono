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
#if NET_2_0

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class ToolStripSeparator : ToolStripItem
	{
		private bool auto_tool_tip;
		private ToolStripItemDisplayStyle display_style;
		private bool double_click_enabled;
		private ContentAlignment image_align;
		private int image_index;
		private ToolStripItemImageScaling image_scaling;
		private ContentAlignment text_align;
		private TextImageRelation text_image_relation;
		private string tool_tip_text;

		public ToolStripSeparator ()
		{
			this.ForeColor = SystemColors.ControlDark;
		}

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool AutoToolTip {
			get { return this.auto_tool_tip; }
			set { this.auto_tool_tip = value; }
		}

		public override bool CanSelect { get { return false; } }

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public ToolStripItemDisplayStyle DisplayStyle {
			get { return this.display_style; }
			set { this.display_style = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool DoubleClickEnabled {
			get { return this.double_click_enabled; }
			set { this.double_click_enabled = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool Enabled {
			get { return base.Enabled; }
			set { base.Enabled = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image Image {
			get { return base.Image; }
			set { base.Image = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public ContentAlignment ImageAlign {
			get { return this.image_align; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ContentAlignment", value));

				this.image_align = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int ImageIndex {
			get { return this.image_index; }
			set {
				if (value < -1)
					throw new ArgumentException ("ImageIndex cannot be less than -1");

				this.image_index = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public ToolStripItemImageScaling ImageScaling {
			get { return this.image_scaling; }
			set { this.image_scaling = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public ContentAlignment TextAlign {
			get { return this.text_align; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ContentAlignment", value));

				this.text_align = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public TextImageRelation TextImageRelation {
			get { return this.text_image_relation; }
			set { this.text_image_relation = value;	}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public string ToolTipText {
			get { return this.tool_tip_text; }
			set { this.tool_tip_text = value; }
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
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
			
			if (this.Owner != null)
				this.Owner.Renderer.DrawSeparator(new ToolStripSeparatorRenderEventArgs(e.Graphics, this, this.Owner.Orientation == Orientation.Horizontal ? true : false));
		}

		protected internal override void SetBounds (Rectangle bounds)
		{
			base.SetBounds (bounds);
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler DisplayStyleChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler EnabledChanged;

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler TextChanged;
		#endregion
	}
}
#endif