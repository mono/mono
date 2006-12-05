//
// ToolStripContentPanel.cs
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
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[ComVisible(true)]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	public class ToolStripContentPanel : Panel
	{
		private ToolStripRenderMode render_mode;
		private ToolStripRenderer renderer;

		#region Public Constructors
		public ToolStripContentPanel () : base ()
		{
			this.renderer = null;
			this.render_mode = ToolStripRenderMode.ManagerRenderMode;
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		public override AnchorStyles Anchor {
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}
		
		[Browsable (false)]
		public override bool AutoScroll {
			get { return base.AutoScroll; }
			set { base.AutoScroll = value; }
		}
		
		[Browsable (false)]
		public new Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}
		
		[Browsable (false)]
		public new Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}
		
		[Browsable (false)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		public override Color BackColor {
			get { return base.BackColor; }
			set { base.BackColor = value; 
				
				if (this.Parent != null)
					this.Parent.BackColor = value;
			}
		}
		
		[Browsable (false)]
		public new bool CausesValidation {
			get { return base.CausesValidation; }
			set { base.CausesValidation = value; }
		}
		
		[Browsable (false)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}
		
		[Browsable (false)]
		public new Point Location {
			get { return base.Location; }
			set { base.Location = value; }
		}

		[Browsable (false)]
		public override Size MaximumSize {
			get { return base.MaximumSize; }
			set { base.MaximumSize = value; }
		}
		
		[Browsable (false)]
		public override Size MinimumSize {
			get { return base.MinimumSize; }
			set { base.MinimumSize = value; }
		}
		
		[Browsable (false)]
		public new string Name {
			get { return base.Name; }
			set { base.Name = value; }
		}

		[Browsable (false)]
		public ToolStripRenderer Renderer {
			get {
				if (this.render_mode == ToolStripRenderMode.ManagerRenderMode)
					return ToolStripManager.Renderer;

				return this.renderer;
			}
			set { 
				this.renderer = value;
				this.OnRendererChanged (EventArgs.Empty);
			}
		}

		[DefaultValue (ToolStripRenderMode.ManagerRenderMode)]
		public ToolStripRenderMode RenderMode {
			get { return this.render_mode; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripRenderMode), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripRenderMode", value));

				if (value == ToolStripRenderMode.Custom && this.renderer == null)
					throw new NotSupportedException ("Must set Renderer property before setting RenderMode to Custom");
				if (value == ToolStripRenderMode.Professional || value == ToolStripRenderMode.System)
					this.renderer = new ToolStripProfessionalRenderer ();

				this.render_mode = value;
			}
		}

		[Browsable (false)]
		public new int TabIndex {
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}
		
		[Browsable (false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}
		#endregion

		#region Protected Methods
		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}
		
		protected virtual void OnLoad (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [LoadEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnPaintBackground (PaintEventArgs pevent)
		{
			base.OnPaintBackground (pevent);
		}
		
		protected virtual void OnRendererChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RendererChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Public Events
		static object AutoSizeChangedEvent = new object ();
		static object LoadEvent = new object ();
		static object RendererChangedEvent = new object ();

		[Browsable (false)]
		public event EventHandler AutoSizeChanged {
			add { Events.AddHandler (AutoSizeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeChangedEvent, value); }
		}

		[Browsable (false)]
		public new event EventHandler CausesValidationChanged {
			add { base.CausesValidationChanged += value; }
			remove { base.CausesValidationChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler DockChanged {
			add { base.DockChanged += value; }
			remove { base.DockChanged -= value; }
		}

		public event EventHandler Load {
			add { Events.AddHandler (LoadEvent, value); }
			remove { Events.RemoveHandler (LoadEvent, value); }
		}

		[Browsable (false)]
		public new event EventHandler LocationChanged {
			add { base.LocationChanged += value; }
			remove { base.LocationChanged -= value; }
		}

		public event EventHandler RendererChanged {
			add { Events.AddHandler (RendererChangedEvent, value); }
			remove { Events.RemoveHandler (RendererChangedEvent, value); }
		}

		[Browsable (false)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}
		#endregion
	}
}
#endif
