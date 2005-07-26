//
// ComboBoxTest.cs: Test cases for ComboBox.
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

[TestFixture]
public class ComboBoxTest
{
	[Test]
	public void ComboBoxPropertyTest ()
	{
		Form myfrm = new Form ();
		ComboBox mycmbbox = new ComboBox ();
		Assert.AreEqual (DrawMode.Normal, mycmbbox.DrawMode, "#1");
		Assert.AreEqual (ComboBoxStyle.DropDown, mycmbbox.DropDownStyle, "#2");
		Assert.AreEqual (121, mycmbbox.DropDownWidth, "#3");
		Assert.AreEqual (false, mycmbbox.DroppedDown, "#4");
		Assert.AreEqual (true, mycmbbox.IntegralHeight, "#5");
		Assert.AreEqual (0, mycmbbox.Items.Count, "#6");
		Assert.AreEqual (15, mycmbbox.ItemHeight, "#7");
		Assert.AreEqual (8, mycmbbox.MaxDropDownItems, "#8");
		Assert.AreEqual (0, mycmbbox.MaxLength, "#9");
		Assert.AreEqual (20, mycmbbox.PreferredHeight, "#10");
		Assert.AreEqual (-1, mycmbbox.SelectedIndex, "#11");
		Assert.AreEqual (null, mycmbbox.SelectedItem, "#12");
		Assert.AreEqual ("", mycmbbox.SelectedText, "#13");
		Assert.AreEqual (0, mycmbbox.SelectionLength, "#14");
		Assert.AreEqual (0, mycmbbox.SelectionStart, "#15");
		Assert.AreEqual (false, mycmbbox.Sorted, "#16");
		Assert.AreEqual ("", mycmbbox.Text, "#17");
	}

	[Test]
	public void BeginEndUpdateTest ()
	{
		Form myform = new Form ();
		myform.Visible = true;
		ComboBox cmbbox = new ComboBox ();
		cmbbox.Items.Add ("A");
		cmbbox.Visible = true;
		myform.Controls.Add (cmbbox);
		cmbbox.BeginUpdate ();
		for (int x = 1 ; x < 5000 ; x++) {
			cmbbox.Items.Add ("Item " + x.ToString ());   
		}
		cmbbox.EndUpdate ();
	}		

	[Test]
	public void FindStringTest ()
	{
		ComboBox cmbbox = new ComboBox ();
		cmbbox.Items.AddRange(new object [] {"ACBD", "ABDC", "ACBD", "ABCD"});
		String myString = "ABC";
		int x = cmbbox.FindString (myString);
		Assert.AreEqual (3, x, "#19");
	}

	[Test]
	public void FindStringExactTest ()
	{
		ComboBox cmbbox = new ComboBox ();
		cmbbox.Items.AddRange (new object [] {"ABCD","ABC","ABDC"});
		String myString = "ABC";
		int x = cmbbox.FindStringExact (myString);
		Assert.AreEqual (1, x, "#20");
	}

	[Test]
	public void GetItemHeightTest ()
	{
		ComboBox cmbbox = new ComboBox ();
		cmbbox.Items.Add ("ABC");
		cmbbox.Items.Add ("BCD");
		cmbbox.Items.Add ("DEF");
		int x = -1;
		x = cmbbox.GetItemHeight (x);
		Assert.IsTrue (cmbbox.ItemHeight > 0, "#21");  
	}

	[Test]
	public void SelectAllTest ()
	{
		ComboBox cmbbox = new ComboBox ();
		cmbbox.Items.Add ("ABC");
		cmbbox.Items.Add ("BCD");
		cmbbox.Items.Add ("DEF");
		int x = -1;
		x = cmbbox.GetItemHeight (x);
		Assert.AreEqual (15, cmbbox.ItemHeight, "#22");
	}
}
