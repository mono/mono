//
// System.Windows.Forms.ApplicationContext
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) Ximian, Inc 2002
//

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies the contextual information about an application thread.
	/// </summary>

	[MonoTODO]
	public class ApplicationContext {

		Form mainForm;
		
		// --- (public) Properties ---
		public Form MainForm {
			get { return mainForm; }
			set 
			{ 
				if (mainForm != null)
					mainForm.Closed -= new System.EventHandler( OnMainFormClosed );
				mainForm = value; 
				mainForm.Closed += new System.EventHandler( OnMainFormClosed );
			}
		}
		
		// --- Constructor ---
		public ApplicationContext() 
		{
			mainForm = null;
		}
		
		public ApplicationContext(Form mainForm) : this() 
		{
			this.MainForm = mainForm;
		}
		
		// --- Methods ---
		[MonoTODO]
		public void Dispose() 
		{
			// see documentation on ApplicationContext.Dispose Method (Boolean)
			this.Dispose(true);
		}
		
		[MonoTODO]
		protected virtual void Dispose(bool disposing) 
		{
			
		}
		
		[MonoTODO]
		public void ExitThread() 
		{
			if (ThreadExit != null)
				ThreadExit( this, null );
		}
		
		[MonoTODO]
		protected virtual void ExitThreadCore() 
		{
			ExitThread();
		}
		
		[MonoTODO]
		protected virtual void OnMainFormClosed(object sender,EventArgs e) 
		{
			ExitThreadCore();
		}
		
		// --- Methods: object ---
		[MonoTODO]
		~ApplicationContext() {
			// see documentation on ApplicationContext.Dispose Method (Boolean)
			this.Dispose(false);
		}
		
		// --- Events ---
		public event EventHandler ThreadExit;
	}
}
