//
// XmlNamespaceManagerTests.cs
//
// Authors:
//   Jason Diamond (jason@injektilo.org)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlNamespaceManagerTests
	{
		private XmlNameTable nameTable;
		private XmlNamespaceManager namespaceManager;

		[SetUp]
		public void GetReady ()
		{
			nameTable = new NameTable ();
			namespaceManager = new XmlNamespaceManager (nameTable);
		}

		[Test]
		public void NewNamespaceManager ()
		{
			// make sure that you can call PopScope when there aren't any to pop.
			Assert.IsTrue (!namespaceManager.PopScope ());

			// the following strings should have been added to the name table by the
			// namespace manager.
			string xmlnsPrefix = nameTable.Get ("xmlns");
			string xmlPrefix = nameTable.Get ("xml");
			string stringEmpty = nameTable.Get (String.Empty);
			string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";
			string xmlNamespace = "http://www.w3.org/XML/1998/namespace";

			// none of them should be null.
			Assert.IsNotNull (xmlnsPrefix);
			Assert.IsNotNull (xmlPrefix);
			Assert.IsNotNull (stringEmpty);
			Assert.IsNotNull (xmlnsNamespace);
			Assert.IsNotNull (xmlNamespace);

			// Microsoft's XmlNamespaceManager reports that these three
			// namespaces aren't declared for some reason.
			Assert.IsTrue (!namespaceManager.HasNamespace ("xmlns"));
			Assert.IsTrue (!namespaceManager.HasNamespace ("xml"));
			Assert.IsTrue (!namespaceManager.HasNamespace (String.Empty));

			// these three namespaces are declared by default.
			Assert.AreEqual ("http://www.w3.org/2000/xmlns/", namespaceManager.LookupNamespace ("xmlns"));
			Assert.AreEqual ("http://www.w3.org/XML/1998/namespace", namespaceManager.LookupNamespace ("xml"));
			Assert.AreEqual (String.Empty, namespaceManager.LookupNamespace (String.Empty));

			// the namespaces should be the same references found in the name table.
			Assert.AreSame (xmlnsNamespace, namespaceManager.LookupNamespace ("xmlns"));
			Assert.AreSame (xmlNamespace, namespaceManager.LookupNamespace ("xml"));
			Assert.AreSame (stringEmpty, namespaceManager.LookupNamespace (String.Empty));

			// looking up undeclared namespaces should return null.
			Assert.IsNull (namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void AddNamespace ()
		{
			// add a new namespace.
			namespaceManager.AddNamespace ("foo", "http://foo/");
			// make sure the new namespace is there.
			Assert.IsTrue (namespaceManager.HasNamespace ("foo"));
			Assert.AreEqual ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// adding a different namespace with the same prefix
			// is allowed.
			namespaceManager.AddNamespace ("foo", "http://foo1/");
			Assert.AreEqual ("http://foo1/", namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void AddNamespaceWithNameTable ()
		{
			// add a known reference to the name table.
			string fooNamespace = "http://foo/";
			nameTable.Add(fooNamespace);

			// create a new string with the same value but different address.
			string fooNamespace2 = "http://";
			fooNamespace2 += "foo/";

			// the references must be different in order for this test to prove anything.
			Assert.IsTrue (!Object.ReferenceEquals (fooNamespace, fooNamespace2));

			// add the namespace with the reference that's not in the name table.
			namespaceManager.AddNamespace ("foo", fooNamespace2);

			// the returned reference should be the same one that's in the name table.
			Assert.AreSame (fooNamespace, namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void AddNamespace_XmlPrefix ()
		{
			namespaceManager.AddNamespace ("xml", "http://www.w3.org/XML/1998/namespace");
			namespaceManager.AddNamespace ("XmL", "http://foo/");
			namespaceManager.AddNamespace ("xmlsomething", "http://foo/");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddNamespace_XmlPrefix_Invalid ()
		{
			namespaceManager.AddNamespace ("xml", "http://foo/");
		}

		[Test]
		public void PushScope ()
		{
			// add a new namespace.
			namespaceManager.AddNamespace ("foo", "http://foo/");
			// make sure the new namespace is there.
			Assert.IsTrue (namespaceManager.HasNamespace ("foo"));
			Assert.AreEqual ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// push a new scope.
			namespaceManager.PushScope ();
			// add a new namespace.
			namespaceManager.AddNamespace ("bar", "http://bar/");
			// make sure the old namespace is not in this new scope.
			Assert.IsTrue (!namespaceManager.HasNamespace ("foo"));
			// but we're still supposed to be able to lookup the old namespace.
			Assert.AreEqual ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// make sure the new namespace is there.
			Assert.IsTrue (namespaceManager.HasNamespace ("bar"));
			Assert.AreEqual ("http://bar/", namespaceManager.LookupNamespace ("bar"));
		}

		[Test]
		public void PopScope ()
		{
			// add some namespaces and a scope.
			PushScope ();
			// pop the scope.
			Assert.IsTrue (namespaceManager.PopScope ());
			// make sure the first namespace is still there.
			Assert.IsTrue (namespaceManager.HasNamespace ("foo"));
			Assert.AreEqual ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// make sure the second namespace is no longer there.
			Assert.IsTrue (!namespaceManager.HasNamespace ("bar"));
			Assert.IsNull (namespaceManager.LookupNamespace ("bar"));
			// make sure there are no more scopes to pop.
			Assert.IsTrue (!namespaceManager.PopScope ());
			// make sure that popping again doesn't cause an exception.
			Assert.IsTrue (!namespaceManager.PopScope ());
		}

		[Test]
		public void PopScopeMustKeepAddedInScope ()
		{
			namespaceManager = new XmlNamespaceManager (new NameTable ()); // clear
			namespaceManager .AddNamespace ("foo", "urn:foo");	// 0
			namespaceManager .AddNamespace ("bar", "urn:bar");	// 0
			namespaceManager .PushScope ();	// 1
			namespaceManager .PushScope ();	// 2
			namespaceManager .PopScope ();	// 2
			namespaceManager .PopScope ();	// 1
			namespaceManager .PopScope ();	// 0
			Assert.AreEqual ("urn:foo", namespaceManager.LookupNamespace ("foo"));
			Assert.AreEqual ("urn:bar", namespaceManager.LookupNamespace ("bar"));
		}

		[Test]
		public void AddPushPopRemove ()
		{
			XmlNamespaceManager nsmgr =
				new XmlNamespaceManager (new NameTable ());
			string ns = nsmgr.NameTable.Add ("urn:foo");
			nsmgr.AddNamespace ("foo", ns);
			Assert.AreEqual ("foo", nsmgr.LookupPrefix (ns));
			nsmgr.PushScope ();
			Assert.AreEqual ("foo", nsmgr.LookupPrefix (ns));
			nsmgr.PopScope ();
			Assert.AreEqual ("foo", nsmgr.LookupPrefix (ns));
			nsmgr.RemoveNamespace ("foo", ns);
			Assert.IsNull (nsmgr.LookupPrefix (ns));
		}

		[Test]
		public void LookupPrefix ()
		{
			// This test should use an empty nametable.
			XmlNamespaceManager nsmgr =
				new XmlNamespaceManager (new NameTable ());
			nsmgr.NameTable.Add ("urn:hoge");
			nsmgr.NameTable.Add ("urn:fuga");
			nsmgr.AddNamespace (string.Empty, "urn:hoge");
			Assert.IsNull (nsmgr.LookupPrefix ("urn:fuga"));
			Assert.AreEqual (String.Empty, nsmgr.LookupPrefix ("urn:hoge"));
		}

		string suffix = "oo";

		[Test]
		public void AtomizedLookup ()
		{
			if (DateTime.Now.Year == 0)
				suffix = String.Empty;
			XmlNamespaceManager nsmgr =
				new XmlNamespaceManager (new NameTable ());
			nsmgr.AddNamespace ("foo", "urn:foo");
			Assert.IsNotNull (nsmgr.LookupPrefix ("urn:foo"));
// FIXME: This returns registered URI inconsistently.
//			Assert.IsNull (nsmgr.LookupPrefix ("urn:f" + suffix), "It is not atomized and thus should be failed");
		}

		[Test]
		public void TryToAddPrefixXml ()
		{
			NameTable nt = new NameTable ();
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (nt);

			nsmgr.AddNamespace ("xml", "http://www.w3.org/XML/1998/namespace");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TryToAddPrefixXmlns ()
		{
			NameTable nt = new NameTable ();
			XmlNamespaceManager nsmgr = new XmlNamespaceManager (nt);

			nsmgr.AddNamespace ("xmlns", "http://www.w3.org/2000/xmlns/");
		}

#if NET_2_0
		XmlNamespaceScope l = XmlNamespaceScope.Local;
		XmlNamespaceScope x = XmlNamespaceScope.ExcludeXml;
		XmlNamespaceScope a = XmlNamespaceScope.All;

		[Test]
		[Category ("NotDotNet")] // MS bug
		public void GetNamespacesInScope ()
		{
			XmlNamespaceManager nsmgr =
				new XmlNamespaceManager (new NameTable ());

			Assert.AreEqual (0, nsmgr.GetNamespacesInScope (l).Count, "#1");
			Assert.AreEqual (0, nsmgr.GetNamespacesInScope (x).Count, "#2");
			Assert.AreEqual (1, nsmgr.GetNamespacesInScope (a).Count, "#3");

			nsmgr.AddNamespace ("foo", "urn:foo");
			Assert.AreEqual (1, nsmgr.GetNamespacesInScope (l).Count, "#4");
			Assert.AreEqual (1, nsmgr.GetNamespacesInScope (x).Count, "#5");
			Assert.AreEqual (2, nsmgr.GetNamespacesInScope (a).Count, "#6");

			// default namespace
			nsmgr.AddNamespace ("", "urn:empty");
			Assert.AreEqual (2, nsmgr.GetNamespacesInScope (l).Count, "#7");
			Assert.AreEqual (2, nsmgr.GetNamespacesInScope (x).Count, "#8");
			Assert.AreEqual (3, nsmgr.GetNamespacesInScope (a).Count, "#9");

			// PushScope
			nsmgr.AddNamespace ("foo", "urn:foo");
			nsmgr.PushScope ();
			Assert.AreEqual (0, nsmgr.GetNamespacesInScope (l).Count, "#10");
			Assert.AreEqual (2, nsmgr.GetNamespacesInScope (x).Count, "#11");
			Assert.AreEqual (3, nsmgr.GetNamespacesInScope (a).Count, "#12");

			// PopScope
			nsmgr.PopScope ();
			Assert.AreEqual (2, nsmgr.GetNamespacesInScope (l).Count, "#13");
			Assert.AreEqual (2, nsmgr.GetNamespacesInScope (x).Count, "#14");
			Assert.AreEqual (3, nsmgr.GetNamespacesInScope (a).Count, "#15");

			nsmgr.AddNamespace ("", "");
			// MS bug - it should return 1 for .Local but it returns 2 instead.
			Assert.AreEqual (1, nsmgr.GetNamespacesInScope (l).Count, "#16");
			Assert.AreEqual (1, nsmgr.GetNamespacesInScope (x).Count, "#17");
			Assert.AreEqual (2, nsmgr.GetNamespacesInScope (a).Count, "#18");
		}
#endif
	}
}
