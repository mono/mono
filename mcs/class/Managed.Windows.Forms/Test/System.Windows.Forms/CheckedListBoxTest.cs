//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//		Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
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
		}

		[Test]
		public void GetItemCheckedTest ()
		{
			Form f = new Form ();
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
		}	

		[Test]
		public void SetItemCheckedTest ()
		{
			Form myform = new Form ();
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
		}

		[Test]
		public void SetItemCheckStateTest ()
		{
			Form myform = new Form ();
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
		}	
	}
}
