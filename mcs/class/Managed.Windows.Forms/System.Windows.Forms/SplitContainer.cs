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
// Copyright (c) 2007 Ivan N. Zlatev
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//	Ivan N. Zlatev (contact@i-nz.net)
//

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
		private Orientation orientation;

		private int splitter_increment;
		private Rectangle splitter_rectangle;
		private Rectangle splitter_rectangle_moving;
		private Rectangle splitter_rectangle_before_move;
		private bool splitter_fixed;
		private bool splitter_dragging;
		private int splitter_prev_move;
		private Cursor restore_cursor;
		private double fixed_none_ratio;

		private SplitterPanel panel1;
		private bool panel1_collapsed;
		private int panel1_min_size;

		private SplitterPanel panel2;
		private bool panel2_collapsed;
		private int panel2_min_size;
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

		#region UIA Framework Events
		static object UIACanResizeChangedEvent = new object ();

		internal event EventHandler UIACanResizeChanged {
			add { Events.AddHandler (UIACanResizeChangedEvent, value); }
			remove { Events.RemoveHandler (UIACanResizeChangedEvent, value); }
		}

		internal void OnUIACanResizeChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIACanResizeChangedEvent];
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Public Constructors
		public SplitContainer ()
		{
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			SetStyle (ControlStyles.OptimizedDoubleBuffer, true);
			
			fixed_panel = FixedPanel.None;
			orientation = Orientation.Vertical;

			splitter_rectangle = new Rectangle (50, 0, 4, this.Height);
			splitter_increment = 1;
			splitter_prev_move = -1;
			restore_cursor = null;

			splitter_fixed = false;
			panel1_collapsed = false;
			panel2_collapsed = false;
			panel1_min_size = 25;
			panel2_min_size = 25;

			panel1 = new SplitterPanel (this);
			panel2 = new SplitterPanel (this);
			panel1.Size = new Size (50, 50);
			UpdateSplitter ();

			this.Controls.Add (panel2);
			this.Controls.Add (panel1);
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

		[Browsable (false)]
		[DefaultValue ("{X=0,Y=0}")]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Point AutoScrollOffset {
			get { return base.AutoScrollOffset; }
			set { base.AutoScrollOffset = value; }
		}

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
			set { base.BackgroundImage = value; }
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
			get { return splitter_fixed; }
			set { splitter_fixed = value; }
		}

		[Localizable (true)]
		[DefaultValue (Orientation.Vertical)]
		public Orientation Orientation {
			get { return this.orientation; }
			set {
				if (!Enum.IsDefined (typeof (Orientation), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for Orientation", value));

				if (this.orientation != value) {
					if (value == Orientation.Vertical) {
						splitter_rectangle.Width = splitter_rectangle.Height;
						splitter_rectangle.X = splitter_rectangle.Y;
					} else {
						splitter_rectangle.Height = splitter_rectangle.Width;
						splitter_rectangle.Y = splitter_rectangle.X;
					}

					this.orientation = value;
					this.UpdateSplitter ();
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
				if (panel1_collapsed != value) {
					this.panel1_collapsed = value;
					panel1.Visible = !value;

					// UIA Framework Event: CanResize Changed
					OnUIACanResizeChanged (EventArgs.Empty);

					PerformLayout ();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (25)]
		[RefreshProperties (RefreshProperties.All)]
		public int Panel1MinSize {
			get { return this.panel1_min_size; }
			set { 
				this.panel1_min_size = value; 
			}
		}

		[Localizable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SplitterPanel Panel2 { get { return this.panel2; } }

		[DefaultValue (false)]
		public bool Panel2Collapsed {
			get { return this.panel2_collapsed; }
			set {
				if (panel2_collapsed != value) {
					this.panel2_collapsed = value;
					panel2.Visible = !value;

					// UIA Framework Event: CanResize Changed
					OnUIACanResizeChanged (EventArgs.Empty);

					PerformLayout ();
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (25)]
		[RefreshProperties (RefreshProperties.All)]
		public int Panel2MinSize {
			get { return this.panel2_min_size; }
			set { this.panel2_min_size = value; }
		}

		// MSDN says the default is 40, MS's implementation defaults to 50.
		[Localizable (true)]
		[DefaultValue (50)]
		[SettingsBindable (true)]
		public int SplitterDistance {
			get { 
				if (orientation == Orientation.Vertical)
					return this.splitter_rectangle.X;
				else
					return this.splitter_rectangle.Y;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();

				if (value < panel1_min_size)
					value = panel1_min_size;

				bool updated = true;
				if (orientation == Orientation.Vertical) {
					if (this.Width - (this.SplitterWidth + value) < panel2_min_size)
						value = this.Width - (this.SplitterWidth + panel2_min_size);
					if (splitter_rectangle.X != value) {
						splitter_rectangle.X = value;
						updated = true;
					}
				} else {
					if (this.Height - (this.SplitterWidth + value) < panel2_min_size)
						value = this.Height - (this.SplitterWidth + panel2_min_size);
					if (splitter_rectangle.Y != value) {
						splitter_rectangle.Y = value;
						updated = true;
					}
				}
				if (updated) {
					UpdateSplitter ();
					OnSplitterMoved (new SplitterEventArgs (Left, Top, splitter_rectangle.X, splitter_rectangle.Y));
				}
			}
		}

		[Localizable (true)]
		[DefaultValue (1)]
		[MonoTODO ("Stub, never called")]
		public int SplitterIncrement {
			get { return this.splitter_increment; }
			set { this.splitter_increment = value; }
		}

		[Browsable (false)]
		public Rectangle SplitterRectangle { get { return splitter_rectangle; } }

		[Localizable (true)]
		[DefaultValue (4)]
		public int SplitterWidth {
			get {
				if (orientation == Orientation.Vertical)
					return this.splitter_rectangle.Width;
				else
					return this.splitter_rectangle.Height;
			}
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ();

				if (orientation == Orientation.Vertical)
					this.splitter_rectangle.Width = value;
				else
					this.splitter_rectangle.Height = value;
				UpdateSplitter ();
			}
		}

		[DispId (-516)]
		[DefaultValue (true)]
		[MonoTODO ("Stub, never called")]
		new public bool TabStop {
			get { return false; }
			set { }
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
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override ControlCollection CreateControlsInstance ()
		{
			return new SplitContainerTypedControlCollection (this);
		}

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

		protected override void OnLayout (LayoutEventArgs e)
		{
			UpdateLayout ();
			base.OnLayout (e);
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
			if (!splitter_fixed && SplitterHitTest (e.Location)) {
				splitter_dragging = true;
				SplitterBeginMove (e.Location);
			}
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
			SplitterRestoreCursor ();
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);

			if (splitter_dragging)
				SplitterMove (e.Location);

			if (!splitter_fixed && SplitterHitTest (e.Location))
				SplitterSetCursor (orientation);
			
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);
			if (splitter_dragging) {
				SplitterEndMove (e.Location, false);
				SplitterRestoreCursor ();
				splitter_dragging = false;
			}
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

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return base.ProcessDialogKey (keyData);
		}

		protected override bool ProcessTabKey (bool forward)
		{
			return base.ProcessTabKey (forward);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
		}
		
		protected override void Select (bool directed, bool forward)
		{
			base.Select (directed, forward);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc (ref Message msg)
		{			
			base.WndProc (ref msg);
		}
		#endregion
		
		#region Private Methods

		private bool SplitterHitTest (Point location)
		{
			if (location.X >= splitter_rectangle.X &&
				location.X <= splitter_rectangle.X + splitter_rectangle.Width &&
				location.Y >= splitter_rectangle.Y &&
				location.Y <= splitter_rectangle.Y + splitter_rectangle.Height) {
				return true;
			}
			return false;				   
		}

		private void SplitterBeginMove (Point location)
		{
			splitter_prev_move = orientation == Orientation.Vertical ? location.X : location.Y;
			splitter_rectangle_moving = splitter_rectangle;
			splitter_rectangle_before_move = splitter_rectangle;
		}

		private void SplitterMove (Point location)
		{
			int currentMove = orientation == Orientation.Vertical ? location.X : location.Y;
			int delta = currentMove - splitter_prev_move;
			Rectangle prev_location = splitter_rectangle_moving;
			bool moved = false;

			if (orientation == Orientation.Vertical) {
				int min = panel1_min_size;
				int max = panel2.Location.X + (panel2.Width - this.panel2_min_size) - splitter_rectangle_moving.Width;

				if (splitter_rectangle_moving.X + delta > min && splitter_rectangle_moving.X + delta < max) {
					splitter_rectangle_moving.X += delta;
					moved = true;
				} else {
					// Ensure that the splitter is set to minimum or maximum position, 
					// even if the mouse "skips".
					//
					if (splitter_rectangle_moving.X + delta <= min && splitter_rectangle_moving.X != min) {
						splitter_rectangle_moving.X = min;
						moved = true;
					} else if (splitter_rectangle_moving.X + delta >= max && splitter_rectangle_moving.X != max) {
						splitter_rectangle_moving.X = max;
						moved = true;
					}
				}
			} else if (orientation == Orientation.Horizontal) {
				int min = panel1_min_size;
				int max = panel2.Location.Y + (panel2.Height - this.panel2_min_size) - splitter_rectangle_moving.Height;

				if (splitter_rectangle_moving.Y + delta > min && splitter_rectangle_moving.Y + delta < max) {
					splitter_rectangle_moving.Y += delta;
					moved = true;
				} else {
					// Ensure that the splitter is set to minimum or maximum position, 
					// even if the mouse "skips".
					//
					if (splitter_rectangle_moving.Y + delta <= min && splitter_rectangle_moving.Y != min) {
						splitter_rectangle_moving.Y = min;
						moved = true;
					} else if (splitter_rectangle_moving.Y + delta >= max && splitter_rectangle_moving.Y != max) {
						splitter_rectangle_moving.Y = max;
						moved = true;
					}
				}
			}

			if (moved) {
				splitter_prev_move = currentMove;
				OnSplitterMoving (new SplitterCancelEventArgs (location.X, location.Y, 
									       splitter_rectangle.X, splitter_rectangle.Y));
				XplatUI.DrawReversibleRectangle (this.Handle, prev_location, 1);
				XplatUI.DrawReversibleRectangle (this.Handle, splitter_rectangle_moving, 1);
			}
		}

		private void SplitterEndMove (Point location, bool cancel)
		{
			if (!cancel) {
				// Prevent updating the splitter distance if the user changes it in e.g. the
				// DoubleClick handler, but no delta move has happened in our drag-handling. 
				// We don't compare to splitter_rectangle for exactly that reason here 
				// (if it gets changed externally) and compare to a cached value.
				// 
				if (splitter_rectangle_before_move != splitter_rectangle_moving) {
					splitter_rectangle = splitter_rectangle_moving;
					UpdateSplitter ();
				}
			}
			SplitterEventArgs args = new SplitterEventArgs (location.X, location.Y, 
									splitter_rectangle.X, splitter_rectangle.Y);
			OnSplitterMoved (args);
		}

		private void SplitterSetCursor (Orientation orientation)
		{
			if (restore_cursor == null)
				restore_cursor = this.Cursor;
			this.Cursor = orientation == Orientation.Vertical ? Cursors.VSplit : Cursors.HSplit;
		}

		private void SplitterRestoreCursor ()
		{
			if (restore_cursor != null) {
				this.Cursor = restore_cursor;
				restore_cursor = null;
			}
		}

		private void UpdateSplitter ()
		{
			this.SuspendLayout ();
			panel1.SuspendLayout ();
			panel2.SuspendLayout ();

			if (panel1_collapsed) {
				panel2.Size = this.Size;
				panel2.Location = new Point (0, 0);
			} else if (panel2_collapsed) {
				panel1.Size = this.Size;
				panel1.Location = new Point (0, 0);
			} else {
				panel1.Location = new Point (0, 0);
				if (orientation == Orientation.Vertical) {
					splitter_rectangle.Y = 0;
					panel1.InternalHeight = panel2.InternalHeight = this.Height;
					panel1.InternalWidth = Math.Max (this.SplitterDistance, panel1_min_size);
					panel2.Location = new Point (this.SplitterWidth + this.SplitterDistance, 0);
					panel2.InternalWidth = Math.Max (this.Width - (this.SplitterWidth + this.SplitterDistance), panel2_min_size);
					fixed_none_ratio = (double) this.Width / (double)this.SplitterDistance;
				} else if (orientation == Orientation.Horizontal) {
					splitter_rectangle.X = 0;
					panel1.InternalWidth = panel2.InternalWidth = this.Width;
					panel1.InternalHeight =  Math.Max (this.SplitterDistance, panel1_min_size);
					panel2.Location = new Point (0, this.SplitterWidth + this.SplitterDistance);
					panel2.InternalHeight =  Math.Max (this.Height - (this.SplitterWidth + this.SplitterDistance), panel2_min_size);
					fixed_none_ratio = (double) this.Height / (double)this.SplitterDistance;
				}
			}
			panel1.ResumeLayout ();
			panel2.ResumeLayout ();
			this.ResumeLayout ();
		}

		private void UpdateLayout ()
		{
			panel1.SuspendLayout ();
			panel2.SuspendLayout ();

			if (panel1_collapsed) {
				panel2.Size = this.Size;
				panel2.Location = new Point (0, 0);
			} else if (panel2_collapsed) {
				panel1.Size = this.Size;
				panel1.Location = new Point (0, 0);
			} else {
				panel1.Location = new Point (0, 0);
				if (orientation == Orientation.Vertical) {
					panel1.Location = new Point (0, 0);
					panel1.InternalHeight = panel2.InternalHeight = this.Height;
					splitter_rectangle.Height = this.Height;
	
					if (fixed_panel == FixedPanel.None) {
						splitter_rectangle.X = Math.Max ((int)Math.Floor (((double)this.Width) / fixed_none_ratio), panel1_min_size); //set distance
						panel1.InternalWidth = this.SplitterDistance;
						panel2.InternalWidth = this.Width - (this.SplitterWidth + this.SplitterDistance);
						panel2.Location = new Point (this.SplitterWidth + this.SplitterDistance, 0);
					} else if (fixed_panel == FixedPanel.Panel1) {
						panel1.InternalWidth = this.SplitterDistance;
						panel2.InternalWidth = Math.Max (this.Width - (this.SplitterWidth + this.SplitterDistance), panel2_min_size);
						panel2.Location = new Point (this.SplitterWidth + this.SplitterDistance, 0);
					} else if (fixed_panel == FixedPanel.Panel2) {
						splitter_rectangle.X = Math.Max (this.Width - (this.SplitterWidth + panel2.Width), panel1_min_size); //set distance
						panel1.InternalWidth = this.SplitterDistance;
						panel2.Location = new Point (this.SplitterWidth + this.SplitterDistance, 0);
					}
				} else if (orientation == Orientation.Horizontal) {
					panel1.Location = new Point (0, 0);
					panel1.InternalWidth = panel2.InternalWidth = this.Width;
					splitter_rectangle.Width = this.Width;

					if (fixed_panel == FixedPanel.None) {
						splitter_rectangle.Y = Math.Max ((int) Math.Floor ((double)this.Height / fixed_none_ratio), panel1_min_size); //set distance
						panel1.InternalHeight = this.SplitterDistance;
						panel2.InternalHeight = this.Height - (this.SplitterWidth + this.SplitterDistance);
						panel2.Location = new Point (0, this.SplitterWidth + this.SplitterDistance);
					} else if (fixed_panel == FixedPanel.Panel1) {
						panel1.InternalHeight = this.SplitterDistance;
						panel2.InternalHeight = Math.Max (this.Height - (this.SplitterWidth + this.SplitterDistance), panel2_min_size);
						panel2.Location = new Point (0, this.SplitterWidth + this.SplitterDistance);
					} else if (fixed_panel == FixedPanel.Panel2) {
						splitter_rectangle.Y =  Math.Max (this.Height - (this.SplitterWidth + panel2.Height), panel1_min_size); //set distance
						panel1.InternalHeight = this.SplitterDistance;
						panel2.Location = new Point (0, this.SplitterWidth + this.SplitterDistance);
					}
				}
			}

			panel1.ResumeLayout ();
			panel2.ResumeLayout ();
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
