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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Dennis Bartok	(pbartok@novell.com)
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
	[DefaultEvent("SplitterMoved")]
	[Designer("System.Windows.Forms.Design.SplitterDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultProperty("Dock")]
	public class Splitter : Control, IMessageFilter {
		#region Enums
		private enum DrawType {
			Initial,
			Redraw,
			Finish
		}
		#endregion	// Enums

		#region Local Variables
		static private Cursor		splitter_ns;
		static private Cursor		splitter_we;
		private BorderStyle		border_style;
		private int			min_extra;
		private int			min_size;
		private int			split_position;		// Current splitter position
		private int			prev_split_position;	// Previous splitter position, only valid during drag
		private int			click_offset;		// Click offset from border of splitter control
		private int			splitter_size;		// Size (width or height) of our splitter control
		private bool			horizontal;		// true if we've got a horizontal splitter
		private Control			affected;		// The control that the splitter resizes
		private Control			filler;			// The control that MinExtra prevents from being shrunk to 0 size
		private SplitterEventArgs	sevent;			// We cache the object, prevents fragmentation
		private int			limit_min;		// The max we're allowed to move the splitter left/up
		private int			limit_max;		// The max we're allowed to move the splitter right/down
		#endregion	// Local Variables

		#region Constructors
		static Splitter() {
			try {
				splitter_ns = new Cursor(typeof(Splitter), "SpliterNS.cur");
			}

			catch (System.IO.FileNotFoundException) {
				splitter_ns = Cursors.SizeNS;
			}

			try {
				splitter_we = new Cursor(typeof(Splitter), "SplitterWE.cur");
			}

			catch (System.IO.FileNotFoundException) {
				splitter_we = Cursors.SizeWE;
			}
		}

		public Splitter() {

			min_extra = 25;
			min_size = 25;
			split_position = -1;
			splitter_size = 3;
			horizontal = false;
			sevent = new SplitterEventArgs(0, 0, 0, 0);

			SetStyle(ControlStyles.Selectable, false);
			Anchor = AnchorStyles.None;

			Paint += new PaintEventHandler(PaintSplitter);
			Layout += new LayoutEventHandler(LayoutSplitter);
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
					case BorderStyle.FixedSingle: {
						splitter_size = 4;	// We don't get motion events for 1px wide windows on X11. sigh.
						break;
					}

					case BorderStyle.Fixed3D: {
						value = BorderStyle.None;
						splitter_size = 3;
						break;
					}

					case BorderStyle.None: {
						splitter_size = 3;
						break;
					}

					default: {
						throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for BorderStyle", value));
					}
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

		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[MWFDescription("Current splitter position")]
		[MWFCategory("Layout")]
		public int SplitPosition {
			get {
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
				if (Capture || (affected == null)) {
					return;
				}

				if (horizontal) {
					affected.Height = value;
				} else {
					affected.Width = value;
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool TabStop {
			get {
				return base.TabStop;
			}

			set {
				base.TabStop = value;
			}
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
		public bool PreFilterMessage(ref Message m) {
			return false;
		}

		public override string ToString() {
			return base.ToString () + String.Format(", MinExtra: {0}, MinSize: {1}", min_extra, min_size);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void OnKeyDown(KeyEventArgs e) {
			base.OnKeyDown (e);
			if (Capture && (e.KeyCode == Keys.Escape)) {
				Capture = false;
				DrawDragHandle(DrawType.Finish);
			}
		}

		protected override void OnMouseDown(MouseEventArgs e) {
			Point	pt;

			base.OnMouseDown (e);

			// Only allow if we are set up properly
			if (affected == null || e.Button != MouseButtons.Left) {
				return;
			}

			// Prepare the job
			Capture = true;

			// Calculate limits
			if (filler != null) {
				if (horizontal) {
					if (dock_style == DockStyle.Top) {
						limit_min = affected.Bounds.Top + min_size;
						limit_max = filler.Bounds.Bottom - min_extra + this.bounds.Top - filler.Bounds.Top;
					} else {
						limit_min = filler.Bounds.Top + min_extra + this.bounds.Top - filler.Bounds.Bottom;
						limit_max = affected.Bounds.Bottom - min_size - this.Height;
					}
				} else {
					if (dock_style == DockStyle.Left) {
						limit_min = affected.Bounds.Left + min_size;
						limit_max = filler.Bounds.Right - min_extra + this.bounds.Left - filler.Bounds.Left;
					} else {
						limit_min = filler.Bounds.Left + min_extra + this.bounds.Left - filler.Bounds.Right;
						limit_max = affected.Bounds.Right - min_size - this.Width;
					}
				}
			} else {
				limit_min = 0;
				if (horizontal) {
					limit_max = affected.Parent.Height;
				} else {
					limit_max = affected.Parent.Width;
				}
			}

			#if Debug
				Console.WriteLine("Sizing limits: Min:{0}, Max:{1}", limit_min, limit_max);
			#endif

			pt = PointToScreen(parent.PointToClient(new Point(e.X, e.Y)));

			if (horizontal) {
				split_position = pt.Y;
				if (dock_style == DockStyle.Top) {
					click_offset = e.Y;
				} else {
					click_offset = -e.Y;
				}
			} else {
				split_position = pt.X;
				if (dock_style == DockStyle.Left) {
					click_offset = e.X;
				} else {
					click_offset = -e.X;
				}
			}

			// We need to set this, in case we never get a mouse move
			prev_split_position = split_position;

			#if Debug
				Console.WriteLine("Click-offset: {0} MouseDown split position: {1}", click_offset, split_position);
			#endif

			// Draw our initial handle
			DrawDragHandle(DrawType.Initial);
		}

		protected override void OnMouseMove(MouseEventArgs e) {
			Point	pt;

			base.OnMouseMove (e);

			if (!Capture  || e.Button != MouseButtons.Left) {
				return;
			}

			// We need our mouse coordinates relative to our parent
			pt = PointToScreen(parent.PointToClient(new Point(e.X, e.Y)));

			// Grab our new coordinates
			prev_split_position = split_position;
			if (horizontal) {
				split_position = pt.Y;
			} else {
				split_position = pt.X;
			}
			// Enforce limits
			if (split_position < limit_min) {
				#if Debug
					Console.WriteLine("SplitPosition {0} less than minimum {1}, setting to minimum", split_position, limit_min);
				#endif
				split_position = limit_min;
			} else if (split_position > limit_max) {
				#if Debug
					Console.WriteLine("SplitPosition {0} more than maximum {1}, setting to maximum", split_position, limit_max);
				#endif
				split_position = limit_max;
			}

			// Don't waste cycles
			if (prev_split_position != split_position) {
				// Update our handle location
				DrawDragHandle(DrawType.Redraw);
			}

			// Prepare the event
			if (horizontal) {
				sevent.split_x = 0;
				sevent.split_y = split_position;
			} else {
				sevent.split_x = split_position;
				sevent.split_y = 0;
			}

			sevent.x = pt.X;
			sevent.y = pt.Y;

			// Fire the event
			OnSplitterMoving(sevent);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			if (!Capture || e.Button != MouseButtons.Left) {
				base.OnMouseUp (e);
				return;
			}

			Capture = false;
			DrawDragHandle(DrawType.Finish);

			// Resize the affected window
			if (horizontal) {
				affected.Height = CalculateSplitPosition() - click_offset;
				#if Debug
					Console.WriteLine("Setting height of affected control to {0}", CalculateSplitPosition() - click_offset);
				#endif
			} else {
				affected.Width = CalculateSplitPosition() - click_offset;
				#if Debug
					Console.WriteLine("Setting width of affected control to {0}", CalculateSplitPosition() - click_offset);
				#endif
			}

			base.OnMouseUp (e);

			// It seems that MS is sending some data that doesn't quite make sense
			// In this event. It tried to match their stuff.., not sure about split_x...

			// Prepare the event
			if (horizontal) {
				sevent.x = 0;
				sevent.y = split_position;
				sevent.split_x = 200;
				sevent.split_y = split_position;
			} else {
				sevent.x = split_position;
				sevent.y = 0;
				sevent.split_x = split_position;
				sevent.split_y = 200;
			}


			// Fire the event
			OnSplitterMoved(sevent);
		}

		protected virtual void OnSplitterMoved(SplitterEventArgs sevent) {
			if (SplitterMoved != null) {
				SplitterMoved(this, sevent);
			}
		}

		protected virtual void OnSplitterMoving(SplitterEventArgs sevent) {
			if (SplitterMoving != null) {
				SplitterMoving(this, sevent);
			}
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// enforce our width / height
			if (horizontal) {
				base.SetBoundsCore (x, y, width, splitter_size, specified);
			} else {
				base.SetBoundsCore (x, y, splitter_size, height, specified);
			}
		}
		#endregion	// Protected Instance Methods

		#region Private Properties and Methods
		private Control AffectedControl {
			get {
				if (parent == null) {
					return null;
				}

				// Doc says the first control preceeding us in the zorder 
				for (int i = parent.Controls.GetChildIndex(this) + 1; i < parent.Controls.Count; i++) {
					switch(this.Dock) {
						case DockStyle.Top: {
							if (Top == parent.Controls[i].Bottom) {
								return parent.Controls[i];
							}
							break;
						}

						case DockStyle.Bottom: {
							if (Bottom == parent.Controls[i].Top) {
								return parent.Controls[i];
							}
							break;
						}

						case DockStyle.Left: {
							if (Left == parent.Controls[i].Right) {
								return parent.Controls[i];
							}
							break;
						}

						case DockStyle.Right: {
							if (Right == parent.Controls[i].Left) {
								return parent.Controls[i];
							}
							break;
						}
					}
				}
				return null;
			}
		}

		private Control FillerControl {
			get {
				if (parent == null) {
					return null;
				}

				// Doc says the first control preceeding us in the zorder 
				for (int i = parent.Controls.GetChildIndex(this) - 1; i >= 0; i--) {
					if (parent.Controls[i].Dock == DockStyle.Fill) {
						return parent.Controls[i];
					}
				}
				return null;
			}
		}

		private int CalculateSplitPosition() {
			if (horizontal) {
				if (dock_style == DockStyle.Top) {
					return split_position;
				} else {
					return affected.Bottom - split_position - splitter_size;
				}
			} else {
				if (dock_style == DockStyle.Left) {
					return split_position;
				} else {
					return affected.Right - split_position - splitter_size;
				}
			}
		}

		private void PaintSplitter(object sender, PaintEventArgs e) {
			e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(this.BackColor), e.ClipRectangle);
		}

		private void LayoutSplitter(object sender, LayoutEventArgs e) {
			affected = AffectedControl;
			filler = FillerControl;
		}

		private void DrawDragHandle(DrawType type) {
			Rectangle	prev;
			Rectangle	current;

			if (horizontal) {
				prev = new Rectangle(0, prev_split_position - click_offset + 1, Width, 0);
				current = new Rectangle(0, split_position - click_offset + 1, Width, 0);
			} else {
				prev = new Rectangle(prev_split_position - click_offset + 1, 0, 0, Height);
				current = new Rectangle(split_position - click_offset + 1, 0, 0, Height);
			}

			switch(type) {
				case DrawType.Initial: {
					XplatUI.DrawReversibleRectangle(Parent.window.Handle, current, 3);
					return;
				}

				case DrawType.Redraw: {
					if (prev.X == current.X && prev.Y == current.Y) {
						return;
					}

					XplatUI.DrawReversibleRectangle(Parent.window.Handle, prev, 3);
					XplatUI.DrawReversibleRectangle(Parent.window.Handle, current, 3);
					return;
				}

				case DrawType.Finish: {
					XplatUI.DrawReversibleRectangle(Parent.window.Handle, prev, 3);
					return;
				}
			}
		}
		#endregion	// Private Properties and Methods

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Enter;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler FontChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler Leave;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged;

		public event SplitterEventHandler SplitterMoved;
		public event SplitterEventHandler SplitterMoving;
		#endregion	// Events
	}
}
