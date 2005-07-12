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

namespace CheckedListBoxEvent
{
	[TestFixture]
	public class CheckedListBoxItemCheckEvent
	{	
		static bool eventhandled = false;
		public void ItemCheck_EventHandler(object sender,ItemCheckEventArgs e)
		{
			eventhandled = true;
		}
	
		[Test]
		public void ItemCheckTest()
		{
			Form myform = new Form ();
			CheckedListBox mychklstbox = new CheckedListBox ();
			//Test ItemCheck Event
			mychklstbox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(ItemCheck_EventHandler);		
			mychklstbox.Items.Add("test1",CheckState.Checked);
			myform.Controls.Add(mychklstbox);
			myform.Show ();
			Assert.AreEqual(true, eventhandled, "#A1");
		}
	}
}
