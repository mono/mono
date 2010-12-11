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
// Copyright (c) 2005-2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Dennis Bartok	(pbartok@novell.com)
//      Ivan N. Zlatev          (contact i-nz.net)
//
//

// COMPLETE

#undef Debug

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
#if NET_2_0
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
#endif
	[DefaultEvent("SplitterMoved")]
	[Designer("System.Windows.Forms.Design.SplitterDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultProperty("Dock")]
	public class Splitter : Control
#if !NET_2_0
	, IMessageFilter
#endif
	{
		#region Local Variables
		static private Cursor		splitter_ns;
		static private Cursor		splitter_we;
		// XXX this "new" shouldn't be here.  Control shouldn't define border_style as internal.
		new private BorderStyle		border_style;
		private int			min_extra;
		private int			min_size;
		private int                     max_size;
		private int			splitter_size;		// Size (width or height) of our splitter control
		private bool			horizontal;		// true if we've got a horizontal splitter
		private Control			affected;		// The control that the splitter resizes
		private int			split_requested;	// If the user requests a position before we have ever laid out the doc
		private int 			splitter_prev_move;
		private Rectangle 		splitter_rectangle_moving;
		private int			moving_offset;
		#endregion	// Local Variables

		#region Constructors
		static Splitter() {
			splitter_ns = Cursors.HSplit;
			splitter_we = Cursors.VSplit;
		}

		public Splitter() {

			min_extra = 25;
			min_size = 25;
			split_requested = -1;
			splitter_size = 3;
			horizontal = false;

			SetStyle(ControlStyles.Selectable, false);
			Anchor = AnchorStyles.None;
			Dock = DockStyle.Left;

			Layout += new LayoutEventHandler(LayoutSplitter);
			this.ParentChanged += new EventHandler(ReparentSplitter);
			Cursor = splitter_we;
		}
		#endregion	// Constructors

		#region Public Instance Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AllowDrop {
			get {
				return base.AllowDrop;
			}

			set {
				base.AllowDrop = value;
			}
		}

		[Browsable(false)]
		[DefaultValue(AnchorStyles.None)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override AnchorStyles Anchor {
			get {
				return AnchorStyles.None;
			}

			set {
				;	// MS doesn't set it
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}

			set {
				base.BackgroundImage = value;
			}
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}
#endif

		[DispId(-504)]
		[DefaultValue (BorderStyle.None)]
		[MWFDescription("Sets the border style for the splitter")]
		[MWFCategory("Appearance")]
		public BorderStyle BorderStyle {
			get {
				return border_style;
			}

			set {
				border_style = value;

				switch(value) {
				case BorderStyle.FixedSingle:
					splitter_size = 4;	// We don't get motion events for 1px wide windows on X11. sigh.
					break;

				case BorderStyle.Fixed3D:
					value = BorderStyle.None;
					splitter_size = 3;
					break;

				case BorderStyle.None:
					splitter_size = 3;
					break;

				default:
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for BorderStyle", value));
				}

				base.InternalBorderStyle = value;
			}
		}

		[DefaultValue(DockStyle.Left)]
		[Localizable(true)]
		public override DockStyle Dock {
			get {
				return base.Dock;
			}

			set {
				if (!Enum.IsDefined (typeof (DockStyle), value) || (value == DockStyle.None) || (value == DockStyle.Fill)) {
					throw new ArgumentException("Splitter must be docked left, top, bottom or right");
				}

				if ((value == DockStyle.Top) || (value == DockStyle.Bottom)) {
					horizontal = true;
					Cursor = splitter_ns;
				} else {
					horizontal = false;
					Cursor = splitter_we;
				}
				base.Dock = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Font Font {
			get {
				return base.Font;
			}

			set {
				base.Font = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			get {
				return base.ForeColor;
			}

			set {
				base.ForeColor = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ImeMode ImeMode {
			get {
				return base.ImeMode;
			}

			set {
				base.ImeMode = value;
			}
		}

		[DefaultValue(25)]
		[Localizable(true)]
		[MWFDescription("Sets minimum size of undocked window")]
		[MWFCategory("Behaviour")]
		public int MinExtra {
			get {
				return min_extra;
			}

			set {
				min_extra = value;
			}
		}

		[DefaultValue(25)]
		[Localizable(true)]
		[MWFDescription("Sets minimum size of the resized control")]
		[MWFCategory("Behaviour")]
		public int MinSize {
			get {
				return min_size;
			}

			set {
				min_size = value;
			}
		}

		internal int MaxSize {
			get {
				if (this.Parent == null)
					return 0;

				if (affected == null)
					affected = AffectedControl;

				int widths = 0;
				int heights = 0;
				int vert_offset = 0;
				int horiz_offset = 0;
				foreach (Control c in this.Parent.Controls) {
					if (c != affected) {
						switch (c.Dock) {
						case DockStyle.Left:
						case DockStyle.Right:
							widths += c.Width;

							if (c.Location.X < this.Location.X)
								vert_offset += c.Width;
							break;
						case DockStyle.Top:
						case DockStyle.Bottom:
							heights += c.Height;

							if (c.Location.Y < this.Location.Y)
								horiz_offset += c.Height;
							break;
						}
					}
				}

				if (horizontal) {
					moving_offset = horiz_offset;

					return Parent.ClientSize.Height - heights - MinExtra;
				} else {
					moving_offset = vert_offset;

					return Parent.ClientSize.Width - widths - MinExtra;
				}
			}
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MWFDescription("Current splitter position")]
		[MWFCategory("Layout")]
		public int SplitPosition {
			get {
				affected = AffectedControl;
				if (affected == null) {
					return -1;
				}

				if (Capture) {
					return CalculateSplitPosition();
				}

				if (horizontal) {
					return affected.Height;
				} else {
					return affected.Width;
				}
			}

			set {
				if (value > MaxSize)
					value = MaxSize;
				if (value < MinSize)
					value = MinSize;

				affected = AffectedControl;
				if (affected == null)
					split_requested = value;
				else {
					if (horizontal)
						affected.Height = value;
					else
						affected.Width = value;
					OnSplitterMoved (new SplitterEventArgs (Left, Top, value, value));
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

#if NET_2_0
		protected override Cursor DefaultCursor {
			get { return base.DefaultCursor; }
		}
#endif

		protected override ImeMode DefaultImeMode {
			get {
				return ImeMode.Disable;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size (3, 3);
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
#if !NET_2_0
		public bool PreFilterMessage(ref Message m) {
			return false;
		}
#endif

		public override string ToString() {
			return base.ToString () + String.Format(", MinExtra: {0}, MinSize: {1}", min_extra, min_size);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown (e);
			if (Capture && (e.KeyCode == Keys.Escape)) {
				Capture = false;
				SplitterEndMove (Point.Empty, true);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			base.OnMouseDown (e);

			// Only allow if we are set up properly
			if (affected == null)
				affected = AffectedControl;
			max_size = MaxSize;

			if (affected == null || e.Button != MouseButtons.Left)
				return;

			Capture = true;
			SplitterBeginMove (Parent.PointToClient (PointToScreen (new Point (e.X, e.Y))));
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);

			if (!Capture  || e.Button != MouseButtons.Left || affected == null)
				return;

			// We need our mouse coordinates relative to our parent
			SplitterMove (Parent.PointToClient (PointToScreen (e.Location)));
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);
			if (!Capture || e.Button != MouseButtons.Left || affected == null) 
				return;

			SplitterEndMove (Parent.PointToClient (PointToScreen (e.Location)), false);
			Capture = false;
		}

		private void SplitterBeginMove (Point location)
		{
			splitter_rectangle_moving = new Rectangle (Bounds.X, Bounds.Y,
								   Width, Height);
			splitter_prev_move = horizontal ? location.Y : location.X;
		}

		private void SplitterMove (Point location)
		{
			int currentMove = horizontal ? location.Y : location.X;
			int delta = currentMove - splitter_prev_move;
			Rectangle prev_location = splitter_rectangle_moving;
			bool moved = false;
			int min = this.MinSize + moving_offset;
			int max = max_size + moving_offset;

			if (horizontal) {
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
			} else {
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
			}

			if (moved) {
				splitter_prev_move = currentMove;
				OnSplitterMoving (new SplitterEventArgs (location.X, location.Y, 
									 splitter_rectangle_moving.X, 
									 splitter_rectangle_moving.Y));
				XplatUI.DrawReversibleRectangle (this.Parent.Handle, prev_location, 1);
				XplatUI.DrawReversibleRectangle (this.Parent.Handle, splitter_rectangle_moving, 1);
			}
		}

		private void SplitterEndMove (Point location, bool cancel)
		{
			if (!cancel) {
				// Resize the affected window
				if (horizontal)
					affected.Height = CalculateSplitPosition();
				else
					affected.Width = CalculateSplitPosition();
			}

			this.Parent.Refresh (); // to clean up the drag handle artifacts from all controls
			SplitterEventArgs args = new SplitterEventArgs (location.X, location.Y, 
									splitter_rectangle_moving.X, 
									splitter_rectangle_moving.Y);
			OnSplitterMoved (args);
		}

		protected virtual void OnSplitterMoved(SplitterEventArgs sevent) {
			SplitterEventHandler eh = (SplitterEventHandler)(Events [SplitterMovedEvent]);
			if (eh != null)
				eh (this, sevent);
		}

		protected virtual void OnSplitterMoving(SplitterEventArgs sevent) {
			SplitterEventHandler eh = (SplitterEventHandler)(Events [SplitterMovingEvent]);
			if (eh != null)
				eh (this, sevent);
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// enforce our width / height
			if (horizontal) {
				splitter_size = height;
				if (splitter_size < 1) {
					splitter_size = 3;
				}
				base.SetBoundsCore (x, y, width, splitter_size, specified);
			} else {
				splitter_size = width;
				if (splitter_size < 1) {
					splitter_size = 3;
				}
				base.SetBoundsCore (x, y, splitter_size, height, specified);
			}
		}
		#endregion	// Protected Instance Methods

		#region Private Properties and Methods
		private Control AffectedControl {
			get {
				if (Parent == null)
					return null;

				// Doc says the first control preceeding us in the zorder 
				for (int i = Parent.Controls.GetChildIndex(this) + 1; i < Parent.Controls.Count; i++) {
					switch (Dock) {
					case DockStyle.Top:
						if (Top == Parent.Controls[i].Bottom)
							return Parent.Controls[i];
						break;
					case DockStyle.Bottom:
						if (Bottom == Parent.Controls[i].Top)
							return Parent.Controls[i];
						break;
					case DockStyle.Left:
						if (Left == Parent.Controls[i].Right)
							return Parent.Controls[i];
						break;
					case DockStyle.Right:
						if (Right == Parent.Controls[i].Left)
							return Parent.Controls[i];
						break;
					}
				}
				return null;
			}
		}

		private int CalculateSplitPosition() {
			if (horizontal) {
				if (Dock == DockStyle.Top)
					return splitter_rectangle_moving.Y - affected.Top;
				else
					return affected.Bottom - splitter_rectangle_moving.Y - splitter_size;
			} else {
				if (Dock == DockStyle.Left)
					return splitter_rectangle_moving.X - affected.Left;
				else
					return affected.Right - splitter_rectangle_moving.X - splitter_size;
			}
		}

		internal override void OnPaintInternal (PaintEventArgs e) {
			e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(this.BackColor), e.ClipRectangle);
		}

		private void LayoutSplitter(object sender, LayoutEventArgs e) {
			affected = AffectedControl;
			if (split_requested != -1) {
				SplitPosition = split_requested;
				split_requested = -1;
			}
		}

		private void ReparentSplitter(object sender, EventArgs e) {
			affected = null;
		}

		#endregion	// Private Properties and Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged
		{
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
		
#endif

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Enter {
			add { base.Enter += value; }
			remove { base.Enter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Leave {
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		static object SplitterMovedEvent = new object ();
		static object SplitterMovingEvent = new object ();

		public event SplitterEventHandler SplitterMoved {
			add { Events.AddHandler (SplitterMovedEvent, value); }
			remove { Events.RemoveHandler (SplitterMovedEvent, value); }
		}

		public event SplitterEventHandler SplitterMoving {
			add { Events.AddHandler (SplitterMovingEvent, value); }
			remove { Events.RemoveHandler (SplitterMovingEvent, value); }
		}
		#endregion	// Events
	}
}
