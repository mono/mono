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
	public class MenuTest
	{
		[Test]
		public void MenuPropertyTest ()
		{
			Menu mymenu = new MainMenu ();
			Assert.AreEqual ("System.IntPtr", mymenu.Handle.GetType().FullName, "#1");
			Assert.AreEqual (false, mymenu.IsParent, "#2");
			Assert.AreEqual (null, mymenu.MdiListItem, "#3");
			Assert.AreEqual (0, mymenu.MenuItems.Count,"#4");
			mymenu.MenuItems.Add ("newmenu1");
			mymenu.MenuItems.Add ("newmenu2");
			Assert.AreEqual (2, mymenu.MenuItems.Count,"#5");
			MainMenu mymainmenu = new MainMenu ();
			Assert.AreEqual (RightToLeft.Inherit, mymainmenu.RightToLeft,"#6");
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
		public void MergeMenuTest ()
		{
			MainMenu mymainmenu1 = new MainMenu ();
			MainMenu mymainmenu2 = new MainMenu ();
			MenuItem mymenuitem1 = new MenuItem ();
			MenuItem mymenuitem2 = new MenuItem ();
			mymenuitem1.Text = "A";
			mymenuitem2.Text = "B";
			mymainmenu1.MenuItems.Add (mymenuitem1);
			mymainmenu2.MenuItems.Add (mymenuitem2);
			mymainmenu1.MergeMenu (mymainmenu2);
			Assert.AreEqual (2, mymainmenu1.MenuItems.Count, "#8");
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
		public void GetFormTest ()
		{
			Form myform = new Form ();
			myform.Name = "New Form";
			MainMenu mymainmenu1 = new MainMenu ();
			MenuItem menuitem1 = new MenuItem ();
			menuitem1.Text = "item1";
			mymainmenu1.MenuItems.Add (menuitem1);
			myform.Menu = mymainmenu1;
			Assert.AreEqual ("New Form", mymainmenu1.GetForm().Name, "#10");
		}
		
		[Test]
		public void GetContextMenuTest ()
		{
			Form myform = new Form ();
			ContextMenu mycontextmenu = new ContextMenu ();
			myform.ContextMenu= mycontextmenu;
			MenuItem menuItem1 = new MenuItem ();
			menuItem1.Text = "1";
			mycontextmenu.MenuItems.Add (menuItem1);
			Assert.AreEqual (mycontextmenu, menuItem1.GetContextMenu (),"#11");
		}
	}
}
