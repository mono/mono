//
// System.Windows.Forms.ApplicationContext
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	Dennis hayes (dennish@raytek.com)
//
// (C) Ximian, Inc 2002/3
//

//
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
