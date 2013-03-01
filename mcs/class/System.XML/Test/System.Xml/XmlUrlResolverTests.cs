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
#if NET_4_5
using System.Reflection;
#endif

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlUrlResolverTests
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
			Uri resolved = resolver.ResolveUri (null, "Test/XmlFiles/xsd/xml.xsd");
			Assert.AreEqual ("file", resolved.Scheme);
			Stream s = resolver.GetEntity (resolved, null, typeof (Stream)) as Stream;
		}

		[Test]
		[Category ("NotDotNet")]
		public void FileUri2 ()
		{
			Assert.AreEqual (resolver.ResolveUri (new Uri ("file://usr/local/src"), null).ToString (), "file://usr/local/src");
			// MS.NET returns the Uri.ToString() as 
			// file://usr/local/src, but it is apparently 
			// incorrect in the context of Unix path.
			Assert.AreEqual (resolver.ResolveUri (new Uri ("file:///usr/local/src"), null).ToString (), "file:///usr/local/src");
		}

		[Test]
		public void HttpUri ()
		{
			Assert.AreEqual (resolver.ResolveUri (null, "http://test.xml").ToString (), "http://test.xml/");
		}

		[Test]
		public void HttpUri2 ()
		{
			Assert.AreEqual (resolver.ResolveUri (new Uri ("http://go-mono.com"), null).ToString (), "http://go-mono.com/");
		}

		[Test]
#if !NET_2_0
		[Category ("NotDotNet")] // It should throw ArgumentNullException.
#endif
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolveUriWithNullArgs ()
		{
			resolver.ResolveUri (null, null);
			Assert.Fail ("Should be error (MS.NET throws ArgumentException here).");
		}

//		[Test] Uncomment if you want to test.
		public void GetEntityWithNullArgs ()
		{
			Uri uri = new Uri ("http://www.go-mono.com/index.rss");
			resolver.GetEntity (uri, null, null);
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetEntityWithRelativeFileUri ()
		{
			resolver.GetEntity (new Uri ("file.txt", UriKind.Relative), null, typeof (Stream));
		}
#endif

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetEntityWithNonStreamReturnType ()
		{
			resolver.GetEntity (new Uri ("http://www.go-mono.com/"), null, typeof (File));
		}

		[Test] // bug #998
		public void NullAbsoluteUriWithCustomSchemedRelativeUri ()
		{
			XmlResolver res = new XmlUrlResolver ();
			var uri = res.ResolveUri (null, "view:Standard.xslt");
			Assert.AreEqual ("view", uri.Scheme, "#1");
			Assert.AreEqual ("Standard.xslt", uri.AbsolutePath, "#2");
			Assert.AreEqual ("view:Standard.xslt", uri.AbsoluteUri, "#2");
		}

#if NET_4_5
		[Test]
		[Category("Async")]
		public void TestAsync ()
		{
			var loc = Assembly.GetExecutingAssembly ().Location;
			Uri resolved = resolver.ResolveUri (null, loc);
			Assert.AreEqual ("file", resolved.Scheme);
			var task = resolver.GetEntityAsync (resolved, null, typeof (Stream));
			Assert.That (task.Wait (3000));
			Assert.IsInstanceOfType (typeof (Stream), task.Result);
		}

		[Test]
		[Category("Async")]
		public void TestAsyncError ()
		{
			var loc = Assembly.GetExecutingAssembly ().Location;
			Uri resolved = resolver.ResolveUri (null, loc);
			Assert.AreEqual ("file", resolved.Scheme);
			var task = resolver.GetEntityAsync (resolved, null, typeof (File));
			try {
				task.Wait (3000);
				Assert.Fail ("#1");
			} catch (Exception ex) {
				if (ex is AggregateException)
					ex = ((AggregateException) ex).InnerException;
				Assert.IsInstanceOfType (typeof (XmlException), ex);
			}
		}
#endif
	}
}
