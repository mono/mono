using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TreeViewTest
	{
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

	[TestFixture]
	public class BeforeSelectEvent
	{
		int beforeSelect;
		TreeViewCancelEventArgs cancelEventArgs;
		bool cancel;

		public void TreeView_BeforeSelect (object sender, TreeViewCancelEventArgs e)
		{
			beforeSelect++;
			cancelEventArgs = e;
			if (cancel)
				e.Cancel = true;
		}

		[SetUp]
		public void SetUp ()
		{
			beforeSelect = 0;
			cancelEventArgs = null;
			cancel = false;
		}

		[Test]
		[Category ("NotWorking")]
		public void CancelBeforeCreationOfHandle ()
		{
			TreeView tvw = new TreeView ();
			tvw.BeforeSelect += new TreeViewCancelEventHandler (TreeView_BeforeSelect);
			cancel = true;

			TreeNode nodeA = tvw.Nodes.Add ("A");
			Assert.AreEqual (0, beforeSelect, "#A1");
			tvw.SelectedNode = nodeA;
			Assert.AreEqual (0, beforeSelect, "#A2");
			Assert.IsNull (cancelEventArgs, "#A3");
			Assert.IsFalse (nodeA.IsSelected, "#A4");
			Assert.AreSame (nodeA, tvw.SelectedNode, "#A5");

			TreeNode nodeB = tvw.Nodes.Add ("B");
			Assert.AreEqual (0, beforeSelect, "#B1");
			tvw.SelectedNode = nodeB;
			Assert.AreEqual (0, beforeSelect, "#B2");
			Assert.IsNull (cancelEventArgs, "#B3");
			Assert.IsFalse (nodeB.IsSelected, "#B4");
			Assert.AreSame (nodeB, tvw.SelectedNode, "#B5");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tvw);
			form.Show ();

			Assert.AreEqual (2, beforeSelect, "#C1");
			Assert.IsNotNull (cancelEventArgs, "#C2");
			Assert.AreEqual (TreeViewAction.Unknown, cancelEventArgs.Action, "#C3");
			Assert.AreSame (nodeA, cancelEventArgs.Node, "#C4");
			Assert.IsFalse (nodeA.IsSelected, "#C5");
			Assert.IsFalse (nodeB.IsSelected, "#C6");
			Assert.IsNull (tvw.SelectedNode, "#C7");
		}

		[Test]
		[Category ("NotWorking")]
		public void SelectBeforeCreationOfHandle ()
		{
			TreeView tvw = new TreeView ();
			tvw.BeforeSelect += new TreeViewCancelEventHandler (TreeView_BeforeSelect);

			TreeNode nodeA =  tvw.Nodes.Add ("A");
			Assert.AreEqual (0, beforeSelect, "#A1");
			tvw.SelectedNode = nodeA;
			Assert.AreEqual (0, beforeSelect, "#A2");
			Assert.IsNull (cancelEventArgs, "#A3");
			Assert.IsFalse (nodeA.IsSelected, "#A4");
			Assert.AreSame (nodeA, tvw.SelectedNode, "#A5");

			TreeNode nodeB = tvw.Nodes.Add ("B");
			Assert.AreEqual (0, beforeSelect, "#B1");
			tvw.SelectedNode = nodeB;
			Assert.AreEqual (0, beforeSelect, "#B2");
			Assert.IsNull (cancelEventArgs, "#B3");
			Assert.IsFalse (nodeB.IsSelected, "#B4");
			Assert.AreSame (nodeB, tvw.SelectedNode, "#B5");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tvw);
			form.Show ();

			Assert.AreEqual (1, beforeSelect, "#C1");
			Assert.IsNotNull (cancelEventArgs, "#C2");
			Assert.AreEqual (TreeViewAction.Unknown, cancelEventArgs.Action, "#C3");
			Assert.AreSame (nodeB, cancelEventArgs.Node, "#C4");
			Assert.IsFalse (nodeA.IsSelected, "#C5");
			Assert.IsTrue (nodeB.IsSelected, "#C6");
			Assert.AreSame (nodeB, tvw.SelectedNode, "#C7");
		}

		[Test]
		[Category ("NotWorking")]
		public void CancelAfterCreationOfHandle ()
		{
			TreeView tvw = new TreeView ();
			tvw.BeforeSelect += new TreeViewCancelEventHandler (TreeView_BeforeSelect);
			cancel = true;

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tvw);
			form.Show ();

			TreeNode nodeA = tvw.Nodes.Add ("A");
			Assert.AreEqual (0, beforeSelect, "#A1");
			tvw.SelectedNode = nodeA;
			Assert.AreEqual (1, beforeSelect, "#A2");
			Assert.IsNotNull (cancelEventArgs, "#A3");
			Assert.AreEqual (TreeViewAction.Unknown, cancelEventArgs.Action, "#A4");
			Assert.AreSame (nodeA, cancelEventArgs.Node, "#A5");
			Assert.IsFalse (nodeA.IsSelected, "#A6");
			Assert.IsNull (tvw.SelectedNode, "#A7");

			TreeNode nodeB = tvw.Nodes.Add ("B");
			Assert.AreEqual (1, beforeSelect, "#B1");
			tvw.SelectedNode = nodeB;
			Assert.AreEqual (2, beforeSelect, "#B2");
			Assert.IsNotNull (cancelEventArgs, "#B3");
			Assert.AreEqual (TreeViewAction.Unknown, cancelEventArgs.Action, "#B4");
			Assert.AreSame (nodeB, cancelEventArgs.Node, "#B5");
			Assert.IsFalse (nodeB.IsSelected, "#B6");
			Assert.IsNull (tvw.SelectedNode, "#B7");
		}

		[Test]
		[Category ("NotWorking")]
		public void SelectAfterCreationOfHandle ()
		{
			TreeView tvw = new TreeView ();
			tvw.BeforeSelect += new TreeViewCancelEventHandler (TreeView_BeforeSelect);

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tvw);
			form.Show ();

			TreeNode nodeA = tvw.Nodes.Add ("A");
			Assert.AreEqual (0, beforeSelect, "#A1");
			tvw.SelectedNode = nodeA;
			Assert.AreEqual (1, beforeSelect, "#A2");
			Assert.IsNotNull (cancelEventArgs, "#A3");
			Assert.AreEqual (TreeViewAction.Unknown, cancelEventArgs.Action, "#A4");
			Assert.AreSame (nodeA, cancelEventArgs.Node, "#A5");
			Assert.IsTrue (nodeA.IsSelected, "#A6");
			Assert.AreSame (nodeA, tvw.SelectedNode, "#A7");

			TreeNode nodeB = tvw.Nodes.Add ("B");
			Assert.AreEqual (1, beforeSelect, "#B1");
			tvw.SelectedNode = nodeB;
			Assert.AreEqual (2, beforeSelect, "#B2");
			Assert.IsNotNull (cancelEventArgs, "#B3");
			Assert.AreEqual (TreeViewAction.Unknown, cancelEventArgs.Action, "#B4");
			Assert.AreSame (nodeB, cancelEventArgs.Node, "#B5");
			Assert.IsTrue (nodeB.IsSelected, "#B6");
			Assert.AreSame (nodeB, tvw.SelectedNode, "#B7");
		}
	}
}
