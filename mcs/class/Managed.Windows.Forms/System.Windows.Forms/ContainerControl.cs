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


// NOT COMPLETE

using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms {
	public class ContainerControl : ScrollableControl, IContainerControl {
		private Control active_control;
		private Control focused_control;
		private Control	unvalidated_control;

		#region Public Constructors
		public ContainerControl() {
			active_control = null;
			focused_control = null;
			unvalidated_control = null;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[Browsable (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Control ActiveControl {
			get {
				return active_control;
			}

			set {
				if ((active_control==value) || (value==null)) {
					return;
				}

				active_control = value;

				if (!Contains(value) && this != value) {
					throw new ArgumentException("Not a child control");
				}

				// Scroll control into view

				// Let the control know it's selected
				Select(value);
			}
		}

		[Browsable (false)]
		public override BindingContext BindingContext {
			get {
				if (base.BindingContext == null) {
					base.BindingContext = new BindingContext();
				}
				return base.BindingContext;
			}

			set {
				base.BindingContext = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form ParentForm {
			get {
				Control parent;

				parent = this.parent;

				while (parent != null) {
					if (parent is Form) {
						return (Form)parent;
					}
					parent = parent.parent;
				}

				return null;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		#endregion	// Public Instance Methods

		#region Public Instance Methods
		[MonoTODO]
		public bool Validate() {
			throw new NotImplementedException();
		}

		bool IContainerControl.ActivateControl(Control control) {
			return Select(control);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void AdjustFormScrollbars(bool displayScrollbars) {
			// Internal MS
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnControlRemoved(ControlEventArgs e) {
			if (e.Control == this.unvalidated_control) {
				this.unvalidated_control = null;
			}

			if (e.Control == this.active_control) {
				this.unvalidated_control = null;
			}

			base.OnControlRemoved(e);
		}

		protected override void OnCreateControl() {
			base.OnCreateControl();
			// MS seems to call this here, it gets the whole databinding process started
			OnBindingContextChanged (EventArgs.Empty);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override bool ProcessDialogChar(char charCode) {
			if (GetTopLevel()) {
				if (ProcessMnemonic(charCode)) {
					return true;
				}
			}
			return base.ProcessDialogChar(charCode);
		}

		protected override bool ProcessDialogKey(Keys keyData) {
			Keys	key;
			bool	forward;

			key = keyData & Keys.KeyCode;
			forward = true;

			switch (key) {
				case Keys.Tab: {
					if (ProcessTabKey((Control.ModifierKeys & Keys.Shift) == 0)) {
						return true;
					}
					break;
				}

				case Keys.Left: {
					forward = false;
					goto case Keys.Down;
				}

				case Keys.Up: {
					forward = false;
					goto case Keys.Down;
				}

				case Keys.Right: {
					goto case Keys.Down;
				}
				case Keys.Down: {
					if (SelectNextControl(active_control, forward, false, false, true)) {
						return true;
					}
					break;
				} 


			}
			return base.ProcessDialogKey(keyData);
		}

		protected override bool ProcessMnemonic(char charCode) {
			bool	wrapped;
			Control	c;

			wrapped = false;
			c = active_control;

			do {
				c = GetNextControl(c, true);
				if (c != null) {
					// This is stupid. I want to be able to call c.ProcessMnemonic directly
					if (c.ProcessControlMnemonic(charCode)) {
						return(true);
					}
					continue;
				} else {
					if (wrapped) {
						break;
					}
					wrapped = true;
				}
			} while (c != active_control);
			
			return false;
		}

		protected virtual bool ProcessTabKey(bool forward) {
			return SelectNextControl(active_control, forward, true, true, true);
		}

		protected override void Select(bool directed, bool forward) {
			base.Select(directed, forward);
		}

		protected virtual void UpdateDefaultButton() {
			// MS Internal
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods
	}
}
