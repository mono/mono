//		
//			System.Windows.Forms.GroupBox
//
//			Author: 
//				Joel Bason			(jstrike@mweb.co.za)
//				Alberto Fernandez	(infjaf00@yahoo.es)
//
//

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// 	Represents a Windows GroupBox.
	///
	/// </summary>

	public class GroupBox : Control {
	
		private Gtk.RadioButton firstRadioButton;
		private int radioButtonsCount = 0;
	
		
		public GroupBox () : base (){
		}
		protected override Size DefaultSize {
			get { return new Size (200,100); }
		}
		[MonoTODO]
		public FlatStyle FlatStyle{
			get{ return FlatStyle.Standard; }
			set{
				if (! Enum.IsDefined (typeof (FlatStyle), value))
					throw new InvalidEnumArgumentException();			
			}
		}
		public override string Text {
			get { return ((Gtk.Frame)Widget).Label; }
			set { ((Gtk.Frame)Widget).Label = value;}		
		}
		
		protected override void OnControlAdded (ControlEventArgs e){
			base.OnControlAdded(e);
			if (e.Control is RadioButton){
				if (radioButtonsCount == 0)
					firstRadioButton = e.Control.Widget as Gtk.RadioButton;
				else
					(e.Control.Widget as Gtk.RadioButton).Group = 
						firstRadioButton.Group;
				radioButtonsCount++;
			}		
		}
		[MonoTODO]
		protected override void OnControlRemoved (ControlEventArgs e){	
			base.OnControlRemoved (e);
		}
		
		public override string ToString(){
			return "System.Windows.Forms.GroupBox, Text: " + this.Text;
		}
		
		public new event EventHandler Click;
		public new event EventHandler DoubleClick;
		public new event KeyEventHandler KeyDown;
		public new event KeyPressEventHandler KeyPress;
		public new event KeyEventHandler KeyUp;
		public new event MouseEventHandler MouseDown;
		public new event EventHandler MouseEnter;
		public new event EventHandler MouseLeave;
		public new event MouseEventHandler MouseMove;
		public new event MouseEventHandler MouseUp;
		public new event EventHandler TabStopChanged;	
	
	
		internal override Gtk.Widget CreateWidget () {			
			Gtk.Widget contents = base.CreateWidget();
			Gtk.Frame gbox1 = new Gtk.Frame(null);
			gbox1.Add (contents);			
			return gbox1;	
		}	
	}
}
