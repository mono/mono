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
//
// $Revision: 1.16 $
// $Modtime: $
// $Log: Form.cs,v $
// Revision 1.16  2004/10/14 06:17:58  ravindra
// Fixed class signature. ShowDialog (Control) is not a public method.
//
// Revision 1.15  2004/10/08 08:50:29  jordi
// more menu work
//
// Revision 1.14  2004/10/02 19:05:52  pbartok
// - Added KeyPreview property
// - Added Menu property (still incomplete, pending Jordi's menu work)
// - Implemented ProcessCmdKey
// - Implemented ProcessDialogKey
// - Implemented ProcessKeyPreview
//
// Revision 1.13  2004/10/01 17:53:26  jackson
// Implement the Close method so work on MessageBox can continue.
//
// Revision 1.12  2004/09/23 19:08:59  jackson
// Temp build fixage
//
// Revision 1.11  2004/09/22 20:09:44  pbartok
// - Added Form.ControllCollection class
// - Added handling for Form owners: Owner, OwnedForms, AddOwnedForm,
//   RemoveOwnedForm (still incomplete, missing on-top and common
//   minimize/maximize behaviour)
// - Added StartPosition property (still incomplete, does not use when
//   creating the form)
// - Added ShowDialog() methods (still incomplete, missing forcing the dialog
//   modal)
//
// Revision 1.10  2004/09/13 16:56:04  pbartok
// - Fixed #region names
// - Moved properties and methods into their proper #regions
//
// Revision 1.9  2004/09/13 16:51:29  pbartok
// - Added Accept and CancelButton properties
// - Added ProcessDialogKey() method
//
// Revision 1.8  2004/09/01 02:05:18  pbartok
// - Added (partial) implementation of DialogResult; rest needs to be
//   implemented when the modal loop code is done
//
// Revision 1.7  2004/08/23 22:10:02  pbartok
// - Fixed handling of WM_CLOSE message
// - Removed debug output
//
// Revision 1.6  2004/08/22 21:10:30  pbartok
// - Removed OverlappedWindow style from Control, instead it's default
//   now is child
// - Made form windows OverlappedWindow by default
//
// Revision 1.5  2004/08/19 21:30:37  pbartok
// - Added handling of WM_CLOSE
//
// Revision 1.4  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.3  2004/08/04 21:13:47  pbartok
// - Added AutoScale properties
//
// Revision 1.2  2004/07/13 15:31:45  jordi
// commit: new properties and fixes form size problems
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
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

		private static bool		autoscale;
		private static Size		autoscale_base_size;
		private bool			is_modal;
		internal bool			end_modal;			// This var is being monitored by the application modal loop
		private IButtonControl		accept_button;
		private IButtonControl		cancel_button;
		private DialogResult		dialog_result;
		private FormStartPosition	start_position;
		private Form			owner;
		private Form.ControlCollection	owned_forms;
		private bool			key_preview;
		private MainMenu		menu;
		#endregion	// Local Variables

		#region Public Classes
		public class ControlCollection : Control.ControlCollection {
			Form	form_owner;

			public ControlCollection(Form owner) : base(owner) {
				this.form_owner = owner;
			}

			public override void Add(Control value) {
				base.Add(value);
				((Form)value).owner = form_owner;
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
			key_preview = false;
			menu = null;
		}
		#endregion	// Public Constructor & Destructor

		#region Private and Internal Methods
		private DialogResult ShowDialog(Control owner) {
			if (is_modal) {
				return DialogResult.None;
			}

			if (owner != null) {
				//this.Parent = owner;
//XplatUIWin32.EnableWindow(owner.window.Handle, false);
			} else {
				;; // get an owner
			}

			if (IsHandleCreated) {
//				if (owner != this.owner) {
//					this.RecreateHandle();
//				}
			} else {
				CreateControl();
			}

			Show();

			is_modal = true;
			Application.ModalRun(this);
			is_modal = false;

			Hide();
//XplatUIWin32.EnableWindow(owner.window.Handle, true);

			return DialogResult;
		}
		#endregion	// Private and Internal Methods

		#region Public Static Properties
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

		public bool KeyPreview {
			get {
				return key_preview;
			}

			set {
				key_preview = value;
			}
		}

		public MainMenu Menu {
			get {
				return menu;
			}

			set {
				if (menu != value) {
					// FIXME - I have to wait for jordi to finish menus before I can do some of this
					// We'll need a way to store what form owns the menu inside the menu; I'd
					// have to set this here.
					menu = value;
					menu.SetForm (this);
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
					owner.RemoveOwnedForm(this);
					owner = value;
					owner.AddOwnedForm(this);
				}
			}
		}

		public FormStartPosition StartPosition {
			get {
				return start_position;
			}

			set {
				start_position = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		[MonoTODO("Need to add MDI support")]
		protected override CreateParams CreateParams {
			get {
				CreateParams create_params = new CreateParams();

				create_params.Caption = "";

				create_params.ClassName=XplatUI.DefaultClassName;
				create_params.ClassStyle = 0;
				create_params.ExStyle=0;
				create_params.Parent=IntPtr.Zero;
				create_params.Param=0;
				create_params.X = Left;
				create_params.Y = Top;
				create_params.Width = Width;
				create_params.Height = Height;
				
				create_params.Style = (int)WindowStyles.WS_OVERLAPPEDWINDOW;
				create_params.Style |= (int)WindowStyles.WS_VISIBLE;

				switch(start_position) {
					case FormStartPosition.CenterParent: {
						if (Parent!=null && Width>0 && Height>0) {
							Size	ParentSize;

							ParentSize = Parent.Size;

							create_params.X = Parent.Size.Width/2-Width/2;
							create_params.Y = Parent.Size.Height/2-Height/2;
						}
						break;
					}

					case FormStartPosition.CenterScreen: {
						if (Width>0 && Height>0) {
							Size	DisplaySize;

							XplatUI.GetDisplaySize(out DisplaySize);

							create_params.X = DisplaySize.Width/2-Width/2;
							create_params.Y = DisplaySize.Height/2-Height/2;
						}
						break;
					}

					case FormStartPosition.Manual: {
						break;
					}

					case FormStartPosition.WindowsDefaultBounds: {
						create_params.X = -1;
						create_params.Y = -1;
						create_params.Width = -1;
						create_params.Height = -1;
						break;
					}

					case FormStartPosition.WindowsDefaultLocation: {
						create_params.X = -1;
						create_params.Y = -1;
						break;
					}
				}

				return create_params;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size (250, 250);
			}
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

		public DialogResult ShowDialog(IWin32Window owner) {
			return ShowDialog(Control.FromHandle(owner.Handle));
		}

		public void Close ()
		{
			CancelEventArgs args = new CancelEventArgs (true);
			OnClosing (args);
			if (!args.Cancel) {
				OnClosed (EventArgs.Empty);
				return;
			}
			closing = true;
		}

		#endregion	// Public Instance Methods

		#region Protected Instance Methods
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

		protected override void WndProc(ref Message m) {
			switch((Msg)m.Msg) {
				case Msg.WM_CLOSE: {
					CancelEventArgs args = new CancelEventArgs(true);

					OnClosing(args);

					if (!args.Cancel) {
						OnClosed(EventArgs.Empty);
						base.WndProc(ref m);
						break;
					}

					closing = true;
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
