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


using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
	public class ContainerControl : ScrollableControl, IContainerControl {
		private Control		active_control;
		private Control		unvalidated_control;

		// This is an internal hack that allows some container controls
		// to not auto select their child when they are activated
		internal bool 		auto_select_child = true;
#if NET_2_0
		private SizeF		auto_scale_dimensions;
		private AutoScaleMode	auto_scale_mode;
		private bool		auto_scale_mode_set;
		private bool		auto_scale_pending;
		private bool		is_auto_scaling;
#endif

		internal bool validation_failed; //track whether validation was cancelled by a validating control

		#region Public Constructors
		public ContainerControl() {
			active_control = null;
			unvalidated_control = null;
			ControlRemoved += new ControlEventHandler(OnControlRemoved);
#if NET_2_0
			auto_scale_dimensions = SizeF.Empty;
			auto_scale_mode = AutoScaleMode.Inherit;
#endif
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
				if (value==null || (active_control == value && active_control.Focused)) {
					return;
				}

				if (!Contains(value)) {
					throw new ArgumentException("Cannot activate invisible or disabled control.");
				}

				// Fire the enter and leave events if possible
				Form form = FindForm ();
				Control active = GetMostDeeplyNestedActiveControl (form == null ? this : form);
				Control common_ancestor = GetCommonContainer (active, value);
				ArrayList chain = new ArrayList ();
				ArrayList validation_chain = new ArrayList ();
				Control walk = active;

				// we split this up into three steps:
				//    1. walk up the tree (if we need to) to our root, firing leave events.
				//    2. validate.
				//    3. walk down the tree (if we need to), firing enter events.

				// "our root" is either the common ancestor of the current active
				// control and the new active control, or the current active control,
				// or the new active control.  That is, it's either one of these three
				// configurations:

				//  (1)     root	    (2)  new   	      (3)  current
				//          /  \	       	/   \		    /  	\
				//	  ...  	...	      ...   ...		  ...  	...
				//     	  /      \	      /	       			  \
				//     current 	 new   	   current   			  new


				// note (3) doesn't require any upward walking, and no leave events are generated.
				//      (2) doesn't require any downward walking, and no enter events are generated.

				// as we walk up the tree, we generate a list of all controls which cause
				// validation.  After firing the leave events, we invoke (in order starting from
				// the most deeply nested) their Validating event.  If one sets CancelEventArgs.Cancel
				// to true, we ignore the control the user wanted to set ActiveControl to, and use
				// the Validating control instead.

				bool fire_enter = true;
				Control root = common_ancestor;

				active_control = value;

				// Generate the leave messages
				while (walk != common_ancestor && walk != null) {
					if (walk == value) {
						root = value;
						fire_enter = false;
						break;
					}
					walk.FireLeave ();
					/* clear our idea of the active control as we go back up */
					if (walk is ContainerControl)
						((ContainerControl)walk).active_control = null;

					if (walk.CausesValidation)
						validation_chain.Add (walk);

					walk = walk.Parent;
				}

				validation_failed = false;
				for (int i = 0; i < validation_chain.Count; i ++) {
					if (!ValidateControl ((Control)validation_chain[i])) {
						active_control = value = (Control)validation_chain[i];
						fire_enter = true;
						validation_failed = true;
					}
				}

				if (fire_enter) {
					walk = value;
					while (walk != root && walk != null) {
						chain.Add (walk);
						walk = walk.Parent;
					}

					if (root != null && walk == root && !(root is ContainerControl))
						chain.Add (walk);

					for (int i = chain.Count - 1; i >= 0; i--) {
						walk = (Control) chain [i];
						walk.FireEnter ();
					}
				}

				walk = this;
				Control ctl = this;
				while (walk != null) {
					if (walk.Parent is ContainerControl) {
						((ContainerControl) walk.Parent).active_control = ctl;
						ctl = walk.Parent;
					}
					walk = walk.Parent;
				}

				if (this is Form)
					CheckAcceptButton();

				// Scroll control into view
				ScrollControlIntoView(active_control);
				
				
				walk = this;
				ctl = this;
				while (walk != null) {
					if (walk.Parent is ContainerControl) {
						ctl = walk.Parent;
					}
					walk = walk.Parent;
				}
				
				// Let the control know it's selected
				if (ctl.InternalContainsFocus)
					SendControlFocus (active_control);
			}
		}

		private bool ValidateControl (Control c)
		{
			CancelEventArgs e = new CancelEventArgs ();

			c.FireValidating (e);

			if (e.Cancel)
				return false;

			c.FireValidated ();
			return true;
		}

		private Control GetMostDeeplyNestedActiveControl (ContainerControl container)
		{
			Control active = container.ActiveControl;
			while (active is ContainerControl) {
				if (((ContainerControl)active).ActiveControl == null)
					break;
				active = ((ContainerControl)active).ActiveControl;
			}
			return active;
		}

		// Just in a separate method to make debugging a little easier,
		// should eventually be rolled into ActiveControl setter
		private Control GetCommonContainer (Control active_control, Control value)
		{
			Control new_container = null;
			Control prev_container = active_control;

			while (prev_container != null) {
				new_container = value.Parent;
				while (new_container != null) {
					if (new_container == prev_container)
						return new_container;
					new_container = new_container.Parent;
				}

				prev_container = prev_container.Parent;
			}

			return null;
		}

		internal void SendControlFocus (Control c)
		{
			if (c.IsHandleCreated) {
				XplatUI.SetFocus (c.window.Handle);
			}
		}

