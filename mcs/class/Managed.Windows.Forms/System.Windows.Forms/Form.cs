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
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Windows.Forms {
	public class Form : ContainerControl {
		#region Local Variables
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
		private bool			key_preview;
		private MainMenu		menu;
		internal FormParentWindow	form_parent_window;
		private	Icon			icon;
		private Size			maximum_size;
		private Size			minimum_size;
		#endregion	// Local Variables

		#region Private Classes

		// This class will take over for the client area
		internal class FormParentWindow : Control {
			#region FormParentWindow Class Local Variables
			internal Form	owner;
			#endregion	// FormParentWindow Class Local Variables

			#region FormParentWindow Class Constructor
			internal FormParentWindow(Form owner) : base() {
				this.owner = owner;

				this.Width = 250;
				this.Height = 250;

				BackColor = owner.BackColor;
				Text = "FormParent";
				this.Location = new Point(0, 0);
				this.Dock = DockStyle.Fill;
				this.is_visible = false;

				MouseDown += new MouseEventHandler (OnMouseDownForm); 
				MouseMove += new MouseEventHandler (OnMouseMoveForm); 
				owner.TextChanged += new EventHandler(OnFormTextChanged);
			}
			#endregion	// FormParentWindow Class Constructor

			#region FormParentWindow Class Protected Instance Methods
			protected override CreateParams CreateParams {
				get {
					CreateParams cp;

					cp = base.CreateParams;

					cp.Style = (int)(WindowStyles.WS_OVERLAPPEDWINDOW | 
						WindowStyles.WS_CLIPSIBLINGS | 
						WindowStyles.WS_CLIPCHILDREN);

					cp.Width = 250;
					cp.Height = 250;

					if (owner != null) {
						if (owner.ShowInTaskbar) {
							cp.ExStyle |= (int)WindowStyles.WS_EX_APPWINDOW;
						}

						if (owner.MaximizeBox) {
							cp.Style |= (int)WindowStyles.WS_MAXIMIZEBOX;
						}

						if (owner.MinimizeBox) {
							cp.Style |= (int)WindowStyles.WS_MINIMIZEBOX;
						}

						if (owner.ControlBox) {
							cp.Style |= (int)WindowStyles.WS_SYSMENU;
						}

						if (owner.HelpButton) {
							cp.ExStyle |= (int)WindowStyles.WS_EX_CONTEXTHELP;
						}
					} else {
						// Defaults
						cp.Style |= (int)(WindowStyles.WS_SYSMENU | WindowStyles.WS_MINIMIZEBOX | WindowStyles.WS_MAXIMIZEBOX);
						cp.ExStyle |=  (int)WindowStyles.WS_EX_APPWINDOW;
					}

					return cp;
				}
			}

			protected override void OnResize(EventArgs e) {
				base.OnResize(e);
				//owner.SetBoundsCore(owner.Bounds.X, owner.Bounds.Y, ClientSize.Width, ClientSize.Height, BoundsSpecified.All);
				if (owner.menu == null) {
					owner.SetBoundsCore(0, 0, ClientSize.Width, ClientSize.Height, BoundsSpecified.All);
				} else {
					int menu_height;

					menu_height = MenuAPI.MenuBarCalcSize(DeviceContext, owner.Menu.menu_handle, ClientSize.Width);
					Invalidate (new Rectangle (0, 0, ClientSize.Width, menu_height));					
					owner.SetBoundsCore(0, menu_height, ClientSize.Width, ClientSize.Height-menu_height, BoundsSpecified.All);
				}
			}

			protected override void OnPaint(PaintEventArgs pevent) {
				OnDrawMenu (pevent.Graphics);
			}

			protected override void WndProc(ref Message m) {
				switch((Msg)m.Msg) {
					case Msg.WM_CLOSE: {
						CancelEventArgs args = new CancelEventArgs();

						owner.OnClosing(args);

						if (!args.Cancel) {
							owner.OnClosed(EventArgs.Empty);
							owner.closing = true;
							base.WndProc(ref m);
							break;
						}
						break;
					}

					default: {
						base.WndProc (ref m);
						break;
					}
				}
			}
			#endregion	// FormParentWindow Class Protected Instance Methods

			#region FormParentWindow Class Private Methods
			private void OnMouseDownForm (object sender, MouseEventArgs e) {			
				if (owner.menu != null)
					owner.menu.OnMouseDown (owner, e);
			}

			private void OnMouseMoveForm (object sender, MouseEventArgs e) {			
				if (owner.menu != null)
					owner.menu.OnMouseMove (owner, e);
			}
		
		
			private void OnDrawMenu (Graphics dc) {
				if (owner.menu != null) {													
					Rectangle rect = new Rectangle (0,0, Width, 0);			
					MenuAPI.DrawMenuBar (dc, owner.menu.Handle, rect);
				}			
			}
			private void OnFormTextChanged(object sender, EventArgs e) {
				this.Text = ((Control)sender).Text;
			}
			#endregion	// FormParentWindow Class Private Methods
		}
		#endregion	// Private Classes

		#region Public Classes
		public class ControlCollection : Control.ControlCollection {
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

			owned_forms = new Form.ControlCollection(this);
		}
		#endregion	// Public Constructor & Destructor


		#region Public Static Properties

		public static Form ActiveForm {
			get {
				//FIXME: create next driver call
				return null;
			}
		}

		#endregion	// Public Static Properties

		#region Public Instance Properties
		public IButtonControl AcceptButton {
			get {
				return accept_button;
			}

			set {
				accept_button = value;
			}
		}
			
		public bool AutoScale {
			get {
				return autoscale;
			}

			set {
				autoscale=value;
			}
		}

		public virtual Size AutoScaleBaseSize {
			get {
				return autoscale_base_size;
			}

			set {
				autoscale_base_size=value;
			}
		}

		public IButtonControl CancelButton {
			get {
				return cancel_button;
			}

			set {
				cancel_button = value;
			}
		}

		public bool ControlBox {
			get {
				return control_box;
			}

			set {
				if (control_box != value) {
					control_box = value;
				}
			}
		}

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

		public FormBorderStyle FormBorderStyle {
			get {
				return formBorderStyle;
			}
			set {
				formBorderStyle = value;
				Invalidate ();
			}
		}

		public bool HelpButton {
			get {
				return help_button;
			}

			set {
				if (help_button != value) {
					help_button = value;
				}
			}
		}

		public Icon Icon {
			get {
				return icon;
			}

			set {
				if (icon != value) {
					icon = value;
				}
			}
		}


		public bool KeyPreview {
			get {
				return key_preview;
			}

			set {
				key_preview = value;
			}
		}

		public bool MaximizeBox {
			get {
				return maximize_box;
			}
			set {
				if (maximize_box != value) {
					maximize_box = value;
				}
			}
		}

		public Size MaximumSize {
			get {
				return maximum_size;
			}

			set {
				if (maximum_size != value) {
					maximum_size = value;
				}
			}
		}

		public MainMenu Menu {
			get {
				return menu;
			}

			set {
				if (menu != value) {
					if (value == null) {
						form_parent_window.Width = form_parent_window.Width;	// Trigger a resize
					}

					menu = value;

					// To simulate the non-client are for menus we create a 
					// new control as the 'client area' of our form.  This
					// way, the origin stays 0,0 and we don't have to fiddle with
					// coordinates. The menu area is part of the original container
					if (menu != null) {
						form_parent_window.Width = form_parent_window.Width;	// Trigger a resize
					}

					menu.SetForm (this);
					MenuAPI.SetMenuBarWindow (menu.Handle, form_parent_window);
				}
			}
		}

		public bool MinimizeBox {
			get {
				return minimize_box;
			}
			set {
				if (minimize_box != value) {
					minimize_box = value;
				}
			}
		}

		public Size MinimumSize {
			get {
				return minimum_size;
			}

			set {
				if (minimum_size != value) {
					minimum_size = value;
				}
			}
		}

		public bool Modal  {
			get {
				return is_modal;
			}
		}

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

		public bool ShowInTaskbar {
			get {
				return show_in_taskbar;
			}
			set {
				if (show_in_taskbar != value) {
					show_in_taskbar = value;
				}
			}
		}

		public FormStartPosition StartPosition {
			get {
				return start_position;
			}

			set {
				if (start_position == FormStartPosition.WindowsDefaultLocation) {		// Only do this if it's not set yet
					start_position = value;
					if (form_parent_window.IsHandleCreated) {
						switch(start_position) {
							case FormStartPosition.CenterParent: {
								if (Parent!=null && Width>0 && Height>0) {
									this.Location = new Point(Parent.Size.Width/2-Width/2, Parent.Size.Height/2-Height/2);
								}
								break;
							}

							case FormStartPosition.CenterScreen: {
								if (Width>0 && Height>0) {
									Size	DisplaySize;

									XplatUI.GetDisplaySize(out DisplaySize);
									this.Location = new Point(DisplaySize.Width/2-Width/2, DisplaySize.Height/2-Height/2);
								}
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

		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		[MonoTODO("Need to add MDI support")]
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = new CreateParams();

				if (this.form_parent_window == null) {
					form_parent_window = new FormParentWindow(this);
				}

				cp.Caption = "ClientArea";
				cp.ClassName=XplatUI.DefaultClassName;
				cp.ClassStyle = 0;
				cp.ExStyle=0;
				cp.Param=0;
				cp.Parent = this.form_parent_window.window.Handle;
				cp.X = Left;
				cp.Y = Top;
				cp.Width = Width;
				cp.Height = Height;
				
				cp.Style = (int)WindowStyles.WS_CHILD;
				cp.Style |= (int)WindowStyles.WS_VISIBLE;
				cp.Style |= (int)WindowStyles.WS_CLIPSIBLINGS;
				cp.Style |= (int)WindowStyles.WS_CLIPCHILDREN;

				return cp;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size (250, 250);
			}
		}		

		protected override void OnPaint (PaintEventArgs pevent)
		{
			base.OnPaint (pevent);
		}		
		
		#endregion	// Protected Instance Properties


		#region Public Static Methods
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public void AddOwnedForm(Form ownedForm) {
			owned_forms.Add(ownedForm);
		}

		public void RemoveOwnedForm(Form ownedForm) {
			owned_forms.Remove(ownedForm);
		}

		public DialogResult ShowDialog() {
			return ShowDialog(null);
		}

		public DialogResult ShowDialog(IWin32Window ownerWin32) {
			Control		owner = null;

			if (ownerWin32 != null) {
				owner = Control.FromHandle(ownerWin32.Handle);
			}

			if (is_modal) {
				return DialogResult.None;
			}

			if (Visible) {
				throw new InvalidOperationException("Already visible forms cannot be displayed as a modal dialog. Set the Visible property to 'false' prior to calling Form.ShowDialog.");
			}

			if (!IsHandleCreated) {
				CreateControl();
			}

			XplatUI.SetModal(form_parent_window.window.Handle, true);

			Show();

			is_modal = true;
			Application.ModalRun(this);
			is_modal = false;
			Hide();

			XplatUI.SetModal(form_parent_window.window.Handle, false);

			return DialogResult;
		}

		public void Close ()
		{
			CancelEventArgs args = new CancelEventArgs ();
			OnClosing (args);
			if (!args.Cancel) {
				OnClosed (EventArgs.Empty);
				closing = true;
				return;
			}
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle() {
			base.CreateHandle ();
		}

		protected override void OnCreateControl() {
			base.OnCreateControl ();
			OnLoad(EventArgs.Empty);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed (e);
		}


		protected override void OnResize(EventArgs e) {
			base.OnResize(e);
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

		protected override bool ProcessDialogKey(Keys keyData) {
			if (keyData == Keys.Enter && accept_button != null) {
				accept_button.PerformClick();
				return true;
			} else if (keyData == Keys.Enter && cancel_button != null) {
				cancel_button.PerformClick();
				return true;
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

#if topmost_workaround
				case Msg.WM_ACTIVATE: {
					if (this.OwnedForms.Length>0) {
						XplatUI.SetZOrder(this.OwnedForms[0].window.Handle, this.window.Handle, false, false);
					}
					break;
				}
#endif
				default: {
					base.WndProc (ref m);
					break;
				}
			}
		}
		#endregion	// Protected Instance Methods

		#region Events
		protected virtual void OnActivated(EventArgs e) {
			if (Activated != null) {
				Activated(this, e);
			}
		}

		protected virtual void OnClosed(EventArgs e) {
			if (Closed != null) {
				Closed(this, e);
			}
		}

		protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e) {
			if (Closing != null) {
				Closing(this, e);
			}
		}

		protected virtual void OnDeactivate(EventArgs e) {
			if (Deactivate != null) {
				Deactivate(this, e);
			}
		}

		protected virtual void OnInputLanguageChanged(InputLanguageChangedEventArgs e) {
			if (InputLanguageChanged!=null) {
				InputLanguageChanged(this, e);
			}
		}

		protected virtual void OnInputLanguageChanging(InputLanguageChangingEventArgs e) {
			if (InputLanguageChanging!=null) {
				InputLanguageChanging(this, e);
			}
		}

		protected virtual void OnLoad(EventArgs e) {
			if (Load != null) {
				Load(this, e);
			}
		}

		protected virtual void OnMaximizedBoundsChanged(EventArgs e) {
			if (MaximizedBoundsChanged != null) {
				MaximizedBoundsChanged(this, e);
			}
		}

		protected virtual void OnMaximumSizeChanged(EventArgs e) {
			if (MaximumSizeChanged != null) {
				MaximumSizeChanged(this, e);
			}
		}

		protected virtual void OnMdiChildActivate(EventArgs e) {
			if (MdiChildActivate != null) {
				MdiChildActivate(this, e);
			}
		}

		protected virtual void OnMenuComplete(EventArgs e) {
			if (MenuComplete != null) {
				MenuComplete(this, e);
			}
		}

		protected virtual void OnMenuStart(EventArgs e) {
			if (MenuStart != null) {
				MenuStart(this, e);
			}
		}

		protected virtual void OnMinimumSizeChanged(EventArgs e) {
			if (MinimumSizeChanged != null) {
				MinimumSizeChanged(this, e);
			}
		}

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
		#endregion	// Events
	}
}
