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
	using System.Drawing;
	/// <summary>
	/// Represents a Windows button control.
	/// </summary>

	public class Button : ButtonBase, IButtonControl{
		
		// private fields
		DialogResult dialogResult;
		
		
		// --- Constructor ---
		public Button() : base(){
			dialogResult = DialogResult.None;
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			//this.
		}
		
				
		// --- IButtonControl property ---
		public virtual DialogResult DialogResult {
			get { return dialogResult; }
			set { 
				if ( !Enum.IsDefined ( typeof(DialogResult), value ) )
					throw new System.ComponentModel.InvalidEnumArgumentException( "DialogResult",
						(int)value,
						typeof(DialogResult));

				dialogResult = value;
			}
		}

		// --- IButtonControl method ---
		public virtual void NotifyDefault(bool value){
		}
		
		public void PerformClick(){
			EventArgs e = new EventArgs();
 			OnClick(e);
		}
		
		// --- Button methods for events ---
		protected override void OnClick(EventArgs e) {
			/*if ( DialogResult != DialogResult.None ) {
				Form parent = Parent as Form;
				if ( parent != null )
					parent.DialogResult = this.DialogResult;
			}
			base.OnClick (e);*/
			base.OnClick (e);
			
		}
		
		/*protected override void OnMouseUp(MouseEventArgs mevent){
			base.OnMouseUp (mevent);
		}*/
		
		// --- Button methods ---
		/*protected override bool ProcessMnemonic (char charCode) {
			return base.ProcessMnemonic (charCode);
		}*/

		//[MonoTODO]
		//public override string ToString (){
		//	return base.ToString();
		//}
		//protected override void WndProc (ref Message m) {
		//	base.WndProc (ref m);
		//}
		internal override Gtk.Widget CreateWidget () {			
			Gtk.Button button = new Gtk.Button ();
			button.Add (label.Widget);
			return button;
		}
		
		internal override void ConnectEvents(){
			base.ConnectEvents();
			Gtk.Button btn = (Gtk.Button) Widget;
			btn.Clicked += new EventHandler (GtkButtonClicked);
		}
		internal void GtkButtonClicked(object sender, EventArgs args){
			OnClick (args);
		}
				
	}
	
}
