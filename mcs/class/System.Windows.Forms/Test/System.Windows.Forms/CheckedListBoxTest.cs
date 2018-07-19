//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//		Ritvik Mayank (mritvik@novell.com)
//		Gert Driesen (drieseng@users.sourceforge.net)
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class CheckedListBoxTest : TestHelper
	{
		[Test]
		public void CheckedListBoxPropertyTest () 
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			CheckedListBox mychklistbox = new CheckedListBox ();
			ArrayList checked_items = new ArrayList (2);
			ArrayList checked_pos = new ArrayList (2);
			mychklistbox.Items.Add ("test1", true);
			checked_items.Add ("test1"); checked_pos.Add (0);
			mychklistbox.Items.Add ("test2");
			mychklistbox.Items.Add ("test3", true);
			checked_items.Add ("test3"); checked_pos.Add (2);
			mychklistbox.Visible = true;
			myform.Controls.Add (mychklistbox);
			Assert.AreEqual (checked_items.Count, mychklistbox.CheckedIndices.Count, "#1");
			Assert.AreEqual (checked_items.Count, mychklistbox.CheckedItems.Count, "#2");
			foreach (object o in mychklistbox.CheckedItems) 
			{
				Assert.IsTrue (checked_items.Contains (o),"#3");
				checked_items.Remove (o);
			}

			Assert.AreEqual (0, checked_items.Count);
			for (int i = 0; i < mychklistbox.Items.Count; ++i) 
			{
				if (checked_pos.Contains (i))
					Assert.AreEqual (CheckState.Checked, mychklistbox.GetItemCheckState (i),"#4");
				else
					Assert.IsFalse (CheckState.Checked == mychklistbox.GetItemCheckState (i),"#5");
			}
			Assert.AreEqual (false, mychklistbox.CheckOnClick, "#6");
			Assert.AreEqual (3, mychklistbox.Items.Count, "#7");
			Assert.AreEqual (SelectionMode.One, mychklistbox.SelectionMode, "#8");
			Assert.AreEqual (false , mychklistbox.ThreeDCheckBoxes, "#9");
			myform.Dispose ();
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void DisplayMember_HandleCreated ()
		{
			MockItem itemA = new MockItem ("A1", 1);
			MockItem itemB = new MockItem ("B2", 2);
			MockItem itemC = new MockItem ("C3", 3);
			MockItem itemD = new MockItem ("D4", 4);
			MockItem itemE = new MockItem ("E5", 5);
			MockItem itemF = new MockItem ("F6", 6);

			CheckedListBox clb = new CheckedListBox ();
			clb.Items.Add (itemA, true);
			clb.Items.Add (itemC, false);
			clb.Items.Add (itemB, true);

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (clb);
			form.Show ();

			Assert.AreEqual (string.Empty, clb.Text, "#A1");
			clb.SelectedIndex = 1;
			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#A2");
			clb.DisplayMember = "Text";
			Assert.AreEqual ("C3", clb.Text, "#A3");
			clb.SelectedIndex = 2;
			Assert.AreEqual ("B2", clb.Text, "#A4");

			clb.Text = "C3";
			Assert.AreEqual (1, clb.SelectedIndex, "#B1");
			Assert.AreEqual ("C3", clb.Text, "#B2");
			clb.Text = "B";
			Assert.AreEqual (1, clb.SelectedIndex, "#B3");
			Assert.AreEqual ("C3", clb.Text, "#B4");

			ArrayList itemList = new ArrayList ();
			itemList.Add (itemD);
			itemList.Add (itemE);
			itemList.Add (itemF);

			clb.DataSource = itemList;
			clb.DisplayMember = string.Empty;
			clb.SelectedIndex = 1;

			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#C1");
			clb.DisplayMember = "Text";
			Assert.AreEqual ("E5", clb.Text, "#C2");
			clb.SelectedIndex = 2;
			Assert.AreEqual ("F6", clb.Text, "#C3");

			clb.Text = "E5";
			Assert.AreEqual (1, clb.SelectedIndex, "#D1");
			Assert.AreEqual ("E5", clb.Text, "#D2");
			clb.Text = "D";
			Assert.AreEqual (1, clb.SelectedIndex, "#D3");
			Assert.AreEqual ("E5", clb.Text, "#D4");

			form.Dispose ();
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void DisplayMember_HandleNotCreated ()
		{
			MockItem itemA = new MockItem ("A1", 1);
			MockItem itemB = new MockItem ("B2", 2);
			MockItem itemC = new MockItem ("C3", 3);
			MockItem itemD = new MockItem ("D4", 4);
			MockItem itemE = new MockItem ("E5", 5);
			MockItem itemF = new MockItem ("F6", 6);

			CheckedListBox clb = new CheckedListBox ();
			clb.Items.Add (itemA, true);
			clb.Items.Add (itemC, false);
			clb.Items.Add (itemB, true);

			Assert.AreEqual (string.Empty, clb.Text, "#A1");
			clb.SelectedIndex = 1;
			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#A2");
			clb.DisplayMember = "Text";
			Assert.AreEqual ("C3", clb.Text, "#A3");
			clb.SelectedIndex = 2;
			Assert.AreEqual ("B2", clb.Text, "#A4");

			clb.Text = "C3";
			Assert.AreEqual (1, clb.SelectedIndex, "#B1");
			Assert.AreEqual ("C3", clb.Text, "#B2");
			clb.Text = "C";
			Assert.AreEqual (1, clb.SelectedIndex, "#B3");
			Assert.AreEqual ("C3", clb.Text, "#B4");

			ArrayList itemList = new ArrayList ();
			itemList.Add (itemD);
			itemList.Add (itemE);
			itemList.Add (itemF);

			clb.DataSource = itemList;
			clb.DisplayMember = string.Empty;
			clb.SelectedIndex = 1;

			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#C1");
			clb.DisplayMember = "Text";
			Assert.AreEqual ("C3", clb.Text, "#C2");
			clb.SelectedIndex = 2;
			Assert.AreEqual ("B2", clb.Text, "#C3");
		}

		[Test]
		public void GetItemCheckedTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Visible = true;
			CheckedListBox mychklistbox = new CheckedListBox ();
			mychklistbox.Items.Add ("test1",true);
			mychklistbox.Items.Add ("test2",CheckState.Indeterminate);
			mychklistbox.Items.Add ("test3");
			mychklistbox.Visible = true;
			f.Controls.Add (mychklistbox);
			Assert.AreEqual (true, mychklistbox.GetItemChecked (0), "#10");
			Assert.AreEqual (true, mychklistbox.GetItemChecked (1), "#11");
			Assert.AreEqual (false, mychklistbox.GetItemChecked (2), "#12");
			f.Dispose ();
		}	

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException) )]
		public void GetItemCheckedExceptionTest ()
		{
			CheckedListBox mychklistbox = new CheckedListBox ();
			mychklistbox.Items.Add ("test1",true);
			Assert.AreEqual (true, mychklistbox.GetItemChecked (1), "#13");
		}

		[Test]
		public void GetItemCheckStateTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Visible = true;
			CheckedListBox mychklistbox = new CheckedListBox ();
			mychklistbox.Items.Add ("test1",true);
			mychklistbox.Items.Add ("test2",CheckState.Indeterminate);
			mychklistbox.Items.Add ("test3");
			mychklistbox.Visible = true;
			f.Controls.Add (mychklistbox);
			Assert.AreEqual (CheckState.Checked, mychklistbox.GetItemCheckState (0), "#14");
			Assert.AreEqual (CheckState.Indeterminate, mychklistbox.GetItemCheckState (1), "#15");
			Assert.AreEqual (CheckState.Unchecked, mychklistbox.GetItemCheckState (2), "#16");
			f.Dispose ();
		}

		[Test]
		public void SelectionMode_Invalid ()
		{
			CheckedListBox clb = new CheckedListBox ();

			try {
				clb.SelectionMode = SelectionMode.MultiSimple;
				Assert.Fail ("#A1");
			} catch (ArgumentException ex) {
				// Multi selection not supported on CheckedListBox
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsNull (ex.ParamName, "#A4");
				Assert.IsNull (ex.InnerException, "#A5");
			}

			try {
				clb.SelectionMode = SelectionMode.MultiExtended;
				Assert.Fail ("#B1");
			} catch (ArgumentException ex) {
				// Multi selection not supported on CheckedListBox
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsNull (ex.ParamName, "#B4");
				Assert.IsNull (ex.InnerException, "#B5");
			}

			try {
				clb.SelectionMode = (SelectionMode) 666;
				Assert.Fail ("#C1");
			} catch (InvalidEnumArgumentException ex) {
				Assert.AreEqual (typeof (InvalidEnumArgumentException), ex.GetType (), "#C2");
				Assert.IsNotNull (ex.Message, "#C3");
				Assert.IsNotNull (ex.ParamName, "#C4");
				Assert.AreEqual ("value", ex.ParamName, "#C5");
				Assert.IsNull (ex.InnerException, "#C6");
			}
		}

		[Test]
		public void SetItemCheckedTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			CheckedListBox mychklistbox = new CheckedListBox ();
			mychklistbox.Items.Add ("test1");
			mychklistbox.Items.Add ("test2");
			mychklistbox.Visible = true;
			myform.Controls.Add (mychklistbox);
			mychklistbox.SetItemChecked (0,true);
			mychklistbox.SetItemChecked (1,false);
			Assert.AreEqual (CheckState.Checked, mychklistbox.GetItemCheckState (0), "#17");
			Assert.AreEqual (CheckState.Unchecked, mychklistbox.GetItemCheckState (1), "#18");
			myform.Dispose ();
		}

		[Test]
		public void SetItemCheckStateTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			CheckedListBox mychklistbox = new CheckedListBox ();
			mychklistbox.Items.Add ("test1");
			mychklistbox.Items.Add ("test2");
			mychklistbox.Items.Add ("test3");
			mychklistbox.Visible = true;
			myform.Controls.Add (mychklistbox);
			mychklistbox.SetItemCheckState (0,CheckState.Checked);
			mychklistbox.SetItemCheckState (1,CheckState.Indeterminate);
			mychklistbox.SetItemCheckState (2,CheckState.Unchecked);
			Assert.AreEqual (CheckState.Checked, mychklistbox.GetItemCheckState (0), "#19");
			Assert.AreEqual (CheckState.Indeterminate, mychklistbox.GetItemCheckState (1), "#20");
			Assert.AreEqual (CheckState.Unchecked, mychklistbox.GetItemCheckState (2), "#21");
			myform.Dispose ();
		}

		// Fails on 1.1 (both MS and Mono) because SelectedIndex is set
		[Test]
		public void Text_SelectionMode_None ()
		{
			MockItem itemA = new MockItem ("A1", 1);
			MockItem itemB = new MockItem ("B2", 2);
			MockItem itemC = new MockItem ("C3", 3);
			MockItem itemD = new MockItem ("C3", 4);
			MockItem itemE = new MockItem ("", 5);
			MockItem itemF = new MockItem (null, 6);

			ArrayList itemList = new ArrayList ();
			itemList.Add (itemA);
			itemList.Add (itemC);
			itemList.Add (itemB);
			itemList.Add (itemD);
			itemList.Add (itemE);
			itemList.Add (itemF);

			CheckedListBox clb = new CheckedListBox ();
			clb.DisplayMember = "Text";
			clb.DataSource = itemList;
			clb.SelectionMode = SelectionMode.None;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (clb);
			form.Show ();

			Assert.AreEqual (string.Empty, clb.Text, "#A1");
			Assert.AreEqual (-1, clb.SelectedIndex, "#A2");

			clb.Text = "B2";
			Assert.AreEqual ("B2", clb.Text, "#B1");
			Assert.AreEqual (-1, clb.SelectedIndex, "#B2");

			clb.Text = "D";
			Assert.AreEqual ("D", clb.Text, "#C1");
			Assert.AreEqual (-1, clb.SelectedIndex, "#C2");

			clb.Text = "Doesnotexist";
			Assert.AreEqual ("Doesnotexist", clb.Text, "#D1");
			Assert.AreEqual (-1, clb.SelectedIndex, "#D2");

			clb.Text = string.Empty;
			Assert.AreEqual (string.Empty, clb.Text, "#E1");
			Assert.AreEqual (-1, clb.SelectedIndex, "#E2");

			clb.Text = null;
			Assert.AreEqual (string.Empty, clb.Text, "#F1");
			Assert.AreEqual (-1, clb.SelectedIndex, "#F2");

			form.Dispose ();
		}

		[Test]
		public void AllowSelection ()
		{
			MockCheckedListBox clb = new MockCheckedListBox ();
			clb.SelectionMode = SelectionMode.None;
			Assert.IsFalse (clb.allow_selection, "#1");
			clb.SelectionMode = SelectionMode.One;
			Assert.IsTrue (clb.allow_selection, "#2");
		}

		[Test]
		public void Text_SelectionMode_One ()
		{
			MockItem itemA = new MockItem ("A1", 1);
			MockItem itemB = new MockItem ("B2", 2);
			MockItem itemC = new MockItem ("C3", 3);
			MockItem itemD = new MockItem ("C3", 4);
			MockItem itemE = new MockItem ("", 5);
			MockItem itemF = new MockItem (null, 6);

			ArrayList itemList = new ArrayList ();
			itemList.Add (itemA);
			itemList.Add (itemC);
			itemList.Add (itemB);
			itemList.Add (itemD);
			itemList.Add (itemE);
			itemList.Add (itemF);

			CheckedListBox clb = new CheckedListBox ();
			clb.DisplayMember = "Text";
			clb.DataSource = itemList;
			clb.SelectionMode = SelectionMode.One;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (clb);
			form.Show ();

			Assert.AreEqual ("A1", clb.Text, "#A1");
			Assert.AreEqual (0, clb.SelectedIndex, "#A2");

			clb.Text = "B2";
			Assert.AreEqual ("B2", clb.Text, "#B1");
			Assert.AreEqual (2, clb.SelectedIndex, "#B2");
			Assert.AreEqual (1, clb.SelectedItems.Count, "#B3");
			Assert.AreSame (itemB, clb.SelectedItems [0], "#B4");

			clb.Text = "D";
			Assert.AreEqual ("B2", clb.Text, "#C1");
			Assert.AreEqual (2, clb.SelectedIndex, "#C2");
			Assert.AreEqual (1, clb.SelectedItems.Count, "#C3");
			Assert.AreSame (itemB, clb.SelectedItems [0], "#C4");

			clb.Text = "Doesnotexist";
			Assert.AreEqual ("B2", clb.Text, "#D1");
			Assert.AreEqual (2, clb.SelectedIndex, "#D2");
			Assert.AreEqual (1, clb.SelectedItems.Count, "#D3");
			Assert.AreSame (itemB, clb.SelectedItems [0], "#D4");

			clb.Text = "C3";
			Assert.AreEqual ("C3", clb.Text, "#E1");
			Assert.AreEqual (1, clb.SelectedIndex, "#E2");
			Assert.AreEqual (1, clb.SelectedItems.Count, "#E3");
			Assert.AreSame (itemC, clb.SelectedItems [0], "#E4");

			clb.Text = string.Empty;
			Assert.AreEqual (string.Empty, clb.Text, "#F1");
			Assert.AreEqual (4, clb.SelectedIndex, "#F2");
			Assert.AreEqual (1, clb.SelectedItems.Count, "#F3");
			Assert.AreSame (itemE, clb.SelectedItems [0], "#F4");

			clb.Text = null;
			Assert.AreEqual (string.Empty, clb.Text, "#G1");
			Assert.AreEqual (4, clb.SelectedIndex, "#G2");
			Assert.AreEqual (1, clb.SelectedItems.Count, "#G3");
			Assert.AreSame (itemE, clb.SelectedItems [0], "#G4");

			clb.SelectedIndex = -1;
			Assert.AreEqual ("A1", clb.Text, "#H1");
			Assert.AreEqual (0, clb.SelectedIndex, "#H2");
			Assert.AreEqual (1, clb.SelectedItems.Count, "#H3");
			Assert.AreSame (itemA, clb.SelectedItems [0], "#H4");

			form.Dispose ();
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void ValueMember_HandleCreated ()
		{
			MockItem itemA = new MockItem ("A", 1);
			MockItem itemB = new MockItem ("B", 2);
			MockItem itemC = new MockItem ("C", 3);
			MockItem itemD = new MockItem ("D", 4);
			MockItem itemE = new MockItem ("E", 5);

			CheckedListBox clb = new CheckedListBox ();
			clb.Items.Add (itemA, true);
			clb.Items.Add (itemC, false);
			clb.Items.Add (itemB, true);
			clb.SelectedIndex = 1;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (clb);
			form.Show ();

			Assert.AreEqual (string.Empty, clb.ValueMember, "#A1");
			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#A2");
			Assert.IsNull (clb.SelectedValue, "#A3");

			clb.ValueMember = "Value";
			Assert.AreEqual ("Value", clb.ValueMember, "#B1");
			Assert.AreEqual ("3", clb.Text, "#B2");
			Assert.IsNull (clb.SelectedValue, "#B3");

			clb.DisplayMember = "Text";
			Assert.AreEqual ("Value", clb.ValueMember, "#C1");
			Assert.AreEqual ("C", clb.Text, "#C2");
			Assert.IsNull (clb.SelectedValue, "#C3");

			ArrayList itemList = new ArrayList ();
			itemList.Add (itemD);
			itemList.Add (itemE);

			clb.DataSource = itemList;
			clb.ValueMember = string.Empty;
			clb.DisplayMember = string.Empty;
			clb.SelectedIndex = 1;

			Assert.AreEqual (string.Empty, clb.ValueMember, "#D1");
			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#D2");
			Assert.IsNotNull (clb.SelectedValue, "#D3");
			Assert.AreSame (itemE, clb.SelectedValue, "#D4");

			clb.ValueMember = "Value";
			Assert.AreEqual ("Value", clb.ValueMember, "#E1");
			Assert.AreEqual ("5", clb.Text, "#E2");
			Assert.IsNotNull (clb.SelectedValue, "#E3");
			Assert.AreEqual (5, clb.SelectedValue, "#E4");

			clb.DisplayMember = "Text";
			Assert.AreEqual ("Value", clb.ValueMember, "#F1");
			Assert.AreEqual ("E", clb.Text, "#F2");
			Assert.IsNotNull (clb.SelectedValue, "#F3");
			Assert.AreEqual (5, clb.SelectedValue, "#F4");

			form.Dispose ();
		}

		[Test]
		[NUnit.Framework.Category ("NotWorking")]
		public void ValueMember_HandleNotCreated ()
		{
			MockItem itemA = new MockItem ("A", 1);
			MockItem itemB = new MockItem ("B", 2);
			MockItem itemC = new MockItem ("C", 3);
			MockItem itemD = new MockItem ("D", 4);
			MockItem itemE = new MockItem ("E", 5);

			CheckedListBox clb = new CheckedListBox ();
			clb.Items.Add (itemA, true);
			clb.Items.Add (itemC, false);
			clb.Items.Add (itemB, true);
			clb.SelectedIndex = 1;

			Assert.AreEqual (string.Empty, clb.ValueMember, "#A1");
			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#A2");
			Assert.IsNull (clb.SelectedValue, "#A3");

			clb.ValueMember = "Value";
			Assert.AreEqual ("Value", clb.ValueMember, "#B1");
			Assert.AreEqual ("3", clb.Text, "#B2");
			Assert.IsNull (clb.SelectedValue, "#B3");

			clb.DisplayMember = "Text";
			Assert.AreEqual ("Value", clb.ValueMember, "#C1");
			Assert.AreEqual ("C", clb.Text, "#C2");
			Assert.IsNull (clb.SelectedValue, "#C3");

			ArrayList itemList = new ArrayList ();
			itemList.Add (itemD);
			itemList.Add (itemE);

			clb.DataSource = itemList;
			clb.ValueMember = string.Empty;
			clb.DisplayMember = string.Empty;
			clb.SelectedIndex = 1;

			Assert.AreEqual (string.Empty, clb.ValueMember, "#D1");
			Assert.AreEqual (itemC.GetType ().FullName, clb.Text, "#D2");
			Assert.IsNull (clb.SelectedValue, "#D3");

			clb.ValueMember = "Value";
			Assert.AreEqual ("Value", clb.ValueMember, "#E1");
			Assert.AreEqual ("3", clb.Text, "#E2");
			Assert.IsNull (clb.SelectedValue, "#E3");

			clb.DisplayMember = "Text";
			Assert.AreEqual ("Value", clb.ValueMember, "#F1");
			Assert.AreEqual ("C", clb.Text, "#F2");
			Assert.IsNull (clb.SelectedValue, "#F3");	
		}

		public class MockCheckedListBox : CheckedListBox
		{
			public bool allow_selection {
				get { return base.AllowSelection; }
			}
		}

		//--------------------------------------------------------------
		// Add method checking add index and event when sorted and not.

		public static object[] Items = new string[] { "c", "a", "d", "b", };
		//
		public static int[] ExpectedAddPositionsSorted = { 0, 0, 2, 1 };
		public static int[] ExpectedAddPositionsUnsorted = { 0, 1, 2, 3 };
		//
		const string ExpectedEventsUnchecked = "";
		const string ExpectedEventsCheckedSorted
			= "ItemCheck, index 0, is: Unchecked, will be: Checked, item is: 'c'" + ", CheckedIndices: { }" + ";\n"
			+ "ItemCheck, index 0, is: Unchecked, will be: Checked, item is: 'a'" + ", CheckedIndices: { '1', }" + ";\n"
			+ "ItemCheck, index 2, is: Unchecked, will be: Checked, item is: 'd'" + ", CheckedIndices: { '0', '1', }" + ";\n"
			+ "ItemCheck, index 1, is: Unchecked, will be: Checked, item is: 'b'" + ", CheckedIndices: { '0', '2', '3', }" + ";\n";
		const string ExpectedEventsCheckedUnsorted
			= "ItemCheck, index 0, is: Unchecked, will be: Checked, item is: 'c'" + ", CheckedIndices: { }" + ";\n"
			+ "ItemCheck, index 1, is: Unchecked, will be: Checked, item is: 'a'" + ", CheckedIndices: { '0', }" + ";\n"
			+ "ItemCheck, index 2, is: Unchecked, will be: Checked, item is: 'd'" + ", CheckedIndices: { '0', '1', }" + ";\n"
			+ "ItemCheck, index 3, is: Unchecked, will be: Checked, item is: 'b'" + ", CheckedIndices: { '0', '1', '2', }" + ";\n";

		class ItemCheckLoggingReceiver
		{
			public string _allItemCheckEvents;

			public void HandleItemCheck (object sender, ItemCheckEventArgs e)
			{
				CheckedListBox clb = (CheckedListBox)sender;
				string text
					= String.Format ("ItemCheck, index {0}, is: {1}, will be: {2}, item is: '{3}'" + ", CheckedIndices: {{ {4}}}" + ";\n",
						e.Index, e.CurrentValue, e.NewValue, clb.Items[e.Index], Join (clb.CheckedIndices));
				_allItemCheckEvents += text;
			}
		}

		[Test]
		public void AddCheckedBoolToSorted ()
		{
			AddBoolItems (true, Items, true, ExpectedAddPositionsSorted, ExpectedEventsCheckedSorted);
		}

		[Test]
		public void AddUncheckedBoolToSorted ()
		{
			AddBoolItems (true, Items, false, ExpectedAddPositionsSorted, ExpectedEventsUnchecked);
		}

		[Test]
		public void AddCheckedBoolToUnsorted ()
		{
			AddBoolItems (false, Items, true, ExpectedAddPositionsUnsorted, ExpectedEventsCheckedUnsorted);
		}

		[Test]
		public void AddUncheckedBoolToUnsorted ()
		{
			AddBoolItems (false, Items, false, ExpectedAddPositionsUnsorted, ExpectedEventsUnchecked);
		}

		void AddBoolItems (bool sorted, object[] items, bool isChecked, int[] expectedAddPositions, string expectedEvents)
		{
			CheckedListBox clb = new CheckedListBox ();
			clb.Sorted = sorted;
			ItemCheckLoggingReceiver target = new ItemCheckLoggingReceiver ();
			clb.ItemCheck += new ItemCheckEventHandler (target.HandleItemCheck);
			target._allItemCheckEvents = String.Empty;
			ArrayList addedAtList = new ArrayList ();
			foreach (object cur in items) {
				int idx = clb.Items.Add (cur, isChecked);
				addedAtList.Add (idx);
			}
			if (isChecked)
				AssertAllItemsChecked (clb);
			else
				AssertAllItemsUnchecked (clb);
			Assert.AreEqual ((Array)expectedAddPositions, (Array)addedAtList.ToArray (typeof (int)), "addedAtList");
			Assert.AreEqual (expectedEvents, target._allItemCheckEvents, "events");
		}

		[Test]
		public void AddCheckedCheckStateToSorted ()
		{
			AddCheckStateItems (true, Items, CheckState.Checked, ExpectedAddPositionsSorted, ExpectedEventsCheckedSorted);
		}

		[Test]
		public void AddUncheckedCheckStateToSorted ()
		{
			AddCheckStateItems (true, Items, CheckState.Unchecked, ExpectedAddPositionsSorted, ExpectedEventsUnchecked);
		}

		[Test]
		public void AddCheckedCheckStateToUnsorted ()
		{
			AddCheckStateItems (false, Items, CheckState.Checked, ExpectedAddPositionsUnsorted, ExpectedEventsCheckedUnsorted);
		}

		[Test]
		public void AddUncheckedCheckStateToUnsorted ()
		{
			AddCheckStateItems (false, Items, CheckState.Unchecked, ExpectedAddPositionsUnsorted, ExpectedEventsUnchecked);
		}

		void AddCheckStateItems (bool sorted, object[] items, CheckState checkState, int[] expectedAddPositions, string expectedEvents)
		{
			CheckedListBox clb = new CheckedListBox ();
			clb.Sorted = sorted;
			ItemCheckLoggingReceiver target = new ItemCheckLoggingReceiver ();
			clb.ItemCheck += new ItemCheckEventHandler (target.HandleItemCheck);
			target._allItemCheckEvents = String.Empty;
			ArrayList addedAtList = new ArrayList ();
			foreach (object cur in items) {
				int idx = clb.Items.Add (cur, checkState);
				addedAtList.Add (idx);
			}
			if (checkState != CheckState.Unchecked)
				AssertAllItemsChecked (clb);
			else
				AssertAllItemsUnchecked (clb);
			Assert.AreEqual ((Array)expectedAddPositions, (Array)addedAtList.ToArray (typeof (int)), "addedAtList");
			Assert.AreEqual (expectedEvents, target._allItemCheckEvents, "events");
		}

		static void AssertAllItemsChecked (CheckedListBox clb)
		{
			//Dump("clb.Items", clb.Items);
			//Dump("clb.CheckedIndices", clb.CheckedIndices);
			//Dump("clb.CheckedItems", clb.CheckedItems);
			Assert.AreEqual (clb.Items.Count, clb.CheckedIndices.Count, "checked count");
			for (int i = 0; i < clb.Items.Count; ++i)
				Assert.AreEqual (i, clb.CheckedIndices[i], "CheckedIndices[i] @" + i);
			for (int i = 0; i < clb.Items.Count; ++i)
				Assert.AreEqual (clb.Items[i], clb.CheckedItems[i], "CheckedItems[i] @" + i);
		}

		static void AssertAllItemsUnchecked (CheckedListBox clb)
		{
			Assert.AreEqual (0, clb.CheckedIndices.Count, "checked count");
			Assert.AreEqual (clb.CheckedIndices.Count, clb.CheckedItems.Count, "checked consistency");
		}


		//----
		static string Join (IEnumerable list)
		{
			global::System.Text.StringBuilder bldr = new global::System.Text.StringBuilder ();
			bldr.Append ("'");
			foreach (object cur in list) {
				bldr.Append (cur);
				bldr.Append ("', '");
			}
			bldr.Append ("'");
			bldr.Length -= 2;
			return bldr.ToString ();
		}

		//static void Dump (string name, IEnumerable list)
		//{
		//	Console.Write (name);
		//	Console.Write (" ");
		//	Dump (list);
		//}
		//
		//static void Dump (IEnumerable list)
		//{
		//	Console.WriteLine ("[" + Join (list) + "]");
		//}

		//--------
		// Check that if the ItemCheck event handler changes the NewValue property 
		// that the new value is used for the item state.

		[Test]
		public void ItemCheckSetNewValue_Adding()
		{
			CheckedListBox clb = new CheckedListBox();
			ItemCheckNewValueSetReceiver target = new ItemCheckNewValueSetReceiver();
			clb.ItemCheck += new ItemCheckEventHandler(target.HandleItemCheck);
			int idx;
			// Unchecked addition.  Note these are not touched by the event
			// as there's no 'check' action.
			target._checkState = CheckState.Indeterminate;
			idx = clb.Items.Add("aaU_x", false);
			Assert.IsFalse(clb.CheckedIndices.Contains(idx), "U_x " + idx);
			target._checkState = CheckState.Unchecked;
			idx = clb.Items.Add("aaU_U", false);
			Assert.IsFalse(clb.CheckedIndices.Contains(idx), "U_U " + idx);
			target._checkState = CheckState.Checked;
			idx = clb.Items.Add("aaU_C", false);
			Assert.IsFalse(clb.CheckedIndices.Contains(idx), "U_C " + idx);
			// Checked addition
			idx = clb.Items.Add("aaC_x", true);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx), "U_x " + idx);
			target._checkState = CheckState.Unchecked;
			idx = clb.Items.Add("aaC_U", true);
			Assert.IsFalse(clb.CheckedIndices.Contains(idx), "C_U " + idx);
			target._checkState = CheckState.Checked;
			idx = clb.Items.Add("aaC_C", true);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx), "C_C " + idx);
		}

		[Test]
		public void ItemCheckSetNewValue_Setting()
		{
			CheckedListBox clb = new CheckedListBox();
			ItemCheckNewValueSetReceiver target = new ItemCheckNewValueSetReceiver();
			clb.Items.Add("aaaa");
			clb.ItemCheck += new ItemCheckEventHandler(target.HandleItemCheck);
			const int idx = 0;
			// Note here the SetItemChecked(idx, false) actions *do* raise an item 
			// check event so the result is touched by the event handler.
			// Uncheck!
			SetInitialState(clb, idx, true, target);
			target._checkState = CheckState.Indeterminate;
			clb.SetItemChecked(idx, false);
			Assert.IsFalse(clb.CheckedIndices.Contains(idx), "C_U_x ");
			SetInitialState(clb, idx, true, target);
			target._checkState = CheckState.Unchecked;
			clb.SetItemChecked(idx, false);
			Assert.IsFalse(clb.CheckedIndices.Contains(idx), "C_U_U ");
			SetInitialState(clb, idx, true, target);
			target._checkState = CheckState.Checked;
			clb.SetItemChecked(idx, false);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx), "C_U_C ");
			// Check!
			SetInitialState(clb, idx, false, target);
			target._checkState = CheckState.Indeterminate;
			clb.SetItemChecked(idx, true);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx), "U_C_x ");
			SetInitialState(clb, idx, false, target);
			target._checkState = CheckState.Unchecked;
			clb.SetItemChecked(idx, true);
			Assert.IsFalse(clb.CheckedIndices.Contains(idx), "U_C_U ");
			SetInitialState(clb, idx, false, target);
			target._checkState = CheckState.Checked;
			clb.SetItemChecked(idx, true);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx), "U_C_C ");
		}
		private void SetInitialState(CheckedListBox clb, int index, bool isChecked, ItemCheckNewValueSetReceiver target)
		{
			CheckState originalCS = target._checkState;
			target._checkState = CheckState.Indeterminate;  // No touchee in the event handler.
			clb.SetItemChecked(index, isChecked);
			target._checkState = originalCS;
		}

		class ItemCheckNewValueSetReceiver
		{
			public CheckState _checkState = CheckState.Indeterminate;

			public void HandleItemCheck(object sender, ItemCheckEventArgs e)
			{
				CheckState originalNewValue = e.NewValue; //logging
				bool changed = false; //logging
				switch (_checkState) {
					case CheckState.Unchecked:
						e.NewValue = CheckState.Unchecked;
						changed = true;
						break;
					case CheckState.Checked:
						e.NewValue = CheckState.Checked;
						changed = true;
						break;
					default:    // No touchee!
						global::System.Diagnostics.Debug.Assert(_checkState == CheckState.Indeterminate);
						break;
				}
#if false // logging
				Console.WriteLine("ItemCheck, item#: {0,2}, current: {1,9}, new was: {2,9}{4}",
					e.Index, e.CurrentValue, originalNewValue, e.NewValue,
					(changed ? String.Format(", ! is: {0,9}", e.NewValue) : String.Empty));
#endif
			}
		}

		[Test]
		public void ResetCheckStateOnRemove()
		{
			CheckedListBox clb = new CheckedListBox();
			int idx = clb.Items.Add("a", true);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx));
			Assert.AreEqual(1, clb.CheckedIndices.Count);
			clb.Items.Clear();
			Assert.AreEqual(0, clb.CheckedIndices.Count);

			idx = clb.Items.Add("a", true);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx));
			Assert.AreEqual(1, clb.CheckedIndices.Count);
			clb.Items.RemoveAt(idx);
			Assert.AreEqual(0, clb.CheckedIndices.Count);

			idx = clb.Items.Add("a", true);
			Assert.IsTrue(clb.CheckedIndices.Contains(idx));
			Assert.AreEqual(1, clb.CheckedIndices.Count);
			clb.Items.Remove("a");
			Assert.AreEqual(0, clb.CheckedIndices.Count);
		}
	}
}
