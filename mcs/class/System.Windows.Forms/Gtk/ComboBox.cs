//		
//			System.Windows.Forms.ComboBox
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows ComboBox control.
	///
	/// </summary>

	public class ComboBox: Control{
		
		private int menusize;
		private bool UpdateState;
		public ItemCollection Items;
 		GLib.List list = new GLib.List (IntPtr.Zero, typeof (string));
		System.Collections.ArrayList alist = new System.Collections.ArrayList();
		
		public class ItemCollection {

			ComboBox owner;
			 
			public ItemCollection (ComboBox owner){

				this.owner = owner;
			}
						
			public void Add(String value){
				owner.alist.Add(value);
				owner.list.Append (Marshal.StringToHGlobalAnsi (value));
				if ( owner.UpdateState == false ) {owner.Update();}
			}

			public void AddRange(object[] items) {
				owner.alist.AddRange(items);
				foreach (object o in items)
					{string s = (string)o;
					owner.list.Append (Marshal.StringToHGlobalAnsi (s));}
				owner.Update();
			}
		}

		public ComboBox () : base (){
			Items = new ItemCollection(this);
	
			UpdateState = false;
		}

		internal override Gtk.Widget CreateWidget () {
			Gtk.Combo com1 = new Gtk.Combo();
			com1.SetPopdownStrings("");		
			return com1;	
		}
	
		public void Update () {
			((Gtk.Combo)Widget).PopdownStrings = list;		
		}
	
		public void BeginUpdate () {

			UpdateState = true;
		}

		public void EndUpdate () {

			UpdateState = false;
			Update();
		}

		public int FindString (string value){
			
			//return alist.BinarySearch(value);	
			return alist.IndexOf(value);
		}
		
		public int SelectedIndex{
			get{
				return alist.IndexOf(((Gtk.Combo)Widget).Entry.Text);
			}
			set{
				((Gtk.Combo)Widget).Entry.Text = (string)alist[value];
			}
		}
	
		public Object SelectedItem{
			get{
				return alist[this.SelectedIndex];
			}
			set{
				throw new NotImplementedException ();
			}
		}

		public int DropDownWidth {
			get {		
				return menusize;
			}
			set {	
				menusize = value;
			}
		}
	}
}
