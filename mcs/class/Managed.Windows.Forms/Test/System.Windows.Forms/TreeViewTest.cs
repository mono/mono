using System;
using NUnit.Framework;
using System.Collections;
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
			f.ShowInTaskbar = false;
			TreeView tv = new TreeView ();
			//tv.BorderStyle = BorderStyle.FixedSingle;
			tv.Location = new Point (20, 20);
			//tv.Text = "adssssss";

			f.Controls.Add (tv);
			f.Show ();
			f.Dispose ();
		}

		[Test]
		public void NodesCopyToTest ()
		{
			TreeView tv = new TreeView();
			TreeNode one = new TreeNode ("one");
			TreeNode two = new TreeNode ("two");
			TreeNode three = new TreeNode ("three");

			tv.Nodes.Add (one);
			tv.Nodes.Add (two);
			tv.Nodes.Add (three);

			ArrayList al = new ArrayList (tv.Nodes);

			Assert.AreEqual (al [0], tv.Nodes [0], "A1");
			Assert.AreEqual (al [1], tv.Nodes [1], "A2");
			Assert.AreEqual (al [2], tv.Nodes [2], "A3");
		}
	}
}
