//
// System.Windows.Forms.ContainerControl.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides focus management functionality for controls that can function as a container for other controls.
	/// </summary>

	public class ContainerControl : ScrollableControl, IContainerControl {

		protected ContainerControl () : base () 
		{
			controlStyles_ |= ControlStyles.ContainerControl;
		}
		
		
		public Control ActiveControl {
			get {
				throw new NotImplementedException ();
			}
			set { 
				//FIXME:
			}
		}
		
		//Compact Framework
		//[MonoTODO]
		// not ready for BindingContext
		//public override BindingContext BindingContext {
		//	get { throw new NotImplementedException (); }
		//	set { throw new NotImplementedException (); }
		//}
		
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}
		
		[MonoTODO]
		public Form ParentForm {
			get { throw new NotImplementedException (); }
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
		
		[MonoTODO]
		// not memeber?
		bool IContainerControl.ActivateControl(Control control) 
		{
			throw new NotImplementedException ();
		}
		
		// [event methods]
		protected override void OnControlRemoved (ControlEventArgs e) 
		{
			//FIXME:
			base.OnControlRemoved (e);
		}
		
		protected override void OnCreateControl ()
		{
			//FIXME:
			base.OnCreateControl ();
		}
		// end of [event methods]
		
		[MonoTODO]
		protected override bool ProcessDialogChar (char charCode) 
		{
			//FIXME:
			return base.ProcessDialogChar(charCode);
		}
		
		[MonoTODO]
		protected override bool ProcessDialogKey (Keys keyData) 
		{
			//FIXME:
			return base.ProcessDialogKey(keyData);
		}
		
		[MonoTODO]
		protected override bool ProcessMnemonic (char charCode) 
		{
			//FIXME:
			return base.ProcessMnemonic(charCode);
		}
		
		[MonoTODO]
		protected virtual bool ProcessTabKey (bool forward) 
		{
			throw new NotImplementedException ();
		}
		
		// Not an overridden function?
		//protected override void Select(bool directed,bool forward) 
		//{
		//	throw new NotImplementedException ();
		//}
		
		[MonoTODO]
		public bool Validate () 
		{
			throw new NotImplementedException ();
		}
		
		protected override void WndProc(ref Message m) 
		{
			//FIXME:
			base.WndProc(ref m);
		}
	}
}
