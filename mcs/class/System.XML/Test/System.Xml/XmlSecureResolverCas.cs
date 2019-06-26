//
// XmlSecureResolverCas.cs - CAS unit tests for System.Xml.XmlSecureResolver
//
// Authors:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if !MOBILE

using NUnit.Framework;

using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Xml;

using MonoTests.System.Xml;

namespace MonoCasTests.System.Xml {

	[TestFixture]
	[Category ("CAS")]
	public class XmlSecureResolverCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[ExpectedException (typeof (SecurityException))]
		[Category ("NotWorking")] // requires imperative stack modifiers to work
		public void EmptyEvidenceDeniedAccess ()
		{
			XmlSecureResolver r = new XmlSecureResolver (new XmlUrlResolver (), (Evidence)null);
			Uri uri = r.ResolveUri (null, "http://www.example.com");
			r.GetEntity (uri, null, typeof (Stream));
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void DenyUnrestricted_UnitTests ()
		{
			XmlSecureResolverTests unittest = new XmlSecureResolverTests ();
			unittest.EmptyCtor ();
			unittest.EmptyEvidenceWontMatter ();
			unittest.CreateEvidenceForUrl_Basic ();
			unittest.CreateEvidenceForUrl_Http ();
		}

		[Test]
		[FileIOPermission (SecurityAction.PermitOnly, Unrestricted = true)]
		public void DenyUnrestricted_CreateEvidenceForUrl_Local ()
		{
			XmlSecureResolverTests unittest = new XmlSecureResolverTests ();
			// requires path discovery to get assembly location
			unittest.CreateEvidenceForUrl_Local ();
		}
	}
}

#endif