#if NET_2_0
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[Localizable (true)]
		public SizeF AutoScaleDimensions {
			get {
				return auto_scale_dimensions;
			}

			set {
				if (auto_scale_dimensions != value) {
					auto_scale_dimensions = value;
					
					PerformAutoScale ();
				}
			}
		}

		protected SizeF AutoScaleFactor {
			get {
				if (auto_scale_dimensions.IsEmpty)
					return new SizeF (1f, 1f);

				return new SizeF(CurrentAutoScaleDimensions.Width / auto_scale_dimensions.Width,
					CurrentAutoScaleDimensions.Height / auto_scale_dimensions.Height);
			}
		}


		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AutoScaleMode AutoScaleMode {
			get {
				return auto_scale_mode;
			}
			set {
				if (this is Form)
					(this as Form).AutoScale = false;

				if (auto_scale_mode != value) {
					auto_scale_mode = value;

					if (auto_scale_mode_set)
						auto_scale_dimensions = SizeF.Empty;

					auto_scale_mode_set = true;

					PerformAutoScale ();
				}
			}
		}
#endif // NET_2_0

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

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public SizeF CurrentAutoScaleDimensions {
			get {
				switch(auto_scale_mode) {
					case AutoScaleMode.Dpi:
						return TextRenderer.GetDpi ();

					case AutoScaleMode.Font:
						Size s = TextRenderer.MeasureText ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890", Font);
						int width = (int)Math.Round ((float)s.Width / 62f);
						
						return new SizeF (width, s.Height);
				}

				return auto_scale_dimensions;
			}
		}
