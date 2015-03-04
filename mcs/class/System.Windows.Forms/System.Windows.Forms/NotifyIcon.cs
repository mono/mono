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
//	Peter Bartok	pbartok@novell.com
//
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
	[DefaultProperty("Text")]
	[DefaultEvent("MouseDoubleClick")]
	[Designer ("System.Windows.Forms.Design.NotifyIconDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Allow)]
	public sealed class NotifyIcon : Component {
		#region Local Variables
		private ContextMenu		context_menu;
		private Icon			icon;
		private Bitmap			icon_bitmap;
		private string			text;
		private bool			visible;
		private NotifyIconWindow	window;
		private bool			systray_active;
		private ToolTip			tooltip;
		private bool			double_click;
		private string balloon_text;
		private string balloon_title;
		private ToolTipIcon balloon_icon;
		private ContextMenuStrip	context_menu_strip;
		private object			tag;
		#endregion	// Local Variables

		#region NotifyIconWindow Class
		internal class NotifyIconWindow : Form {
			NotifyIcon	owner;
			Rectangle	rect;

			public NotifyIconWindow(NotifyIcon owner) {
				this.owner = owner;
				is_visible = false;
				rect = new Rectangle(0, 0, 1, 1);

				FormBorderStyle = FormBorderStyle.None;

				//CreateControl();

				SizeChanged += new EventHandler(HandleSizeChanged);

				// Events that need to be sent to our parent
				DoubleClick += new EventHandler(HandleDoubleClick);
				MouseDown +=new MouseEventHandler(HandleMouseDown);
				MouseUp +=new MouseEventHandler(HandleMouseUp);
				MouseMove +=new MouseEventHandler(HandleMouseMove);
				ContextMenu = owner.context_menu;
				ContextMenuStrip = owner.context_menu_strip;
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams cp;

					cp = base.CreateParams;

					cp.Parent = IntPtr.Zero;
					cp.Style = (int)WindowStyles.WS_POPUP;
					cp.Style |= (int)WindowStyles.WS_CLIPSIBLINGS;

					cp.ExStyle = (int)(WindowExStyles.WS_EX_TOOLWINDOW);

					return cp;
				}
			}

			protected override void WndProc(ref Message m) {
				switch((Msg)m.Msg) {
						//
						//  NotifyIcon does CONTEXTMENU on mouse up, not down
						//  so we swallow the message here, and handle it on our own
						// 
				        case Msg.WM_CONTEXTMENU:
						return;

					case Msg.WM_USER: {
						switch ((Msg)m.LParam.ToInt32()) {
							case Msg.WM_LBUTTONDOWN: {
								owner.OnMouseDown (new MouseEventArgs(MouseButtons.Left, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_LBUTTONUP: {
								owner.OnMouseUp (new MouseEventArgs(MouseButtons.Left, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_LBUTTONDBLCLK: {
								owner.OnDoubleClick (EventArgs.Empty);
								owner.OnMouseDoubleClick (new MouseEventArgs (MouseButtons.Left, 2, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_MOUSEMOVE: {
								owner.OnMouseMove (new MouseEventArgs(MouseButtons.None, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_RBUTTONDOWN: {
								owner.OnMouseDown (new MouseEventArgs(MouseButtons.Right, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_RBUTTONUP: {
								owner.OnMouseUp (new MouseEventArgs(MouseButtons.Right, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_RBUTTONDBLCLK: {
								owner.OnDoubleClick (EventArgs.Empty);
								owner.OnMouseDoubleClick (new MouseEventArgs (MouseButtons.Left, 2, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.NIN_BALLOONUSERCLICK: {
								owner.OnBalloonTipClicked (EventArgs.Empty);
								return;
							}

							case Msg.NIN_BALLOONSHOW: {
								owner.OnBalloonTipShown (EventArgs.Empty);
								return;
							}

							case Msg.NIN_BALLOONHIDE:
							case Msg.NIN_BALLOONTIMEOUT: {
								owner.OnBalloonTipClosed (EventArgs.Empty);
								return;
							}
						}
						return;
					}
				}
				base.WndProc (ref m);
			}

			internal void CalculateIconRect() {
				int		x;
				int		y;
				int		size;

				// Icons are always square. Try to center them in the window
				if (ClientRectangle.Width < ClientRectangle.Height) {
					size = ClientRectangle.Width;
				} else {
					size = ClientRectangle.Height;
				}
				x = this.ClientRectangle.Width / 2 - size / 2;
				y = this.ClientRectangle.Height / 2 - size / 2;
				rect = new Rectangle(x, y, size, size);

				Bounds = new Rectangle (0, 0, size, size);
			}

			internal override void OnPaintInternal (PaintEventArgs e) {
				if (owner.icon != null) {
					// At least in Gnome, the background of the panel is the same as the Menu, so we go for it
					// instead of (most of the time) plain white.
					e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(SystemColors.Menu), rect);
					e.Graphics.DrawImage(owner.icon_bitmap,
							     rect,
							     new Rectangle (0, 0, owner.icon_bitmap.Width, owner.icon_bitmap.Height),
							     GraphicsUnit.Pixel);

				}
			}

			internal void InternalRecreateHandle () {
				base.RecreateHandle ();
			}

			private void HandleSizeChanged(object sender, EventArgs e) {
				owner.Recalculate ();
			}

			private void HandleDoubleClick (object sender, EventArgs e)
			{
				owner.OnDoubleClick (e);
				owner.OnMouseDoubleClick (new MouseEventArgs (MouseButtons.Left, 2, Control.MousePosition.X, Control.MousePosition.Y, 0));
			}

			private void HandleMouseDown (object sender, MouseEventArgs e)
			{
				owner.OnMouseDown (e);
			}

			private void HandleMouseUp (object sender, MouseEventArgs e)
			{
				owner.OnMouseUp (e);
			}

			private void HandleMouseMove (object sender, MouseEventArgs e)
			{
				owner.OnMouseMove (e);
			}
		}
		#endregion	// NotifyIconWindow Class
		
		#region NotifyIconBalloonWindow Class
		internal class BalloonWindow : Form 
		{
			private IntPtr owner;
			private Timer timer;
			
			private string title;
			private string text;
			private ToolTipIcon icon;

			public BalloonWindow (IntPtr owner)
			{
				this.owner = owner;
				
				StartPosition = FormStartPosition.Manual;
				FormBorderStyle = FormBorderStyle.None;

				MouseDown += new MouseEventHandler (HandleMouseDown);
				
				timer = new Timer ();
				timer.Enabled = false;
				timer.Tick += new EventHandler (HandleTimer);
			}

			public IntPtr OwnerHandle {
				get {
					return owner;
				}
			}
			
			protected override void Dispose (bool disposing)
			{
				if (disposing) {
					timer.Stop();
					timer.Dispose();
				}
				base.Dispose (disposing);
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams cp;

					cp = base.CreateParams;

					cp.Style = (int)WindowStyles.WS_POPUP;
					cp.Style |= (int)WindowStyles.WS_CLIPSIBLINGS;

					cp.ExStyle = (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);

					return cp;
				}
			}

			public new void Close () {
				base.Close ();
				XplatUI.SendMessage (owner, Msg.WM_USER, IntPtr.Zero, (IntPtr) Msg.NIN_BALLOONHIDE);
			}
			
			protected override void OnShown (EventArgs e)
			{
				base.OnShown (e);
				timer.Start ();
			}
			
			protected override void OnPaint (PaintEventArgs e) 
			{
				ThemeEngine.Current.DrawBalloonWindow (e.Graphics, ClientRectangle, this);
				base.OnPaint (e);
			}

			private void Recalculate () 
			{
				Rectangle rect = ThemeEngine.Current.BalloonWindowRect (this);
				
				Left = rect.Left;
				Top = rect.Top;
				Width = rect.Width;
				Height = rect.Height;
			}

			// To be used when we have a "close button" inside balloon.
			//private void HandleClick (object sender, EventArgs e)
			//{
			//	Close ();
			//}

			private void HandleMouseDown (object sender, MouseEventArgs e)
			{
				XplatUI.SendMessage (owner, Msg.WM_USER, IntPtr.Zero, (IntPtr) Msg.NIN_BALLOONUSERCLICK);
				base.Close ();
			}

			private void HandleTimer (object sender, EventArgs e)
			{
				timer.Stop ();
				XplatUI.SendMessage (owner, Msg.WM_USER, IntPtr.Zero, (IntPtr) Msg.NIN_BALLOONTIMEOUT);
				base.Close ();
			}
			
			internal StringFormat Format {
				get {
					StringFormat format = new StringFormat ();
					format.Alignment = StringAlignment.Near;
					format.HotkeyPrefix = HotkeyPrefix.Hide;

					return format;
				}
			}

			public new ToolTipIcon Icon {
				get { return this.icon; }
				set { 
					if (value == this.icon)
						return;

					this.icon = value;
					Recalculate ();
				}
			}

			public string Title {
				get { return this.title; }
				set { 
					if (value == this.title)
						return;

					this.title = value;
					Recalculate ();
				}
			}

			public override string Text {
				get { return this.text; }
				set { 
					if (value == this.text)
						return;

					this.text = value;
					Recalculate ();
				}
			}
			
			public int Timeout {
				get { return timer.Interval; }
				set {
					// Some systems theres a limitiation in timeout, WinXP is between 10k and 30k.
					if (value < 10000)
						timer.Interval = 10000;
					else if (value > 30000)
						timer.Interval = 30000;
					else
						timer.Interval = value;
				}
			}
		}
		#endregion  // NotifyIconBalloonWindow Class

		#region Public Constructors
		public NotifyIcon() {
			window = new NotifyIconWindow(this);
			systray_active = false;

			balloon_title = "";
			balloon_text = "";
		}

		public NotifyIcon(System.ComponentModel.IContainer container) : this() {
		}
		#endregion	// Public Constructors

		#region Public Methods
		public void ShowBalloonTip (int timeout)
		{
			ShowBalloonTip(timeout, balloon_title, balloon_text, balloon_icon);
		}

		public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
		{
			XplatUI.SystrayBalloon(window.Handle, timeout, tipTitle, tipText, tipIcon);
		}
		#endregion Public Methods
		
		#region Private Methods
		private void OnBalloonTipClicked (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BalloonTipClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnBalloonTipClosed (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BalloonTipClosedEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnBalloonTipShown (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [BalloonTipShownEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnDoubleClick (EventArgs e)
		{
			double_click = true;
			EventHandler eh = (EventHandler)(Events [DoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnMouseClick (MouseEventArgs e)
		{
			MouseEventHandler eh = (MouseEventHandler)(Events[MouseClickEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		private void OnMouseDoubleClick (MouseEventArgs e)
		{
			MouseEventHandler eh = (MouseEventHandler)(Events[MouseDoubleClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnMouseDown (MouseEventArgs e)
		{
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnMouseUp (MouseEventArgs e)
		{
			if ((e.Button & MouseButtons.Right) == MouseButtons.Right) {
				if (context_menu != null) {
					XplatUI.SetForegroundWindow (window.Handle);
					context_menu.Show (window, new Point(e.X, e.Y));
				}
				else if (context_menu_strip != null) {
					XplatUI.SetForegroundWindow (window.Handle);
					context_menu_strip.Show (window, new Point (e.X, e.Y), ToolStripDropDownDirection.AboveLeft);
				}
			}

			MouseEventHandler eh = (MouseEventHandler)(Events [MouseUpEvent]);
			if (eh != null)
				eh (this, e);

			if (!double_click) {
				OnClick (EventArgs.Empty);
				OnMouseClick (e);
				double_click = false;
			}
		}

		private void OnMouseMove (MouseEventArgs e)
		{
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseMoveEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void Recalculate () 
		{
			window.CalculateIconRect ();

			if (!Visible || (text == string.Empty && icon == null)) {
				HideSystray ();
			} else {

				if (systray_active)
					UpdateSystray ();
				else
					ShowSystray ();
			}
		}

		private void ShowSystray()
		{
			if (icon == null)
				return;

			icon_bitmap = icon.ToBitmap();

			systray_active = true;
			XplatUI.SystrayAdd(window.Handle, text, icon, out tooltip);
		}

		private void HideSystray()
		{
			if (!systray_active) {
				return;
			}

			systray_active = false;
			XplatUI.SystrayRemove(window.Handle, ref tooltip);
		}

		private void UpdateSystray()
		{
			if (icon_bitmap != null) {
				icon_bitmap.Dispose();
			}

			if (icon != null) {
				icon_bitmap = icon.ToBitmap();
			}

			window.Invalidate();
			XplatUI.SystrayChange(window.Handle, text, icon, ref tooltip);
		}
		#endregion	// Private Methods

		#region Public Instance Properties
		[DefaultValue ("None")]
		public ToolTipIcon BalloonTipIcon {
			get { return this.balloon_icon; }
			set {
				if (value == this.balloon_icon)
					return;
        	
            	this.balloon_icon = value;
			}
		}

		[Localizable(true)]
		[DefaultValue ("")]
		[Editor ("System.ComponentModel.Design.MultilineStringEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string BalloonTipText {
			get { return this.balloon_text; }
			set {
				if (value == this.balloon_text)
					return;
				
				this.balloon_text = value;
			}
		}
 		
		[Localizable(true)]
		[DefaultValue ("")]
		public string BalloonTipTitle {
			get { return this.balloon_title; }
			set {
				if (value == this.balloon_title)
					return;
	
				this.balloon_title = value;
			}
		}
		
		[DefaultValue(null)]
		[Browsable (false)]
		public ContextMenu ContextMenu {
			get {
				return context_menu;
			}

			set {
				if (context_menu != value) {
					context_menu = value;
					window.ContextMenu = value;
				}
			}
		}

		[DefaultValue (null)]
		public ContextMenuStrip ContextMenuStrip {
			get { return this.context_menu_strip; }
			set {
				if (this.context_menu_strip != value) {
					this.context_menu_strip = value;
					window.ContextMenuStrip = value;
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(null)]
		public Icon Icon {
			get {
				return icon;
			}

			set {
				if (icon != value) {
					icon = value;
					Recalculate ();
				}
			}
		}

		[Localizable (false)]
		[Bindable (true)]
		[TypeConverter (typeof (StringConverter))]
		[DefaultValue (null)]
		public object Tag {
			get { return this.tag; }
			set { this.tag = value; }
		}

		[DefaultValue ("")]
		[Editor ("System.ComponentModel.Design.MultilineStringEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
		[Localizable (true)]
		public string Text {
			get {
				return text;
			}

			set {
				if (text != value) {
					if (value.Length >= 64) {
						throw new ArgumentException("ToolTip length must be less than 64 characters long", "Text");
					}
					text = value;
					Recalculate ();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue(false)]
		public bool Visible {
			get {
				return visible;
			}

			set {
				if (visible != value) {
					visible = value;

					// Let our control know, too
					window.is_visible = value;

					if (visible) {
						ShowSystray ();
					} else {
						HideSystray();
					}
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			if (visible)
				HideSystray();

			if (icon_bitmap != null) {
				icon_bitmap.Dispose();
			}

			if (disposing)
				icon = null;

			base.Dispose (disposing);
		}

		#endregion	// Protected Instance Methods

		#region Events
		static object ClickEvent = new object ();
		static object DoubleClickEvent = new object ();
		static object MouseDownEvent = new object ();
		static object MouseMoveEvent = new object ();
		static object MouseUpEvent = new object ();
		static object BalloonTipClickedEvent = new object ();
		static object BalloonTipClosedEvent = new object ();
		static object BalloonTipShownEvent = new object ();
		static object MouseClickEvent = new object ();
		static object MouseDoubleClickEvent = new object ();

		[MWFCategory("Action")]
		public event EventHandler BalloonTipClicked {
			add { Events.AddHandler (BalloonTipClickedEvent, value); }
			remove { Events.RemoveHandler (BalloonTipClickedEvent, value); }
		}

		[MWFCategory("Action")]
		public event EventHandler BalloonTipClosed {
			add { Events.AddHandler (BalloonTipClosedEvent, value); }
			remove { Events.RemoveHandler (BalloonTipClosedEvent, value); }
		}

		[MWFCategory("Action")]
		public event EventHandler BalloonTipShown {
			add { Events.AddHandler (BalloonTipShownEvent, value); }
			remove { Events.RemoveHandler (BalloonTipShownEvent, value); }
		}

		[MWFCategory("Action")]
		public event MouseEventHandler MouseClick {
			add { Events.AddHandler (MouseClickEvent, value); }
			remove { Events.RemoveHandler (MouseClickEvent, value); }
		}

		[MWFCategory ("Action")]
		public event MouseEventHandler MouseDoubleClick {
			add { Events.AddHandler (MouseDoubleClickEvent, value); }
			remove { Events.RemoveHandler (MouseDoubleClickEvent, value); }
		}

		[MWFCategory("Action")]
		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}

		[MWFCategory("Action")]
		public event EventHandler DoubleClick {
			add { Events.AddHandler (DoubleClickEvent, value); }
			remove { Events.RemoveHandler (DoubleClickEvent, value); }
		}

		public event MouseEventHandler MouseDown {
			add { Events.AddHandler (MouseDownEvent, value); }
			remove { Events.RemoveHandler (MouseDownEvent, value); }
		}

		public event MouseEventHandler MouseMove {
			add { Events.AddHandler (MouseMoveEvent, value); }
			remove { Events.RemoveHandler (MouseMoveEvent, value); }
		}

		public event MouseEventHandler MouseUp {
			add { Events.AddHandler (MouseUpEvent, value); }
			remove { Events.RemoveHandler (MouseUpEvent, value); }
		}

		#endregion	// Events
	}
}
