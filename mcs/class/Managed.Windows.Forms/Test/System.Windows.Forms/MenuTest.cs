//
// MenuTest.cs: Test cases for Menu, MainMenu
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]	
	public class MenuTest : TestHelper
	{
		[Test]
		public void MenuPropertyTest ()
		{
			Menu mymenu = new MainMenu ();
			Assert.AreEqual ("System.IntPtr", mymenu.Handle.GetType().FullName, "#1");
			Assert.AreEqual (false, mymenu.IsParent, "#2");
			// TODO: MDI is not completed  yet
			//Assert.AreEqual (null, mymenu.MdiListItem, "#3");
			Assert.AreEqual (0, mymenu.MenuItems.Count,"#4");
			mymenu.MenuItems.Add ("newmenu1");
			mymenu.MenuItems.Add ("newmenu2");
			Assert.AreEqual (2, mymenu.MenuItems.Count,"#5");
			MainMenu mymainmenu = new MainMenu ();
			Assert.AreEqual (RightToLeft.Inherit, mymainmenu.RightToLeft,"#6");
			
#if NET_2_0
			Assert.IsNull (mymenu.Tag);
#endif
		}

		[Test]
		public void GetMainMenuTest ()
		{
			MainMenu mymainmenu = new MainMenu ();
			MenuItem mymenuitem = new MenuItem ();
			mymenuitem.Text = "menu 1";
			mymainmenu.MenuItems.Add (mymenuitem);
			Assert.AreEqual (mymainmenu, mymenuitem.GetMainMenu (), "#7");
		}	
		
		[Test]
		public void CloneMenuTest ()
		{
			MainMenu mymainmenu1 = new MainMenu ();
			MenuItem menuitem1 = new MenuItem ();
			MenuItem menuitem2 = new MenuItem ();
			menuitem1.Text = "item1";
			menuitem2.Text = "item2";
			mymainmenu1.MenuItems.Add (menuitem1);
			mymainmenu1.MenuItems.Add (menuitem2);
			MainMenu mymainmenu2 = mymainmenu1.CloneMenu ();
			Assert.AreEqual ("item1", mymainmenu2.MenuItems[0].Text, "#9");
		}

		[Test]
		public void CloneWindowMenuTest ()
		{
				MenuItem menuitem1 = new MenuItem ();
				menuitem1.MdiList = true;
				MenuItem menuitem2 = menuitem1.CloneMenu ();
				Assert.IsTrue (menuitem2.MdiList, "#1");
		}

		[Test]
		public void GetFormTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Name = "New Form";
			MainMenu mymainmenu1 = new MainMenu ();
			MenuItem menuitem1 = new MenuItem ();
			menuitem1.Text = "item1";
			mymainmenu1.MenuItems.Add (menuitem1);
			myform.Menu = mymainmenu1;
			Assert.AreEqual ("New Form", mymainmenu1.GetForm().Name, "#10");
			myform.Dispose ();
		}
		
		[Test]
		public void MenuItemMerge ()
		{
			MenuItem itemA2 = new MenuItem ("Exit");
			itemA2.MergeType = MenuMerge.MergeItems;
			itemA2.MergeOrder = 3;

			MenuItem itemA1 = new MenuItem ("File");
			itemA1.MenuItems.Add (itemA2);
			itemA1.MergeType = MenuMerge.MergeItems;

			MenuItem itemB2 = new MenuItem ("Open");
			itemB2.MergeOrder = 1;
			itemB2.MergeType = MenuMerge.Add;

			MenuItem itemB3 = new MenuItem ("Close");
			itemB3.MergeOrder = 2;
			itemB3.MergeType = MenuMerge.Add;

			MenuItem itemB1 = new MenuItem ("File");
			itemB1.MenuItems.Add (itemB2);
			itemB1.MenuItems.Add (itemB3);
			itemB1.MergeType = MenuMerge.MergeItems;

			MainMenu mainMenu1 = new MainMenu();
			mainMenu1.MenuItems.Add (itemA1);
			
			MainMenu mainMenu2 = new MainMenu();
			mainMenu2.MenuItems.Add (itemB1);
			
			mainMenu1.MergeMenu (mainMenu2);

			Assert.AreEqual ("File",  mainMenu1.MenuItems[0].Text,              "ItemMerge#1");
			Assert.AreEqual ("Open",  mainMenu1.MenuItems[0].MenuItems[0].Text, "ItemMerge#2");
			Assert.AreEqual ("Close", mainMenu1.MenuItems[0].MenuItems[1].Text, "ItemMerge#3");
			Assert.AreEqual ("Exit",  mainMenu1.MenuItems[0].MenuItems[2].Text, "ItemMerge#4");
		}

		[Test] // Xamarin bug 3418
		public void TestMenuItemsDispose ()
		{
			Menu menu = new MainMenu ();
			menu.MenuItems.Add (new MenuItem ());
			menu.Dispose ();
			try {
				MenuItem item = menu.MenuItems[0];
				Assert.Fail ();
			} catch (ArgumentOutOfRangeException) {
			}
		}
	}
}
