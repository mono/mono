//
// System.Windows.Forms.CommonDialog.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) Ximian, Inc., 2002
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

using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies the base class used for displaying dialog boxes on the screen.
	/// </summary>

	[MonoTODO]
	public abstract class CommonDialog : Component {

		[MonoTODO]
		public CommonDialog() : base () 
		{
		}
		
		[MonoTODO]
		protected virtual IntPtr HookProc(IntPtr hWnd,int msg,IntPtr wparam,IntPtr lparam) 
		{
			// FIXME: center window in response to WM_INITDIALOG message
			return IntPtr.Zero;
		}
		
		protected virtual void OnHelpRequest(EventArgs e) 
		{
			if ( HelpRequest != null )
				HelpRequest ( this, e );
		}
		
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
			IntPtr handle;			
			Control ctrl=Control.getOwnerWindow(null);
						
			if ((ctrl!=null) && (ctrl.Handle!=(IntPtr)0))
				handle=ctrl.Handle;
			else
				handle = Win32.GetDesktopWindow();												
			
			bool res = RunDialog (handle);
			return res ? DialogResult.OK : DialogResult.Cancel;
		}
		
		public DialogResult ShowDialog(IWin32Window owner) 
		{
			bool res = RunDialog ( owner.Handle );
			return res ? DialogResult.OK : DialogResult.Cancel;
		}
		
		public event EventHandler HelpRequest;
	}
}
