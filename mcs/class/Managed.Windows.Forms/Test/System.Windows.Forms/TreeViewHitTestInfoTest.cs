//
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//     Jackson Harper (jackson@ximian.com)
//

using System;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.ComponentModel;
using System.Runtime.Remoting;

using NUnit.Framework;

#if NET_2_0
namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class TreeViewHitTestInfoTest : TestHelper {

		[Test]
		public void TestCtor ()
		{
			TreeViewHitTestInfo t = new TreeViewHitTestInfo (null, TreeViewHitTestLocations.None);

			Assert.AreEqual (t.Node, null, "null-1");
			Assert.AreEqual (t.Location, TreeViewHitTestLocations.None, "null-2");

			t = new TreeViewHitTestInfo (null, TreeViewHitTestLocations.Image);

			Assert.AreEqual (t.Node, null, "loc-1");
			Assert.AreEqual (t.Location, TreeViewHitTestLocations.Image, "loc-2");

			TreeNode tn = new TreeNode ("test");
			t = new TreeViewHitTestInfo (tn, TreeViewHitTestLocations.PlusMinus);

			Assert.AreEqual (t.Node, tn, "node-1");
			Assert.AreEqual (t.Location, TreeViewHitTestLocations.PlusMinus);
		}

		[Test]
		public void TestBadLocation ()
		{
			TreeViewHitTestInfo t = new TreeViewHitTestInfo (null, (TreeViewHitTestLocations) (-1));

			Assert.AreEqual (t.Node, null, "bad-loc-1");
			Assert.AreEqual ((int) t.Location, -1, "bad-loc-2");
		}
	}

}
#endif
