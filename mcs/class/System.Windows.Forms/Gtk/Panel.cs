//
//	System.Windows.Forms.Panel
//
//	Author:
//		Alberto Fernandez	(infjaf00@yahoo.es)
//

using System.Drawing;


namespace System.Windows.Forms{

	public class Panel : ScrollableControl {
	
		private Gtk.RadioButton firstRadioButton;
		private int radioButtonsCount = 0;
		
		[MonoTODO]
		public Panel(){
		}
		[MonoTODO]
		public BorderStyle BorderStyle {
			get {return BorderStyle.None; }
			set {}
		}		
		protected override Size DefaultSize {
			get { return new Size (200,100); }
		}
		//public new bool TabStop {get; set;}
		//public override string Text {get; set;}

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
		//protected override void OnResize(EventArgs eventargs);
		public override string ToString(){
			return String.Format ("{0}, BorderStyle: {1}",
				"System.Windows.Forms.Panel",
				this.BorderStyle);
		}

		public new event KeyEventHandler KeyDown;
		public new event KeyPressEventHandler KeyPress;
		public new event KeyEventHandler KeyUp;
		public new event EventHandler TextChanged;
	
	}

}
