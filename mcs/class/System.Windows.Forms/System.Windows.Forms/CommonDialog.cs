//
// System.Windows.Forms.CommonDialog.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies the base class used for displaying dialog boxes on the screen.
	/// </summary>

	[MonoTODO]
	public abstract class CommonDialog : Component {

		// private fields
		
		/// --- constructor ---
		[MonoTODO]
		public CommonDialog() : base () 
		{
		}
		
		/// --- Methods ---
		[MonoTODO]
		protected virtual IntPtr HookProc(IntPtr hWnd,int msg,IntPtr wparam,IntPtr lparam) 
		{
			throw new NotImplementedException ();
		}
		
		// event methods
		[MonoTODO]
		protected virtual void OnHelpRequest(EventArgs e) 
		{
			//FIXME:
		}
		// end of event methods
		
		[MonoTODO]
		protected virtual IntPtr OwnerWndProc(IntPtr hWnd,int msg,IntPtr wparam,IntPtr lparam) 
		{
			throw new NotImplementedException ();
		}
		
		public abstract void Reset();
		
		protected abstract bool RunDialog(IntPtr hwndOwner);
		
		[MonoTODO]
		public DialogResult ShowDialog() 
		{
			bool res = RunDialog ( Control.getOwnerWindow ( null ).Handle );
			return res ? DialogResult.OK : DialogResult.Cancel;
		}
		
		[MonoTODO]
		public DialogResult ShowDialog(IWin32Window owner) 
		{
			bool res = RunDialog ( owner.Handle );
			return res ? DialogResult.OK : DialogResult.Cancel;
		}
		
		/// events
		[MonoTODO]
		public event EventHandler HelpRequest;
	}
}
