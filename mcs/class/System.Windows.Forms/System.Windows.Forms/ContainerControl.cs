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
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class ContainerControl : ScrollableControl, IContainerControl {
		private Control		active_control;
		private Control		unvalidated_control;
		private ArrayList	pending_validation_chain;

		// This is an internal hack that allows some container controls
		// to not auto select their child when they are activated
		internal bool 		auto_select_child = true;
		private SizeF		auto_scale_dimensions;
		private AutoScaleMode	auto_scale_mode;
		private bool		auto_scale_mode_set;
		private bool		auto_scale_pending;
		private bool		is_auto_scaling;

		internal bool validation_failed; //track whether validation was cancelled by a validating control

		#region Public Constructors
		public ContainerControl() {
			active_control = null;
			unvalidated_control = null;
			ControlRemoved += new ControlEventHandler(OnControlRemoved);
			auto_scale_dimensions = SizeF.Empty;
			auto_scale_mode = AutoScaleMode.Inherit;
			can_cache_preferred_size = true;
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

				// Validation can be postponed due to all the controls
				// in the enter chain not causing validation. If we don't have any
				// enter chain, it means that the selected control is a child and then
				// we need to validate the controls anyway
				bool postpone_validation;
				Control topmost_under_root = null; // topmost under root, in the *enter* chain
				if (value == root)
					postpone_validation = false;
				else {
					postpone_validation = true;
					walk = value;
					while (walk != root && walk != null) {
						if (walk.CausesValidation)
							postpone_validation = false;

						topmost_under_root = walk;
						walk = walk.Parent;
					}
				}

				Control failed_validation_control = PerformValidation (form == null ? this : form, postpone_validation, 
						validation_chain, topmost_under_root);
				if (failed_validation_control != null) {
					active_control = value = failed_validation_control;
					fire_enter = true;
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

		// Return the control where validation failed, and null otherwise
		// @topmost_under_root is the control under the root in the enter chain, if any
		//
		// The process of validation happens as described:
		//
		// 	1. Iterate over the nodes in the enter chain (walk down), looking for any node
		// 	causing validation. If we can't find any, don't validate the current validation chain, but postpone it,
		// 	saving it in the top_container.pending_validation_chain field, since we need to keep track of it later.
		// 	If we have a previous pending_validation_chain, add the new nodes, making sure they are not repeated
		// 	(this is computed in ActiveControl and we receive if as the postpone_validation parameter).
		//
		// 	2. If we found at least one node causing validation in the enter chain, try to validate the elements
		// 	in pending_validation_chain, if any. Then continue with the ones receives as parameters.
		//
		// 	3. Return null if all the validation performed successfully, and return the control where the validation
		// 	failed otherwise.
		//
		private Control PerformValidation (ContainerControl top_container, bool postpone_validation, ArrayList validation_chain, 
				Control topmost_under_root)
		{
			validation_failed = false;

			if (postpone_validation) {
				AddValidationChain (top_container, validation_chain);
				return null;
			}

			// if not null, pending chain has always one element or more
			if (top_container.pending_validation_chain != null) {
				// if the topmost node in the enter chain is exactly the topmost
				// int the validation chain, remove it, as .net does
				int last_idx = top_container.pending_validation_chain.Count - 1;
				if (topmost_under_root == top_container.pending_validation_chain [last_idx])
					top_container.pending_validation_chain.RemoveAt (last_idx);

				AddValidationChain (top_container, validation_chain);
				validation_chain = top_container.pending_validation_chain;
				top_container.pending_validation_chain = null;
			}

			for (int i = 0; i < validation_chain.Count; i ++) {
				if (!ValidateControl ((Control)validation_chain[i])) {
					validation_failed = true;
					return (Control)validation_chain[i];
				}
			}

			return null;
		}

		// Add the elements in validation_chain to the pending validation chain stored in top_container
		private void AddValidationChain (ContainerControl top_container, ArrayList validation_chain)
		{
			if (validation_chain.Count == 0)
				return;

			if (top_container.pending_validation_chain == null || top_container.pending_validation_chain.Count == 0) {
				top_container.pending_validation_chain = validation_chain;
				return;
			}

			foreach (Control c in validation_chain)
				if (!top_container.pending_validation_chain.Contains (c))
					top_container.pending_validation_chain.Add (c);
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
			if (c != null && c.IsHandleCreated) {
				XplatUI.SetFocus (c.window.Handle);
			}
		}

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
		protected override bool CanEnableIme {
			get { return false; }
		}
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		#endregion	// Public Instance Methods

		#region Public Instance Methods
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
			active_control = null;
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

		protected override bool ProcessCmdKey (ref Message msg, Keys keyData)
		{
			if (ToolStripManager.ProcessCmdKey (ref msg, keyData) == true)
				return true;
				
			return base.ProcessCmdKey (ref msg, keyData);
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
			ContainerControl top_container = FindForm ();
			if (top_container == null)
				top_container = this;

			// Remove controls -as well as any sub control- that was in the pending validation chain
			ArrayList pending_validation_chain = top_container.pending_validation_chain;
			if (pending_validation_chain != null) {
				RemoveChildrenFromValidation (pending_validation_chain, control);

				if (pending_validation_chain.Count == 0)
					top_container.pending_validation_chain = null;
			}

			if (control == active_control || control.Contains (active_control)) {
				SelectNextControl (this, true, true, true, true);
				if (control == active_control || control.Contains (active_control)) {
					active_control = null;
				}
			}
		}

		// Check that this control (or any child) is included in the pending validation chain
		bool RemoveChildrenFromValidation (ArrayList validation_chain, Control c)
		{
			if (RemoveFromValidationChain (validation_chain, c))
				return true;

			foreach (Control child in c.Controls)
				if (RemoveChildrenFromValidation (validation_chain, child))
					return true;

			return false;
		}

		// Remove the top most control in the pending validation chain, as well as any children there,
		// taking advantage of the fact that the chain is in reverse order of the control's hierarchy
		bool RemoveFromValidationChain (ArrayList validation_chain, Control c)
		{
			int idx = validation_chain.IndexOf (c);
			if (idx > -1) {
				pending_validation_chain.RemoveAt (idx--);
				return true;
			}

			return false;
		}

		internal virtual void CheckAcceptButton()
		{
			// do nothing here, only called if it is a Form
		}

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
		#endregion	// Internal Methods

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

		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			// Translating 0,0 from ClientSize to actual Size tells us how much space
			// is required for the borders.
			Size borderSize = SizeFromClientSize(Size.Empty);
			Size totalPadding = borderSize + Padding.Size;
			return LayoutEngine.GetPreferredSize(this, proposedSize) + totalPadding;
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
	}
}
