//
// System.Web.SiteMapProviderTest.cs - Unit tests for System.Web.SiteMapProvider
//
// Author:
//	Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell, Inc (http://novell.com)
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


using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Hosting;
using NUnit.Framework;

using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;

using Tests;

namespace MonoTests.System.Web
{
	[TestFixture]
	public class XmlSiteMapProviderTest
	{

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type myType = GetType ();
			WebTest.CopyResource (myType, "test_map_01.sitemap", "test_map_01.sitemap");
			WebTest.CopyResource (myType, "test_map_02.sitemap", "test_map_02.sitemap");
			WebTest.CopyResource (myType, "test_map_03.sitemap", "test_map_03.sitemap");
			WebTest.CopyResource (myType, "test_map_04.sitemap", "test_map_04.sitemap");
			WebTest.CopyResource (myType, "test_map_05.sitemap", "test_map_05.sitemap");
			WebTest.CopyResource (myType, "test_map_06.sitemap", "test_map_06.sitemap");
			WebTest.CopyResource (myType, "test_map_07.sitemap", "test_map_07.sitemap");
			WebTest.CopyResource (myType, "test_map_08.sitemap", "test_map_08.sitemap");
			WebTest.CopyResource (myType, "test_map_09.sitemap", "test_map_09.sitemap");
			WebTest.CopyResource (myType, "sub_map_01.sitemap", "sub_map_01.sitemap");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNode_Null_1 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var node = new SiteMapNode (provider, "/test.aspx");

			provider.DoAddNode (null, node);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNode_Null_2 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var node = new SiteMapNode (provider, "/test.aspx");

			provider.DoAddNode (node, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddNode_DifferentProviders_01 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var node = new SiteMapNode (new TestSiteMapProvider (), "/test.aspx");
			var parentNode = new SiteMapNode (provider, "/test2.aspx");

			// SiteMapNode  cannot be found in current provider, only nodes in the same provider can be added.
			provider.DoAddNode (node, parentNode);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddNode_DifferentProviders_02 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var node = new SiteMapNode (provider, "/test.aspx");
			var parentNode = new SiteMapNode (new TestSiteMapProvider (), "/test2.aspx");

			// SiteMapNode  cannot be found in current provider, only nodes in the same provider can be added.
			provider.DoAddNode (node, parentNode);
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void AddNode_01 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var node = new SiteMapNode (provider, "/test.aspx");
			var parentNode = new SiteMapNode (provider, "/test2.aspx");

			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "~/test_map_01.sitemap");
			provider.Initialize ("TestMap", nvc);

			// The application relative virtual path '~/test_map_01.sitemap' cannot be made absolute, because the path to the application is not known.
			provider.DoAddNode (node, parentNode);
		}

