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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	[ToolboxItemFilter("System.Windows.Forms")]
	[ProvideProperty("IconAlignment", "System.Windows.Forms.Control, " + Consts.AssemblySystem_Windows_Forms)]
	[ProvideProperty("IconPadding", "System.Windows.Forms.Control, " + Consts.AssemblySystem_Windows_Forms)]
	[ProvideProperty("Error", "System.Windows.Forms.Control, " + Consts.AssemblySystem_Windows_Forms)]
	[ComplexBindingProperties ("DataSource", "DataMember")]
	public class ErrorProvider : Component, IExtenderProvider, ISupportInitialize
	{
		private class ErrorWindow : UserControl
		{
			public ErrorWindow ()
			{
				SetStyle (ControlStyles.Selectable, false);
			}
		}

		#region Private Classes
		private class ErrorProperty {
			public ErrorIconAlignment	alignment;
			public int			padding;
			public string			text;
			public Control			control;
			public ErrorProvider		ep;
			private ErrorWindow		window;
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

				window = new ErrorWindow ();
				window.Visible = false;
				window.Width = ep.icon.Width;
				window.Height = ep.icon.Height;

				// UIA Framework: Associate ErrorProvider with Control
				ErrorProvider.OnUIAErrorProviderHookUp (ep, new ControlEventArgs (control));

				// UIA Framework: Generate event to associate UserControl with ErrorProvider
				window.VisibleChanged += delegate (object sender, EventArgs args) {
					if (window.Visible == true)
						ErrorProvider.OnUIAControlHookUp (control, new ControlEventArgs (window));
					else 
						ErrorProvider.OnUIAControlUnhookUp (control, new ControlEventArgs (window));
				};

				if (control.Parent != null) {
					// UIA Framework: Generate event to associate UserControl with ErrorProvider
					ErrorProvider.OnUIAControlHookUp (control, new ControlEventArgs (window));
					control.Parent.Controls.Add(window);
					control.Parent.Controls.SetChildIndex(window, control.Parent.Controls.IndexOf (control) + 1);
				}

				window.Paint += new PaintEventHandler(window_Paint);
				window.MouseEnter += new EventHandler(window_MouseEnter);
				window.MouseLeave += new EventHandler(window_MouseLeave);
				control.SizeChanged += new EventHandler(control_SizeLocationChanged);
				control.LocationChanged += new EventHandler(control_SizeLocationChanged);
				control.ParentChanged += new EventHandler (control_ParentChanged);
				// Do we want to block mouse clicks? if so we need a few more events handled

				CalculateAlignment();
			}

			public string Text {
				get {
					return text;
				}

				set {
					if (value == null)
						value = string.Empty;

					bool differentError = text != value;
					text = value;

					if (text != String.Empty) {
						window.Visible = true;
					} else {
						window.Visible = false;
						return;
					}

					// even if blink style is NeverBlink we need it to allow
					// the timer to elapse at least once to get the icon to 
					// display
					if (differentError || ep.blinkstyle == ErrorBlinkStyle.AlwaysBlink) {
						if (timer == null) {
							timer = new System.Windows.Forms.Timer();
							timer.Tick += tick;
						}
						timer.Interval = ep.blinkrate;
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

					// UIA Framework: Associate Control with ToolTip, used on Popup events
					ep.UIAControl = control;

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

			private void control_ParentChanged (object sender, EventArgs e)
			{
				if (control.Parent != null) {

					// UIA Framework: Generate event to disassociate UserControl with ErrorProvider
					ErrorProvider.OnUIAControlUnhookUp (control, new ControlEventArgs (window));
					control.Parent.Controls.Add (window);
					control.Parent.Controls.SetChildIndex (window, control.Parent.Controls.IndexOf (control) + 1);
	
					// UIA Framework: Generate event to associate UserControl with ErrorProvider
					ErrorProvider.OnUIAControlHookUp (control, new ControlEventArgs (window));
				}
			}

			private void window_Tick(object sender, EventArgs e) {
				if (timer.Enabled && control.IsHandleCreated && control.Visible) {
					blink_count++;

					using (Graphics g = window.CreateGraphics()) {
						if ((blink_count % 2) == 0) {
							g.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(window.Parent.BackColor), window.ClientRectangle);
						} else {
							g.DrawIcon(this.ep.icon, 0, 0);
						}
					}

					switch (ep.blinkstyle) {
					case ErrorBlinkStyle.AlwaysBlink:
						break;
					case ErrorBlinkStyle.BlinkIfDifferentError:
						if (blink_count > 10)
							timer.Stop();
						break;
					case ErrorBlinkStyle.NeverBlink:
						timer.Stop ();
						break;
					}

					if (blink_count == 11)
						blink_count = 1;
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

		private bool right_to_left;
		private object tag;
		#endregion	// Local Variables

		#region Public Constructors
		public ErrorProvider()
		{
			controls = new Hashtable();

			blinkrate = 250;
			blinkstyle = ErrorBlinkStyle.BlinkIfDifferentError;

			icon = ResourceImageLoader.GetIcon ("errorProvider.ico");
			tooltip = new ToolTip.ToolTipWindow();

			//UIA Framework: Event used to indicate the ToolTip is shown/hidden.
			tooltip.VisibleChanged += delegate (object sender, EventArgs args) {
				if (tooltip.Visible == true)
					OnUIAPopup (this, new PopupEventArgs (UIAControl, UIAControl, false, Size.Empty));
				else if (tooltip.Visible == false)
					OnUIAUnPopup (this, new PopupEventArgs (UIAControl, UIAControl, false, Size.Empty));
			};
		}

		public ErrorProvider(ContainerControl parentControl) : this ()
		{
			container = parentControl;
		}
		
		public ErrorProvider (IContainer container) : this ()
		{
			container.Add (this);
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

		[MonoTODO ("Stub, does nothing")]
		[DefaultValue (null)]
		[Editor ("System.Windows.Forms.Design.DataMemberListEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DataMember {
			get {
				return datamember;
			}

			set {
				datamember = value;
				// FIXME - add binding magic and also update BindToDataAndErrors with it
			}
		}

		[MonoTODO ("Stub, does nothing")]
		[DefaultValue (null)]
		[AttributeProvider (typeof (IListSource))]
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
				if (value != null && (value.Height != 16 || value.Width != 16))
					icon = new Icon (value, 16, 16);
				else
					icon = value;
			}
		}

		public override ISite Site {
			set {
				base.Site = value;
			}
		}

		[MonoTODO ("RTL not supported")]
		[Localizable (true)]
		[DefaultValue (false)]
		public virtual bool RightToLeft {
			get { return right_to_left; }
			set { right_to_left = value; }
		}

		[Localizable (false)]
		[Bindable (true)]
		[TypeConverter (typeof (StringConverter))]
		[DefaultValue (null)]
		[MWFCategory ("Data")]
		public object Tag {
			get { return this.tag; }
			set { this.tag = value; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		[MonoTODO ("Stub, does nothing")]
		public void BindToDataAndErrors (object newDataSource, string newDataMember)
		{
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

		public void Clear ()
		{
			foreach (ErrorProperty ep in controls.Values)
				ep.Text = string.Empty;
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

		[MonoTODO ("Stub, does nothing")]
		public void UpdateBinding ()
		{
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			base.Dispose (disposing);
		}

		[EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[RightToLeftChangedEvent]);
			if (eh != null)
				eh (this, e);
		}
		#endregion	// Protected Instance Methods

		#region Private Methods
		private ErrorProperty GetErrorProperty(Control control) {
			ErrorProperty ep = (ErrorProperty)controls[control];
			if (ep == null) {
				ep = new ErrorProperty(this, control);
				controls[control] = ep;
			}
			return ep;
		}
		#endregion	// Private Methods

		void ISupportInitialize.BeginInit ()
		{
		}

		void ISupportInitialize.EndInit ()
		{
		}

		#region Public Events
		static object RightToLeftChangedEvent = new object ();

		public event EventHandler RightToLeftChanged {
			add { Events.AddHandler (RightToLeftChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftChangedEvent, value); }
		}
		#endregion

		#region UIA Framework: Events, Properties and Methods
		// NOTE: 
		//	We are using Reflection to add/remove internal events.
		//      Class ToolTipListener uses the events.
		//
		//	- UIAControlHookUp. Event used to associate UserControl with ErrorProvider
		//	- UIAControlUnhookUp. Event used to disassociate UserControl with ErrorProvider
		//	- UIAErrorProviderHookUp. Event used to associate Control with ErrorProvider
		//	- UIAErrorProviderUnhookUp. Event used to disassociate Control with ErrorProvider
		//	- UIAPopup. Event used show Popup
		//	- UIAUnPopup. Event used to hide popup.

		private Control uia_control;

		internal Control UIAControl {
			get { return uia_control; }
			set { uia_control = value; }
		}

		internal Rectangle UIAToolTipRectangle {
			get { return tooltip.Bounds; }
		}

		internal static event ControlEventHandler UIAControlHookUp;
		internal static event ControlEventHandler UIAControlUnhookUp;
		internal static event ControlEventHandler UIAErrorProviderHookUp;
		internal static event ControlEventHandler UIAErrorProviderUnhookUp;
		internal static event PopupEventHandler UIAPopup;
		internal static event PopupEventHandler UIAUnPopup;

		internal static void OnUIAPopup (ErrorProvider sender, PopupEventArgs args)
		{
			if (UIAPopup != null)
				UIAPopup (sender, args);
		}

		internal static void OnUIAUnPopup (ErrorProvider sender, PopupEventArgs args)
		{
			if (UIAUnPopup != null)
				UIAUnPopup (sender, args);
		}

		internal static void OnUIAControlHookUp (object sender, ControlEventArgs args)
		{
			if (UIAControlHookUp != null)
				UIAControlHookUp (sender, args);
		}

		internal static void OnUIAControlUnhookUp (object sender, ControlEventArgs args)
		{
			if (UIAControlUnhookUp != null)
				UIAControlUnhookUp (sender, args);
		}

		internal static void OnUIAErrorProviderHookUp (object sender, ControlEventArgs args) 
		{
			if (UIAErrorProviderHookUp != null)
				UIAErrorProviderHookUp (sender, args);
		}

		internal static void OnUIAErrorProviderUnhookUp (object sender, ControlEventArgs args) 
		{
			if (UIAErrorProviderUnhookUp != null)
				UIAErrorProviderUnhookUp (sender, args);
		}
		#endregion
	}
}
