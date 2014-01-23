//
// XmlSecureResolverTests.cs
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

#if !MOBILE

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;
using System.Xml;
using NUnit.Framework;

namespace MonoTestsXml
{
	[TestFixture]
	public class XmlSecureResolverTests
	{
		[Test]
		public void EmptyCtor ()
		{
			new XmlSecureResolver (null, (Evidence) null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void EmptyCtorCannotResolve ()
		{
			XmlSecureResolver r = new XmlSecureResolver (null, (Evidence)null);
			r.ResolveUri (null, "http://www.go-mono.com");
		}

		[Test]
		public void EmptyEvidenceWontMatter ()
		{
			XmlSecureResolver r = new XmlSecureResolver (new XmlUrlResolver (), (Evidence)null);
			Uri uri = r.ResolveUri (null, "http://www.go-mono.com");
			Assert.IsNotNull (uri);
		}

		[Test]
		public void CreateEvidenceForUrl_Basic ()
		{
			Evidence e = XmlSecureResolver.CreateEvidenceForUrl (null);
			Assert.AreEqual (0, e.Count, "null");

			e = XmlSecureResolver.CreateEvidenceForUrl (String.Empty);
			Assert.AreEqual (0, e.Count, "String.Empty");
		}

		[Test]
		public void CreateEvidenceForUrl_Local ()
		{
			// "normal" path
			Evidence e = XmlSecureResolver.CreateEvidenceForUrl (Assembly.GetExecutingAssembly ().Location);
			Assert.AreEqual (2, e.Count, "Assembly.GetExecutingAssembly ().Location");
			bool url = false;
			bool zone = false;
			IEnumerator en = e.GetHostEnumerator ();
			while (en.MoveNext ()) {
				if (en.Current is Url)
					url = true;
				else if (en.Current is Zone)
					zone = true;
			}
			Assert.IsTrue (url, "Url-1");
			Assert.IsTrue (zone, "Zone-1");

			// file://
			e = XmlSecureResolver.CreateEvidenceForUrl (Assembly.GetExecutingAssembly ().CodeBase);
			Assert.AreEqual (2, e.Count, "Assembly.GetExecutingAssembly ().CodeBase");
			url = false;
			zone = false;
			en = e.GetHostEnumerator ();
			while (en.MoveNext ()) {
				if (en.Current is Url)
					url = true;
				else if (en.Current is Zone)
					zone = true;
			}
			Assert.IsTrue (url, "Url-1");
			Assert.IsTrue (zone, "Zone-1");
		}

		[Test]
		public void CreateEvidenceForUrl_Http ()
		{
			// http://
			Evidence e = XmlSecureResolver.CreateEvidenceForUrl ("http://www.go-mono.com");
			Assert.AreEqual (3, e.Count, "http://www.go-mono.com");
			bool url = false;
			bool zone = false;
			bool site = false;
			IEnumerator en = e.GetHostEnumerator ();
			while (en.MoveNext ()) {
				if (en.Current is Url)
					url = true;
				else if (en.Current is Zone)
					zone = true;
				else if (en.Current is Site)
					site = true;
			}
			Assert.IsTrue (url, "Url-2");
			Assert.IsTrue (zone, "Zone-2");
			Assert.IsTrue (site, "Site-2");
		}

#if NET_4_5
		[Test]
		[Category("Async")]
		public void TestAsync ()
		{
			var loc = Assembly.GetExecutingAssembly ().Location;
			Evidence e = XmlSecureResolver.CreateEvidenceForUrl (loc);
			var ur = new XmlUrlResolver ();
			var sr = new XmlSecureResolver (ur, e);
			Uri resolved = sr.ResolveUri (null, loc);
			Assert.AreEqual ("file", resolved.Scheme);
			var task = sr.GetEntityAsync (resolved, null, typeof (Stream));
			Assert.That (task.Wait (3000));
			Assert.IsInstanceOfType (typeof (Stream), task.Result);
		}
#endif

	}
}

#endif
