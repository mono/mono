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
//	Peter Bartok	(pbartok@novell.com)
//
//

// COMPLETE

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	[ToolboxItemFilter("System.Windows.Forms")]
	[ProvideProperty("Error", "System.Windows.Forms.Control, System.Windows.Forms, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
	[ProvideProperty("IconPadding", "System.Windows.Forms.Control, System.Windows.Forms, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
	[ProvideProperty("IconAlignment", "System.Windows.Forms.Control, System.Windows.Forms, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
	public class ErrorProvider : Component, IExtenderProvider {
		#region Private Classes
		private class ErrorProperty {
			public ErrorIconAlignment	alignment;
			public int			padding;
			public string			text;
			public Control			control;
			public ErrorProvider		ep;
			private UserControl		window;
			private bool			visible;
			private int			blink_count;
			private EventHandler		tick;
			private System.Windows.Forms.Timer	timer;

			public ErrorProperty(ErrorProvider ep, Control control) {
				this.ep = ep;
				this.control = control;

				alignment = ErrorIconAlignment.MiddleRight;
				padding = 0;
				text = string.Empty;
				blink_count = 0;

				tick = new EventHandler(window_Tick);

				window = new UserControl();
				window.Visible = false;
				window.Width = ep.icon.Width;
				window.Height = ep.icon.Height;

				if (ep.container != null) {
					ep.container.Controls.Add(window);
					ep.container.Controls.SetChildIndex(window, 0);
				} else {
					control.parent.Controls.Add(window);
					control.parent.Controls.SetChildIndex(window, 0);
				}

				window.Paint += new PaintEventHandler(window_Paint);
				window.MouseEnter += new EventHandler(window_MouseEnter);
				window.MouseLeave += new EventHandler(window_MouseLeave);
				control.SizeChanged += new EventHandler(control_SizeLocationChanged);
				control.LocationChanged += new EventHandler(control_SizeLocationChanged);
				// Do we want to block mouse clicks? if so we need a few more events handled

				CalculateAlignment();
			}

			public string Text {
				get {
					return text;
				}

				set {
					text = value;
					if (text != String.Empty) {
						window.Visible = true;
					} else {
						window.Visible = false;
					}

					if (ep.blinkstyle != ErrorBlinkStyle.NeverBlink) {
						if (timer == null) {
							timer = new System.Windows.Forms.Timer();
						}
						timer.Interval = ep.blinkrate;
						timer.Tick += tick;
						blink_count = 0;
						timer.Enabled = true;
					}
				}
			}

			public ErrorIconAlignment Alignment {
				get {
					return alignment;
				}

				set {
					if (alignment != value) {
						alignment = value;
						CalculateAlignment();
					}
				}
			}

			public int Padding {
				get {
					return padding;
				}

				set {
					if (padding != value) {
						padding = value;
						CalculateAlignment();
					}
				}
			}

			private void CalculateAlignment() {
				if (visible) {
					visible = false;
					ep.tooltip.Visible = false;
				}

				switch (alignment) {
					case ErrorIconAlignment.TopLeft: {
						window.Left = control.Left - ep.icon.Width - padding;
						window.Top = control.Top;
						break;
					}

					case ErrorIconAlignment.TopRight: {
						window.Left = control.Left + control.Width + padding;
						window.Top = control.Top;
						break;
					}

					case ErrorIconAlignment.MiddleLeft: {
						window.Left = control.Left - ep.icon.Width - padding;
						window.Top = control.Top + (control.Height - ep.icon.Height) / 2;
						break;
					}

					case ErrorIconAlignment.MiddleRight: {
						window.Left = control.Left + control.Width + padding;
						window.Top = control.Top + (control.Height - ep.icon.Height) / 2;
						break;
					}

					case ErrorIconAlignment.BottomLeft: {
						window.Left = control.Left - ep.icon.Width - padding;
						window.Top = control.Top + control.Height - ep.icon.Height;
						break;
					}

					case ErrorIconAlignment.BottomRight: {
						window.Left = control.Left + control.Width + padding;
						window.Top = control.Top + control.Height - ep.icon.Height;
						break;
					}
				}
			}

			private void window_Paint(object sender, PaintEventArgs e) {
				if (text != string.Empty) {
					e.Graphics.DrawIcon(this.ep.icon, 0, 0);
				}
			}

			private void window_MouseEnter(object sender, EventArgs e) {
				if (!visible) {
					Size	size;
					Point	pt;

					visible = true;

					pt = Control.MousePosition;

					size = ThemeEngine.Current.ToolTipSize(ep.tooltip, text);
					ep.tooltip.Width = size.Width;
					ep.tooltip.Height = size.Height;
					ep.tooltip.Text = text;

					if ((pt.X + size.Width) < SystemInformation.WorkingArea.Width) {
						ep.tooltip.Left = pt.X;
					} else {
						ep.tooltip.Left = pt.X - size.Width;
					}

					if ((pt.Y + size.Height) < (SystemInformation.WorkingArea.Height - 16)) {
						ep.tooltip.Top = pt.Y + 16;
					} else {
						ep.tooltip.Top = pt.Y - size.Height;
					}
					ep.tooltip.Visible = true;
				}
			}

			private void window_MouseLeave(object sender, EventArgs e) {
				if (visible) {
					visible = false;
					ep.tooltip.Visible = false;
				}
			}

			private void control_SizeLocationChanged(object sender, EventArgs e) {
				if (visible) {
					visible = false;
					ep.tooltip.Visible = false;
				}
				CalculateAlignment();
			}

			private void window_Tick(object sender, EventArgs e) {
				if (timer.Enabled) {
					Graphics g;

					blink_count++;

					// Dunno why this POS doesn't reliably blink
					g = window.CreateGraphics();
					if ((blink_count % 2) == 0) {
						g.FillRectangle(new SolidBrush(window.parent.BackColor), window.ClientRectangle);
					} else {
						g.DrawIcon(this.ep.icon, 0, 0);
					}
					g.Dispose();

					if ((blink_count > 6) && (ep.blinkstyle == ErrorBlinkStyle.BlinkIfDifferentError)) {
						timer.Stop();
						blink_count = 0;
					}
				}
			}
		}
		#endregion

		#region Local Variables
		private int			blinkrate;
		private ErrorBlinkStyle		blinkstyle;
		private string			datamember;
		private object			datasource;
		private ContainerControl	container;
		private Icon			icon;
		private Hashtable		controls;
		private ToolTip.ToolTipWindow	tooltip;
		#endregion	// Local Variables

		#region Public Constructors
		public ErrorProvider() {
			controls = new Hashtable();

			blinkrate = 250;
			blinkstyle = ErrorBlinkStyle.BlinkIfDifferentError;

			icon = (Icon)Locale.GetResource("errorProvider.ico");
			tooltip = new ToolTip.ToolTipWindow(null);
		}

		public ErrorProvider(ContainerControl parentControl) : this() {
			container = parentControl;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[DefaultValue(250)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public int BlinkRate {
			get {
				return blinkrate;
			}

			set {
				blinkrate = value;
			}
		}

		[DefaultValue(ErrorBlinkStyle.BlinkIfDifferentError)]
		public ErrorBlinkStyle BlinkStyle {
			get {
				return blinkstyle;
			}

			set {
				blinkstyle = value;
			}
		}

		[DefaultValue(null)]
		public ContainerControl ContainerControl {
			get {
				return container;
			}

			set {
				container = value;
			}
		}

		[MonoTODO]
		[DefaultValue(null)]
		[Editor ("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public string DataMember {
			get {
				return datamember;
			}

			set {
				datamember = value;
				// FIXME - add binding magic and also update BindToDataAndErrors with it
			}
		}

		[MonoTODO]
		[DefaultValue(null)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public object DataSource {
			get {
				return datasource;
			}

			set {
				datasource = value;
				// FIXME - add binding magic and also update BindToDataAndErrors with it
			}
		}

		[Localizable(true)]
		public Icon Icon {
			get {
				return icon;
			}

			set {
				icon = value;
			}
		}

		public override ISite Site {
			set {
				base.Site = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		[MonoTODO]
		public void BindToDataAndErrors(object newDataSource, string newDataMember) {
			datasource = newDataSource;
			datamember = newDataMember;
			// FIXME - finish
		}

		public bool CanExtend(object extendee) {
			if (!(extendee is Control)) {
				return false;
			}

			if ((extendee is Form) || (extendee is ToolBar)) {
				return false;
			}

			return true;
		}

		[Localizable(true)]
		[DefaultValue("")]
		public string GetError(Control control) {
			return GetErrorProperty(control).Text;
		}

		[Localizable(true)]
		[DefaultValue(ErrorIconAlignment.MiddleRight)]
		public ErrorIconAlignment GetIconAlignment(Control control) {
			return GetErrorProperty(control).Alignment;
		}

		[Localizable(true)]
		[DefaultValue(0)]
		public int GetIconPadding(Control control) {
			return GetErrorProperty(control).padding;
		}

		public void SetError(Control control, string value) {
			GetErrorProperty(control).Text = value;
		}

		public void SetIconAlignment(Control control, ErrorIconAlignment value) {
			GetErrorProperty(control).Alignment = value;
		}

		public void SetIconPadding(Control control, int padding) {
			GetErrorProperty(control).Padding = padding;
		}

		[MonoTODO]
		public void UpdateBinding() {
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			base.Dispose (disposing);
		}
		#endregion	// Protected Instance Methods

		#region Private Methods
		private ErrorProperty GetErrorProperty(Control control) {
			ErrorProperty ep;

			ep = (ErrorProperty)controls[control];
			if (ep == null) {
				ep = new ErrorProperty(this, control);
				controls[control] = ep;
			}

			return ep;
		}
		#endregion	// Private Methods
	}
}
