//
// XmlNamespaceManagerTests.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlNamespaceManagerTests : TestCase
	{
		public XmlNamespaceManagerTests () : base ("MonoTests.System.Xml.XmlNameSpaceManagerTests testsuite") { }
		public XmlNamespaceManagerTests (string name) : base (name) { }

		private XmlNameTable nameTable;
		private XmlNamespaceManager namespaceManager;

		protected override void SetUp ()
		{
			nameTable = new NameTable ();
			namespaceManager = new XmlNamespaceManager (nameTable);
		}

		public void TestNewNamespaceManager ()
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

		public void TestAddNamespace ()
		{
			// add a new namespace.
			namespaceManager.AddNamespace ("foo", "http://foo/");
			// make sure the new namespace is there.
			Assert (namespaceManager.HasNamespace ("foo"));
			AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
		}

		public void TestAddNamespaceWithNameTable ()
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

		public void TestPushScope ()
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

		public void TestPopScope ()
		{
			// add some namespaces and a scope.
			TestPushScope ();
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

		public void TestLookupPrefix ()
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
	}
}
