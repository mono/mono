//
// System.Windows.Forms.ContainerControl.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) Ximian, Inc., 2002/3
//

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides focus management functionality for controls that can function as a container for other controls.
	/// </summary>

	public class ContainerControl : ScrollableControl, IContainerControl {

		private Control	activeControl;

		public ContainerControl () : base () 
		{
			controlStyles_ |= ControlStyles.ContainerControl;
		}
		
		
		[MonoTODO]
		public Control ActiveControl {
			get {
				return(activeControl);
			}
			set { 
				if ((value==activeControl) || (value==null)) {
					return;
				}

				if (!Contains(value)) {
					throw new ArgumentException("Not a child control");
				}

				// FIXME; need to let Wine know
				activeControl=value;
			}
		}
		
		//Compact Framework
		[MonoTODO]
		// not ready for BindingContext
		public override BindingContext BindingContext {
			get {
				throw new NotImplementedException ();
			}
			set {
				//fixme:
			}
		}
		
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}
		
		public Form ParentForm {
			get {
				if (Parent != null) {
					return(Parent.FindForm());
				} else {
					return(FindForm());
				}
			}
		}
		
		/// --- Methods ---
		/// internal .NET framework supporting methods, not stubbed out:
		/// - protected virtual void UpdateDefaultButton()

		protected override void AdjustFormScrollbars (
			bool displayScrollbars) 
		{
			//FIXME:
			base.AdjustFormScrollbars (displayScrollbars);
		}
		
		protected override void Dispose (bool disposing) 
		{
			//FIXME
			base.Dispose(disposing);
		}
		
		public bool ActivateControl(Control control) 
		{
			Control 		parent=Parent;
			ContainerControl	container;
			bool			result=true;

			// We might have a container inside a container, etc...
			// if we do, make sure that the whole chain up knows about the activation
			if (parent!=null) {
				container=parent.GetContainerControl() as ContainerControl;
				if ((container!=null) && (container.ActiveControl!=this)) {
					container.ActivateControl(this);
				}
			}

			if (activeControl!=control) {
				// FIXME - let wine know
				activeControl=control;
			}
			return(result);
		}
		
		// [event methods]
		protected override void OnControlRemoved (ControlEventArgs e) 
		{
			if ((e.Control==activeControl) || (e.Control.Contains(activeControl))) {
				ActiveControl=null;
			}
			base.OnControlRemoved(e);
		}
		
		protected override void OnCreateControl ()
		{
			base.OnCreateControl ();
			OnBindingContextChanged(EventArgs.Empty);
		}
		// end of [event methods]
		
		protected override bool ProcessDialogChar (char charCode) 
		{
			// Only process if we own the whole thing
			if (GetTopLevel()) {
				if (ProcessMnemonic(charCode)) {
					return(true);
				}
			}
			return base.ProcessDialogChar(charCode);
		}
		
		[MonoTODO]
		protected override bool ProcessDialogKey (Keys keyData)
		{
			if ( keyData == Keys.Tab ) {
				return ProcessTabKey ( Control.ModifierKeys != Keys.Shift );
			} else if ((keyData==Keys.Left) || (keyData==Keys.Right) || (keyData==Keys.Up) || (keyData==Keys.Down)) {
				// Select the next control

				Control	select=this;
				bool	forward=true;

				if ((keyData==Keys.Left) || (keyData==Keys.Up)) {
					forward=false;
				}

				if (activeControl!=null) {
					select=activeControl.Parent;
				}

				select.SelectNextControl(activeControl, forward, false, false, true);
				return(true);
			}
			return base.ProcessDialogKey(keyData);
		}
		
		[MonoTODO]
		protected override bool ProcessMnemonic (char charCode) 
		{
			//FIXME:
			return base.ProcessMnemonic(charCode);
		}
		
		[MonoTODO]
		protected virtual bool ProcessTabKey ( bool forward ) 
		{
			Control newFocus = getNextFocusedControl ( this, forward );
			if ( newFocus != null )
				return newFocus.Focus ( );
			return false;
		}
		
		// Not an overridden function?
		protected override void Select(bool directed,bool forward) 
		{
			base.Select(directed, forward);
		}

		protected virtual void UpdateDefaultButton() {
			
		}
	
		[MonoTODO]
		public bool Validate () 
		{
			throw new NotImplementedException ();
		}
		
		protected override void WndProc(ref Message m) 
		{
			base.WndProc(ref m);
		}
	}
}
