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

		public MenuItemCollection MenuItems = new MenuItemCollection(this);
		String text;
		Gtk.MenuBar mb;

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

			CreateMenuBar();
		}

		internal override Gtk.Widget CreateWidget () {
			
			return mb;
		}

		
		private void CreateMenuBar (){
			
			mb = new Gtk.MenuBar ();
			this.Location = new Point(0, 0);
			this.Size = new Size(1024, 27);
		}

	}

}
