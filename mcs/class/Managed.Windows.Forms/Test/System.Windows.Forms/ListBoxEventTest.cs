//
// Copyright  (c) 2005 Novell, Inc.
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
	[Ignore ("This test has to be completly reviewed")]
	public class ListBoxDrawItemEvent : TestHelper
	{	
		static bool eventhandled = false;
		public void DrawItem_EventHandler (object sender,DrawItemEventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void DrawItemTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			ListBox lb1 = new ListBox ();
			lb1.Items.Add ("A");
			// Test DrawItem Event
			lb1.DrawItem += new DrawItemEventHandler (DrawItem_EventHandler);		
			lb1.DrawMode = DrawMode.OwnerDrawFixed;
			myform.Controls.Add (lb1);
			myform.Show ();
			Assert.AreEqual (true, eventhandled, "#A1");
			myform.Dispose();
		}

		[TestFixture]
		[Ignore ("This test has to be completly reviewed")]
		public class ListBoxMeasureItemEvent : TestHelper
		{
			static bool eventhandled = false;
			public void MeasureItem_EventHandler (object sender,MeasureItemEventArgs e)
			{
				eventhandled = true;
			}		

			[Test]
			public void MeasureItemTest ()
			{
				Form myform = new Form ();
				myform.ShowInTaskbar = false;
				myform.Visible = true;
				ListBox lb1 = new ListBox ();
				lb1.Items.Add ("B");
				lb1.Visible = true;
				myform.Controls.Add (lb1);
				// Test MeasureItem Event
				lb1.MeasureItem += new MeasureItemEventHandler (MeasureItem_EventHandler);		
				lb1.DrawMode = DrawMode.OwnerDrawVariable;
				Assert.AreEqual (true, eventhandled, "#A2");
				myform.Dispose();
			}
		}
	}
}
