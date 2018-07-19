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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;

namespace System.Windows.Forms {
	[DefaultEvent ("Popup")]
	[ProvideProperty ("ToolTip", typeof(System.Windows.Forms.Control))]
	[ToolboxItemFilter("System.Windows.Forms", ToolboxItemFilterType.Allow)]
	public class ToolTip : System.ComponentModel.Component, System.ComponentModel.IExtenderProvider {
		#region Local variables
		internal bool		is_active;
		internal int		automatic_delay;
		internal int		autopop_delay;
		internal int		initial_delay;
		internal int		re_show_delay;
		internal bool		show_always;

		internal Color		back_color;
		internal Color		fore_color;
		
		internal ToolTipWindow	tooltip_window;			// The actual tooltip window
		internal Hashtable	tooltip_strings;		// List of strings for each control, indexed by control
		internal ArrayList	controls;
		internal Control	active_control;			// Control for which the tooltip is currently displayed
		internal Control	last_control;			// last control the mouse was in
		internal Timer		timer;				// Used for the various intervals
		private Form		hooked_form;

		private bool isBalloon;
		private bool owner_draw;
		private bool stripAmpersands;
		private ToolTipIcon tool_tip_icon;
		private bool useAnimation;
		private bool useFading;
		private object tag;

		#endregion	// Local variables

		#region ToolTipWindow Class
		internal class ToolTipWindow : Control {
			#region ToolTipWindow Class Local Variables
			private Control associated_control;
			internal Icon icon;
			internal string title = String.Empty;
			internal Rectangle icon_rect;
			internal Rectangle title_rect;
			internal Rectangle text_rect;
			#endregion	// ToolTipWindow Class Local Variables
			
			#region ToolTipWindow Class Constructor
			internal ToolTipWindow() {
				Visible = false;
				Size = new Size(100, 20);
				ForeColor = ThemeEngine.Current.ColorInfoText;
				BackColor = ThemeEngine.Current.ColorInfo;

				VisibleChanged += new EventHandler(ToolTipWindow_VisibleChanged);

				// UIA Framework: Used to generate UnPopup
				VisibleChanged += new EventHandler (OnUIAToolTip_VisibleChanged);

				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw, true);
				if (ThemeEngine.Current.ToolTipTransparentBackground) {
					SetStyle (ControlStyles.SupportsTransparentBackColor, true);
					BackColor = Color.Transparent;
				} else
					SetStyle (ControlStyles.Opaque, true);

				SetTopLevel (true);
			}

			#endregion	// ToolTipWindow Class Constructor

