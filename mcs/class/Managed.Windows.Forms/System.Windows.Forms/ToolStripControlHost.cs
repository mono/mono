//
// ToolStripControlHost.cs
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
	public class ToolStripControlHost : ToolStripItem
	{
		private Control control;
		private ContentAlignment control_align;
		private bool double_click_enabled;
		private ContentAlignment image_align;
		private ToolStripItemImageScaling image_scaling;
		private Color image_transparent_color;
		private ContentAlignment text_align;
		private TextImageRelation text_image_relation;

		#region Public Constructors
		public ToolStripControlHost (Control c) : base ()
		{
			if (c == null)
				throw new ArgumentNullException ("c");

			this.control = c;
			this.control_align = ContentAlignment.MiddleCenter;
			this.OnSubscribeControlEvents (this.control);
		}

		public ToolStripControlHost (Control c, string name) : this (c)
		{
			this.control.name = name;
		}
		#endregion

		#region Public Properties
		public override Color BackColor {
			get { return base.BackColor; }
			set { 
				base.BackColor = value;
				control.BackColor = value;
			}
		}

		public override bool CanSelect {
			get { return control.CanSelect; }
		}

		[Browsable (false)]
		public Control Control {
			get { return this.control; }
		}

		[Browsable (false)]
		[DefaultValue (ContentAlignment.MiddleCenter)]
		public ContentAlignment ControlAlign {
			get { return this.control_align; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ContentAlignment", value));

				this.control_align = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ToolStripItemDisplayStyle DisplayStyle {
			get { return base.DisplayStyle; }
			set { base.DisplayStyle = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DefaultValue (false)]
		public new bool DoubleClickEnabled {
			get { return this.double_click_enabled; }
			set { this.double_click_enabled = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool Enabled {
			get { return base.Enabled; }
			set {
				base.Enabled = value;
				control.Enabled = value;
			}
		}

		public virtual bool Focused {
			get { return control.Focused; }
		}

		public override Font Font {
			get { return base.Font; }
			set {
				base.Font = value;
				control.Font = value;
			}
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set { 
				base.ForeColor = value;
				control.ForeColor = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Image Image {
			get { return base.Image; }
			set { base.Image = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ContentAlignment ImageAlign {
			get { return this.image_align; }
			set { this.image_align = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ToolStripItemImageScaling ImageScaling {
			get { return this.image_scaling; }
			set { this.image_scaling = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new Color ImageTransparentColor {
			get { return this.image_transparent_color; }
			set { this.image_transparent_color = value; }
		}

		public override bool Selected {
			get { return base.Selected; }
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override ISite Site {
			get { return base.Site; }
			set { 
				base.Site = value;
				control.Site = value;
			}
		}

		[DefaultValue ("")]
		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				control.Text = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new ContentAlignment TextAlign {
			get { return this.text_align; }
			set { this.text_align = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new TextImageRelation TextImageRelation {
			get { return this.text_image_relation; }
			set { this.text_image_relation = value; }
		}
		#endregion

		#region Protected Properties
		protected override Size DefaultSize {
			get {
				if (control == null)
					return new Size (23, 23);

				return control.GetPreferredSize (Size.Empty);
			}
		}
		#endregion

		#region Public Methods
		public void Focus ()
		{
			control.Focus ();
		}

		public override Size GetPreferredSize (Size constrainingSize)
		{
			return this.Size;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void ResetBackColor ()
		{
			base.ResetBackColor ();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void ResetForeColor ()
		{
			base.ResetForeColor ();
		}
		#endregion

		#region Protected Methods
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return this.Control.AccessibilityObject;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			if (!control.IsDisposed)
				control.Dispose ();
		}
		
		protected override void OnBoundsChanged ()
		{
			base.OnBoundsChanged ();

			if (this.Parent != null) {
				control.Size = this.Size;
				OnLayout (new LayoutEventArgs (null, string.Empty));
			}
		}
		
		protected virtual void OnEnter (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [EnterEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnGotFocus (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [GotFocusEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnHostedControlResize (EventArgs e)
		{
		}
		
		protected virtual void OnKeyDown (KeyEventArgs e)
		{
			KeyEventHandler eh = (KeyEventHandler)(Events [KeyDownEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnKeyPress (KeyPressEventArgs e)
		{
			KeyPressEventHandler eh = (KeyPressEventHandler)(Events [KeyPressEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnKeyUp (KeyEventArgs e)
		{
			KeyEventHandler eh = (KeyEventHandler)(Events [KeyUpEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout (e);
			
			if (control != null)
				control.Bounds = AlignInRectangle (this.Bounds, control.Size, this.control_align);
		}
		
		protected virtual void OnLeave (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [LeaveEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnLostFocus (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [LostFocusEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			base.OnPaint (e);
		}
		
		protected override void OnParentChanged (ToolStrip oldParent, ToolStrip newParent)
		{
			base.OnParentChanged (oldParent, newParent);

			if (oldParent != null)
				oldParent.Controls.Remove (control);

			if (newParent != null)
				newParent.Controls.Add (control);
		}
		
		protected virtual void OnSubscribeControlEvents (Control control)
		{
			this.control.Enter += new EventHandler (HandleEnter);
			this.control.GotFocus += new EventHandler (HandleGotFocus);
			this.control.KeyDown += new KeyEventHandler (HandleKeyDown);
			this.control.KeyPress += new KeyPressEventHandler (HandleKeyPress);
			this.control.KeyUp += new KeyEventHandler (HandleKeyUp);
			this.control.Leave += new EventHandler (HandleLeave);
			this.control.LostFocus += new EventHandler (HandleLostFocus);
			this.control.Validated += new EventHandler (HandleValidated);
			this.control.Validating += new CancelEventHandler (HandleValidating);
		}
		
		protected virtual void OnUnsubscribeControlEvents (Control control)
		{
		}
		
		protected virtual void OnValidated (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ValidatedEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		protected virtual void OnValidating (CancelEventArgs e)
		{
			CancelEventHandler eh = (CancelEventHandler)(Events [ValidatingEvent]);
			if (eh != null)
				eh (this, e);
		}
		#endregion

		#region Public Events
		static object EnterEvent = new object ();
		static object GotFocusEvent = new object ();
		static object KeyDownEvent = new object ();
		static object KeyPressEvent = new object ();
		static object KeyUpEvent = new object ();
		static object LeaveEvent = new object ();
		static object LostFocusEvent = new object ();
		static object ValidatedEvent = new object ();
		static object ValidatingEvent = new object ();

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DisplayStyleChanged {
			add { base.DisplayStyleChanged += value; }
			remove { base.DisplayStyleChanged -= value; }
		}

		public event EventHandler Enter {
			add { Events.AddHandler (EnterEvent, value); }
			remove { Events.RemoveHandler (EnterEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler GotFocus {
			add { Events.AddHandler (GotFocusEvent, value); }
			remove { Events.RemoveHandler (GotFocusEvent, value); }
		}

		public event KeyEventHandler KeyDown {
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}

		public event KeyPressEventHandler KeyPress {
			add { Events.AddHandler (KeyPressEvent, value); }
			remove { Events.RemoveHandler (KeyPressEvent, value); }
		}

		public event KeyEventHandler KeyUp {
			add { Events.AddHandler (KeyUpEvent, value); }
			remove { Events.RemoveHandler (KeyUpEvent, value); }
		}

		public event EventHandler Leave {
			add { Events.AddHandler (LeaveEvent, value); }
			remove { Events.RemoveHandler (LeaveEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler LostFocus {
			add { Events.AddHandler (LostFocusEvent, value); }
			remove { Events.RemoveHandler (LostFocusEvent, value); }
		}

		public event EventHandler Validated {
			add { Events.AddHandler (ValidatedEvent, value); }
			remove { Events.RemoveHandler (ValidatedEvent, value); }
		}

		public event CancelEventHandler Validating {
			add { Events.AddHandler (ValidatingEvent, value); }
			remove { Events.RemoveHandler (ValidatingEvent, value); }
		}
		#endregion

		#region Private Methods
		private void HandleEnter (object sender, EventArgs e)
		{
			this.OnEnter (e);
		}

		private void HandleGotFocus (object sender, EventArgs e)
		{
			this.OnGotFocus (e);
		}

		private void HandleKeyDown (object sender, KeyEventArgs e)
		{
			this.OnKeyDown (e);
		}

		private void HandleKeyPress (object sender, KeyPressEventArgs e)
		{
			this.OnKeyPress (e);
		}

		private void HandleKeyUp (object sender, KeyEventArgs e)
		{
			this.OnKeyUp (e);
		}

		private void HandleLeave (object sender, EventArgs e)
		{
			this.OnLeave (e);
		}

		private void HandleLostFocus (object sender, EventArgs e)
		{
			this.OnLostFocus (e);
		}

		private void HandleValidated (object sender, EventArgs e)
		{
			this.OnValidated (e);
		}
		
		private void HandleValidating (object sender, CancelEventArgs e)
		{
			this.OnValidating (e);
		}
		#endregion
	}
}
#endif
