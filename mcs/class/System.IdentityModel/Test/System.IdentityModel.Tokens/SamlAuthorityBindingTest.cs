//
// SamlAuthorityBindingTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System;
using System.Globalization;
using System.IO;
using System.IdentityModel.Claims;
using System.IdentityModel.Selectors;
using System.IdentityModel.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.IdentityModel.Tokens
{
	[TestFixture]
	public class SamlAuthorityBindingTest
	{
		XmlDictionaryWriter CreateWriter (StringWriter sw)
		{
			return XmlDictionaryWriter.CreateDictionaryWriter (XmlWriter.Create (sw));
		}

		[Test]
		public void DefaultValues ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			Assert.IsNull (a.AuthorityKind, "#1");
			Assert.IsNull (a.Binding, "#2");
			Assert.IsNull (a.Location, "#3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullKind ()
		{
			new SamlAuthorityBinding (null, "binding", "location");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNullBinding ()
		{
			new SamlAuthorityBinding (new XmlQualifiedName ("local", "urn:ns"), null, "location");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorNullLocation ()
		{
			new SamlAuthorityBinding (new XmlQualifiedName ("local", "urn:ns"), "binding", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetAuthorityKindEmptyName ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			a.AuthorityKind = XmlQualifiedName.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetLocationEmpty ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			a.Location = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SetBindingEmpty ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			a.Binding = String.Empty;
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoAuthorityKind ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			a.Binding = "binding";
			a.Location = "location";

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoBinding ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			a.AuthorityKind = new XmlQualifiedName ("local", "urn:ns");
			a.Location = "location";

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		[ExpectedException (typeof (SecurityTokenException))]
		public void WriteXmlNoLocation ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			a.AuthorityKind = new XmlQualifiedName ("local", "urn:ns");
			a.Binding = "binding";

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
		}

		[Test]
		public void WriteXml1 ()
		{
			SamlAuthorityBinding a = new SamlAuthorityBinding ();
			a.AuthorityKind = new XmlQualifiedName ("local", "urn:ns");
			a.Binding = "binding";
			a.Location = "location";

			StringWriter sw = new StringWriter ();
			using (XmlDictionaryWriter dw = CreateWriter (sw)) {
				a.WriteXml (dw, new SamlSerializer (), null);
			}
			Assert.AreEqual (String.Format ("<?xml version=\"1.0\" encoding=\"utf-16\"?><saml:AuthorityBinding xmlns=\"urn:ns\" AuthorityKind=\"local\" Location=\"location\" Binding=\"binding\" xmlns:saml=\"{0}\" />", SamlConstants.Namespace), sw.ToString ());
		}
	}
}
#endif
