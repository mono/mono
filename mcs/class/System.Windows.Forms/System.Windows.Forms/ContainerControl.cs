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
		}
		
		
		public Control ActiveControl {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
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
			base.AdjustFormScrollbars (displayScrollbars);
		}
		
		[MonoTODO]
		protected override void Dispose (bool disposing) 
		{
			throw new NotImplementedException ();
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
			base.OnControlRemoved (e);
		}
		
		protected override void OnCreateControl ()
		{
			base.OnCreateControl ();
		}
		// end of [event methods]
		
		[MonoTODO]
		protected override bool ProcessDialogChar (char charCode) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ProcessDialogKey (Keys keyData) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override bool ProcessMnemonic (char charCode) 
		{
			throw new NotImplementedException ();
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
			base.WndProc(ref m);
		}
	}
}
