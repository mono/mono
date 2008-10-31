using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TreeViewTest : TestHelper
	{
		[Test] // bug #80620
		public void ClientRectangle_Borders ()
		{
			TreeView tv = new TreeView ();
			tv.CreateControl ();
			Assert.AreEqual (tv.ClientRectangle, new TreeView ().ClientRectangle);
		}

		[Test]
		public void CollapseAll_NoNodes ()
		{
			TreeView tv = new TreeView ();
			tv.CollapseAll ();
		}
	
		[Test]
		public void DefaultCtor ()
		{
			TreeView tv = new TreeView ();
			Assert.AreEqual (121, tv.Width, "#1");
			Assert.AreEqual (97, tv.Height, "#2");

			Assert.IsTrue (tv.Scrollable, "#3");
			Assert.AreEqual (tv.SelectedNode, null, "#4");
		}

#if NET_2_0
		[Test] // bug #81424
		public void DoubleBuffered ()
		{
			MockTreeView tv = new MockTreeView ();
			Assert.IsFalse (tv.IsDoubleBuffered, "#A1");
			Assert.IsTrue (tv.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#A2");
			Assert.IsFalse (tv.GetControlStyle (ControlStyles.DoubleBuffer), "#A3");
			Assert.IsFalse (tv.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#A4");
			Assert.IsFalse (tv.GetControlStyle (ControlStyles.UserPaint), "#A5");
			Assert.IsFalse (tv.GetControlStyle (ControlStyles.StandardClick), "#A6");

			tv.IsDoubleBuffered = true;
			Assert.IsTrue (tv.IsDoubleBuffered, "#B1");
			Assert.IsTrue (tv.GetControlStyle (ControlStyles.AllPaintingInWmPaint), "#B2");
			Assert.IsFalse (tv.GetControlStyle (ControlStyles.DoubleBuffer), "#B3");
			Assert.IsTrue (tv.GetControlStyle (ControlStyles.OptimizedDoubleBuffer), "#B4");
			Assert.IsFalse (tv.GetControlStyle (ControlStyles.UserPaint), "#B5");
		}
#endif

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
		public void NodeAddIndex ()
		{
			TreeView tv = new TreeView ();

			TreeNode tn1 = new TreeNode ("blah");
			TreeNode tn2 = new TreeNode ("blah2");
			TreeNode tn3 = new TreeNode ("blah3");

			Assert.AreEqual (0, tv.Nodes.Add (tn1), "A1");
			Assert.AreEqual (1, tv.Nodes.Add (tn2), "A2");
			Assert.AreEqual (2, tv.Nodes.Add (tn3), "A3");
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

		[Test]
		public void ExpandAll_Flat_Created ()
		{
			TreeView tv = new TreeView ();
			tv.Size = new Size (300, 100);

			for (int i = 0; i <= 100; i++)
				tv.Nodes.Add (i.ToString (CultureInfo.InvariantCulture));

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tv);
			form.Show ();

			Assert.IsFalse (tv.Nodes [0].IsExpanded, "#A1");
			Assert.IsFalse (tv.Nodes [99].IsExpanded, "#A2");

			Assert.IsTrue (tv.Nodes [0].IsVisible, "#B1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#B2");

			tv.ExpandAll ();

			Assert.IsFalse (tv.Nodes [0].IsExpanded, "#C1");
			Assert.IsFalse (tv.Nodes [99].IsExpanded, "#C2");

			Assert.IsTrue (tv.Nodes [0].IsVisible, "#D1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#D2");
			
			form.Close ();
		}

		[Test]
		public void ExpandAll_Tree_Created ()
		{
			TreeView tv = new TreeView ();
			tv.Size = new Size (300, 100);

			for (int i = 0; i <= 100; i++) {
				TreeNode node = tv.Nodes.Add (i.ToString (CultureInfo.InvariantCulture));
				node.Nodes.Add ("child");
			}

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tv);
			form.Show ();

			Assert.IsFalse (tv.Nodes [0].IsExpanded, "#A1");
			Assert.IsFalse (tv.Nodes [99].IsExpanded, "#A2");

			Assert.IsTrue (tv.Nodes [0].IsVisible, "#B1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#B2");

			tv.ExpandAll ();

			Assert.IsTrue (tv.Nodes [0].IsExpanded, "#C1");
			Assert.IsTrue (tv.Nodes [99].IsExpanded, "#C2");

			Assert.IsFalse (tv.Nodes [0].IsVisible, "#D1");
			Assert.IsTrue (tv.Nodes [99].IsVisible, "#D2");
			
			form.Close ();
		}

		[Test]
		public void ExpandAll_Flat_NotCreated ()
		{
			TreeView tv = new TreeView ();
			tv.Size = new Size (300, 100);

			for (int i = 0; i <= 100; i++)
				tv.Nodes.Add (i.ToString (CultureInfo.InvariantCulture));

			Assert.IsFalse (tv.Nodes [0].IsExpanded, "#A1");
			Assert.IsFalse (tv.Nodes [99].IsExpanded, "#A2");

			Assert.IsFalse (tv.Nodes [0].IsVisible, "#B1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#B2");

			tv.ExpandAll ();

			Assert.IsTrue (tv.Nodes [0].IsExpanded, "#C1");
			Assert.IsTrue (tv.Nodes [99].IsExpanded, "#C2");

			Assert.IsFalse (tv.Nodes [0].IsVisible, "#D1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#D2");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tv);
			form.Show ();

			Assert.IsFalse (tv.Nodes [0].IsExpanded, "#E1");
			Assert.IsFalse (tv.Nodes [99].IsExpanded, "#E2");

			Assert.IsTrue (tv.Nodes [0].IsVisible, "#F1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#F2");

			form.Dispose ();
		}

		[Test] // bug #80284
		public void ExpandAll_Tree_NotCreated ()
		{
			TreeView tv = new TreeView ();
			tv.Size = new Size (300, 100);

			for (int i = 0; i <= 100; i++) {
				TreeNode node = tv.Nodes.Add (i.ToString (CultureInfo.InvariantCulture));
				node.Nodes.Add ("child");
			}

			Assert.IsFalse (tv.Nodes [0].IsExpanded, "#A1");
			Assert.IsFalse (tv.Nodes [99].IsExpanded, "#A2");

			Assert.IsFalse (tv.Nodes [0].IsVisible, "#B1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#B2");

			tv.ExpandAll ();

			Assert.IsTrue (tv.Nodes [0].IsExpanded, "#C1");
			Assert.IsTrue (tv.Nodes [99].IsExpanded, "#C2");

			Assert.IsFalse (tv.Nodes [0].IsVisible, "#D1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#D2");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tv);
			form.Show ();

			Assert.IsTrue (tv.Nodes [0].IsExpanded, "#E1");
			Assert.IsTrue (tv.Nodes [99].IsExpanded, "#E2");

#if NET_2_0
			Assert.IsTrue (tv.Nodes [0].IsVisible, "#F1");
			Assert.IsFalse (tv.Nodes [99].IsVisible, "#F2");
#else
			Assert.IsFalse (tv.Nodes [0].IsVisible, "#F1");
			Assert.IsTrue (tv.Nodes [99].IsVisible, "#F2");
#endif

			form.Dispose ();
		}

		class MockTreeView : TreeView
		{
			public bool GetControlStyle (ControlStyles style)
			{
				return base.GetStyle (style);
			}

#if NET_2_0
			public bool IsDoubleBuffered {
				get { return DoubleBuffered; }
				set { DoubleBuffered = value; }
			}
#endif
		}

		[Test]
		public void MethodIsInputChar ()
		{
			// Basically, show that this method always returns true
			InputCharControl m = new InputCharControl ();
			bool result = true;

			for (int i = 0; i < 256; i++)
				result &= m.PublicIsInputChar ((char)i);

			Assert.AreEqual (true, result, "I1");
		}

		private class InputCharControl : TreeView
		{
			public bool PublicIsInputChar (char charCode)
			{
				return base.IsInputChar (charCode);
			}
		}
		
