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
	public class CheckedListBoxTest
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
		[NUnit.Framework.Category ("NotWorking")]
		public void GetItemText ()
		{
			MockItem itemA = new MockItem ("A", 1);
			MockItem itemB = new MockItem ("B", 2);

			CheckedListBox clb = new CheckedListBox ();
			clb.DisplayMember = "Text";
			clb.Items.Add (itemA, true);

			Assert.AreEqual ("A", clb.GetItemText (itemA), "#A1");
			Assert.AreEqual ("B", clb.GetItemText (itemB), "#A2");

			clb.DisplayMember = string.Empty;

			Assert.AreEqual (itemA.GetType ().FullName, clb.GetItemText (itemA), "#B1");
			Assert.AreEqual (itemB.GetType ().FullName, clb.GetItemText (itemB), "#B2");
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

#if NET_2_0
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
#endif

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
#if NET_2_0
			public bool allow_selection {
				get { return base.AllowSelection; }
			}
#endif
		}

		public class MockItem
		{
			public MockItem (string text, int value)
			{
				_text = text;
				_value = value;
			}

			public string Text {
				get { return _text; }
			}

			public int Value {
				get { return _value; }
			}

			private readonly string _text;
			private readonly int _value;
		}
	}
}
