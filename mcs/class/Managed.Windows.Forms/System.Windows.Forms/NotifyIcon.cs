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

namespace System.Windows.Forms {
	[DefaultProperty("Text")]
#if NET_2_0
	[DefaultEvent("MouseDoubleClick")]
#else
	[DefaultEvent("MouseDown")]
#endif
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
#if NET_2_0
		private string balloon_text;
		private string balloon_title;
		private ToolTipIcon balloon_icon;
		private ContextMenuStrip	context_menu_strip;
		private object			tag;
#endif
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
				Click += new EventHandler(HandleClick);
				DoubleClick += new EventHandler(HandleDoubleClick);
				MouseDown +=new MouseEventHandler(HandleMouseDown);
				MouseUp +=new MouseEventHandler(HandleMouseUp);
				MouseMove +=new MouseEventHandler(HandleMouseMove);
				ContextMenu = owner.context_menu;
#if NET_2_0
				ContextMenuStrip = owner.context_menu_strip;
#endif
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
								owner.OnClick (EventArgs.Empty);
								return;
							}

							case Msg.WM_LBUTTONDBLCLK: {
								owner.OnDoubleClick (EventArgs.Empty);
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
								owner.OnClick (EventArgs.Empty);
								return;
							}

							case Msg.WM_RBUTTONDBLCLK: {
								owner.OnDoubleClick (EventArgs.Empty);
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
					e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(SystemColors.Window), rect);
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

			private void HandleClick (object sender, EventArgs e)
			{
				owner.OnClick (e);
			}

			private void HandleDoubleClick (object sender, EventArgs e)
			{
				owner.OnDoubleClick (e);
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

		#region Public Constructors
		public NotifyIcon() {
			window = new NotifyIconWindow(this);
			systray_active = false;

#if NET_2_0			
			balloon_title = "";
			balloon_text = "";
#endif
		}

		public NotifyIcon(System.ComponentModel.IContainer container) : this() {
		}
		#endregion	// Public Constructors

		#region Public Methods
#if NET_2_0
		public void ShowBalloonTip (int timeout)
		{
			ShowBalloonTip(timeout, balloon_title, balloon_text, balloon_icon);
		}

		public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
		{
			// TODO: Call ShowBalloonTip in XplatUI.
		}
#endif
		#endregion Public Methods
		
		#region Private Methods
		private void OnClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ClickEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void OnDoubleClick (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [DoubleClickEvent]);
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
			if ((e.Button & MouseButtons.Right) == MouseButtons.Right && context_menu != null)
				context_menu.Show (window, new Point(e.X, e.Y));
#if NET_2_0
			else if ((e.Button & MouseButtons.Right) == MouseButtons.Right && context_menu_strip != null)
				context_menu_strip.Show (window, new Point (e.X, e.Y), ToolStripDropDownDirection.AboveLeft);
#endif
			
			MouseEventHandler eh = (MouseEventHandler)(Events [MouseUpEvent]);
			if (eh != null)
				eh (this, e);
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

			if (systray_active)
				UpdateSystray ();
		}

		private void ShowSystray()
		{
			systray_active = true;

			if (icon == null)
				return;

			icon_bitmap = icon.ToBitmap();

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

			XplatUI.SystrayChange(window.Handle, text, icon, ref tooltip);
			window.Invalidate();
		}
		#endregion	// Private Methods

		#region Public Instance Properties
#if NET_2_0
		public ToolTipIcon BalloonTipIcon {
			get { return this.balloon_icon; }
			set {
				if (value == this.balloon_icon)
					return;
        	
            	this.balloon_icon = value;
			}
		}

		[Localizable(true)]
		public string BalloonTipText {
			get { return this.balloon_text; }
			set {
				if (value == this.balloon_text)
					return;
				
				this.balloon_text = value;
			}
		}
 		
		[Localizable(true)]
		public string BalloonTipTitle {
			get { return this.balloon_title; }
			set {
				if (value == this.balloon_title)
					return;
	
				this.balloon_title = value;
			}
		}
#endif
		
		[DefaultValue(null)]
#if NET_2_0
		[Browsable (false)]
#endif
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

#if NET_2_0
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
#endif

		[Localizable(true)]
		[DefaultValue(null)]
		public Icon Icon {
			get {
				return icon;
			}

			set {
				if (icon != value) {
					icon = value;
					if (text == string.Empty && icon == null) {
						HideSystray ();
					}
					else {
						Recalculate ();
					}
				}
			}
		}

#if NET_2_0
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
#endif
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
					if (text == string.Empty && icon == null) {
						HideSystray();
					} else {
						Recalculate ();
					}
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

#if NET_2_0
		[MWFCategory("Action")]
#else
		[Category("Action")]
#endif
		public event EventHandler Click {
			add { Events.AddHandler (ClickEvent, value); }
			remove { Events.RemoveHandler (ClickEvent, value); }
		}

#if NET_2_0
		[MWFCategory("Action")]
#else
		[Category("Action")]
#endif
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
