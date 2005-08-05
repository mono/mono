//
// ListViewEventTest.cs: Test cases for ListView events.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture, Ignore ("Needs Manual Intervention")]
	public class ListViewEvent
	{	
		static bool eventhandled = false;
		public void LabelEdit_EventHandler (object sender,LabelEditEventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void AfterLabelEditTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();
			mylistview.LabelEdit = true ;
			mylistview.AfterLabelEdit += new LabelEditEventHandler (LabelEdit_EventHandler);
			mylistview.View = View.Details;
			mylistview.SetBounds (10, 10, 200, 200, BoundsSpecified.All);
			mylistview.Columns.Add ("A", -2, HorizontalAlignment.Center);
			mylistview.Columns.Add ("B", -2, HorizontalAlignment.Center);
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			myform.Controls.Add (mylistview);
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A1");
		}

		[Test]
		public void BeforeLabelEditTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();
			mylistview.LabelEdit = true ;
			mylistview.BeforeLabelEdit += new LabelEditEventHandler (LabelEdit_EventHandler);
			eventhandled = false;
			mylistview.View = View.Details;
			mylistview.SetBounds (10, 10, 200, 200, BoundsSpecified.All);
			mylistview.Columns.Add ("A", -2, HorizontalAlignment.Center);
			mylistview.Columns.Add ("B", -2, HorizontalAlignment.Center);
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			myform.Controls.Add (mylistview);
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A2");
		}
	}

	[TestFixture, Ignore ("Needs Manual Intervention")]

	public class ColumnClickEvent
	{	
		static bool eventhandled = false;
		public void ColumnClickEventHandler (object sender, ColumnClickEventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void ColumnClickTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();

			mylistview.LabelEdit = true ;
			mylistview.ColumnClick += new ColumnClickEventHandler (ColumnClickEventHandler);		
			mylistview.View = View.Details;
			mylistview.SetBounds (10, 10, 200, 200, BoundsSpecified.All);
			mylistview.Columns.Add ("A", -2, HorizontalAlignment.Center);
			mylistview.Columns.Add ("B", -2, HorizontalAlignment.Center);
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			myform.Controls.Add (mylistview);
			myform.ShowDialog ();
			mylistview.Sort ();
			Assert.AreEqual (true, eventhandled, "#A3");
		}
	}

	[TestFixture, Ignore ("Needs Manual Intervention")]

	public class  MyEvent
	{	
		static bool eventhandled = false;
		public void New_EventHandler (object sender, EventArgs e)
		{
			eventhandled = true;
		}

		[Test]
		public void ItemActivateTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();
			mylistview.Activation = ItemActivation.OneClick;
			mylistview.LabelEdit = true ;
			mylistview.ItemActivate += new EventHandler (New_EventHandler);		
			mylistview.View = View.Details;
			mylistview.SetBounds (10, 10, 200, 200, BoundsSpecified.All);
			mylistview.Columns.Add ("A", -2, HorizontalAlignment.Center);
			mylistview.Columns.Add ("B", -2, HorizontalAlignment.Center);
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			myform.Controls.Add (mylistview);
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A4");
		}

		[Test]
		public void SelectedIndexChangedTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();
			mylistview.LabelEdit = true ;
			mylistview.SelectedIndexChanged += new EventHandler (New_EventHandler);		
			eventhandled = false;
			mylistview.View = View.Details;
			mylistview.SetBounds (10, 10, 200, 200, BoundsSpecified.All);
			mylistview.Columns.Add ("A", -2, HorizontalAlignment.Center);
			mylistview.Columns.Add ("B", -2, HorizontalAlignment.Center);
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			myform.Controls.Add (mylistview);
			myform.ShowDialog ();
			Assert.AreEqual (true, eventhandled, "#A5");
		}
	}

	[TestFixture, Ignore ("Needs Manual Intervention")]

	public class ItemCheckEvent
	{	
		static bool eventhandled = false;
		public void ItemCheckEventHandler (object sender, ItemCheckEventArgs e)

		{
			eventhandled = true;
		}

		[Test]
		public void ItemCheckTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();
			mylistview.CheckBoxes = true;
			mylistview.LabelEdit = true ;
			mylistview.ItemCheck += new ItemCheckEventHandler (ItemCheckEventHandler);		
			mylistview.View = View.Details;
			mylistview.SetBounds (10, 10, 200, 200, BoundsSpecified.All);
			mylistview.Columns.Add ("A", -2, HorizontalAlignment.Center);
			mylistview.Columns.Add ("B", -2, HorizontalAlignment.Center);
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			myform.Controls.Add (mylistview);
			myform.ShowDialog ();
			mylistview.Visible = true;
			Assert.AreEqual (true, eventhandled, "#A6");
		}
	}


	[TestFixture, Ignore ("Needs Manual Intervention")]

	public class ItemDragEvent
	{	
		static bool eventhandled = false;
		public void ItemDragEventHandler (object sender, ItemDragEventArgs e)

		{
			eventhandled = true;
		}

		[Test]
		public void ItemDragTest ()
		{
			Form myform = new Form ();
			ListView mylistview = new ListView ();
			mylistview.ItemDrag += new ItemDragEventHandler (ItemDragEventHandler);
			mylistview.View = View.Details;
			mylistview.SetBounds (10, 10, 200, 200, BoundsSpecified.All);
			mylistview.Columns.Add ("A", -2, HorizontalAlignment.Center);
			mylistview.Columns.Add ("B", -2, HorizontalAlignment.Center);
			ListViewItem item1 = new ListViewItem ("A", -1);
			mylistview.Items.Add (item1);
			myform.Controls.Add (mylistview);
			myform.ShowDialog ();
			mylistview.Visible = true;
			mylistview.DoDragDrop (mylistview.SelectedItems, DragDropEffects.Link);
			Assert.AreEqual (true, eventhandled, "#A7");
		}
	}
}
