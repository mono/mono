using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

[TestFixture]
public class TreeViewTest {

	[Test]
	public void DefaultCtor ()
	{
		TreeView tv = new TreeView ();
		Assert.AreEqual (121, tv.Width, "#1");
		Assert.AreEqual (97, tv.Height, "#2");
		//Assert.AreEqual (BorderStyle.Fixed3D, tv.BorderStyle, "#3");

		// Windows specific
		Assert.AreEqual (SystemColors.Window, tv.BackColor);
	}

	[Test]
	public void SimpleShowTest ()
	{
		Form f = new Form ();
		TreeView tv = new TreeView ();
		//tv.BorderStyle = BorderStyle.FixedSingle;
		tv.Location = new Point (20, 20);
		//tv.Text = "adssssss";

		f.Controls.Add (tv);
		f.Show ();
	}
}
