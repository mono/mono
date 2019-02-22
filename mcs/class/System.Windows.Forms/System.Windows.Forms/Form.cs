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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace System.Windows.Forms {
	[DesignerCategory("Form")]
	[DesignTimeVisible(false)]
	[Designer("System.Windows.Forms.Design.FormDocumentDesigner, " + Consts.AssemblySystem_Design, typeof(IRootDesigner))]
	[DefaultEvent("Load")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[InitializationEvent ("Load")]
	[ComVisible (true)]
	[ToolboxItemFilter ("System.Windows.Forms.Control.TopLevel")]
	[ToolboxItem(false)]
	public class Form : ContainerControl {
		#region Local Variables
		internal bool			closing;
		private bool			closed;
		FormBorderStyle			form_border_style;
		private bool			is_active;
		private bool			autoscale;
		private Size			clientsize_set;
		private Size			autoscale_base_size;
		private bool			allow_transparency;
		private static Icon		default_icon;
		internal bool			is_modal;
		internal FormWindowState	window_state;
		private bool			control_box;
		private bool			minimize_box;
		private bool			maximize_box;
		private bool			help_button;
		private bool			show_in_taskbar;
		private bool			topmost;
		private IButtonControl		accept_button;
		private IButtonControl		cancel_button;
		private DialogResult		dialog_result;
		private FormStartPosition	start_position;
		private Form			owner;
		private Form.ControlCollection	owned_forms;
		private MdiClient		mdi_container;
		internal InternalWindowManager	window_manager;
		private Form			mdi_parent;
		private bool			key_preview;
		private MainMenu		menu;
		private	Icon			icon;
		private Size			maximum_size;
		private Size			minimum_size;
		private Size			minimum_auto_size;
		private SizeGripStyle		size_grip_style;
		private SizeGrip		size_grip;
		private Rectangle		maximized_bounds;
		private Rectangle		default_maximized_bounds;
		private double			opacity;
		internal ApplicationContext	context;
		Color				transparency_key;
		private bool			is_loaded;
		internal int			is_changing_visible_state;
		internal bool			has_been_visible;
		private bool			shown_raised;
		private bool			close_raised;
		private bool			is_clientsize_set;
		internal bool			suppress_closing_events;
		internal bool			waiting_showwindow; // for XplatUIX11
		private bool			is_minimizing;
		private bool			show_icon = true;
		private MenuStrip		main_menu_strip;
		private bool			right_to_left_layout;
		private Rectangle		restore_bounds;
		private bool			autoscale_base_size_set;
		internal ArrayList disabled_by_showdialog = new ArrayList();
		internal static ArrayList modal_dialogs = new ArrayList();
		#endregion	// Local Variables

		#region Private & Internal Methods
		static Form ()
		{
			default_icon = ResourceImageLoader.GetIcon ("mono.ico");
		}

		internal bool IsLoaded {
			get { return is_loaded; }
		}

		internal bool IsActive {
			get {
				return is_active;
			}
			set {
				if (is_active == value || IsRecreating) {
					return;
				}
				
				is_active = value;
				if (is_active) {
					Application.AddForm (this);
					OnActivated (EventArgs.Empty);
				} else {
					OnDeactivate (EventArgs.Empty);
				}
			}
		}

		// warning: this is only hooked up when an mdi container is created.
		private void ControlAddedHandler (object sender, ControlEventArgs e)
		{
			if (mdi_container != null) {
				mdi_container.SendToBack ();
			}
		}

		// Convenience method for fire BOTH OnClosing and OnFormClosing events
		// Returns the value of Cancel, so true means the Close was cancelled,
		// and you shouldn't close the form.
		internal bool FireClosingEvents (CloseReason reason, bool cancel)
		{
			CancelEventArgs cea = new CancelEventArgs (cancel);
			this.OnClosing (cea);
			
			FormClosingEventArgs fcea = new FormClosingEventArgs (reason, cea.Cancel);
			this.OnFormClosing (fcea);
			return fcea.Cancel;
		}

		// Convenience method for fire BOTH OnClosed and OnFormClosed events
		private void FireClosedEvents (CloseReason reason)
		{
			this.OnClosed (EventArgs.Empty);
			this.OnFormClosed (new FormClosedEventArgs (reason));
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override Rectangle GetScaledBounds (Rectangle bounds, SizeF factor, BoundsSpecified specified)
		{
			if ((specified & BoundsSpecified.Width) == BoundsSpecified.Width) {
				int border = Size.Width - ClientSize.Width;
				bounds.Width = (int)Math.Round ((bounds.Width - border) * factor.Width) + border;
			}
			if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height) {
				int border = Size.Height - ClientSize.Height;
				bounds.Height = (int)Math.Round ((bounds.Height - border) * factor.Height) + border;
			}

			return bounds;
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			return base.ProcessMnemonic (charCode);
		}
		
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
		}

		internal void OnActivatedInternal ()
		{
			OnActivated (EventArgs.Empty);
		}
		
		internal void OnDeactivateInternal ()
		{
			OnDeactivate (EventArgs.Empty);
		}

		internal override void UpdateWindowText ()
		{
			if (!IsHandleCreated) {
				return;
			}
			
			if (shown_raised) {
				/* we need to call .SetWindowStyle here instead of just .Text
				   because the presence/absence of Text (== "" or not) can cause
				   other window style things to appear/disappear */
				XplatUI.SetWindowStyle (window.Handle, CreateParams);
			}
			
			XplatUI.Text (Handle, Text.Replace (Environment.NewLine, string.Empty));
		}
		
		internal void SelectActiveControl ()
		{
			if (this.IsMdiContainer) {
				mdi_container.SendFocusToActiveChild ();
				return;
			}
				
			if (this.ActiveControl == null) {
				bool visible;

				// This visible hack is to work around CanSelect always being false if one of the parents
				// is not visible; and we by default create Form invisible...
				visible = this.is_visible;
				this.is_visible = true;

				if (SelectNextControl (this, true, true, true, true) == false) {
					Select (this);
				}

				this.is_visible = visible;
			} else {
				Select (ActiveControl);
			}
		}
		
		private new void UpdateSizeGripVisible ()
		{
			// Following link explains when to show size grip:
			// http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=138687&SiteID=1
			// if SizeGripStyle.Auto, only shown if form is shown using ShowDialog and is sizable
			// if SizeGripStyle.Show, only shown if form is sizable
			
			bool show = false;
			
			switch (size_grip_style) {
			case SizeGripStyle.Auto:
				show = is_modal && (form_border_style == FormBorderStyle.Sizable || form_border_style == FormBorderStyle.SizableToolWindow);
				break;
			case SizeGripStyle.Hide:
				show = false;
				break;
			case SizeGripStyle.Show:
				show = (form_border_style == FormBorderStyle.Sizable || form_border_style == FormBorderStyle.SizableToolWindow);
				break;
			}
			
			if (!show) {
				if (size_grip != null && size_grip.Visible)
					size_grip.Visible = false;
			} else {
				if (size_grip == null) {
					size_grip = new SizeGrip (this);
					size_grip.Virtual = true;
					size_grip.FillBackground = false;
				}
				size_grip.Visible = true;
			}
		}
		
		internal void ChangingParent (Control new_parent)
		{
			if (IsMdiChild) {
				return;
			}
			
			bool recreate_necessary = false;
			
			if (new_parent == null) {
				window_manager = null;
			} else if (new_parent is MdiClient) {
				window_manager = new MdiWindowManager (this, (MdiClient) new_parent);
			} else {
				window_manager = new FormWindowManager (this);
				recreate_necessary = true;
			}
			
			if (recreate_necessary) {
				if (IsHandleCreated) {
					if (new_parent != null && new_parent.IsHandleCreated) {
						RecreateHandle ();
					} else {
						DestroyHandle ();
					}
				}
			} else {
				if (IsHandleCreated) {
					IntPtr new_handle = IntPtr.Zero;
					if (new_parent != null && new_parent.IsHandleCreated) {
						new_handle = new_parent.Handle;
					}
					XplatUI.SetParent (Handle, new_handle);
				}
			}
		
			if (window_manager != null) {
				window_manager.UpdateWindowState (window_state, window_state, true);
			}
		}

		internal override bool FocusInternal (bool skip_check)
		{
			if (IsMdiChild) {
				// MS always creates handles when Focus () is called for mdi clients.
				if (!IsHandleCreated)
					CreateHandle ();
			} 
			return base.FocusInternal (skip_check);
		}
		#endregion	// Private & Internal Methods

		#region Public Classes
		[ComVisible (false)]
		public new class ControlCollection : Control.ControlCollection {
			Form	form_owner;

			public ControlCollection(Form owner) : base(owner) {
				this.form_owner = owner;
			}

			public override void Add(Control value) {
				if (Contains (value))
					return;
				AddToList (value);
				((Form)value).owner=form_owner;
			}

			public override void Remove(Control value) {
				((Form)value).owner = null;
				base.Remove (value);
			}
		}
		#endregion	// Public Classes

		#region Public Constructor & Destructor
		public Form ()
		{
			SizeF current_scale = GetAutoScaleSize (Font);

			autoscale = true;
			autoscale_base_size = new Size ((int)Math.Round (current_scale.Width), (int)Math.Round(current_scale.Height));
			allow_transparency = false;
			closing = false;
			is_modal = false;
			dialog_result = DialogResult.None;
			start_position = FormStartPosition.WindowsDefaultLocation;
			form_border_style = FormBorderStyle.Sizable;
			window_state = FormWindowState.Normal;
			key_preview = false;
			opacity = 1D;
			menu = null;
			icon = default_icon;
			minimum_size = Size.Empty;
			maximum_size = Size.Empty;
			clientsize_set = Size.Empty;
			control_box = true;
			minimize_box = true;
			maximize_box = true;
			help_button = false;
			show_in_taskbar = true;
			is_visible = false;
			is_toplevel = true;
			size_grip_style = SizeGripStyle.Auto;
			maximized_bounds = Rectangle.Empty;
			default_maximized_bounds = Rectangle.Empty;
			owned_forms = new Form.ControlCollection(this);
			transparency_key = Color.Empty;
			CreateDockPadding ();
			InternalClientSize = new Size (this.Width - (SystemInformation.FrameBorderSize.Width * 2), this.Height - (SystemInformation.FrameBorderSize.Height * 2) - SystemInformation.CaptionHeight);
			restore_bounds = Bounds;
		}
		#endregion // Public Constructor & Destructor

		#region Public Static Properties (with helper functions)

		public static Form ActiveForm {
			get {
				Control ctrl = FromHandle (XplatUI.GetActive ());

				while (!(ctrl == null || Form.IsVisibleAndNotClosedForm (ctrl))) {
					ctrl = ctrl.Parent;
				}

				return (Form) ctrl;  // null or Form
			}
		}

		private static bool IsVisibleAndNotClosedForm (Control ctrl)
		{
			return (ctrl is Form form) && !form.closed && form.Visible;
		}

		#endregion	// Public Static Properties

		#region Public Instance Properties
		[DefaultValue(null)]
		public IButtonControl AcceptButton {
			get {
				return accept_button;
			}

			set {
				if (accept_button != null)
					accept_button.NotifyDefault (false);

				accept_button = value;
				if (accept_button != null)
					accept_button.NotifyDefault (true);

				CheckAcceptButton ();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowTransparency {
			get {
				return allow_transparency;
			}

			set {
				if (value == allow_transparency) {
					return;
				}

				allow_transparency = value;

				if (value) {
					if (IsHandleCreated) {
						if ((XplatUI.SupportsTransparency() & TransparencySupport.Set) != 0) {
							XplatUI.SetWindowTransparency(Handle, Opacity, TransparencyKey);
						}
					} else {
						UpdateStyles(); // Remove the WS_EX_LAYERED style
					}
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Obsolete ("This property has been deprecated in favor of AutoScaleMode.")]
		[MWFCategory("Layout")]
		public bool AutoScale {
			get {
				return autoscale;
			}

			set {
				if (value)
					AutoScaleMode = AutoScaleMode.None;

				autoscale = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Localizable(true)]
		[Browsable(false)]
		public virtual Size AutoScaleBaseSize {
			get {
				return autoscale_base_size;
			}
			[MonoTODO ("Setting this is probably unintentional and can cause Forms to be improperly sized.  See http://www.mono-project.com/FAQ:_Winforms#My_forms_are_sized_improperly for details.")]
			set {
				autoscale_base_size = value;
				autoscale_base_size_set = true;
			}
		}

		[Localizable(true)]
		public override bool AutoScroll {
			get {
				return base.AutoScroll;
			}
			set {
				base.AutoScroll = value;
			}
		}

		internal bool ShouldSerializeAutoScroll ()
		{
			return this.AutoScroll != false;
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { 
				if (base.AutoSize != value) {
					base.AutoSize = value;
					if (!value) {
						minimum_auto_size = Size.Empty;
						UpdateMinMax ();
					}
					PerformLayout (this, "AutoSize");
				}
			}
		}

		internal bool ShouldSerializeAutoSize ()
		{
			return this.AutoSize != false;
		}
		
		[Browsable (true)]
		[Localizable (true)]
		[DefaultValue (AutoSizeMode.GrowOnly)]
		public AutoSizeMode AutoSizeMode {
			get { return base.GetAutoSizeMode (); }
			set { 
				if (base.GetAutoSizeMode () != value) {
					if (!Enum.IsDefined (typeof (AutoSizeMode), value))
						throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for AutoSizeMode", value));

					base.SetAutoSizeMode (value);
					PerformLayout (this, "AutoSizeMode");
				}
			}
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public override AutoValidate AutoValidate {
			get { return base.AutoValidate; }
			set { base.AutoValidate = value; }
		}

		public override Color BackColor {
			get {
				/* we don't let parents override our
				 default background color for forms.
				 this fixes the default color for mdi
				 children. */
				if (background_color.IsEmpty)
					return DefaultBackColor;
				else
					return background_color;
			}
			set {
				base.BackColor = value;
			}
		}

		[DefaultValue(null)]
		public IButtonControl CancelButton {
			get {
				return cancel_button;
			}

			set {
				cancel_button = value;
				if (cancel_button != null && cancel_button.DialogResult == DialogResult.None)
					cancel_button.DialogResult = DialogResult.Cancel;
			}
		}

		// new property so we can change the DesignerSerializationVisibility
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Localizable(true)]
		public new Size ClientSize {
			get { return base.ClientSize; }
			set {
				is_clientsize_set = true;
				base.ClientSize = value;
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool ControlBox {
			get {
				return control_box;
			}

			set {
				if (control_box != value) {
					control_box = value;
					UpdateStyles();
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Rectangle DesktopBounds {
			get {
				return new Rectangle(Location, Size);
			}

			set {
				Bounds = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Point DesktopLocation {
			get {
				return Location;
			}

			set {
				Location = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DialogResult DialogResult {
			get {
				return dialog_result;
			}

			set {
				if (value < DialogResult.None || value > DialogResult.No)
					throw new InvalidEnumArgumentException ("value", (int) value, 
							typeof (DialogResult));

				dialog_result = value;
				if (dialog_result != DialogResult.None && is_modal)
					RaiseCloseEvents (false, false); // .Net doesn't send WM_CLOSE here.
			}
		}

		[DefaultValue(FormBorderStyle.Sizable)]
		[DispId(-504)]
		[MWFCategory("Appearance")]
		public FormBorderStyle FormBorderStyle {
			get {
				return form_border_style;
			}
			set {
				form_border_style = value;

				if (window_manager == null) {
					if (IsHandleCreated) {
						XplatUI.SetBorderStyle(window.Handle, form_border_style);
					}
				} else {
					window_manager.UpdateBorderStyle (value);
				}

				Size current_client_size = ClientSize;
				UpdateStyles();
				
				if (this.IsHandleCreated) {
					this.Size = InternalSizeFromClientSize (current_client_size);
					XplatUI.InvalidateNC (this.Handle);
				} else if (is_clientsize_set) {
					this.Size = InternalSizeFromClientSize (current_client_size);
				}
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Window Style")]
		public bool HelpButton {
			get {
				return help_button;
			}

			set {
				if (help_button != value) {
					help_button = value;
					UpdateStyles();
				}
			}
		}

		[Localizable(true)]
		[AmbientValue(null)]
		[MWFCategory("Window Style")]
		public Icon Icon {
			get {
				return icon;
			}

			set {
				if (value == null)
					value = default_icon;
				if (icon == value)
					return;
				icon = value;
				if (IsHandleCreated)
					XplatUI.SetIcon (Handle, icon);
			}
		}

		internal bool ShouldSerializeIcon ()
		{
			return this.Icon != default_icon;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsMdiChild {
			get {
				return mdi_parent != null;
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Window Style")]
		public bool IsMdiContainer {
			get {
				return mdi_container != null;
			}

			set {
				if (value && mdi_container == null) {
					mdi_container = new MdiClient ();
					Controls.Add(mdi_container);
					ControlAdded += new ControlEventHandler (ControlAddedHandler);
					mdi_container.SendToBack ();
					mdi_container.SetParentText (true);
				} else if (!value && mdi_container != null) {
					Controls.Remove(mdi_container);
					mdi_container = null;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form ActiveMdiChild {
			get {
				if (!IsMdiContainer)
					return null;
				return (Form) mdi_container.ActiveMdiChild;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool IsRestrictedWindow {
			get {
				return false;
			}
		}

		[DefaultValue(false)]
		public bool KeyPreview {
			get {
				return key_preview;
			}

			set {
				key_preview = value;
			}
		}

		[DefaultValue (null)]
		[TypeConverter (typeof (ReferenceConverter))]
		public MenuStrip MainMenuStrip {
			get { return this.main_menu_strip; }
			set { 
				if (this.main_menu_strip != value) {
					this.main_menu_strip = value;
					this.main_menu_strip.RefreshMdiItems ();
				}
			}
		}
		
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public new Padding Margin {
			get { return base.Margin; }
			set { base.Margin = value; }
		}

		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool MaximizeBox {
			get {
				return maximize_box;
			}
			set {
				if (maximize_box != value) {
					maximize_box = value;
					UpdateStyles();
				}
			}
		}

		[DefaultValue(typeof (Size),"0, 0")]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFCategory("Layout")]
		public override Size MaximumSize {
			get {
				return maximum_size;
			}

			set {
				if (maximum_size != value) {
					maximum_size = value;

					// If this is smaller than the min, adjust the min
					if (!minimum_size.IsEmpty) {
						if (maximum_size.Width <= minimum_size.Width)
							minimum_size.Width = maximum_size.Width;
						if (maximum_size.Height <= minimum_size.Height)
							minimum_size.Height = maximum_size.Height;
					}
						
					OnMaximumSizeChanged(EventArgs.Empty);
					if (IsHandleCreated) {
						UpdateMinMax();
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form[] MdiChildren {
			get {
				if (mdi_container != null)
					return mdi_container.MdiChildren;
				else
					return new Form[0];
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form MdiParent {
			get {
				return mdi_parent;
			}

			set {
				if (value == mdi_parent)
					return;

				if (value != null && !value.IsMdiContainer)
					throw new ArgumentException ("Form that was specified to be "
						+ "the MdiParent for this form is not an MdiContainer.");

				if (mdi_parent != null) {
					mdi_parent.MdiContainer.Controls.Remove (this);
				}

				if (value != null) {
					mdi_parent = value;
					if (window_manager == null) {
						window_manager = new MdiWindowManager (this, mdi_parent.MdiContainer);
					}
					
					mdi_parent.MdiContainer.Controls.Add (this);
					mdi_parent.MdiContainer.Controls.SetChildIndex (this, 0);
					
					if (IsHandleCreated)
						RecreateHandle ();
				} else if (mdi_parent != null) {
					mdi_parent = null;

					// Create a new window manager
					window_manager = null;
					FormBorderStyle = form_border_style;

					if (IsHandleCreated)
						RecreateHandle ();
				}
				is_toplevel = mdi_parent == null;
			}
		}

		internal MdiClient MdiContainer {
			get { return mdi_container; }
		}

		internal InternalWindowManager WindowManager {
			get { return window_manager; }
		}

		[Browsable (false)]
		[TypeConverter (typeof (ReferenceConverter))]
		[DefaultValue(null)]
		[MWFCategory("Window Style")]
		public MainMenu Menu {
			get {
				return menu;
			}

			set {
				if (menu != value) {
					menu = value;

					if (menu != null && !IsMdiChild) {
						menu.SetForm (this);

						if (IsHandleCreated) {
							XplatUI.SetMenu (window.Handle, menu);
						}

						if (clientsize_set != Size.Empty) {
							SetClientSizeCore(clientsize_set.Width, clientsize_set.Height);
						} else {
							UpdateBounds (bounds.X, bounds.Y, bounds.Width, bounds.Height, ClientSize.Width, ClientSize.Height - 
								ThemeEngine.Current.CalcMenuBarSize (DeviceContext, menu, ClientSize.Width));
						}
					} else
						UpdateBounds ();

					// UIA Framework Event: Menu Changed
					OnUIAMenuChanged (EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public MainMenu MergedMenu {
			get {
				if (!IsMdiChild || window_manager == null)
					return null;
				return ((MdiWindowManager) window_manager).MergedMenu;
			}
		}

		// This is the menu in display and being used because of merging this can
		// be different then the menu that is actually assosciated with the form
		internal MainMenu ActiveMenu {
			get {
				if (IsMdiChild)
					return null;

				if (IsMdiContainer && mdi_container.Controls.Count > 0 &&
						((Form) mdi_container.Controls [0]).WindowState == FormWindowState.Maximized) {
					MdiWindowManager wm = (MdiWindowManager) ((Form) mdi_container.Controls [0]).WindowManager;
					return wm.MaximizedMenu;
				}

				Form amc = ActiveMdiChild;
				if (amc == null || amc.Menu == null)
					return menu;
				return amc.MergedMenu;
			}
		}

		internal MdiWindowManager ActiveMaximizedMdiChild {
			get {
				Form child = ActiveMdiChild;
				if (child == null)
					return null;
				if (child.WindowManager == null || child.window_state != FormWindowState.Maximized)
					return null;
				return (MdiWindowManager) child.WindowManager;
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool MinimizeBox {
			get {
				return minimize_box;
			}
			set {
				if (minimize_box != value) {
					minimize_box = value;
					UpdateStyles();
				}
			}
		}

		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFCategory("Layout")]
		public override Size MinimumSize {
			get {
				return minimum_size;
			}

			set {
				if (minimum_size != value) {
					minimum_size = value;

					// If this is bigger than the max, adjust the max
					if (!maximum_size.IsEmpty) {
						if (minimum_size.Width >= maximum_size.Width)
							maximum_size.Width = minimum_size.Width;
						if (minimum_size.Height >= maximum_size.Height)
							maximum_size.Height = minimum_size.Height;
					}
					
					if ((Size.Width < value.Width) || (Size.Height < value.Height)) {
						Size = new Size(Math.Max(Size.Width, value.Width), Math.Max(Size.Height, value.Height));
					}
  

					OnMinimumSizeChanged(EventArgs.Empty);
					if (IsHandleCreated) {
						UpdateMinMax();
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Modal  {
			get {
				return is_modal;
			}
		}

		[DefaultValue(1D)]
		[TypeConverter(typeof(OpacityConverter))]
		[MWFCategory("Window Style")]
		public double Opacity {
			get {
				if (IsHandleCreated) {
					if ((XplatUI.SupportsTransparency () & TransparencySupport.Get) != 0)
						return XplatUI.GetWindowTransparency (Handle);
				}

				return opacity;
			}

			set {
				opacity = value;

				if (opacity < 0)
					opacity = 0;

				if (opacity > 1)
					opacity = 1;

				AllowTransparency = true;

				if (IsHandleCreated) {
					UpdateStyles();
					if ((XplatUI.SupportsTransparency () & TransparencySupport.Set) != 0)
						XplatUI.SetWindowTransparency(Handle, opacity, TransparencyKey);
				}
			}
		}
			

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form[] OwnedForms {
			get {
				Form[] form_list;

				form_list = new Form[owned_forms.Count];

				for (int i=0; i<owned_forms.Count; i++) {
					form_list[i] = (Form)owned_forms[i];
				}

				return form_list;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form Owner {
			get {
				return owner;
			}

			set {
				if (owner != value) {
					if (owner != null) {
						owner.RemoveOwnedForm(this);
					}
					owner = value;
					if (owner != null)
						owner.AddOwnedForm(this);
					if (IsHandleCreated) {
						if (owner != null && owner.IsHandleCreated) {
							XplatUI.SetOwner(this.window.Handle, owner.window.Handle);
						} else {
							XplatUI.SetOwner(this.window.Handle, IntPtr.Zero);
						}
					}
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Rectangle RestoreBounds {
			get { return restore_bounds; }
		}
		
		[Localizable (true)]
		[DefaultValue (false)]
		public virtual bool RightToLeftLayout {
			get { return this.right_to_left_layout; }
			set { this.right_to_left_layout = value; }
		}
		
		[DefaultValue (true)]
		public bool ShowIcon {
			get { return this.show_icon; }
			set {
				if (this.show_icon != value ) {
					this.show_icon = value;
					UpdateStyles ();
					
					if (IsHandleCreated) {
						XplatUI.SetIcon (this.Handle, value == true ? this.Icon : null);
						XplatUI.InvalidateNC (this.Handle);
					}
				}
			}
		}
	
		[DefaultValue(true)]
		[MWFCategory("Window Style")]
		public bool ShowInTaskbar {
			get {
				return show_in_taskbar;
			}
			set {
				if (show_in_taskbar != value) {
					show_in_taskbar = value;
					if (IsHandleCreated) {
						RecreateHandle();
					}
					UpdateStyles();
				}
			}
		}

		// new property so we can set the DesignerSerializationVisibility
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Localizable(false)]
		public new Size Size {
			get { return base.Size; }
			set { base.Size = value; }
		}

		[DefaultValue(SizeGripStyle.Auto)]
		[MWFCategory("Window Style")]
		public SizeGripStyle SizeGripStyle {
			get {
				return size_grip_style;
			}

			set {
				size_grip_style = value;
				UpdateSizeGripVisible ();
			}
		}

		[DefaultValue(FormStartPosition.WindowsDefaultLocation)]
		[Localizable(true)]
		[MWFCategory("Layout")]
		public FormStartPosition StartPosition {
			get {
				return start_position;
			}

			set {
				start_position = value;
			}
		}

		// new property so we can set EditorBrowsable to never
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int TabIndex {
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		[Browsable(false)]
		[DefaultValue (true)]
		[DispIdAttribute (-516)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool TopLevel {
			get {
				return GetTopLevel();
			}

			set {
				if (!value && IsMdiContainer)
					throw new ArgumentException ("MDI Container forms must be top level.");
				SetTopLevel(value);
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Window Style")]
		public bool TopMost {
			get {
				return topmost;
			}

			set {
				if (topmost != value) {
					topmost = value;
					if (IsHandleCreated)
						XplatUI.SetTopmost(window.Handle, value);

					// UIA Framework: Raises internal event
					OnUIATopMostChanged ();
				}
			}
		}

		[MWFCategory("Window Style")]
		public Color TransparencyKey {
			get {
				return transparency_key;
			}

			set {
				transparency_key = value;

				AllowTransparency = true;
				UpdateStyles();
				if (IsHandleCreated && (XplatUI.SupportsTransparency () & TransparencySupport.Set) != 0)
					XplatUI.SetWindowTransparency(Handle, Opacity, transparency_key);
			}
		}

		internal bool ShouldSerializeTransparencyKey ()
		{
			return this.TransparencyKey != Color.Empty;
		}

		[DefaultValue(FormWindowState.Normal)]
		[MWFCategory("Layout")]
		public FormWindowState WindowState {
			get {
				// Don't actually rely on the WM until we've been shown
				if (IsHandleCreated && shown_raised) {

					if (window_manager != null)
						return window_manager.GetWindowState ();

					FormWindowState new_state = XplatUI.GetWindowState(Handle);
					if (new_state != (FormWindowState)(-1))
						window_state = new_state;
				}

				return window_state;
			}

			set {
				FormWindowState old_state = window_state;
				window_state = value;
				if (IsHandleCreated && shown_raised) {

					if (window_manager != null) {
						window_manager.SetWindowState (old_state, value);
						return;
					}

					XplatUI.SetWindowState(Handle, value);
				}

				// UIA Framework: Raises internal event
				if (old_state != window_state) 
					OnUIAWindowStateChanged ();
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = new CreateParams ();

				if (Text != null)
					cp.Caption = Text.Replace (Environment.NewLine, string.Empty);
				
				cp.ClassName = XplatUI.GetDefaultClassName (GetType ());
				cp.ClassStyle = 0;
				cp.Style = 0;
				cp.ExStyle = 0;
				cp.Param = 0;
				cp.Parent = IntPtr.Zero;
				cp.menu = ActiveMenu;
				cp.control = this;

				if (((Parent != null || !TopLevel) && !IsMdiChild)) {
					// Parented forms and non-toplevel forms always gets the specified location, no matter what
					cp.X = Left;
					cp.Y = Top;
				} else {
					switch (start_position) {
					case FormStartPosition.Manual:
						cp.X = Left;
						cp.Y = Top;
						break;
					case FormStartPosition.CenterScreen:
						if (IsMdiChild) {
							cp.X = Math.Max ((MdiParent.mdi_container.ClientSize.Width - Width) / 2, 0);
							cp.Y = Math.Max ((MdiParent.mdi_container.ClientSize.Height - Height) / 2, 0);
						} else {
							cp.X = Math.Max ((Screen.PrimaryScreen.WorkingArea.Width - Width) / 2, 0);
							cp.Y = Math.Max ((Screen.PrimaryScreen.WorkingArea.Height - Height) / 2, 0);
						}
						break;
					case FormStartPosition.CenterParent:
					case FormStartPosition.WindowsDefaultBounds:
					case FormStartPosition.WindowsDefaultLocation:
						cp.X = int.MinValue;
						cp.Y = int.MinValue;
						break;
					}
				}
				cp.Width = Width;
				cp.Height = Height;

				cp.Style = (int)(WindowStyles.WS_CLIPCHILDREN);
				if (!Modal) {
					cp.WindowStyle |= WindowStyles.WS_CLIPSIBLINGS;
				}

				if (Parent != null && Parent.IsHandleCreated) {
					cp.Parent = Parent.Handle;
					cp.Style |= (int) WindowStyles.WS_CHILD;
				}

				if (IsMdiChild) {
					cp.Style |= (int)(WindowStyles.WS_CHILD | WindowStyles.WS_CAPTION);
					if (Parent != null) {
						cp.Parent = Parent.Handle;
					}

					cp.ExStyle |= (int) (WindowExStyles.WS_EX_WINDOWEDGE | WindowExStyles.WS_EX_MDICHILD);

					switch (FormBorderStyle) {
					case FormBorderStyle.None:
						break;
					case FormBorderStyle.FixedToolWindow:
					case FormBorderStyle.SizableToolWindow:
						cp.ExStyle |= (int) WindowExStyles.WS_EX_TOOLWINDOW;
						goto default;
					default:
						cp.Style |= (int) WindowStyles.WS_OVERLAPPEDWINDOW;
						break;
					}
					
				} else {
					switch (FormBorderStyle) {
						case FormBorderStyle.Fixed3D: {
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							cp.ExStyle |= (int)WindowExStyles.WS_EX_CLIENTEDGE; 
							break;
						}

						case FormBorderStyle.FixedDialog: {
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							cp.ExStyle |= (int)(WindowExStyles.WS_EX_DLGMODALFRAME | WindowExStyles.WS_EX_CONTROLPARENT);
							break;
						}

						case FormBorderStyle.FixedSingle: {
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							break;
						}

						case FormBorderStyle.FixedToolWindow: { 
							cp.Style |= (int)(WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER);
							cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW);
							break;
						}

						case FormBorderStyle.Sizable: {
							cp.Style |= (int)(WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME | WindowStyles.WS_CAPTION); 
							break;
						}

						case FormBorderStyle.SizableToolWindow: {
							cp.Style |= (int)(WindowStyles.WS_BORDER | WindowStyles.WS_THICKFRAME | WindowStyles.WS_CAPTION);
							cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW);
							break;
						}

						case FormBorderStyle.None: {
							break;
						}
					}
				}

				switch(window_state) {
					case FormWindowState.Maximized: {
						cp.Style |= (int)WindowStyles.WS_MAXIMIZE;
						break;
					}

					case FormWindowState.Minimized: {
						cp.Style |= (int)WindowStyles.WS_MINIMIZE;
						break;
					}
				}

				if (TopMost) {
					cp.ExStyle |= (int) WindowExStyles.WS_EX_TOPMOST;
				}

				if (ShowInTaskbar) {
					cp.ExStyle |= (int)WindowExStyles.WS_EX_APPWINDOW;
				}

				if (MaximizeBox) {
					cp.Style |= (int)WindowStyles.WS_MAXIMIZEBOX;
				}

				if (MinimizeBox) {
					cp.Style |= (int)WindowStyles.WS_MINIMIZEBOX;
				}

				if (ControlBox) {
					cp.Style |= (int)WindowStyles.WS_SYSMENU;
				}

				if (!this.show_icon) {
					cp.ExStyle |= (int)WindowExStyles.WS_EX_DLGMODALFRAME;
				}

				cp.ExStyle |= (int)WindowExStyles.WS_EX_CONTROLPARENT;

				if (HelpButton && !MaximizeBox && !MinimizeBox) {
					cp.ExStyle |= (int)WindowExStyles.WS_EX_CONTEXTHELP;
				}

				// bug 80775:
				//don't set WS_VISIBLE if we're changing visibility. We can't create forms visible, 
				//since we have to set the owner before making the form visible 
				//(otherwise Win32 will do strange things with task bar icons). 
				//The problem is that we set the internal is_visible to true before creating the control, 
				//so is_changing_visible_state is the only way of determining if we're 
				//in the process of creating the form due to setting Visible=true.
				//This works because SetVisibleCore explicitly makes the form visibile afterwards anyways.
				// bug 81957:
				//only do this when on Windows, since X behaves weirdly otherwise
				//modal windows appear below their parent/owner/ancestor.
				//(confirmed on several window managers, so it's not a wm bug).
				int p = (int) Environment.OSVersion.Platform;
				bool is_unix = (p == 128) || (p == 4) || (p == 6);
				if ((VisibleInternal && (is_changing_visible_state == 0 || is_unix)) || this.IsRecreating)
					cp.Style |= (int)WindowStyles.WS_VISIBLE;

				if (opacity < 1.0 || TransparencyKey != Color.Empty) {
					cp.ExStyle |= (int)WindowExStyles.WS_EX_LAYERED;
				}

				if (!is_enabled && context == null) {
					cp.Style |= (int)(WindowStyles.WS_DISABLED);
				}

				if (!ControlBox && Text == string.Empty) {
					cp.WindowStyle &= ~WindowStyles.WS_DLGFRAME;
				}
				
				return cp;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {
				return ImeMode.NoControl;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size (300, 300);
			}
		}

		protected Rectangle MaximizedBounds {
			get {
				if (maximized_bounds != Rectangle.Empty) {
					return maximized_bounds;
				}
				return default_maximized_bounds;
			}

			set {
				maximized_bounds = value;
				OnMaximizedBoundsChanged(EventArgs.Empty);
				if (IsHandleCreated) {
					UpdateMinMax();
				}
			}
		}
		
		[Browsable (false)]
		[MonoTODO ("Implemented for Win32, needs X11 implementation")]
		protected virtual bool ShowWithoutActivation {
			get { return false; }
		}
		#endregion	// Protected Instance Properties

		#region Public Static Methods
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete ("This method has been deprecated.  Use AutoScaleDimensions instead")]
		public static SizeF GetAutoScaleSize (Font font)
		{
			return XplatUI.GetAutoScaleSize(font);
		}

		#endregion	// Public Static Methods

		#region Public Instance Methods
						 
		public void Activate ()
		{
			if (IsHandleCreated) {
				if (IsMdiChild) {
					MdiParent.ActivateMdiChild (this);
				} else if (IsMdiContainer) {
					mdi_container.SendFocusToActiveChild ();
				} else {
					XplatUI.Activate(window.Handle);
				}
			}
		}

		public void AddOwnedForm(Form ownedForm) {
			if (!owned_forms.Contains(ownedForm)) {
				owned_forms.Add(ownedForm);
			}
			ownedForm.Owner = this;
		}

		public void Close () {
			if (IsDisposed)
				return;

			if (!IsHandleCreated) {
				base.Dispose ();
				return;
			}
 
			if (Menu != null)
				XplatUI.SetMenu (window.Handle, null);

			XplatUI.SendMessage(this.Handle, Msg.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);

			closed = true;
		}

		public void LayoutMdi(MdiLayout value) {
			if (mdi_container != null) {
				mdi_container.LayoutMdi(value);
			}
		}

		public void RemoveOwnedForm(Form ownedForm) {
			owned_forms.Remove(ownedForm);
		}

		public void SetDesktopBounds(int x, int y, int width, int height) {
			DesktopBounds = new Rectangle(x, y, width, height);
		}

		public void SetDesktopLocation(int x, int y) {
			DesktopLocation = new Point(x, y);
		}

		public void Show (IWin32Window owner)
		{
			if (owner == null)
				this.Owner = null;
			else
				this.Owner = Control.FromHandle (owner.Handle).TopLevelControl as Form;

			if (owner == this)
				throw new InvalidOperationException ("The 'owner' cannot be the form being shown.");

			if (TopLevelControl != this) {
				throw new InvalidOperationException ("Forms that are not top level"
					+ " forms cannot be displayed as a modal dialog. Remove the"
					+ " form from any parent form before calling Show.");
			}

			base.Show ();
		}

		public DialogResult ShowDialog() {
			return ShowDialog (null);
		}

		public DialogResult ShowDialog(IWin32Window owner) {
			Rectangle	area;
			bool		confined;
			IntPtr		capture_window;

			IWin32Window original_owner = owner;
			Form owner_to_be = null;

			if ((owner == null) && (Application.MWFThread.Current.Context != null)) {
				IntPtr active = XplatUI.GetActive ();
				if (active != IntPtr.Zero) {
					owner = Control.FromHandle (active) as Form;
				}
			}

			if (owner != null) {
				Control c = Control.FromHandle (owner.Handle);
				if (c != null)
					owner_to_be = c.TopLevelControl as Form;
			}

			if (owner_to_be == this) {
				// Don't let a null owner become self-referential.  This has been observed,
				// but the circumstances are unclear.
				if (original_owner == null) {
					owner = null;
					owner_to_be = null;
				}
				else {
					throw new ArgumentException ("Forms cannot own themselves or their owners.", "owner");
				}
			}

			if (is_modal) {
				throw new InvalidOperationException ("The form is already displayed as a modal dialog.");
			}

			if (Visible) {
				throw new InvalidOperationException ("Forms that are already "
					+ " visible cannot be displayed as a modal dialog. Set the"
					+ " form's visible property to false before calling"
					+ " ShowDialog.");
			}

			if (!Enabled) {
				throw new InvalidOperationException ("Forms that are not enabled"
					+ " cannot be displayed as a modal dialog. Set the form's"
					+ " enabled property to true before calling ShowDialog.");
			}

			if (TopLevelControl != this) {
				throw new InvalidOperationException ("Forms that are not top level"
					+ " forms cannot be displayed as a modal dialog. Remove the"
					+ " form from any parent form before calling ShowDialog.");
			}

			if (owner_to_be != null)
				this.owner = owner_to_be;
				
			// If our owner is topmost, we better be too, or else we'll show up under our owner
			if (this.owner != null && this.owner.TopMost)
				this.TopMost = true;
				
			#if broken
			// Can't do this, will screw us in the modal loop
			form_parent_window.Parent = this.owner;
			#endif

			// Release any captures
			XplatUI.GrabInfo(out capture_window, out confined, out area);
			if (capture_window != IntPtr.Zero) {
				XplatUI.UngrabWindow(capture_window);
			}

			var disable = new List<Form> ();
			foreach (Form form in Application.OpenForms)
				if (form.Enabled)
					disable.Add (form);
			foreach (Form form in disable){
				disabled_by_showdialog.Add (form);
				form.Enabled = false;
			}
			modal_dialogs.Add(this);

#if not
			// Commented out; we instead let the Visible=true inside the runloop create the control
			// otherwise setting DialogResult inside any of the events that are triggered by the
			// create will not actually cause the form to not be displayed.
			// Leaving this comment here in case there was an actual purpose to creating the control
			// in here.
			if (!IsHandleCreated) {
				CreateControl();
			}
#endif

			Application.RunLoop(true, new ApplicationContext(this));

			if (this.owner != null) {
				// Cannot use Activate(), it has a check for the current active window...
				XplatUI.Activate(this.owner.window.Handle);
			}
			
			if (IsHandleCreated) {
				DestroyHandle ();
			}

			if (DialogResult == DialogResult.None) {
				DialogResult = DialogResult.Cancel;
			}
			
			return DialogResult;
		}

		public override string ToString() {
			return GetType().FullName + ", Text: " + Text;
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public override bool ValidateChildren ()
		{
			return base.ValidateChildren ();
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public override bool ValidateChildren (ValidationConstraints validationConstraints)
		{
			return base.ValidateChildren (validationConstraints);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected void ActivateMdiChild(Form form) {
			if (!IsMdiContainer)
				return;
			mdi_container.ActivateChild (form);
			OnMdiChildActivate(EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void AdjustFormScrollbars(bool displayScrollbars) {
			base.AdjustFormScrollbars (displayScrollbars);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete ("This method has been deprecated")] // XXX what to use instead?
		protected void ApplyAutoScaling()
		{
			SizeF current_size_f = GetAutoScaleSize (Font);
			Size current_size = new Size ((int)Math.Round (current_size_f.Width), (int)Math.Round (current_size_f.Height));
			float	dx;
			float	dy;

			if (current_size == autoscale_base_size)
				return;

			if (Environment.GetEnvironmentVariable ("MONO_MWF_SCALING") == "disable"){
				return;
			}
			
			//
			// I tried applying the Fudge height factor from:
			// http://blogs.msdn.com/mharsh/archive/2004/01/25/62621.aspx
			// but it makes things larger without looking better.
			//
			if (current_size.Width != AutoScaleBaseSize.Width) {
				dx = (float)current_size.Width / AutoScaleBaseSize.Width + 0.08f;
			} else {
				dx = 1;
			}

			if (current_size.Height != AutoScaleBaseSize.Height) {
				dy = (float)current_size.Height / AutoScaleBaseSize.Height + 0.08f;
			} else {
				dy = 1;
			}

			Scale (dx, dy);
			
			AutoScaleBaseSize = current_size;
		}

		protected void CenterToParent() {
			Control	ctl;
			int	w;
			int	h;

			// MS creates the handle here.
			if (TopLevel) {
				if (!IsHandleCreated)
					CreateHandle ();
			}
			
			if (Width > 0) {
				w = Width;
			} else {
				w = DefaultSize.Width;
			}

			if (Height > 0) {
				h = Height;
			} else {
				h = DefaultSize.Height;
			}

			ctl = null;
			if (Parent != null) {
				ctl = Parent;
			} else if (owner != null) {
				ctl = owner;
			}

			if (owner != null) {
				this.Location = new Point(ctl.Left + ctl.Width / 2 - w /2, ctl.Top + ctl.Height / 2 - h / 2);
			}
		}

		protected void CenterToScreen() {
			int w;
			int h;

			// MS creates the handle here.
			if (TopLevel) {
				if (!IsHandleCreated)
					CreateHandle ();
			}
			
			if (Width > 0) {
				w = Width;
			} else {
				w = DefaultSize.Width;
			}

			if (Height > 0) {
				h = Height;
			} else {
				h = DefaultSize.Height;
			}

			Rectangle workingArea;
			if (Owner == null) {
				workingArea = Screen.FromPoint (MousePosition).WorkingArea;
			} else {
				workingArea = Screen.FromControl (Owner).WorkingArea;
			}
			this.Location = new Point (workingArea.Left + workingArea.Width / 2 - w / 2,
				workingArea.Top + workingArea.Height / 2 - h / 2);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override Control.ControlCollection CreateControlsInstance() {
			return base.CreateControlsInstance ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void CreateHandle() {
			base.CreateHandle ();

			if (!IsHandleCreated) {
				return;
			}
			
			UpdateBounds();

			if ((XplatUI.SupportsTransparency() & TransparencySupport.Set) != 0) {
				if (allow_transparency) {
					XplatUI.SetWindowTransparency(Handle, opacity, TransparencyKey);
				}
			}

			UpdateMinMax();
			
			if (show_icon && (FormBorderStyle != FormBorderStyle.FixedDialog) && (icon != null)) {
				XplatUI.SetIcon(window.Handle, icon);
			}

			if ((owner != null) && (owner.IsHandleCreated)) {
				XplatUI.SetOwner(window.Handle, owner.window.Handle);
			}

			if (topmost) {
				XplatUI.SetTopmost(window.Handle, topmost);
			}

			for (int i = 0; i < owned_forms.Count; i++) {
				if (owned_forms[i].IsHandleCreated)
					XplatUI.SetOwner(owned_forms[i].window.Handle, window.Handle);
			}
			
			if (window_manager != null) {
				if (IsMdiChild && VisibleInternal) {
					MdiWindowManager wm;
					// Loop through all the other mdi siblings and raise Deactivate events.
					if (MdiParent != null) {
						foreach (Form form in MdiParent.MdiChildren) {
							wm = form.window_manager as MdiWindowManager;
							if (wm != null && form != this) {
								// This will only raise deactivate once, and only if activate has
								// already been raised.
								wm.RaiseDeactivate ();
							}
						}
					}
					
					wm = window_manager as MdiWindowManager;
					wm.RaiseActivated ();

					// We need to tell everyone who may have just been deactivated to redraw their titlebar
					if (MdiParent != null)
						foreach (Form form in MdiParent.MdiChildren)
							if (form != this && form.IsHandleCreated)
								XplatUI.InvalidateNC (form.Handle);
				}
				
				if (window_state != FormWindowState.Normal) {
					window_manager.SetWindowState ((FormWindowState) int.MaxValue, window_state);
				}
				XplatUI.RequestNCRecalc (window.Handle);
			}

		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void DefWndProc(ref Message m) {
			base.DefWndProc (ref m);
		}

		protected override void Dispose(bool disposing)
		{
			if (owned_forms != null) {
				for (int i = 0; i < owned_forms.Count; i++)
					((Form)owned_forms[i]).Owner = null;

				owned_forms.Clear ();
			}
			Owner = null;
			base.Dispose (disposing);
			
			Application.RemoveForm (this);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnActivated(EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ActivatedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClosed(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ClosedEvent]);
			if (eh != null)
				eh (this, e);
		}

		// Consider calling FireClosingEvents instead of calling this directly.
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e) {
			CancelEventHandler eh = (CancelEventHandler)(Events [ClosingEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnCreateControl() {
			base.OnCreateControl ();

			if (menu != null) {
				XplatUI.SetMenu(window.Handle, menu);
			}

			OnLoadInternal (EventArgs.Empty);
			
			// Send initial location
			OnLocationChanged(EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDeactivate(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DeactivateEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged (e);
			
			if (!autoscale_base_size_set) {
				SizeF sizef = Form.GetAutoScaleSize (Font);
				autoscale_base_size = new Size ((int)Math.Round (sizef.Width), (int)Math.Round (sizef.Height));
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnHandleCreated(EventArgs e) {
			XplatUI.SetBorderStyle(window.Handle, form_border_style);
			base.OnHandleCreated (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnHandleDestroyed(EventArgs e) {
			Application.RemoveForm (this);
			base.OnHandleDestroyed (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInputLanguageChanged(InputLanguageChangedEventArgs e) {
			InputLanguageChangedEventHandler eh = (InputLanguageChangedEventHandler)(Events [InputLanguageChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInputLanguageChanging(InputLanguageChangingEventArgs e) {
			InputLanguageChangingEventHandler eh = (InputLanguageChangingEventHandler)(Events [InputLanguageChangingEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLoad (EventArgs e){
			Application.AddForm (this);

			EventHandler eh = (EventHandler)(Events[LoadEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMaximizedBoundsChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MaximizedBoundsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMaximumSizeChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MaximumSizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMdiChildActivate(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MdiChildActivateEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal virtual void OnMenuComplete(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MenuCompleteEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMenuStart(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MenuStartEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMinimumSizeChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MinimumSizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnPaint (PaintEventArgs e) {
			base.OnPaint (e);

			if (size_grip != null) {
				size_grip.HandlePaint (this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnResize(EventArgs e) {
			base.OnResize(e);

		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnStyleChanged(EventArgs e) {
			base.OnStyleChanged (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnTextChanged(EventArgs e) {
			base.OnTextChanged (e);

			if (mdi_container != null)
				mdi_container.SetParentText(true);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged (e);
			
			if (Visible) {
				if (window_manager != null)
					if (WindowState == FormWindowState.Normal)
						window_manager.SetWindowState (WindowState, WindowState);
					else
						// We don't really have an old_state, and if we pass the same thing,
						// it may not really change the state for us
						window_manager.SetWindowState ((FormWindowState)(-1), WindowState);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (base.ProcessCmdKey (ref msg, keyData)) {
				return true;
			}

			// Handle keyboard cues state.
			if ((keyData & Keys.Alt) != 0) {
				Control toplevel = TopLevelControl;
				if (toplevel != null) {
					IntPtr param = MakeParam ((int) MsgUIState.UIS_CLEAR, (int) MsgUIState.UISF_HIDEACCEL);
					XplatUI.SendMessage (toplevel.Handle, Msg.WM_CHANGEUISTATE, param, IntPtr.Zero);
				}
			}

			// Give our menu a shot
			if (ActiveMenu != null) {
				if (ActiveMenu.ProcessCmdKey (ref msg, keyData))
					return true;
			}

			// Detect any active ContextMenu for a child control that
			// can't receive focus (which means: both input and preprocess)
			if (ActiveTracker != null && ActiveTracker.TopMenu is ContextMenu) {
				ContextMenu cmenu = ActiveTracker.TopMenu as ContextMenu;
				if (cmenu.SourceControl != this && cmenu.ProcessCmdKey (ref msg, keyData))
					return true;
			}

			if (IsMdiChild) {
				switch (keyData)
				{
				case Keys.Control | Keys.F4:
				case Keys.Control | Keys.Shift | Keys.F4:
					Close ();
					return true;
				case Keys.Control | Keys.Tab:
				case Keys.Control | Keys.F6:
					MdiParent.MdiContainer.ActivateNextChild ();
					return true;
				case Keys.Control | Keys.Shift | Keys.Tab:
				case Keys.Control | Keys.Shift | Keys.F6:
					MdiParent.MdiContainer.ActivatePreviousChild ();
					return true;
				case Keys.Alt | Keys.OemMinus:
				case Keys.Alt | Keys.Subtract:
					(this.WindowManager as MdiWindowManager).ShowPopup (Point.Empty);
					return true;
				}
			}

			return false;
		}

		// LAMESPEC - Not documented that Form overrides ProcessDialogChar; class-status showed
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override bool ProcessDialogChar(char charCode) {
			return base.ProcessDialogChar (charCode);
		}

		protected override bool ProcessDialogKey(Keys keyData) {
			if ((keyData & Keys.Modifiers) == 0) {
				if (keyData == Keys.Enter) {
					IntPtr window = XplatUI.GetFocus ();
					Control c = Control.FromHandle (window);
					if (c is Button && c.FindForm () == this) {
						((Button)c).PerformClick ();
						return true;
					}
					else if (accept_button != null) {
						// Set ActiveControl to force any Validation to take place.
						ActiveControl = (accept_button as Control);
						if (ActiveControl == accept_button) // else Validation failed
							accept_button.PerformClick();
						return true;
					}
				} else if (keyData == Keys.Escape && cancel_button != null) {
					cancel_button.PerformClick();
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override bool ProcessKeyPreview(ref Message m) {
			if (key_preview) {
				if (ProcessKeyEventArgs(ref m)) {
					return true;
				}
			}
			return base.ProcessKeyPreview (ref m);
		}

		protected override bool ProcessTabKey(bool forward) {
			bool need_refresh = !show_focus_cues;
			show_focus_cues = true;
			
			bool control_activated = SelectNextControl(ActiveControl, forward, true, true, true);
			
			if (need_refresh && ActiveControl != null)
				ActiveControl.Invalidate ();
				
			return control_activated;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore (float x, float y)
		{
			base.ScaleCore (x, y);
		}

		protected override void Select(bool directed, bool forward) {
			Form	parent;


			// MS causes the handle to be created here.
			if (!IsHandleCreated)
				if (!IsHandleCreated)
					CreateHandle ();
			
			if (directed) {
				base.SelectNextControl(null, forward, true, true, true);
			}

			parent = this.ParentForm;
			if (parent != null) {
				parent.ActiveControl = this;
			}

			Activate();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			Size min_size;
			
			if (WindowState == FormWindowState.Minimized)
				min_size = SystemInformation.MinimizedWindowSize;
			else
				switch (FormBorderStyle) {
					case FormBorderStyle.None:
						min_size = XplatUI.MinimumNoBorderWindowSize;
						break;
					case FormBorderStyle.FixedToolWindow:
						min_size = XplatUI.MinimumFixedToolWindowSize;
						break;
					case FormBorderStyle.SizableToolWindow:
						min_size = XplatUI.MinimumSizeableToolWindowSize;
						break;
					default:
						min_size = SystemInformation.MinimumWindowSize;
						break;
				}
			
			if ((specified & BoundsSpecified.Width) == BoundsSpecified.Width)
				width = Math.Max (width, min_size.Width);
			if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height)
				height = Math.Max (height, min_size.Height);
				
			base.SetBoundsCore (x, y, width, height, specified);

			int restore_x = (specified & BoundsSpecified.X) == BoundsSpecified.X ? x : restore_bounds.X;
			int restore_y = (specified & BoundsSpecified.Y) == BoundsSpecified.Y ? y : restore_bounds.Y;
			int restore_w = (specified & BoundsSpecified.Width) == BoundsSpecified.Width ? width : restore_bounds.Width;
			int restore_h = (specified & BoundsSpecified.Height) == BoundsSpecified.Height ? height : restore_bounds.Height;
			restore_bounds = new Rectangle (restore_x, restore_y, restore_w, restore_h);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void SetClientSizeCore(int x, int y) {
			if ((minimum_size.Width != 0) && (x < minimum_size.Width)) {
				x = minimum_size.Width;
			} else if ((maximum_size.Width != 0) && (x > maximum_size.Width)) {
				x = maximum_size.Width;
			}

			if ((minimum_size.Height != 0) && (y < minimum_size.Height)) {
				y = minimum_size.Height;
			} else if ((maximum_size.Height != 0) && (y > maximum_size.Height)) {
				y = maximum_size.Height;
			}

			Rectangle ClientRect = new Rectangle(0, 0, x, y);
			Rectangle WindowRect;
			CreateParams cp = this.CreateParams;

			clientsize_set = new Size(x, y);

			if (XplatUI.CalculateWindowRect(ref ClientRect, cp, cp.menu, out WindowRect)) {
				SetBounds(bounds.X, bounds.Y, WindowRect.Width, WindowRect.Height, BoundsSpecified.Size);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void SetVisibleCore(bool value)
		{
			if (value)
				close_raised = false;

			if (IsMdiChild && !MdiParent.Visible) {
				if (value != Visible) {
					MdiWindowManager wm = (MdiWindowManager) window_manager;
					wm.IsVisiblePending = value;
					OnVisibleChanged (EventArgs.Empty);
					return;
				}
			} else {
				is_changing_visible_state++;
				has_been_visible = value || has_been_visible;
				base.SetVisibleCore (value);
				if (value) {
					Application.AddForm (this);
				}
				
				if (value && WindowState != FormWindowState.Normal)
					XplatUI.SendMessage (Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);
					
				is_changing_visible_state--;
			}
			
			if (value && IsMdiContainer) {
				Form [] children = MdiChildren;
				for (int i = 0; i < children.Length; i++) {
					Form child = children [i];
					MdiWindowManager wm = (MdiWindowManager) child.window_manager;
					if (!child.IsHandleCreated && wm.IsVisiblePending) {
						wm.IsVisiblePending = false;
						child.Visible = true;
					}
				}
			}
			
			if (value && IsMdiChild){
				PerformLayout ();
				ThemeEngine.Current.ManagedWindowSetButtonLocations (window_manager);
				if (ActivateOnShow && MdiParent != null)
					MdiParent.ActivateMdiChild (this);
			}
			
			// Shown event is only called once, the first time the form is made visible
			if (value && !shown_raised) {
				this.OnShown (EventArgs.Empty);
				shown_raised = true;
			}
			
			if (value && !IsMdiChild) {
				if (ActiveControl == null)
					SelectNextControl (null, true, true, true, false);
				if (ActiveControl != null)
					SendControlFocus (ActiveControl);
				else
					this.Focus ();
			}
		}

		protected override void UpdateDefaultButton() {
			base.UpdateDefaultButton ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
#if debug
			Console.WriteLine(DateTime.Now.ToLongTimeString () + " Form {0} ({2}) received message {1}", window.Handle == IntPtr.Zero ? this.Text : XplatUI.Window(window.Handle), m.ToString (), Text);
#endif

			if (window_manager != null && window_manager.WndProc (ref m)) {
				return;
			}

			switch ((Msg)m.Msg) {
			case Msg.WM_DESTROY: {
				WmDestroy (ref m);
				return;
			}

			case Msg.WM_CLOSE: {
				WmClose (ref m);
				return;
			}

			case Msg.WM_WINDOWPOSCHANGED: {
				WmWindowPosChanged (ref m);
				return;
			}

			case Msg.WM_SYSCOMMAND: {
				WmSysCommand (ref m);
				break;
			}

			case Msg.WM_ACTIVATE: {
				WmActivate (ref m);
				return;
			}

			case Msg.WM_KILLFOCUS: {
				WmKillFocus (ref m);
				return;
			}

			case Msg.WM_SETFOCUS: {
				WmSetFocus (ref m);
				return;
			}

			// Menu drawing
			case Msg.WM_NCHITTEST: {
				WmNcHitTest (ref m);
				return;
			}

			case Msg.WM_NCLBUTTONDOWN: {
				WmNcLButtonDown (ref m);
				return;
			}

			case Msg.WM_NCLBUTTONUP: {
				WmNcLButtonUp (ref m);
				return;
			}

			case Msg.WM_NCMOUSELEAVE: {
				WmNcMouseLeave (ref m);
				return;
			}

			case Msg.WM_NCMOUSEMOVE: {
				WmNcMouseMove (ref m);
				return;
			}

			case Msg.WM_NCPAINT: {
				WmNcPaint (ref m);
				return;
			}

			case Msg.WM_NCCALCSIZE: {
				WmNcCalcSize (ref m);
				break;
			}

			case Msg.WM_GETMINMAXINFO: {
				WmGetMinMaxInfo (ref m);
				break;
			}
			
			case Msg.WM_ENTERSIZEMOVE: {
				OnResizeBegin (EventArgs.Empty);
				break;
			}
			
			case Msg.WM_EXITSIZEMOVE: {
				OnResizeEnd (EventArgs.Empty);
				break;
			}

			default: {
				base.WndProc (ref m);
				break;
			}
			}
		}
		#endregion	// Protected Instance Methods

#region WM methods

		private void WmDestroy (ref Message m)
		{
			if (!RecreatingHandle)
				this.closing = true;

			base.WndProc (ref m);
		}
		
		internal bool RaiseCloseEvents (bool last_check, bool cancel)
		{
			if (last_check && Visible) {
				Hide ();
			}
			
			if (close_raised || (last_check && closed)) {
				return false;
			}
			
			close_raised = true;
			bool cancelled = FireClosingEvents (CloseReason.UserClosing, cancel);
			if (!cancelled) {
				if (!last_check || DialogResult != DialogResult.None) {
					if (mdi_container != null)
						foreach (Form mdi_child in mdi_container.MdiChildren)
							mdi_child.FireClosedEvents (CloseReason.UserClosing);

					FireClosedEvents (CloseReason.UserClosing);
				}
				closing = true;
				shown_raised = false;
			} else {
				DialogResult = DialogResult.None;
				closing = false;
				close_raised = false;
			}
				
			return cancelled;
		}

		private void WmClose (ref Message m)
		{
			if (this.Enabled == false)
				return; // prevent closing a disabled form.

			Form act = Form.ActiveForm;

			// Don't close this form if there's another modal form visible.
			if (act != null && act != this && act.Modal == true) {
				// Check if any of the parents up the tree is the modal form, 
				// in which case we can still close this form.
				Control current = this;
				while (current != null && current.Parent != act) {
					current = current.Parent;
				}
				if (current == null || current.Parent != act) {
					return;
				}
			}

			bool mdi_cancel = false;
			
			// Give any MDI children the opportunity to cancel the close
			if (mdi_container != null)
				foreach (Form mdi_child in mdi_container.MdiChildren)
					mdi_cancel = mdi_child.FireClosingEvents (CloseReason.MdiFormClosing, mdi_cancel);

			bool validate_cancel = false;
			if (!suppress_closing_events)
				validate_cancel = !ValidateChildren ();

			if (suppress_closing_events || 
			    !RaiseCloseEvents (false, validate_cancel || mdi_cancel)) {
				if (is_modal) {
					Hide ();
				} else {
					Dispose ();
					if (act != null && act != this)
						act.SelectActiveControl();
				}
				mdi_parent = null;
			} else {
				if (is_modal) {
					DialogResult = DialogResult.None;
				}
				closing = false;	
			}
		}
		
		private void WmWindowPosChanged (ref Message m)
		{
			// When a form is minimized/restored:
			// * Win32: X and Y are set to negative values/restored, 
			//   size remains the same.
			// * X11: Location and Size remain the same.
			// 
			// In both cases we have to fire Resize explicitly here, 
			// because of the unmodified Size due to which Control
			// doesn't fire it.
			// 
			if (window_state != FormWindowState.Minimized && WindowState != FormWindowState.Minimized)
				base.WndProc (ref m);
			else { // minimized or restored
				if (!is_minimizing) {
					// Avoid recursive calls here as code in OnSizeChanged might 
					// cause a WM_WINDOWPOSCHANGED to be sent.
					is_minimizing = true;
					OnSizeChanged (EventArgs.Empty);
					is_minimizing = false;
				}
			}

			if (WindowState == FormWindowState.Normal)
				restore_bounds = Bounds;
		}

		private void WmSysCommand (ref Message m)
		{
			// Let *Strips know the app's title bar was clicked
			if (XplatUI.IsEnabled (Handle))
				ToolStripManager.FireAppClicked ();

			base.WndProc (ref m);
		}

		private void WmActivate (ref Message m)
		{
			if (!this.Enabled && modal_dialogs.Count > 0)
			{
				(modal_dialogs[modal_dialogs.Count -1] as Form).Activate ();
				return; // prevent Activating of disabled form.
			}

			if (m.WParam != (IntPtr)WindowActiveFlags.WA_INACTIVE) {
				if (is_loaded) {
					SelectActiveControl ();

					if (ActiveControl != null && !ActiveControl.Focused)
						SendControlFocus (ActiveControl);
				}

				IsActive = true;
			} else {
				if (XplatUI.IsEnabled (Handle) && !IsChild (Handle, m.LParam))
					ToolStripManager.FireAppFocusChanged (this);
				IsActive = false;
			}
		}
		
		private void WmKillFocus (ref Message m)
		{
			base.WndProc (ref m);
		}
		
		private void WmSetFocus (ref Message m)
		{
			if (ActiveControl != null && ActiveControl != this) {
				ActiveControl.Focus ();
				return;	// FIXME - do we need to run base.WndProc, even though we just changed focus?
			}
			if (IsMdiContainer) {
				mdi_container.SendFocusToActiveChild ();
				return;
			}
			base.WndProc (ref m);
		}
		
		private void WmNcHitTest (ref Message m)
		{
			if (XplatUI.IsEnabled (Handle) && ActiveMenu != null) {
				int x = LowOrder ((int)m.LParam.ToInt32 ());
				int y = HighOrder ((int)m.LParam.ToInt32 ());

				XplatUI.ScreenToMenu (ActiveMenu.Wnd.window.Handle, ref x, ref y);

				// If point is under menu return HTMENU, it prevents Win32 to return HTMOVE.
				if ((x > 0) && (y > 0) && (x < ActiveMenu.Rect.Width) && (y < ActiveMenu.Rect.Height)) {
					m.Result = new IntPtr ((int)HitTest.HTMENU);
					return;
				}
			}

			base.WndProc (ref m);
		}
		
		private void WmNcLButtonDown (ref Message m)
		{
			if (XplatUI.IsEnabled (Handle) && ActiveMenu != null) {
				ActiveMenu.OnMouseDown (this, new MouseEventArgs (FromParamToMouseButtons ((int)m.WParam.ToInt32 ()), mouse_clicks, Control.MousePosition.X, Control.MousePosition.Y, 0));
			}

			if (ActiveMaximizedMdiChild != null && ActiveMenu != null) {
				if (ActiveMaximizedMdiChild.HandleMenuMouseDown (ActiveMenu,
						LowOrder ((int)m.LParam.ToInt32 ()),
						HighOrder ((int)m.LParam.ToInt32 ()))) {
					// Don't let base process this message, otherwise we won't
					// get a WM_NCLBUTTONUP.
					return;
				}
			}
			base.WndProc (ref m);
		}
		
		private void WmNcLButtonUp (ref Message m)
		{
			if (ActiveMaximizedMdiChild != null && ActiveMenu != null) {
				ActiveMaximizedMdiChild.HandleMenuMouseUp (ActiveMenu,
						LowOrder ((int)m.LParam.ToInt32 ()),
						HighOrder ((int)m.LParam.ToInt32 ()));
			}
			base.WndProc (ref m);
		}
		
		private void WmNcMouseLeave (ref Message m)
		{
			if (ActiveMaximizedMdiChild != null && ActiveMenu != null) {
				ActiveMaximizedMdiChild.HandleMenuMouseLeave (ActiveMenu,
						LowOrder ((int)m.LParam.ToInt32 ()),
						HighOrder ((int)m.LParam.ToInt32 ()));
			}
			base.WndProc (ref m);
		}
		
		private void WmNcMouseMove (ref Message m)
		{
			if (XplatUI.IsEnabled (Handle) && ActiveMenu != null) {
				ActiveMenu.OnMouseMove (this, new MouseEventArgs (FromParamToMouseButtons ((int)m.WParam.ToInt32 ()), mouse_clicks, LowOrder ((int)m.LParam.ToInt32 ()), HighOrder ((int)m.LParam.ToInt32 ()), 0));
			}

			if (ActiveMaximizedMdiChild != null && ActiveMenu != null) {
				XplatUI.RequestAdditionalWM_NCMessages (Handle, false, true);
				ActiveMaximizedMdiChild.HandleMenuMouseMove (ActiveMenu,
						LowOrder ((int)m.LParam.ToInt32 ()),
						HighOrder ((int)m.LParam.ToInt32 ()));
			}
			base.WndProc (ref m);
		}
		
		private void WmNcPaint (ref Message m)
		{
			if (ActiveMenu != null) {
				PaintEventArgs pe = XplatUI.PaintEventStart (ref m, Handle, false);
				Point pnt = XplatUI.GetMenuOrigin (window.Handle);

				// The entire menu has to be in the clip rectangle because the 
				// control buttons are right-aligned and otherwise they would
				// stay painted when the window gets resized.
				Rectangle clip = new Rectangle (pnt.X, pnt.Y, ClientSize.Width, 0);
				clip = Rectangle.Union (clip, pe.ClipRectangle);
				pe.SetClip (clip);
				pe.Graphics.SetClip (clip);

				ActiveMenu.Draw (pe, new Rectangle (pnt.X, pnt.Y, ClientSize.Width, 0));

				if (ActiveMaximizedMdiChild != null)
					ActiveMaximizedMdiChild.DrawMaximizedButtons (ActiveMenu, pe);

				XplatUI.PaintEventEnd (ref m, Handle, false, pe);
			}

			base.WndProc (ref m);
		}
		
		private void WmNcCalcSize (ref Message m)
		{
			XplatUIWin32.NCCALCSIZE_PARAMS ncp;

			if ((ActiveMenu != null) && (m.WParam == (IntPtr)1)) {
				ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure (m.LParam, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

				// Adjust for menu
				ncp.rgrc1.top += ThemeEngine.Current.CalcMenuBarSize (DeviceContext, ActiveMenu, ClientSize.Width);
				Marshal.StructureToPtr (ncp, m.LParam, true);
			}
			DefWndProc (ref m);		
		}
		
		private void WmGetMinMaxInfo (ref Message m)
		{
			MINMAXINFO mmi;

			if (m.LParam != IntPtr.Zero) {
				mmi = (MINMAXINFO)Marshal.PtrToStructure (m.LParam, typeof (MINMAXINFO));

				default_maximized_bounds = new Rectangle (mmi.ptMaxPosition.x, mmi.ptMaxPosition.y, mmi.ptMaxSize.x, mmi.ptMaxSize.y);
				if (maximized_bounds != Rectangle.Empty) {
					mmi.ptMaxPosition.x = maximized_bounds.Left;
					mmi.ptMaxPosition.y = maximized_bounds.Top;
					mmi.ptMaxSize.x = maximized_bounds.Width;
					mmi.ptMaxSize.y = maximized_bounds.Height;
				}

				if (minimum_size != Size.Empty) {
					mmi.ptMinTrackSize.x = minimum_size.Width;
					mmi.ptMinTrackSize.y = minimum_size.Height;
				}

				if (maximum_size != Size.Empty) {
					mmi.ptMaxTrackSize.x = maximum_size.Width;
					mmi.ptMaxTrackSize.y = maximum_size.Height;
				}
				Marshal.StructureToPtr (mmi, m.LParam, false);
			}
		}
#endregion

		#region Internal / Private Methods
		internal void ActivateFocusCues ()
		{
			bool need_refresh = !show_focus_cues;
			show_focus_cues = true;
			
			if (need_refresh)
				ActiveControl.Invalidate ();
		}
		
		internal override void FireEnter ()
		{
			// do nothing - forms don't generate OnEnter
		}

		internal override void FireLeave ()
		{
			// do nothing - forms don't generate OnLeave
		}

		internal void RemoveWindowManager ()
		{
			window_manager = null;
		}
		
		internal override void CheckAcceptButton ()
		{
			if (accept_button != null) {
				Button a_button = accept_button as Button;

				if (ActiveControl == a_button)
					return;
				
				// If the accept_button isn't a Button, we don't need to do
				// the rest of this.
				if (a_button == null)
					return;
					
				if (ActiveControl is Button)
					a_button.paint_as_acceptbutton = false;
				else
					a_button.paint_as_acceptbutton = true;
					
				a_button.Invalidate ();
			}
		}

		internal override bool ActivateOnShow { get { return !this.ShowWithoutActivation; } }
		
		private void OnLoadInternal (EventArgs e)
		{
			if (AutoScale) {
				ApplyAutoScaling ();
				AutoScale = false;
			}

			if (!IsDisposed) {
				OnSizeInitializedOrChanged ();
				
				// We do this here because when we load the MainForm,
				// it happens before the exception catcher in NativeWindow,
				// so the user can error in handling Load and we wouldn't catch it.
				try {
					OnLoad (e);
				}
				catch (Exception ex) {
					Application.OnThreadException (ex);
				}

				if (!IsDisposed)
					is_visible = true;
			}
			
			if (!IsMdiChild && !IsDisposed) {
				switch (StartPosition) {
					case FormStartPosition.CenterScreen:
						this.CenterToScreen ();
						break;
					case FormStartPosition.CenterParent:
						this.CenterToParent ();
						break;
					case FormStartPosition.Manual:
						Left = CreateParams.X;
						Top = CreateParams.Y;
						break;
				}
			}
			
			is_loaded = true;
		}

		private void UpdateMinMax()
		{
			var min_size = AutoSize ? new Size (Math.Max (minimum_auto_size.Width, minimum_size.Width), Math.Max (minimum_auto_size.Height, minimum_size.Height)) : minimum_size;
			if (IsHandleCreated)
				XplatUI.SetWindowMinMax (Handle, maximized_bounds, min_size, maximum_size);
		}		
		#endregion
		
		#region Events
		static object ActivatedEvent = new object ();
		static object ClosedEvent = new object ();
		static object ClosingEvent = new object ();
		static object DeactivateEvent = new object ();
		static object InputLanguageChangedEvent = new object ();
		static object InputLanguageChangingEvent = new object ();
		static object LoadEvent = new object ();
		static object MaximizedBoundsChangedEvent = new object ();
		static object MaximumSizeChangedEvent = new object ();
		static object MdiChildActivateEvent = new object ();
		static object MenuCompleteEvent = new object ();
		static object MenuStartEvent = new object ();
		static object MinimumSizeChangedEvent = new object ();

		public event EventHandler Activated {
			add { Events.AddHandler (ActivatedEvent, value); }
			remove { Events.RemoveHandler (ActivatedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler Closed {
			add { Events.AddHandler (ClosedEvent, value); }
			remove { Events.RemoveHandler (ClosedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event CancelEventHandler Closing {
			add { Events.AddHandler (ClosingEvent, value); }
			remove { Events.RemoveHandler (ClosingEvent, value); }
		}

		public event EventHandler Deactivate {
			add { Events.AddHandler (DeactivateEvent, value); }
			remove { Events.RemoveHandler (DeactivateEvent, value); }
		}

		public event InputLanguageChangedEventHandler InputLanguageChanged {
			add { Events.AddHandler (InputLanguageChangedEvent, value); }
			remove { Events.RemoveHandler (InputLanguageChangedEvent, value); }
		}

		public event InputLanguageChangingEventHandler InputLanguageChanging {
			add { Events.AddHandler (InputLanguageChangingEvent, value); }
			remove { Events.RemoveHandler (InputLanguageChangingEvent, value); }
		}

		public event EventHandler Load {
			add { Events.AddHandler (LoadEvent, value); }
			remove { Events.RemoveHandler (LoadEvent, value); }
		}

		public event EventHandler MaximizedBoundsChanged {
			add { Events.AddHandler (MaximizedBoundsChangedEvent, value); }
			remove { Events.RemoveHandler (MaximizedBoundsChangedEvent, value); }
		}

		public event EventHandler MaximumSizeChanged {
			add { Events.AddHandler (MaximumSizeChangedEvent, value); }
			remove { Events.RemoveHandler (MaximumSizeChangedEvent, value); }
		}

		public event EventHandler MdiChildActivate {
			add { Events.AddHandler (MdiChildActivateEvent, value); }
			remove { Events.RemoveHandler (MdiChildActivateEvent, value); }
		}

		[Browsable (false)]
		public event EventHandler MenuComplete {
			add { Events.AddHandler (MenuCompleteEvent, value); }
			remove { Events.RemoveHandler (MenuCompleteEvent, value); }
		}

		[Browsable (false)]
		public event EventHandler MenuStart {
			add { Events.AddHandler (MenuStartEvent, value); }
			remove { Events.RemoveHandler (MenuStartEvent, value); }
		}

		public event EventHandler MinimumSizeChanged {
			add { Events.AddHandler (MinimumSizeChangedEvent, value); }
			remove { Events.RemoveHandler (MinimumSizeChangedEvent, value); }
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[SettingsBindable (true)]
		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		[SettingsBindable (true)]
		public new Point Location {
			get {
				return base.Location;
			}

			set {
				base.Location = value;
			}
		}

		static object FormClosingEvent = new object ();
		static object FormClosedEvent = new object ();
		static object HelpButtonClickedEvent = new object ();
		static object ResizeEndEvent = new object ();
		static object ResizeBeginEvent = new object ();
		static object RightToLeftLayoutChangedEvent = new object ();
		static object ShownEvent = new object ();

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoValidateChanged {
			add { base.AutoValidateChanged += value; }
			remove { base.AutoValidateChanged -= value; }
		}

		public event FormClosingEventHandler FormClosing {
			add { Events.AddHandler (FormClosingEvent, value); }
			remove { Events.RemoveHandler (FormClosingEvent, value); }
		}

		public event FormClosedEventHandler FormClosed {
			add { Events.AddHandler (FormClosedEvent, value); }
			remove { Events.RemoveHandler (FormClosedEvent, value); }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public event CancelEventHandler HelpButtonClicked {
			add { Events.AddHandler (HelpButtonClickedEvent, value); }
			remove { Events.RemoveHandler (HelpButtonClickedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler MarginChanged {
			add { base.MarginChanged += value; }
			remove { base.MarginChanged -= value; }
		}

		public event EventHandler RightToLeftLayoutChanged {
			add { Events.AddHandler (RightToLeftLayoutChangedEvent, value); }
			remove { Events.RemoveHandler (RightToLeftLayoutChangedEvent, value); }
		}

		public event EventHandler ResizeBegin {
			add { Events.AddHandler (ResizeBeginEvent, value); }
			remove { Events.RemoveHandler (ResizeBeginEvent, value); }
		}

		public event EventHandler ResizeEnd {
			add { Events.AddHandler (ResizeEndEvent, value); }
			remove { Events.RemoveHandler (ResizeEndEvent, value); }
		}

		public event EventHandler Shown {
			add { Events.AddHandler (ShownEvent, value); }
			remove { Events.RemoveHandler (ShownEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		protected override void OnBackgroundImageChanged (EventArgs e)
		{
			base.OnBackgroundImageChanged (e);
		}

		protected override void OnBackgroundImageLayoutChanged (EventArgs e)
		{
			base.OnBackgroundImageLayoutChanged (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnEnabledChanged (EventArgs e)
		{
			base.OnEnabledChanged (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnFormClosed (FormClosedEventArgs e) {
			Application.RemoveForm (this);
			FormClosedEventHandler eh = (FormClosedEventHandler)(Events[FormClosedEvent]);
			if (eh != null)
				eh (this, e);

			foreach (Form form in disabled_by_showdialog)
			{
				form.Enabled = true;
			}
			disabled_by_showdialog.Clear();
			if (modal_dialogs.Contains(this))
				modal_dialogs.Remove(this);
		}
		
		// Consider calling FireClosingEvents instead of calling this directly.
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnFormClosing (FormClosingEventArgs e)
		{
			FormClosingEventHandler eh = (FormClosingEventHandler)(Events [FormClosingEvent]);
			if (eh != null)
				eh (this, e);
		}

		[MonoTODO ("Will never be called")]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnHelpButtonClicked (CancelEventArgs e)
		{
			CancelEventHandler eh = (CancelEventHandler)(Events[HelpButtonClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnLayout (LayoutEventArgs levent)
		{
			if (AutoSize) {
				Size new_size = GetPreferredSizeCore (Size.Empty);
				if (new_size != minimum_auto_size) {
					minimum_auto_size = new_size;
					UpdateMinMax();
				}				
				if (AutoSizeMode == AutoSizeMode.GrowOnly) {
					new_size.Width = Math.Max (new_size.Width, Width);
					new_size.Height = Math.Max (new_size.Height, Height);
				}
				if (new_size == Size)
					return;

				SetBoundsCore (bounds.X, bounds.Y, new_size.Width, new_size.Height, BoundsSpecified.None);
			}

			base.OnLayout (levent);			
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnResizeBegin (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events [ResizeBeginEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnResizeEnd (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events [ResizeEndEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnRightToLeftLayoutChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events[RightToLeftLayoutChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected virtual void OnShown (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events [ShownEvent]);
			if (eh != null)
				eh (this, e);
		}

		#region UIA Framework Events
		static object UIAMenuChangedEvent = new object ();
		static object UIATopMostChangedEvent = new object ();
		static object UIAWindowStateChangedEvent = new object ();

		internal event EventHandler UIAMenuChanged {
			add { Events.AddHandler (UIAMenuChangedEvent, value); }
			remove { Events.RemoveHandler (UIAMenuChangedEvent, value); }
		}

		internal event EventHandler UIATopMostChanged {
			add { Events.AddHandler (UIATopMostChangedEvent, value); }
			remove { Events.RemoveHandler (UIATopMostChangedEvent, value); }
		}

		internal event EventHandler UIAWindowStateChanged {
			add { Events.AddHandler (UIAWindowStateChangedEvent, value); }
			remove { Events.RemoveHandler (UIAWindowStateChangedEvent, value); }
		}

		internal void OnUIAMenuChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) Events [UIAMenuChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		internal void OnUIATopMostChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIATopMostChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		internal void OnUIAWindowStateChanged ()
		{
			EventHandler eh = (EventHandler) Events [UIAWindowStateChangedEvent];
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		#endregion	// UIA Framework Events
		#endregion	// Events
	}
}
