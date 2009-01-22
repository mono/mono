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
using System.Drawing;

namespace MonoTests.System.Web.UI.WebControls {
	[TestFixture]
	public class TreeNodeStyleCollectionTest {

		[Test]
		public void TreeNodeStyleCollection_Method_Add () {
			TreeView tv = new TreeView ();
			Assert.AreEqual (0, tv.LevelStyles.Count, "BeforeAdd");
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (1, tv.LevelStyles.Count, "AfterAdd");
		}

		[Test]
		public void TreeNodeStyleCollection_Method_Clear () {
			TreeView tv = new TreeView ();
			tv.LevelStyles.Add (new TreeNodeStyle ());
			tv.LevelStyles.Add (new TreeNodeStyle ());
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (3, tv.LevelStyles.Count, "BeforeClear");
			tv.LevelStyles.Clear ();
			Assert.AreEqual (0, tv.LevelStyles.Count, "AfterClear");
		}

		[Test]
		public void TreeNodeStyleCollection_Method_Contains () {
			TreeView tv = new TreeView ();
			TreeNodeStyle tns = new TreeNodeStyle ();
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (false, tv.LevelStyles.Contains (tns), "BeforeContains");
			tv.LevelStyles.Add (tns);
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (true, tv.LevelStyles.Contains (tns), "AfterContains");
		}

		[Test]
		public void TreeNodeStyleCollection_Method_CopyTo () {
			TreeView tv = new TreeView ();
			TreeNodeStyle[] styleArray = new TreeNodeStyle[10];
			tv.LevelStyles.Add (new TreeNodeStyle ());
			TreeNodeStyle tns = new TreeNodeStyle ();
			tns.ImageUrl = "StyleImageUrl";
			tv.LevelStyles.Add (tns);
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (3, tv.LevelStyles.Count, "BeforeCopyTo");
			tv.LevelStyles.CopyTo (styleArray, 3);
			Assert.AreEqual ("StyleImageUrl", styleArray[4].ImageUrl, "AfterCopyTo");
		}

		[Test]
		public void TreeNodeStyleCollection_Method_IndexOf () {
			TreeView tv = new TreeView ();
			TreeNodeStyle tns = new TreeNodeStyle ();
			tv.LevelStyles.Add (new TreeNodeStyle ());
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (-1, tv.LevelStyles.IndexOf (tns), "BeforeIndexOf");
			tv.LevelStyles.Add (tns);
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (2, tv.LevelStyles.IndexOf (tns), "AfterIndexOf");
		}

		[Test]
		public void TreeNodeStyleCollection_Method_Insert () {
			TreeView tv = new TreeView ();
			tv.LevelStyles.Add (new TreeNodeStyle ());
			tv.LevelStyles.Add (new TreeNodeStyle ());
			Assert.AreEqual (2, tv.LevelStyles.Count, "BeforeInsert");
			TreeNodeStyle tns = new TreeNodeStyle ();
			tns.ImageUrl = "StyleImageUrl";
			tv.LevelStyles.Insert (1, tns);
			Assert.AreEqual (3, tv.LevelStyles.Count, "AfterInsert1");
			Assert.AreEqual ("StyleImageUrl", tv.LevelStyles[1].ImageUrl, "AfterInsert2");
		}

		[Test]
		public void TreeNodeStyleCollection_Method_Remove () {
			TreeView tv = new TreeView ();
			TreeNodeStyle tns1 = new TreeNodeStyle ();
			tns1.ImageUrl = "first";
			TreeNodeStyle tns2 = new TreeNodeStyle ();
			tns2.ImageUrl = "second";
			TreeNodeStyle tns3 = new TreeNodeStyle ();
			tns3.ImageUrl = "third";
			tv.LevelStyles.Add (tns1);
			tv.LevelStyles.Add (tns2);
			tv.LevelStyles.Add (tns3);
			Assert.AreEqual (3, tv.LevelStyles.Count, "BeforeRemove1");
			Assert.AreEqual ("second", tv.LevelStyles[1].ImageUrl, "BeforeRemove2");
			tv.LevelStyles.Remove (tns2);
			Assert.AreEqual (2, tv.LevelStyles.Count, "AfterRemove1");
			Assert.AreEqual ("third", tv.LevelStyles[1].ImageUrl, "AfterRemove2");
		}

		[Test]
		public void TreeNodeStyleCollection_Method_RemoveAt () {
			TreeView tv = new TreeView ();
			TreeNodeStyle tns1 = new TreeNodeStyle ();
			tns1.ImageUrl = "first";
			TreeNodeStyle tns2 = new TreeNodeStyle ();
			tns2.ImageUrl = "second";
			TreeNodeStyle tns3 = new TreeNodeStyle ();
			tns3.ImageUrl = "third";
			tv.LevelStyles.Add (tns1);
			tv.LevelStyles.Add (tns2);
			tv.LevelStyles.Add (tns3);
			Assert.AreEqual (3, tv.LevelStyles.Count, "BeforeRemove1");
			Assert.AreEqual ("second", tv.LevelStyles[1].ImageUrl, "BeforeRemove2");
			tv.LevelStyles.RemoveAt (1);
			Assert.AreEqual (2, tv.LevelStyles.Count, "AfterRemove1");
			Assert.AreEqual ("third", tv.LevelStyles[1].ImageUrl, "AfterRemove2");
		}
		
		[Test]
		public void TreeNodeStyleCollection_ViewState () {
			TreeView tv = new TreeView ();
			((IStateManager) tv.LevelStyles).TrackViewState ();

			TreeNodeStyle style = new TreeNodeStyle ();
			tv.LevelStyles.Add (style);
			Assert.AreEqual (false, style.IsEmpty, "TreeNodeStyleCollection_ViewState#1");

			tv.LevelStyles.Remove (style);
			Assert.AreEqual (false, style.IsEmpty, "TreeNodeStyleCollection_ViewState#2");

			tv.LevelStyles.Add (style);
			tv.LevelStyles.Add (new TreeNodeStyle ());
			tv.LevelStyles [1].BackColor = Color.Blue;

			object state = ((IStateManager) tv.LevelStyles).SaveViewState ();
			TreeView copy = new TreeView ();
			((IStateManager) copy.LevelStyles).TrackViewState ();
			((IStateManager) copy.LevelStyles).LoadViewState (state);

			Assert.AreEqual (2, copy.LevelStyles.Count, "TreeNodeStyleCollection_ViewState#3");
			Assert.AreEqual (false, copy.LevelStyles [0].IsEmpty, "TreeNodeStyleCollection_ViewState#4");
			Assert.AreEqual (false, copy.LevelStyles [1].IsEmpty, "TreeNodeStyleCollection_ViewState#5");
			Assert.AreEqual (Color.Blue, copy.LevelStyles [1].BackColor, "TreeNodeStyleCollection_ViewState#6");
		}
	}
}


#endif
