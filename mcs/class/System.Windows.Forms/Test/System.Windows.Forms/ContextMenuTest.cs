//
// ContextMenuTest.cs: Test cases for ContextMenu
//
// Author:
//   Everaldo Canuto (ecanuto@novell.com)
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
	public class ContextMenuTest : TestHelper
	{
		[Test]
		public void GetContextMenuTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			ContextMenu mycontextmenu = new ContextMenu ();
			myform.ContextMenu= mycontextmenu;
			MenuItem menuItem1 = new MenuItem ();
			menuItem1.Text = "1";
			mycontextmenu.MenuItems.Add (menuItem1);
			Assert.AreEqual (mycontextmenu, menuItem1.GetContextMenu (),"#1");
			myform.Dispose ();
		}
	}
}