			#region ToolTipWindow Class Protected Instance Methods
			protected override void OnCreateControl() {
				base.OnCreateControl ();
				XplatUI.SetTopmost(this.window.Handle, true);
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

			protected override void OnPaint(PaintEventArgs pevent) {
				// We don't do double-buffering on purpose:
				// 1) we'd have to meddle with is_visible, it destroys the buffers if !visible
				// 2) We don't draw much, no need to double buffer
				base.OnPaint(pevent);

				OnDraw (new DrawToolTipEventArgs (pevent.Graphics, associated_control, associated_control, ClientRectangle, this.Text, this.BackColor, this.ForeColor, this.Font));
			}

			protected override void OnTextChanged (EventArgs args)
			{
				Invalidate ();
				base.OnTextChanged (args); 
			}

			protected override void WndProc(ref Message m) {
				if (m.Msg == (int)Msg.WM_SETFOCUS) {
					if (m.WParam != IntPtr.Zero) {
						XplatUI.SetFocus(m.WParam);
					}
				}
				base.WndProc (ref m);
			}


			#endregion	// ToolTipWindow Class Protected Instance Methods

			#region ToolTipWindow Class Private Methods
			internal virtual void OnDraw (DrawToolTipEventArgs e)
			{
				DrawToolTipEventHandler eh = (DrawToolTipEventHandler)(Events[DrawEvent]);
				if (eh != null)
					eh (this, e);
				else
					ThemeEngine.Current.DrawToolTip (e.Graphics, e.Bounds, this);
			}
			
			internal virtual void OnPopup (PopupEventArgs e)
			{
				PopupEventHandler eh = (PopupEventHandler)(Events[PopupEvent]);
				if (eh != null)
					eh (this, e);
				else
					e.ToolTipSize = ThemeEngine.Current.ToolTipSize (this, Text);
			}

			private void ToolTipWindow_VisibleChanged(object sender, EventArgs e) {
				Control control = (Control)sender;

				if (control.is_visible) {
					XplatUI.SetTopmost(control.window.Handle, true);
				} else {
					XplatUI.SetTopmost(control.window.Handle, false);
				}
			}

			// UIA Framework
			private void OnUIAToolTip_VisibleChanged (object sender, EventArgs e)
			{
				if (Visible == false) 
					OnUnPopup (new PopupEventArgs (associated_control, associated_control, false, Size.Empty));
			}

			private void OnUnPopup (PopupEventArgs e)
			{
				PopupEventHandler eh = (PopupEventHandler) (Events [UnPopupEvent]);
				if (eh != null)
					eh (this, e);
			}


			#endregion	// ToolTipWindow Class Protected Instance Methods

			#region Internal Properties
			internal override bool ActivateOnShow { get { return false; } }
			#endregion

			// This Present is used when we are using the expicit Show methods for 2.0.
			// It will not reposition the window.
			public void PresentModal (Control control, string text)
			{
				if (IsDisposed)
					return;

				Size display_size;
				XplatUI.GetDisplaySize (out display_size);

				associated_control = control;

				Text = text;

				PopupEventArgs pea = new PopupEventArgs (control, control, false, Size.Empty);
				OnPopup (pea);

				if (pea.Cancel)
					return;

				Size = pea.ToolTipSize;

				Visible = true;
			}
		
			public void Present (Control control, string text)
			{
				if (IsDisposed)
					return;

				Size display_size;
				XplatUI.GetDisplaySize (out display_size);

				associated_control = control;

				Text = text;

				PopupEventArgs pea = new PopupEventArgs (control, control, false, Size.Empty);
				OnPopup (pea);
				
				if (pea.Cancel)
					return;
					
				Size size = pea.ToolTipSize;

				Width = size.Width;
				Height = size.Height;

				int cursor_w, cursor_h, hot_x, hot_y;
				XplatUI.GetCursorInfo (control.Cursor.Handle, out cursor_w, out cursor_h, out hot_x, out hot_y);
				Point loc = Control.MousePosition;
				loc.Y += (cursor_h - hot_y);

				if ((loc.X + Width) > display_size.Width)
					loc.X = display_size.Width - Width;

				if ((loc.Y + Height) > display_size.Height)
					loc.Y = Control.MousePosition.Y - Height - hot_y;
				
				Location = loc;
				Visible = true;
				BringToFront ();
			}


			#region Internal Events
			static object DrawEvent = new object ();
			static object PopupEvent = new object ();
	
			// UIA Framework
			static object UnPopupEvent = new object ();

			public event DrawToolTipEventHandler Draw {
				add { Events.AddHandler (DrawEvent, value); }
				remove { Events.RemoveHandler (DrawEvent, value); }
			}

			public event PopupEventHandler Popup {
				add { Events.AddHandler (PopupEvent, value); }
				remove { Events.RemoveHandler (PopupEvent, value); }
			}

			internal event PopupEventHandler UnPopup {
				add { Events.AddHandler (UnPopupEvent, value); }
				remove { Events.RemoveHandler (UnPopupEvent, value); }
			}
			#endregion
		}
		#endregion	// ToolTipWindow Class

		#region Public Constructors & Destructors
		public ToolTip() {

			// Defaults from MS
			is_active = true;
			automatic_delay = 500;
			autopop_delay = 5000;
			initial_delay = 500;
			re_show_delay = 100;
			show_always = false;
			back_color = SystemColors.Info;
			fore_color = SystemColors.InfoText;
			
			isBalloon = false;
			stripAmpersands = false;
			useAnimation = true;
			useFading = true;
			tooltip_strings = new Hashtable(5);
			controls = new ArrayList(5);

			tooltip_window = new ToolTipWindow();
			tooltip_window.MouseLeave += new EventHandler(control_MouseLeave);
			tooltip_window.Draw += new DrawToolTipEventHandler (tooltip_window_Draw);
			tooltip_window.Popup += new PopupEventHandler (tooltip_window_Popup);

			// UIA Framework: Static event handlers
			tooltip_window.UnPopup += delegate (object sender, PopupEventArgs args) {
				OnUnPopup (args);
			};
			UnPopup += new PopupEventHandler (OnUIAUnPopup);

			timer = new Timer();
			timer.Enabled = false;
			timer.Tick +=new EventHandler(timer_Tick);

		}


