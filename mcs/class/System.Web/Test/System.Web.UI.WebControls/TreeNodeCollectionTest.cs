//
// Tests for System.Web.UI.WebControls.ImageMap.cs
//
// Author:
//  Hagit Yidov (hagity@mainsoft.com
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using System.Threading;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls {
	[TestFixture]
	public class TreeNodeCollectionTest {

		class PokerTreeView : TreeView
		{
			public void DoTrackViewState () {
				TrackViewState ();
			}

			public object DoSaveViewState () {
				return SaveViewState ();
			}

			public void DoLoadViewState (object state) {
				LoadViewState (state);
			}
		}

		[Test]
		public void TreeNodeCollection_DefaultProperties () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			Assert.AreEqual (0, tnc.Count, "Count");
			Assert.AreEqual (false, tnc.IsSynchronized, "IsSynchronized");
		}

		[Test]
		public void TreeNodeCollection_Method_Add () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			Assert.AreEqual (0, tnc.Count, "BeforeAdd");
			tnc.Add (new TreeNode ("TreeNodeName"));
			Assert.AreEqual (1, tnc.Count, "AfterAdd1");
			Assert.AreEqual ("TreeNodeName", tnc[0].Text, "AfterAdd2");
		}

		[Test]
		public void TreeNodeCollection_Method_AddAt () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			tnc.Add (new TreeNode ());
			tnc.Add (new TreeNode ());
			Assert.AreEqual (2, tnc.Count, "BeforeAddAt");
			tnc.AddAt (1, new TreeNode ("TreeNodeName"));
			Assert.AreEqual (3, tnc.Count, "AfterAddAt1");
			Assert.AreEqual ("TreeNodeName", tnc[1].Text, "AfterAdd2");
		}

		[Test]
		public void TreeNodeCollection_Method_Clear () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			tnc.Add (new TreeNode ());
			tnc.Add (new TreeNode ());
			tnc.Add (new TreeNode ());
			Assert.AreEqual (3, tnc.Count, "BeforeClear");
			tnc.Clear ();
			Assert.AreEqual (0, tnc.Count, "AfterClear");
		}

		[Test]
		public void TreeNodeCollection_Method_Contains () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			TreeNode tn = new TreeNode ("TreeNodeName");
			tnc.Add (new TreeNode ());
			Assert.AreEqual (false, tnc.Contains (tn), "BeforeContains");
			tnc.Add (tn);
			tnc.Add (new TreeNode ());
			Assert.AreEqual (true, tnc.Contains (tn), "AfterContains");
		}

		[Test]
		public void TreeNodeCollection_Method_CopyTo () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			TreeNode[] nodeArray = new TreeNode[10];
			tnc.Add (new TreeNode ());
			tnc.Add (new TreeNode ("TreeNodeName"));
			tnc.Add (new TreeNode ());
			Assert.AreEqual (3, tnc.Count, "BeforeCopyTo");
			tnc.CopyTo (nodeArray, 3);
			Assert.AreEqual ("TreeNodeName", nodeArray[4].Text, "AfterCopyTo");
		}

		[Test]
		public void TreeNodeCollection_Method_GetEnumerator () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			for (int i = 0; i < 3; i++)
				tnc.Add (new TreeNode (i.ToString ()));
			IEnumerator nodeEnumerator = tnc.GetEnumerator ();
			int j = 0;
			while (nodeEnumerator.MoveNext ()) {
				Assert.AreEqual (j.ToString (), ((TreeNode) (nodeEnumerator.Current)).Text, "AfterGetEnumerator");
				j++;
			}
		}

		[Test]
		public void TreeNodeCollection_Method_IndexOf () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			TreeNode tn = new TreeNode ("TreeNodeName");
			tnc.Add (new TreeNode ());
			tnc.Add (new TreeNode ());
			Assert.AreEqual (-1, tnc.IndexOf (tn), "BeforeIndexOf");
			tnc.Add (tn);
			tnc.Add (new TreeNode ());
			Assert.AreEqual (2, tnc.IndexOf (tn), "AfterIndexOf");
		}

		[Test]
		public void TreeNodeCollection_Method_Remove () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			TreeNode tn = new TreeNode ("second");
			tnc.Add (new TreeNode ("first"));
			tnc.Add (tn);
			tnc.Add (new TreeNode ("third"));
			Assert.AreEqual (3, tnc.Count, "BeforeRemove1");
			Assert.AreEqual ("second", tnc[1].Text, "BeforeRemove2");
			tnc.Remove (tn);
			Assert.AreEqual (2, tnc.Count, "AfterRemove1");
			Assert.AreEqual ("third", tnc[1].Text, "AfterRemove2");
		}

		[Test]
		public void TreeNodeCollection_Method_RemoveAt () {
			TreeNodeCollection tnc = new TreeNodeCollection ();
			tnc.Add (new TreeNode ("first"));
			tnc.Add (new TreeNode ("second"));
			tnc.Add (new TreeNode ("third"));
			Assert.AreEqual (3, tnc.Count, "BeforeRemoveAt1");
			Assert.AreEqual ("second", tnc[1].Text, "BeforeRemoveAt2");
			tnc.RemoveAt (1);
			Assert.AreEqual (2, tnc.Count, "AfterRemoveAt1");
			Assert.AreEqual ("third", tnc[1].Text, "AfterRemoveAt2");
		}

		[Test]
		public void TreeNodeCollection_ViewState () {
			PokerTreeView orig = new PokerTreeView ();
			orig.DoTrackViewState ();
			BuildTree (orig);

			PokerTreeView copy = new PokerTreeView ();
			copy.DoTrackViewState ();
			object state = orig.DoSaveViewState ();
			copy.DoLoadViewState (state);

			// restored collection that was created after TrackViewState
			Assert.AreEqual (1, copy.Nodes.Count, "TreeNodeCollection_ViewState#1");
			Assert.AreEqual (2, copy.Nodes [0].ChildNodes.Count, "TreeNodeCollection_ViewState#2");
			Assert.AreEqual (0, copy.Nodes [0].ChildNodes [0].ChildNodes.Count, "TreeNodeCollection_ViewState#3");
			Assert.AreEqual ("node1", copy.Nodes [0].ChildNodes [0].Text, "TreeNodeCollection_ViewState#4");
			Assert.AreEqual ("value-node1", copy.Nodes [0].ChildNodes [0].Value, "TreeNodeCollection_ViewState#5");
			Assert.AreEqual (false, copy.Nodes [0].ChildNodes [0].DataBound, "TreeNodeCollection_ViewState#6");
			Assert.AreEqual ("", copy.Nodes [0].ChildNodes [0].DataPath, "TreeNodeCollection_ViewState#7");


			PokerTreeView orig2 = new PokerTreeView ();
			BuildTree (orig2);
			orig2.DoTrackViewState ();

			orig2.Nodes [0].ChildNodes [0].Text = "changed text 1";
			orig2.Nodes [0].ChildNodes [0].Value = "changed value 1";

			PokerTreeView copy2 = new PokerTreeView ();
			BuildTree (copy2);
			copy2.DoTrackViewState ();
			object state2 = orig2.DoSaveViewState ();
			copy2.DoLoadViewState (state2);

			// restored collection that was changed (item's properties only) after TrackViewState
			Assert.AreEqual (1, copy2.Nodes.Count, "TreeNodeCollection_ViewState#8");
			Assert.AreEqual (2, copy2.Nodes [0].ChildNodes.Count, "TreeNodeCollection_ViewState#9");
			Assert.AreEqual (0, copy2.Nodes [0].ChildNodes [0].ChildNodes.Count, "TreeNodeCollection_ViewState#10");
			Assert.AreEqual ("changed text 1", copy2.Nodes [0].ChildNodes [0].Text, "TreeNodeCollection_ViewState#11");
			Assert.AreEqual ("changed value 1", copy2.Nodes [0].ChildNodes [0].Value, "TreeNodeCollection_ViewState#12");
			Assert.AreEqual (false, copy2.Nodes [0].ChildNodes [0].DataBound, "TreeNodeCollection_ViewState#13");
			Assert.AreEqual ("", copy2.Nodes [0].ChildNodes [0].DataPath, "TreeNodeCollection_ViewState#14");


			PokerTreeView orig3 = new PokerTreeView ();
			BuildTree (orig3);
			orig3.DoTrackViewState ();

			orig3.Nodes [0].ChildNodes [0].Text = "changed text 1";
			orig3.Nodes [0].ChildNodes [0].Value = "changed value 1";
			orig3.Nodes [0].ChildNodes.RemoveAt (1);

			PokerTreeView copy3 = new PokerTreeView ();
			BuildTree (copy3);
			copy3.DoTrackViewState ();
			object state3 = orig3.DoSaveViewState ();
			copy3.DoLoadViewState (state3);

			// restored collection that was changed after TrackViewState
			Assert.AreEqual (1, copy3.Nodes.Count, "TreeNodeCollection_ViewState#15");
			Assert.AreEqual (1, copy3.Nodes [0].ChildNodes.Count, "TreeNodeCollection_ViewState#16");
			Assert.AreEqual (0, copy3.Nodes [0].ChildNodes [0].ChildNodes.Count, "TreeNodeCollection_ViewState#17");
			Assert.AreEqual ("changed text 1", copy3.Nodes [0].ChildNodes [0].Text, "TreeNodeCollection_ViewState#18");
			Assert.AreEqual ("changed value 1", copy3.Nodes [0].ChildNodes [0].Value, "TreeNodeCollection_ViewState#19");
			Assert.AreEqual (false, copy3.Nodes [0].ChildNodes [0].DataBound, "TreeNodeCollection_ViewState#20");
			Assert.AreEqual ("", copy3.Nodes [0].ChildNodes [0].DataPath, "TreeNodeCollection_ViewState#21");
		}

		private static void BuildTree (TreeView tv) {
			TreeNode R = new TreeNode ("root", "value-root");
			TreeNode N1 = new TreeNode ("node1", "value-node1");
			TreeNode N2 = new TreeNode ("node2", "value-node2");
			R.ChildNodes.Add (N1);
			R.ChildNodes.Add (N2);
			tv.Nodes.Add (R);
		}
		[Test]
		public void ViewState1 ()
		{
			TreeView m = new TreeView ();
			fillTree (m);

			((IStateManager) m.Nodes).TrackViewState ();
			m.Nodes [0].Text = "root";
			m.Nodes [0].ChildNodes [0].Text = "node";
			m.Nodes [0].ChildNodes [0].ChildNodes [0].Text = "subnode";
			object state = ((IStateManager) m.Nodes).SaveViewState ();

			TreeView copy = new TreeView ();
			fillTree (copy);
			((IStateManager) copy.Nodes).TrackViewState ();
			((IStateManager) copy.Nodes).LoadViewState (state);

			Assert.AreEqual (1, copy.Nodes.Count);
			Assert.AreEqual (2, copy.Nodes [0].ChildNodes.Count);
			Assert.AreEqual (1, copy.Nodes [0].ChildNodes [0].ChildNodes.Count);

			Assert.AreEqual ("root", copy.Nodes [0].Text);
			Assert.AreEqual ("node", copy.Nodes [0].ChildNodes [0].Text);
			Assert.AreEqual ("subnode", copy.Nodes [0].ChildNodes [0].ChildNodes [0].Text);
		}

		[Test]
		public void ViewState2 ()
		{
			TreeView m = new TreeView ();
			fillTree (m);

			((IStateManager) m.Nodes).TrackViewState ();
			m.Nodes [0].Text = "root";
			m.Nodes [0].ChildNodes [0].Text = "node";
			m.Nodes [0].ChildNodes [0].ChildNodes [0].Text = "subnode";
			m.Nodes.Add (new TreeNode ("root 2"));
			object state = ((IStateManager) m.Nodes).SaveViewState ();

			TreeView copy = new TreeView ();
			fillTree (copy);
			((IStateManager) copy.Nodes).TrackViewState ();
			((IStateManager) copy.Nodes).LoadViewState (state);

			Assert.AreEqual (2, copy.Nodes.Count);
			Assert.AreEqual (2, copy.Nodes [0].ChildNodes.Count);
			Assert.AreEqual (1, copy.Nodes [0].ChildNodes [0].ChildNodes.Count);

			Assert.AreEqual ("root", copy.Nodes [0].Text);
			Assert.AreEqual ("node", copy.Nodes [0].ChildNodes [0].Text);
			Assert.AreEqual ("subnode", copy.Nodes [0].ChildNodes [0].ChildNodes [0].Text);
			Assert.AreEqual ("root 2", copy.Nodes [1].Text);
		}

		[Test]
		public void ViewState3 ()
		{
			TreeView m = new TreeView ();
			fillTree (m);
			m.Nodes.Add (new TreeNode ("root 2"));

			((IStateManager) m.Nodes).TrackViewState ();
			m.Nodes [0].Text = "root";
			m.Nodes [0].ChildNodes [0].Text = "node";
			m.Nodes [0].ChildNodes [0].ChildNodes [0].Text = "subnode";
			m.Nodes.RemoveAt (1);
			object state = ((IStateManager) m.Nodes).SaveViewState ();

			TreeView copy = new TreeView ();
			fillTree (copy);
			copy.Nodes.Add (new TreeNode ("root 2"));
			((IStateManager) copy.Nodes).TrackViewState ();
			((IStateManager) copy.Nodes).LoadViewState (state);

			Assert.AreEqual (1, copy.Nodes.Count);
			Assert.AreEqual (2, copy.Nodes [0].ChildNodes.Count);
			Assert.AreEqual (1, copy.Nodes [0].ChildNodes [0].ChildNodes.Count);

			Assert.AreEqual ("root", copy.Nodes [0].Text);
			Assert.AreEqual ("node", copy.Nodes [0].ChildNodes [0].Text);
			Assert.AreEqual ("subnode", copy.Nodes [0].ChildNodes [0].ChildNodes [0].Text);
		}

		[Test]
		public void ViewState4 ()
		{
			TreeView m = new TreeView ();
			fillTree (m);
			m.Nodes.Add (new TreeNode ("root 2"));
			m.Nodes [0].ChildNodes.RemoveAt (1);

			((IStateManager) m.Nodes).TrackViewState ();
			m.Nodes [0].Text = "root";
			m.Nodes [0].ChildNodes [0].Text = "node";
			m.Nodes [0].ChildNodes [0].ChildNodes [0].Text = "subnode";
			object state = ((IStateManager) m.Nodes).SaveViewState ();

			TreeView copy = new TreeView ();
			fillTree (copy);
			copy.Nodes.Add (new TreeNode ("root 2"));
			copy.Nodes [0].ChildNodes.RemoveAt (1);
			((IStateManager) copy.Nodes).TrackViewState ();
			((IStateManager) copy.Nodes).LoadViewState (state);

			Assert.AreEqual (2, copy.Nodes.Count);
			Assert.AreEqual (1, copy.Nodes [0].ChildNodes.Count);
			Assert.AreEqual (1, copy.Nodes [0].ChildNodes [0].ChildNodes.Count);

			Assert.AreEqual ("root", copy.Nodes [0].Text);
			Assert.AreEqual ("node", copy.Nodes [0].ChildNodes [0].Text);
			Assert.AreEqual ("subnode", copy.Nodes [0].ChildNodes [0].ChildNodes [0].Text);
		}

		[Test]
		public void ViewState5 ()
		{
			TreeView m = new TreeView ();
			((IStateManager) m.Nodes).TrackViewState ();
			fillTree (m);

			object state = ((IStateManager) m.Nodes).SaveViewState ();

			TreeView copy = new TreeView ();
			((IStateManager) copy.Nodes).TrackViewState ();
			((IStateManager) copy.Nodes).LoadViewState (state);

			Assert.AreEqual (1, copy.Nodes.Count);
			Assert.AreEqual (2, copy.Nodes [0].ChildNodes.Count);
			Assert.AreEqual (1, copy.Nodes [0].ChildNodes [0].ChildNodes.Count);
		}

		private static void fillTree (TreeView tv)
		{
			tv.Nodes.Clear ();
			tv.Nodes.Add (new TreeNode ());
			tv.Nodes [0].ChildNodes.Add (new TreeNode ());
			tv.Nodes [0].ChildNodes.Add (new TreeNode ());
			tv.Nodes [0].ChildNodes [0].ChildNodes.Add (new TreeNode ());
		}

	}
}


#endif
