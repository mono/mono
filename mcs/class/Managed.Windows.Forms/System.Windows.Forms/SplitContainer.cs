//
// SplitContainer.cs
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
using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[ComVisibleAttribute (true)]
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
	[DefaultEvent ("SplitterMoved")]
	[Docking (DockingBehavior.AutoDock)]
	[Designer ("System.Windows.Forms.Design.SplitContainerDesigner, " + Consts.AssemblySystem_Design)]
	public class SplitContainer : ContainerControl
	{
		#region Local Variables
		private FixedPanel fixed_panel;
		private int splitter_distance;
		private int splitter_width;
		private int splitter_increment;
		private Orientation orientation;

		private SplitterPanel panel1;
		private bool panel1_collapsed;
		private int panel1_min_size;

		private SplitterPanel panel2;
		private bool panel2_collapsed;
		private int panel2_min_size;

		private Splitter splitter;
		#endregion

		#region Public Events
		static object SplitterMovedEvent = new object ();
		static object SplitterMovingEvent = new object ();

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event ControlEventHandler ControlAdded {
			add { base.ControlAdded += value; }
			remove { base.ControlAdded -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event ControlEventHandler ControlRemoved {
			add { base.ControlRemoved += value; }
			remove { base.ControlRemoved -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}
		
		public event SplitterEventHandler SplitterMoved {
			add { Events.AddHandler (SplitterMovedEvent, value); }
			remove { Events.RemoveHandler (SplitterMovedEvent, value); }
		}

		public event SplitterCancelEventHandler SplitterMoving {
			add { Events.AddHandler (SplitterMovingEvent, value); }
			remove { Events.RemoveHandler (SplitterMovingEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion

		#region Public Constructors
		public SplitContainer ()
		{
			fixed_panel = FixedPanel.None;
			orientation = Orientation.Vertical;
			splitter_distance = 50;
			splitter_width = 4;
			splitter_increment = 1;
			panel1_collapsed = false;
			panel2_collapsed = false;
			panel1_min_size = 25;
			panel2_min_size = 25;

			panel1 = new SplitterPanel (this);
			panel2 = new SplitterPanel (this);
			splitter = new Splitter ();

			splitter.TabStop = true;
			splitter.Size = new System.Drawing.Size (4, 4);
			splitter.SplitterMoved += new SplitterEventHandler (splitter_SplitterMoved);
			splitter.SplitterMoving += new SplitterEventHandler (splitter_SplitterMoving);

			panel1.Size = new Size (50, 50);

			this.Controls.Add (panel2);
			this.Controls.Add (splitter);
			this.Controls.Add (panel1);

			panel1.Dock = DockStyle.Left;
			panel2.Dock = DockStyle.Fill;
			splitter.Dock = DockStyle.Left;
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Localizable (true)]
		[DefaultValue (false)]
		public override bool AutoScroll {
			get { return base.AutoScroll; }
			set { base.AutoScroll = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}

		//Uncomment once this has been implemented in Control.cs
		//[Browsable (false)]
		//[EditorBrowsable (EditorBrowsableState.Never)]
		//public override Point AutoScrollOffset {
		//        get { return base.AutoScrollOffset; }
		//        set { base.AutoScrollOffset = value; }
		//}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Point AutoScrollPosition {
			get { return base.AutoScrollPosition; }
			set { base.AutoScrollPosition = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				base.BackgroundImage = value;
				UpdateSplitterBackground ();
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
		        get { return base.BackgroundImageLayout; }
		        set { base.BackgroundImageLayout = value; }
		}

		[Browsable (false)]
		public override BindingContext BindingContext {
			get { return base.BindingContext; }
			set { base.BindingContext = value; }
		}

		// MSDN says default is Fixed3D, creating a new SplitContainer says otherwise.
		[DefaultValue (BorderStyle.None)]
		[DispId (-504)]
		public BorderStyle BorderStyle {
			get { return panel1.BorderStyle; }
			set {
				if (!Enum.IsDefined (typeof (BorderStyle), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for BorderStyle", value));
					
				panel1.BorderStyle = value;
				panel2.BorderStyle = value;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public ControlCollection Controls { get { return base.Controls; } }

		new public DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		[DefaultValue (FixedPanel.None)]
		public FixedPanel FixedPanel {
			get { return this.fixed_panel; }
			set {
				if (!Enum.IsDefined (typeof (FixedPanel), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for FixedPanel", value));

				this.fixed_panel = value;
			}
		}

		[Localizable (true)]
		[DefaultValue (false)]
		public bool IsSplitterFixed {
			get { return !splitter.Enabled; }
			set { splitter.Enabled = !value; }
		}

		[Localizable (true)]
		[DefaultValue (Orientation.Vertical)]
		public Orientation Orientation {
			get { return this.orientation; }
			set {
				if (!Enum.IsDefined (typeof (Orientation), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for Orientation", value));

				if (this.orientation != value) {
					this.orientation = value;

					switch (value) {
						case Orientation.Vertical:
							panel1.Dock = DockStyle.Left;
							panel2.Dock = DockStyle.Fill;
							splitter.Dock = DockStyle.Left;
							splitter.Width = this.splitter_width;
							panel1.InternalWidth = this.splitter_distance;
							if (panel2.Width < panel2_min_size)
								panel1.InternalWidth = this.Width - this.splitter_width - panel2_min_size;
							break;
						case Orientation.Horizontal:
						default:
							panel1.Dock = DockStyle.Top;
							panel2.Dock = DockStyle.Fill;
							splitter.Dock = DockStyle.Top;
							splitter.Height = this.splitter_width;
							panel1.InternalHeight = this.splitter_distance;
							if (panel2.Height < panel2_min_size)
								panel1.InternalHeight = this.Height - this.splitter_width - panel2_min_size;
							break;
					}

					this.PerformLayout ();
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}

		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SplitterPanel Panel1 { get { return this.panel1; } }

		[DefaultValue (false)]
		public bool Panel1Collapsed {
			get { return this.panel1_collapsed; }
			set {
				this.panel1_collapsed = value;
				this.panel1.Visible = !value;
				this.splitter.Visible = !value;
			}
		}

		[Localizable (true)]
		[DefaultValue (25)]
		[RefreshProperties (RefreshProperties.All)]
		public int Panel1MinSize {
			get { return this.panel1_min_size; }
			set { 
				this.panel1_min_size = value; 
				this.splitter.MinSize = value; 
			}
		}

		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SplitterPanel Panel2 { get { return this.panel2; } }

		[DefaultValue (false)]
		public bool Panel2Collapsed {
			get { return this.panel2_collapsed; }
			set {
				this.panel2_collapsed = value; 
				this.panel2.Visible = !value;
				this.splitter.Visible = !value;
			}
		}

		[Localizable (true)]
		[DefaultValue (25)]
		[RefreshProperties (RefreshProperties.All)]
		public int Panel2MinSize {
			get { return this.panel2_min_size; }
			set { this.panel2_min_size = value; this.splitter.MinExtra = value; }
		}

		// MSDN says the default is 40, MS's implementation defaults to 50.
		[Localizable (true)]
		[DefaultValue (50)]
		[SettingsBindable (true)]
		public int SplitterDistance {
			get { return this.splitter_distance; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();

				if (value < this.panel1_min_size)
					value = this.panel1_min_size;

				switch (this.orientation) {
					case Orientation.Vertical:
						if (value > this.Width - this.panel2_min_size - this.splitter_width)
							value = this.Width - this.panel2_min_size - this.splitter_width;
						panel1.InternalWidth = value;
						break;
					case Orientation.Horizontal:
					default:
						if (value > this.Height - this.panel2_min_size - this.splitter_width)
							value = this.Height - this.panel2_min_size - this.splitter_width;
						panel1.InternalHeight = value;
						break;
				}

				this.splitter_distance = value;

				UpdateSplitterBackground ();
			}
		}

		[Localizable (true)]
		[DefaultValue (1)]
		[MonoTODO ("Not implemented.")]
		public int SplitterIncrement {
			get { return this.splitter_increment; }
			set { this.splitter_increment = value; }
		}

		[Browsable (false)]
		public Rectangle SplitterRectangle { get { return splitter.Bounds; } }

		[Localizable (true)]
		[DefaultValue (4)]
		public int SplitterWidth {
			get { return this.splitter_width; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ();

				this.splitter_width = value;

				switch (this.orientation) {
					case Orientation.Horizontal:
						splitter.Height = value;
						break;
					case Orientation.Vertical:
					default:
						splitter.Width = value;
						break;
				}
			}
		}

		[DispId (-516)]
		[DefaultValue (true)]
		new public bool TabStop {
			get { return splitter.TabStop; }
			set { splitter.TabStop = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable (false)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}
		#endregion

		#region Protected Properties
		protected override Size DefaultSize { get { return new Size (150, 100); } }
		#endregion

		#region Public Methods
		public void OnSplitterMoved (SplitterEventArgs e)
		{
			SplitterEventHandler eh = (SplitterEventHandler)(Events [SplitterMovedEvent]);
			if (eh != null)
				eh (this, e);
		}

		public void OnSplitterMoving (SplitterCancelEventArgs e)
		{
			SplitterCancelEventHandler eh = (SplitterCancelEventHandler)(Events [SplitterMovingEvent]);
			if (eh != null)
				eh (this, e);

			if (e.Cancel == true) {
				e.SplitX = splitter.Location.X;
				e.SplitY = splitter.Location.Y;
			}
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override ControlCollection CreateControlsInstance ()
		{
			return new SplitContainerTypedControlCollection (this);
		}

		[MonoTODO ("Special focus semantics not implemented")]
		protected override void OnGotFocus (EventArgs e)
		{
			base.OnGotFocus (e);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			base.OnKeyDown (e);
		}

		protected override void OnKeyUp (KeyEventArgs e)
		{
			base.OnKeyUp (e);
		}

		protected override void OnLayout (LayoutEventArgs levent)
		{
			base.OnLayout (levent);
		}

		protected override void OnLostFocus (EventArgs e)
		{
			base.OnLostFocus (e);
		}

		protected override void OnMouseCaptureChanged (EventArgs e)
		{
			base.OnMouseCaptureChanged (e);
		}
		
		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);
		}
		
		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}

		[MonoTODO ("Special focus semantics not implemented")]
		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey (keyData);
		}

		[MonoTODO ("Special focus semantics not implemented")]
		protected override bool ProcessTabKey (bool forward)
		{
			return base.ProcessTabKey (forward);
		}

		[MonoTODO ("Special focus semantics not implemented")]
		protected override void Select (bool directed, bool forward)
		{
			base.Select (directed, forward);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		#endregion
		
		#region Private Methods
		private void splitter_SplitterMoving (object sender, SplitterEventArgs e)
		{
			SplitterCancelEventArgs ea = new SplitterCancelEventArgs (e.X, e.Y, e.SplitX, e.SplitY);
			this.OnSplitterMoving (ea);
			e.SplitX = ea.SplitX;
			e.SplitY = ea.SplitY;
		}

		private void splitter_SplitterMoved (object sender, SplitterEventArgs e)
		{
			this.OnSplitterMoved (e);
		}

		private void UpdateSplitterBackground ()
		{
			if (this.BackgroundImage != null) {
				Bitmap b = new Bitmap (splitter.Width, splitter.Height);
				Graphics.FromImage (b).DrawImage (base.BackgroundImage, new Rectangle (0, 0, b.Width, b.Height), this.SplitterRectangle, GraphicsUnit.Pixel);
				splitter.BackgroundImage = b;
			}
			else
				splitter.BackgroundImage = this.BackgroundImage;
		}
		#endregion

		#region Internal Classes
		internal class SplitContainerTypedControlCollection : ControlCollection
		{
			public SplitContainerTypedControlCollection (Control owner) : base (owner)
			{
			}
		}
		#endregion
	}
}
#endif
