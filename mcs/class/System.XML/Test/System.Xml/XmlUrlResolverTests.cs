//
// System.Xml.XmlUrlResolver.cs
//
// Authors:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlUrlResolverTests : Assertion
	{
		XmlUrlResolver resolver;

		[SetUp]
		public void GetReady ()
		{
			resolver = new XmlUrlResolver ();
		}

		[Test]
		public void FileUri ()
		{
			Uri resolved = resolver.ResolveUri (null, "XmlFiles/xsd/xml.xsd");
			AssertEquals ("file", resolved.Scheme);
			Stream s = resolver.GetEntity (resolved, null, typeof (Stream)) as Stream;
		}

		[Test]
		public void FileUri2 ()
		{
			AssertEquals ("file://usr/local/src", resolver.ResolveUri (new Uri ("file://usr/local/src"), null).ToString ());
			AssertEquals ("file://usr/local/src", resolver.ResolveUri (new Uri ("file:///usr/local/src"), null).ToString ());
		}

		[Test]
		public void HttpUri ()
		{
			AssertEquals ("http://test.xml/", resolver.ResolveUri (null, "http://test.xml").ToString ());
		}

		[Test]
		public void HttpUri2 ()
		{
			AssertEquals ("http://go-mono.com/", resolver.ResolveUri (new Uri ("http://go-mono.com"), null).ToString ());
		}

		[Test]
		public void NullArgs ()
		{
			try {
				resolver.ResolveUri (null, null);
				Fail ("Should be error (MS.NET throws ArgumentException here).");
			} catch (Exception) {
				// OK
			}
		}
	}
}
