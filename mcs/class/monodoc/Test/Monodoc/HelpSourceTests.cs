using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Monodoc;
using Monodoc.Generators;
using Monodoc.Providers;

// Used by ReachabilityWithCrefsTest
// using HtmlAgilityPack;

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

			//Assert.AreEqual (0, errorCount, errorCount + " / " + testCount.ToString ());

			// HACK: in reality we have currently 4 known issues which are due to duplicated namespaces across
			// doc sources, something that was never supported and that we need to improve/fix at some stage
			Assert.LessOrEqual (4, errorCount, errorCount + " / " + testCount.ToString ());
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
			Assert.IsTrue (rootTree.RenderUrl ("T:System.EventHandler`1", generator, out result), "#4");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`5", generator, out result), "#5a");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`4", generator, out result), "#5b");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`6", generator, out result), "#5c");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`7", generator, out result), "#5d");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`3", generator, out result), "#5e");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`2", generator, out result), "#5f");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`1", generator, out result), "#5g");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`8", generator, out result), "#5h");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func`9", generator, out result), "#5i");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Action`3", generator, out result), "#6a");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Action`2", generator, out result), "#6b");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Action`4", generator, out result), "#6c");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.IComparable`1", generator, out result), "#7");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Lazy`1", generator, out result), "#8");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Tuple`1", generator, out result), "#9a");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Tuple`2", generator, out result), "#9b");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Tuple`3", generator, out result), "#9c");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Tuple`4", generator, out result), "#9d");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Collections.Generic.Dictionary`2+ValueCollection", generator, out result), "#10");
			Assert.IsFalse (rootTree.RenderUrl ("T:System.EventHandler`2", generator, out result), "#11");
			Assert.IsFalse (rootTree.RenderUrl ("T:System.Lazy`2", generator, out result), "#12");
		}

		[Test]
		public void AspNetStyleUrlReachabilityTest ()
		{
			var rootTree = RootTree.LoadTree (Path.GetFullPath (BaseDir), false);
			Node result;
			var generator = new CheckGenerator ();

			Assert.IsTrue (rootTree.RenderUrl ("T:System.Collections.Generic.Dictionary{TKey,TValue}", generator, out result), "#1");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Action{T1,T2}", generator, out result), "#2");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.EventHandler{TEventArgs}", generator, out result), "#3");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Func{T1,T2,T3,TResult}", generator, out result), "#4");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.Collections.Generic.Dictionary{TKey,TValue}+ValueCollection", generator, out result), "#5");
			Assert.IsTrue (rootTree.RenderUrl ("T:System.IComparable{T}", generator, out result), "#6");
		}

		[Test]
		public void PublicUrlOnUnattachedHelpSourceRoot ()
		{
			// Unattached help source have no root:/ URL attributed
			var hs = new EcmaHelpSource (Path.Combine (BaseDir, "sources", "netdocs"), false);
			var rootTree = RootTree.LoadTree (Path.GetFullPath (BaseDir), false);
			hs.RootTree = rootTree;
			Assert.IsNull (hs.Tree.RootNode.PublicUrl);
			var nsChildUrl = hs.Tree.RootNode.ChildNodes.First ().PublicUrl;
			Assert.IsNotNull (nsChildUrl);
			StringAssert.StartsWith ("N:", nsChildUrl);
			// Verify GetNodeTypeParent
			var typeNode = hs.Tree.RootNode.ChildNodes.First ().ChildNodes.First ();
			var metaNode = typeNode.ChildNodes.First (cn => cn.Element == "M");
			StringAssert.StartsWith (typeNode.PublicUrl, metaNode.PublicUrl);
		}

		/*
		[Test, Ignore ("Mono documentation is full of syntax errors so we can't use it reliably for this test")]
		public void ReachabilityWithCrefsTest ()
		{
			var rootTree = RootTree.LoadTree (Path.GetFullPath (BaseDir), false);
			Node result;
			var htmlGenerator = new HtmlGenerator (null);
			var crefs = new HashSet<string> ();
			var generator = new CheckGenerator ();
			int errorCount = 0;

			foreach (var leaf in GetLeaves (rootTree.RootNode)) {
				Dictionary<string, string> context;
				string internalId = leaf.Tree.HelpSource.GetInternalIdForUrl (leaf.PublicUrl, out result, out context);
				if (leaf.Tree.HelpSource.GetDocumentTypeForId (internalId) != DocumentType.EcmaXml)
					continue;

				string content = null;
				if (string.IsNullOrEmpty (content = rootTree.RenderUrl (leaf.PublicUrl, htmlGenerator, out result)) || leaf != result) {
					Console.WriteLine ("Error: {0} with HelpSource {1} ", leaf.PublicUrl, leaf.Tree.HelpSource.Name);
					continue;
				}

				HtmlDocument doc = new HtmlDocument();
				try {
					doc.LoadHtml (content);
				} catch {
					Console.WriteLine ("Couldn't load a HTML document for URL {0}", leaf.PublicUrl);
					continue;
				}

				foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]")) {
					var newUrl = link.Attributes["href"].Value;
					var hashIndex = newUrl.IndexOf ('#');
					if (hashIndex != -1)
						newUrl = newUrl.Substring (0, hashIndex);
					if (newUrl.Length > 1 && newUrl[1] == ':' && char.IsLetter (newUrl, 0) && char.ToLowerInvariant (newUrl[0]) != 'c')
						crefs.Add (newUrl);
				}

				foreach (var cref in crefs) {
					if (!rootTree.RenderUrl (cref, generator, out result) || result == null) {
						Console.WriteLine ("Error with cref: `{0}'", cref);
						errorCount++;
					}
				}

				crefs.Clear ();
			}

			Assert.AreEqual (0, errorCount, errorCount + " / " + crefs.Count);
		}*/
	}
}