#if NET_2_0
		[Test]
		public void SelectedNodeNullTest ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			
			TreeView tv = new TreeView ();
			tv.Nodes.Add ("Node 1");
			
			f.Controls.Add (tv);
			f.Show ();
			
			tv.BeforeSelect += new TreeViewCancelEventHandler (delegate (object sender, TreeViewCancelEventArgs e) { if (e.Node == null) Assert.Fail ("BeforeSelect should not be called with a null node"); });
			tv.AfterSelect += new TreeViewEventHandler (delegate (object sender, TreeViewEventArgs e) { if (e.Node == null) Assert.Fail ("AfterSelect should not be called with a null node"); });
			
			tv.SelectedNode = null;
			
			f.Dispose ();
		}

		[Test]
		public void SortChangesSorted ()
		{
			TreeView tv = new TreeView ();
			
			Assert.AreEqual (false, tv.Sorted, "A1");
			
			tv.Sort ();
			Assert.AreEqual (true, tv.Sorted, "A2");
		}
		
		[Test]
		public void SortedBeforeNodeAdd ()
		{
			TreeView tv = new TreeView ();
			tv.Sort ();
			
			tv.Nodes.Add ("bbb");
			tv.Nodes.Add ("aaa");
			
			Assert.AreEqual ("aaa", tv.Nodes[0].Text, "A1");
		}
