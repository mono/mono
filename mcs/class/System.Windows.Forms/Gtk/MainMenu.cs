//		
//			System.Windows.Forms.MainMenu
//
//			Author: 
//						Joel Basson		(jstrike@mweb.co.za)
//
//

using System.Drawing;

namespace System.Windows.Forms{

	public class MainMenu : Control{

		public MenuItemCollection MenuItems;
		String text;
		internal Gtk.MenuBar mb;

		public class MenuItemCollection{

			MainMenu owner;

			public MenuItemCollection (MainMenu owner) {
				
				this.owner = owner;
			}

			public void Add (MenuItem item) {

				owner.mb.Append (item.file_item);
				
			}

			public void AddRange(MenuItem[] items) {
				
				foreach (MenuItem m in items)
					{owner.mb.Append (m.file_item);}
				
			}

		}

		public MainMenu() : base (){

			this.MenuItems = new MenuItemCollection(this);

			CreateMenuBar();
		}

		internal override Gtk.Widget CreateWidget () {
			return mb;
		}

		
		private void CreateMenuBar (){
			
			mb = new Gtk.MenuBar ();
			// you cannot call this because then locationchanged gets triggered
			// and that means that a parent will be set for this widget
			//this.Location = new Point(0, 0);
			//this.Size = new Size(1024, 27);
		}

	}

}