		[Test]
		public void AddNode_02 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (AddNode_02_OnLoad)).Run ();
		}

		public static void AddNode_02_OnLoad (Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			provider.CallTrace = null;
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "~/test_map_01.sitemap");
			provider.Initialize ("TestMap", nvc);

			SiteMapNode rootNode = provider.RootNode;
			provider.CallTrace = null;

			var node = new SiteMapNode (provider, "test3.aspx", "~/test3.aspx");
			provider.DoAddNode (node, rootNode);

			Assert.IsNotNull (provider.CallTrace, "#A1");
			AssertHelper.Greater (provider.CallTrace.Length, 1, "#A1-1");
			Assert.AreEqual (provider.CallTrace[0].Name, "BuildSiteMap", "#A1-2");
		}

		[Test]
		public void Initialize_1 ()
		{
			var provider = new XmlSiteMapProviderPoker ();

			provider.Initialize ("TestMap", null);
			Assert.AreEqual ("TestMap", provider.Name, "#A1");
		}

		[Test]
		public void Initialize_2 ()
		{
			var provider = new XmlSiteMapProviderPoker ();

			provider.Initialize ("TestMap", new NameValueCollection ());
			Assert.AreEqual ("TestMap", provider.Name, "#A1");
		}

		[Test]
		public void Initialize_3 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "test.sitemap");
			provider.Initialize ("TestMap", nvc);
			Assert.AreEqual ("TestMap", provider.Name, "#A1");
		}

		[Test]
		public void Initialize_4 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "test.sitemap");
			nvc.Add ("description", "Test XML provider");
			provider.Initialize ("TestMap", nvc);
			Assert.AreEqual ("TestMap", provider.Name, "#A1");
			Assert.AreEqual ("Test XML provider", provider.Description, "#A2");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void Initialize_5 ()
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "test.sitemap");
			nvc.Add ("description", "Test XML provider");

			// The attribute 'acme' is unexpected in the configuration of the 'TestMap' provider.
			nvc.Add ("acme", "test provider");
			provider.Initialize ("TestMap", nvc);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Initialize_6 ()
		{
			var provider = new XmlSiteMapProviderPoker ();

			provider.Initialize ("TestMap", null);

			// XmlSiteMapProvider cannot be initialized twice
			provider.Initialize ("TestMap2", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RootNode_1 ()
		{
			var provider = new XmlSiteMapProviderPoker ();

			// Thrown from internal GetConfigDocument ():
			// The siteMapFile attribute must be specified on the XmlSiteMapProvider
			provider.Initialize ("TestMap", null);
			var rn = provider.RootNode;
		}

		[Test]
		public void RootNode_2 ()
		{
			new WebTest (PageInvoker.CreateOnLoad (RootNode_2_OnLoad)).Run ();
		}

		public static void RootNode_2_OnLoad (Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			provider.CallTrace = null;
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "~/test_map_01.sitemap");
			provider.CallTrace = null;
			provider.Initialize ("TestMap", nvc);
			Assert.IsNotNull (provider.RootNode, "#A1");
			Assert.AreEqual (provider.RootNode.Provider, provider, "#A2");
			Assert.IsNotNull (provider.CallTrace, "#A3");
			AssertHelper.Greater (provider.CallTrace.Length, 1, "#A3-1");
			Assert.AreEqual ("BuildSiteMap", provider.CallTrace[0].Name, "#A3-2");
			Assert.AreEqual ("get_RootNode", provider.CallTrace[1].Name, "#A3-3");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void InvalidFileExtension ()
		{
			// The file /NunitWeb/test_map_01.extension has an invalid extension, only .sitemap files are allowed in XmlSiteMapProvider.
			new WebTest (PageInvoker.CreateOnLoad (InvalidFileExtension_OnLoad)).Run ();
		}

		public static void InvalidFileExtension_OnLoad (Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "~/test_map_01.extension");

			provider.Initialize ("TestMap", nvc);
			var rn = provider.RootNode;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MissingMapFile ()
		{
			new WebTest (PageInvoker.CreateOnLoad (MissingMapFile_OnLoad)).Run ();
		}

		public static void MissingMapFile_OnLoad (Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "~/missing_map_file.sitemap");

			provider.Initialize ("TestMap", nvc);
			var rn = provider.RootNode;
		}

		[Test]
		public void NodeWithSiteMapFile_01 ()
		{
			var test = new WebTest (PageInvoker.CreateOnLoad (NodeWithSiteMapFile_01_OnLoad)).Run ();
		}

		public static void NodeWithSiteMapFile_01_OnLoad (Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "~/test_map_02.sitemap");

			provider.Initialize ("TestMap", nvc);
			var rn = provider.RootNode;

			string expectedTreeString = "UNTITLED_0[0]; Test 1[1]; Sub 1 [/NunitWeb/sub_map_01.sitemap][1]; Sub Sub 1 [/NunitWeb/sub_map_01.sitemap][2]";
			string treeString = provider.GetTreeString ();

			Assert.AreEqual (expectedTreeString, treeString, "#A1");
		}

		[Test]
		public void NodeWithProvider_01 ()
		{
			var test = new WebTest (PageInvoker.CreateOnLoad ((Page p) => {
				NodeWithProvider_OnLoad ("~/test_map_07.sitemap", p);
			})).Run ();
		}

		[Test]
		[ExpectedException (typeof (ProviderException))]
		public void NodeWithProvider_02 ()
		{
			new WebTest (PageInvoker.CreateOnLoad ((Page p) => {
				NodeWithProvider_OnLoad ("~/test_map_08.sitemap", p);
			})).Run ();
		}

		public static void NodeWithProvider_OnLoad (string filePath, Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", filePath);

			provider.Initialize ("TestMap", nvc);
			var rn = provider.RootNode;

			string expectedTreeString = "UNTITLED_0[0]; Test 1[1]; Test [TestSiteMapProvider][1]";
			string treeString = provider.GetTreeString ();
			Assert.AreEqual (expectedTreeString, treeString, "#A1");

			SiteMapNode node = provider.FindSiteMapNode ("default.aspx");
			Assert.IsNotNull (node, "#B1");
			Assert.AreEqual ("Test", node.Title, "#B1-1");
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void InvalidMapFile_01 ()
		{
			// Top element must be siteMap.
			new WebTest (PageInvoker.CreateOnLoad ((Page p) => {
				InvalidMapFile_OnLoad ("~/test_map_03.sitemap", p);
			})).Run ();
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void InvalidMapFile_02 ()
		{
			// Only <siteMapNode> elements are allowed at this location.
			var test = new WebTest (PageInvoker.CreateOnLoad ((Page p) => {
				InvalidMapFile_OnLoad ("~/test_map_04.sitemap", p);
			})).Run ();
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void InvalidMapFile_03 ()
		{
			// Only <siteMapNode> elements are allowed at this location.
			var test = new WebTest (PageInvoker.CreateOnLoad ((Page p) => {
				InvalidMapFile_OnLoad ("~/test_map_05.sitemap", p);
			})).Run ();
		}

		[Test]
		[ExpectedException (typeof (ConfigurationErrorsException))]
		public void InvalidMapFile_04 ()
		{
			// Only <siteMapNode> elements are allowed at this location.
			var test = new WebTest (PageInvoker.CreateOnLoad ((Page p) => {
				InvalidMapFile_OnLoad ("~/test_map_06.sitemap", p);
			})).Run ();
		}

		public static void InvalidMapFile_OnLoad (string filePath, Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", filePath);

			provider.Initialize ("TestMap", nvc);
			var rn = provider.RootNode;
		}

		[Test]
		public void MapFileWithNonStandardAttributes ()
		{
			// Only <siteMapNode> elements are allowed at this location.
			new WebTest (PageInvoker.CreateOnLoad (MapFileWithNonStandardAttributes_OnLoad)).Run ();
		}

		public static void MapFileWithNonStandardAttributes_OnLoad (Page p)
		{
			var provider = new XmlSiteMapProviderPoker ();
			var nvc = new NameValueCollection ();
			nvc.Add ("siteMapFile", "~/test_map_09.sitemap");

			provider.Initialize ("TestMap", nvc);
			var rn = provider.RootNode;

			//TODO: find out what happens to non-standard attributes
			//SiteMapNode node = rn.ChildNodes[0];
			//Assert.IsNotNull (node, "#A1");
			//Assert.AreEqual ("some, keyword, another, one", node["keywords"], "#A1-1");

			//node = rn.ChildNodes[1];
			//Assert.IsNotNull (node, "#B1");
			//Assert.AreEqual("value", node["someattribute"], "#B1-1");
		}
	}

	class XmlSiteMapProviderPoker : XmlSiteMapProvider
	{
		public MethodBase[] CallTrace { get; set; }

		public void DoAddNode (SiteMapNode node, SiteMapNode parentNode)
		{
			AddNode (node, parentNode);
		}

		public override SiteMapNode BuildSiteMap ()
		{
			StoreCallTrace ();
			return base.BuildSiteMap ();
		}

		public string GetTreeString ()
		{
			var sb = new StringBuilder ();
			int untitled_counter = 0;
			BuildTreeString (RootNode, sb, 0, ref untitled_counter);
			return sb.ToString ();
		}

		void BuildTreeString (SiteMapNode top, StringBuilder sb, int level, ref int untitled_counter)
		{
			string title = top.Title;

			if (String.IsNullOrEmpty (title))
				title = "UNTITLED_" + untitled_counter++;

			SiteMapProvider provider = top.Provider;
			if (provider != this) {
				if (provider == null)
					title += " [NULL_PROVIDER]";
				else {
					string name = provider.Name;
					if (String.IsNullOrEmpty (name))
						title += " [" + provider.GetType () + "]";
					else
						title += " [" + name + "]";
				}
			}

			if (sb.Length > 0)
				sb.Append ("; ");
			sb.Append (title + "[" + level + "]");
			SiteMapNodeCollection childNodes = top.ChildNodes;
			if (childNodes != null && childNodes.Count > 0) {
				foreach (SiteMapNode child in childNodes)
					BuildTreeString (child, sb, level + 1, ref untitled_counter);
			}
		}

		void StoreCallTrace ()
		{
			CallTrace = null;
			StackFrame[] frames = new StackTrace (1).GetFrames ();
			var frameMethods = new List<MethodBase> ();

			int i = 0;
			foreach (StackFrame sf in frames)
				frameMethods.Add (sf.GetMethod ());
			CallTrace = frameMethods.ToArray ();
		}
	}
}