#endif

		[Test]
		public void MethodToString ()
		{
			TreeView tv = new TreeView ();

			Assert.AreEqual (@"System.Windows.Forms.TreeView, Nodes.Count: 0", tv.ToString (), "A1");

			tv.Nodes.Add ("A");
			tv.Nodes.Add ("B");

			Assert.AreEqual (@"System.Windows.Forms.TreeView, Nodes.Count: 2, Nodes[0]: TreeNode: A", tv.ToString (), "A2");
		}
	}

	[TestFixture]
	public class BeforeSelectEvent : TestHelper
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
		protected override void SetUp () {
			beforeSelect = 0;
			cancelEventArgs = null;
			cancel = false;
			base.SetUp ();
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
			form.Close ();
		}

		[Test]
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
			form.Close ();
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

	[TestFixture]
	public class AfterSelectEvent : TestHelper
	{
		[SetUp]
		protected override void SetUp () {
			afterSelect = 0;
			base.SetUp ();
		}

		[Test] // bug #81319
		[Category ("NotWorking")]
		public void SelectedNode_Created ()
		{
			TreeView tv = new TreeView ();
			tv.AfterSelect += new TreeViewEventHandler (TreeView_AfterSelect);
			TreeNode one = new TreeNode ("one");
			TreeNode two = new TreeNode ("two");

			tv.Nodes.Add (one);
			tv.Nodes.Add (two);

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tv);
			form.Show ();

			Assert.IsNotNull (tv.SelectedNode, "#A1");
			Assert.AreSame (one, tv.SelectedNode, "#A2");
			Assert.IsTrue (one.IsSelected, "#A3");
			Assert.IsFalse (two.IsSelected, "#A4");
			Assert.AreEqual (1, afterSelect, "#A5");

			tv.SelectedNode = null;
			Assert.IsNull (tv.SelectedNode, "#C1");
			Assert.IsFalse (one.IsSelected, "#C2");
			Assert.IsFalse (two.IsSelected, "#C3");
			Assert.AreEqual (1, afterSelect, "#C4");

			tv.SelectedNode = two;
			Assert.IsNotNull (tv.SelectedNode, "#D1");
			Assert.AreSame (two, tv.SelectedNode, "#D2");
			Assert.IsFalse (one.IsSelected, "#D3");
			Assert.IsTrue (two.IsSelected, "#D4");
			Assert.AreEqual (2, afterSelect, "#D5");

			form.Dispose ();
		}

		[Test]
		public void SelectedNode_NotCreated ()
		{
			TreeView tv = new TreeView ();
			tv.AfterSelect += new TreeViewEventHandler (TreeView_AfterSelect);
			TreeNode one = new TreeNode ("one");
			TreeNode two = new TreeNode ("two");

			tv.Nodes.Add (one);
			tv.Nodes.Add (two);

			Assert.IsNull (tv.SelectedNode, "#A1");
			Assert.IsFalse (one.IsSelected, "#A2");
			Assert.IsFalse (two.IsSelected, "#A3");
			Assert.AreEqual (0, afterSelect, "#A4");

			tv.SelectedNode = two;
			Assert.IsNotNull (tv.SelectedNode, "#B1");
			Assert.AreSame (two, tv.SelectedNode, "#B2");
			Assert.IsFalse (one.IsSelected, "#B3");
			Assert.IsFalse (two.IsSelected, "#B4");
			Assert.AreEqual (0, afterSelect, "#B5");

			tv.SelectedNode = null;
			Assert.IsNull (tv.SelectedNode, "#C1");
			Assert.IsFalse (one.IsSelected, "#C2");
			Assert.IsFalse (two.IsSelected, "#C3");
			Assert.AreEqual (0, afterSelect, "#C4");

			tv.SelectedNode = one;
			Assert.IsNotNull (tv.SelectedNode, "#D1");
			Assert.AreSame (one, tv.SelectedNode, "#D2");
			Assert.IsFalse (one.IsSelected, "#D3");
			Assert.IsFalse (two.IsSelected, "#D4");
			Assert.AreEqual (0, afterSelect, "#D5");
		}

		void TreeView_AfterSelect (object sender, TreeViewEventArgs e)
		{
			afterSelect++;
		}

		int afterSelect;
	}

