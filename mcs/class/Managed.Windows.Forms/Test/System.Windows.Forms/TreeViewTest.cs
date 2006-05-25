using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;


namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class TreeViewTest {

		[Test]
		public void DefaultCtor ()
		{
			TreeView tv = new TreeView ();
			Assert.AreEqual (121, tv.Width, "#1");
			Assert.AreEqual (97, tv.Height, "#2");

			Assert.IsTrue (tv.Scrollable, "#3");
			Assert.AreEqual (tv.SelectedNode, null, "#4");
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
}
