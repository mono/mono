//
// ToolStripDropDown.cs
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
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace System.Windows.Forms
{
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Designer ("System.Windows.Forms.Design.ToolStripDropDownDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ToolStripDropDown : ToolStrip
	{
		private bool allow_transparency;
		private bool auto_close;
		private bool can_overflow;
		private bool drop_shadow_enabled = true;
		private double opacity = 1D;
		private ToolStripItem owner_item;

		#region Public Constructor
		public ToolStripDropDown () : base ()
		{
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			SetStyle (ControlStyles.ResizeRedraw, true);

			this.auto_close = true;
			is_visible = false;
			this.DefaultDropDownDirection = ToolStripDropDownDirection.Right;
			this.GripStyle = ToolStripGripStyle.Hidden;
			this.is_toplevel = true;
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool AllowItemReorder {
			get { return base.AllowItemReorder; }
			set { base.AllowItemReorder = value; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool AllowTransparency {
			get { return allow_transparency; }
			set {
				if (value == allow_transparency)
					return;

				if ((XplatUI.SupportsTransparency () & TransparencySupport.Set) != 0) {
					allow_transparency = value;

					if (this.IsHandleCreated) {
						if (value) 
							XplatUI.SetWindowTransparency (Handle, Opacity, Color.Empty);
						else
							UpdateStyles (); // Remove the WS_EX_LAYERED style
					}
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override AnchorStyles Anchor {
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		[DefaultValue (true)]
		public bool AutoClose
		{
			get { return this.auto_close; }
			set { this.auto_close = value; }
		}

		[DefaultValue (true)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (false)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool CanOverflow {
			get { return this.can_overflow; }
			set { this.can_overflow = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ContextMenu ContextMenu {
			get { return null; }
			set { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ContextMenuStrip ContextMenuStrip {
			get { return null; }
			set { }
		}

		public override ToolStripDropDownDirection DefaultDropDownDirection {
			get { return base.DefaultDropDownDirection; }
			set { base.DefaultDropDownDirection = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (DockStyle.None)]
		public override DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}
		
		public bool DropShadowEnabled {
			get { return this.drop_shadow_enabled; }
			set {
				if (this.drop_shadow_enabled == value)
					return;
					
				this.drop_shadow_enabled = value;
				UpdateStyles ();	// Re-CreateParams
			}
		}

		public override Font Font {
			get { return base.Font; }
			set { base.Font = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ToolStripGripDisplayStyle GripDisplayStyle {
			get { return ToolStripGripDisplayStyle.Vertical; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Padding GripMargin {
			get { return Padding.Empty; }
			set { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Rectangle GripRectangle {
			get { return Rectangle.Empty; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (ToolStripGripStyle.Hidden)]
		public new ToolStripGripStyle GripStyle {
			get { return base.GripStyle; }
			set { base.GripStyle = value; }
		}

		[Browsable (false)]
		public bool IsAutoGenerated {
			get { return this is ToolStripOverflow; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Point Location {
			get { return base.Location; }
			set { base.Location = value; }
		}

		[DefaultValue (1D)]
		[TypeConverter (typeof (OpacityConverter))]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public double Opacity {
			get { return this.opacity; }
			set {
					if (this.opacity == value)
						return;
						
					this.opacity = value;
					this.allow_transparency = true;
					
					if (this.IsHandleCreated) {
						UpdateStyles ();
						XplatUI.SetWindowTransparency (Handle, opacity, Color.Empty);
					}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ToolStripOverflowButton OverflowButton {
			get { return base.OverflowButton; }
		}

		[Browsable (false)]
		[DefaultValue (null)]
		public ToolStripItem OwnerItem {
			get { return this.owner_item; }
			set { this.owner_item = value; 
				
				if (this.owner_item != null) {
					if (this.owner_item.Owner != null && this.owner_item.Owner.RenderMode != ToolStripRenderMode.ManagerRenderMode)
						this.Renderer = this.owner_item.Owner.Renderer;

					Font = owner_item.Font;
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new Region Region {
			get { return base.Region; }
			set { base.Region = value; }
		}

		[Localizable (true)]
		[AmbientValue (RightToLeft.Inherit)]
		public override RightToLeft RightToLeft {
			get { return base.RightToLeft; }
			set { base.RightToLeft = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new bool Stretch {
			get { return false; }
			set { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new int TabIndex {
			get { return 0; }
			set { }
		}

		[Browsable (false)]
		[DefaultValue (ToolStripTextDirection.Horizontal)]
		public override ToolStripTextDirection TextDirection {
			get { return base.TextDirection; }
			set { base.TextDirection = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool TopLevel {
			get { return GetTopLevel (); }
			set { SetTopLevel (value); }
		}
		
		[Browsable (false)]
		[Localizable (true)]
		[DefaultValue (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool Visible {
			get { return base.Visible; }
			set { base.Visible = value; }
		}
		#endregion

		#region Protected Properties
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;

				cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_CLIPCHILDREN));
				cp.ClassStyle |= (int)XplatUIWin32.ClassStyle.CS_DROPSHADOW;
				cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);

				if (Opacity < 1.0 && allow_transparency)
					cp.ExStyle |= (int)WindowExStyles.WS_EX_LAYERED;
				if (TopMost)
					cp.ExStyle |= (int) WindowExStyles.WS_EX_TOPMOST;

				return cp;
			}
		}

		protected override DockStyle DefaultDock {
			get { return DockStyle.None; }
		}

		protected override Padding DefaultPadding {
			get { return new Padding (1, 2, 1, 2); }
		}

		protected override bool DefaultShowItemToolTips {
			get { return true; }
		}

		protected internal override Size MaxItemSize {
			get { return new Size (Screen.PrimaryScreen.Bounds.Width - 2, Screen.PrimaryScreen.Bounds.Height - 34); }
		}

		protected virtual bool TopMost {
			get { return true; }
		}
		#endregion

		#region Public Methods
		public void Close ()
		{
			this.Close (ToolStripDropDownCloseReason.CloseCalled);
		}

		public void Close (ToolStripDropDownCloseReason reason)
		{
			if (!this.Visible)
				return;
				
			// Give users a chance to cancel the close
			ToolStripDropDownClosingEventArgs e = new ToolStripDropDownClosingEventArgs (reason);
			this.OnClosing (e);

			if (e.Cancel)
				return;

			// Don't actually close if AutoClose == true unless explicitly called
			if (!this.auto_close && reason != ToolStripDropDownCloseReason.CloseCalled)
				return;

			// Detach from the tracker
			ToolStripManager.AppClicked -= new EventHandler (ToolStripMenuTracker_AppClicked); ;
			ToolStripManager.AppFocusChange -= new EventHandler (ToolStripMenuTracker_AppFocusChange);

			// Hide this dropdown
			this.Hide ();

			// Owner MenuItem needs to be told to redraw (it's no longer selected)
			if (owner_item != null)
				owner_item.Invalidate ();

			// Recursive hide all child dropdowns
			foreach (ToolStripItem tsi in this.Items)
				tsi.Dismiss (reason);
			
			this.OnClosed (new ToolStripDropDownClosedEventArgs (reason));
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new void Show ()
		{
			Show (Location, DefaultDropDownDirection);
		}
		
		public void Show (Point screenLocation)
		{
			Show (screenLocation, DefaultDropDownDirection);
		}
		
		public void Show (Control control, Point position)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
			
			XplatUI.SetOwner (Handle, control.Handle);
			Show (control.PointToScreen (position), DefaultDropDownDirection);
		}
		
		public void Show (int x, int y)
		{
			Show (new Point (x, y), DefaultDropDownDirection);
		}
		
		public void Show (Point position, ToolStripDropDownDirection direction)
		{
			this.PerformLayout ();
			
			Point show_point = position;
			Point max_screen = new Point (SystemInformation.VirtualScreen.Width, SystemInformation.VirtualScreen.Height);
			
			if (this is ContextMenuStrip) {
				// If we are going to go offscreen, adjust our direction so we don't...
				// X direction
				switch (direction) {
					case ToolStripDropDownDirection.AboveLeft:
						if (show_point.X - this.Width < 0)
							direction = ToolStripDropDownDirection.AboveRight;
						break;
					case ToolStripDropDownDirection.BelowLeft:
						if (show_point.X - this.Width < 0)
							direction = ToolStripDropDownDirection.BelowRight;
						break;
					case ToolStripDropDownDirection.Left:
						if (show_point.X - this.Width < 0)
							direction = ToolStripDropDownDirection.Right;
						break;
					case ToolStripDropDownDirection.AboveRight:
						if (show_point.X + this.Width > max_screen.X)
							direction = ToolStripDropDownDirection.AboveLeft;
						break;
					case ToolStripDropDownDirection.BelowRight:
					case ToolStripDropDownDirection.Default:
						if (show_point.X + this.Width > max_screen.X)
							direction = ToolStripDropDownDirection.BelowLeft;
						break;
					case ToolStripDropDownDirection.Right:
						if (show_point.X + this.Width > max_screen.X)
							direction = ToolStripDropDownDirection.Left;
						break;
				}

				// Y direction
				switch (direction) {
					case ToolStripDropDownDirection.AboveLeft:
						if (show_point.Y - this.Height < 0)
							direction = ToolStripDropDownDirection.BelowLeft;
						break;
					case ToolStripDropDownDirection.AboveRight:
						if (show_point.Y - this.Height < 0)
							direction = ToolStripDropDownDirection.BelowRight;
						break;
					case ToolStripDropDownDirection.BelowLeft:
						if (show_point.Y + this.Height > max_screen.Y && show_point.Y - this.Height > 0)
							direction = ToolStripDropDownDirection.AboveLeft;
						break;
					case ToolStripDropDownDirection.BelowRight:
					case ToolStripDropDownDirection.Default:
						if (show_point.Y + this.Height > max_screen.Y && show_point.Y - this.Height > 0)
							direction = ToolStripDropDownDirection.AboveRight;
						break;
					case ToolStripDropDownDirection.Left:
						if (show_point.Y + this.Height > max_screen.Y && show_point.Y - this.Height > 0)
							direction = ToolStripDropDownDirection.AboveLeft;
						break;
					case ToolStripDropDownDirection.Right:
						if (show_point.Y + this.Height > max_screen.Y && show_point.Y - this.Height > 0)
							direction = ToolStripDropDownDirection.AboveRight;
						break;
				}
			}
		
			switch (direction) {
				case ToolStripDropDownDirection.AboveLeft:
					show_point.Y -= this.Height;
					show_point.X -= this.Width;
					break;
				case ToolStripDropDownDirection.AboveRight:
					show_point.Y -= this.Height;
					break;
				case ToolStripDropDownDirection.BelowLeft:
					show_point.X -= this.Width;
					break;
				case ToolStripDropDownDirection.Left:
					show_point.X -= this.Width;
					break;
				case ToolStripDropDownDirection.Right:
					break;
			}

			// Fix offscreen horizontal positions
			if ((show_point.X + this.Width) > max_screen.X)
				show_point.X = max_screen.X - this.Width;
			if (show_point.X < 0)
				show_point.X = 0;

			// Fix offscreen vertical positions
			if ((show_point.Y + this.Height) > max_screen.Y)
				show_point.Y = max_screen.Y - this.Height;
			if (show_point.Y < 0)
				show_point.Y = 0;

			if (this.Location != show_point)
				this.Location = show_point;

			CancelEventArgs e = new CancelEventArgs ();
			this.OnOpening (e);

			if (e.Cancel)
				return;

			// The tracker lets us know when the form is clicked or loses focus
			ToolStripManager.AppClicked += new EventHandler (ToolStripMenuTracker_AppClicked);
			ToolStripManager.AppFocusChange += new EventHandler (ToolStripMenuTracker_AppFocusChange);

			base.Show ();

			ToolStripManager.SetActiveToolStrip (this, ToolStripManager.ActivatedByKeyboard);

			this.OnOpened (EventArgs.Empty);
		}
		
		public void Show (Control control, int x, int y)
		{
			if (control == null)
				throw new ArgumentNullException ("control");

			Show (control, new Point (x, y));
		}
		
		public void Show (Control control, Point position, ToolStripDropDownDirection direction)
		{
			if (control == null)
				throw new ArgumentNullException ("control");

			XplatUI.SetOwner (Handle, control.Handle);
			Show (control.PointToScreen (position), direction);
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new ToolStripDropDownAccessibleObject (this);
		}
		
		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}

		protected override LayoutSettings CreateLayoutSettings (ToolStripLayoutStyle style)
		{
			return base.CreateLayoutSettings (style);
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected virtual void OnClosed (ToolStripDropDownClosedEventArgs e)
		{
			ToolStripDropDownClosedEventHandler eh = (ToolStripDropDownClosedEventHandler)(Events [ClosedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnClosing (ToolStripDropDownClosingEventArgs e)
		{
			ToolStripDropDownClosingEventHandler eh = (ToolStripDropDownClosingEventHandler)(Events [ClosingEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);

			if (Application.MWFThread.Current.Context != null && Application.MWFThread.Current.Context.MainForm != null)
				XplatUI.SetOwner (this.Handle, Application.MWFThread.Current.Context.MainForm.Handle);
		}

		protected override void OnItemClicked (ToolStripItemClickedEventArgs e)
		{
			base.OnItemClicked (e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			// Find the widest menu item, so we know how wide to make our dropdown
			int widest = 0;

			foreach (ToolStripItem tsi in this.Items) {
				if (!tsi.Available) 
					continue;
					
				tsi.SetPlacement (ToolStripItemPlacement.Main);
				
				widest = Math.Max (widest, tsi.GetPreferredSize (Size.Empty).Width + tsi.Margin.Horizontal);
			}
			
			// Add any padding our dropdown has set
			widest += this.Padding.Horizontal;
			
			int x = this.Padding.Left;
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

				tsi.SetBounds (new Rectangle (x, y, preferred_size.Width, height));
				y += height + tsi.Margin.Bottom;
			}

			this.Size = new Size (widest, y + this.Padding.Bottom);
			this.SetDisplayedItems ();
			this.OnLayoutCompleted (EventArgs.Empty);
			this.Invalidate ();
		}

		protected override void OnMouseUp (MouseEventArgs mea)
		{
			base.OnMouseUp (mea);
		}

		protected virtual void OnOpened (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [OpenedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnOpening (CancelEventArgs e)
		{
			CancelEventHandler eh = (CancelEventHandler)(Events [OpeningEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
			
			if (Parent is ToolStrip)
				this.Renderer = (Parent as ToolStrip).Renderer;
		}

		protected override void OnVisibleChanged (EventArgs e)
		{
			base.OnVisibleChanged (e);

			if (owner_item != null && owner_item is ToolStripDropDownItem) {
				ToolStripDropDownItem dropdown_owner = (ToolStripDropDownItem)owner_item;
				if (Visible)
					dropdown_owner.OnDropDownOpened (EventArgs.Empty);
				else
					dropdown_owner.OnDropDownClosed (EventArgs.Empty);
			}
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override bool ProcessDialogChar (char charCode)
		{
			return base.ProcessDialogChar (charCode);
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			// We don't want to let our base change the active ToolStrip
			switch (keyData) {
				case Keys.Control | Keys.Tab:
				case Keys.Control | Keys.Shift | Keys.Tab:
					return true;
			}
			
			return base.ProcessDialogKey (keyData);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			return base.ProcessMnemonic (charCode);
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore (float dx, float dy)
		{
			base.ScaleCore (dx, dy);
		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void SetVisibleCore (bool visible)
		{
			base.SetVisibleCore (visible);
		}

		protected override void WndProc (ref Message m)
		{
			const int MA_NOACTIVATE = 0x0003;

			// Don't activate when the WM tells us to
			if ((Msg)m.Msg == Msg.WM_MOUSEACTIVATE) {
				m.Result = (IntPtr)MA_NOACTIVATE;
				return;
			}

			base.WndProc (ref m);
		}
		#endregion

		#region Public Events
		static object ClosedEvent = new object ();
		static object ClosingEvent = new object ();
		static object OpenedEvent = new object ();
		static object OpeningEvent = new object ();
		static object ScrollEvent = new object ();

		[Browsable (false)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}

		[Browsable (false)]
		public new event EventHandler BindingContextChanged {
			add { base.BindingContextChanged += value; }
			remove { base.BindingContextChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event UICuesEventHandler ChangeUICues {
			add { base.ChangeUICues += value; }
			remove { base.ChangeUICues -= value; }
		}

		public event ToolStripDropDownClosedEventHandler Closed {
			add { Events.AddHandler (ClosedEvent, value); }
			remove { Events.RemoveHandler (ClosedEvent, value); }
		}

		public event ToolStripDropDownClosingEventHandler Closing {
			add { Events.AddHandler (ClosingEvent, value); }
			remove { Events.RemoveHandler (ClosingEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ContextMenuChanged {
			add { base.ContextMenuChanged += value; }
			remove { base.ContextMenuChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler ContextMenuStripChanged {
			add { base.ContextMenuStripChanged += value; }
			remove { base.ContextMenuStripChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler DockChanged {
			add { base.DockChanged += value; }
			remove { base.DockChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler Enter {
			add { base.Enter += value; }
			remove { base.Enter -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event GiveFeedbackEventHandler GiveFeedback {
			add { base.GiveFeedback += value; }
			remove { base.GiveFeedback -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event HelpEventHandler HelpRequested {
			add { base.HelpRequested += value; }
			remove { base.HelpRequested -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler Leave {
			add { base.Leave += value; }
			remove { base.Leave -= value; }
		}

		public event EventHandler Opened {
			add { Events.AddHandler (OpenedEvent, value); }
			remove { Events.RemoveHandler (OpenedEvent, value); }
		}

		public event CancelEventHandler Opening {
			add { Events.AddHandler (OpeningEvent, value); }
			remove { Events.RemoveHandler (OpeningEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler RegionChanged {
			add { base.RegionChanged += value; }
			remove { base.RegionChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event ScrollEventHandler Scroll {
			add { Events.AddHandler (ScrollEvent, value); }
			remove { Events.RemoveHandler (ScrollEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler StyleChanged {
			add { base.StyleChanged += value; }
			remove { base.StyleChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Validated {
			add { base.Validated += value; }
			remove { base.Validated -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event CancelEventHandler Validating {
			add { base.Validating += value; }
			remove { base.Validating -= value; }
		}
		#endregion

		#region Private Methods
		internal override void Dismiss (ToolStripDropDownCloseReason reason)
		{
			this.Close (reason);
			base.Dismiss (reason);
		}

		internal override ToolStrip GetTopLevelToolStrip ()
		{
			if (this.OwnerItem == null)
				return this;
				
			return this.OwnerItem.GetTopLevelToolStrip ();
		}

		internal override bool ProcessArrowKey (Keys keyData)
		{
			switch (keyData) {
				case Keys.Down:
				case Keys.Tab:
					this.SelectNextToolStripItem (this.GetCurrentlySelectedItem (), true);
					return true;
				case Keys.Up:
				case Keys.Shift | Keys.Tab:
					this.SelectNextToolStripItem (this.GetCurrentlySelectedItem (), false);
					return true;
				case Keys.Right:
					this.GetTopLevelToolStrip ().SelectNextToolStripItem (this.TopLevelOwnerItem, true);
					return true;
				case Keys.Left:
				case Keys.Escape:
					this.Dismiss (ToolStripDropDownCloseReason.Keyboard);
					
					// ContextMenuStrip won't have a parent
					if (this.OwnerItem == null)
						return true;
						
					ToolStrip parent_strip = this.OwnerItem.Parent;
					ToolStripManager.SetActiveToolStrip (parent_strip, true);
					
					if (parent_strip is MenuStrip && keyData == Keys.Left) {
						parent_strip.SelectNextToolStripItem (this.TopLevelOwnerItem, false);
						this.TopLevelOwnerItem.Invalidate ();
					} else if (parent_strip is MenuStrip && keyData == Keys.Escape) {
						(parent_strip as MenuStrip).MenuDroppedDown = false;
						this.TopLevelOwnerItem.Select ();
					}				
					return true;
			}
			
			return false;
		}

		internal override ToolStripItem SelectNextToolStripItem (ToolStripItem start, bool forward)
		{
			ToolStripItem next_item = this.GetNextItem (start, forward ? ArrowDirection.Down : ArrowDirection.Up);

			if (next_item != null)
				this.ChangeSelection (next_item);
				
			return (next_item);
		}
		
		private void ToolStripMenuTracker_AppFocusChange (object sender, EventArgs e)
		{
			this.GetTopLevelToolStrip ().Dismiss (ToolStripDropDownCloseReason.AppFocusChange);
		}

		private void ToolStripMenuTracker_AppClicked (object sender, EventArgs e)
		{
			this.GetTopLevelToolStrip ().Dismiss (ToolStripDropDownCloseReason.AppClicked);
		}
		#endregion

		#region Internal Properties
		internal override bool ActivateOnShow { get { return false; } }
		
		internal ToolStripItem TopLevelOwnerItem {
			get {
				ToolStripItem owner_item = this.OwnerItem;
				ToolStrip ts = null;

				while (owner_item != null) {
					ts = owner_item.Owner;

					if (ts != null && (ts is ToolStripDropDown))
						owner_item = (ts as ToolStripDropDown).OwnerItem;
					else
						return owner_item;
				}

				return null;
			}
		}
		#endregion

		#region ToolStripDropDownAccessibleObject
		[ComVisible (true)]
		public class ToolStripDropDownAccessibleObject : ToolStripAccessibleObject
		{
			#region Public Constructor
			public ToolStripDropDownAccessibleObject (ToolStripDropDown owner) : base (owner)
			{
			}
			#endregion
			
			#region Public Properties
			public override string Name {
				get { return base.Name; }
				set { base.Name = value; }
			}

			public override AccessibleRole Role {
				get { return AccessibleRole.MenuPopup; }
			}
			#endregion
		}
		#endregion
	}
}
