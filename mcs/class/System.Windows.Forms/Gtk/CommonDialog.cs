//
// System.Windows.Forms.CommonDialog.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies the base class used for displaying dialog boxes on the screen.
	/// </summary>

	[MonoTODO]
	public abstract class CommonDialog : Component {

		internal bool dialogRetValue = false;

		static CommonDialog(){
			Gtk.Application.Init();
		}
		
		[MonoTODO]
		public CommonDialog() : base (){
		}
		
		[MonoTODO]
		protected virtual IntPtr HookProc(IntPtr hWnd,int msg,IntPtr wparam,IntPtr lparam){
			return IntPtr.Zero;
		}
		
		protected virtual void OnHelpRequest(EventArgs e){
			if ( HelpRequest != null )
				HelpRequest ( this, e );
		}
		
		[MonoTODO]
		protected virtual IntPtr OwnerWndProc(IntPtr hWnd,int msg,IntPtr wparam,IntPtr lparam) {
			throw new NotImplementedException ();
		}
		
		public abstract void Reset();		
		protected abstract bool RunDialog(IntPtr hwndOwner);
		
		[MonoTODO]
		public DialogResult ShowDialog(){
			bool res = RunDialog (IntPtr.Zero);			
			return res ? DialogResult.OK : DialogResult.Cancel;
		}
		
		public DialogResult ShowDialog(IWin32Window owner){
			bool res = RunDialog ( owner.Handle );
			return res ? DialogResult.OK : DialogResult.Cancel;
		}		
		public event EventHandler HelpRequest;
		
		internal Gtk.Dialog dialog;
		internal Gtk.Dialog Dialog{
			get {
				if (dialog == null){
					dialog = CreateDialog();
				}
				return dialog;
			}
		}
		// TODO: Change to abstract.
		internal abstract Gtk.Dialog CreateDialog();
			
		internal void OnDelete (object sender, GtkSharp.DeleteEventArgs args){
			Gtk.Window d = (Gtk.Window) sender;
			d.Hide ();
			args.RetVal = true;
		}
		internal void OnResponse (object o, GtkSharp.ResponseArgs args){
			Console.WriteLine ("OnResponse {0}" ,args.ResponseId);
			switch (args.ResponseId){
				case (int)Gtk.ResponseType.Accept:
				case (int)Gtk.ResponseType.Ok:
				case (int)Gtk.ResponseType.Yes:
				case (int)Gtk.ResponseType.Apply:
									dialogRetValue = true;
									break;
				default:dialogRetValue = false;
						break;
			}
		}
	}
}