		#region UIA Framework: Events, Delegates and Methods
		// NOTE: 
		//	We are using Reflection to add/remove internal events.
		//      Class ToolTipListener uses the events.
		//
		//	- UIAUnPopup. Event used to generate ChildRemoved in ToolTip
		//	- UIAToolTipHookUp. Event used to keep track of associated controls
		//	- UIAToolTipUnhookUp. Event used to remove track of associated controls
		static object UnPopupEvent = new object ();

		internal event PopupEventHandler UnPopup {
			add { Events.AddHandler (UnPopupEvent, value); }
			remove { Events.RemoveHandler (UnPopupEvent, value); }
		}

		internal static event PopupEventHandler UIAUnPopup;
		internal static event ControlEventHandler UIAToolTipHookUp;
		internal static event ControlEventHandler UIAToolTipUnhookUp;

		internal Rectangle UIAToolTipRectangle {
			get { return tooltip_window.Bounds; }
		}

		internal static void OnUIAUnPopup (object sender, PopupEventArgs args)
		{
			if (UIAUnPopup != null)
				UIAUnPopup (sender, args);
		}

		internal static void OnUIAToolTipHookUp (object sender, ControlEventArgs args)
		{
			if (UIAToolTipHookUp != null)
				UIAToolTipHookUp (sender, args);
		}

		internal static void OnUIAToolTipUnhookUp (object sender, ControlEventArgs args)
		{
			if (UIAToolTipUnhookUp != null)
				UIAToolTipUnhookUp (sender, args);
		}

		#endregion

		public ToolTip(System.ComponentModel.IContainer cont) : this() {
			cont.Add (this);
		}

		~ToolTip() {
		}
		#endregion	// Public Constructors & Destructors

		#region Public Instance Properties
		[DefaultValue (true)]
		public bool Active {
			get {
				return is_active;
			}

			set {
				if (is_active != value) {
					is_active = value;

					if (tooltip_window.Visible) {
						tooltip_window.Visible = false;
						active_control = null;
					}
				}
			}
		}

		[DefaultValue (500)]
		[RefreshProperties (RefreshProperties.All)]
		public int AutomaticDelay {
			get {
				return automatic_delay;
			}

			set {
				if (automatic_delay != value) {
					automatic_delay = value;
					autopop_delay = automatic_delay * 10;
					initial_delay = automatic_delay;
					re_show_delay = automatic_delay / 5;
				}
			}
		}

		[RefreshProperties (RefreshProperties.All)]
		public int AutoPopDelay {
			get {
				return autopop_delay;
			}

			set {
				if (autopop_delay != value) {
					autopop_delay = value;
				}
			}
		}

		[DefaultValue ("Color [Info]")]
		public Color BackColor {
			get { return this.back_color; }
			set { this.back_color = value; tooltip_window.BackColor = value; }
		}

		[DefaultValue ("Color [InfoText]")]
		public Color ForeColor
		{
			get { return this.fore_color; }
			set { this.fore_color = value; tooltip_window.ForeColor = value; }
		}

		[RefreshProperties (RefreshProperties.All)]
		public int InitialDelay {
			get {
				return initial_delay;
			}

			set {
				if (initial_delay != value) {
					initial_delay = value;
				}
			}
		}

		[DefaultValue (false)]
		public bool OwnerDraw {
			get { return this.owner_draw; }
			set { this.owner_draw = value; }
		}

		[RefreshProperties (RefreshProperties.All)]
		public int ReshowDelay {
			get {
				return re_show_delay;
			}

			set {
				if (re_show_delay != value) {
					re_show_delay = value;
				}
			}
		}

		[DefaultValue (false)]
		public bool ShowAlways {
			get {
				return show_always;
			}

			set {
				if (show_always != value) {
					show_always = value;
				}
			}
		}


		[DefaultValue (false)]
		public bool IsBalloon {
			get { return isBalloon; }
			set { isBalloon = value; }
		}

		[Browsable (true)]
		[DefaultValue (false)]
		public bool StripAmpersands {
			get { return stripAmpersands; }
			set { stripAmpersands = value; }
		}

