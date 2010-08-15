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
	[TestFixture]
	[Ignore ("Needs Manual Intervention")]
	public class LabelEditEvent : TestHelper
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
			myform.ShowInTaskbar = false;
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

			myform.Dispose ();
		}

		[Test]
		public void BeforeLabelEditTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
		}
	}

	[TestFixture]
	[Ignore ("Needs Manual Intervention")]
	public class ColumnClickEvent : TestHelper
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
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
		}
	}

	[TestFixture]
	[Ignore ("Needs Manual Intervention")]
	public class  MyEvent : TestHelper
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
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
		}

		[Test]
		public void SelectedIndexChangedTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
		}
	}

	[TestFixture]
	[Ignore ("Needs Manual Intervention")]
	public class ItemCheckEvent : TestHelper
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
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
		}
	}

	[TestFixture]
	[Ignore ("Needs Manual Intervention")]
	public class ItemDragEvent : TestHelper
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
			myform.ShowInTaskbar = false;
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
			myform.Dispose ();
		}
	}

	[TestFixture]
	public class ListViewSelectedIndexChangedEvent : TestHelper
	{
		int selectedIndexChanged;

		public void ListView_SelectedIndexChanged (object sender, EventArgs e)
		{
			selectedIndexChanged++;
		}

		[SetUp]
		protected override void SetUp () {
			selectedIndexChanged = 0;
			base.SetUp ();
		}

		[Test] // bug #79849
		public void SelectBeforeCreationOfHandle ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			ListView lvw = new ListView ();
			lvw.SelectedIndexChanged += new EventHandler (ListView_SelectedIndexChanged);
			lvw.View = View.Details;
			ListViewItem itemA = new ListViewItem ("A");
			lvw.Items.Add (itemA);
			Assert.AreEqual (0, selectedIndexChanged, "#A1");
			itemA.Selected = true;
			Assert.AreEqual (0, selectedIndexChanged, "A2");

			ListViewItem itemB = new ListViewItem ("B");
			lvw.Items.Add (itemB);
			Assert.AreEqual (0, selectedIndexChanged, "#B1");
			itemB.Selected = true;
			Assert.AreEqual (0, selectedIndexChanged, "B2");

			form.Controls.Add (lvw);
			Assert.AreEqual (0, selectedIndexChanged, "#C1");
			form.Show ();
			Assert.AreEqual (2, selectedIndexChanged, "#C2");
			form.Dispose ();
		}

		[Test]
		public void RemoveSelectedItem ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			ListView lvw = new ListView ();
			lvw.SelectedIndexChanged += new EventHandler (ListView_SelectedIndexChanged);
			lvw.View = View.Details;
			ListViewItem itemA = new ListViewItem ("A");
			lvw.Items.Add (itemA);
			Assert.AreEqual (0, selectedIndexChanged, "#A1");
			itemA.Selected = true;
			Assert.AreEqual (0, selectedIndexChanged, "A2");

			ListViewItem itemB = new ListViewItem ("B");
			lvw.Items.Add (itemB);
			Assert.AreEqual (0, selectedIndexChanged, "#B1");
			itemB.Selected = true;
			Assert.AreEqual (0, selectedIndexChanged, "B2");
			lvw.Items.Remove (itemB);
			Assert.IsTrue (itemB.Selected, "#B3");

			form.Controls.Add (lvw);
			Assert.AreEqual (0, selectedIndexChanged, "#C1");
			form.Show ();
			Assert.AreEqual (1, selectedIndexChanged, "#C2");
			lvw.Items.Remove (itemA);
			Assert.AreEqual (2, selectedIndexChanged, "#C3");
			Assert.IsTrue (itemA.Selected, "#C4");
			
			form.Close ();
		}

		[Test]
		public void AddAndSelectItem ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			ListView lvw = new ListView ();
			lvw.SelectedIndexChanged += new EventHandler (ListView_SelectedIndexChanged);
			lvw.View = View.Details;
			form.Controls.Add (lvw);
			form.Show ();

			ListViewItem itemA = new ListViewItem ();
			lvw.Items.Add (itemA);
			Assert.AreEqual (0, selectedIndexChanged, "#A1");
			itemA.Selected = true;
			Assert.AreEqual (1, selectedIndexChanged, "#A2");

			ListViewItem itemB = new ListViewItem ();
			lvw.Items.Add (itemB);
			Assert.AreEqual (1, selectedIndexChanged, "#B1");
			itemB.Selected = true;
			Assert.AreEqual (2, selectedIndexChanged, "#B2");
			
			form.Close ();
		}

		[Test]
		public void InsertSelectedItem ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			ListView lvw = new ListView ();
			lvw.SelectedIndexChanged += new EventHandler (ListView_SelectedIndexChanged);
			form.Controls.Add (lvw);
			form.Show ();

			ListViewItem item = new ListViewItem ();
			item.Selected = true;
			Assert.AreEqual (0, selectedIndexChanged, "#A1");
			lvw.Items.Insert (0, item);
			Assert.AreEqual (1, selectedIndexChanged, "#A1");

			form.Close ();
		}
	}
}
