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

// NOT COMPLETE

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace System.Windows.Forms {
	[DefaultProperty("Text")]
	[DefaultEvent("MouseDown")]
	[Designer ("System.Windows.Forms.Design.NotifyIconDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Allow)]
	public sealed class NotifyIcon : System.ComponentModel.Component {
		#region Local Variables
		private ContextMenu		context_menu;
		private Icon			icon;
		private Bitmap			icon_bitmap;
		private string			text;
		private bool			visible;
		private NotifyIconWindow	window;
		private bool			systray_active;
		private ToolTip			tooltip;
		#endregion	// Local Variables

		#region NotifyIconWindow Class
		internal class NotifyIconWindow : Control {
			NotifyIcon	owner;
			Rectangle	rect;

			public NotifyIconWindow(NotifyIcon owner) {
				this.owner = owner;
				is_visible = false;
				rect = new Rectangle(0, 0, 1, 1);

				CreateControl();

				Paint += new PaintEventHandler(HandlePaint);
				SizeChanged += new EventHandler(HandleSizeChanged);

				// Events that need to be sent to our parent
				Click += new EventHandler(HandleClick);
				DoubleClick += new EventHandler(HandleDoubleClick);
				MouseDown +=new MouseEventHandler(HandleMouseDown);
				MouseUp +=new MouseEventHandler(HandleMouseUp);
				MouseMove +=new MouseEventHandler(HandleMouseMove);
				ContextMenu = owner.context_menu;
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams cp;

					cp = base.CreateParams;

					cp.Parent = IntPtr.Zero;
					cp.Style = (int)WindowStyles.WS_POPUP;
					cp.Style |= (int)WindowStyles.WS_CLIPSIBLINGS;

					cp.ExStyle = (int)(WindowStyles.WS_EX_TOOLWINDOW);

					return cp;
				}
			}

			protected override void WndProc(ref Message m) {
				switch((Msg)m.Msg) {
					case Msg.WM_USER: {
						switch ((Msg)m.LParam.ToInt32()) {
							case Msg.WM_LBUTTONDOWN: {
								HandleMouseDown(this, new MouseEventArgs(MouseButtons.Left, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_LBUTTONUP: {
								HandleMouseUp(this, new MouseEventArgs(MouseButtons.Left, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								HandleClick(this, EventArgs.Empty);
								return;
							}

							case Msg.WM_LBUTTONDBLCLK: {
								HandleDoubleClick(this, EventArgs.Empty);
								return;
							}

							case Msg.WM_MOUSEMOVE: {
								HandleMouseMove(this, new MouseEventArgs(MouseButtons.None, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_RBUTTONDOWN: {
								HandleMouseDown(this, new MouseEventArgs(MouseButtons.Right, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								return;
							}

							case Msg.WM_RBUTTONUP: {
								HandleMouseUp(this, new MouseEventArgs(MouseButtons.Right, 1, Control.MousePosition.X, Control.MousePosition.Y, 0));
								HandleClick(this, EventArgs.Empty);
								return;
							}

							case Msg.WM_RBUTTONDBLCLK: {
								HandleDoubleClick(this, EventArgs.Empty);
								return;
							}
						}
						return;
					}
				}
				base.WndProc (ref m);
			}

			internal void CalculateIconRect() {
				if (owner != null && owner.icon != null) {
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

					// Force our window to be square
					if (Width != size) {
						this.Width = size;
					}

					if (Height != size) {
						this.Height = size;
					}
				}
			}

			private void HandlePaint(object sender, PaintEventArgs e) {
				if (owner.icon != null) {
					e.Graphics.DrawImage(owner.icon_bitmap, rect);

				}
			}

			private void HandleSizeChanged(object sender, EventArgs e) {
				CalculateIconRect();
			}

			private void HandleClick(object sender, EventArgs e) {
				if (owner.Click != null) {
					owner.Click(owner, e);
				}
			}

			private void HandleDoubleClick(object sender, EventArgs e) {
				if (owner.DoubleClick != null) {
					owner.DoubleClick(owner, e);
				}
			}

			private void HandleMouseDown(object sender, MouseEventArgs e) {
				if (owner.MouseDown != null) {
					owner.MouseDown(owner, e);
				}
			}

			private void HandleMouseUp(object sender, MouseEventArgs e) {
				if (owner.context_menu != null) {
					owner.context_menu.Show(this, new Point(e.X, e.Y));
				}

				if (owner.MouseUp != null) {
					owner.MouseUp(owner, e);
				}
			}

			private void HandleMouseMove(object sender, MouseEventArgs e) {
				if (owner.MouseMove != null) {
					owner.MouseMove(owner, e);
				}
			}
		}
		#endregion	// NotifyIconWindow Class

		#region Public Constructors
		public NotifyIcon() {
			window = new NotifyIconWindow(this);
			systray_active = false;
		}

		public NotifyIcon(System.ComponentModel.IContainer container) : this() {
		}
		#endregion	// Public Constructors

		#region Private Methods
		private void ShowSystray(bool property_changed) {
			if (property_changed) {
				window.CalculateIconRect();
			}

			if (systray_active) {
				if (property_changed) {
					UpdateSystray();
				}
				return;
			}

			if (icon != null) {
				icon_bitmap = icon.ToBitmap();
			}

			systray_active = true;
			XplatUI.SystrayAdd(window.Handle, text, icon, out tooltip);
		}

		private void HideSystray() {
			if (!systray_active) {
				return;
			}

			systray_active = false;
			XplatUI.SystrayRemove(window.Handle, ref tooltip);
		}

		private void UpdateSystray() {
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
		[DefaultValue(null)]
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

		[Localizable(true)]
		[DefaultValue(null)]
		public Icon Icon {
			get {
				return icon;
			}

			set {
				if (icon != value) {
					if (icon != null) {
						icon.Dispose();
					}
					icon = value;
					ShowSystray(true);
				}
			}
		}

		[Localizable(true)]
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
						ShowSystray(true);
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
						ShowSystray(false);
					} else {
						HideSystray();
					}
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			if (icon != null) {
				icon.Dispose();
			}

			if (icon_bitmap != null) {
				icon_bitmap.Dispose();
			}
			base.Dispose (disposing);
		}

		#endregion	// Protected Instance Methods

		#region Events
		[Category]
		public event EventHandler	Click;

		[Category]
		public event EventHandler	DoubleClick;

		public event MouseEventHandler	MouseDown;
		public event MouseEventHandler	MouseMove;
		public event MouseEventHandler	MouseUp;
		#endregion	// Events
	}
}
