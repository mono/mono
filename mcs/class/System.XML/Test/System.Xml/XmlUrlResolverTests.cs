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
		[ExpectedException (typeof (NullReferenceException))]
		public void ResolveUriWithNullArgs ()
		{
			resolver.ResolveUri (null, null);
			Fail ("Should be error (MS.NET throws ArgumentException here).");
		}

//		[Test] Uncomment if you want to test.
		public void GetEntityWithNullArgs ()
		{
			Uri uri = new Uri ("http://www.go-mono.com/index.rss");
			resolver.GetEntity (uri, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEntityWithRelativeFileUri ()
		{
			resolver.GetEntity (new Uri ("file://file.txt"), null, typeof (Stream));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetEntityWithNonStreamReturnType ()
		{
			resolver.GetEntity (new Uri ("http://www.go-mono.com/"), null, typeof (File));
		}
	}
}
