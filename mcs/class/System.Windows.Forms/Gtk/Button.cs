//
// System.Windows.Forms.Button.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//
// (C) Ximian, Inc., 2002
//

namespace System.Windows.Forms
{
	/// <summary>
	/// Represents a Windows button control.
	/// </summary>

	public class Button : ButtonBase, IButtonControl
	{
		// private fields
		DialogResult dialogResult;
//		
//		// --- Constructor ---
//		protected Button() : base() {
//			dialogResult = DialogResult.None;
//		}
//		
//		
//		
//		
//		// --- Properties ---
//		[MonoTODO]
//		protected override CreateParams CreateParams {
//			get { throw new NotImplementedException (); }
//		}
//		
		// --- IButtonControl property ---
		public virtual DialogResult DialogResult {
			get { return dialogResult; }
			set { dialogResult=value; }
		}
//		
//		
//		
//		
		// --- IButtonControl method ---
		[MonoTODO]
		public virtual void NotifyDefault(bool value) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void PerformClick() {
			throw new NotImplementedException ();
		}
//		
//		// --- Button methods for events ---
//		[MonoTODO]
//		protected override void OnClick(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnMouseUp(MouseEventArgs mevent) {
//			throw new NotImplementedException ();
//		}
//		
//		// --- Button methods ---
//		[MonoTODO]
//		protected override bool ProcessMnemonic(char charCode) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		public override string ToString() {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void WndProc(ref Message m) {
//			throw new NotImplementedException ();
//		}
//		
//		
//		
//		
//		/// --- Button events ---
//		/// commented out, cause it only supports the .NET Framework infrastructure
//		/*
//		[MonoTODO]
//		public new event EventHandler DoubleClick {
//			add {
//				throw new NotImplementedException ();
//			}
//			remove {
//				throw new NotImplementedException ();
//			}
//		}
//		*/

		internal override Gtk.Widget CreateWidget () {
			
			Gtk.Button button = new Gtk.Button ();
			button.Add (label.Widget);
			return button;
		}
	}
//	
}
