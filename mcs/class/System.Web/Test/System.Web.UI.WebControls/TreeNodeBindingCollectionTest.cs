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
	public class TreeNodeBindingCollectionTest {

		[Test]
		public void TreeNodeBindingCollection_Method_Add () {
			TreeView tv = new TreeView ();
			Assert.AreEqual (0, tv.DataBindings.Count, "BeforeAdd");
			TreeNodeBinding tnb = new TreeNodeBinding ();
			tnb.DataMember = "TreeNodeBinding";
			tv.DataBindings.Add (tnb);
			Assert.AreEqual (1, tv.DataBindings.Count, "AfterAdd1");
			Assert.AreEqual ("TreeNodeBinding", tv.DataBindings[0].DataMember, "AfterAdd2");
		}

		[Test]
		public void TreeNodeBindingCollection_Method_Clear () {
			TreeView tv = new TreeView ();
			tv.DataBindings.Add (new TreeNodeBinding ());
			tv.DataBindings.Add (new TreeNodeBinding ());
			tv.DataBindings.Add (new TreeNodeBinding ());
			Assert.AreEqual (3, tv.DataBindings.Count, "BeforeClear");
			tv.DataBindings.Clear ();
			Assert.AreEqual (0, tv.DataBindings.Count, "AfterClear");
		}

		[Test]
		public void TreeNodeBindingCollection_Method_Contains () {
			TreeView tv = new TreeView ();
			TreeNodeBinding tnb = new TreeNodeBinding ();
			tv.DataBindings.Add (new TreeNodeBinding ());
			Assert.AreEqual (false, tv.DataBindings.Contains (tnb), "BeforeContains");
			tv.DataBindings.Add (tnb);
			tv.DataBindings.Add (new TreeNodeBinding ());
			Assert.AreEqual (true, tv.DataBindings.Contains (tnb), "AfterContains");
		}

		[Test]
		public void TreeNodeBindingCollection_Method_CopyTo () {
			TreeView tv = new TreeView ();
			TreeNodeBinding[] bindingArray = new TreeNodeBinding[10];
			tv.DataBindings.Add (new TreeNodeBinding ());
			TreeNodeBinding tnb = new TreeNodeBinding ();
			tnb.DataMember = "TreeNodeBinding";
			tv.DataBindings.Add (tnb);
			tv.DataBindings.Add (new TreeNodeBinding ());
			Assert.AreEqual (3, tv.DataBindings.Count, "BeforeCopyTo");
			tv.DataBindings.CopyTo (bindingArray, 3);
			Assert.AreEqual ("TreeNodeBinding", bindingArray[4].DataMember, "AfterCopyTo");
		}

		[Test]
		public void TreeNodeBindingCollection_Method_IndexOf () {
			TreeView tv = new TreeView ();
			TreeNodeBinding tnb = new TreeNodeBinding ();
			tv.DataBindings.Add (new TreeNodeBinding ());
			tv.DataBindings.Add (new TreeNodeBinding ());
			Assert.AreEqual (-1, tv.DataBindings.IndexOf (tnb), "BeforeIndexOf");
			tv.DataBindings.Add (tnb);
			tv.DataBindings.Add (new TreeNodeBinding ());
			Assert.AreEqual (2, tv.DataBindings.IndexOf (tnb), "AfterIndexOf");
		}

		[Test]
		public void TreeNodeBindingCollection_Method_Insert () {
			TreeView tv = new TreeView ();
			tv.DataBindings.Add (new TreeNodeBinding ());
			tv.DataBindings.Add (new TreeNodeBinding ());
			Assert.AreEqual (2, tv.DataBindings.Count, "BeforeInsert");
			TreeNodeBinding tnb = new TreeNodeBinding ();
			tnb.DataMember = "TreeNodeBinding";
			tv.DataBindings.Insert (1, tnb);
			Assert.AreEqual (3, tv.DataBindings.Count, "AfterInsert1");
			Assert.AreEqual ("TreeNodeBinding", tv.DataBindings[1].DataMember, "AfterInsert2");
		}

		[Test]
		public void TreeNodeBindingCollection_Method_Remove () {
			TreeView tv = new TreeView ();
			TreeNodeBinding tnb1 = new TreeNodeBinding ();
			tnb1.DataMember = "first";
			TreeNodeBinding tnb2 = new TreeNodeBinding ();
			tnb2.DataMember = "second";
			TreeNodeBinding tnb3 = new TreeNodeBinding ();
			tnb3.DataMember = "third";
			tv.DataBindings.Add (tnb1);
			tv.DataBindings.Add (tnb2);
			tv.DataBindings.Add (tnb3);
			Assert.AreEqual (3, tv.DataBindings.Count, "BeforeRemove1");
			Assert.AreEqual ("second", tv.DataBindings[1].DataMember, "BeforeRemove2");
			tv.DataBindings.Remove (tnb2);
			Assert.AreEqual (2, tv.DataBindings.Count, "AfterRemove1");
			Assert.AreEqual ("third", tv.DataBindings[1].DataMember, "AfterRemove2");
		}

		[Test]
		public void TreeNodeBindingCollection_Method_RemoveAt () {
			TreeView tv = new TreeView ();
			TreeNodeBinding tnb1 = new TreeNodeBinding ();
			tnb1.DataMember = "first";
			TreeNodeBinding tnb2 = new TreeNodeBinding ();
			tnb2.DataMember = "second";
			TreeNodeBinding tnb3 = new TreeNodeBinding ();
			tnb3.DataMember = "third";
			tv.DataBindings.Add (tnb1);
			tv.DataBindings.Add (tnb2);
			tv.DataBindings.Add (tnb3);
			Assert.AreEqual (3, tv.DataBindings.Count, "BeforeRemove1");
			Assert.AreEqual ("second", tv.DataBindings[1].DataMember, "BeforeRemove2");
			tv.DataBindings.RemoveAt (1);
			Assert.AreEqual (2, tv.DataBindings.Count, "AfterRemove1");
			Assert.AreEqual ("third", tv.DataBindings[1].DataMember, "AfterRemove2");
		}
	}
}


#endif
