using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;

namespace MonoTests.Monodoc
{
	[TestFixture]
	public class TreeTest
	{
		const string BaseDir = "../../class/monodoc/Test/monodoc_test/";

		[Test]
		public void TestLoadingTree_2_10 ()
		{
			TestTreeLoading ("tree-from-2-10.tree", 0, 2);
		}

		[Test]
		public void TestLoadingTree_3_0_old ()
		{
			TestTreeLoading ("tree-from-3-0-old.tree", 1, 2);
		}

		[Test]
		public void TestLoadingTree_3_0 ()
		{
			TestTreeLoading ("tree-from-3-0.tree", 1, 2);
		}

		void TestTreeLoading (string treeFileName, int expectedVersion, int expectedNodeCount)
		{
			var filePath = Path.Combine (BaseDir, "trees", treeFileName);
			var tree = new Tree (null, filePath);
			Assert.AreEqual (expectedVersion, tree.VersionNumber);
			Assert.IsNotNull (tree.RootNode);
			Assert.AreEqual (expectedNodeCount, tree.RootNode.ChildNodes.Count);
		}
	}
}
