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
			bool res = RunDialog ( IntPtr.Zero );
			return res ? DialogResult.OK : DialogResult.Cancel;
		}		
		public event EventHandler HelpRequest;
		
		internal Gtk.Dialog dialog;
		internal Gtk.Dialog Dialog{
			get {
				if (dialog == null){
					dialog = CreateDialog();
					dialog.Modal = true;
					dialog.Response += new GtkSharp.ResponseHandler (OnResponse);
					dialog.DeleteEvent += new GtkSharp.DeleteEventHandler (OnDelete);
				}
				return dialog;
			}
		}

		internal abstract Gtk.Dialog CreateDialog();
			
		internal void OnDelete (object sender, GtkSharp.DeleteEventArgs args){
			Gtk.Window d = (Gtk.Window) sender;
			d.Hide ();
			dialogRetValue = false;
			OnCancel();
			args.RetVal = true;
		}
		internal virtual void OnResponse (object o, GtkSharp.ResponseArgs args){
			switch (args.ResponseId){
				case (int)Gtk.ResponseType.Accept:
				case (int)Gtk.ResponseType.Ok:
				case (int)Gtk.ResponseType.Yes:
				case (int)Gtk.ResponseType.Apply:
									dialogRetValue = true;
									OnAccept();
									Dialog.Hide();
									break;
				default:dialogRetValue = false;
						OnCancel();
						Dialog.Hide();
						break;
			}
		}
		internal virtual void OnCancel(){
			
		}
		internal virtual void OnAccept (){
		}
	}
}