#if NET_2_0
	[TestFixture]
	public class TreeViewNodeSorterTest : TestHelper {
		static bool node_sorter_called;

		[Test]
		public void SortedAfterTreeViewNodeSorterIsSetToSomething() {
			TreeView t = new TreeView();
			t.TreeViewNodeSorter = new NodeSorter();
			Assert.IsTrue(t.Sorted);
		}

		[Test]
		public void SortedAfterTreeViewNodeSorterIsSetToNull() {
			TreeView t = new TreeView();
			t.TreeViewNodeSorter = null;
			Assert.IsFalse(t.Sorted);
		}

		[Test]
		public void NormalTreeViewNodeSorter() {
			TreeView t = new TreeView();
			t.Nodes.Add("2");
			t.Nodes.Add("1");
			node_sorter_called = false;
			t.TreeViewNodeSorter = new NodeSorter();
			Assert.IsTrue(node_sorter_called, "Node sorter called");
			Assert.IsTrue(t.Nodes[0].Text == "2", "Order");
		}

		[Test]
		public void NormalSorted() {
			TreeView t = new TreeView();
			t.Nodes.Add("2");
			t.Nodes.Add("1");
			t.Sorted = true;
			Assert.IsTrue(t.Nodes[0].Text == "1", "Order");
		}

		[Test]
		public void SortedDoesNotSortWhenTreeViewNodeSorterIsSet() {
			TreeView t = new TreeView();
			t.Nodes.Add("2");
			t.Nodes.Add("1");
			t.TreeViewNodeSorter = new NodeSorter();
			t.Sorted = false;
			t.Sorted = true;
			Assert.IsTrue(t.Nodes[0].Text == "2", "Order");
		}

		[Test]
		public void SortedDoesNotSortWhenItIsAlreadyTrue() {
			TreeView t = new TreeView();
			t.Nodes.Add("2");
			t.Nodes.Add("1");
			t.TreeViewNodeSorter = new NodeSorter();
			t.TreeViewNodeSorter = null;
			t.Sorted = true;
			Assert.IsTrue(t.Nodes[0].Text == "2", "Order");
		}

		[Test]
		public void SortedSorts() {
			TreeView t = new TreeView();
			t.Nodes.Add("2");
			t.Nodes.Add("1");
			t.TreeViewNodeSorter = new NodeSorter();
			t.TreeViewNodeSorter = null;
			t.Sorted = false;
			t.Sorted = true;
			Assert.IsTrue(t.Nodes[0].Text == "1", "Order");
		}

		class NodeSorter : IComparer {
			public int Compare(object x, object y) {
				node_sorter_called = true;
				if (x == y)
					return 0;
				return ((TreeNode)x).Text == "2" ? -1 : 1;
			}
		}
	}
#endif
}
