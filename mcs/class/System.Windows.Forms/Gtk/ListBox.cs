//		
//			System.Windows.Forms.ListBox
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//
using System;
using System.Drawing;
using Gtk;
using GtkSharp;
using GLib;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows ListBox control.
	///
	/// </summary>

	public class ListBox: ListControl{
	
		ListStore store = null;
		TreeIter iter = new TreeIter ();
		public ItemCollection Items;
		//ListStore store = new ListStore ((int)TypeFundamentals.TypeString);
		
		public class ItemCollection {

			ListBox owner;
			TreeIter iter = new TreeIter ();
			 
			public ItemCollection (ListBox owner){

				this.owner = owner;
				owner.store = new ListStore ((int)TypeFundamentals.TypeString);
				
			}
						
			public void Add(String items){
			
				Value value = new Value(items);
				owner.store.Append (out iter);
 				owner.store.SetValue (iter, 0, value);
				owner.UpdateStore();
			}
		}
		
		public ListBox () : base (){
			this.Items = new ItemCollection(this);
		}
	
		internal override Gtk.Widget CreateWidget () {
		
			ListStore store = new ListStore ((int)TypeFundamentals.TypeString);
			TreeView tv = new TreeView ();
			tv.HeadersVisible = true;
			tv.HeadersClickable = false;
			tv.EnableSearch = false;
			TreeViewColumn NameCol = new TreeViewColumn ();
			CellRenderer NameRenderer = new CellRendererText ();
			NameCol.Title = "Name";
			NameCol.PackStart (NameRenderer, true);
			NameCol.AddAttribute (NameRenderer, "text", 0);
			tv.AppendColumn (NameCol);
			tv.Model = store;
			return tv;
		}
		
		public void UpdateStore () {
			((Gtk.TreeView)Widget).Model = store;		
		}

		protected override void RefreshItem(int index) {
			//FIXME:
		}

		public override int SelectedIndex {
			get{
				throw new NotImplementedException ();
			}
			set{
				//FIXME:
			}
		}
	}
}
