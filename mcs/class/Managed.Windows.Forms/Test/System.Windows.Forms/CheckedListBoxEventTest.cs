//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class CheckedListBoxItemCheckEvent : TestHelper
	{	
		static bool eventhandled = false;
		public void ItemCheck_EventHandler (object sender,ItemCheckEventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void ItemCheckTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			CheckedListBox mychklstbox = new CheckedListBox ();
			mychklstbox.Items.Add ("test1"); 
			mychklstbox.Items.Add ("test2"); 
			//Test ItemCheck Event
			mychklstbox.ItemCheck += new ItemCheckEventHandler (ItemCheck_EventHandler);		
			mychklstbox.Items.Add ("test1",CheckState.Checked);
			myform.Controls.Add (mychklstbox);
			myform.Show ();
			Assert.AreEqual (true, eventhandled, "#A1");
			eventhandled = false;
			mychklstbox.SetItemChecked (1,true);
			Assert.AreEqual (true, eventhandled, "#A2");
			myform.Dispose ();
		}
	}
}
