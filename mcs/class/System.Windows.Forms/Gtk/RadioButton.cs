//		
//			System.Windows.Forms.Frame
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System;
using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;

namespace System.Windows.Forms { 

	
	/// <summary>
	/// Represents a Windows RadioButton control.
	///
	/// </summary>
	///	This doesn't work properly. The "else" statement never seems
	///	to get invoked, causing the grouping to not work.
	///
	
		class RadioButton : ButtonBase {

        	private static bool initialized;
  				private static Gtk.RadioButton first_radio_button;
			
         public RadioButton()
         {
             initialized = false;
         }
 
         private static Gtk.Widget MakeWidget() { 			
				
					if ( initialized == false ) {
                 initialized = true;
  							
                 first_radio_button = new Gtk.RadioButton(null, "");
             		return first_radio_button;
            } 
					else {
         		   	return Gtk.RadioButton.NewWithLabelFromWidget(first_radio_button, "");
            }
        	}
				
				internal override Gtk.Widget CreateWidget() {
						
						return MakeWidget();
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