		[Localizable (false)]
		[Bindable (true)]
		[TypeConverter (typeof (StringConverter))]
		[DefaultValue (null)]
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		[DefaultValue (ToolTipIcon.None)]
		public ToolTipIcon ToolTipIcon {
			get { return this.tool_tip_icon; }
			set {
				switch (value) {
					case ToolTipIcon.None:
						tooltip_window.icon = null;
						break;
					case ToolTipIcon.Error:
						tooltip_window.icon = SystemIcons.Error;
						break;
					case ToolTipIcon.Warning:
						tooltip_window.icon = SystemIcons.Warning;
						break;
					case ToolTipIcon.Info:
						tooltip_window.icon = SystemIcons.Information;
						break;
				}

				tool_tip_icon = value;
		       	}
		}
		
		[DefaultValue ("")]
		public string ToolTipTitle {
			get { return tooltip_window.title; }
			set {
			       if (value == null)
				       value = String.Empty;
			       
			       tooltip_window.title = value; 
			}
		}
		
		[Browsable (true)]
		[DefaultValue (true)]
		public bool UseAnimation {
			get { return useAnimation; }
			set { useAnimation = value; }
		}

		[Browsable (true)]
		[DefaultValue (true)]
		public bool UseFading {
			get { return useFading; }
			set { useFading = value; }
		}

		#endregion	// Public Instance Properties

		#region Protected Properties
		protected virtual CreateParams CreateParams
		{
			get
			{
				CreateParams cp = new CreateParams ();

				cp.Style = 2;

				return cp;
			}
		}
		#endregion

		#region Public Instance Methods
		public bool CanExtend(object target) {
			return false;
		}

