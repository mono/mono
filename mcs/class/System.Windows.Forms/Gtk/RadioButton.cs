//		
//			System.Windows.Forms.Frame
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System;

namespace System.Windows.Forms { 

	
	/// <summary>
	/// Represents a Windows RadioButton control.
	///
	/// </summary>
	
	
	public	class RadioButton : ButtonBase {

        	private static int initialized;
  				static Gtk.RadioButton first_radio_button;
			
         public RadioButton(){
			
         }
 
         internal override Gtk.Widget CreateWidget() { 			
					initialized = initialized + 1;
					if ( initialized == 1 ) {
                 first_radio_button = new Gtk.RadioButton(null, "");
             		return first_radio_button;
            } 
					else {
							return Gtk.RadioButton.NewWithLabelFromWidget(first_radio_button, "");
            }
        	}
				
				public override string Text {
					get {
		   					return ((Gtk.RadioButton)Widget).Label; 
					}
					set {
								((Gtk.RadioButton)Widget).Label = value;
					}
				}
     }
}
