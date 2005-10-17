//
// TabControlTest.cs: Test cases for TabControl.
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
public class TabControlTest
{
	[Test]
	public void TabControlPropertyTest ()
	{
		Form myForm = new Form ();
		TabControl myTabControl = new TabControl ();
		myTabControl.Visible = true;
		myTabControl.Name = "Mono TabControl";
		
		// A 
		Assert.AreEqual (TabAlignment.Top, myTabControl.Alignment, "A1");
		Assert.AreEqual (TabAppearance.Normal, myTabControl.Appearance, "#A2");
		
		// D 
		Assert.AreEqual (4, myTabControl.DisplayRectangle.X, "#D1");
		Assert.AreEqual (4, myTabControl.DisplayRectangle.Y, "#D2");
		Assert.AreEqual (192, myTabControl.DisplayRectangle.Width, "#D3");
		Assert.AreEqual (92, myTabControl.DisplayRectangle.Height, "#D4");
		Assert.AreEqual (TabDrawMode.Normal, myTabControl.DrawMode, "#D5");
		
		// H
		Assert.AreEqual (false, myTabControl.HotTrack, "#H1");
		
		// I 
		Assert.AreEqual (null, myTabControl.ImageList, "#I1");
		Assert.AreEqual (18, myTabControl.ItemSize.Height, "#I2");
		Assert.AreEqual (0, myTabControl.ItemSize.Width, "#I3");

		// M 
		Assert.AreEqual (false, myTabControl.Multiline, "#M1");
		
		// P
		Assert.AreEqual (6, myTabControl.Padding.X, "#P1");
		Assert.AreEqual (3, myTabControl.Padding.Y, "#P1");

		// R
		Assert.AreEqual (0, myTabControl.RowCount, "#R1");

		// S
		Assert.AreEqual (-1, myTabControl.SelectedIndex, "#S1");
                Assert.AreEqual (null, myTabControl.SelectedTab, "#S2");
		Assert.AreEqual (false, myTabControl.ShowToolTips, "#S3");
		Assert.AreEqual (TabSizeMode.Normal, myTabControl.SizeMode, "#S4");

		// T
		Assert.AreEqual (0, myTabControl.TabCount, "#T1");
		Assert.AreEqual (0, myTabControl.TabPages.Count, "#T2");
	}

	[Test]
	public void GetTabRectTest ()
	{
		TabControl myTabControl = new TabControl ();
		TabPage myTabPage = new TabPage();
		myTabControl.Controls.Add(myTabPage);
		myTabPage.TabIndex = 0;
		Rectangle myTabRect = new Rectangle ();
		Assert.AreEqual (2, myTabControl.GetTabRect (0).X, "#GetT1");
		Assert.AreEqual (2, myTabControl.GetTabRect (0).Y, "#GetT2");
		Assert.AreEqual (42, myTabControl.GetTabRect (0).Width, "#GetT3");
		Assert.AreEqual (18, myTabControl.GetTabRect (0).Height, "#GetT4");
	}		

	[Test]
	public void ToStringTest ()
	{
		TabControl myTabControl = new TabControl ();
		Assert.AreEqual ("System.Windows.Forms.TabControl, TabPages.Count: 0", myTabControl.ToString(), "#Str1");
	}
}
