//		
//			System.Windows.Forms.MenuItem
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;

namespace System.Windows.Forms{

	public class MenuItem : Control {

		public MenuItemCollection MenuItems = new MenuItemCollection(this);
		internal Gtk.MenuItem file_item;
		String text;
		
		public class MenuItemCollection{

			MenuItem owner;
			Gtk.Menu file_menu;

			public MenuItemCollection (MenuItem owner) {
		
				this.owner = owner;
				file_menu = new Gtk.Menu(); 
			}

			public void Add (MenuItem items) {

				file_menu.Append (items.file_item);
				owner.file_item.Submenu = file_menu;
			}
		
			public void AddRange(MenuItem[] items) {
				
				foreach (MenuItem m in items)
					{file_menu.Append (m.file_item);
					owner.file_item.Submenu = file_menu;}
				
			}

		}

		public MenuItem() : base (){
			
			CreateMenuItem();
		}

		public override String Text{
			get{
				return text;
			}
			set{
				text = value;
				CreateMenuItem();
			}
		}	
		
		public void CreateMenuItem (){
			
			file_item = new Gtk.MenuItem(text);			
			file_item.Show();

		}

	}

}