		[Editor ("System.ComponentModel.Design.MultilineStringEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[Localizable (true)]
		[DefaultValue ("")]
		public string GetToolTip (Control control)
		{
			string tooltip = (string)tooltip_strings[control];
			if (tooltip == null)
				return "";
			return tooltip;
		}

		public void RemoveAll() {
			tooltip_strings.Clear();
			//UIA Framework: ToolTip isn't associated anymore
			foreach (Control control in controls)
				OnUIAToolTipUnhookUp (this, new ControlEventArgs (control));

			controls.Clear();
		}

		public void SetToolTip(Control control, string caption) {
			// UIA Framework
			OnUIAToolTipHookUp (this, new ControlEventArgs (control));
			tooltip_strings[control] = caption;

			// no need for duplicates
			if (!controls.Contains(control)) {
				control.MouseEnter += new EventHandler(control_MouseEnter);
				control.MouseMove += new MouseEventHandler(control_MouseMove);
				control.MouseLeave += new EventHandler(control_MouseLeave);
				control.MouseDown += new MouseEventHandler (control_MouseDown);
				controls.Add(control);
			}
			
			// if SetToolTip is called from a control and the mouse is currently over that control,
			// make sure that tooltip_window.Text gets updated if it's being shown,
			// or show the tooltip for it if is not
			if (active_control == control && caption != null && state == TipState.Show) {
				Size size = ThemeEngine.Current.ToolTipSize(tooltip_window, caption);
				tooltip_window.Width = size.Width;
				tooltip_window.Height = size.Height;
				tooltip_window.Text = caption;
				timer.Stop ();
				timer.Start ();
			} else if (control.IsHandleCreated && MouseInControl (control, false))
				ShowTooltip (control);
		}

		public override string ToString() {
			return base.ToString() + " InitialDelay: " + initial_delay + ", ShowAlways: " + show_always;
		}

		public void Show (string text, IWin32Window window)
		{
			Show (text, window, 0);
		}

		public void Show (string text, IWin32Window window, int duration)
		{
			if (window == null)
				throw new ArgumentNullException ("window");
			if (duration < 0)
				throw new ArgumentOutOfRangeException ("duration", "duration cannot be less than zero");

			if (!Active)
				return;
				
			timer.Stop ();
			
			Control c = (Control)window;

			XplatUI.SetOwner (tooltip_window.Handle, c.TopLevelControl.Handle);
			
			// If the mouse is in the requested window, use that position
			// Else, center in the requested window
			if (c.ClientRectangle.Contains (c.PointToClient (Control.MousePosition))) {
				tooltip_window.Location = Control.MousePosition;
				tooltip_strings[c] = text;
				HookupControlEvents (c);
			}
			else
				tooltip_window.Location = c.PointToScreen (new Point (c.Width / 2, c.Height / 2));
			
			// We need to hide our tooltip if the form loses focus, is closed, or is minimized
			HookupFormEvents ((Form)c.TopLevelControl);
			
			tooltip_window.PresentModal ((Control)window, text);
			
			state = TipState.Show;
			
			if (duration > 0) {
				timer.Interval = duration;
				timer.Start ();
			}
		}
		
		public void Show (string text, IWin32Window window, Point point)
		{
			Show (text, window, point, 0);
		}

		public void Show (string text, IWin32Window window, int x, int y)
		{
			Show (text, window, new Point (x, y), 0);
		}
		
		public void Show (string text, IWin32Window window, Point point, int duration)
		{
			if (window == null)
				throw new ArgumentNullException ("window");
			if (duration < 0)
				throw new ArgumentOutOfRangeException ("duration", "duration cannot be less than zero");

			if (!Active)
				return;

			timer.Stop ();

			Control c = (Control)window;
			
			Point display_point = c.PointToScreen (Point.Empty);
			display_point.X += point.X;
			display_point.Y += point.Y;

			XplatUI.SetOwner (tooltip_window.Handle, c.TopLevelControl.Handle);

			// We need to hide our tooltip if the form loses focus, is closed, or is minimized
			HookupFormEvents ((Form)c.TopLevelControl);

			tooltip_window.Location = display_point;
			tooltip_window.PresentModal ((Control)window, text);

			state = TipState.Show;
			
			if (duration > 0) {
				timer.Interval = duration;
				timer.Start ();
			}
		}
		
		public void Show (string text, IWin32Window window, int x, int y, int duration)
		{
			Show (text, window, new Point (x, y), duration);
		}
		
		public void Hide (IWin32Window win)
 		{
			timer.Stop ();
			state = TipState.Initial;

			UnhookFormEvents ();
			tooltip_window.Visible = false;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void Dispose(bool disposing) {
			// call the base impl first to avoid conflicts with any parent's events
			base.Dispose (disposing);

			if (disposing) {
				// Mop up the mess; or should we wait for the GC to kick in?
				timer.Stop();
				timer.Dispose();

				// Not sure if we should clean up tooltip_window
				tooltip_window.Dispose();

				tooltip_strings.Clear();
				
				//UIA Framework: ToolTip isn't associated anymore
				foreach (Control control in controls)
					OnUIAToolTipUnhookUp (this, new ControlEventArgs (control));
				controls.Clear();
			}
		}

		protected void StopTimer ()
		{
			timer.Stop ();
		}
		#endregion	// Protected Instance Methods

		internal enum TipState {
			Initial,
			Show,
			Down
		}

		TipState state = TipState.Initial;

		#region Private Methods

		private void HookupFormEvents (Form form)
		{
			hooked_form = form;

			form.Deactivate += new EventHandler (Form_Deactivate);
			form.Closed += new EventHandler (Form_Closed);
			form.Resize += new EventHandler (Form_Resize);
		}

		private void HookupControlEvents (Control control)
		{
			if (!controls.Contains (control)) {
				control.MouseEnter += new EventHandler (control_MouseEnter);
				control.MouseMove += new MouseEventHandler (control_MouseMove);
				control.MouseLeave += new EventHandler (control_MouseLeave);
				control.MouseDown += new MouseEventHandler (control_MouseDown);
				controls.Add (control);
			}
		}

		private void UnhookControlEvents (Control control)
		{
			control.MouseEnter -= new EventHandler (control_MouseEnter);
			control.MouseMove -= new MouseEventHandler (control_MouseMove);
			control.MouseLeave -= new EventHandler (control_MouseLeave);
			control.MouseDown -= new MouseEventHandler (control_MouseDown);
		}
		private void UnhookFormEvents ()
		{
			if (hooked_form == null)
				return;

			hooked_form.Deactivate -= new EventHandler (Form_Deactivate);
			hooked_form.Closed -= new EventHandler (Form_Closed);
			hooked_form.Resize -= new EventHandler (Form_Resize);

			hooked_form = null;
		}


		private void Form_Resize (object sender, EventArgs e)
		{
			Form f = (Form)sender;

			if (f.WindowState == FormWindowState.Minimized)
				tooltip_window.Visible = false;
		}

		private void Form_Closed (object sender, EventArgs e)
		{
			tooltip_window.Visible = false;
		}

		private void Form_Deactivate (object sender, EventArgs e)
		{
			tooltip_window.Visible = false;
		}

		internal void Present (Control control, string text)
		{
			tooltip_window.Present (control, text);
		}
		
		private void control_MouseEnter (object sender, EventArgs e) 
		{
			ShowTooltip (sender as Control);
		}

		private void ShowTooltip (Control control) 
		{
			last_control = control;

			// Whatever we're displaying right now, we don't want it anymore
			tooltip_window.Visible = false;
			timer.Stop();
			state = TipState.Initial;

			if (!is_active)
				return;

			// ShowAlways controls whether the controls in non-active forms
			// can display its tooltips, even if they are not current active control.
			if (!show_always && control.FindForm () != Form.ActiveForm)
				return;

			string text = (string)tooltip_strings[control];
			if (text != null && text.Length > 0) {
				if (active_control == null) {
					timer.Interval = Math.Max (initial_delay, 1);
				} else {
					timer.Interval = Math.Max (re_show_delay, 1);
				}

				active_control = control;
				timer.Start ();
			}
		}

		private void timer_Tick(object sender, EventArgs e) {
			timer.Stop();

			switch (state) {
			case TipState.Initial:
				if (active_control == null)
					return;
				tooltip_window.Present (active_control, (string)tooltip_strings[active_control]);
				state = TipState.Show;
				timer.Interval = autopop_delay;
				timer.Start();
				break;

			case TipState.Show:
				tooltip_window.Visible = false;
				state = TipState.Down;
				break;

			default:
				throw new Exception ("Timer shouldn't be running in state: " + state);
			}
		}

		private void tooltip_window_Popup (object sender, PopupEventArgs e)
		{
			e.ToolTipSize = ThemeEngine.Current.ToolTipSize (tooltip_window, tooltip_window.Text);
			OnPopup (e);
		}

		private void tooltip_window_Draw (object sender, DrawToolTipEventArgs e)
		{
			if (OwnerDraw)
				OnDraw (e);
			else
				ThemeEngine.Current.DrawToolTip (e.Graphics, e.Bounds, tooltip_window);
		}
		
		private bool MouseInControl (Control control, bool fuzzy) {
			Point	m;
			Point	c;
			Size	cw;

			if (control == null) {
				return false;
			}

			m = Control.MousePosition;
			c = new Point(control.Bounds.X, control.Bounds.Y);
			if (control.Parent != null) {
				c = control.Parent.PointToScreen(c);
			}
			cw = control.ClientSize;


			Rectangle rect = new Rectangle (c, cw);
			
			//
			// We won't get mouse move events on all platforms with the exact same
			// frequency, so cheat a bit.
			if (fuzzy)
				rect.Inflate (2, 2);

			return rect.Contains (m);
		}

		private void control_MouseLeave(object sender, EventArgs e) 
		{
			timer.Stop ();

			active_control = null;
			tooltip_window.Visible = false;

			if (last_control == sender)
				last_control = null;
		}


		void control_MouseDown (object sender, MouseEventArgs e)
		{
			timer.Stop();

			active_control = null;
			tooltip_window.Visible = false;
			
			if (last_control == sender)
				last_control = null;
		}

		private void control_MouseMove(object sender, MouseEventArgs e) {
			if (state != TipState.Down) {
				timer.Stop();
				timer.Start();
			}
		}

		internal void OnDraw (DrawToolTipEventArgs e)
		{
			DrawToolTipEventHandler eh = (DrawToolTipEventHandler)(Events[DrawEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void OnPopup (PopupEventArgs e)
		{
			PopupEventHandler eh = (PopupEventHandler) (Events [PopupEvent]);
			if (eh != null)
				eh (this, e);
		}

		internal void OnUnPopup (PopupEventArgs e)
		{
			PopupEventHandler eh = (PopupEventHandler) (Events [UnPopupEvent]);
			if (eh != null)
				eh (this, e);
		}
		
		internal bool Visible {
			get { return tooltip_window.Visible; }
		}
		#endregion	// Private Methods

		#region Events
		static object PopupEvent = new object ();
		static object DrawEvent = new object ();
		
		public event PopupEventHandler Popup {
			add { Events.AddHandler (PopupEvent, value); }
			remove { Events.RemoveHandler (PopupEvent, value); }
		}

		public event DrawToolTipEventHandler Draw {
			add { Events.AddHandler (DrawEvent, value); }
			remove { Events.RemoveHandler (DrawEvent, value); }
		}
		#endregion
	}
}
