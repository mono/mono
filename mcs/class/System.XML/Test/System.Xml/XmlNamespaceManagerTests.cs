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
	public class XmlNamespaceManagerTests : Assertion
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
			Assert (!namespaceManager.PopScope ());

			// the following strings should have been added to the name table by the
			// namespace manager.
			string xmlnsPrefix = nameTable.Get ("xmlns");
			string xmlPrefix = nameTable.Get ("xml");
			string stringEmpty = nameTable.Get (String.Empty);
			string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";
			string xmlNamespace = "http://www.w3.org/XML/1998/namespace";

			// none of them should be null.
			AssertNotNull (xmlnsPrefix);
			AssertNotNull (xmlPrefix);
			AssertNotNull (stringEmpty);
			AssertNotNull (xmlnsNamespace);
			AssertNotNull (xmlNamespace);

			// Microsoft's XmlNamespaceManager reports that these three
			// namespaces aren't declared for some reason.
			Assert (!namespaceManager.HasNamespace ("xmlns"));
			Assert (!namespaceManager.HasNamespace ("xml"));
			Assert (!namespaceManager.HasNamespace (String.Empty));

			// these three namespaces are declared by default.
			AssertEquals ("http://www.w3.org/2000/xmlns/", namespaceManager.LookupNamespace ("xmlns"));
			AssertEquals ("http://www.w3.org/XML/1998/namespace", namespaceManager.LookupNamespace ("xml"));
			AssertEquals (String.Empty, namespaceManager.LookupNamespace (String.Empty));

			// the namespaces should be the same references found in the name table.
			AssertSame (xmlnsNamespace, namespaceManager.LookupNamespace ("xmlns"));
			AssertSame (xmlNamespace, namespaceManager.LookupNamespace ("xml"));
			AssertSame (stringEmpty, namespaceManager.LookupNamespace (String.Empty));

			// looking up undeclared namespaces should return null.
			AssertNull (namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void AddNamespace ()
		{
			// add a new namespace.
			namespaceManager.AddNamespace ("foo", "http://foo/");
			// make sure the new namespace is there.
			Assert (namespaceManager.HasNamespace ("foo"));
			AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// adding a different namespace with the same prefix
			// is allowed.
			namespaceManager.AddNamespace ("foo", "http://foo1/");
			AssertEquals ("http://foo1/", namespaceManager.LookupNamespace ("foo"));
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
			Assert (!Object.ReferenceEquals (fooNamespace, fooNamespace2));

			// add the namespace with the reference that's not in the name table.
			namespaceManager.AddNamespace ("foo", fooNamespace2);

			// the returned reference should be the same one that's in the name table.
			AssertSame (fooNamespace, namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void PushScope ()
		{
			// add a new namespace.
			namespaceManager.AddNamespace ("foo", "http://foo/");
			// make sure the new namespace is there.
			Assert (namespaceManager.HasNamespace ("foo"));
			AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// push a new scope.
			namespaceManager.PushScope ();
			// add a new namespace.
			namespaceManager.AddNamespace ("bar", "http://bar/");
			// make sure the old namespace is not in this new scope.
			Assert (!namespaceManager.HasNamespace ("foo"));
			// but we're still supposed to be able to lookup the old namespace.
			AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// make sure the new namespace is there.
			Assert (namespaceManager.HasNamespace ("bar"));
			AssertEquals ("http://bar/", namespaceManager.LookupNamespace ("bar"));
		}

		[Test]
		public void PopScope ()
		{
			// add some namespaces and a scope.
			PushScope ();
			// pop the scope.
			Assert (namespaceManager.PopScope ());
			// make sure the first namespace is still there.
			Assert (namespaceManager.HasNamespace ("foo"));
			AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// make sure the second namespace is no longer there.
			Assert (!namespaceManager.HasNamespace ("bar"));
			AssertNull (namespaceManager.LookupNamespace ("bar"));
			// make sure there are no more scopes to pop.
			Assert (!namespaceManager.PopScope ());
			// make sure that popping again doesn't cause an exception.
			Assert (!namespaceManager.PopScope ());
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
			AssertEquals ("urn:foo", namespaceManager.LookupNamespace ("foo"));
			AssertEquals ("urn:bar", namespaceManager.LookupNamespace ("bar"));
		}

		[Test]
		public void AddPushPopRemove ()
		{
			XmlNamespaceManager nsmgr =
				new XmlNamespaceManager (new NameTable ());
			string ns = nsmgr.NameTable.Add ("urn:foo");
			nsmgr.AddNamespace ("foo", ns);
			AssertEquals ("foo", nsmgr.LookupPrefix (ns));
			nsmgr.PushScope ();
			AssertEquals ("foo", nsmgr.LookupPrefix (ns));
			nsmgr.PopScope ();
			AssertEquals ("foo", nsmgr.LookupPrefix (ns));
			nsmgr.RemoveNamespace ("foo", ns);
			AssertNull (nsmgr.LookupPrefix (ns));
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
			AssertNull (nsmgr.LookupPrefix ("urn:fuga"));
			AssertEquals (String.Empty, nsmgr.LookupPrefix ("urn:hoge"));
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
			AssertNotNull (nsmgr.LookupPrefix ("urn:foo"));
// FIXME: This returns registered URI inconsistently.
//			AssertNull ("It is not atomized and thus should be failed", nsmgr.LookupPrefix ("urn:f" + suffix));
#if NET_2_0
			AssertNotNull ("Atomization should not matter.", nsmgr.LookupPrefix ("urn:f" + suffix, false));
#endif
		}

#if NET_2_0
		XmlNamespaceScope l = XmlNamespaceScope.Local;
		XmlNamespaceScope x = XmlNamespaceScope.ExcludeXml;
		XmlNamespaceScope a = XmlNamespaceScope.All;

		[Test]
		public void GetNamespacesInScope ()
		{
			XmlNamespaceManager nsmgr =
				new XmlNamespaceManager (new NameTable ());

			AssertEquals (0, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (0, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (a).Count);

			nsmgr.AddNamespace ("foo", "urn:foo");
			AssertEquals (1, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (2, nsmgr.GetNamespacesInScope (a).Count);

			nsmgr.RemoveNamespace ("foo", "urn:foo", false);
			AssertEquals (0, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (0, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (a).Count);

			// default namespace
			nsmgr.AddNamespace ("", "urn:empty");
			AssertEquals (1, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (2, nsmgr.GetNamespacesInScope (a).Count);

			nsmgr.RemoveNamespace ("", "urn:empty", false);
			AssertEquals (0, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (0, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (a).Count);

			// PushScope
			nsmgr.AddNamespace ("foo", "urn:foo");
			nsmgr.PushScope ();
			AssertEquals (0, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (2, nsmgr.GetNamespacesInScope (a).Count);

			// PopScope
			nsmgr.PopScope ();
			AssertEquals (1, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (2, nsmgr.GetNamespacesInScope (a).Count);

			nsmgr.AddNamespace ("", "");
			AssertEquals (1, nsmgr.GetNamespacesInScope (l).Count);
			AssertEquals (1, nsmgr.GetNamespacesInScope (x).Count);
			AssertEquals (2, nsmgr.GetNamespacesInScope (a).Count);
		}
#endif
	}
}
