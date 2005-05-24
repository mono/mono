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
// Copyright (c) 2004 Novell, Inc.
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

namespace System.Windows.Forms {
	[DesignerCategory("Form")]
	[DesignTimeVisible(false)]
	[Designer("System.Windows.Forms.Design.FormDocumentDesigner, " + Consts.AssemblySystem_Design, typeof(IRootDesigner))]
	[DefaultEvent("Load")]
	[ToolboxItem(false)]
	public class Form : ContainerControl {
		#region Local Variables
		internal static Form		active_form;
		internal bool			closing;
		FormBorderStyle			formBorderStyle;
		private static bool		autoscale;
		private static Size		autoscale_base_size;
		internal bool			is_modal;
		internal bool			end_modal;			// This var is being monitored by the application modal loop
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
		private Form			mdi_parent;
		private bool			key_preview;
		private MainMenu		menu;
		private	Icon			icon;
		private Size			maximum_size;
		private Size			minimum_size;
		private SizeGripStyle		size_grip_style;
		private Rectangle		maximized_bounds;
		private Rectangle		default_maximized_bounds;
		Color				transparency_key;

		#endregion	// Local Variables

		#region Private & Internal Methods
		#endregion	// Private & Internal Methods

		#region Public Classes
		public new class ControlCollection : Control.ControlCollection {
			Form	form_owner;

			public ControlCollection(Form owner) : base(owner) {
				this.form_owner = owner;
			}

			public override void Add(Control value) {
				for (int i=0; i<list.Count; i++) {
					if (list[i]==value) {
						// Do we need to do anything here?
						return;
					}
				}
				list.Add(value);
				((Form)value).owner=(Form)owner;
			}

			public override void Remove(Control value) {
				((Form)value).owner = null;
				base.Remove (value);
			}
		}
		#endregion	// Public Classes

		#region Public Constructor & Destructor
		public Form() {
			closing = false;
			is_modal = false;
			end_modal = false;
			dialog_result = DialogResult.None;
			start_position = FormStartPosition.WindowsDefaultLocation;
			formBorderStyle = FormBorderStyle.Sizable;
			key_preview = false;
			menu = null;
			icon = null;
			minimum_size = new Size(0, 0);
			maximum_size = new Size(0, 0);
			control_box = true;
			minimize_box = true;
			maximize_box = true;
			help_button = false;
			show_in_taskbar = true;
			ime_mode = ImeMode.NoControl;
			is_visible = false;
			is_toplevel = true;
			size_grip_style = SizeGripStyle.Auto;
			maximized_bounds = Rectangle.Empty;
			default_maximized_bounds = Rectangle.Empty;
			owned_forms = new Form.ControlCollection(this);
			transparency_key = Color.Empty;
		}
		#endregion	// Public Constructor & Destructor

		#region Public Static Properties

		public static Form ActiveForm {
			get {
				Control	active;

				active = FromHandle(XplatUI.GetActive());

				if (active != null) {
					if ( !(active is Form)) {
						Control	parent;

						parent = active.Parent;
						while (parent != null) {
							if (parent is Form) {
								return (Form)parent;
							}
							parent = parent.Parent;
						}
					} else {
						return (Form)active;
					}
				}
				return null;
			}
		}

		#endregion	// Public Static Properties

		#region Public Instance Properties
		[DefaultValue(null)]
		public IButtonControl AcceptButton {
			get {
				return accept_button;
			}

			set {
				accept_button = value;
			}
		}

