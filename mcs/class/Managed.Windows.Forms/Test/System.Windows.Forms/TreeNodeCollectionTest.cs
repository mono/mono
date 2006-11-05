using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TreeNodeCollectionTest
	{
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
#if ONLY_1_1
		[ExpectedException (typeof (NullReferenceException))]
#endif
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
	}
}
