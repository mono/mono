//
// ToolStripContainer.cs
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

using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Designer ("System.Windows.Forms.Design.ToolStripContainerDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ToolStripContainer : ContainerControl
	{
		private ToolStripPanel bottom_panel;
		private ToolStripContentPanel content_panel;
		private ToolStripPanel left_panel;
		private ToolStripPanel right_panel;
		private ToolStripPanel top_panel;

		#region Public Constructors
		public ToolStripContainer () : base ()
		{
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			SetStyle (ControlStyles.ResizeRedraw, true);

			content_panel = new ToolStripContentPanel ();
			content_panel.Dock = DockStyle.Fill;
			this.Controls.Add (content_panel);

			this.top_panel = new ToolStripPanel ();
			this.top_panel.Dock = DockStyle.Top;
			this.top_panel.Height = 0;
			this.Controls.Add (top_panel);

			this.bottom_panel = new ToolStripPanel ();
			this.bottom_panel.Dock = DockStyle.Bottom;
			this.bottom_panel.Height = 0;
			this.Controls.Add (bottom_panel);

			this.left_panel = new ToolStripPanel ();
			this.left_panel.Dock = DockStyle.Left;
			this.left_panel.Width = 0;
			this.Controls.Add (left_panel);

			this.right_panel = new ToolStripPanel ();
			this.right_panel.Dock = DockStyle.Right;
			this.right_panel.Width = 0;
			this.Controls.Add (right_panel);
	}
		#endregion

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AutoScroll {
			get { return base.AutoScroll; }
			set { base.AutoScroll = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Image BackgroundImage {
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
		
		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ToolStripPanel BottomToolStripPanel {
			get { return this.bottom_panel; }
		}

		[DefaultValue (true)]
		public bool BottomToolStripPanelVisible {
			get { return this.bottom_panel.Visible; }
			set { this.bottom_panel.Visible = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}

		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ToolStripContentPanel ContentPanel {
			get { return this.content_panel; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new ContextMenuStrip ContextMenuStrip {
			get { return base.ContextMenuStrip; }
			set { base.ContextMenuStrip = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new ControlCollection Controls {
			get { return base.Controls; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override Cursor Cursor {
			get { return base.Cursor; }
			set { base.Cursor = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}
		
		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ToolStripPanel LeftToolStripPanel {
			get { return this.left_panel; }
		}

		[DefaultValue (true)]
		public bool LeftToolStripPanelVisible {
			get { return this.left_panel.Visible; }
			set { this.left_panel.Visible = value; }
		}

		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ToolStripPanel RightToolStripPanel {
			get { return this.right_panel; }
		}

		[DefaultValue (true)]
		public bool RightToolStripPanelVisible {
			get { return this.right_panel.Visible; }
			set { this.right_panel.Visible = value; }
		}
	
		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public ToolStripPanel TopToolStripPanel {
			get { return this.top_panel; }
		}

		[DefaultValue (true)]
		public bool TopToolStripPanelVisible {
			get { return this.top_panel.Visible; }
			set { this.top_panel.Visible = value; }
		}
		#endregion

		#region Protected Properties
		protected override Size DefaultSize {
			get { return new Size (150, 175); }
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override ControlCollection CreateControlsInstance ()
		{
			return new ToolStripContainerTypedControlCollection (this);
		}

		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}
		
		protected override void OnSizeChanged (EventArgs e)
		{
			base.OnSizeChanged (e);
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler BackColorChanged {
			add { base.BackColorChanged += value; }
			remove { base.BackColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ContextMenuStripChanged {
			add { base.ContextMenuStripChanged += value; }
			remove { base.ContextMenuStripChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler CursorChanged {
			add { base.CursorChanged += value; }
			remove { base.CursorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}
		#endregion

		#region Private Class : ToolStripContainerTypedControlCollection
		private class ToolStripContainerTypedControlCollection : ControlCollection
		{
			public ToolStripContainerTypedControlCollection (Control owner) : base (owner)
			{
			}
		}
		#endregion
	}
}
