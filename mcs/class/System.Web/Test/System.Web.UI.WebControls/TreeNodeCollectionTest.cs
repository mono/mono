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
	}
}


#endif
