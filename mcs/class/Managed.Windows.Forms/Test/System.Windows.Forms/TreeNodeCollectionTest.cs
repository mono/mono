using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TreeNodeCollectionTest : TestHelper
	{

        [Test]
        public void Add()
        {
            ImageList imglist = null;

            TestHelper.RemoveWarning (imglist);
	    
            TreeView tv = new TreeView();
            tv.Nodes.Add("text");
            Assert.AreEqual(1, tv.Nodes.Count, "#A1");
            Assert.AreEqual("text", tv.Nodes[0].Text, "#A2");
#if NET_2_0
            Assert.AreEqual("", tv.Nodes[0].Name, "#A3");
#endif
            tv.Nodes.Clear();

            tv.Nodes.Add(new TreeNode());
            Assert.AreEqual("", tv.Nodes[0].Text, "#B1");
#if NET_2_0
            Assert.AreEqual("", tv.Nodes[0].Name, "#B2");
#endif

#if NET_2_0
            tv.Nodes.Clear();

            tv.Nodes.Add("key", "text");
            Assert.AreEqual("text", tv.Nodes[0].Text, "#C1");
            Assert.AreEqual("key", tv.Nodes[0].Name, "#C2");
#endif

#if NET_2_0
            tv.Nodes.Clear();
            imglist = new ImageList();
            imglist.Images.Add("image key", new Bitmap(16, 16));

            tv.Nodes.Add("key", "text", 0);
            Assert.AreEqual("text", tv.Nodes[0].Text, "#D1");
            Assert.AreEqual("key", tv.Nodes[0].Name, "#D2");
            Assert.AreEqual(0, tv.Nodes[0].ImageIndex, "#D3");
            Assert.AreEqual("", tv.Nodes[0].ImageKey, "#D4");
#endif

#if NET_2_0
            tv.Nodes.Clear();
            imglist = new ImageList();
            imglist.Images.Add("image key", new Bitmap(16, 16));

            tv.Nodes.Add("key", "text", "image key");
            Assert.AreEqual("text", tv.Nodes[0].Text, "#E1");
            Assert.AreEqual("key", tv.Nodes[0].Name, "#E2");
            Assert.AreEqual(-1, tv.Nodes[0].ImageIndex, "#E3");
            Assert.AreEqual("image key", tv.Nodes[0].ImageKey, "#E4");
#endif

#if NET_2_0
            tv.Nodes.Clear();
            imglist = new ImageList();
            imglist.Images.Add("image key", new Bitmap(16, 16));
            imglist.Images.Add("selected image key", new Bitmap(16, 16));

            tv.Nodes.Add("key", "text", 0, 1);
            Assert.AreEqual("text", tv.Nodes[0].Text, "#F1");
            Assert.AreEqual("key", tv.Nodes[0].Name, "#F2");
            Assert.AreEqual(0, tv.Nodes[0].ImageIndex, "#F3");
            Assert.AreEqual("", tv.Nodes[0].ImageKey, "#F4");
            Assert.AreEqual(1, tv.Nodes[0].SelectedImageIndex, "#F5");
            Assert.AreEqual("", tv.Nodes[0].SelectedImageKey, "#F6");
#endif

#if NET_2_0
            tv.Nodes.Clear();
            imglist = new ImageList();
            imglist.Images.Add("image key", new Bitmap(16, 16));
            imglist.Images.Add("selected image key", new Bitmap(16, 16));

            tv.Nodes.Add("key", "text", "image key", "selected image key");
            Assert.AreEqual("text", tv.Nodes[0].Text, "#G1");
            Assert.AreEqual("key", tv.Nodes[0].Name, "#G2");
            Assert.AreEqual(-1, tv.Nodes[0].ImageIndex, "#G3");
            Assert.AreEqual("image key", tv.Nodes[0].ImageKey, "#G4");
            Assert.AreEqual(-1, tv.Nodes[0].SelectedImageIndex, "#G5");
            Assert.AreEqual("selected image key", tv.Nodes[0].SelectedImageKey, "#G6");
#endif

#if NET_2_0
            tv.Nodes.Clear();
            tv.Nodes.Add("key", "text");
            tv.Nodes.Add("key", "text 2");
            Assert.AreEqual(2, tv.Nodes.Count, "#H1");
            Assert.AreEqual("text", tv.Nodes[0].Text, "#H1");
            Assert.AreEqual("key", tv.Nodes[0].Name, "#H2");
            Assert.AreEqual("text 2", tv.Nodes[1].Text, "#H1");
            Assert.AreEqual("key", tv.Nodes[1].Name, "#H2");

            Assert.IsTrue(tv.Nodes.ContainsKey("key"), "#I1");
            Assert.AreEqual(0, tv.Nodes.IndexOfKey("key"), "#I2");
#endif
        }

		[Test]
		public void Add_Child_Null ()
		{
			TreeView tv = new TreeView ();
			try {
				tv.Nodes.Add ((TreeNode) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("node", ex.ParamName, "#6");
			}
		}

		// This is related to bug #601766, where basically we are reproducing a buggy behaviour:
		// When adding a node don't remove it from any previous collection.
		[Test]
		public void Add_Other_Collection ()
		{
			TreeView tv = new TreeView ();
			TreeNode root = tv.Nodes.Add ("Root 1");
			TreeNode nodeA = root.Nodes.Add ("A");
			TreeNode nodeB = root.Nodes.Add ("B");
			TreeNode nodeC = root.Nodes.Add ("C");

			Assert.AreEqual (1, tv.Nodes.Count, "#A0");
			Assert.AreEqual (3, root.Nodes.Count, "#A1");

			TreeView tv2 = new TreeView ();
			TreeNode root2 = tv2.Nodes.Add ("Root 2");
			root2.Nodes.Add (nodeA);

			Assert.AreEqual (1, tv.Nodes.Count, "#B0");
			Assert.AreEqual (3, root.Nodes.Count, "#B1");
			Assert.AreEqual (1, tv2.Nodes.Count, "#B2");
			Assert.AreEqual (1, root2.Nodes.Count, "#B3");

			// The next ones *could* be a little tricky, since even if we are not
			// removing the element from the previous collection,
			// parent should be set correctly.
			Assert.AreEqual (root2, nodeA.Parent, "#B4");
			Assert.AreEqual (root, nodeB.Parent, "#B5");
			Assert.AreEqual (root, nodeC.Parent, "#B6");
		}

		[Test]
		public void AddRange ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = new TreeNode ("A");
			TreeNode nodeB = new TreeNode ("B");
			TreeNode nodeC = new TreeNode ("C");
			TreeNode nodeD = new TreeNode ("D");
			tv.Nodes.AddRange (new TreeNode [] { nodeA, nodeB });
			Assert.AreEqual (2, tv.Nodes.Count, "#A1");
			Assert.AreSame (nodeA, tv.Nodes [0], "#A2");
			Assert.AreSame (nodeB, tv.Nodes [1], "#A3");
			tv.Nodes.AddRange (new TreeNode [] { nodeC });
			Assert.AreEqual (3, tv.Nodes.Count, "#B1");
			Assert.AreSame (nodeC, tv.Nodes [2], "#B2");
		}

		[Test]
		public void AddRange_Node_Null ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = new TreeNode ("A");
			TreeNode nodeB = new TreeNode ("B");
			try {
				tv.Nodes.AddRange (new TreeNode [] { nodeA, null, nodeB });
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("node", ex.ParamName, "#6");
			}
		}

		[Test]
		public void AddRange_Nodes_Null ()
		{
			TreeView tv = new TreeView ();
			try {
				tv.Nodes.AddRange ((TreeNode []) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("nodes", ex.ParamName, "#6");
			}
		}

		[Test]
		public void Remove ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = tv.Nodes.Add ("A");
			TreeNode nodeB = tv.Nodes.Add ("B");
			TreeNode nodeC = tv.Nodes.Add ("C");
			Assert.AreEqual (3, tv.Nodes.Count, "#A1");
			Assert.AreSame (nodeA, tv.Nodes [0], "#A2");
			Assert.AreSame (nodeB, tv.Nodes [1], "#A3");
			Assert.AreSame (nodeC, tv.Nodes [2], "#A3");
			tv.Nodes.Remove (nodeB);
			Assert.AreEqual (2, tv.Nodes.Count, "#B1");
			Assert.AreSame (nodeA, tv.Nodes [0], "#B2");
			Assert.AreSame (nodeC, tv.Nodes [1], "#B3");
			tv.Nodes.Remove (nodeA);
			Assert.AreEqual (1, tv.Nodes.Count, "#C1");
			Assert.AreSame (nodeC, tv.Nodes [0], "#C2");
			tv.Nodes.Remove (nodeC);
			Assert.AreEqual (0, tv.Nodes.Count, "#D1");
		}

		[Test]
		public void Remove_NotInCollection ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = tv.Nodes.Add ("A");
			tv.Nodes.Remove (nodeA);
			tv.Nodes.Remove (nodeA);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void Remove_Null ()
		{
			TreeView tv = new TreeView ();
			tv.Nodes.Remove (null);
		}

		[Test]
		public void Enumerator_Reset ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = tv.Nodes.Add ("A");
			IEnumerator enumerator = tv.Nodes.GetEnumerator ();
			Assert.IsNull (enumerator.Current, "#A1");
			Assert.IsTrue (enumerator.MoveNext (), "#A2");
			Assert.IsNotNull (enumerator.Current, "#A3");
			Assert.AreSame (nodeA, enumerator.Current, "#A4");
			Assert.IsFalse (enumerator.MoveNext (), "#A5");
			enumerator.Reset ();
			Assert.IsNull (enumerator.Current, "#B1");
			Assert.IsTrue (enumerator.MoveNext (), "#B2");
			Assert.IsNotNull (enumerator.Current, "#B3");
			Assert.AreSame (nodeA, enumerator.Current, "#B4");
			Assert.IsFalse (enumerator.MoveNext (), "#B5");
		}

		[Test]
		public void Enumerator_MoveNext ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = tv.Nodes.Add ("A");
			
			TestHelper.RemoveWarning (nodeA);
			
			IEnumerator enumerator = tv.Nodes.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#A1");
			Assert.IsFalse (enumerator.MoveNext (), "#A2");
			Assert.IsFalse (enumerator.MoveNext (), "#A3");

			tv = new TreeView ();
			enumerator = tv.Nodes.GetEnumerator ();
			Assert.IsFalse (enumerator.MoveNext (), "#B1");
			Assert.IsFalse (enumerator.MoveNext (), "#B2");
		}

		[Test]
		public void Enumerator_Current ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = tv.Nodes.Add ("A");
			TreeNode nodeB = tv.Nodes.Add ("B");
			IEnumerator enumerator = tv.Nodes.GetEnumerator ();
			Assert.IsNull (enumerator.Current, "#A1");
			enumerator.MoveNext ();
			Assert.IsNotNull (enumerator.Current, "#A2");
			Assert.AreSame (nodeA, enumerator.Current, "#A3");
			enumerator.MoveNext ();
			Assert.IsNotNull (enumerator.Current, "#A4");
			Assert.AreSame (nodeB, enumerator.Current, "#A5");
			enumerator.MoveNext ();
			Assert.IsNotNull (enumerator.Current, "#A6");
			Assert.AreSame (nodeB, enumerator.Current, "#A7");

			tv = new TreeView ();
			enumerator = tv.Nodes.GetEnumerator ();
			Assert.IsNull (enumerator.Current, "#B1");
			enumerator.MoveNext ();
			Assert.IsNull (enumerator.Current, "#B2");
		}

		[Test]
		public void Insert_Sorted ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeB = tv.Nodes.Add ("B");
			TreeNode nodeA = tv.Nodes.Add ("A");
			TreeNode nodeF = tv.Nodes.Add ("F");
			tv.Sorted = true;

			Assert.AreEqual (3, tv.Nodes.Count, "#A0");
			Assert.AreEqual (true, tv.Sorted, "#A1");
			Assert.AreEqual (nodeA, tv.Nodes [0], "#A2");
			Assert.AreEqual (nodeB, tv.Nodes [1], "#A3");
			Assert.AreEqual (nodeF, tv.Nodes [2], "#A4");

			TreeNode nodeZ = new TreeNode ("Z");
			tv.Nodes.Insert (1, nodeZ);

			Assert.AreEqual (4, tv.Nodes.Count, "#B0");
			Assert.AreEqual (true, tv.Sorted, "#B1");
			Assert.AreEqual (nodeA, tv.Nodes [0], "#B2");
			Assert.AreEqual (nodeB, tv.Nodes [1], "#B3");
			Assert.AreEqual (nodeF, tv.Nodes [2], "#B4");
			Assert.AreEqual (nodeZ, tv.Nodes [3], "#B5");
		}

		// It seems we are not getting sorted when using our indexer.
		[Test]
		public void Indexer_Sorted ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeB = tv.Nodes.Add ("B");
			TreeNode nodeA = tv.Nodes.Add ("A");
			TreeNode nodeF = tv.Nodes.Add ("F");
			tv.Sorted = true;

			Assert.AreEqual (3, tv.Nodes.Count, "#A0");
			Assert.AreEqual (true, tv.Sorted, "#A1");
			Assert.AreEqual (nodeA, tv.Nodes [0], "#A2");
			Assert.AreEqual (nodeB, tv.Nodes [1], "#A3");
			Assert.AreEqual (nodeF, tv.Nodes [2], "#A4");

			TreeNode nodeZ = new TreeNode ("Z");
			tv.Nodes [1] = nodeZ;

			Assert.AreEqual (3, tv.Nodes.Count, "#B0");
			Assert.AreEqual (true, tv.Sorted, "#B1");
			Assert.AreEqual (nodeA, tv.Nodes [0], "#B2");
			Assert.AreEqual (nodeZ, tv.Nodes [1], "#B3");
			Assert.AreEqual (nodeF, tv.Nodes [2], "#B4");
		}

		[Test]
		public void IList_Indexer_Get ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = tv.Nodes.Add ("A");
			TreeNode nodeB = tv.Nodes.Add ("B");
			TreeNode nodeC = tv.Nodes.Add ("C");

			IList list = (IList) tv.Nodes;

			Assert.AreSame (nodeA, list [0], "#A1");
			Assert.AreSame (nodeB, list [1], "#A2");
			Assert.AreSame (nodeC, list [2], "#A3");

			try {
				object item = list [3];
				Assert.Fail ("#B1: " + item);
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.ActualValue, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
				Assert.AreEqual ("index", ex.ParamName, "#B5");
			}

			try {
				object item = list [-1];
				Assert.Fail ("#C1: " + item);
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.ActualValue, "#C3");
				Assert.IsNull (ex.InnerException, "#C4");
				Assert.AreEqual ("index", ex.ParamName, "#C5");
			}
		}

		[Test]
		public void IList_Indexer_Set ()
		{
			TreeView tv = new TreeView ();
			TreeNode nodeA = tv.Nodes.Add ("A");

			IList list = (IList) tv.Nodes;
			TreeNode nodeB = new TreeNode ("B");
			list [0] = nodeB;
			Assert.AreSame (nodeB, list [0], "#A1");

			try {
				list [1] = nodeA;
				Assert.Fail ("#B1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNull (ex.ActualValue, "#B3");
				Assert.IsNull (ex.InnerException, "#B4");
#if NET_2_0
				Assert.AreEqual ("index", ex.ParamName, "#B5");
#endif
			}

			try {
				list [-1] = nodeA;
				Assert.Fail ("#C1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#C2");
				Assert.IsNull (ex.ActualValue, "#C3");
				Assert.IsNull (ex.InnerException, "#C4");
#if NET_2_0
				Assert.AreEqual ("index", ex.ParamName, "#C5");
#endif
			}

			try {
				list [0] = "whatever";
				Assert.Fail ("#D1");
			} catch (ArgumentException ex) {
				// Parameter must be of type TreeNode
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
#if NET_2_0
				Assert.AreEqual ("value", ex.ParamName, "#D4");
#endif
			}

			try {
				list [0] = null;
				Assert.Fail ("#E1");
			} catch (ArgumentException ex) {
				// Parameter must be of type TreeNode
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#E2");
				Assert.IsNull (ex.InnerException, "#E3");
#if NET_2_0
				Assert.AreEqual ("value", ex.ParamName, "#E4");
#endif
			}
		}

#if NET_2_0
		[Test]
		public void ContainsKey ()
		{
			TreeView tv = new TreeView ();

			for (int i = 0; i < 10; i++) {
				TreeNode node = new TreeNode ();
				node.Name = i.ToString ();
				tv.Nodes.Add (node);
			}

			for (int i = 0; i < 10; i++)
				Assert.IsTrue (tv.Nodes.ContainsKey (i.ToString ()), "#CK1");
				
			Assert.AreEqual (false, tv.Nodes.ContainsKey ("42"), "#CK2");
		}

		[Test]
		public void IndexOfKey ()
		{
			TreeView tv = new TreeView ();

			for (int i = 0; i < 10; i++) {
				TreeNode node = new TreeNode ();
				node.Name = i.ToString ();
				tv.Nodes.Add (node);
			}

			for (int i = 0; i < 10; i++)
				Assert.AreEqual (tv.Nodes.IndexOfKey (i.ToString ()), i, "#KI1");

			Assert.AreEqual (-1, tv.Nodes.IndexOfKey ("42"), "#KI2");
		}
#endif

		[Test]
		public void Bug408999 ()
		{
			TreeView tv = new TreeView ();

			for (int i = 0; i < 100; i++)
				tv.Nodes.Add (i.ToString ());

			tv.Nodes.RemoveAt (0);
		}

		[Test]
		public void AddedSortedNodeIndex ()
		{
			TreeView tv = new TreeView ();
			tv.Sorted = true;

			string[] nodeNames = new string[] { "Hello", "this", "is", "a", "test" };
			int[] nodeIndexes = new int[] { 0, 1, 1, 0, 3 };

			for (int i = 0; i < nodeNames.Length; i++) {
				TreeNode node = new TreeNode (nodeNames [i]);
				int nodeIndex = tv.Nodes.Add (node);
				Assert.AreEqual (nodeIndexes [i], nodeIndex);
			}
		}
	}
}
