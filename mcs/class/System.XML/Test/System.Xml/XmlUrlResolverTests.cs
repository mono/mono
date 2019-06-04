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
using System.Reflection;

using MonoTests.Helpers;

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
			Uri resolved = resolver.ResolveUri (null, TestResourceHelper.GetFullPathOfResource ("Test/XmlFiles/xsd/xml.xsd"));
			Assert.AreEqual ("file", resolved.Scheme);
			Stream s = resolver.GetEntity (resolved, null, typeof (Stream)) as Stream;
		}

		[Test]
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
			Assert.AreEqual (resolver.ResolveUri (new Uri ("http://example.com"), null).ToString (), "http://example.com/");
		}

		[Test]
		[Category ("NotDotNet")] // It should throw ArgumentNullException.
		[Ignore(".NET implementation does not throw ArgumentNullException.")]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ResolveUriWithNullArgs ()
		{
			resolver.ResolveUri (null, null);
			Assert.Fail ("Should be error (MS.NET throws ArgumentException here).");
		}

//		[Test] Uncomment if you want to test.
		public void GetEntityWithNullArgs ()
		{
			Uri uri = new Uri ("http://www.example.com/index.rss");
			resolver.GetEntity (uri, null, null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetEntityWithRelativeFileUri ()
		{
			resolver.GetEntity (new Uri ("file.txt", UriKind.Relative), null, typeof (Stream));
		}

		[Test]
		[ExpectedException (typeof (XmlException))]
		public void GetEntityWithNonStreamReturnType ()
		{
			resolver.GetEntity (new Uri ("http://www.example.com/"), null, typeof (File));
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

		[Test]
		[Category ("StaticLinkedAotNotWorking")] // Can't find .dll files when bundled in .exe
		public void TestAsync ()
		{
			var loc = Assembly.GetExecutingAssembly ().Location;
			Uri resolved = resolver.ResolveUri (null, loc);
			Assert.AreEqual ("file", resolved.Scheme);
			var task = resolver.GetEntityAsync (resolved, null, typeof (Stream));
			Assert.IsTrue (task.Wait (3000));
			Assert.IsTrue (task.Result is Stream);
		}

		[Test]
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
				Assert.IsTrue (ex is XmlException);
			}
		}
	}
}
