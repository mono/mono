//
// XmlSecureResolverTests.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//
using System;
using System.IO;
using System.Security.Policy;
using System.Security.Permissions;
using System.Xml;
using NUnit.Framework;

namespace MonoTestsXml
{
	public class XmlSecureResolverTests : Assertion
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
			new XmlSecureResolver (null, (Evidence) null).ResolveUri (null, "http://www.go-mono.com");
		}

		[Test]
		public void EmptyEvidenceWontMatter ()
		{
			new XmlSecureResolver (new XmlUrlResolver (), (Evidence) null).ResolveUri (null, "http://www.go-mono.com");
		}

		[Test]
		[ExpectedException (typeof (PolicyException))]
		public void EmptyEvidenceDeniedAccess ()
		{
			XmlResolver r = new XmlSecureResolver (new XmlUrlResolver (), (Evidence) null);
			r.GetEntity (r.ResolveUri (null, "http://www.go-mono.com"), null, typeof (Stream));
		}
	}
}

