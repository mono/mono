//		
//			System.Windows.Forms.ComboBox
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows ComboBox control.
	///
	/// </summary>

	public class ComboBox: Control{
	
		public ItemCollection Items = new ItemCollection(this);
 		GLib.List list = new GLib.List (IntPtr.Zero, typeof (string));

		public class ItemCollection {

			ComboBox owner;
			
			public ItemCollection (ComboBox owner){

				this.owner = owner;
			}
						
			public void Add(String value){
				owner.list.Append (Marshal.StringToHGlobalAnsi (value));
				owner.Update();
			}

			public void AddRange(object[] items) {
				foreach (object o in items)
					{string s = (string)o;
					owner.list.Append (Marshal.StringToHGlobalAnsi (s));}
				owner.Update();
			}
		}

		public ComboBox () : base (){
		}

		internal override Gtk.Widget CreateWidget () {
			Gtk.Combo com1 = new Gtk.Combo();
			com1.SetPopdownStrings("");		
			return com1;	
		}
	
		public void Update () {
		((Gtk.Combo)Widget).PopdownStrings = list;		
		}

	}
}
