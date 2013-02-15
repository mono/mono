using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;

namespace MonoTests.Monodoc
{
	[TestFixture]
	public class HelpSourceTest
	{
		const string BaseDir = "../../class/monodoc/Test/monodoc_test/";

		class CheckGenerator : IDocGenerator<bool>
		{
			public string LastCheckMessage { get; set; }

			public bool Generate (HelpSource hs, string id, Dictionary<string, string> context)
			{
				LastCheckMessage = string.Format ("#1 : {0} {1}", hs, id);
				if (hs == null || string.IsNullOrEmpty (id))
					return false;

				// Stripe the arguments parts since we don't need it
				var argIdx = id.LastIndexOf ('?');
				if (argIdx != -1)
					id = id.Substring (0, argIdx);

				LastCheckMessage = string.Format ("#2 : {0} {1}", hs, id);
				if (hs.IsRawContent (id))
					return hs.GetText (id) != null;

				IEnumerable<string> parts;
				if (hs.IsMultiPart (id, out parts)) {
					LastCheckMessage = string.Format ("#4 : {0} {1} ({2})", hs, id, string.Join (", ", parts));
					foreach (var partId in parts)
						if (!Generate (hs, partId, context))
							return false;
				}

				LastCheckMessage = string.Format ("#3 : {0} {1}", hs, id);
				if (hs.IsGeneratedContent (id))
					return hs.GetCachedText (id) != null;
				else {
					var s = hs.GetCachedHelpStream (id);
					if (s != null) {
						s.Close ();
						return true;
					} else {
						return false;
					}
				}
			}
		}

		/* This test verifies that for every node in our tree that possed a PublicUrl,
		 * we can correctly access it back through RenderUrl
		 */
		[Test]
		public void ReachabilityTest ()
		{
			var rootTree = RootTree.LoadTree (Path.GetFullPath (BaseDir), false);
			Node result;
			var generator = new CheckGenerator ();
			int errorCount = 0;
			int testCount = 0;

			foreach (var leaf in GetLeaves (rootTree.RootNode)) {
				if (!rootTree.RenderUrl (leaf.PublicUrl, generator, out result) || leaf != result) {
					Console.WriteLine ("Error: {0} with HelpSource {1} ", leaf.PublicUrl, leaf.Tree.HelpSource.Name);
					errorCount++;
				}
				testCount++;
			}

			Assert.AreEqual (0, errorCount, errorCount + " / " + testCount.ToString ());
		}

		IEnumerable<Node> GetLeaves (Node node)
		{
			if (node == null)
				yield break;

			if (node.IsLeaf)
				yield return node;
			else {
				foreach (var child in node.ChildNodes) {
					if (!string.IsNullOrEmpty (child.Element) && !child.Element.StartsWith ("root:/"))
						yield return child;
					foreach (var childLeaf in GetLeaves (child))
						yield return childLeaf;
				}
			}
		}

		[Test]
		public void ReachabilityWithShortGenericNotationTest ()
		{
			var rootTree = RootTree.LoadTree (Path.GetFullPath (BaseDir), false);
			Node result;
			var generator = new CheckGenerator ();

			Assert.IsTrue (rootTree.RenderUrl ("T:System.Collections.Concurrent.IProducerConsumerCollection`1", generator, out result), "#1");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Collections.Generic.Dictionary`2", generator, out result), "#2");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Action`4", generator, out result), "#3");
		}
	}
}