		[MonoTODO("Figure out a way for transparency support in windows")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool AllowTransparency {
			get {
				return false;
			}

			set {
			}
		}
			
		[DefaultValue(true)]
		public bool AutoScale {
			get {
				return autoscale;
			}

			set {
				autoscale=value;
			}
		}

		[Localizable(true)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual Size AutoScaleBaseSize {
			get {
				return autoscale_base_size;
			}

			set {
				autoscale_base_size=value;
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

		public override Color BackColor {
			get {
				return base.BackColor;
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
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		[Localizable(true)]
		public Size ClientSize {
			get {
				return base.ClientSize;
			}

			set {
				base.ClientSize = value;
			}
		}

		[DefaultValue(true)]
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
				dialog_result = value;

				if (is_modal && (dialog_result != DialogResult.None)) {
					end_modal = true;
				}
			}
		}

		[DefaultValue(FormBorderStyle.Sizable)]
		[DispId(-504)]
		public FormBorderStyle FormBorderStyle {
			get {
				return formBorderStyle;
			}
			set {
				formBorderStyle = value;
				UpdateStyles();
			}
		}

		[DefaultValue(false)]
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
		public Icon Icon {
			get {
				return icon;
			}

			set {
				if (icon != value) {
					icon = value;

					XplatUI.SetIcon(Handle, icon);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool IsMdiChild {
			get {
				return mdi_parent != null;
			}
		}

		[DefaultValue(false)]
		public bool IsMdiContainer {
			get {
				return mdi_container != null;
			}

			set {
				if (value && mdi_container == null) {
					mdi_container = new MdiClient();
					Controls.Add(mdi_container);
				} else if (!value && mdi_container != null) {
					Controls.Remove(mdi_container);
					mdi_container.Dispose();
				}
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

		[DefaultValue(true)]
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

		[DefaultValue("{Width=0, Height=0}")]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public Size MaximumSize {
			get {
				return maximum_size;
			}

			set {
				if (maximum_size != value) {
					maximum_size = value;
					OnMaximumSizeChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form[] MdiChildren {
			get {
				if (mdi_container != null) {
					Form[] form_list;

					form_list = new Form[mdi_container.Controls.Count];
					for (int i = 0; i < mdi_container.Controls.Count; i++) {
						form_list[i] = (Form)mdi_container.Controls[i];
					}
					return form_list;
				} else {
					return new Form[0];
				}
			}
		}

		[MonoTODO("Finish setter")]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form MdiParent {
			get {
				return mdi_parent;
			}

			set {
				if (mdi_parent != null) {
					mdi_parent.Controls.Remove(this);
				}

				mdi_parent = value;
				if (mdi_parent != null) {
					mdi_parent.Controls.Add(this);
				}
			}
		}

		[DefaultValue(null)]
		public MainMenu Menu {
			get {
				return menu;
			}

			set {				
				if (menu != value) {					
					menu = value;

					menu.SetForm (this);
					MenuAPI.SetMenuBarWindow (menu.Handle, this);
				
					if (IsHandleCreated && menu != null) {	
						XplatUI.SetMenu(window.Handle, menu.Handle);
					}

					// FIXME - Do we still need this?
					this.SetBoundsCore(0, 0, 0, 0, BoundsSpecified.None);

					ThemeEngine.Current.CalcMenuBarSize (DeviceContext, menu.Handle, ClientSize.Width);
				}
			}
		}

		[DefaultValue(true)]
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

		[DefaultValue("{Width=0, Height=0}")]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public Size MinimumSize {
			get {
				return minimum_size;
			}

			set {
				if (minimum_size != value) {
					minimum_size = value;
					OnMinimumSizeChanged(EventArgs.Empty);
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
					owner.AddOwnedForm(this);
					if (owner != null) {
						XplatUI.SetTopmost(this.window.Handle, owner.window.Handle, true);
					} else {
						XplatUI.SetTopmost(this.window.Handle, IntPtr.Zero, false);
					}
				}
			}
		}

		[DefaultValue(true)]
		public bool ShowInTaskbar {
			get {
				return show_in_taskbar;
			}
			set {
				if (show_in_taskbar != value) {
					show_in_taskbar = value;
					UpdateStyles();
				}
			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Localizable(false)]
		public Size Size {
			get {
				return base.Size;
			}

			set {
				base.Size = value;
			}
		}

		[MonoTODO("Trigger something when GripStyle is set")]
		[DefaultValue(SizeGripStyle.Auto)]
		public SizeGripStyle SizeGripStyle {
			get {
				return size_grip_style;
			}

			set {
				size_grip_style = value;
			}
		}

		[DefaultValue(FormStartPosition.WindowsDefaultLocation)]
		[Localizable(true)]
		public FormStartPosition StartPosition {
			get {
				return start_position;
			}

			set {
				if (start_position == FormStartPosition.WindowsDefaultLocation) {		// Only do this if it's not set yet
					start_position = value;
					if (IsHandleCreated) {
						switch(start_position) {
							case FormStartPosition.CenterParent: {
								CenterToParent();
								break;
							}

							case FormStartPosition.CenterScreen: {
								CenterToScreen();
								break;
							}

							default: {
								break;
							}
						}
					}
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int TabIndex {
			get {
				return base.TabIndex;
			}

			set {
				base.TabIndex = value;
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public bool TopLevel {
			get {
				return GetTopLevel();
			}

			set {
				SetTopLevel(value);
			}
		}

		[DefaultValue(false)]
		public bool TopMost {
			get {
				return topmost;
			}

			set {
				if (topmost != value) {
					topmost = value;
					XplatUI.SetTopmost(window.Handle, owner != null ? owner.window.Handle : IntPtr.Zero, value);
				}
			}
		}

		public Color TransparencyKey {
			get {
				return transparency_key;
			}

			set {
				AllowTransparency = true;
				transparency_key = value;
				// TODO: change window attributes; a new driver call
			}
		}

		[DefaultValue(FormWindowState.Normal)]
		public FormWindowState WindowState {
			get {
				return XplatUI.GetWindowState(window.Handle);
			}

			set {
				XplatUI.SetWindowState(window.Handle, value);
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		[MonoTODO("Need to add MDI support")]
		protected override CreateParams CreateParams {
			get {
				CreateParams cp;

				cp = new CreateParams();

				cp.Caption = "FormWindow";
				cp.ClassName = XplatUI.DefaultClassName;
				cp.ClassStyle = 0;
				cp.ExStyle = 0;
				cp.Param = 0;
				cp.Parent = IntPtr.Zero;
//				if (start_position == FormStartPosition.WindowsDefaultLocation) {
					cp.X = unchecked((int)0x80000000);
					cp.Y = unchecked((int)0x80000000);
//				} else {
//					cp.X = Left;
//					cp.Y = Top;
//				}
				cp.Width = Width;
				cp.Height = Height;
				
				cp.Style = (int)(WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN);

				switch (FormBorderStyle) {
					case FormBorderStyle.Fixed3D: {
						cp.Style |= (int)WindowStyles.WS_CAPTION;
						cp.ExStyle |= (int)WindowStyles.WS_EX_OVERLAPPEDWINDOW;
						break;
					}

					case FormBorderStyle.FixedDialog: {
						cp.Style |= (int)WindowStyles.WS_CAPTION;
						cp.ExStyle |= (int)(WindowStyles.WS_EX_DLGMODALFRAME | WindowStyles.WS_EX_WINDOWEDGE);
						break;
					}

					case FormBorderStyle.FixedSingle: {
						cp.Style |= (int)WindowStyles.WS_CAPTION;
						cp.ExStyle |= (int)(WindowStyles.WS_EX_WINDOWEDGE);
						break;
					}

					case FormBorderStyle.FixedToolWindow: {
						cp.Style |= (int)WindowStyles.WS_CAPTION;
						cp.ExStyle |= (int)(WindowStyles.WS_EX_WINDOWEDGE | WindowStyles.WS_EX_TOOLWINDOW);
						break;
					}

					case FormBorderStyle.Sizable: {
						cp.Style |= (int)WindowStyles.WS_OVERLAPPEDWINDOW;
						cp.ExStyle |= (int)(WindowStyles.WS_EX_WINDOWEDGE);
						break;
					}

					case FormBorderStyle.SizableToolWindow: {
						cp.Style |= (int)WindowStyles.WS_OVERLAPPEDWINDOW;
						cp.ExStyle |= (int)(WindowStyles.WS_EX_WINDOWEDGE | WindowStyles.WS_EX_TOOLWINDOW);
						break;
					}
				}

				if (ShowInTaskbar) {
					cp.ExStyle |= (int)WindowStyles.WS_EX_APPWINDOW;
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

				if (HelpButton) {
					cp.ExStyle |= (int)WindowStyles.WS_EX_CONTEXTHELP;
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
				return new Size (250, 250);
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
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Static Methods
		[MonoTODO("Figure out the math")]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static SizeF GetAutoScaleSize(Font font) {
			SizeF	result;

			result = new SizeF(250, 250);
			result.Width *= font.SizeInPoints / 12;
			result.Height *= font.SizeInPoints / 12;
			return result;
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public void Activate() {
			Form	active;

			// The docs say activate only activates if our app is already active
			active = ActiveForm;
			if ((active != null) && (this != active)) {
				XplatUI.Activate(window.Handle);
			}
		}

		public void AddOwnedForm(Form ownedForm) {
			owned_forms.Add(ownedForm);
		}

		public void Close () {
			CancelEventArgs args = new CancelEventArgs ();
			OnClosing (args);
			if (!args.Cancel) {
				OnClosed (EventArgs.Empty);
				closing = true;
				return;
			}
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

		public DialogResult ShowDialog() {
			return ShowDialog(null);
		}

		public DialogResult ShowDialog(IWin32Window ownerWin32) {
			Form		previous;

			#if broken
			Control		owner = null;

			if (ownerWin32 != null) {
				owner = Control.FromHandle(ownerWin32.Handle);
			}
			#endif

			if (is_modal) {
				return DialogResult.None;
			}

			if (Visible) {
				throw new InvalidOperationException("Already visible forms cannot be displayed as a modal dialog. Set the Visible property to 'false' prior to calling Form.ShowDialog.");
			}

			#if broken
			// Can't do this, will screw us in the modal loop
			form_parent_window.Parent = owner;
			#endif

			previous = Form.ActiveForm;

			if (!IsHandleCreated) {
				CreateControl();
			}

			XplatUI.SetModal(window.Handle, true);

			Show();
			PerformLayout();

			is_modal = true;
			Application.ModalRun(this);
			is_modal = false;
			Hide();

			XplatUI.SetModal(window.Handle, false);

			if (previous != null) {
				// Cannot use Activate(), it has a check for the current active window...
				XplatUI.Activate(previous.window.Handle);
			}

			return DialogResult;
		}

		public override string ToString() {
			return GetType().FullName.ToString() + ", Text: " + Text;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[MonoTODO("Finish when MDI is more complete")]
		protected void ActivateMdiChild(Form form) {
			OnMdiChildActivate(EventArgs.Empty);
			throw new NotImplementedException();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void AdjustFormScrollbars(bool displayScrollbars) {
			base.AdjustFormScrollbars (displayScrollbars);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected void ApplyAutoScaling() {
			// Hm, not sure what this does
		}

		protected void CenterToParent() {
			Control	ctl;
			int	w;
			int	h;

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
			if (parent != null) {
				ctl = parent;
			} else if (owner != null) {
				ctl = owner;
			}

			if (owner != null) {
				this.Location = new Point(ctl.Left + ctl.Width / 2 - w /2, ctl.Top + ctl.Height / 2 - h / 2);
			}
		}

		protected void CenterToScreen() {
			Size	DisplaySize;
			int	w;
			int	h;

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

			XplatUI.GetDisplaySize(out DisplaySize);
			this.Location = new Point(DisplaySize.Width / 2 - w / 2, DisplaySize.Height / 2 - h / 2);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override Control.ControlCollection CreateControlsInstance() {
			return base.CreateControlsInstance ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void CreateHandle() {
			base.CreateHandle ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void DefWndProc(ref Message m) {
			base.DefWndProc (ref m);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose (disposing);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnActivated(EventArgs e) {
			if (Activated != null) {
				Activated(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClosed(EventArgs e) {
			if (Closed != null) {
				Closed(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e) {
			if (Closing != null) {
				Closing(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnCreateControl() {
			base.OnCreateControl ();
			if (this.ActiveControl == null) {
				bool visible;

				// This visible hack is to work around CanSelect always being false if one of the parents
				// is not visible; and we by default create Form invisible...
				visible = this.is_visible;
				this.is_visible = true;

				if (SelectNextControl(this, true, true, true, true) == false) {
					Select(this);
				}

				this.is_visible = visible;
			}
			
			if (menu != null) {
				XplatUI.SetMenu(window.Handle, menu.Handle);
			}
			
			OnLoad(EventArgs.Empty);

			// Send initial location
			OnLocationChanged(EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnDeactivate(EventArgs e) {
			if (Deactivate != null) {
				Deactivate(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInputLanguageChanged(InputLanguageChangedEventArgs e) {
			if (InputLanguageChanged!=null) {
				InputLanguageChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnInputLanguageChanging(InputLanguageChangingEventArgs e) {
			if (InputLanguageChanging!=null) {
				InputLanguageChanging(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnLoad(EventArgs e) {
			if (Load != null) {
				Load(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMaximizedBoundsChanged(EventArgs e) {
			if (MaximizedBoundsChanged != null) {
				MaximizedBoundsChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMaximumSizeChanged(EventArgs e) {
			if (MaximumSizeChanged != null) {
				MaximumSizeChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMdiChildActivate(EventArgs e) {
			if (MdiChildActivate != null) {
				MdiChildActivate(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMenuComplete(EventArgs e) {
			if (MenuComplete != null) {
				MenuComplete(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMenuStart(EventArgs e) {
			if (MenuStart != null) {
				MenuStart(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected virtual void OnMinimumSizeChanged(EventArgs e) {
			if (MinimumSizeChanged != null) {
				MinimumSizeChanged(this, e);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnPaint (PaintEventArgs pevent) {
			base.OnPaint (pevent);
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
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
			if (base.ProcessCmdKey (ref msg, keyData)) {
				return true;
			}

			// Give our menu a shot
			if (menu != null) {
				return menu.ProcessCmdKey(ref msg, keyData);
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
				if (keyData == Keys.Enter && accept_button != null) {
					accept_button.PerformClick();
					return true;
				} else if (keyData == Keys.Escape && cancel_button != null) {
					cancel_button.PerformClick();
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override bool ProcessKeyPreview(ref Message msg) {
			if (key_preview) {
				if (ProcessKeyEventArgs(ref msg)) {
					return true;
				}
			}
			return base.ProcessKeyPreview (ref msg);
		}

		protected override bool ProcessTabKey(bool forward) {
			return SelectNextControl(ActiveControl, forward, true, true, true);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void ScaleCore(float dx, float dy) {
			base.ScaleCore (dx, dy);
		}

		protected override void Select(bool directed, bool forward) {
			Form	parent;

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
			base.SetBoundsCore (x, y, width, height, specified);
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

			base.SetClientSizeCore (x, y);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void SetVisibleCore(bool value) {
			base.SetVisibleCore (value);
		}

		protected override void UpdateDefaultButton() {
			base.UpdateDefaultButton ();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
			switch((Msg)m.Msg) {
				case Msg.WM_CLOSE: {
					CancelEventArgs args = new CancelEventArgs();

					OnClosing(args);

					if (!args.Cancel) {
						OnClosed(EventArgs.Empty);
						closing = true;
						base.WndProc(ref m);
						break;
					}
					break;
				}

				case Msg.WM_ACTIVATE: {
					if (m.WParam != (IntPtr)WindowActiveFlags.WA_INACTIVE) {
						OnActivated(EventArgs.Empty);
					} else {
						OnDeactivate(EventArgs.Empty);
					}
					return;
				}

				case Msg.WM_KILLFOCUS: {
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_SETFOCUS: {
#if not
					if (this.ActiveControl != null) {
						ActiveControl.Focus();
					}
#endif
					base.WndProc(ref m);
					return;
				}

				// Menu drawing
				case Msg.WM_NCLBUTTONDOWN: {
					if (this.menu != null) {
						int x = LowOrder ((int) m.LParam.ToInt32 ()) ;
						int y = HighOrder ((int) m.LParam.ToInt32 ());						
						menu.OnMouseDown(this, new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), mouse_clicks, x, y, 0));
					}
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_NCMOUSEMOVE: {
					if (this.menu != null) {
						menu.OnMouseMove(this, new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
							mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()), HighOrder ((int) m.LParam.ToInt32 ()), 0));
					}
					base.WndProc(ref m);
					return;
				}

				case Msg.WM_NCPAINT: {
					if (this.menu != null) {
						Point pnt = XplatUI.GetMenuOrigin(window.Handle);
						MenuAPI.DrawMenuBar (menu.Handle, new Rectangle (pnt.X, pnt.Y, ClientSize.Width, 0));
					}

					base.WndProc(ref m);
					return;
				}

				// This message is only received under Win32
				case Msg.WM_NCCALCSIZE: {
					XplatUIWin32.NCCALCSIZE_PARAMS	ncp;

					if ((menu != null) && (m.WParam == (IntPtr)1)) {
						ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(m.LParam, typeof(XplatUIWin32.NCCALCSIZE_PARAMS));

						// Adjust for menu
						ncp.rgrc1.top += ThemeEngine.Current.CalcMenuBarSize (DeviceContext, menu.menu_handle, ClientSize.Width);
						Marshal.StructureToPtr(ncp, m.LParam, true);
					}
					DefWndProc(ref m);
					break;
				}

				case Msg.WM_GETMINMAXINFO: {
					XplatUIWin32.MINMAXINFO	mmi;

					if (m.LParam != IntPtr.Zero) {
						mmi = (XplatUIWin32.MINMAXINFO)Marshal.PtrToStructure(m.LParam, typeof(XplatUIWin32.MINMAXINFO));
						default_maximized_bounds = new Rectangle(mmi.ptMaxPosition.x, mmi.ptMaxPosition.y, mmi.ptMaxSize.x, mmi.ptMaxSize.y);
						if (maximized_bounds != Rectangle.Empty) {
							mmi.ptMaxSize.x = maximized_bounds.Width;
							mmi.ptMaxSize.y = maximized_bounds.Height;
						}

						Marshal.StructureToPtr(mmi, m.LParam, false);
					}
					break;
				}

				default: {
					base.WndProc (ref m);
					break;
				}
			}
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler Activated;
		public event EventHandler Closed;
		public event CancelEventHandler Closing;
		public event EventHandler Deactivate;
		public event InputLanguageChangedEventHandler InputLanguageChanged;
		public event InputLanguageChangingEventHandler InputLanguageChanging;
		public event EventHandler Load;
		public event EventHandler MaximizedBoundsChanged;
		public event EventHandler MaximumSizeChanged;
		public event EventHandler MdiChildActivate;
		public event EventHandler MenuComplete;
		public event EventHandler MenuStart;
		public event EventHandler MinimumSizeChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged;
		#endregion	// Events
	}
}