#endif

		[Browsable (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form ParentForm {
			get {
				Control parent;

				parent = this.Parent;

				while (parent != null) {
					if (parent is Form) {
						return (Form)parent;
					}
					parent = parent.Parent;
				}

				return null;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
#if NET_2_0
		protected override bool CanEnableIme {
			get { return false; }
		}
#endif
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		#endregion	// Public Instance Methods

		#region Public Instance Methods
#if NET_2_0
		internal void PerformAutoScale (bool called_by_scale)
		{
			if ((AutoScaleMode == AutoScaleMode.Inherit) && !called_by_scale)
				return;

			if ((layout_suspended > 0) && !called_by_scale) {
				auto_scale_pending = true;
				return;
			}
			// Set this first so we don't get called again from
			// PerformDelayedAutoScale after ResumeLayout
			auto_scale_pending = false;

			SizeF factor = AutoScaleFactor;
			if (AutoScaleMode == AutoScaleMode.Inherit) {
				ContainerControl cc = FindContainer (this.Parent);
				if (cc != null)
					factor = cc.AutoScaleFactor;
			}
			if (factor != new SizeF (1F, 1F)) {
				is_auto_scaling = true;
				SuspendLayout ();
				Scale (factor);
				ResumeLayout (false);
				is_auto_scaling = false;
			}

			auto_scale_dimensions = CurrentAutoScaleDimensions;
		}

		public void PerformAutoScale ()
		{
			PerformAutoScale (false);
		}

		internal void PerformDelayedAutoScale ()
		{
			if (auto_scale_pending)
				PerformAutoScale ();
		}

		internal bool IsAutoScaling {
			get { return is_auto_scaling; }
		}
#endif

		[MonoTODO ("Stub, not implemented")]
		static bool ValidateWarned;
		public bool Validate() {
			//throw new NotImplementedException();
			if (!ValidateWarned) {
				Console.WriteLine("ContainerControl.Validate is not yet implemented");
				ValidateWarned = true;
			}
			return true;
		}

#if NET_2_0
		public bool Validate (bool checkAutoValidate)
		{
			if ((checkAutoValidate && (AutoValidate != AutoValidate.Disable)) || !checkAutoValidate)
				return Validate ();
				
			return true;
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual bool ValidateChildren ()
		{
			return ValidateChildren (ValidationConstraints.Selectable);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual bool ValidateChildren (ValidationConstraints validationConstraints)
		{
			bool recurse = !((validationConstraints & ValidationConstraints.ImmediateChildren) == ValidationConstraints.ImmediateChildren);
			
			foreach (Control control in Controls)
				if (!ValidateNestedControls (control, validationConstraints, recurse))
					return false;

			return true;
		}
#endif

		bool IContainerControl.ActivateControl(Control control) {
			return Select(control);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void AdjustFormScrollbars(bool displayScrollbars) {
			base.AdjustFormScrollbars(displayScrollbars);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		// LAMESPEC This used to be documented, but it's not in code
		// and no longer listed in MSDN2
		// [EditorBrowsable (EditorBrowsableState.Advanced)]
		// protected override void OnControlRemoved(ControlEventArgs e) {
		private void OnControlRemoved(object sender, ControlEventArgs e) {
			if (e.Control == this.unvalidated_control) {
				this.unvalidated_control = null;
			}

			if (e.Control == this.active_control) {
				this.unvalidated_control = null;
			}

			// base.OnControlRemoved(e);
		}

		protected override void OnCreateControl() {
			base.OnCreateControl();
			// MS seems to call this here, it gets the whole databinding process started
			OnBindingContextChanged (EventArgs.Empty);
		}

#if NET_2_0
		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			if (ToolStripManager.ProcessCmdKey (ref msg, keyData) == true)
				return true;
				
			return base.ProcessCmdKey (ref msg, keyData);
		}
#endif

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
					if ((keyData & (Keys.Alt | Keys.Control)) == Keys.None) {
						if (ProcessTabKey ((Control.ModifierKeys & Keys.Shift) == 0)) {
							return true;
						}
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
			return SelectNextControl(active_control, forward, true, true, false);
		}

		protected override void Select(bool directed, bool forward)
		{
			if (Parent != null) {
				IContainerControl parent = Parent.GetContainerControl ();
				if (parent != null) {
					parent.ActiveControl = this;
				}
			}

			if (directed && auto_select_child) {
				SelectNextControl (null, forward, true, true, false);
			}
		}

		protected virtual void UpdateDefaultButton() {
			// MS Internal
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
			switch ((Msg) m.Msg) {

				case Msg.WM_SETFOCUS:
					if (active_control != null)
						Select (active_control);
					else
						base.WndProc (ref m);
#if false
					else
						SelectNextControl (null, true, true, true, false);
#endif
				break;

				default:
					base.WndProc(ref m);
					break;
			}
		}
		#endregion	// Protected Instance Methods

		#region Internal Methods
		internal void ChildControlRemoved (Control control)
		{
			if (control == active_control || control.Contains (active_control)) {
				SelectNextControl (this, true, true, true, true);
				if (control == active_control || control.Contains (active_control)) {
					active_control = null;
				}
			}
		}
		internal virtual void CheckAcceptButton()
		{
			// do nothing here, only called if it is a Form
		}

#if NET_2_0
		private bool ValidateNestedControls (Control c, ValidationConstraints constraints, bool recurse)
		{
			bool validate_result = true;

			if (!c.CausesValidation)
				validate_result = true;
			else if (!ValidateThisControl (c, constraints))
				validate_result = true;
			else if (!ValidateControl (c))
				validate_result = false;

			if (recurse)
				foreach (Control control in c.Controls)
					if (!ValidateNestedControls (control, constraints, recurse))
						return false;

			return validate_result;
		}

		private bool ValidateThisControl (Control c, ValidationConstraints constraints)
		{
			if (constraints == ValidationConstraints.None)
				return true;

			if ((constraints & ValidationConstraints.Enabled) == ValidationConstraints.Enabled && !c.Enabled)
				return false;

			if ((constraints & ValidationConstraints.Selectable) == ValidationConstraints.Selectable && !c.GetStyle (ControlStyles.Selectable))
				return false;

			if ((constraints & ValidationConstraints.TabStop) == ValidationConstraints.TabStop && !c.TabStop)
				return false;

			if ((constraints & ValidationConstraints.Visible) == ValidationConstraints.Visible && !c.Visible)
				return false;

			return true;
		}
#endif
		#endregion	// Internal Methods

#if NET_2_0
		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			
			if (AutoScaleMode == AutoScaleMode.Font)
				PerformAutoScale ();
		}

		protected override void OnLayout (LayoutEventArgs e)
		{
			base.OnLayout (e);
		}
		
		AutoValidate auto_validate = AutoValidate.Inherit;

		[Browsable (false)]
		[AmbientValue (AutoValidate.Inherit)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public virtual AutoValidate AutoValidate {
			get {
				return auto_validate;
			}

			[MonoTODO("Currently does nothing with the setting")]
			set {
				if (auto_validate != value){
					auto_validate = value;
					OnAutoValidateChanged (new EventArgs ());
				}
			}
		}

		internal bool ShouldSerializeAutoValidate ()
		{
			return this.AutoValidate != AutoValidate.Inherit;
		}

		static object OnValidateChanged = new object ();

		protected virtual void OnAutoValidateChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler) (Events [OnValidateChanged]);
			if (eh != null)
				eh (this, e);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler AutoValidateChanged {
			add { Events.AddHandler (OnValidateChanged, value); }
			remove { Events.RemoveHandler (OnValidateChanged, value); }
		}
#endif
	}
}
