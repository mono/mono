//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//		Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

[TestFixture]
public class ListBoxTest
{
	[Test]
	public void ListBoxPropertyTest()
	{
		ListBox lb1 = new ListBox();
		Assert.AreEqual(0, lb1.ColumnWidth, "#1");
		Assert.AreEqual(DrawMode.Normal, lb1.DrawMode, "#2");
		Assert.AreEqual(0 , lb1.HorizontalExtent, "#3");
		Assert.AreEqual(false , lb1.HorizontalScrollbar, "#4");
		Assert.AreEqual(true , lb1.IntegralHeight, "#5");
		Assert.AreEqual(13 , lb1.ItemHeight, "#6");
		lb1.Items.Add("a");
		lb1.Items.Add("b");
		lb1.Items.Add("c");
		Assert.AreEqual("System.Windows.Forms.ListBox+ObjectCollection",  lb1.Items.ToString(), "#7");
		Assert.AreEqual(false , lb1.MultiColumn, "#8");
		Assert.AreEqual(46 , lb1.PreferredHeight, "#9");
		Assert.AreEqual(RightToLeft.No , lb1.RightToLeft, "#10");
		Assert.AreEqual(false , lb1.ScrollAlwaysVisible, "#11");
		Assert.AreEqual(-1 , lb1.SelectedIndex, "#12");
		Assert.AreEqual("System.Windows.Forms.ListBox+SelectedIndexCollection", lb1.SelectedIndices.ToString(), "#13");
		Assert.AreEqual(null , lb1.SelectedItem, "#14");
		Assert.AreEqual("System.Windows.Forms.ListBox+SelectedObjectCollection" , lb1.SelectedItems.ToString(), "#15");
		Assert.AreEqual(SelectionMode.One , lb1.SelectionMode, "#16");
		Assert.AreEqual(false , lb1.Sorted, "#17");
		Assert.AreEqual("" , lb1.Text, "#18");
		Assert.AreEqual(0 , lb1.TopIndex, "#19");
		Assert.AreEqual(true , lb1.UseTabStops, "#20");
	}

	[Test]
	public void BeginEndUpdateTest()
	{
		Form f = new Form ();
		f.Visible = true;
		ListBox lb1 = new ListBox();
		lb1.Items.Add("A");
		lb1.Visible = true;
		f.Controls.Add(lb1);
		lb1.BeginUpdate();
		for(int x = 1; x < 5000; x++)
		{
			lb1.Items.Add("Item " + x.ToString());   
		}
		lb1.EndUpdate();
		lb1.SetSelected(1, true);
		lb1.SetSelected(3, true);
		Assert.AreEqual("Item 3", lb1.SelectedItems[0].ToString(),"#21");
	}		

	[Test]
	public void ClearSelectedTest()
	{
		Form f = new Form ();
		f.Visible = true;
		ListBox lb1 = new ListBox();
		lb1.Items.Add("A");
		lb1.Visible = true;
		f.Controls.Add(lb1);
		lb1.SetSelected(0, true);
		Assert.AreEqual("A", lb1.SelectedItems[0].ToString(),"#22");
		lb1.ClearSelected();
		Assert.AreEqual(0, lb1.SelectedItems.Count,"#23");
	}

	[Test]
	public void FindStringTest()
	{
		ListBox lb1 = new ListBox();
		lb1.Items.Add("ABCD");
		lb1.Items.Add("DABCD");
		lb1.Items.Add("ABDC");
		lb1.SelectionMode = SelectionMode.MultiExtended;
		int x = -1;
		x = lb1.FindString("ABC", x );
		lb1.SetSelected(x,true);
		Assert.AreEqual("ABCD", lb1.SelectedItems[0],"#24");
		Assert.AreEqual(1, lb1.SelectedItems.Count,"#25");
	}

	[Test]
	public void FindExactTest()
	{
		ListBox lb2 = new ListBox();
		lb2.Items.Add("ABC");
		lb2.Items.Add("DEFGHI");
		lb2.Items.Add("DEF");
		int x = -1;
		x = lb2.FindStringExact("DEF", x);
		lb2.SetSelected(x,true);
		Assert.AreEqual(1, lb2.SelectedItems.Count,"#26");
		Assert.AreEqual("DEF", lb2.SelectedItems[0],"#27");
	}

  	[Test]
	public void GetItemHeightTest()
	{
		Form f = new Form ();
		ListBox lb1 = new ListBox();
		lb1.Visible = true;
		f.Controls.Add(lb1);
		lb1.Items.Add("A");
		Assert.AreEqual(13, lb1.GetItemHeight(0) , "#28");
	}

	[Test]
	public void GetItemRectangleTest()
	{
		Form f = new Form ();
		f.Visible = true;
		ListBox lb1 = new ListBox();
		lb1.Visible = true;
		f.Controls.Add(lb1);
		lb1.Items.Add("A");
		//Assert.AreEqual(Rectangle.Equals(116,13), lb1.GetItemRectangle(0) , "#29");
	}

	[Test]
	public void GetSelectedTest()
	{
		ListBox lb1 = new ListBox();
		lb1.Items.Add("A");
		lb1.Items.Add("B");
		lb1.Items.Add("C");
		lb1.Items.Add("D");
		lb1.Sorted = true;
		lb1.SetSelected(0,true);
		lb1.SetSelected(2,true);
		lb1.TopIndex=0;
		Assert.AreEqual(true , lb1.GetSelected(0), "#30");
		lb1.SetSelected(2,false);
		Assert.AreEqual(false , lb1.GetSelected(2), "#31");
	}

	[Test]
	public void IndexFromPointTest ()
	{
		ListBox lb1 = new ListBox();
		lb1.Items.Add("A");
		Point pt = new Point(100,100);
		int index = lb1.IndexFromPoint(pt);
		Assert.AreEqual(-1 , lb1.IndexFromPoint(100,100), "#32");
	}

}