// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Xml.Test.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
//

using System;
using System.Diagnostics;
using System.Xml;

using NUnit.Framework;

namespace Ximian.Mono.Tests
{
	public class XmlNamespaceManagerTests : TestCase
	{
		public XmlNamespaceManagerTests(string name) : base(name) { }

		private XmlNamespaceManager _NamespaceManager;

		protected override void SetUp()
		{
			//XmlNameTable nameTable = new NameTable();
			_NamespaceManager = new XmlNamespaceManager(null);
		}

		public void TestNewNamespaceManager()
		{
			Assert(!_NamespaceManager.HasNamespace("xmlns"));
			Assert(!_NamespaceManager.HasNamespace("xml"));
			Assert(!_NamespaceManager.HasNamespace(String.Empty));

			AssertEquals("http://www.w3.org/2000/xmlns/", _NamespaceManager.LookupNamespace("xmlns"));
			AssertEquals("http://www.w3.org/XML/1998/namespace", _NamespaceManager.LookupNamespace("xml"));
			AssertEquals(String.Empty, _NamespaceManager.LookupNamespace(String.Empty));

			AssertNull(_NamespaceManager.LookupNamespace("foo"));
		}

		public void TestAddNamespace()
		{
			// add a new namespace.
			_NamespaceManager.AddNamespace("foo", "http://foo/");
			// make sure the new namespace is there.
			Assert(_NamespaceManager.HasNamespace("foo"));
			AssertEquals("http://foo/", _NamespaceManager.LookupNamespace("foo"));
		}

		public void TestPushScope()
		{
			// add a new namespace.
			_NamespaceManager.AddNamespace("foo", "http://foo/");
			// make sure the new namespace is there.
			Assert(_NamespaceManager.HasNamespace("foo"));
			AssertEquals("http://foo/", _NamespaceManager.LookupNamespace("foo"));
			// push a new scope.
			_NamespaceManager.PushScope();
			// add a new namespace.
			_NamespaceManager.AddNamespace("bar", "http://bar/");
			// make sure the old namespace is still there.
			Assert(_NamespaceManager.HasNamespace("foo"));
			AssertEquals("http://foo/", _NamespaceManager.LookupNamespace("foo"));
			// make sure the new namespace is there.
			Assert(_NamespaceManager.HasNamespace("bar"));
			AssertEquals("http://bar/", _NamespaceManager.LookupNamespace("bar"));
		}

		public void TestPopScope()
		{
			// add some namespaces and a scope.
			TestPushScope();
			// pop the scope.
			Assert(_NamespaceManager.PopScope());
			// make sure the first namespace is still there.
			Assert(_NamespaceManager.HasNamespace("foo"));
			AssertEquals("http://foo/", _NamespaceManager.LookupNamespace("foo"));
			// make sure the second namespace is no longer there.
			Assert(!_NamespaceManager.HasNamespace("bar"));
			AssertNull(_NamespaceManager.LookupNamespace("bar"));
		}
	}
}
